using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace CookingRecipeApi.Repositories.Repos
{
    public class CommentBatchRepository : ICommentBatchRepository
    {
        private readonly IMongoCollection<CommentBatch> _commentBatchCollection;
        private readonly int _commentBatchSize;
        private readonly ClientConstants _clientConstants;
        public CommentBatchRepository(DatabaseConfigs databaseConfigs,
            ClientConstants clientConstants)
        {
            _commentBatchCollection = databaseConfigs.CommentBatchCollection;
            _commentBatchSize = databaseConfigs.CommentBatchSize;
            _clientConstants = clientConstants;
        }
        public Task DeleteComment(string recipeId, int commentOffset)
        {
            var page = commentOffset / _commentBatchSize;
            var offset = commentOffset % _commentBatchSize;
            var filter = Builders<CommentBatch>.Filter
                .Where(s=> s.recipeId == recipeId && s.page == page); 
            var update = Builders<CommentBatch>.Update.Set(n => n.comments[offset], null);
            return _commentBatchCollection.UpdateOneAsync(filter, update);
        }

        public Task<IEnumerable<Comment?>> GetComments(string recipeId, int clientPageOffSet)
        {
            throw new NotImplementedException();
        }

        public async Task<Comment> PushComment(string recipeId, Comment comment)
        {
            var filter = Builders<CommentBatch>.Filter.Where(s=>s.recipeId == recipeId);
            var sort = Builders<CommentBatch>.Sort.Descending(s=>s.page);
            FindOptions<CommentBatch> findOption = new FindOptions<CommentBatch>() { Sort=sort };
            using(var cursor = await _commentBatchCollection.FindAsync(filter, findOption))
            {
                var currentBatch = await cursor.FirstOrDefaultAsync();
                if(currentBatch == null)
                {
                    comment.offSet = 0;
                    currentBatch = new CommentBatch()
                    {
                        recipeId = recipeId,
                        page = 0,
                        count = 0,
                        comments = new List<Comment?>() { comment }
                    };
                    await _commentBatchCollection.InsertOneAsync(currentBatch);
                    return comment;
                }
                else
                {
                    if(currentBatch.comments.Count < _commentBatchSize)
                    {
                        comment.offSet = currentBatch.comments.Count;
                        var update = Builders<CommentBatch>.Update.Combine(
                            Builders<CommentBatch>.Update.Push(n => n.comments, comment),
                            Builders<CommentBatch>.Update.Inc(n => n.count, 1));
                        await _commentBatchCollection.UpdateOneAsync(filter, update);
                        return comment;
                    }
                    else
                    {
                        comment.offSet = currentBatch.page*_commentBatchSize;
                        currentBatch = new CommentBatch()
                        {
                            recipeId = recipeId,
                            page = currentBatch.page + 1,
                            count = 1,
                            comments = new List<Comment?>() { comment }
                        };
                        await _commentBatchCollection.InsertOneAsync(currentBatch);
                        return comment;
                    }
                }
            }

        }

        public Task UpdateComment(string recipeId,Comment comment)
        {
            throw new NotImplementedException();
        }
    }
}
