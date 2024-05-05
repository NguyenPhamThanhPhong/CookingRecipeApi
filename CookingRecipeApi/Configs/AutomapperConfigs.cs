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
            CreateMap<LoginRegisterRequestBase, LoginTicket>()
                .ConstructUsing(src => new LoginTicket(
                    string.Empty, 
                    src.deviceInfo== string.Empty?"UNKNOWN":"NO DEVICE INFO FOUND", 
                    src.deviceId ?? Guid.NewGuid().ToString()
                    ));
            CreateMap<RegisterWithEmailRequest, User>()
                .ForMember(dest => dest.authenticationInfo,
                opt => opt.MapFrom(src => new AuthenticationInformation
                {
                    email = src.email,
                    password = src.password,
                }))
                .ForMember(dest => dest.profileInfo,
                opt => opt.MapFrom(src => new ProfileInformation
                {
                    fullName = src.fullName ?? "",
                    bio = src.bio ?? "",
                    avatarUrl = src.avatarUrl ?? "",
                    categories = src.categories ?? new List<string>(),
                    hungryHeads = src.hungryHeads,
                    isVegan = src.isVegan,
                }));

            CreateMap<RegisterWithLoginIdRequest, User>()
                .ForMember(dest => dest.authenticationInfo,
                opt => opt.MapFrom(src => new AuthenticationInformation
                {
                    loginId = src.loginId,
                    linkedAccountType = src.linkedAccountType
                }))
                .ForMember(dest => dest.profileInfo,
                opt => opt.MapFrom(src => new ProfileInformation
                {
                    fullName = src.fullName ?? "",
                    bio = src.bio ?? "",
                    avatarUrl = src.avatarUrl ?? "",
                    categories = src.categories ?? new List<string>(),
                    hungryHeads = src.hungryHeads,
                    isVegan = src.isVegan,
                }));

            CreateMap<RecipeCreateRequest, Recipe>();
            CreateMap<User,UserProfileResponse>();
            // mapping keepUrls => attatchmentUrls
            CreateMap<RecipeUpdateRequest, Recipe>().ForMember(
                dest => dest.attachmentUrls, opt => opt.MapFrom(src => src.keepUrls));
        }
    }
}