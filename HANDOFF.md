# CarsInfo — Handoff

**Date:** 2026-10-21 · **Branch:** `master` · **Head:** `263299c` "7/Deleting a Resource" · **Working tree:** clean (before this file)

---

## 0. Verdict first

**The project builds (0 errors), but the PointsOfInterest write endpoints are broken at runtime.**
The user is following an ASP.NET Core Web API course module by module and committing after each
video. Module 7 (EF Core + repository pattern + AutoMapper) has just been finished on paper, but
POST/PATCH/PUT/DELETE in `PointsOfInterestController` have the bugs listed in §4 and have not been
exercised against the database. Fix those before starting module 8.

---

## 1. Who / how to work here

- This is a **learning project**. The user follows a course built around a "CityInfo" API and
  renames everything to Cars (`City` → `Car`). Leftover names such as `cityId` and
  `AddPointOfInterestForCityAsync` come from that renaming, not from a design decision.
- The user writes in **Azerbaijani** and wants explanations in Azerbaijani, step by step, with
  small code examples taken from this project's real types (`Car`, `PointOfInterest`, `CarContext`).
- **`Readme.txt`** at the repo root is the user's numbered study log (currently up to **32**).
  When asked to "add to readme", append as the next number, keep the wording of the explanation
  that was given, indent body 4 spaces and code 8 spaces, **CRLF** line endings. Tables in it are
  box-drawn (`┌─┬─┐`), not Markdown pipes, with `[+]`/`[-]` instead of emoji (alignment).
- Commit messages follow `<module>/<video title>`, e.g. `7/Deleting a Resource`. Only commit when asked.

---

## 2. Solution map

`CarsInfo.sln` → `CarsInfo.Api/CarsInfo.Api.csproj`, .NET 8, nullable enabled.

```
Program.cs                 Serilog (console + logs/carsinfo.txt daily), controllers + NewtonsoftJson + XML,
                           ProblemDetails, IMailService (Local in DEBUG / Cloud in Release),
                           CarContext (SQLite, "DefaultConnection"), Swagger, AutoMapper,
                           ICarInfoRepository (Scoped)
DbContexts/CarContext.cs   DbSet<Car> Cars, DbSet<PointOfInterest> PointsOfInterest, seed data
Entities/                  Car (Id, Name, Description, PointsOfInterests), PointOfInterest (…, CarId)
Models/                    CarDto, CarWithoutPointsOfInterestDto, PointOfInterestDto,
                           PointOfInterestForCreationDto, PointOfInterestForUpdateDto
Profiles/                  CarProfile, PointOfInterestProfile (AutoMapper)
Services/                  ICarInfoRepository / CarInfoRepository, IMailService + Local/Cloud
Controllers/               CarsController, PointsOfInterestController, FilesController,
                           WeatherForecastController (template leftover)
Migrations/                InitialMigration, AddDescriptionToPoinOfInterest, InitialDataSeed
CarsDataStroe.cs           old in-memory store from modules 1–5, still registered as singleton
```

### Endpoints

| Route | Action | Repository call |
|---|---|---|
| `GET api/cars` | `GetCars` | `GetCarsAsync` |
| `GET api/cars/{id}?includePoi=` | `GetCar` | `GetCarAsync(id, includePoi)` |
| `GET api/cars/{carId}/pointsofinterest` | `GetPointsOfInterest` | `GetPointsOfInterestsAsync` |
| `GET api/cars/{carId}/pointsofinterest/{pointofinterestid}` | `GetPointOfInterest` | **wrong call**, see §4.1 |
| `POST api/cars/{carId}/pointsofinterest` | `CreatePointOfInterest` | `AddPointOfInterestForCityAsync` + `SaveChangesAsync` |
| `PATCH api/cars/{carId}/pointsofinterest` | `PartiallyUpdatePointsOfInterest` | see §4.3 |
| `PUT api/cars/{carId}/pointsofinterest/pointofinterestid` | `UpdatePointOfInterest` | see §4.4 |
| `DELETE api/cars/{carId}/pointsofinterest/{poi}` | `DeletePointOfInterest` | see §4.5 |
| `GET/POST api/files` | `FilesController` | none |

---

## 3. Decisions already made (do not re-litigate)

### 3.1 AutoMapper is pinned to 12.0.1 + the separate DI package

```xml
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
```

- The course uses AutoMapper 12, where `AddAutoMapper(...)` lives in the **separate**
  `AutoMapper.Extensions.Microsoft.DependencyInjection` package. Without it you get
  `IServiceCollection does not contain a definition for 'AddAutoMapper'`.
- The user first had **16.2.0**. From v15 on, `AddAutoMapper` requires a config action as the first
  argument (`AddAutoMapper(cfg => { }, typeof(Program))`), so the course's
  `AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())` gives `CS1503`. v15+ is also
  commercially licensed (logs a warning without `cfg.LicenseKey`).
