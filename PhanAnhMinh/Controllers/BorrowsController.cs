using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        // 1. LẤY DANH SÁCH MƯỢN (Kèm thông tin Sách và Người dùng)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Borrow>>> GetBorrows()
        {
            return await _context.Borrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .OrderByDescending(b => b.BorrowDate) // Sắp xếp mới nhất lên đầu
                .ToListAsync();
        }

        // 2. XEM CHI TIẾT 1 ĐƠN MƯỢN
        [HttpGet("{id}")]
        public async Task<ActionResult<Borrow>> GetBorrow(int id)
        {
            var borrow = await _context.Borrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow == null) return NotFound("Không tìm thấy đơn mượn này.");

            return borrow;
        }

        // 3. NGHIỆP VỤ MƯỢN SÁCH (POST)
        [HttpPost]
        public async Task<ActionResult<Borrow>> PostBorrow(Borrow borrow)
        {
            var book = await _context.Books.FindAsync(borrow.BookId);
            if (book == null) return NotFound("Sách không tồn tại.");

            if (book.Status != "Available")
            {
                return BadRequest("Sách này hiện không sẵn sàng để mượn.");
            }

            // Tự động gán các giá trị mặc định nếu client không gửi
            borrow.BorrowDate = DateTime.UtcNow;
            borrow.CreatedAt = DateTime.UtcNow;
            borrow.Status = BorrowStatus.BORROWED;

            if (borrow.DueDate == default)
            {
                borrow.DueDate = DateTime.UtcNow.AddDays(14); // Mặc định mượn 14 ngày
            }

            // Cập nhật trạng thái sách
            book.Status = "Borrowed";

            _context.Borrows.Add(borrow);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBorrow", new { id = borrow.Id }, borrow);
        }

        // 4. NGHIỆP VỤ TRẢ SÁCH (PUT)
        [HttpPut("return/{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var borrow = await _context.Borrows
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow == null) return NotFound("Đơn mượn không tồn tại.");
            if (borrow.ReturnDate != null) return BadRequest("Sách này đã được trả rồi.");

            // Cập nhật thông tin trả sách
            borrow.ReturnDate = DateTime.UtcNow;
            borrow.Status = BorrowStatus.RETURNED;

            if (borrow.Book != null)
            {
                borrow.Book.Status = "Available";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Trả sách thành công!",
                returnDate = borrow.ReturnDate,
                status = "RETURNED"
            });
        }

        // 5. NGHIỆP VỤ KIỂM TRA SÁCH QUÁ HẠN
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<Borrow>>> GetOverdue()
        {
            var today = DateTime.UtcNow;
            var overdueList = await _context.Borrows
                .Where(b => b.ReturnDate == null && b.DueDate < today)
                .Include(b => b.Book)
                .Include(b => b.User)
                .ToListAsync();

            // Cập nhật Enum Status sang LATE cho các đơn này (nếu cần)
            foreach (var b in overdueList) { b.Status = BorrowStatus.LATE; }

            return Ok(overdueList);
        }

        // 6. XÓA ĐƠN MƯỢN
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