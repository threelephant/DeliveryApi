using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Delivery.Cache
{
    public class ResponseCacheService : IResponseCacheService
    {
        private readonly IDistributedCache distributedCache;

        public ResponseCacheService(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }

        public async Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive)
        {
            if (response == null)
            {
                return;
            }

            var serializedResponse = JsonConvert.SerializeObject(response);
            await distributedCache.SetStringAsync(cacheKey, serializedResponse, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            });
        }

        public async Task<string> GetCacheResponseAsync(string cacheKey)
        {
            var cachedResponse = await distributedCache.GetStringAsync(cacheKey);
            return string.IsNullOrEmpty(cachedResponse) ? null : cachedResponse;
        }
    }
}