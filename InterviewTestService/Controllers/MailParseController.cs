using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InterviewTestService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailParseController : ControllerBase
    {
        public MailParseController() { }

        [HttpPost("parsedata")]
        public IActionResult ParsedData([FromBody] MailData data)
        {
            return Ok("Successfully Parsed");
        }
        public class MailData
        {
            public string data { get; set; }
        }

    }
}
