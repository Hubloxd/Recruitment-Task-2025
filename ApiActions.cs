using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruitment_Task_2025.Data.Contexts;
using Recruitment_Task_2025.Data.DTOs;

namespace Recruitment_Task_2025
{
    public static class ApiActions
    {
        #region CREATE
        internal static async Task<IResult> CreateTodo(TodoItemCtx db, TodoItemDto todoItemDto)
        {
            await db.AddAsync(todoItemDto);
            await db.SaveChangesAsync();
            return Results.Created();
        }
        #endregion

        #region RETRIEVE
        internal static async Task<IResult> GetAllTodos(TodoItemCtx db) => 
            Results.Ok(await db.TodoItems.ToListAsync());
        
        internal static async Task<IResult> GetTodoById(TodoItemCtx db, int id)
        {
            var existingTodo = await db.TodoItems.FindAsync(id);
            return existingTodo == null? Results.NotFound() : Results.Ok(existingTodo);
        }

        internal static async Task<IResult> GetTodosByTimeframe(TodoItemCtx db, string timeframe)
        {
            var today = DateTime.Today.Date;

            return timeframe.ToLower() switch
            {
                "today" => Results.Ok(await db.TodoItems.Where(todo => todo.ExpireAt == today).ToListAsync()),
                "tomorrow" => Results.Ok(await db.TodoItems.Where(todo => todo.ExpireAt.AddDays(1) == today).ToListAsync()),
                "thisweek" => throw new NotImplementedException(),
                _ => Results.NotFound(),
            };
        }
        #endregion

        #region UPDATE
        internal static async Task<IResult> UpdateTodoItem(TodoItemCtx db, TodoItemDto todoItemDto, int id)
        {
            var existingTodoItem = await db.TodoItems.FindAsync(id);
            if (existingTodoItem == null) return Results.NotFound();

            var newProperties = todoItemDto.GetType().GetProperties();
            foreach (var newProperty in newProperties)
            {
                var oldProperty = existingTodoItem.GetType().GetProperty(newProperty.Name);

                if (oldProperty != null && oldProperty.CanWrite)
                {
                    var newValue = newProperty.GetValue(todoItemDto);
                    oldProperty.SetValue(todoItemDto, newValue);
                }
            }
            
            await db.SaveChangesAsync();
            return Results.NoContent();
        }

        internal static async Task<IResult> SetTodoCompletionPercentage(TodoItemCtx db, int id, [FromBody] JsonElement body)
        {
            var existingTodoItem = await db.TodoItems.FindAsync(id);
            if (existingTodoItem == null) return TypedResults.NotFound();

            if (!body.TryGetProperty("completion_percentage", out var jsonCompletionPercentage))
                return TypedResults.BadRequest("\"completion_percentage\" field must be included in the body");

            if(!jsonCompletionPercentage.TryGetUInt16(out var completionPercentage))
                return TypedResults.BadRequest("\"completion_percentage\" field must be a proper integer");

            existingTodoItem.CompletionPercentage = completionPercentage;

            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

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
        internal static async Task<IResult> DeleteTodoItem(TodoItemCtx db, int id)
        {
            var existingTodoItem = await db.TodoItems.FindAsync(id);
            if (existingTodoItem == null) return TypedResults.NotFound();

            db.TodoItems.Remove(existingTodoItem);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        #endregion
    }
}
