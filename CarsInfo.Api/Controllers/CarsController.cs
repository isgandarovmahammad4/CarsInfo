using AutoMapper;
using CarsInfo.Api.Models;
using CarsInfo.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CarsInfo.Api.Controllers
{
    [ApiController]
    [Route("api/cars")]
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
        public async Task<ActionResult<IEnumerable<CarWithoutPointsOfInterestDto>>> GetCars(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxPageSize)
            {
                pageSize = maxPageSize;
            }

            var cars = await _carInfoRepository.GetCarsAsync(name, searchQuery, pageNumber, pageSize);

            return Ok(_mapper.Map<IEnumerable<CarWithoutPointsOfInterestDto>>(cars));

        }

        [HttpGet("{id}")]
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
