using Microsoft.EntityFrameworkCore;
using PhanAnhMinh.Models;

namespace PhanAnhMinh.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor này bắt buộc phải có để nhận Connection String từ
        Program.cs
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){ }
        public DbSet<Book> Books { get; set; }
    }
}
