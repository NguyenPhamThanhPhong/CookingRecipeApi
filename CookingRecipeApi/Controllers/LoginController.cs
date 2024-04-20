using AutoMapper;
using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.Requests.LoginRequests;
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
        public async Task<IActionResult> Register([FromBody] LoginRegisterRequest request)
        {
            var result = await _loginService.Register(request);
            if(result == null)
                return BadRequest("User account info already exists");

            return Ok(result);
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
        public async Task<IActionResult> Login([FromBody] LoginRegisterRequest request)
        {
            if(request.email == null || request.password == null)
                return BadRequest("Invalid request");

            var loginResult = await _loginService.LoginwithGmail(request);
            if(loginResult == null)
                return BadRequest("Invalid request");


            return Ok(loginResult);
        }
        [HttpPost("login-loginId")]
        //facebook oauth login
        public async Task<IActionResult> LoginWithFacebook([FromBody] LoginRegisterRequest request)
        {
            var result = await _loginService.LoginwithLoginId(request);
            if (result == null)
                return BadRequest("Invalid request");
            return Ok(result);
        }
        [HttpPost("auto-login")]
        //refresh token
        public async Task<IActionResult> GetAccessToken([FromBody] string refreshToken)
        {
            var user = await _loginService.GetUserfromRefreshToken(refreshToken);
            Console.WriteLine(JsonSerializer.Serialize(user));
            Console.WriteLine(refreshToken);
            if(user == null)
                return BadRequest("Invalid request");
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return Ok(accessToken);
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

    }
}
