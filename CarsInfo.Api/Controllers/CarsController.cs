using CarsInfo.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarsInfo.Api.Controllers
{
    [ApiController]
    [Route("api/cars")]
    public class CarsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<CarDto>> Get()
        {
            return CarsDataStroe.Current.Cars;
            //return Ok(car);

        }

        [HttpGet("{id}")]
        public ActionResult GetCar(int id)
        {
            var car = CarsDataStroe.Current.Cars.FirstOrDefault(x => x.Id == id);
            if (car == null) 
            { 
                return NotFound();
            }
            return Ok(car);
                
        }
    }
}
