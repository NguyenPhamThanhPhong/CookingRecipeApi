using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.Requests.UserRequests;

namespace CookingRecipeApi.Services.BusinessServices.IServicies
{
    public interface IUserService
    {
        public Task<ProfileInformation> getProfilebyId(string id);
        public Task<IEnumerable<ProfileInformation>> getProfileSearch(string search,int skip);
        public Task<ProfileInformation> UpdateProfilebyId(UserUpdateRequest request,string userId);
        public Task<bool> DeleteUser(string id);
        public Task<bool> UpdatePassword(string id, string password);
        public Task<bool> UpdateFollow(string id, string followId,bool option);
        public Task<IEnumerable<User>> GetUserFromFollowRank();
    }
}
