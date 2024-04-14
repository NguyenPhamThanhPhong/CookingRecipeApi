using AutoMapper;
using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.LoginRequests;
using CookingRecipeApi.Services.AuthenticationServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.BusinessServices.Services;
using CookingRecipeApi.Services.SMTPServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CookingRecipeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly TokenGenerator _tokenGenerator;
        private readonly IMapper _mapper;
        private readonly EmailService _emailService;

        public LoginController(ILoginService loginService, IMapper mapper, TokenGenerator tokenGenerator, EmailService emailService)
        {
            _loginService = loginService;
            _mapper = mapper;
            _tokenGenerator = tokenGenerator;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _loginService.Register(request);
            if(result == null)
                return BadRequest("User account info already exists");

            return Ok(new { refreshToken = result.Item1, accessToken = result.Item2, user = result.Item3 });
        }
        [Authorize]
        [HttpGet("test-login")]
        public IActionResult TestLogin()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userFullName = User.FindFirst(ClaimTypes.Name)?.Value;
            return Ok($"done authenticated userid={userId} & fullname={userFullName}");

        }
        //first time login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if(request.email == null || request.password == null)
                return BadRequest("Invalid request");

            var loginResult = await _loginService.LoginwithGmail(request.email,request.password);
            if(loginResult == null)
                return BadRequest("Invalid request");


            return Ok(new { refreshToken = loginResult.Item1,accessToken=loginResult.Item2,user=loginResult.Item3 });
        }
        [HttpPost("auto-login")]
        public async Task<IActionResult> GetAccessToken([FromBody] string refreshToken)
        {
            bool isRefreshTokenValid = _tokenGenerator.ValidateToken(refreshToken);
            if(!isRefreshTokenValid)
                return Unauthorized("Invalid request");

            var user = await _loginService.GetUserfromRefreshToken(refreshToken);
            if(user == null)
                return BadRequest("Invalid request");

            var accessToken = _tokenGenerator.GenerateAccessToken(user);

            return Ok(new {accessToken=accessToken, user=user});
        }
        [HttpPost("forgot-password")]
        // lấy userId ra từ token, gửi mail cái mật khẩu
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (userId == null || userEmail == null)
                return BadRequest("Invalid request");

            var password = await _loginService.GetUserPassword(userId);
            //send mail
            var result = await _emailService.SendEmail(userEmail, 
                "Cooking recipe social media: your password is", 
                $"Your password is: ${password??""} ");

            return result ? Ok("Email sent") : BadRequest("Email not sent");
        }
        [HttpPost("login-facebook")]
        //facebook oauth login
        public async Task<IActionResult> LoginWithFacebook([FromBody] string facebookOauthId)
        {
            var result = await _loginService.LoginwithFacebook(facebookOauthId);
            if (result == null)
                return BadRequest("Invalid request");

            return Ok(new { refreshToken = result.Item1, accessToken = result.Item2, user = result.Item3 });
        }
        [HttpPost( "login-google")]
        //google oauth login
        public async Task<IActionResult> LoginWithGoogle([FromBody] string googleOauthId)
        {
            var result = await _loginService.LoginwithGoogle(googleOauthId);
            if (result == null)
                return BadRequest("Invalid request");
            return Ok(new { refreshToken = result.Item1, accessToken = result.Item2, user = result.Item3 });
        }
    }
}
