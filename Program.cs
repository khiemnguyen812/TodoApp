using System.Reflection;
using Microsoft.OpenApi.Models;
using TodoApp.Configurations;
using TodoApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Add Application Services
builder.Services.AddApplicationServices();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    
    // Set document info (optional)
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Todo API", 
        Version = "v1",
        Description = "API for managing tasks and dependencies"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add custom exception handling middleware
app.UseExceptionHandlingMiddleware();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/HelloWorld", () => "Hello World!");

app.Run();
