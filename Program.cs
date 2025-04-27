using Microsoft.EntityFrameworkCore;
using Recruitment_Task_2025;
using Recruitment_Task_2025.Data.Contexts;

var ENV = Environment.GetEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoItemCtx>(options =>
{
    options.UseNpgsql($@"Host={ENV["POSTGRES_HOST"]};Username={ENV["POSTGRES_USER"]};Password={ENV["POSTGRES_PASSWORD"]};Database={ENV["POSTGRES_DB"]}");
});

var app = builder.Build();
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
