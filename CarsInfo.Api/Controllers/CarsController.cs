using AutoMapper;
using CarsInfo.Api.Models;
using CarsInfo.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarsInfo.Api.Controllers
{
    [ApiController]
    [Route("api/cars")]
    public class CarsController : ControllerBase
    {
        private readonly ICarInfoRepository _carInfoRepository;
        private readonly IMapper _mapper;

        public CarsController(ICarInfoRepository carInfoRepository, IMapper mapper)
        {
            _carInfoRepository = carInfoRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarWithoutPointsOfInterestDto>>> GetCars()
        {
            var cars = await _carInfoRepository.GetCarsAsync();

            return Ok(_mapper.Map<IEnumerable<CarWithoutPointsOfInterestDto>>(cars));

        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetCar(int id)
        {
            var car = await _carInfoRepository.GetCarAsync(id, false);
            if (car == null)
            {
                return NotFound();
            }
            return Ok(car);

        }
    }
}
