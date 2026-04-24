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

        // 1. LẤY DANH SÁCH MƯỢN (Kèm thông tin Sách và Người dùng)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Borrow>>> GetBorrows()
        {
            return await _context.Borrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .OrderByDescending(b => b.BorrowDate)
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

            if (borrow == null) return NotFound("Không tìm thấy đơn mượn.");
            return borrow;
        }

        // 3. NGHIỆP VỤ MƯỢN SÁCH (POST)
        [HttpPost]
        public async Task<ActionResult<Borrow>> PostBorrow(Borrow borrow)
        {
            var book = await _context.Books.FindAsync(borrow.BookId);
            if (book == null) return NotFound("Sách không tồn tại.");

            // Chỉ cho mượn nếu sách đang Available
            if (book.Status != "Available") return BadRequest("Sách này hiện không sẵn sàng.");

            // Gán các giá trị mặc định cho đơn mới
            borrow.BorrowDate = DateTime.UtcNow;
            borrow.CreatedAt = DateTime.UtcNow;
            borrow.Status = BorrowStatus.BORROWED;
            if (borrow.DueDate == default) borrow.DueDate = DateTime.UtcNow.AddDays(14);

            // Chuyển trạng thái sách sang mượn
            book.Status = "Borrowed";

            _context.Borrows.Add(borrow);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBorrow", new { id = borrow.Id }, borrow);
        }

        // 4. CẬP NHẬT THÔNG TIN ĐƠN MƯỢN (PUT)
        // Giải quyết lỗi 405 Method Not Allowed khi gọi trực tiếp vào ID
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBorrow(int id, Borrow borrow)
        {
            if (id != borrow.Id) return BadRequest("ID không khớp.");

            _context.Entry(borrow).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Borrows.Any(e => e.Id == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // 5. NGHIỆP VỤ TRẢ SÁCH NHANH (PUT)
        [HttpPut("return/{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var borrow = await _context.Borrows
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow == null) return NotFound("Đơn mượn không tồn tại.");
            if (borrow.ReturnDate != null) return BadRequest("Sách này đã được trả rồi.");

            // Cập nhật ngày trả và trạng thái
            borrow.ReturnDate = DateTime.UtcNow;
            borrow.Status = BorrowStatus.RETURNED;

            // Mở khóa sách để người khác mượn
            if (borrow.Book != null)
            {
                borrow.Book.Status = "Available";
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Trả sách thành công!", status = "RETURNED" });
        }

        // 6. XÓA ĐƠN MƯỢN (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBorrow(int id)
        {
            var borrow = await _context.Borrows.FindAsync(id);
            if (borrow == null) return NotFound();

            // Nếu xóa đơn mượn khi chưa trả sách, trả lại trạng thái sách về Available
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