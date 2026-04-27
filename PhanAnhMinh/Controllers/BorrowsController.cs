using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanAnhMinh.Data;
using PhanAnhMinh.Models;

namespace PhanAnhMinh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BorrowsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH MƯỢN (Đã sửa để tránh lỗi 500 Circular Reference)
        [HttpGet]
        public async Task<IActionResult> GetBorrows()
        {
            var result = await _context.Borrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .OrderByDescending(b => b.BorrowDate)
                .Select(b => new {
                    id = b.Id,
                    bookId = b.BookId,
                    // Lấy Title từ object Book, nếu null thì hiện N/A
                    book = b.Book != null ? new { title = b.Book.Title } : null,
                    userId = b.UserId,
                    // Ưu tiên lấy Username từ bảng User nếu có liên kết
                    borrowerName = b.User != null ? b.User.Username : b.BorrowerName,
                    borrowDate = b.BorrowDate,
                    dueDate = b.DueDate,
                    returnDate = b.ReturnDate,
                    status = (int)b.Status,
                    
                })
                .ToListAsync();

            return Ok(result);
        }

        // 2. XEM CHI TIẾT 1 ĐƠN MƯỢN
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBorrow(int id)
        {
            var b = await _context.Borrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (b == null) return NotFound("Không tìm thấy đơn mượn.");

            // Trả về object sạch để tránh lỗi vòng lặp
            var result = new
            {
                id = b.Id,
                bookId = b.BookId,
                book = b.Book != null ? new { title = b.Book.Title } : null,
                userId = b.UserId,
                borrowerName = b.User != null ? b.User.Username : b.BorrowerName,
                borrowDate = b.BorrowDate,
                dueDate = b.DueDate,
                returnDate = b.ReturnDate,
                status = (int)b.Status,
                
            };

            return Ok(result);
        }

        // 3. NGHIỆP VỤ MƯỢN SÁCH (POST)
        [HttpPost]
        public async Task<ActionResult<Borrow>> PostBorrow(Borrow borrow)
        {
            var book = await _context.Books.FindAsync(borrow.BookId);
            if (book == null) return NotFound("Sách không tồn tại.");

            if (book.Status != "Available") return BadRequest("Sách này hiện không sẵn sàng.");

            // Gán giá trị mặc định
            borrow.BorrowDate = DateTime.Now;
            borrow.CreatedAt = DateTime.Now;
            borrow.Status = BorrowStatus.BORROWED;
            if (borrow.DueDate == default) borrow.DueDate = DateTime.Now.AddDays(14);

            book.Status = "Borrowed";

            _context.Borrows.Add(borrow);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBorrow", new { id = borrow.Id }, borrow);
        }

        // 4. CẬP NHẬT TRẠNG THÁI TRẢ SÁCH NHANH (PUT)
        [HttpPut("return/{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var borrow = await _context.Borrows
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow == null) return NotFound("Đơn mượn không tồn tại.");
            if (borrow.ReturnDate != null) return BadRequest("Sách này đã được trả rồi.");

            borrow.ReturnDate = DateTime.Now;
            borrow.Status = BorrowStatus.RETURNED;

            if (borrow.Book != null)
            {
                borrow.Book.Status = "Available";
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Trả sách thành công!" });
        }

        // 5. XÓA ĐƠN MƯỢN
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBorrow(int id)
        {
            var borrow = await _context.Borrows.FindAsync(id);
            if (borrow == null) return NotFound();

            var book = await _context.Books.FindAsync(borrow.BookId);
            if (book != null && borrow.ReturnDate == null)
            {
                book.Status = "Available";
            }

            _context.Borrows.Remove(borrow);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}