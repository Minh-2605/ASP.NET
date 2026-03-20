using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhanAnhMinh.Models
{
    public enum BorrowStatus
    {
        BORROWED,
        RETURNED,
        LATE
    }
    public class Borrow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        // Navigation property
        [ForeignKey("BookId")]
        public virtual Book? Book { get; set; }

        [Required]
        [StringLength(255)]
        public string BorrowerName { get; set; } = string.Empty;

        [Required]
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedReturnDate { get; set; }

        public DateTime? ActualReturnDate { get; set; }

        [Required]
        public BorrowStatus Status { get; set; } = BorrowStatus.BORROWED;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
