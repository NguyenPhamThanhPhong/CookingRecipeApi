﻿namespace CookingRecipeApi.RequestsResponses.LoginRequests
{

    public class RegisterRequest
    {
        public string? googleId { get; set; }
        public string? facebookId { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }

    }
}
