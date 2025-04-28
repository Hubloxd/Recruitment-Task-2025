using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Recruitment_Task_2025.Data.DTOs
{
    public class TodoItemDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "The Title's length must range between 3 and 100 characters")]
        [Column("title")]
        [Display(Name = "Todo Item Title")]
        public required string Title { get; set; }

        [StringLength(256)]
        [Column("description")]
        [Display(Name = "Todo Item Description")]
        public string Description { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "The Completion Percentage must be between 0 and 100")]
        [Column("completion_percentage")]
        [Display(Name = "Todo Item Completion Percentage")]
        public ushort CompletionPercentage { get; set; } = 0;

        [NotMapped]
        internal DateTime CreatedAt { get; } = DateTime.UtcNow;

        [Column("expire_at")]
        [Display(Name = "Todo Item Expiration Date")]
        [CustomValidation(typeof(TodoItemDto), nameof(ValidateExpireAt))]
        public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddDays(7);

        public static ValidationResult? ValidateExpireAt(DateTime expireAt, ValidationContext context)
        {
            var todoItem = (TodoItemDto)context.ObjectInstance;

            if (expireAt < todoItem.CreatedAt)
            {
                return new ValidationResult("The Expiration Date mustn't preceede the Creation Date");
            }
            return ValidationResult.Success;
        }
    }
}
