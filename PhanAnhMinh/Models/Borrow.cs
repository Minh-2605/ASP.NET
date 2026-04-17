using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhanAnhMinh.Models
{
    public enum BorrowStatus
    {
        BORROWED, // Đang mượn
        RETURNED, // Đã trả
        LATE      // Quá hạn
    }

    public class Borrow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public virtual Book? Book { get; set; }

        [Required]
        [StringLength(255)]
        public string BorrowerName { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }

        // Navigation property - Chỉ giữ lại 1 User duy nhất để tránh UserId1
        public virtual User? User { get; set; }

        [Required]
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime DueDate { get; set; }     // Ngày hẹn trả (Thay thế cho ExpectedReturnDate)

        public DateTime? ReturnDate { get; set; } // Ngày trả thực tế (Thay thế cho ActualReturnDate)

        [Required]
        public BorrowStatus Status { get; set; } = BorrowStatus.BORROWED;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}