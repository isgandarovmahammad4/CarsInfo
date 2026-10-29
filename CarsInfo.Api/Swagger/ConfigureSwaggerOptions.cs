using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CarsInfo.Api.Swagger
{
    /// <summary>
    /// Registers one Swagger document per API version. Runs through DI, so the
    /// version provider is injected instead of building a second service provider.
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo
                    {
                        Title = "CarsInfo API",
                        Version = description.ApiVersion.ToString(),
                        Description = "Through this API you can access cars and their points of interest."
                    });
            }
        }
    }
}
