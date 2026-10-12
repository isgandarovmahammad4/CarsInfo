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
25.