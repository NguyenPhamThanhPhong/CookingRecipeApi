using AutoMapper;
using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.LoginRequests;
using CookingRecipeApi.Services.AuthenticationServices;
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
        private readonly TokenGenerator _tokenGenerator;
        private readonly IMapper _mapper;

        public LoginController(ILoginService loginService, IMapper mapper, TokenGenerator tokenGenerator)
        {
            _loginService = loginService;
            _mapper = mapper;
            _tokenGenerator = tokenGenerator;
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
        //first time login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if(request.email == null || request.password == null)
            {
                return BadRequest("Invalid request");
            }
            var loginResult = await _loginService.LoginwithGmail(request.email,request.password);
            if(loginResult == null)
            {
                return BadRequest("Invalid request");
            }
            //how to use anonymous object in c#

            return Ok(new {accessToken=loginResult.Item1,refreshToken=loginResult.Item2,user=loginResult.Item3 });
        }
        [HttpPost("forgot-password")]
        // lấy userId ra từ token, gửi mail cái mật khẩu
        public Task<IActionResult> ForgotPassword()
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
