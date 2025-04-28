using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruitment_Task_2025.Data.Contexts;
using Recruitment_Task_2025.Data.DTOs;
using Recruitment_Task_2025.Data.Models;

namespace Recruitment_Task_2025
{
    public static class ApiActions
    {
        #region CREATE
        /// <summary>
        /// Creates a new TodoItem in the database.
        /// </summary>
        /// <param name="db">The database context for TodoItems.</param>
        /// <param name="todoItemDto">The DTO containing the details of the TodoItem to create.</param>
        /// <returns>A result indicating the creation status, including the created TodoItem's URI and data.</returns>
        internal static async Task<IResult> CreateTodo(TodoItemCtx db, TodoItemDto todoItemDto)
        {
            var isValid = ValidateTodoItemDto(todoItemDto, out var errors);
            if (!isValid)
                return Results.BadRequest(errors);

            CreateTodoItemByReflection(todoItemDto, out var newTodoItem);

            var createdTodoItem = await db.TodoItems.AddAsync(newTodoItem);
            await db.SaveChangesAsync();
            return Results.Created($"/todos/{createdTodoItem.Entity.Id}", createdTodoItem.Entity);
        }
        #endregion

        #region RETRIEVE
        /// <summary>
        /// Retrieves all TodoItems from the database.
        /// </summary>
        /// <param name="db">The database context for TodoItems.</param>
        /// <returns>A result containing the list of all TodoItems.</returns>
        internal static async Task<IResult> GetAllTodos(TodoItemCtx db) =>
            Results.Ok(await db.TodoItems.ToListAsync());

        /// <summary>
        /// Retrieves a specific TodoItem by its ID.
        /// </summary>
        /// <param name="db">The database context for TodoItems.</param>
        /// <param name="id">The ID of the TodoItem to retrieve.</param>
        /// <returns>A result containing the TodoItem if found, or a NotFound result if not.</returns>
        internal static async Task<IResult> GetTodoById(TodoItemCtx db, int id)
        {
            var existingTodo = await db.TodoItems.FindAsync(id);
            return existingTodo == null ? Results.NotFound() : Results.Ok(existingTodo);
        }

        /// <summary>
        /// Retrieves TodoItems based on a specified timeframe.
        /// </summary>
        /// <param name="db">The database context for TodoItems.</param>
        /// <param name="timeframe">The timeframe to filter TodoItems (e.g., "today", "tomorrow", "thisweek").</param>
        /// <returns>A result containing the filtered TodoItems or a NotFound result for invalid timeframes.</returns>
        internal static async Task<IResult> GetTodosByTimeframe(TodoItemCtx db, string timeframe)
        {
            var today = DateTime.UtcNow.Date;

            return timeframe.ToLower() switch
            {
                "today" => Results.Ok(await db.TodoItems.Where(todo => todo.ExpireAt == today).ToListAsync()),
                "tomorrow" => Results.Ok(await db.TodoItems.Where(todo => todo.ExpireAt.AddDays(1) == today).ToListAsync()),
                "thisweek" => Results.Ok(await db.TodoItems.Where(todo => todo.ExpireAt >= today && todo.ExpireAt < today.AddDays(7)).ToListAsync()),
                _ => Results.NotFound($"{timeframe} is not a valid timeframe [today, tomorrow, thisweek]"),
            };
        }
        #endregion

        #region UPDATE
        /// <summary>
        /// Updates an existing TodoItem with new data.
        /// </summary>
        /// <param name="db">The database context for TodoItems.</param>
        /// <param name="todoItemDto">The DTO containing the updated details of the TodoItem.</param>
        /// <param name="id">The ID of the TodoItem to update.</param>
        /// <returns>A result indicating the update status, or NotFound if the TodoItem does not exist.</returns>
        internal static async Task<IResult> UpdateTodoItem(TodoItemCtx db, TodoItemDto todoItemDto, int id)
        {
            var isValid = ValidateTodoItemDto(todoItemDto, out var errors);
            if (!isValid)
                return Results.BadRequest(errors);

            var existingTodoItem = await db.TodoItems.FindAsync(id);
            if (existingTodoItem == null) return Results.NotFound();

            UpdateTodoItemByReflection(todoItemDto, existingTodoItem);

            await db.SaveChangesAsync();
            return Results.NoContent();
        }

