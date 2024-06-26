﻿using AutoMapper;
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
using System.Text;
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

        [HttpPost("register-email")]
        public async Task<IActionResult> Register([FromForm] RegisterWithEmailRequest request)
        {
            var result = await _loginService.RegisterWithEmail(request);
            if(result == null)
                return BadRequest("User account info already exists");
            return Ok(result);
        }
        [HttpPost("register-loginId")]
        public async Task<IActionResult> RegisterWithLoginId([FromForm] RegisterWithLoginIdRequest request)
        {
            var result = await _loginService.RegisterWithLoginId(request);
            if (result == null)
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
        [HttpPost("verify-login")]
        public async Task<IActionResult> VerifyLogin()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string email = await reader.ReadToEndAsync();
                var result = await _loginService.VerifyLogin(email);
                return result ? Ok("not yet exist"): BadRequest("existed") ;
            }
        }
        //first time login
        [HttpPost("login-email")]
        public async Task<IActionResult> Login([FromBody] LoginWithEmailRequest request)
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
        public async Task<IActionResult> LoginWithFacebook([FromBody] LoginWithLoginIdRequest request)
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
            //Console.WriteLine(JsonSerializer.Serialize(user));
            //Console.WriteLine(refreshToken);
            if (user == null)
                return BadRequest("Invalid request");
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return Ok(accessToken);
        }
        [HttpPost("forgot-password")]
        //  gửi mail cái mật khẩu
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (email == null)
                return BadRequest("Invalid request");
            string? password = await _loginService.GetUserPassword(email);
            if (password == null)
                return BadRequest("Invalid email");
            //send mail
            var result = await _emailService.SendEmail(email, 
                "Cooking recipe social media: your password is", 
                $"Your password is: {password??""} ");
            return result ? Ok() : BadRequest("Email not sent");
        }

    }
}
