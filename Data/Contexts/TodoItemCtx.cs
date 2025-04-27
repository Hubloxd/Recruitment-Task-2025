using Microsoft.EntityFrameworkCore;
using Recruitment_Task_2025.Data.Models;

namespace Recruitment_Task_2025.Data.Contexts
{
    public class TodoItemCtx(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    }
}
