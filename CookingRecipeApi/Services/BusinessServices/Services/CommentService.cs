using AutoMapper;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.RequestsResponses.CommentRequests;
using MongoDB.Driver;

namespace CookingRecipeApi.Services.BusinessServices.Services
{
    public class CommentService
    {
        private readonly ICommentBatchRepository _commentBatchRepository;
        private readonly IMongoCollection<CommentBatch> _commentBatchCollection;
        private readonly IMapper _mapper;

        public CommentService(ICommentBatchRepository commentBatchRepository,
            DatabaseConfigs databaseConfigs, IMapper mapper)
        {
            _commentBatchRepository = commentBatchRepository;
            _commentBatchCollection = databaseConfigs.CommentBatchCollection;
            _mapper = mapper;
        }
    }
}
