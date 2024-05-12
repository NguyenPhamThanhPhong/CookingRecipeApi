using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.Requests.UserRequests;
using CookingRecipeApi.RequestsResponses.Responses;

namespace CookingRecipeApi.Services.BusinessServices.IServicies
{
    public interface IUserService
    {
        Task<UserProfileResponse> getProfilebyId(string id);
        Task<User?> getSelf(string id);
        Task<IEnumerable<UserProfileResponse>> getProfileSearch(string search, int page);
        Task<ProfileInformation> UpdateProfileBasicbyId(UserUpdateRequest request, string userId);
        Task<ProfileInformation> UpdateProfileOptionalbyId(UserUpdateProfileOptional request, string userId);
        Task<bool> DeleteUser(string id);
        Task<bool> UpdatePassword(string id, string password);
        Task<bool> UpdateFollow(string id, string followId, bool option);
        Task<IEnumerable<UserProfileResponse>> GetUserFromFollowRank();
    }
}
