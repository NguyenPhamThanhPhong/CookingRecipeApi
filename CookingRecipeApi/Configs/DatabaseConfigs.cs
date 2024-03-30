using MongoDB.Driver;
using CookingRecipeApi.Models;

namespace CookingRecipeApi.Configs
{
    public class DatabaseConfigs
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public DatabaseConfigs()
        {
            ConnectionString = string.Empty;
            DatabaseName = string.Empty;
            UsersCollectionName = string.Empty;
            RecipeCollectionName = string.Empty;
            NotificationBatchCollectionName = string.Empty;
            NotificationBatchSize = 1;
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UsersCollectionName { get; set; }
        public string RecipeCollectionName { get; set; }
        public string NotificationBatchCollectionName { get; set; }
        public int NotificationBatchSize { get; set; }

        public IMongoCollection<User> UserCollection { get; set; }
        public IMongoCollection<Recipe> RecipeCollection { get; set; }
        public IMongoCollection<NotificationBatch> NotificationBatchCollection { get; set; }

        public void Initialize()
        {
            var client = new MongoClient(ConnectionString);
            var database = client.GetDatabase(DatabaseName);

            UserCollection = database.GetCollection<User>(UsersCollectionName);
            RecipeCollection = database.GetCollection<Recipe>(RecipeCollectionName);
            NotificationBatchCollection = database.GetCollection<NotificationBatch>(NotificationBatchCollectionName);
        }

        public void CreateIndex()
        {
            var indexKeysDefinition = Builders<User>.IndexKeys.Ascending(user => user.authenticationInfo.email);
            var indexModel = new CreateIndexModel<User>(indexKeysDefinition);
            UserCollection.Indexes.CreateOne(indexModel);
        }

    }
}
