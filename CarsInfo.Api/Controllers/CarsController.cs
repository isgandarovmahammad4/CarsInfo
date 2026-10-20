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

        public CarsController(ICarInfoRepository carInfoRepository)
        {
            _carInfoRepository = carInfoRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarWithoutPointsOfInterestDto>>> GetCars()
        {
            var cars = await _carInfoRepository.GetCarsAsync();

            var carsWithoutPoi = new List<CarWithoutPointsOfInterestDto>();
            foreach (var car in cars)
            {
                carsWithoutPoi.Add(new CarWithoutPointsOfInterestDto
                {
                    Id = car.Id,
                    Name = car.Name,
                    Description = car.Description
                });

            }

            return Ok(carsWithoutPoi);

        }

        [HttpGet("{id}")]
        public ActionResult GetCar(int id)
        {
            var car = _carInfoRepository.Cars.FirstOrDefault(x => x.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            return Ok(car);

        }
    }
}
