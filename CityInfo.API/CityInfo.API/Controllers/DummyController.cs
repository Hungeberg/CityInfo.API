using CityInfo.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    public class DummyController : Controller
    {
        private CityInfoContext _ctx;

        public DummyController(CityInfoContext ctx)
        {
            _ctx = ctx;
        }

        /// <summary>
        /// Test the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/testdatabase")]
        [ProducesResponseType(typeof(void), 200)]
        public IActionResult TestDatabase()
        {
            return Ok();
        }
    }
}
