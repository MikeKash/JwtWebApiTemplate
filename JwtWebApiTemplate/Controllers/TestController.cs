using JwtWebApiTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtWebApiTemplate.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetTestRoute"), Authorize]
        public Test Get()
        {
            return new Test
            {
                Date = DateTime.Now,
                Message = "Well, looks like it works!"
            };
        }
    }
}