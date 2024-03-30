﻿using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.LoginRequests;

namespace CookingRecipeApi.Services.BusinessServices.IServicies
{
    public interface ILoginService
    {
        public Task<Tuple<string,User>?> LoginwithGmail(string email,string password);
        public Task<Tuple<string,User>?> LoginwithGoogle(string googleId);
        public Task<Tuple<string,User>?> LoginwithFacebook(string facebookId);
        public Task<User?> Register(RegisterRequest request);

    }
}
