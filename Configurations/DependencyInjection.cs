
using Microsoft.EntityFrameworkCore;
using TodoApp.Database;
using TodoApp.Interfaces;
using TodoApp.Services;

namespace TodoApp.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Database services
            services.AddDbContext<TodoDbContext>(options =>
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            // Register Repositories

            // Register Services
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IDependencyService, DependencyService>();

            // Register HttpClient
            services.AddHttpClient();

            return services;
        }
    }
}