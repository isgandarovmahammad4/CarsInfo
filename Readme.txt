1. We are analyzing csproj file.

2. We are analyzing versions of packages in dependencies.

3. We are analyzing LaunchSettings.json file. That consist of profile settings.

4. We are launching the application.

5. We are analyzing the program.cs class and adding the necessary services to services container. for example builder.servicess.addmvc or .addmvccore and .addcontrollerswithviews.

6. We just keeping builder.services.addcontroller() for now.

7. We are adding new class to controllers folder. We are adding instance to this class with name ControllerBase for using relational datas with this.

8.We are adding [APIController] attribute to the CarsController class.
9. We are configuring get method inside the CarsController class to return a list of cars.
10 We are configuring request pipeline, for that we are adding middlewares to program.cs with .UseRouting, UseEndpoints with enpoints.mapcontrollermethods.
11. We are adding Httpget(api/cities)
12.we are updating it to route(api/cities) for working without Httpget(api/cities). we will continue only [httpget] in this case.
13.             var temp = new JsonResult(CarsDataStroe.Current.Cars);
            temp.StatusCode = 200; 

            below code equals = Ok();
13.1 We are ading ActionResult instead of JsonResult to return the result. It help us manage http status codes and return more overhandling results.
14. We are adding PointofInterestDto and seed dummy data to Carsdatastore. 
15. Adding Pointsofinterest controller.
16.Adding Httpnotacceptable tru to be strict about request format. Sometimes user asks for xml format but system doesnt supports this format at that thime system returns 406 not acceptable.
17. Now we are adding addproblems with details to the services.
18. Lambda (arrow function) syntax in C#:

    (parameters)  =>   { body }
       ↑           ↑       ↑
     inputs    "goes to"  the code that runs
19. Adding .AddXmlDataContractSerializerFormatters() to AddControllers. By default the API only
    has a JSON output formatter, so it can only produce JSON. This registers an XML output (and input)
    formatter too, so when a client sends Accept: application/xml the API can return XML instead of JSON.
    It works together with ReturnHttpNotAcceptable = true (item 16): now XML is a real supported format,
    so a request for XML returns 200 with XML rather than 406 Not Acceptable.

    Code example:
        builder.Services.AddControllers(options =>
        {
            options.ReturnHttpNotAcceptable = true;   // be strict about the Accept header
        })
        .AddXmlDataContractSerializerFormatters();    // also support XML, not just JSON

    Behavior (with this config):
        Accept: application/json  ->  200 OK, body in JSON
        Accept: application/xml   ->  200 OK, body in XML
        Accept: text/csv          ->  406 Not Acceptable (no formatter for that type)
20. Returning a file for download from a controller action.

    Code example:
        public ActionResult GetFile(string fileId)
        {
            var pathToFile = "Boy Artmı.pptx";
            if (!System.IO.File.Exists(pathToFile))   // check the file actually exists on disk
            {
                return NotFound();                    // 404 if it does not
            }
            var bytes = System.IO.File.ReadAllBytes(pathToFile);   // read the whole file into a byte[]
            return File(bytes, "text/plain", Path.GetFileName(pathToFile));   // send the bytes to the client
        }

    How it works:
    - System.IO.File.Exists(path): returns true/false whether the file is present. We use System.IO.File
      with the full namespace because the controller already has a File(...) method, so plain "File" would
      be ambiguous.
    - return NotFound(): helper from ControllerBase that returns HTTP 404 when the file is missing.
    - System.IO.File.ReadAllBytes(path): loads the file content into a byte array (byte[]).
    - return File(bytes, contentType, downloadName): the FileResult helper. It writes the bytes to the
      response and tells the browser to download them.
        * 1st arg (bytes)        -> the actual file content.
        * 2nd arg ("text/plain") -> the MIME/content type sent in the Content-Type header. NOTE: this file
          is a .pptx, so the correct type would be
          "application/vnd.openxmlformats-officedocument.presentationml.presentation"
          (or "application/octet-stream" for a generic binary download). "text/plain" is technically wrong
          here and may make the client mis-handle the file.
        * 3rd arg (file name)    -> Path.GetFileName extracts just "Boy Artmı.pptx" from the path; this
          becomes the suggested download file name (Content-Disposition header).
21. Detecting the correct content type automatically with FileExtensionContentTypeProvider.

    Instead of hard-coding "text/plain" (item 20), we let ASP.NET Core figure out the MIME type from the
    file extension. It keeps a built-in map of extensions -> content types (.pptx, .pdf, .png, .txt, ...).

    Registration in Program.cs (add it to the services container):
        builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

    Inject it into the controller via the constructor:
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider
                ?? throw new ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }

    Use it inside the action:
        _fileExtensionContentTypeProvider.TryGetContentType(pathToFile, out var contentType);
        if (string.IsNullOrEmpty(contentType))
        {
            contentType = "application/octet-stream";   // safe fallback for unknown types
        }
        return File(bytes, contentType, Path.GetFileName(pathToFile));

    How it works:
    - AddSingleton: registers one shared instance for the whole app (the extension map never changes,
      so a single instance is enough). The DI container then supplies it to the controller automatically.
    - TryGetContentType(path, out var contentType): looks at the file extension and tries to find the
      matching MIME type. It returns true and sets contentType when it knows the extension, or returns
      false (leaving contentType null) when it does not. The "out var" gives us the result in contentType.
    - The if (string.IsNullOrEmpty(...)) check covers the "not found" case: if the extension is unknown,
      we fall back to "application/octet-stream", which tells the browser "this is a generic binary file,
      just download it." This fixes the wrong "text/plain" from item 20 -> a .pptx now gets its proper type.
22. Difference between ++maxPointOfInterest (pre-increment) and maxPointOfInterest++ (post-increment).

    Both add 1 to the variable. The difference is the VALUE the expression returns at that moment:
    - maxPointOfInterest++ (post): "use first, then add" -> returns the OLD value, then increments.
    - ++maxPointOfInterest (pre):  "add first, then use" -> increments, then returns the NEW value.

    Example:
        int maxPointOfInterest = 5;
        int a = maxPointOfInterest++;   // a = 5 (old value), variable is now 6
        int b = ++maxPointOfInterest;   // b = 7 (new value), variable is now 7

    Why it matters here (generating a new Id):
        var maxPointOfInterest = car.PointsOfInterest.Max(p => p.Id);   // e.g. 5

        Id = ++maxPointOfInterest,   // assigns 6  -> correct, a fresh unused id
        Id = maxPointOfInterest++,   // assigns 5  -> BUG, collides with an existing id

    When it makes NO difference:
    - If the increment is on its own line and the returned value is not used, both behave the same:
        maxPointOfInterest++;   // same effect
        ++maxPointOfInterest;   // same effect
    The pre/post distinction only matters when the result is used in the same expression
    (an assignment, a condition, an array index, etc.).
