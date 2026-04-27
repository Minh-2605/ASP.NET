using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Thêm cái này

namespace PhanAnhMinh.Models
{
    public enum BorrowStatus
    {
        BORROWED = 0, // Đang mượn
        RETURNED = 1, // Đã trả
        LATE = 2      // Quá hạn
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

        // FIX: Chỉ định rõ UserId là Foreign Key để tránh sinh ra cột UserId1
        [ForeignKey("UserId")]
        [JsonIgnore] // Quan trọng: Chặn vòng lặp JSON ở đây để Swagger không bị lỗi 500
        public virtual User? User { get; set; }

        [Required]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        [Required]
        public BorrowStatus Status { get; set; } = BorrowStatus.BORROWED;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}