        /// <summary>
        /// Updates the completion percentage of a specific TodoItem.
        /// </summary>
        /// <param name="db">The database context for TodoItems.</param>
        /// <param name="id">The ID of the TodoItem to update.</param>
        /// <param name="body">The JSON body containing the new completion percentage.</param>
        /// <returns>A result indicating the update status, or NotFound/BadRequest for invalid input.</returns>
        internal static async Task<IResult> SetTodoCompletionPercentage(TodoItemCtx db, int id, [FromBody] JsonElement body)
        {
            var existingTodoItem = await db.TodoItems.FindAsync(id);
            if (existingTodoItem == null) return TypedResults.NotFound();

            if (!body.TryGetProperty("completion_percentage", out var jsonCompletionPercentage))
                return TypedResults.BadRequest("\"completion_percentage\" field must be included in the body");

            if (!jsonCompletionPercentage.TryGetUInt16(out var completionPercentage))
                return TypedResults.BadRequest("\"completion_percentage\" field must be a proper integer");

            existingTodoItem.CompletionPercentage = completionPercentage;

            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        /// <summary>
        /// Marks a specific TodoItem as complete by setting its completion percentage to 100.
        /// </summary>
        /// <param name="db">The database context for TodoItems.</param>
        /// <param name="id">The ID of the TodoItem to mark as complete.</param>
        /// <returns>A result indicating the update status, or NotFound if the TodoItem does not exist.</returns>
        internal static async Task<IResult> SetTodoAsComplete(TodoItemCtx db, int id)
        {
            var existingTodoItem = await db.TodoItems.FindAsync(id);
            if (existingTodoItem == null) return TypedResults.NotFound();

            existingTodoItem.CompletionPercentage = 100;

            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Deletes a specific TodoItem from the database.
        /// </summary>
        /// <param name="db">The database context for TodoItems.</param>
        /// <param name="id">The ID of the TodoItem to delete.</param>
        /// <returns>A result indicating the deletion status, or NotFound if the TodoItem does not exist.</returns>
        internal static async Task<IResult> DeleteTodoItem(TodoItemCtx db, int id)
        {
            var existingTodoItem = await db.TodoItems.FindAsync(id);
            if (existingTodoItem == null) return TypedResults.NotFound();

            db.TodoItems.Remove(existingTodoItem);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        #endregion

        #region PRIVATE METHODS
        /// <summary>
        /// Creates a new TodoItem using reflection to copy properties from the DTO.
        /// </summary>
        /// <param name="todoItemDto"></param>
        /// <param name="newTodoItem"></param>
        private static void CreateTodoItemByReflection(TodoItemDto todoItemDto, out TodoItem newTodoItem)
        {
            newTodoItem = new TodoItem() { Title = "" };
            var properties = todoItemDto.GetType().GetProperties();
            foreach (var newProperty in properties)
            {
                var newValue = newProperty.GetValue(todoItemDto);
                if (newValue == null) continue;
                
                var oldProperty = newTodoItem.GetType().GetProperty(newProperty.Name);
                if (oldProperty != null && oldProperty.CanWrite)
                {
                    oldProperty.SetValue(newTodoItem, newValue);
                }
            }
        }

        /// <summary>
        /// Updates an existing TodoItem using reflection to copy properties from the DTO.
        /// </summary>
        /// <param name="todoItemDto"></param>
        /// <param name="existingTodoItem"></param>
        private static void UpdateTodoItemByReflection(TodoItemDto todoItemDto, TodoItem existingTodoItem)
        {
            var properties = todoItemDto.GetType().GetProperties();
            foreach (var newProperty in properties)
            {
                var newValue = newProperty.GetValue(todoItemDto);
                if (newValue == null) continue;

                var oldProperty = existingTodoItem.GetType().GetProperty(newProperty.Name);
                if (oldProperty != null && oldProperty.CanWrite)
                {
                    oldProperty.SetValue(existingTodoItem, newValue);
                }
            }
        }

        /// <summary>
        /// Validates the TodoItemDto object using data annotations.
        /// </summary>
        /// <param name="todoItemDto"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool ValidateTodoItemDto(TodoItemDto todoItemDto, out string errors)
        {
            var validationContext = new ValidationContext(todoItemDto);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(todoItemDto, validationContext, validationResults, true);

            if (!isValid)
            {
                errors = string.Join(", ", validationResults.Select(vr => vr.ErrorMessage));
                return false;
            }

            errors = string.Empty;
            return true;
        }
        #endregion
    }
}
