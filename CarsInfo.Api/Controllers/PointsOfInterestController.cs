using Asp.Versioning;
using CarsInfo.Api.Mapping;
using CarsInfo.Api.Models;
using CarsInfo.Api.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarsInfo.Api.Controllers
{
    [Route("api/v{version:apiVersion}/cars/{carId}/pointsofinterest")]
    [Authorize]
    [ApiController]
    [ApiVersion(2)]
    public class PointsOfInterestController : ControllerBase
    {

        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICarInfoRepository _carInfoRepository;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            IMailService mailService,
            ICarInfoRepository carInfoRepository)

        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _carInfoRepository = carInfoRepository ?? throw new ArgumentNullException(nameof(carInfoRepository));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int carId)
        {
            if (!await CanAccessCar(carId)) return Forbid();

            if (!await _carInfoRepository.CarExist(carId))
            {
                _logger.LogInformation("Car with id {CarId} doesn't exist.", carId);
                return NotFound();
            }

            var poi = await _carInfoRepository.GetPointsOfInterestsAsync(carId);
            return Ok(poi.ToDtos());
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int carId, int pointofinterestid)
        {
            if (!await CanAccessCar(carId)) return Forbid();
            if (!await _carInfoRepository.CarExist(carId))
            {
                _logger.LogInformation("Car with id {CarId} doesn't exist.", carId);
                return NotFound();
            }
            var pointOfInterest = await _carInfoRepository.GetPointOfInterestsAsync(carId, pointofinterestid);
            if (pointOfInterest == null)
            {
                return NotFound();
            }
            return Ok(pointOfInterest.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int carId, PointOfInterestForCreationDto pointOfInterestDto)
        {
            //if (!await CanAccessCar(carId)) return Forbid();
            if (!await _carInfoRepository.CarExist(carId))
            {
                return NotFound();
            }

            var finalPointOfInterest = pointOfInterestDto.ToEntity();

            await _carInfoRepository.AddPointOfInterestForCarAsync(carId, finalPointOfInterest);
            await _carInfoRepository.SaveChangesAsync();

            var createdPOIToReturn = finalPointOfInterest.ToDto();

            return CreatedAtRoute("GetPointOfInterest", new
            {
                carId = carId,
                pointofinterestid = createdPOIToReturn.Id
            }, createdPOIToReturn);

        }

        [HttpPatch("{pointofinterestid}")]
        public async Task<ActionResult<PointOfInterestDto>> PartiallyUpdatePointsOfInterest(int carId, int pointofinterestid, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            if (!await CanAccessCar(carId)) return Forbid();
            if (patchDocument == null)
            {
                return BadRequest();
            }
            if (!await _carInfoRepository.CarExist(carId))
            {
                return NotFound();
            }

            var poi = await _carInfoRepository.GetPointOfInterestsAsync(carId, pointofinterestid);
            if (poi == null)
            {
                return NotFound();
            }

            var poiToPatch = poi.ToUpdateDto();

            patchDocument.ApplyTo(poiToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!TryValidateModel(poiToPatch))
            {
                return BadRequest(ModelState);
            }

            poiToPatch.ApplyTo(poi);

            await _carInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult<PointOfInterestDto>> UpdatePointOfInterest(int carId, int pointofinterestid, PointOfInterestForUpdateDto updateDto)
        {
            if (!await CanAccessCar(carId)) return Forbid();
            if (!await _carInfoRepository.CarExist(carId))
            {
                return NotFound();
            }
            var poi = await _carInfoRepository.GetPointOfInterestsAsync(carId, pointofinterestid);
            if (poi == null)
            {
                return NotFound();
            }

            updateDto.ApplyTo(poi);

            await _carInfoRepository.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{pointofinterestid}")]
        public async Task<ActionResult> DeletePointOfInterest(int carId, int pointofinterestid)
        {
            if (!await CanAccessCar(carId)) return Forbid();
            if (!await _carInfoRepository.CarExist(carId))
            {
                return NotFound();
            }
            var poi = await _carInfoRepository.GetPointOfInterestsAsync(carId, pointofinterestid);
            if (poi == null)
            {
                return NotFound();
            }

            _carInfoRepository.DeletePointOfInterest(poi);
            await _carInfoRepository.SaveChangesAsync();

            _mailService.Send("Point of interest deleted.", $"Point of interest {poi.Name} with id {poi.Id} was deleted.");
            return NoContent();
        }

        private Task<bool> CanAccessCar(int carId)
        {
            var carName = User.FindFirst("car")?.Value;
            return _carInfoRepository.CarNameMatchesCarID(carName, carId);
        }
    }
}
