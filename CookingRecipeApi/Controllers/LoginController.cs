using AutoMapper;
using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.LoginRequests;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.BusinessServices.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CookingRecipeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IMapper _mapper;

        public LoginController(ILoginService loginService, IMapper mapper)
        {
            _loginService = loginService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = await _loginService.Register(request);
            if(user == null)
            {
                return BadRequest("Invalid request");
            }
            return Ok(user);
        }
        [HttpPost("login")]
        public Task<IActionResult> Login([FromBody] User user)
        {
            return Task.FromResult<IActionResult>(Ok("LoginController"));
        }
        [HttpPost("logout")]
        public Task<IActionResult> Logout([FromBody] User user)
        {
            return Task.FromResult<IActionResult>(Ok("LoginController"));
        }
        [HttpPost("forgot-password")]
        public Task<IActionResult> ForgotPassword([FromBody] User user)
        {
            return Task.FromResult<IActionResult>(Ok("LoginController"));
        }
        [HttpPost("login-facebook")]
        //facebook oauth login
        public Task<IActionResult> LoginWithFacebook([FromBody] string facebookOauthId)
        {
            return Task.FromResult<IActionResult>(Ok("LoginController"));
        }
        [HttpPost( "login-google")]
        //google oauth login
        public Task<IActionResult> LoginWithGoogle([FromBody] string googleOauthId)
        {
            return Task.FromResult<IActionResult>(Ok("LoginController"));
        }
    }
}
