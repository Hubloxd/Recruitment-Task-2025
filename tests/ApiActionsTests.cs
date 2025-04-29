using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Recruitment_Task_2025;
using Recruitment_Task_2025.Data.Contexts;
using Recruitment_Task_2025.Data.DTOs;
using Recruitment_Task_2025.Data.Models;

namespace TodoAPITest
{
    public class ApiActionsTests
    {
        private DbContextOptions Options => new DbContextOptionsBuilder<TodoItemCtx>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        [Fact]
        public async Task GetTodoItems_ShouldReturnOk()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();

            // Act
            var response = await ApiActions.GetAllTodos(context);

            // Assert
            var okResult = Assert.IsType<Ok<TodoItemCollectionResponse>>(response);
            var todoItems = Assert.IsType<IEnumerable<TodoItem>>(okResult.Value!.Items, exactMatch: false);
        }

        [Fact]
        public async Task GetTodoItemById_ShouldReturnNotFound()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();

            // Act
            var response = await ApiActions.GetTodoById(context, 999);

            // Assert
            var notFoundResult = Assert.IsType<NotFound>(response);
        }

        [Fact]
        public async Task CreateTodoItem_ShouldReturnCreated()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            var todoItemDto = new TodoItemDto
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
                ExpireAt = DateTime.UtcNow.AddDays(7)
            };
            // Act
            var response = await ApiActions.CreateTodo(context, todoItemDto);
            // Assert
            var createdResult = Assert.IsType<Created<TodoItemResponse>>(response);
            var createdTodoItem = Assert.IsType<TodoItem>(createdResult.Value!.Item);
        }

        [Fact]
        public async Task CreateTodoItem_ShouldReturnBadRequest()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            var todoItemDto = new TodoItemDto
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
                ExpireAt = DateTime.UtcNow.AddDays(-1) // Invalid ExpireAt date
            };
            // Act
            var response = await ApiActions.CreateTodo(context, todoItemDto);
            // Assert
            var badRequestResult = Assert.IsType<BadRequest<string>>(response, exactMatch: false);
        }

        [Fact]
        public async Task GetTodoItemById_ShouldReturnOk()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();

            var newTodoItem = new TodoItem
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
                ExpireAt = DateTime.UtcNow.AddDays(7)
            };
            context.TodoItems.Add(newTodoItem);
            await context.SaveChangesAsync();
            var todoItemId = newTodoItem.Id;

            // Act
            var response = await ApiActions.GetTodoById(context, todoItemId);
            // Assert
            var okResult = Assert.IsType<Ok<TodoItemResponse>>(response);
            var todoItem = Assert.IsType<TodoItem>(okResult.Value!.Item);
        }

        [Fact]
        public async Task GetTodosByTimeframe_ShouldReturnOk()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            var random = new Random();
            string[] timeframes = ["today", "tomorrow", "thisweek"];
            var timeframe = timeframes[random.Next(0, timeframes.Length)];
            // Act
            var response = await ApiActions.GetTodosByTimeframe(context, timeframe);
            // Assert
            var okResult = Assert.IsType<Ok<TodoItemCollectionResponse>>(response);
            var todoItems = Assert.IsType<IEnumerable<TodoItem>>(okResult.Value!.Items, exactMatch: false);
        }

        [Fact]
        public async Task GetTodosByTimeframe_ShouldReturnNotFound()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            string[] timeframes = ["yesterday", "nextweek"];
            var timeframe = timeframes[0];
            // Act
            var response = await ApiActions.GetTodosByTimeframe(context, timeframe);
            // Assert
            var notFoundResult = Assert.IsType<NotFound<string>>(response);
        }

        [Fact]
        public async Task DeleteTodoItem_ShouldReturnNoContent()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();

            var todoItem = new TodoItem
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
                ExpireAt = DateTime.UtcNow.AddDays(7)
            };
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
            var todoItemId = todoItem.Id;

            // Act
            var response = await ApiActions.DeleteTodoItem(context, todoItemId);
            // Assert
            var noContentResult = Assert.IsType<NoContent>(response);
        }

        [Fact]
        public async Task DeleteTodoItem_ShouldReturnNotFound()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            // Act
            var response = await ApiActions.DeleteTodoItem(context, 999);
            // Assert
            var notFoundResult = Assert.IsType<NotFound>(response);
        }

        [Fact]
        public async Task UpdateTodoItem_ShouldReturnOk()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            var todoItem = new TodoItem
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
                ExpireAt = DateTime.UtcNow.AddDays(7)
            };
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
            var todoItemId = todoItem.Id;
            var updatedTodoItemDto = new TodoItemDto
            {
                Title = "Updated Todo",
                Description = "Updated Description",
                CompletionPercentage = 75,
            };
            // Act
            var response = await ApiActions.UpdateTodoItem(context, updatedTodoItemDto, todoItemId);
            // Assert
            var okResult = Assert.IsType<NoContent>(response);

            var updatedTodoItem = await context.TodoItems.FindAsync(todoItemId);

            Assert.NotNull(updatedTodoItem);
            Assert.Equal(updatedTodoItemDto.Title, updatedTodoItem.Title);
            Assert.Equal(updatedTodoItemDto.Description, updatedTodoItem.Description);
            Assert.Equal(updatedTodoItemDto.CompletionPercentage, updatedTodoItem.CompletionPercentage);
            Assert.Equal(updatedTodoItemDto.ExpireAt, updatedTodoItem.ExpireAt);
        }

        [Fact]
        public async Task UpdateTodoItem_ShouldReturnNotFound()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            var updatedTodoItemDto = new TodoItemDto
            {
                Title = "Updated Todo",
                Description = "Updated Description",
                CompletionPercentage = 75,
            };
            // Act
            var response = await ApiActions.UpdateTodoItem(context, updatedTodoItemDto, 999);
            // Assert
            var notFoundResult = Assert.IsType<NotFound>(response);
        }

        [Fact]
        public async Task UpdateTodoItem_ShouldReturnBadRequest()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            var todoItem = new TodoItem
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
                ExpireAt = DateTime.UtcNow.AddDays(7)
            };
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
            var todoItemId = todoItem.Id;
            var updatedTodoItemDto = new TodoItemDto
            {
                Title = "Updated Todo",
                Description = "Updated Description",
                CompletionPercentage = 75,
                ExpireAt = DateTime.UtcNow.AddDays(-1) // Invalid ExpireAt date
            };
            // Act
            var response = await ApiActions.UpdateTodoItem(context, updatedTodoItemDto, todoItemId);
            // Assert
            var badRequestResult = Assert.IsType<BadRequest<string>>(response, exactMatch: false);
        }

        [Fact]
        public async Task SetTodoCompletionPercentage_ShouldReturnNoContent()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            var todoItem = new TodoItem
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
                ExpireAt = DateTime.UtcNow.AddDays(7)
            };
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
            var todoItemId = todoItem.Id;
            JsonElement jsonBody = JsonSerializer.Deserialize<JsonElement>("{\"completion_percentage\": 75}");
            // Act
            var response = await ApiActions.SetTodoCompletionPercentage(context, todoItemId, jsonBody);
            // Assert
            var noContentResult = Assert.IsType<NoContent>(response);
        }

        [Fact]
        public async Task SetTodoCompletionPercentage_ShouldReturnNotFound()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            JsonElement jsonBody = JsonSerializer.Deserialize<JsonElement>("{\"completion_percentage\": 75}");
            // Act
            var response = await ApiActions.SetTodoCompletionPercentage(context, 999, jsonBody);
            // Assert
            var notFoundResult = Assert.IsType<NotFound>(response);
        }

        [Fact]
        public async Task SetTodoCompletionPercentage_ShouldReturnBadRequest()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            var todoItem = new TodoItem
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
                ExpireAt = DateTime.UtcNow.AddDays(7)
            };
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
            var todoItemId = todoItem.Id;
            JsonElement jsonBody = JsonSerializer.Deserialize<JsonElement>("{\"completion_percentage\": 150}");
            // Act
            var response = await ApiActions.SetTodoCompletionPercentage(context, todoItemId, jsonBody);
            // Assert
            var badRequestResult = Assert.IsType<BadRequest<string>>(response, exactMatch: false);
        }

        [Fact]
        public async Task SetTodoAsComplete_ShouldReturnNoContent()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            var todoItem = new TodoItem
            {
                Title = "Test Todo",
                Description = "Test Description",
                CompletionPercentage = 50,
                ExpireAt = DateTime.UtcNow.AddDays(7)
            };
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
            var todoItemId = todoItem.Id;
            // Act
            var response = await ApiActions.SetTodoAsComplete(context, todoItemId);
            // Assert
            var noContentResult = Assert.IsType<NoContent>(response);
        }

        [Fact]
        public async Task SetTodoAsComplete_ShouldReturnNotFound()
        {
            // Arrange
            using var context = new TodoItemCtx(Options);
            context.Database.EnsureCreated();
            // Act
            var response = await ApiActions.SetTodoAsComplete(context, 999);
            // Assert
            var notFoundResult = Assert.IsType<NotFound>(response);
        }
    }
}
