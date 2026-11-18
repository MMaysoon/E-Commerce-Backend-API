using E_commerce.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;

        public EmailTestController(IEmailService emailService, IWebHostEnvironment environment)
        {
            _emailService = emailService;
            _environment = environment;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromQuery] string to)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            await _emailService.SendEmailAsync(
                to,
                "SMTP Test",
                "<h1>Hello!</h1><p>SMTP is working successfully.</p>"
            );

            return Ok(new
            {
                Message = "Email sent successfully."
            });
        }
    }
}
