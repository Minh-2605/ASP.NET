using System.ComponentModel.DataAnnotations;
namespace PhanAnhMinh.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation (optional - nên có để mapping với Book)
        public virtual ICollection<Book>? Books { get; set; }
    }
}
