using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using TaskManagement.Identity.Dtos;
using TaskManagement.Identity.Interfaces;

namespace TaskManagement.Identity.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (result == null)
                return Conflict("Email is in use");

            return Created("", result); // har ik url til hvor den user lavet (ingen GetById). Auth klare kun login + register 
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null)
                return Unauthorized("invalid email or password");

            return Ok(result);
        }
    }
}
