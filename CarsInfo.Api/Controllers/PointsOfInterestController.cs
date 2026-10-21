using AutoMapper;
using CarsInfo.Api.Entities;
using CarsInfo.Api.Models;
using CarsInfo.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarsInfo.Api.Controllers
{
    [Route("api/cars/{carId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {

        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly CarsInfo.Api.Services.IMailService _mailService;
        private readonly ICarInfoRepository _carInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            CarsInfo.Api.Services.IMailService mailService,
            ICarInfoRepository carInfoRepository,
            IMapper mapper)

        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _carInfoRepository = carInfoRepository ?? throw new ArgumentNullException(nameof(carInfoRepository));
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<List<PointOfInterestDto>>>> GetPointsOfInterest(int carId)
        {
            if (!await _carInfoRepository.CarExist(carId))
            {
                _logger.LogInformation($"Car with id: {carId} doesnt exist.");
                return NotFound();
            }

            var poi = await _carInfoRepository.GetPointsOfInterestsAsync(carId);
            return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(poi));
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointOfInterest(int carId, int pointofinterestid)
        {
            if (!await _carInfoRepository.CarExist(carId))
            {
                _logger.LogInformation("City doesnt exist.");
                return NotFound();
            }
            var pointsOfInterestForCar = await _carInfoRepository.GetPointsOfInterestsAsync(carId);

            return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCar));
        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int carId, PointOfInterestForCreationDto pointOfInterestDto)
        {
            if (!await _carInfoRepository.CarExist(carId))
            {
                return NotFound();
            }

            var finalPointOfInterest = _mapper.Map<PointOfInterest>(pointOfInterestDto);

            await _carInfoRepository.AddPointOfInterestForCityAsync(carId, finalPointOfInterest);
            _carInfoRepository.SaveChangesAsync();

            var createdPOIToReturn = _mapper.Map<PointOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest", new
            {
                carId = carId,
                pointofinterestid = createdPOIToReturn.Id
            }, createdPOIToReturn);

        }

        [HttpPatch]
        public ActionResult<PointOfInterestDto> PartiallyUpdatePointsOfInterest(int carId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var car = _carInfoRepository.Cars.FirstOrDefault(x => x.Id == carId);
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
            var car = _carInfoRepository.Cars.FirstOrDefault(c => c.Id == carId);
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
            var car = _carInfoRepository.Cars.FirstOrDefault(c => c.Id == carId);
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
            _mailService.Send("Point of interest deleted.", $"Point of interest {poi.Name} with id {poi.Id} was deleted.");
            return NoContent();
        }
    }
}
