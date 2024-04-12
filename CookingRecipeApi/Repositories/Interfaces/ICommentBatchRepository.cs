using CookingRecipeApi.Models;

namespace CookingRecipeApi.Repositories.Interfaces
{
    public interface ICommentBatchRepository
    {
        public Task<IEnumerable<Comment?>> GetComments(string recipeId, int clientPageOffSet);
        public Task<Comment> PushComment(string recipeId, Comment comment );
        public Task UpdateComment(string recipeId, Comment comment);
        public Task DeleteComment(string recipeId, int commentOffset);
    }
}
