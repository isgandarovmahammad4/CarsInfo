using Asp.Versioning;
using AutoMapper;
using CarsInfo.Api.Models;
using CarsInfo.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CarsInfo.Api.Controllers
{
    [ApiController]
//    [Authorize]
    [Route("api/v{version:apiVersion}/cars")]
    [ApiVersion(1)]
    public class CarsController : ControllerBase
    {
        private readonly ICarInfoRepository _carInfoRepository;
        private readonly IMapper _mapper;
        const int maxPageSize = 20;

        public CarsController(ICarInfoRepository carInfoRepository, IMapper mapper)
        {
            _carInfoRepository = carInfoRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<(IEnumerable<CarWithoutPointsOfInterestDto>, PaginationMetadata)>> GetCarsAsync(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxPageSize)
            {
                pageSize = maxPageSize;
            }

            var (cars, paginationMetadata) = await _carInfoRepository.GetCarsAsync(name, searchQuery, pageNumber, pageSize);

            Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationMetadata));

            return Ok(_mapper.Map<IEnumerable<CarWithoutPointsOfInterestDto>>(cars));

        }

        /// <summary>
        /// Get a car by id
        /// </summary>
        /// <param name="id">The id of the car to get</param>
        /// <param name="includeDetails">Whether or not to include the car details</param>
        /// <returns>A car with or without details</returns>
        /// <response code="200">Returns the requested car</response>

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCar(int id, bool includePoi = false)
        {
            var car = await _carInfoRepository.GetCarAsync(id, includePoi);
            if (car == null)
            {
                return NotFound();
            }

            if (includePoi)
            {
                return Ok(_mapper.Map<CarDto>(car));
            }

            return Ok(_mapper.Map<CarWithoutPointsOfInterestDto>(car));

        }
    }
}
