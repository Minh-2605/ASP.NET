using System;
using System.ComponentModel.DataAnnotations;
namespace PhanAnhMinh.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // UUID / PK

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Role { get; set; } = "User";
    }
}
