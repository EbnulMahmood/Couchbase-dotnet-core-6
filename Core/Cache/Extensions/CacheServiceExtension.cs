using Cache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cache.Extensions
{
    public static class CacheServiceExtension
    {
        public static void AddCacheService(this IServiceCollection services)
        {
            services.AddSingleton<IGiftsService, GiftsService>();
            services.AddSingleton<IOrderService, OrderService>();
        }
    }
}