23. Creating a new point of interest with an HTTP POST action.

    Code:
        [HttpPost]
        public ActionResult<PointOfInterestDto> CreatePointOfInterest(
            int carId, PointOfInterestCreationDto pointOfInterestDto)
        {
            var car = CarsDataStroe.Current.Cars.FirstOrDefault(x => x.Id == carId);
            if (car == null)
            {
                return NotFound("Car isnt exist.");
            }

            var maxPointOfInterest = CarsDataStroe.Current.Cars
                .SelectMany(x => x.PointsOfInterest)
                .Max(x => x.Id);

            var finalPointOfInterest = new PointOfInterestDto
            {
                Id = ++maxPointOfInterest,
                Name = pointOfInterestDto.Name,
                Description = pointOfInterestDto.Description
            };

            car.PointsOfInterest.Add(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest", new
            {
                carId = car.Id,
                pointofinterestid = finalPointOfInterest.Id
            }, finalPointOfInterest);
        }

    How it works (line by line):
    - [HttpPost]: maps this action to the HTTP POST verb (POST is used to create new resources).
    - ActionResult<PointOfInterestDto>: lets the action return EITHER an HTTP status result
      (like NotFound) OR a PointOfInterestDto object. The generic type also helps Swagger document
      what the success body looks like.
    - Parameters:
        * carId comes from the route (the URL, e.g. /api/cars/1/pointsofinterest).
        * pointOfInterestDto comes from the request BODY (the JSON the client sends). ASP.NET Core
          model-binds the JSON into the object automatically.
      Note we use a separate "CreationDto" (only Name + Description, no Id) for input, because the
      client must not set the Id - the server decides it.
    - FirstOrDefault(x => x.Id == carId): finds the matching car, or null if there is none.
    - if (car == null) return NotFound(...): if the parent car does not exist, return 404 with a message.
    - SelectMany(x => x.PointsOfInterest): flattens every car's points-of-interest into one list, so we
      can find the highest Id across ALL cars (see item on SelectMany). .Max(x => x.Id) gets that biggest id.
    - Id = ++maxPointOfInterest: pre-increment -> uses max + 1 as the next fresh id (see item 22).
    - car.PointsOfInterest.Add(...): stores the new point of interest in the in-memory data store.
    - return CreatedAtRoute(...): returns HTTP 201 Created. It does three useful things:
        * sets the status code to 201 (the correct code for "a new resource was created"),
        * adds a Location response header pointing to the new resource's URL,
        * the route name "GetPointOfInterest" must match the [HttpGet] action decorated with
          [Name("GetPointOfInterest")]; the anonymous object supplies the route values (carId +
          pointofinterestid) needed to build that URL,
        * the last argument (finalPointOfInterest) is the body returned to the client.

    Known issue: .Max(x => x.Id) throws if there are NO points of interest yet (Max on an empty
    sequence). A safer version is:
        var maxPointOfInterest = CarsDataStroe.Current.Cars
            .SelectMany(x => x.PointsOfInterest)
            .Select(x => x.Id)
            .DefaultIfEmpty(0)
            .Max();
24. 405 Method Not Allowed: this means the URL exists, but you used the wrong HTTP verb for it.
    For example, sending a POST request to an endpoint that only has a [HttpGet] method (or sending
    GET to a POST-only action). The route matches, but no action accepts that verb, so the server
    returns 405. Fix: use the correct verb (e.g. POST to a [HttpPost] action, GET to a [HttpGet] action).
25.Logger is built in services. so we dont need to register it in program cs class.
26. Why the Serilog logs folder was not created.

    No folder appeared because nothing was ever being written to Serilog. You built the Serilog logger,
    but ASP.NET Core was never told to use it:

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

    Those two lines remove the default logging and add Microsoft's own console logger. So every
    ILogger<T> message went to that console logger, and Log.Logger sat unused. Serilog's daily rolling
    file sink only creates the folder and file when the first message reaches it, so with no messages
    there was no folder.

    I replaced those two lines in CarsInfo.Api/Program.cs with:

        builder.Host.UseSerilog();

    That sends all framework and controller logs through your Serilog setup. Serilog's .WriteTo.Console()
    covers the console output, so you don't need AddConsole() anymore.
27. Scoped will used for context and repository.
28. "Unable to resolve service for type 'IMailService'" error (500).

    The error happens because the controller asks for IMailService, but you only registered the
    concrete classes:

        builder.Services.AddTransient<LocalMailService>();

    That registration only answers requests for LocalMailService itself. The DI container doesn't
    connect an interface to a class that implements it, so when it tried to build
    PointsOfInterestController(..., IMailService mailService) it found nothing for IMailService and
    returned a 500.

    I changed both registrations in Program.cs to map the interface to the class:

        #if DEBUG
        builder.Services.AddTransient<CarsInfo.Api.Services.IMailService, CarsInfo.Api.Services.LocalMailService>();
        #else
        builder.Services.AddTransient<CarsInfo.Api.Services.IMailService, CarsInfo.Api.Services.CloudMailService>();
        #endif
