
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
            services.AddDbContext<TodoDbContext>(options =>
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IDependencyService, DependencyService>();

            services.AddHttpClient();

            services.AddMemoryCache();

            return services;
        }
    }
}