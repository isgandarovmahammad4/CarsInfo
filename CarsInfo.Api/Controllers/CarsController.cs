using Asp.Versioning;
using CarsInfo.Api.Mapping;
using CarsInfo.Api.Models;
using CarsInfo.Api.Services;
using CarsInfo.Api.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CarsInfo.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v{version:apiVersion}/Cars")]
    [ApiVersion(1)]
    public class CarsController : ControllerBase
    {
        private readonly ICarInfoRepository _carInfoRepository;
        private const int MaxPageSize = 20;

        public CarsController(ICarInfoRepository carInfoRepository)
        {
            _carInfoRepository = carInfoRepository;
        }

        [HttpGet]
        public async Task<ActionResult<(IEnumerable<CarWithoutPointsOfInterestDto>, PaginationMetadata)>> GetCarsAsync(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var (cars, paginationMetadata) = await _carInfoRepository.GetCarsAsync(name, searchQuery, pageNumber, pageSize);

            Response.Headers["X-Pagination"] = System.Text.Json.JsonSerializer.Serialize(paginationMetadata);

            return Ok(cars.ToCarWithoutPointsOfInterestDtos());

        }

        /// <summary>
        /// Get a car by id
        /// </summary>
        /// <param name="id">The id of the car to get</param>
        /// <param name="includeDetails">Whether or not to include the car details</param>
        /// <returns>A car with or without details</returns>
        /// <response code="200">Returns the requested car</response>

        [HttpGet("{carId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCar(int carId, bool includePoi = false)
        {
            var car = await _carInfoRepository.GetCarAsync(carId, includePoi);
            if (car == null)
            {
                return NotFound();
            }

            if (includePoi)
            {
                return Ok(car.ToCarDto());
            }

            return Ok(car.ToCarWithoutPointsOfInterestDto());

        }
    }
}