- The user chose to downgrade to match the course. Build shows `NU1903` (known high-severity
  advisory GHSA-rvv3-g6hj-g44x for 12.0.1). Accepted for a learning project; mention it again only
  if they move toward anything real.

### 3.2 Repository returns `IEnumerable`, not `IQueryable`

Course choice, explained to the user at length (Readme items 29 and 31). If filtering/paging comes
later, use a parameters object (`CarQueryParameters`) and build the `IQueryable` **inside** the
repository; return `IEnumerable`.

### 3.3 All repository methods are async (`Task<…>`, `…Async` suffix)

Except `DeletePointOfInterest` (sync, only marks the entity for removal). Readme item 30.

---

## 4. Known bugs (all in `PointsOfInterestController.cs` unless stated)

### 4.1 GET single POI returns the whole list — line 55
`GetPointOfInterest` calls `GetPointsOfInterestsAsync(carId)` and ignores `pointofinterestid`.
Should call `GetPointOfInterestsAsync(carId, pointofinterestid)`, 404 when null, map to
`PointOfInterestDto`, and return type `ActionResult<PointOfInterestDto>`.
Also line 34: return type is `IEnumerable<List<PointOfInterestDto>>` (nested), should be
`IEnumerable<PointOfInterestDto>`.

### 4.2 POST does not await the save — line 71 (`CS4014`)
`_carInfoRepository.SaveChangesAsync();` without `await`. The response is built before the
INSERT finishes, so `createdPOIToReturn.Id` can be `0`, and the scoped `DbContext` can be used
concurrently. Add `await`.

### 4.3 PATCH is broken three ways — lines 83–112
- No route template: `[HttpPatch]` should be `[HttpPatch("{pointofinterestid}")]`; currently
  `pointOfInterestId` only binds from the query string.
- Line 91: `GetPointOfInterestsAsync(...)` is **not awaited**, so `poi` is a `Task`, never null,
  and `_mapper.Map<PointOfInterestForUpdateDto>(poi)` throws (no Task → DTO map).
- Line 110 (`CS4014`) is the same root cause: `_mapper.Map(poiToPatch, poi)` maps onto the Task.

### 4.4 PUT route is a literal string — line 117
`[HttpPut("pointofinterestid")]` is missing braces, so the URL is literally
`.../pointsofinterest/pointofinterestid` and the id comes from the query string.
Should be `[HttpPut("{pointofinterestid}")]`.

### 4.5 DELETE never matches and never saves — lines 138–151
- Route parameter is `{poi}` but the action parameter is `pointOfInterestId`, so it stays `0`
  and the endpoint always returns 404. Use `[HttpDelete("{pointofinterestid}")]`.
- After `DeletePointOfInterest(poi)` there is no `await _carInfoRepository.SaveChangesAsync();`,
  so nothing is removed from the database even when the route is fixed.

### 4.6 `GET api/cars/{id}?includePoi=true` returns an empty POI list
`Car.PointsOfInterests` (entity, plural "s") vs `CarDto.PointsOfInterest` (DTO). AutoMapper
matches by name, so the collection is not mapped and `NumberOfPointsOfInterest` is always 0.
Either rename one side or add `.ForMember(d => d.PointsOfInterest, o => o.MapFrom(s => s.PointsOfInterests))`
in `CarProfile`.

### 4.7 Smaller items
- `Program.cs:33` still registers `CarsDataStroe` (old in-memory store). Nothing uses it any more.
- `Program.cs:90-94`: `UseEndpoints(... MapControllers())` **and** `app.MapControllers()`
  (`ASP0014`). Keep only `app.MapControllers()`.
- `CarInfoRepository.AddPointOfInterestForCityAsync`: course name leftover; rename to
  `…ForCarAsync` if the user wants consistency (touch interface + controller).
- Nullable warnings (`CS8618`/`CS8601`) on `Description` properties and mail services. Harmless.
- `CarsInfo.db`, `-shm`, `-wal` and `Boy Artmı.pptx` are tracked in git.

---

## 5. Build status

```
dotnet build CarsInfo.Api   ->  Build succeeded, 0 Error(s)
```

Warnings: `NU1903` (AutoMapper 12.0.1, §3.1), two `CS4014` (§4.2, §4.3), `ASP0014` (§4.7),
nullable warnings (§4.7).

---

## 6. Next steps

1. Fix §4.1–§4.5 (each is a one- or two-line change). Then run the API and try every
   POI endpoint from Swagger against the SQLite db.
2. Fix §4.6 and check `GET api/cars/1?includePoi=true` returns the points of interest.
3. Optional cleanup from §4.7.
4. Continue with course module 8. Filtering, searching and paging will likely come there; the
   `CarQueryParameters` approach in Readme item 31 is the plan.
