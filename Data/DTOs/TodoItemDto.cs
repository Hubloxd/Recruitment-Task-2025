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

        [Column("completion_percentage")]
        [Display(Name = "Todo Item Completion Percentage")]
        public ushort CompletionPercentage { get; set; } = 0;

        [Column("expire_at")]
        [Display(Name = "Todo Item Expiration Date")]
        public DateTime ExpireAt { get; set; } = DateTime.Now.AddDays(7);
    }
}
