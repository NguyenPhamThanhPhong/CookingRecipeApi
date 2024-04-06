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
            CreateMap<UserUpdateRequest,User>().ForMember(dest => dest.profileInfo, opt => opt.MapFrom(src => new ProfileInformation
            {
                fullName = src.fullName,
                avatarUrl = src.avatarUrl,
                isVegan = src.isVegan,
                bio = src.bio,
                categories = src.categories,
                hungryHeads = src.hungryHeads
            })).ReverseMap();
            CreateMap<RecipeCreateRequest, Recipe>();
            // mapping keepUrls => attatchmentUrls
            CreateMap<RecipeUpdateRequest, Recipe>().ForMember(dest => dest.attachmentUrls, opt => opt.MapFrom(src => src.keepUrls));
        }
    }
}