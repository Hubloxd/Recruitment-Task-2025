using Microsoft.EntityFrameworkCore;
using Recruitment_Task_2025.Data.Contexts;
using Recruitment_Task_2025.Data.DTOs;
using Recruitment_Task_2025.Data.Models;

namespace Recruitment_Task_2025
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            if (!builder.Environment.IsEnvironment("Test"))
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                builder.Services.AddDbContext<TodoItemCtx>(options =>
                {
                    options.UseNpgsql(connectionString);
                });
            }

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Todo API", Version = "v1" });
            });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<TodoItemCtx>();
                dbContext.Database.Migrate();
            }

            var todoRoute = app.MapGroup("/todos").WithTags("Todo");

            todoRoute.MapPost("/", ApiActions.CreateTodo)
                .WithName("CreateTodo")
                .WithDescription("Endpoint tworzący nowy TodoItem")
                .Accepts<TodoItemDto>("application/json")
                .Produces<TodoItem>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            todoRoute.MapGet("/", ApiActions.GetAllTodos)
                .WithName("GetAllTodos")
                .WithDescription("Endpoint zwracający wszystkie TodoItemy")
                .Produces<TodoItem[]>(StatusCodes.Status200OK);

            todoRoute.MapGet("/{id:int}", ApiActions.GetTodoById)
                .WithName("GetTodoById")
                .WithDescription("Endpoint zwracający TodoItem o podanym id")
                .Produces<TodoItem>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            todoRoute.MapGet("/{timeframe:alpha}", ApiActions.GetTodosByTimeframe)
                .WithName("GetTodosByTimeframe")
                .WithDescription("Endpoint zwracający TodoItemy w podanym przedziale czasowym")
                .Produces<TodoItem[]>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            todoRoute.MapPatch("/{id:int}", ApiActions.UpdateTodoItem)
                .WithName("UpdateTodoItem")
                .WithDescription("Endpoint aktualizujący TodoItem o podanym id")
                .Accepts<object>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            todoRoute.MapPost("/{id:int}/completion", ApiActions.SetTodoCompletionPercentage)
                .WithName("SetTodoCompletionPercentage")
                .WithDescription("Endpoint aktualizujący procent wykonania TodoItemu o podanym id")
                .Accepts<object>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            todoRoute.MapPost("/{id:int}/complete", ApiActions.SetTodoAsComplete)
                .WithName("SetTodoAsComplete")
                .WithDescription("Endpoint aktualizujący TodoItem o podanym id jako wykonany")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            todoRoute.MapDelete("/{id:int}", ApiActions.DeleteTodoItem)
                .WithName("DeleteTodoItem")
                .WithDescription("Endpoint usuwający TodoItem o podanym id")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            app.Run();
        }
    }
}