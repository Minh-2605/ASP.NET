using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanAnhMinh.Data;
using PhanAnhMinh.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhanAnhMinh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            try
            {
                // Trả về danh sách thô, không Select thủ công nữa để tránh lỗi Mapping
                var books = await _context.Books.AsNoTracking().ToListAsync();
                return Ok(books);
            }
            catch (Exception ex)
            {
                // Nếu vẫn lỗi, dòng này sẽ giúp chủ nhân thấy lỗi gì trong Log của Render
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return book;
        }

        // CẬP NHẬT: Hàm Sửa sách tự động check Status
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.Id) return BadRequest();

            // LOGIC: Tự động cập nhật trạng thái dựa trên số lượng mới
            if (book.Quantity > 0)
            {
                book.Status = "Available";
            }
            else
            {
                book.Status = "Borrowed"; // Hoặc "Out of Stock" tùy chủ nhân đặt
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // CẬP NHẬT: Hàm Thêm sách tự động check Status
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            // LOGIC: Nếu thêm sách mà có số lượng > 0 thì mặc định là Available
            if (book.Quantity > 0)
            {
                book.Status = "Available";
            }
            else
            {
                book.Status = "Borrowed";
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBook", new { id = book.Id }, book);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}