29. IEnumerable vs IQueryable (repository-dən nə qaytarmaq lazımdır).

    IEnumerable<City> - repository sorğunu bazada özü icra edir və hazır nəticəni qaytarır.
    Sonradan yazılan Where, OrderBy yaddaşda (C# tərəfində) işləyir, bazaya getmir.

        IEnumerable<City> GetCities();
        var cities = repo.GetCities()                    // SELECT * FROM Cities -> hamısı yaddaşa gəlir
                         .Where(c => c.Name == "Baku");  // filtr yaddaşda olur

    IQueryable<City> - hələ icra olunmamış sorğu qaytarır. Controller üzərinə Where, OrderBy
    əlavə edə bilir və hamısı bir SQL sorğusuna çevrilib bazada icra olunur.

        IQueryable<City> GetCities();
        var cities = repo.GetCities()
                         .Where(c => c.Name == "Baku")
                         .ToList();   // SQL yalnız burada gedir: SELECT ... WHERE Name = 'Baku'

    Mübahisə:
    - IQueryable rahatdır, amma persistence məntiqi repository-dən "sızır" (leak), yəni controller
      sorğu qurur. Bu, repository pattern-in məqsədinə ziddir.
    - IEnumerable pattern-ə uyğundur, amma çox filter/sort/paging olan böyük API-da onlarla metod
      yazmaq lazım gəlir.
    - DbContext özü də bir repository-dir və IQueryable qaytarır.
    Kursda API sadə olduğu üçün IEnumerable seçilir.

    Async versiya:

        Task<IEnumerable<City>> GetCitiesAsync();

    Task metodun asinxron olduğunu bildirir: baza cavab verənə qədər thread bloklanmır və başqa
    sorğulara xidmət edə bilir. Metod adı Async ilə bitir. Bu, .NET-də qəbul olunmuş qaydadır.
30. Sync, async və Task.

    ## 1. Thread nədir?

    **Thread** proqramda kodu icra edən "işçi"dir. ASP.NET Core-da hər gələn HTTP sorğusunu **thread pool**-dan götürülən bir thread emal edir. Thread pool hazır işçilər hovuzudur. Bu işçilərin sayı **məhduddur**.

    ## 2. Sinxron (sync) kod

    ```csharp
    public IEnumerable<Car> GetCars()
    {
        return _context.Cars.ToList();   // bazadan cavab gələnə qədər GÖZLƏYİR
    }
    ```

    Bazaya sorğu getdikdən sonra cavab, tutaq ki, 200 ms-də gəlir. Bu müddət ərzində thread **heç nə etmir, sadəcə gözləyir (bloklanır)**. Amma başqa işə də götürülə bilmir.

    **Restoran misalı:** ofisiant sifarişi mətbəxə verir və yemək hazır olana qədər mətbəxin qapısında dayanır. Başqa masalara xidmət etmir.

    Az sorğu olanda bu problem deyil. Amma 1000 istifadəçi eyni anda gələndə bütün thread-lər gözləməkdə olur. Yeni sorğular növbəyə düşür və API yavaşlayır (**thread pool starvation**).

    ## 3. Asinxron (async) kod

    ```csharp
    public async Task<IEnumerable<Car>> GetCarsAsync()
    {
        return await _context.Cars.ToListAsync();
    }
    ```

    Sorğu bazaya göndərilir. `await` yerində thread **sərbəst buraxılır və pool-a qayıdır**, orada başqa sorğulara xidmət edə bilir. Baza cavab verəndə pool-dan bir thread (eyni olmaya da bilər) götürülür və kod `await`-dən sonrakı sətirdən davam edir.

    **Restoran misalı:** ofisiant sifarişi mətbəxə verir, gedib başqa masalara xidmət edir. Yemək hazır olanda onu götürüb aparır.

    > Vacib məqam: async kodu **daha sürətli etmir**. Bir sorğu yenə 200 ms çəkir. Async **miqyaslanmanı (scalability)** artırır, yəni eyni sayda thread ilə daha çox sorğuya xidmət etmək olur.

    ## 4. Task nədir?

    `Task` **"gələcəkdə bitəcək bir iş"** deməkdir, yəni bir vəd (promise).

    ┌─────────┬─────────────────────────────────────────────────┐
    │ Tip     │ Mənası                                          │
    ├─────────┼─────────────────────────────────────────────────┤
    │ void    │ sync metod, heç nə qaytarmır                    │
    │ Task    │ async metod, heç nə qaytarmır (async-in void-i) │
    │ T       │ sync metod, T qaytarır                          │
    │ Task<T> │ async metod, bitəndə T qaytaracaq               │
    └─────────┴─────────────────────────────────────────────────┘

    ```csharp
    Task<IEnumerable<Car>> task = repo.GetCarsAsync(); // iş başladı, hələ nəticə yoxdur
    IEnumerable<Car> cars = await task;                // nəticəni gözlə və götür
    ```

    ## 5. async və await açar sözləri

    - **`async`**: metoda "içində `await` istifadə edə bilərsən" deyir. Kompilyator metodu xüsusi formaya (state machine) çevirir.
    - **`await`**: "bu Task bitənə qədər gözlə, amma thread-i bloklama" deməkdir. Task-ın içindəki nəticəni də açıb verir (`Task<T>` → `T`).

    `async` ilə `await` həmişə cüt istifadə olunur. `await` yazmaq üçün metod `async` olmalıdır.

    ## 6. "Async all the way" qaydası

    Async bir zəncir kimi yuxarıya qədər gedir:

    ```csharp
    // Repository
    public async Task<IEnumerable<Car>> GetCarsAsync()
        => await _context.Cars.ToListAsync();

    // Controller
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarDto>>> GetCars()
    {
        var cars = await _repository.GetCarsAsync();
        return Ok(cars);
    }
    ```

    **Bunu etmə:**

    ```csharp
    var cars = _repository.GetCarsAsync().Result;   // ❌
    _repository.GetCarsAsync().Wait();              // ❌
    ```

    `.Result` və `.Wait()` thread-i yenə bloklayır, ona görə async-in bütün faydası itir. Bəzi hallarda bu **deadlock**-a da səbəb ola bilər.

    ## 7. Nə vaxt async istifadə etməli?

    - ✅ **I/O əməliyyatları**, yəni gözləmə tələb edən işlər: verilənlər bazası, HTTP sorğuları, fayl oxuma/yazma. Repository üçün tam uyğundur.
    - ❌ **CPU-ya ağır hesablamalar** (məsələn, böyük dövrlər). Burada gözləmə yoxdur, thread onsuz da işləyir, ona görə async heç bir fayda vermir.

    ## 8. Adlandırma qaydası

    Async metodların adı `Async` ilə bitir: `GetCarsAsync`, `SaveChangesAsync`, `ToListAsync`. Bu, .NET-də qəbul olunmuş konvensiyadır və çağıranın `await` etməli olduğunu bildirir.

    ---

    **Videodakı "should we or shouldn't we?" sualının cavabı:** API-də bazaya müraciət I/O əməliyyatıdır. Ona görə repository metodlarını **async** yazmaq lazımdır, yəni `Task<IEnumerable<Car>> GetCarsAsync()`.
31. IEnumerable vs IQueryable cədvəlinin izahı, nümunələr, Where/search və AND/OR.

    # A. Cədvəlin izahı

    ## Sətir 1: Rahatlıq

    **IEnumerable ilə:** controller-in nəyə ehtiyacı varsa, repository-də onun üçün **hazır metod** olmalıdır.

    ```csharp
    public interface ICarInfoRepository
    {
        Task<IEnumerable<Car>> GetCarsAsync();
        Task<IEnumerable<Car>> GetCarsByBrandAsync(string brand);
        Task<IEnumerable<Car>> GetCarsOrderedByYearAsync();
        Task<IEnumerable<Car>> GetCarsByBrandOrderedByYearAsync(string brand);
        // yeni ehtiyac çıxdıqca yeni metod...
    }
    ```

    Controller yeni filtr istəyirsə, əvvəlcə repository-yə yeni metod yazılmalıdır. Buna görə "az çevik" deyilir.

    **IQueryable ilə:** bir metod kifayətdir, controller qalanını özü qurur:

    ```csharp
    IQueryable<Car> GetCars();

    // Controller-də:
    var result = await _repo.GetCars()
        .Where(c => c.Brand == "BMW")
        .OrderBy(c => c.Year)
        .ToListAsync();
    ```

    Repository-yə toxunmadan istənilən kombinasiyanı yazmaq olur. Buna görə "çox çevik" deyilir.

    ## Sətir 2: Repository pattern

    Əvvəlcə **repository pattern-in məqsədini** xatırlayaq. Məqsəd bazaya aid bütün işləri (sorğular, EF Core, SQL) **bir yerdə gizlətməkdir**. Controller yalnız bunu bilməlidir: "mənə maşınları ver". Necə gətirildiyi onu maraqlandırmamalıdır.

    **IEnumerable ilə bu məqsədə çatılır:**

    - Bütün sorğu məntiqi repository-nin içindədir.
    - Controller EF Core haqqında heç nə bilmir.
    - Sabah EF Core-u Dapper-ə və ya başqa bir API-yə dəyişsən, yalnız repository dəyişir.
    - Test yazmaq asandır: repository-ni sadə bir siyahı qaytaran mock ilə əvəz edirsən.

    **IQueryable ilə məntiq "sızır" (leak):**

    - Controller `Where`, `OrderBy`, `Include` yazır, yəni **sorğu qurmağa başlayır**. Bazaya aid məntiq repository-dən çıxıb controller-ə yayılır.
    - Controller-də `ToListAsync()` çağırılır. Bu EF Core metodudur, deməli controller EF Core-dan asılı olur.
    - Bəzi C# ifadələri SQL-ə çevrilə bilmir. Məsələn, `Where(c => MyHelper.Check(c))` **runtime-da xəta** verir. Bu xəta controller-də çıxır, halbuki problem bazaya aiddir.
    - Abstraksiya qorunmur: repository var, amma faktiki olaraq controller bazaya birbaşa sorğu yazır.

    Buna görə "pattern-in məqsədinə zidd" deyilir.

    ## Sətir 3: Böyük API

    İndi əks tərəfə baxaq. Tutaq ki, API istifadəçiyə belə imkanlar verir:

    ```
    GET /api/cars?brand=BMW&minYear=2015&maxPrice=30000&sort=price_desc&page=2&pageSize=20&fields=name,price
    ```

    Burada filter, sort, paging və field seçimi (**data shaping**) var. Bu parametrlərin **hər kombinasiyası** fərqli sorğu deməkdir.

    **IEnumerable ilə:** hər kombinasiya üçün ayrıca metod yazmaq mümkün deyil, çünki onlarla, yüzlərlə variant alınır. Praktikada belə edirlər: bütün parametrləri bir obyektə toplayıb **repository-nin içində** dinamik sorğu qururlar:

    ```csharp
    Task<IEnumerable<Car>> GetCarsAsync(CarQueryParameters parameters);
    ```

    Bu həm pattern-i qoruyur, həm də metod partlayışının qarşısını alır. Kursun sonrakı hissələrində yəqin ki, filter və paging məhz belə əlavə olunacaq.

    **IQueryable ilə:** bu cür API-də onun "sızan" olması bəzən **faydaya çevrilir**. Hər yeni parametri controller-də (və ya ayrıca bir servisdə) bir `Where` sətri ilə əlavə edirsən. Bəzi komandalar rahatlıq üçün bu güzəştə gedir.

    ## Yekun

    ┌───────────────────────────────────────────────────────┬──────────────────────────────────────────────────┐
    │ Vəziyyət                                              │ Seçim                                            │
    ├───────────────────────────────────────────────────────┼──────────────────────────────────────────────────┤
    │ Sadə API, az sorğu (kursdakı kimi)                    │ IEnumerable                                      │
    │ Çox filter/sort/paging                                │ IEnumerable + parametr obyekti (pattern qorunur) │
    │ Komanda rahatlığa üstünlük verir və leak-i qəbul edir │ IQueryable                                       │
    └───────────────────────────────────────────────────────┴──────────────────────────────────────────────────┘

    Videodakı əsas fikir də budur: **hər iki yanaşma real layihələrdə istifadə olunur, mütləq "düzgün" cavab yoxdur.** Kurs sadə API qurduğu üçün `IEnumerable` seçir.

    # B. Nümunələr (Car entity və CarContext ilə)

    **Məqsəd:** API belə sorğuları qəbul etsin:

    ```
    GET /api/cars?name=BMW
    GET /api/cars?searchQuery=sport
    GET /api/cars?searchQuery=sport&orderBy=name&pageNumber=2&pageSize=5
    ```

    ## Yanaşma 1: IEnumerable + parametr obyekti (pattern qorunur)

    ### Addım 1: Parametr obyekti

    Bütün mümkün parametrləri bir sinifə yığırıq:

    ```csharp
    // Models/CarQueryParameters.cs
    namespace CarsInfo.Api.Models
    {
        public class CarQueryParameters
        {
            const int maxPageSize = 20;

            public string? Name { get; set; }          // dəqiq ad filtri
            public string? SearchQuery { get; set; }   // ad və ya təsvirdə axtarış
            public string? OrderBy { get; set; }       // "name" və ya "description"
            public int PageNumber { get; set; } = 1;

            private int _pageSize = 10;
            public int PageSize
            {
                get => _pageSize;
                set => _pageSize = value > maxPageSize ? maxPageSize : value;
            }
        }
    }
    ```

    **İzah:**
    - Bütün sahələr `?` ilə nullable-dır, çünki istifadəçi istədiyi parametri göndərməyə bilər.
    - `PageSize` üçün yuxarı hədd qoyulub. Kimsə `pageSize=1000000` göndərib bazanı yükləyə bilməsin.
    - Yeni parametr lazım olanda **yalnız bu sinfə bir property əlavə edirsən**. Interface dəyişmir.

    ### Addım 2: Repository interface

    ```csharp
    public interface ICarInfoRepository
    {
        Task<IEnumerable<Car>> GetCarsAsync();
        Task<IEnumerable<Car>> GetCarsAsync(CarQueryParameters parameters);
    }
    ```

    Onlarla `GetCarsByNameAsync`, `GetCarsByNameOrderedAsync`... əvəzinə **bir metod** var.

    ### Addım 3: Repository implementasiyası (əsas hissə)

    ```csharp
    public class CarInfoRepository : ICarInfoRepository
    {
        private readonly CarContext _context;

        public CarInfoRepository(CarContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Car>> GetCarsAsync()
        {
            return await _context.Cars.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<IEnumerable<Car>> GetCarsAsync(CarQueryParameters parameters)
        {
            // 1) Başlanğıc: hələ heç bir sorğu icra olunmayıb
            IQueryable<Car> collection = _context.Cars;

            // 2) Filtr: yalnız parametr gəlibsə əlavə olunur
            if (!string.IsNullOrWhiteSpace(parameters.Name))
            {
                var name = parameters.Name.Trim();
                collection = collection.Where(c => c.Name == name);
            }

            // 3) Axtarış
            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
            {
                var search = parameters.SearchQuery.Trim();
                collection = collection.Where(c => c.Name.Contains(search)
                    || (c.Description != null && c.Description.Contains(search)));
            }

            // 4) Sıralama
            collection = parameters.OrderBy?.ToLower() switch
            {
                "description" => collection.OrderBy(c => c.Description),
                _             => collection.OrderBy(c => c.Name)
            };

            // 5) Paging
            collection = collection
                .Skip(parameters.PageSize * (parameters.PageNumber - 1))
                .Take(parameters.PageSize);

            // 6) YALNIZ BURADA bazaya sorğu gedir
            return await collection.ToListAsync();
        }
    }
    ```

    **Addım-addım nə baş verir:**

    1. `IQueryable<Car> collection = _context.Cars;`: bu sətir bazaya **heç nə göndərmir**. Bu, sadəcə "Cars cədvəlindən sorğu quracağam" deməkdir.
    2. Hər `if` bloku sorğuya bir şərt **əlavə edir**, amma yenə heç nə icra olunmur. Sorğu sanki kağız üzərində yazılır.
    3. `switch` ifadəsi sıralamanı seçir. Tanınmayan dəyər gəlsə, default olaraq ada görə sıralayır.
    4. `Skip` və `Take` paging edir. `pageNumber=2, pageSize=5` üçün: ilk `5 * (2-1) = 5` qeydi keç, sonrakı 5-i götür.
    5. `ToListAsync()` çağırılanda EF Core bütün yığılmış şərtləri **bir SQL sorğusuna** çevirir.

    `?searchQuery=sport&pageNumber=2&pageSize=5` üçün yaranan SQL təxminən belədir:

    ```sql
    SELECT c.Id, c.Name, c.Description
    FROM Cars AS c
    WHERE c.Name LIKE '%sport%' OR (c.Description IS NOT NULL AND c.Description LIKE '%sport%')
    ORDER BY c.Name
    LIMIT 5 OFFSET 5
    ```

    > **Diqqət:** repository-nin **içində** `IQueryable` istifadə edirik. Bu tamamilə normaldır. Problem `IQueryable`-ı repository-dən **çölə qaytarmaqdadır**. Çölə hazır `IEnumerable` çıxır.

    ### Addım 4: Controller

    ```csharp
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarWithoutPointsOfInterestDto>>> GetCars(
        [FromQuery] CarQueryParameters parameters)
    {
        var cars = await _carInfoRepository.GetCarsAsync(parameters);
        return Ok(_mapper.Map<IEnumerable<CarWithoutPointsOfInterestDto>>(cars));
    }
    ```

    **İzah:**
    - `[FromQuery]` URL-dəki `?name=...&pageSize=...` dəyərlərini avtomatik olaraq `CarQueryParameters` obyektinə doldurur.
    - Controller-də **heç bir** `Where`, `OrderBy` və ya `ToListAsync` yoxdur. O, EF Core haqqında heç nə bilmir.
    - (`_mapper` və DTO adı nümunə üçündür. Kursda AutoMapper sonradan gələcək.)

    ### Yeni parametr lazım olsa?

    Məsələn, "yalnız PointOfInterest-i olan maşınlar" filtri:

    ```csharp
    // CarQueryParameters-ə:
    public bool? HasPointsOfInterest { get; set; }

    // Repository-yə bir if:
    if (parameters.HasPointsOfInterest == true)
    {
        collection = collection.Where(c => c.PointsOfInterests.Any());
    }
    ```

    Interface və controller **dəyişmir**. Metod partlayışı yoxdur.

    ## Yanaşma 2: IQueryable qaytarmaq (rahatlıq, amma leak)

    ### Repository

    ```csharp
    public interface ICarInfoRepository
    {
        IQueryable<Car> GetCars();
    }

    public class CarInfoRepository : ICarInfoRepository
    {
        private readonly CarContext _context;
        public CarInfoRepository(CarContext context) => _context = context;

        public IQueryable<Car> GetCars() => _context.Cars;
    }
    ```

    Repository çox sadədir, demək olar ki, heç nə etmir. Metodun `async` olmadığına diqqət edin. O, sorğu icra etmir, sadəcə sorğunun "başlanğıcını" qaytarır.

    ### Controller (bütün iş burada)

    ```csharp
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Car>>> GetCars(
        string? name, string? searchQuery, string? orderBy,
        int pageNumber = 1, int pageSize = 10)
    {
        var query = _carInfoRepository.GetCars();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name == name.Trim());

        if (!string.IsNullOrWhiteSpace(searchQuery))
            query = query.Where(c => c.Name.Contains(searchQuery)
                || (c.Description != null && c.Description.Contains(searchQuery)));

        query = orderBy == "description"
            ? query.OrderBy(c => c.Description)
            : query.OrderBy(c => c.Name);

        var cars = await query
            .Skip(pageSize * (pageNumber - 1))
            .Take(Math.Min(pageSize, 20))
            .ToListAsync();          // EF Core metodu controller-də!

        return Ok(cars);
    }
    ```

    **Nəticə eynidir.** Eyni SQL yaranır. Fərq yalnız kodun **harada** yazılmasındadır.

    ### Burada "leak" konkret olaraq harada görünür?

    1. **`using Microsoft.EntityFrameworkCore;`** controller-ə lazım olur, çünki `ToListAsync()` oradadır. Controller EF Core-dan asılı olur.
    2. **Sorğu məntiqi controller-dədir.** Başqa bir controller də maşın axtarışı etsə, bu `Where`-ləri təkrar yazmalı olacaq (kod təkrarı).
    3. **Runtime xətası riski.** Controller-də belə yazsan:
       ```csharp
       query = query.Where(c => IsSportCar(c));   // adi C# metodu
       ```
       kod kompilyasiya olunur, amma işə düşəndə EF Core bu metodu SQL-ə çevirə bilmir və `InvalidOperationException ... could not be translated` xətası verir. Bazaya aid xəta controller-də çıxır.
    4. **Test çətinləşir.** `ToListAsync()` yalnız EF Core-un `IQueryable`-ında işləyir. Mock repository-dən adi `List<Car>().AsQueryable()` qaytarsan, `ToListAsync` xəta verir.

    ### Bəs niyə bəzi komandalar bunu seçir?

    - Kod azdır: parametr sinfi yoxdur, repository demək olar ki boşdur.
    - Hər ekranın/endpoint-in öz xüsusi sorğusu varsa, hamısını repository-yə daşımaq yorucu olur.
    - Bəziləri deyir: "`DbContext` onsuz da repository-dir, üstünə ikinci qat əlavə etməyə ehtiyac yoxdur." Onlar controller-də (və ya servis qatında) birbaşa `DbContext` istifadə edirlər.

    ## Müqayisə

    ┌────────────────────────────┬──────────────────────────────────────────────────┬───────────────────────────────────┐
    │                            │ Yanaşma 1 (IEnumerable + parametr)               │ Yanaşma 2 (IQueryable)            │
    ├────────────────────────────┼──────────────────────────────────────────────────┼───────────────────────────────────┤
    │ Sorğu harada qurulur       │ Repository-nin içində                            │ Controller-də                     │
    │ Controller EF Core bilirmi │ Xeyr                                             │ Bəli (ToListAsync)                │
    │ Yeni filtr əlavə etmək     │ Parametr sinfinə property + repository-yə bir if │ Controller-ə bir Where            │
    │ Kod təkrarı                │ Yoxdur, məntiq bir yerdədir                      │ Hər controller öz sorğusunu yazır │
    │ Test                       │ Asan                                             │ Çətin                             │
    │ Kod həcmi                  │ Bir az çox                                       │ Az                                │
    └────────────────────────────┴──────────────────────────────────────────────────┴───────────────────────────────────┘

    **Tövsiyə:** bu layihə üçün **Yanaşma 1** daha uyğundur. Kurs repository pattern öyrədir və bu yanaşma pattern-i qoruyur. Kursun sonrakı hissələrində filtr, axtarış və paging çox güman ki, məhz bu formada əlavə olunacaq.

    # C. Where sətrinin izahı

    ```csharp
    collection = collection.Where(c => c.Name.Contains(search)
        || (c.Description != null && c.Description.Contains(search)));
    ```

    **Bir cümlə ilə:** "Elə maşınları saxla ki, ya **adında**, ya da **təsvirində** axtarılan söz olsun."

    ## 1. `collection.Where(...)`

    `Where` **filtr** deməkdir. Kolleksiyadakı hər elementi yoxlayır və yalnız şərtə uyğun gələnləri saxlayır.

    ## 2. `c => ...` (lambda)

    `c` **hər bir maşını** təmsil edir. Adı istədiyin kimi qoya bilərsən (`car => ...` da olar).

    `=>` işarəsindən sonra gələn hissə **şərtdir**. Nəticəsi `true` və ya `false` olur:
    - `true` olarsa, maşın nəticədə qalır.
    - `false` olarsa, maşın çıxarılır.

    Yəni bu, "hər maşın `c` üçün sağ tərəfdəki şərti yoxla" deməkdir.

    ## 3. `c.Name.Contains(search)`

    `Contains` mətnin içində başqa bir mətnin olub-olmadığını yoxlayır.

    ```csharp
    "BMW Sport".Contains("Sport")   // true
    "Audi A4".Contains("Sport")     // false
    ```

    ## 4. `||` (VƏ YA)

    `||` iki şərtdən **ən azı biri** doğrudursa `true` qaytarır:

    ┌───────────┬───────────┬────────┐
    │ Sol tərəf │ Sağ tərəf │ Nəticə │
    ├───────────┼───────────┼────────┤
    │ true      │ true      │ true   │
    │ true      │ false     │ true   │
    │ false     │ true      │ true   │
    │ false     │ false     │ false  │
    └───────────┴───────────┴────────┘

    Burada sol tərəf "ad uyğundur", sağ tərəf isə "təsvir uyğundur" deməkdir. Hər hansı biri uyğun gəlsə, maşın qalır.

    ## 5. `(c.Description != null && c.Description.Contains(search))`

    Bu, ən çox çaşdıran hissədir.

    `&&` (**VƏ**) yalnız **hər iki** şərt doğru olanda `true` qaytarır.

    **Niyə `!= null` yoxlaması lazımdır?** Bazada bəzi maşınların təsviri boş (`NULL`) ola bilər. Əgər `Description` null olsa və birbaşa `null.Contains("sport")` yazılsa, C# **`NullReferenceException`** verər, çünki olmayan bir şeyin üzərində metod çağırmaq olmur.

    `&&` operatorunun xüsusiyyəti (**short-circuit**) budur: sol tərəf `false` olarsa, sağ tərəfə **heç baxılmır**, çünki nəticə onsuz da `false` olacaq. Deməli:
    - `Description` null olarsa, `c.Description != null` → `false` olur, `Contains` heç çağırılmır və xəta yaranmır.
    - `Description` null deyilsə, `Contains` yoxlanılır.

    Mötərizə `( )` bu iki şərti bir qrup edir ki, `||` ilə düzgün birləşsin.

    ## 6. `collection = ...`

    `Where` köhnə kolleksiyanı dəyişmir, **yeni** sorğu qaytarır. Ona görə nəticəni yenidən `collection`-a yazırıq. Belə olmasa, filtr itərdi.

    ## Konkret misal

    Bazada belə maşınlar olsun, `search = "sport"`:

    ┌────────────────┬─────────────────────────┬────────────────┬───────────────────────────────────┬────────┐
    │ Name           │ Description             │ Ad uyğundurmu? │ Təsvir uyğundurmu?                │ Nəticə │
    ├────────────────┼─────────────────────────┼────────────────┼───────────────────────────────────┼────────┤
    │ BMW sport      │ NULL                    │ [+] true       │ yoxlanmır, onsuz da true-dur      │ qalır  │
    │ Audi A4        │ comfortable sport sedan │ [-] false      │ [+] true                          │ qalır  │
    │ Toyota Corolla │ family car              │ [-] false      │ [-] false                         │ çıxır  │
    │ Kia Rio        │ NULL                    │ [-] false      │ null → false, Contains çağırılmır │ çıxır  │
    └────────────────┴─────────────────────────┴────────────────┴───────────────────────────────────┴────────┘

    > `||` operatorunda da short-circuit var: sol tərəf `true` olsa, sağ tərəfə baxılmır. Birinci sətirdə məhz bu baş verir.

    ## Bazaya necə gedir?

    Bu sorğu EF Core-da `IQueryable` üzərində yazıldığı üçün C#-da icra olunmur. EF Core onu **SQL-ə çevirir**:

    ```sql
    WHERE c.Name LIKE '%sport%'
       OR (c.Description IS NOT NULL AND c.Description LIKE '%sport%')
    ```

    - `Contains` → `LIKE '%...%'`
    - `||` → `OR`
    - `&&` → `AND`
    - `!= null` → `IS NOT NULL`

    Qeyd: SQL-də `NULL LIKE '%sport%'` xəta vermir, sadəcə uyğun gəlmir. Ona görə bazada `!= null` yoxlaması texniki baxımdan vacib deyil. Amma C# kompilyatoru `Description`-un null ola biləcəyini bilir və xəbərdarlıq verir. Kod yaddaşda (`IEnumerable` üzərində) işləsə, həqiqətən xəta da yaranar. Buna görə yoxlamanı yazmaq yaxşı vərdişdir.

    ## Daha oxunaqlı yazılış

    Eyni məna, sadəcə ayrıca sətirlərlə:

    ```csharp
    collection = collection.Where(c =>
        c.Name.Contains(search)                 // adında var
        ||                                      // VƏ YA
        (c.Description != null                  // təsvir boş deyil
         && c.Description.Contains(search))     // VƏ təsvirdə var
    );
    ```

    Və ya null-conditional operatoru ilə qısa forma:

    ```csharp
    collection = collection.Where(c => c.Name.Contains(search)
        || (c.Description ?? "").Contains(search));
    ```

    `c.Description ?? ""` "təsvir null olarsa, boş mətn götür" deməkdir. Boş mətndə "sport" olmadığı üçün nəticə `false` olur və xəta yaranmır.

    # D. search nədir?

    `search` **entity deyil**. O, sadəcə bir **`string`** dəyişənidir, yəni istifadəçinin URL-də yazdığı axtarış sözüdür.

    ## Haradan gəlir?

    Kodda belə idi:

    ```csharp
    if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
    {
        var search = parameters.SearchQuery.Trim();
        collection = collection.Where(c => c.Name.Contains(search) || ...);
    }
    ```

    Yolu addım-addım izləyək:

    **1. İstifadəçi sorğu göndərir:**
    ```
    GET /api/cars?searchQuery=  sport
    ```

    **2. `[FromQuery]` URL-dəki dəyəri obyektə yazır:**
    ```csharp
    public class CarQueryParameters
    {
        public string? SearchQuery { get; set; }   // = "  sport  "
    }
    ```

    **3. Repository-də yerli dəyişən yaradılır:**
    ```csharp
    var search = parameters.SearchQuery.Trim();   // = "sport"
    ```

    - `var` burada `string` deməkdir. Kompilyator tipi sağ tərəfdən özü müəyyən edir. Bu sətir `string search = ...` ilə eynidir.
    - `Trim()` əvvəldəki və sondakı boşluqları silir: `"  sport  "` → `"sport"`.

    Yəni `search` sadəcə `"sport"` mətnidir.

    ## Entity ilə fərqi

    ┌───────────────┬────────────────────────────────────┬──────────────────────────────────────┐
    │               │ c                                  │ search                               │
    ├───────────────┼────────────────────────────────────┼──────────────────────────────────────┤
    │ Nədir         │ Car entity-si (bazadakı bir sətir) │ string (sadə mətn)                   │
    │ Haradan gəlir │ Bazadakı Cars cədvəlindən          │ İstifadəçinin URL-dəki parametrindən │
    │ Dəyişirmi     │ Hər maşın üçün fərqlidir           │ Bütün sorğu boyu eynidir             │
    └───────────────┴────────────────────────────────────┴──────────────────────────────────────┘

    Lambda-da hər **maşının** (`c`) adını eyni **söz** (`search`) ilə müqayisə edirik:

    ```csharp
    c => c.Name.Contains(search)
    //   ↑ Car     ↑ string   ↑ string
    ```

    `c.Name` də `string`-dir (`Car` sinfində `public string Name`). `Contains` bir mətnin içində başqa mətn axtarır.

    ## Niyə birbaşa `parameters.SearchQuery` yazmadıq?

    Belə də yazmaq olardı:

    ```csharp
    collection = collection.Where(c => c.Name.Contains(parameters.SearchQuery.Trim()));
    ```

    Amma ayrıca dəyişən daha yaxşıdır:
    1. `Trim()` **bir dəfə** icra olunur, iki yerdə (`Name` və `Description` üçün) təkrarlanmır.
    2. Kod qısa və oxunaqlı olur.

    ## Bazaya necə gedir? (vacib bonus)

    EF Core lambda-nın içindəki `search` dəyişənini görür və onu SQL-ə **parametr** kimi göndərir:

    ```sql
    -- @__search_0 = 'sport'
    WHERE c.Name LIKE '%' + @__search_0 + '%'
    ```

    Mətn birbaşa SQL-in içinə yapışdırılmır. Bu, **SQL injection**-dan qoruyur. İstifadəçi `searchQuery='; DROP TABLE Cars; --` göndərsə belə, bu sadəcə axtarılan mətn kimi qəbul olunur, əmr kimi icra olunmur.

    # E. Eyni vaxtda name = car1 və description = desc3 istənilsə?

    Burada iki fərqli ehtiyac var və onlar fərqli həll olunur.

    ## 1. `searchQuery` bunu edə bilmir

    `searchQuery` **bir mətndir** və eyni mətni həm adda, həm təsvirdə axtarır (`OR`). Ona görə ad üçün bir söz, təsvir üçün başqa söz demək mümkün deyil.

    İstifadəçi belə yazsa:

    ```
    GET /api/cars?searchQuery=car1 desc3
    ```

    `search = "car1 desc3"` olur və SQL belə alınır:

    ```sql
    WHERE Name LIKE '%car1 desc3%' OR Description LIKE '%car1 desc3%'
    ```

    Bu, **bütöv** `"car1 desc3"` mətnini axtarır. Heç bir maşının adında və ya təsvirində bu mətn tam şəkildə olmadığı üçün **nəticə boş qayıdır**.

    ## 2. Həll: hər sahə üçün ayrıca parametr

    "Adı car1 **VƏ** təsviri desc3 olan" demək üçün ayrıca parametrlər lazımdır:

    ```csharp
    public class CarQueryParameters
    {
        public string? Name { get; set; }          // ada görə filtr
        public string? Description { get; set; }   // təsvirə görə filtr (YENİ)
        public string? SearchQuery { get; set; }   // ümumi axtarış
        // ... paging, orderBy
    }
    ```

    Repository:

    ```csharp
    IQueryable<Car> collection = _context.Cars;

    if (!string.IsNullOrWhiteSpace(parameters.Name))
    {
        var name = parameters.Name.Trim();
        collection = collection.Where(c => c.Name == name);
    }

    if (!string.IsNullOrWhiteSpace(parameters.Description))
    {
        var description = parameters.Description.Trim();
        collection = collection.Where(c => c.Description == description);
    }
    ```

    Sorğu:

    ```
    GET /api/cars?name=car1&description=desc3
    ```

    SQL:

    ```sql
    WHERE Name = 'car1' AND Description = 'desc3'
    ```

    ## Əsas qayda: ardıcıl `Where` = `AND`

    Bu hissə vacibdir. **Hər yeni `Where` əvvəlkilərin üzərinə `AND` kimi əlavə olunur.**

    ```csharp
    collection = collection.Where(A);
    collection = collection.Where(B);
    // nəticə: WHERE A AND B
    ```

    `OR` lazım olanda isə hər iki şərt **bir `Where`-in içində** `||` ilə yazılır. `searchQuery`-də etdiyimiz məhz bu idi.

    ┌─────────────────────┬───────────────┐
    │ Kod                 │ SQL           │
    ├─────────────────────┼───────────────┤
    │ .Where(A).Where(B)  │ WHERE A AND B │
    │ .Where(c => A || B) │ WHERE A OR B  │
    └─────────────────────┴───────────────┘

    ## Hamısını birlikdə göndərsə nə olur?

    ```
    GET /api/cars?name=car1&description=desc3&searchQuery=sport
    ```

    Hər `if` öz `Where`-ini əlavə edir və hamısı `AND` ilə birləşir:

    ```sql
    WHERE Name = 'car1'
      AND Description = 'desc3'
      AND (Name LIKE '%sport%' OR Description LIKE '%sport%')
    ```

    Yəni maşın **hər üç şərtə** uyğun gəlməlidir.

    ## Misal

    ┌──────┬─────────────┬──────────────────────────────┐
    │ Name │ Description │ ?name=car1&description=desc3 │
    ├──────┼─────────────┼──────────────────────────────┤
    │ car1 │ desc3       │ [+] qalır                    │
    │ car1 │ desc1       │ [-] (təsvir uyğun deyil)     │
    │ car2 │ desc3       │ [-] (ad uyğun deyil)         │
    └──────┴─────────────┴──────────────────────────────┘

    ## `==` yoxsa `Contains`?

    - `c.Name == name` **dəqiq** uyğunluq tələb edir: yalnız tam olaraq `"car1"`.
    - `c.Name.Contains(name)` **hissəvi** uyğunluq verir: `"car1"`, `"my car1"`, `"car10"` da gəlir.

    Adətən **filtr** parametrləri (`name`, `description`) `==` ilə, **axtarış** parametri (`searchQuery`) isə `Contains` ilə yazılır. Amma bu, API-ni dizayn edənin seçimidir.

    **Qısaca:** `searchQuery` "harada olursa olsun bu sözü tap" deməkdir. Konkret sahələr üzrə `AND` lazımdırsa, hər sahə üçün ayrıca parametr və ayrıca `Where` yazılır.

32. Trasient using for Services.
    Singleton using for  =>
    Scoped for DbContext,Repository.

33. Swagger "Failed to load API definition" (404) və HttpRepl ilə 401 Unauthorized problemi.

    Burada 3 ayrı problem olub. Kodda yalnız birincisi üçün dəyişiklik lazım oldu.

    # A. Swagger: "Failed to load API definition" (404 /swagger/2.0/swagger.json)

    ## Sadə dillə nə baş verdi?

    Swagger iki hissədən ibarətdir:
    - **SwaggerGen** — API-ni oxuyur və JSON sənəd **yaradır** (məs: `/swagger/1.0/swagger.json`). Bu, "mətbəx"dir.
    - **SwaggerUI** — həmin JSON-u **oxuyub** gözəl səhifə kimi göstərir. Bu, "ofisiant"dır.

    Bizdə ofisiant (UI) mətbəxdən (Gen) olmayan yeməyi istəyirdi:

    ```csharp
    app.UseSwaggerUI(setupAction =>
    {
        foreach (var description in app.DescribeApiVersions())
        {
            // UI hər versiya üçün siyahıya bir sətir əlavə edir: 0.1, 1.0, 2.0
            setupAction.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", ...);
        }
    });
    ```

    Controller-lərdə 3 versiya var: `[ApiVersion(0.1)]`, `[ApiVersion(1)]`, `[ApiVersion(2)]`.
    Ona görə UI `/swagger/2.0/swagger.json` faylını istəyirdi.

    Amma `AddSwaggerGen`-də heç bir `SwaggerDoc(...)` yazılmamışdı. Belə olanda Swashbuckle
    yalnız **bir** default sənəd yaradır və onun adı `v1`-dir. Yəni `2.0` adlı sənəd **yox idi** → **404**.

    ┌──────────────────────┬──────────────────────────────────────┐
    │ UI nə istəyirdi      │ Gen nə yaradırdı                     │
    ├──────────────────────┼──────────────────────────────────────┤
    │ /swagger/0.1/...json │ yox                                  │
    │ /swagger/1.0/...json │ yox                                  │
    │ /swagger/2.0/...json │ yox                                  │
    │ -                    │ /swagger/v1/swagger.json (default)   │
    └──────────────────────┴──────────────────────────────────────┘

    ## Həll: hər versiya üçün ayrıca sənəd yaratmaq (Program.cs)

    ```csharp
    using Asp.Versioning.ApiExplorer;   // əlavə olundu

    // Versiyaların siyahısını almaq üçün provider-i götürürük
    var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider()
        .GetRequiredService<IApiVersionDescriptionProvider>();

    builder.Services.AddSwaggerGen(setupAction =>
    {
        // Hər versiya (0.1, 1.0, 2.0) üçün bir Swagger sənədi
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            setupAction.SwaggerDoc(
                description.GroupName,          // sənədin adı = UI-ın istədiyi ad
                new OpenApiInfo
                {
                    Title = "CarsInfo API",
                    Version = description.ApiVersion.ToString(),
                    Description = "Through this API you can access cars and their points of interest."
                });
        }

        // ... XML comments və security hissəsi olduğu kimi qalır
    });
    ```

    **Əsas qayda:** UI-da `SwaggerEndpoint`-də yazılan ad ilə Gen-də `SwaggerDoc`-da yazılan ad **eyni olmalıdır**.
    İkisi də `description.GroupName`-dən gəldiyi üçün indi həmişə uyğun gəlir.

    Swashbuckle hər endpoint-i öz versiyasının sənədinə qoyur (endpoint-in `GroupName`-i = sənədin adı).
    Ona görə 1.0 sənədində yalnız `CarsController`, 2.0 sənədində yalnız `PointsOfInterestController` görünür.

    Qeyd: `BuildServiceProvider()` burada ASP0000 xəbərdarlığı verə bilər. Öyrənmə layihəsi üçün problem deyil.
    Daha "təmiz" yol `IConfigureOptions<SwaggerGenOptions>` sinfi yazmaqdır.

    # B. HttpRepl: GET /api/cars → 401 Unauthorized

    ## Niyə?

    `CarsController`-in üstündə `[Authorize]` var. Yəni token (JWT) olmadan bu controller-ə girmək olmaz.

    Swagger-də "Authorize" düyməsinə token yazmaq **yalnız Swagger səhifəsinə** aiddir.
    HttpRepl ayrıca proqramdır, o token-i bilmir. Ona özün `Authorization` header-i verməlisən.

    Server cavabındakı bu sətir də bunu deyir:

    ```
    WWW-Authenticate: Bearer
    ```

    Yəni: "Mənə Bearer token lazımdır, sən göndərməmisən."

    ## İkinci kiçik səbəb: səhv versiyanın sənədi

    HttpRepl-i **2.0** sənədinə qoşmuşduq. 2.0-da yalnız `pointsofinterest` var. `/api/cars` isə **1.0**-dadır.
    Ona görə `ls` əmri `/api/cars []` göstərirdi — `[]` "bu sənəddə bu yol üçün HTTP metodu yoxdur" deməkdir.
    1.0-a qoşulanda `/api/cars [GET]` göründü.

    Bəs niyə `get` yenə də serverə getdi? Çünki HttpRepl sənəddə olmasa belə sorğunu göndərir.
    URL-də versiya olmadığı üçün server `AssumeDefaultVersionWhenUnspecified = true` sayəsində 1.0 götürdü.

    ## Həll (kod dəyişikliyi yoxdur)

    HttpRepl-in **içində** (prompt `https://localhost:7098/...>` olanda):

    ```
    connect https://localhost:7098 --openapi https://localhost:7098/swagger/1.0/swagger.json
    set header content-type application/json
    post /api/authentication/authenticate -c "{\"username\":\"kevin\",\"password\":\"x\"}"
    set header Authorization "Bearer TOKEN_BURAYA"
    cd api/cars
    get
    ```

    Nəticə: `HTTP/1.1 200 OK` və 3 maşın.

    Niyə adi `connect https://localhost:7098` "Unable to find an OpenAPI description" dedi?
    Çünki HttpRepl standart yolları yoxlayır (məs: `/swagger/v1/swagger.json`). Bizim sənədlər isə
    `1.0`, `2.0`, `0.1` adlanır. Ona görə `--openapi` ilə yolu özümüz göstəririk.

    # C. HttpRepl çökdü və əmrlər Windows-a getdi

    ## 3 səhv bir yerdə

    1. **Token `<` `>` içində yazıldı.** Nümunədəki `<token>` "token-i bura qoy" deməkdir, mötərizələr token-in hissəsi deyil.
       Düzgün: `"Bearer eyJhbGci..."`, səhv: `"Bearer <eyJhbGci...>"`.

    2. **HttpRepl çökdü** (`ArgumentOutOfRangeException ... Parameter 'top'`). Bu, HttpRepl-in öz bug-udur:
       çox uzun sətir pəncərənin aşağı kənarından keçəndə kursoru pəncərədən kənara aparmağa çalışır və çökür.
       Token çox uzun olduğu üçün bu baş verdi.

    3. **Çökəndən sonra əmrlər cmd-yə yazıldı.** HttpRepl bağlandı, prompt `C:\Users\...>` oldu.
       Bundan sonra `connect`, `run`, `set header`, `cd api/cars` Windows-a getdi:
       - `connect`, `run` → Windows belə əmr tanımır.
       - `set` → cmd-də environment variable yaradır, header yox.
       - `cd api/cars` → cmd qovluq axtarır.

    ## Hansı prompt-dayam?

    ┌───────────────────────────────┬──────────────────┬──────────────────────────────────┐
    │ Prompt                        │ Harada           │ Nə yazmaq olar                   │
    ├───────────────────────────────┼──────────────────┼──────────────────────────────────┤
    │ C:\Users\...>                 │ Windows cmd      │ httprepl (əvvəlcə bunu işə sal)  │
    │ (Disconnected)>               │ HttpRepl         │ connect, run                     │
    │ https://localhost:7098/...>   │ HttpRepl         │ set header, cd, ls, get, post    │
    └───────────────────────────────┴──────────────────┴──────────────────────────────────┘

    ## Həll: uzun sətri script faylına yazmaq

    Bir mətn faylı yarat (məs: `auth.txt`):

    ```
    connect https://localhost:7098 --openapi https://localhost:7098/swagger/1.0/swagger.json
    set header Authorization "Bearer TOKEN_BURAYA"
    cd api/cars
    ```

    Sonra:

    ```
    C:\...> httprepl
    (Disconnected)> run "C:\yol\auth.txt"
    https://localhost:7098/api/cars> get
    ```

    `run` faylı sətir-sətir icra edir, uzun sətri yapışdırmağa ehtiyac qalmır, HttpRepl çökmür.
    Alternativ: əvvəl `clear` yaz, pəncərəni böyüt, sonra yapışdır.

    **Yadda saxla:** token 1 saat etibarlıdır (`DateTime.UtcNow.AddHours(1)`). Vaxtı bitəndə yenə 401 alacaqsan —
    `post /api/authentication/authenticate` ilə yeni token al və faylda dəyiş.

    **Qısaca:**
    - 404 → Swagger UI olmayan sənədi istəyirdi → hər versiya üçün `SwaggerDoc` əlavə etdik.
    - 401 → HttpRepl token göndərmirdi → `set header Authorization "Bearer ..."`.
    - Çökmə → uzun sətir HttpRepl bug-u → script faylı + `run`.

34. HttpRepl: `run` script faylı və `get -h Accept:application/xml` nə edir?

    Bu, HttpRepl-in içində ard-arda yazılan 2 əmrdir:

    ```
    run "C:\yol\auth.txt"
    get -h Accept:application/xml
    ```

    # A. `run "...auth.txt"`

    `run` mətn faylını oxuyur və **hər sətri sanki sən özün yazmısan kimi** icra edir.
    Faylda 3 sətir var:

    ┌───────────────────────────────────────────────┬────────────────────────────────────────────────────────┐
    │ Fayldakı sətir                                │ Nə edir                                                │
    ├───────────────────────────────────────────────┼────────────────────────────────────────────────────────┤
    │ connect https://localhost:7098 --openapi ...  │ HttpRepl-i API-yə qoşur, 1.0 Swagger sənədini yükləyir │
    │ /swagger/1.0/swagger.json                     │ → `ls` və `cd` endpoint-ləri tanıyır.                  │
    ├───────────────────────────────────────────────┼────────────────────────────────────────────────────────┤
    │ set header Authorization "Bearer eyJ..."      │ JWT token-i bundan sonrakı **hər** sorğuya əlavə edir  │
    │                                               │ → `[Authorize]` olan controller-lər içəri buraxır.     │
    ├───────────────────────────────────────────────┼────────────────────────────────────────────────────────┤
    │ cd api/cars                                   │ `/api/cars`-a keçir → sadəcə `get` yazanda             │
    │                                               │ `GET https://localhost:7098/api/cars` getsin.          │
    └───────────────────────────────────────────────┴────────────────────────────────────────────────────────┘

    Yəni bu 3 əmri əllə yazmaqla eynidir. Fərq: uzun token sətrini yapışdırmırsan, HttpRepl çökmür (bax: 33-C).

    # B. `get -h Accept:application/xml`

    - `get` → olduğun yola (`/api/cars`) **GET** sorğusu göndərir. `Authorization` header-i A-dan avtomatik gedir.
    - `-h` (`--header`-in qısası) → header-i **yalnız bu bir sorğu üçün** əlavə edir.
      (`set header` isə **bütün** sorğular üçündür.)
    - `Accept:application/xml` → serverə deyir: "Cavabı **JSON yox, XML** kimi göndər."

    ## Content negotiation (məzmun razılaşması)

    Client `Accept` header-i ilə **hansı formatı istədiyini** deyir, server isə uyğun **formatter** seçir.

    Bizim API XML qaytara bilir, çünki `Program.cs`-də bu var:

    ```csharp
    builder.Services.AddControllers(options =>
    {
        options.ReturnHttpNotAcceptable = false;
    })
    .AddXmlDataContractSerializerFormatters();   // XML formatter əlavə edir
    ```

    `AddXmlDataContractSerializerFormatters()` olmasa, API yalnız JSON qaytara bilərdi.

    Gözlənilən nəticə:

    ```
    HTTP/1.1 200 OK
    Content-Type: application/xml; charset=utf-8

    <ArrayOfCarWithoutPointsOfInterestDto ...>
      ...
    </ArrayOfCarWithoutPointsOfInterestDto>
    ```

    ## Dəstəklənməyən format istəsən nə olur?

    Məsələn `get -h Accept:text/csv`. Cavab `ReturnHttpNotAcceptable`-dan asılıdır:

    ┌─────────────────────────┬─────────────────────────────────────────────────┐
    │ ReturnHttpNotAcceptable │ Accept:text/csv göndərəndə                      │
    ├─────────────────────────┼─────────────────────────────────────────────────┤
    │ false (bizdə belədir)   │ Xəta yox, default olaraq JSON qaytarır          │
    │ true                    │ 406 Not Acceptable ("bu formatı verə bilmirəm") │
    └─────────────────────────┴─────────────────────────────────────────────────┘

    # C. Bonus: "The token expired" (401)

    Bir saatdan sonra eyni əmr belə cavab verdi:

    ```
    HTTP/1.1 401 Unauthorized
    WWW-Authenticate: Bearer error="invalid_token", error_description="The token expired at '10/28/2026 09:48:45'"
    ```

    Səbəb: `AuthenticationController`-də token **1 saatlıq** yaradılır:

    ```csharp
    DateTime.UtcNow.AddHours(1)   // token-in bitmə vaxtı
    ```

    Vaxtı bitmiş token = token yoxdur. Yəni API düzgün işləyir, sadəcə yeni token lazımdır:
    1. `post /api/authentication/authenticate -c "{\"username\":\"kevin\",\"password\":\"x\"}"` → yeni token al.
    2. `auth.txt`-də 2-ci sətirdəki köhnə token-i yenisi ilə əvəz et.
    3. Yenə `run "C:\yol\auth.txt"`.

    Qeyd: "No connection could be made because the target machine actively refused it" isə fərqli xətadır —
    bu, API-nin həmin an **işləmədiyini** göstərir (məs: restart olunurdu). Token ilə əlaqəsi yoxdur.

    **Qısaca:**
    - `run` → fayldakı əmrləri bir-bir icra edir (qoşul + token + cd).
    - `get -h Accept:application/xml` → GET sorğusu, cavabı XML formatında istəyir.
    - `-h` bir sorğu üçün, `set header` bütün sorğular üçün.
    - Token 1 saatdan sonra bitir → 401 → yeni token al.