﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Task_2025.Data.Models
{
    [Table("todo")]
    public class TodoItem
    {
        [Key]
        [Column("id")]
        [Display(Name = "Todo Item Primary Key")]
        public int Id { get; set; }

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

        [Column("created_at")]
        [Display(Name = "Todo Item Creation Date")]
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        [Column("expire_at")]
        [Display(Name = "Todo Item Expiration Date")]
        public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddDays(7);
    }
}
