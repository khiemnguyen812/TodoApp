
namespace TodoApp.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register Repositories

            // Register Services
          
            // Register Memory Cache for performance optimization
            services.AddMemoryCache();

            // Register HttpClient
            services.AddHttpClient();

            // Register Background Service for task notifications

            return services;
        }
    }
}