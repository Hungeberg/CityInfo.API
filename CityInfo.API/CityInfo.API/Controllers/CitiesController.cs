using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CityInfo.API.Controllers
{
    [Produces("application/json")]
    [Route("api/cities")]
    public class CitiesController : Controller
    {
        private ICityInfoRepository _cityInfoRepository;

        public CitiesController(ICityInfoRepository cityInfoRepository)
        {
            _cityInfoRepository = cityInfoRepository;
        }

        /// <summary>
        /// Get list of cities
        /// </summary>
        /// <returns>List of cities</returns>
        /// <response code="200">Returns the list of cities</response>
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<CityWithoutPointsOfInterestDto>), 200)]
        public IActionResult GetCities()
        {
            var cityEntities = _cityInfoRepository.GetCities();
            var results = Mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities);
            return Ok(results);
        }

        /// <summary>
        /// Get city by id
        /// </summary>
        /// <param name="id">City ID</param>
        /// <param name="includePointsOfInterest">Option to include points of interest</param>
        /// <returns>City</returns>
        /// <response code="200">Returns the city if found</response>
        /// <response code="404">If the city is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CityWithoutPointsOfInterestDto), 200)]
        [ProducesResponseType(typeof(CityDto), 200)]
        [ProducesResponseType(typeof(void), 404)]
        public IActionResult GetCity(int id, bool includePointsOfInterest = false)
        {
            // Look for the city...
            var city = _cityInfoRepository.GetCity(id, includePointsOfInterest);

            if (city == null)
            {
                return NotFound(); // return 404 not found status result.
            }
            
            if (includePointsOfInterest)
            {
                var cityResult = Mapper.Map<CityDto>(city);
                return Ok(cityResult);
            }

            var cityWithoutPoiResult = Mapper.Map<CityWithoutPointsOfInterestDto>(city);
            return Ok(cityWithoutPoiResult);
        }
    }
}
