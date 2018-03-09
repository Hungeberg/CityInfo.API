using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CityInfo.API.Controllers
{
    [Produces("application/json")]
    [Route("api/cities")]
    public class PointsOfInterestController : Controller
    {
        private ICityInfoRepository _cityInfoRepository;
        private ILogger<PointsOfInterestController> _logger;
        private IMailService _mailService;

        public PointsOfInterestController(
            ILogger<PointsOfInterestController> logger,
            IMailService mailService,
            ICityInfoRepository cityInfoRepository)
        {
            _cityInfoRepository = cityInfoRepository;
            _logger = logger;
            _mailService = mailService;
        }

        [HttpGet("{cityId}/poi")]
        [ProducesResponseType(typeof(IEnumerable<PointOfInterestDto>), 200)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            try
            {
                // Look for the city...
                if (!_cityInfoRepository.CityExists(cityId))
                {
                    _logger.LogInformation($"City with id {cityId} was not found when accessing points of interest.");
                    return NotFound(); // return 404 not found status result.
                }

                var pointsOfInterestForCity = _cityInfoRepository.GetPointsOfInterestForCity(cityId);
                var pointsOfInterestForCityResults = 
                    Mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity);
                return Ok(pointsOfInterestForCityResults);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        [HttpGet("{cityId}/poi/{poiId}", Name = "GetPointOfInterest")]
        [ProducesResponseType(typeof(PointOfInterestDto), 200)]
        [ProducesResponseType(typeof(void), 404)]
        public IActionResult GetPointOfInterest(int cityId, int poiId)
        {
            // Look for the city...
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound(); // return 404 not found status result.
            }

            // Look for the city poi...
            var poi = _cityInfoRepository.GetPointOfInterestForCity(cityId, poiId);

            if (poi == null)
            {
                return NotFound(); // return 404 not found status result.
            }
            var poiResult = Mapper.Map<PointOfInterestDto>(poi);
            return Ok(poiResult);
        }

        [HttpPost("{cityId}/poi")]
        [ProducesResponseType(typeof(PointOfInterestDto), 201)]
        [ProducesResponseType(typeof(string), 500)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 400)]
        public IActionResult CreatePointOfInterest(int cityId,
            [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            // Check data input...
            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            // Check for same name and description...
            if (pointOfInterest.Name == pointOfInterest.Description)
            {
                ModelState.AddModelError(
                    "Description",
                    "The provided description should be different from the name.");
            }

            // Validate data input agains model data annotations...
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Look for the city...
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound(); // return 404 not found status result.
            }

            var finalPoi = Mapper.Map<PointOfInterest>(pointOfInterest);
            _cityInfoRepository.AddPointOfInterestForCity(cityId, finalPoi);
            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            var createdPoi = Mapper.Map<PointOfInterestDto>(finalPoi);

            return CreatedAtRoute(
                "GetPointOfInterest",
                new { cityId, poiId = createdPoi.Id },
                createdPoi);
        }

        [HttpPut("{cityId}/poi/{poiId}")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(string), 500)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 400)]
        public IActionResult UpdatePointOfInterest(int cityId, int poiId,
            [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
            // Check data input...
            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            // Check for same name and description...
            if (pointOfInterest.Name == pointOfInterest.Description)
            {
                ModelState.AddModelError(
                    "Description",
                    "The provided description should be different from the name.");
            }

            // Validate data input agains model data annotations...
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Look for the city...
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound(); // return 404 not found status result.
            }

            // Look for the city poi...
            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, poiId);

            if (pointOfInterestEntity == null)
            {
                return NotFound(); // return 404 not found status result.
            }
            Mapper.Map(pointOfInterest, pointOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            return NoContent();
        }

        [HttpPatch("{cityId}/poi/{poiId}")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(string), 500)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 400)]
        public IActionResult PartialUpdatePointOfInterest(int cityId, int poiId,
            [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            // Look for the city...
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound(); // return 404 not found status result.
            }

            // Look for the city poi...
            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, poiId);

            if (pointOfInterestEntity == null)
            {
                return NotFound(); // return 404 not found status result.
            }

            var poiToPatch = Mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

            patchDoc.ApplyTo(poiToPatch, ModelState);

            // Validate data input agains model data annotations...
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check for same name and description...
            if (poiToPatch.Name == poiToPatch.Description)
            {
                ModelState.AddModelError(
                    "Description",
                    "The provided description should be different from the name.");
            }

            TryValidateModel(poiToPatch);

            // Validate data input agains model data annotations...
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Mapper.Map(poiToPatch, pointOfInterestEntity);
            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }
            return NoContent();
        }

        [HttpDelete("{cityId}/poi/{poiId}")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(string), 500)]
        [ProducesResponseType(typeof(void), 404)]
        public IActionResult DeletePointOfInterest(int cityId, int poiId)
        {
            // Look for the city...
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound(); // return 404 not found status result.
            }

            // Look for the city poi...
            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, poiId);

            if (pointOfInterestEntity == null)
            {
                return NotFound(); // return 404 not found status result.
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            _mailService.Send("Point of interest deleted.",
                $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

            return NoContent();
        }
    }
}