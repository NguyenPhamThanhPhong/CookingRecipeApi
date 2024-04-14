using AutoMapper;
using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.LoginRequests;
using CookingRecipeApi.RequestsResponses.RecipeRequests;
using CookingRecipeApi.RequestsResponses.UserRequests;

namespace CookingRecipeApi.Configs
{
    public class AutomapperConfigs : Profile
    {
        public AutomapperConfigs()
        {
            CreateMap<RegisterRequest, User>().ForMember(dest => dest.authenticationInfo, opt => opt.MapFrom(src => new AuthenticationInformation
            {
                email = src.email,
                password = src.password,
                googleId = src.googleId,
                facebookId = src.facebookId
            }));
            CreateMap<UserUpdateRequest, ProfileInformation>();
                //.ForMember(dest => dest.fullName, opt => opt.MapFrom(src => src.fullName))
                //.ForMember(dest => dest.avatarUrl, opt => opt.MapFrom(src => src.avatarUrl))
                //.ForMember(dest => dest.isVegan, opt => opt.MapFrom(src => src.isVegan))
                //.ForMember(dest => dest.bio, opt => opt.MapFrom(src => src.bio))
                //.ForMember(dest => dest.categories, opt => opt.MapFrom(src => src.categories))
                //.ForMember(dest => dest.hungryHeads, opt => opt.MapFrom(src => src.hungryHeads));

            CreateMap<RecipeCreateRequest, Recipe>();
            // mapping keepUrls => attatchmentUrls
            CreateMap<RecipeUpdateRequest, Recipe>().ForMember(dest => dest.attachmentUrls, opt => opt.MapFrom(src => src.keepUrls));
        }
    }
}