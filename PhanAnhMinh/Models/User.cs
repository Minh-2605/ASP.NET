using System;
using System.Collections.Generic; // Thêm dòng này
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // Thêm dòng này

namespace PhanAnhMinh.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Dùng Now cho đồng bộ giờ VN

        [Required]
        public string Role { get; set; } = "User";

        // THÊM DÒNG NÀY: Để EF Core hiểu mối quan hệ 1 User có nhiều đơn mượn
        // [JsonIgnore] rất quan trọng để tránh vòng lặp: User -> Borrow -> User -> ...
        [JsonIgnore]
        public virtual ICollection<Borrow>? Borrows { get; set; }
    }
}