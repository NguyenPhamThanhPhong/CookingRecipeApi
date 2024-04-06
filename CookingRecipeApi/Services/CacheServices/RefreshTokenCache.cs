using Microsoft.Extensions.Caching.Memory;

namespace CookingRecipeApi.Services.CacheServices
{
    public class RefreshTokenCache : IMemoryCache
    {
        public ICacheEntry CreateEntry(object key)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(object key, out object? value)
        {
            throw new NotImplementedException();
        }
    }
}
