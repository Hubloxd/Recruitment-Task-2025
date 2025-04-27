using Microsoft.EntityFrameworkCore;
using Recruitment_Task_2025;
using Recruitment_Task_2025.Data.Contexts;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<TodoItemCtx>(options =>
{
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<TodoItemCtx>();
    dbContext.Database.Migrate();
}

var todoRoute = app.MapGroup("/todos");

todoRoute.MapPost("/", ApiActions.CreateTodo);

todoRoute.MapGet("/", ApiActions.GetAllTodos);
todoRoute.MapGet("/{id:int}", ApiActions.GetTodoById);
todoRoute.MapGet("/{timeframe:alpha}", ApiActions.GetTodosByTimeframe);

todoRoute.MapPut("/{id:int}", ApiActions.UpdateTodoItem);
todoRoute.MapPost("/{id:int}/completion", ApiActions.SetTodoCompletionPercentage);
todoRoute.MapPost("/{id:int}/complete", ApiActions.SetTodoAsComplete);

todoRoute.MapDelete("/{id:int}", ApiActions.DeleteTodoItem);

app.Run();
