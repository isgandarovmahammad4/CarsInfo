using Asp.Versioning;
using AutoMapper;
using CarsInfo.Api.Entities;
using CarsInfo.Api.Models;
using CarsInfo.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarsInfo.Api.Controllers
{
    [Route("api/cars/{carId}/pointsofinterest")]
//    [Authorize(Policy = "MustBeFromCar1")]
    [ApiController]
    [ApiVersion(2)]
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
            //var carname = User.Claims.FirstOrDefault(c=>c.Type == "Car 1")?.Value;

            //if(!await _carInfoRepository.CarNameMatchesCarID(carname,carId))
            //{
            //    return Forbid();
            //}
            

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
        public async Task<ActionResult<PointOfInterestDto>> PartiallyUpdatePointsOfInterest(int carId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            if (!await _carInfoRepository.CarExist(carId))
            {
                return NotFound();
            }

            var poi = _carInfoRepository.GetPointOfInterestsAsync(carId, pointOfInterestId);
            if (poi == null)
            {
                return NotFound();
            }

            var poiToPatch = _mapper.Map<PointOfInterestForUpdateDto>(poi);

            patchDocument.ApplyTo(poiToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!TryValidateModel(poiToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(poiToPatch, poi);

            await _carInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("pointofinterestid")]
        public async Task<ActionResult<PointOfInterestDto>> UpdatePointOfInterest(int carId, int pointOfInterestId, PointOfInterestForUpdateDto updateDto)
        {
            if (!await _carInfoRepository.CarExist(carId))
            {
                return NotFound();
            }
            var poi = await _carInfoRepository.GetPointOfInterestsAsync(carId, pointOfInterestId);
            if (poi == null)
            {
                return NotFound();
            }

            _mapper.Map(updateDto, poi);

            await _carInfoRepository.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{poi}")]
        public async Task<ActionResult> DeletePointOfInterest(int carId, int pointOfInterestId)
        {
            if (!await _carInfoRepository.CarExist(carId))
            {
                return NotFound();
            }
            var poi = await _carInfoRepository.GetPointOfInterestsAsync(carId, pointOfInterestId);
            if (poi == null)
            {
                return NotFound();
            }

            _carInfoRepository.DeletePointOfInterest(poi);

            _mailService.Send("Point of interest deleted.", $"Point of interest {poi.Name} with id {poi.Id} was deleted.");
            return NoContent();
        }
    }
}
