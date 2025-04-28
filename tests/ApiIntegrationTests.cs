using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recruitment_Task_2025;
using Recruitment_Task_2025.Data.Contexts;
using Recruitment_Task_2025.Data.DTOs;
using Recruitment_Task_2025.Data.Models;

namespace TodoAPITest
{
    public class ApiIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly TodoItemCtx _dbContext;
        public ApiIntegrationTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");

                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<TodoItemCtx>));
                    
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Use SQLite for temporary testing
                    services.AddDbContext<TodoItemCtx>(options =>
                    {
                        options.UseSqlite("DataSource=test_db.sqlite");
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });
                });
            });

            _client = _factory.CreateClient();
            _dbContext = _factory.Services.GetRequiredService<TodoItemCtx>();
        }

        [Theory]
        [InlineData("/todos")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("/todos/")]
        public async Task Post_CreateTodoItem_ReturnsOk(string url)
        {
            var todoItem = new TodoItemDto
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
            };
            var response = await _client.PostAsJsonAsync(url, todoItem);
            response.EnsureSuccessStatusCode();
            var createdTodoItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            Assert.NotNull(createdTodoItem);
            Assert.Equal(todoItem.Title, createdTodoItem.Title);
        }

        [Theory]
        [InlineData("/todos/")]
        public async Task Get_RetrieveTodoItems_ReturnsOk(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var todoItems = await response.Content.ReadFromJsonAsync<TodoItem[]>();
            Assert.NotNull(todoItems);
        }

        [Theory]
        [InlineData("/todos/")]
        public async Task Get_RetrieveTodoItemById_ReturnsOk(string url)
        {
            var newTodoItem = new TodoItemDto
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
            };
            if (!AddTodoItemToDb(newTodoItem, out var entity))
            {
                throw new Exception("Failed to add TodoItem to the database.");
            }

            var response = await _client.GetAsync($"{url}{entity.Id}");
            response.EnsureSuccessStatusCode();
            var todoItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            Assert.NotNull(todoItem);
        }

        [Theory]
        [InlineData("/todos/")]
        public async Task Patch_UpdateTodoItem_ReturnsNoContent(string url)
        {
            var todoItem = new TodoItemDto
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
            };
            if (!AddTodoItemToDb(todoItem, out var entity))
            {
                throw new Exception("Failed to add TodoItem to the database.");
            }
            var updatedTodoItem = new TodoItemDto
            {
                Title = "Updated Test Todo",
                Description = "Updated Test Description",
                CompletionPercentage = 75,
                ExpireAt = DateTime.UtcNow.AddDays(10)
            };

            var response = await _client.PatchAsJsonAsync($"{url}{entity.Id}", updatedTodoItem);
            response.EnsureSuccessStatusCode();
            var getResponse = await _client.GetAsync($"{url}{entity.Id}");
            getResponse.EnsureSuccessStatusCode();
            var retrievedTodoItem = await getResponse.Content.ReadFromJsonAsync<TodoItem>();

            Assert.NotNull(retrievedTodoItem);
            Assert.Equal(updatedTodoItem.Title, retrievedTodoItem.Title);
            Assert.Equal(updatedTodoItem.Description, retrievedTodoItem.Description);
            Assert.Equal(updatedTodoItem.CompletionPercentage, retrievedTodoItem.CompletionPercentage);
            Assert.Equal(updatedTodoItem.ExpireAt, retrievedTodoItem.ExpireAt);
        }

        [Theory]
        [InlineData("/todos/")]
        public async Task Delete_RetrieveTodoItem_ReturnsOk(string url)
        {
            var todoItem = new TodoItemDto
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
            };
            if (!AddTodoItemToDb(todoItem, out var entity))
            {
                throw new Exception("Failed to add TodoItem to the database.");
            }

            var response = await _client.DeleteAsync($"{url}{entity.Id}");
            response.EnsureSuccessStatusCode();
            var getResponse = await _client.GetAsync($"{url}{entity.Id}");
            Assert.Equal(StatusCodes.Status404NotFound, (int)getResponse.StatusCode);
        }

        private bool AddTodoItemToDb(TodoItemDto todoItemDto, out TodoItem entity)
        {
            var todoItem = new TodoItem
            {
                Title = todoItemDto.Title,
                Description = todoItemDto.Description,
                CompletionPercentage = todoItemDto.CompletionPercentage,
                ExpireAt = todoItemDto.ExpireAt
            };
            entity = _dbContext.TodoItems.Add(todoItem).Entity;
            return _dbContext.SaveChanges() > 0;
        }
    }
}
