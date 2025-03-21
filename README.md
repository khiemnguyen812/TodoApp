# TodoApp - Task Management API

A comprehensive task management API built with ASP.NET Core and Entity Framework Core that allows you to organize tasks with dependencies.

## Features

- Create, read, update, and delete tasks
- Associate dependencies between tasks with circular dependency detection
- Filter tasks by status, priority, due date, and text search
- Pagination support for efficient data retrieval
- Task dependency hierarchy visualization
- In-memory caching for improved performance
- Docker support for easy deployment

## Tech Stack

- **Framework**: ASP.NET Core 9.0
- **ORM**: Entity Framework Core 9.0
- **Database**: SQL Server 2022
- **Documentation**: Swagger/OpenAPI
- **Containerization**: Docker
- **Caching**: In-Memory Cache

## Prerequisites

- .NET 9.0 SDK or later
- SQL Server 2022 (local instance or Docker)
- Docker and Docker Compose (for containerized setup)

## Getting Started

### Local Development Setup

1. Clone the repository
   ```bash
   git clone <repository-url>
   cd TodoApp
   ```

2. Update the connection string in `appsettings.json` if needed
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=<your-server>;Initial Catalog=ToDoApp;Integrated Security=True;TrustServerCertificate=True"
   }
   ```

3. Apply migrations to create the database
   ```bash
   dotnet ef database update
   ```

4. Run the application
   ```bash
   dotnet run
   ```

5. Open a browser and navigate to [http://localhost:5182/swagger](http://localhost:5182/swagger) to access the Swagger UI.

### Docker Setup

1. Clone the repository
   ```bash
   git clone <repository-url>
   cd TodoApp
   ```

2. Build and run the application with Docker Compose
   ```bash
   docker-compose up -d
   ```

3. Open a browser and navigate to [http://localhost:5182/swagger](http://localhost:5182/swagger) to access the Swagger UI.

## Project Structure

```
TodoApp/
├── Configurations/         # Service registration and app configurations
├── Controllers/            # API endpoints
├── Database/               # DbContext and database specific code
├── DTOs/                   # Data Transfer Objects
├── Interfaces/             # Service interfaces
├── Middlewares/            # Custom middleware components
├── Migrations/             # EF Core migrations
├── Models/                 # Domain entities
├── Services/               # Business logic implementation
└── Dockerfile              # Container definition
```

## API Endpoints

### Tasks

- `GET /api/tasks` - Get a paged list of tasks with optional filtering
- `GET /api/tasks/{id}` - Get details of a specific task
- `POST /api/tasks` - Create a new task
- `PUT /api/tasks/{id}` - Update an existing task
- `DELETE /api/tasks/{id}` - Delete a task

### Dependencies

- `GET /api/dependencies/task/{taskId}` - Get dependencies for a specific task
- `GET /api/dependencies/hierarchy/{taskId}` - Get the full dependency hierarchy for a task
- `POST /api/dependencies` - Create a new dependency between tasks
- `DELETE /api/dependencies/{id}` - Delete a dependency
- `GET /api/dependencies/check-circular` - Check if adding a dependency would create a circular reference

## Database Schema

### Tasks Table
- Id (PK)
- Title
- Description
- DueDate
- Priority (Low, Medium, High, Critical)
- Status (NotStarted, InProgress, Completed, Blocked, Cancelled)
- CreatedAt
- UpdatedAt

### TaskDependencies Table
- Id (PK)
- DependentTaskId (FK)
- DependencyTaskId (FK)
- CreatedAt

## License

[MIT License](LICENSE)

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Support

For support, please open an issue in the GitHub repository or contact the development team.