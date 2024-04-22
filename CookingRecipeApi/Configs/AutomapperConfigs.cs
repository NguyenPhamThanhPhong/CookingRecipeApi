using AutoMapper;
using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.Requests.LoginRequests;
using CookingRecipeApi.RequestsResponses.Requests.RecipeRequests;
using CookingRecipeApi.RequestsResponses.Requests.UserRequests;
using CookingRecipeApi.RequestsResponses.Responses;

namespace CookingRecipeApi.Configs
{
    public class AutomapperConfigs : Profile
    {
        public AutomapperConfigs()
        {
            CreateMap<UserUpdateRequest, ProfileInformation>();
            CreateMap<LoginRegisterRequest, LoginTicket>()
                .ConstructUsing(src => new LoginTicket(
                    string.Empty, src.deviceInfo??"NO DEVICE INFO FOUND", src.deviceId ?? Guid.NewGuid().ToString()));
            CreateMap<LoginRegisterRequest, User>()
                .ForMember(dest => dest.authenticationInfo, 
                opt => opt.MapFrom(src => new AuthenticationInformation
                {
                    email = src.email,
                    password = src.password,
                    loginId = src.loginId,
                    linkedAccountType = src.linkedAccountType,
                }));
            CreateMap<RecipeCreateRequest, Recipe>();
            CreateMap<User,UserProfileResponse>();
            // mapping keepUrls => attatchmentUrls
            CreateMap<RecipeUpdateRequest, Recipe>().ForMember(dest => dest.attachmentUrls, opt => opt.MapFrom(src => src.keepUrls));
        }
    }
}