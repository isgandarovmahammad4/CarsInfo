using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CarsInfo.Api;
using CarsInfo.Api.DbContexts;
using CarsInfo.Api.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Security.Cryptography;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/carsinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = false;
})
    .AddNewtonsoftJson()
    .AddXmlDataContractSerializerFormatters();

builder.Services.AddProblemDetails();
#if DEBUG
builder.Services.AddTransient<CarsInfo.Api.Services.IMailService, CarsInfo.Api.Services.LocalMailService>();
#else
builder.Services.AddTransient<CarsInfo.Api.Services.IMailService, CarsInfo.Api.Services.CloudMailService>();
#endif
//we can now path dbcontext options and we can say we want to use sqlite on it. We need to provide a connection string. 
//We can do that by using the configuration object that is available in the program.cs file. We can use the configuration object to get the connection string from the appsettings.json file. 
//So let's do that now. Let's add a connection string to the appsettings.json file. Let's call it "DefaultConnection". The value of the connection string will be "Data Source=CarsInfo.db". 
//This will create a sqlite database file called CarsInfo.db in the root of the project. Now we can use this connection string in our program.cs file.
//We can use the configuration object to get the connection string and pass it to the UseSqlite method.
//builder.Services.AddDbContext<CarContext>((IServiceProvider provider, DbContextOptionsBuilder optionsBuilder) =>
//{
//    optionsBuilder.UseSqlite("Data Source=CarsInfo.db");
//});

//Bunu bele de yazmaq olar

builder.Services.AddDbContext<CarContext>(optionsBuilder =>
{
    optionsBuilder.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});


//builder.Services.AddProblemDetails(options =>
//{
//    options.CustomizeProblemDetails = ctx =>
//    {
//        ctx.ProblemDetails.Extensions.Add("additionalInfo", "Additional");
//        ctx.ProblemDetails.Extensions.Add("Server", "Donut Request");
//    };
//});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<FileExtensionContentTypeProvider>();
builder.Services.AddScoped<ICarInfoRepository, CarInfoRepository>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var bearerConfig = builder.Configuration.GetSection("Authentication:Schemes:Bearer");
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = bearerConfig["ValidIssuer"],
            ValidAudiences = bearerConfig.GetSection("ValidAudiences").Get<string[]>(),
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromCar1", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("Car 1", "Car 1");
    });
});

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.ReportApiVersions = true;
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
})
.AddMvc()
.AddApiExplorer(setupAction =>
{
    setupAction.SubstituteApiVersionInUrl = true;
});

var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider()
    .GetRequiredService<IApiVersionDescriptionProvider>();

builder.Services.AddSwaggerGen(setupAction =>
{
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        setupAction.SwaggerDoc(
            description.GroupName,
            new OpenApiInfo
            {
                Title = "CarsInfo API",
                Version = description.ApiVersion.ToString(),
                Description = "Through this API you can access cars and their points of interest."
            });
    }

    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);
    setupAction.IncludeXmlComments(xmlCommentsFullPath);

    setupAction.AddSecurityDefinition("CarInfoApiBearerAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Input a valid token to access this API",
    });

    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "CarInfoApiBearerAuth"
            }
        }] = Array.Empty<string>()
    });
});

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler();
    }


    // Configure the HTTP request pipeline. Theese are also middlewares that will be executed in the order they are added to the pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(setupAction =>
        {
            var descriptions = app.DescribeApiVersions();
            foreach (var description in descriptions)
            {
                setupAction.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });
    }
    app.UseRouting();

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
