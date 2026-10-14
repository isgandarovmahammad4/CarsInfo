using CarsInfo.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarsInfo.Api.Controllers
{
    [ApiController]
    [Route("api/cars")]
    public class CarsController : ControllerBase
    {
        private readonly CarsDataStroe carsDataStroe;

        public CarsController(CarsDataStroe dataStroe)
        {
            carsDataStroe = dataStroe;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CarDto>> Get()
        {
            return carsDataStroe.Cars;
            //return Ok(car);

        }

        [HttpGet("{id}")]
        public ActionResult GetCar(int id)
        {
            var car = carsDataStroe.Cars.FirstOrDefault(x => x.Id == id);
            if (car == null) 
            { 
                return NotFound();
            }
            return Ok(car);
                
        }
    }
}
