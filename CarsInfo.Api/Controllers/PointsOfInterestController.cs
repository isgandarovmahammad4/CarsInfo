using CarsInfo.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CarsInfo.Api.Controllers
{
    [Route("api/cars/{carId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {

        private readonly ILogger<PointsOfInterestController> _logger;
        public PointsOfInterestController(ILogger<PointsOfInterestController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public ActionResult<IEnumerable<List<PointOfInterestDto>>> GetPointsOfInterest(int carId)
        {
            throw new Exception("Sample");
            try
            {
                var car = CarsDataStroe.Current.Cars.FirstOrDefault(x => x.Id == carId);
                if (car == null)
                {
                    return NotFound();
                }
                _logger.LogInformation($"Points of interest for car with id {carId} were requested.");
                return Ok(car.PointsOfInterest.FirstOrDefault());

            }
            catch (Exception exception)
            {

                _logger.LogCritical($"Exception while getting points of interest for car with id {carId}.", exception);
                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointOfInterest(int carId, int pointofinterestid)
        {
            var car = CarsDataStroe.Current.Cars.FirstOrDefault(x => x.Id == carId);
            if (car == null)
            {
                return NotFound("Car isnt exist.");
            }
            var pointOfInterest = car.PointsOfInterest.FirstOrDefault(x => x.Id == pointofinterestid);

            if (pointOfInterest == null)
            {
                return NotFound();
            }
            return Ok(pointOfInterest);
        }
        [HttpPost]
        public ActionResult<PointOfInterestDto> CreatePointOfInterest(int carId, PointOfInterestForCreationDto pointOfInterestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var car = CarsDataStroe.Current.Cars.FirstOrDefault(x => x.Id == carId);
            if (car == null)
            {
                return NotFound("Car isnt exist.");
            }
            var maxPointOfInterest = CarsDataStroe.Current.Cars.SelectMany(x => x.PointsOfInterest).Max(x => x.Id);

            var finalPointOfInterest = new PointOfInterestDto
            {
                Id = ++maxPointOfInterest,
                Name = pointOfInterestDto.Name,
                Description = pointOfInterestDto.Description
            };

            car.PointsOfInterest.Add(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest", new
            {
                carId = car.Id,
                pointofinterestid = finalPointOfInterest.Id
            }, finalPointOfInterest);

        }

        [HttpPatch]
        public ActionResult<PointOfInterestDto> PartiallyUpdatePointsOfInterest(int carId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var car = CarsDataStroe.Current.Cars.FirstOrDefault(x => x.Id == carId);
            if (car == null)
            {
                return NotFound("Car isnt exist.");
            }

            var poi = car.PointsOfInterest.FirstOrDefault(x => x.Id == pointOfInterestId);

            if (poi == null)
            {
                return NotFound();
            }

            var poiToPatch = new PointOfInterestForUpdateDto
            {
                Name = poi.Name,
                Description = poi.Description
            };

            patchDocument.ApplyTo(poiToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!TryValidateModel(poiToPatch))
            {
                return BadRequest(ModelState);
            }
            poi.Name = poiToPatch.Name;
            poi.Description = poiToPatch.Description;

            return NoContent();
        }

        [HttpPut("pointofinterestid")]
        public ActionResult<PointOfInterestDto> UpdatePointOfInterest(int carId, int pointOfInterestId, PointOfInterestForUpdateDto updateDto)
        {
            var car = CarsDataStroe.Current.Cars.FirstOrDefault(c => c.Id == carId);
            if (car == null)
            {
                return NotFound();
            }
            var poi = car.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);
            if (poi == null)
            {
                return NotFound();
            }
            poi.Name = updateDto.Name;
            poi.Description = updateDto.Description;

            return NoContent();
        }
        [HttpDelete("{poi}")]
        public ActionResult DeletePointOfInterest(int carId, int pointOfInterestId)
        {
            var car = CarsDataStroe.Current.Cars.FirstOrDefault(c => c.Id == carId);
            if (car == null)
            {
                return NotFound();
            }
            var poi = car.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);
            if (poi == null)
            {
                return NotFound();
            }
            car.PointsOfInterest.Remove(poi);
            return NoContent();
        }
    }
}
