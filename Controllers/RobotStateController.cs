using MeRobot.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeRobot.Controllers
{
    [ApiController]
    [Route("robot")]
    public class RobotStateController : ControllerBase
    {
        private readonly RobotService _robot;

        public RobotStateController(RobotService robot)
        {
            _robot = robot;
        }

        [HttpGet("state")]
        public IActionResult GetState()
        
        {
            return Ok(_robot.GetState());
        }
    }
}
