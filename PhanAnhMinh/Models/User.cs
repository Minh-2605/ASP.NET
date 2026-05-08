using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        // BỎ [Required] Ở ĐÂY để Server tự sinh giá trị
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // BỎ [Required] Ở ĐÂY vì đã có giá trị mặc định là "User"
        public string Role { get; set; } = "User";

        [JsonIgnore]
        public virtual ICollection<Borrow>? Borrows { get; set; }
    }
}