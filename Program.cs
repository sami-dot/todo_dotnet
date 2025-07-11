using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Connection string to your SQL Server with your password here:
var connectionString = "Server=127.0.0.1,1433;Database=TodoDb;User Id=SA;Password=@Admin1234!;TrustServerCertificate=True;";

builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
