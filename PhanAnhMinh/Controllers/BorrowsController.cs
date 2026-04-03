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
        // GET: api/Borrows
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Borrow>>> GetBorrows()
        {
            return await _context.Borrows
                .Include(b => b.Book) // Lấy kèm thông tin sách
                .Include(b => b.User) // Lấy kèm thông tin người mượn
                .ToListAsync();
        }

        // 2. XEM CHI TIẾT 1 ĐƠN MƯỢN
        // GET: api/Borrows/5
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
        // POST: api/Borrows
        [HttpPost]
        public async Task<ActionResult<Borrow>> PostBorrow(Borrow borrow)
        {
            // Kiểm tra sách có tồn tại không
            var book = await _context.Books.FindAsync(borrow.BookId);
            if (book == null) return NotFound("Sách không tồn tại.");

            // LOGIC: Chỉ cho mượn nếu sách đang ở trạng thái 'Available'
            if (book.Status != "Available")
            {
                return BadRequest("Sách này hiện không sẵn sàng (đã có người mượn hoặc đang bảo trì).");
            }

            // Tự động gán ngày mượn là hôm nay
            borrow.BorrowDate = DateTime.Now;

            // Nếu không nhập ngày hẹn trả, mặc định là 14 ngày sau
            if (borrow.DueDate == default)
            {
                borrow.DueDate = DateTime.Now.AddDays(14);
            }

            // LOGIC: Cập nhật trạng thái sách sang 'Borrowed'
            book.Status = "Borrowed";

            _context.Borrows.Add(borrow);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBorrow", new { id = borrow.Id }, borrow);
        }

        // 4. NGHIỆP VỤ TRẢ SÁCH (PUT)
        // PUT: api/Borrows/return/5
        [HttpPut("return/{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var borrow = await _context.Borrows
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow == null) return NotFound("Đơn mượn không tồn tại.");
            if (borrow.ReturnDate != null) return BadRequest("Sách này đã được trả trước đó rồi.");

            // LOGIC: Cập nhật ngày trả thực tế
            borrow.ReturnDate = DateTime.Now;

            // LOGIC: Chuyển trạng thái sách về 'Available' để người khác có thể mượn
            if (borrow.Book != null)
            {
                borrow.Book.Status = "Available";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Trả sách thành công!",
                returnDate = borrow.ReturnDate,
                status = "Sách đã sẵn sàng cho người tiếp theo."
            });
        }

        // 5. NGHIỆP VỤ KIỂM TRA SÁCH QUÁ HẠN
        // GET: api/Borrows/overdue
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<Borrow>>> GetOverdue()
        {
            var today = DateTime.Now;
            var overdueList = await _context.Borrows
                .Where(b => b.ReturnDate == null && b.DueDate < today)
                .Include(b => b.Book)
                .Include(b => b.User)
                .ToListAsync();

            return Ok(overdueList);
        }

        // 6. XÓA ĐƠN MƯỢN (Dùng khi nhập sai dữ liệu)
        // DELETE: api/Borrows/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBorrow(int id)
        {
            var borrow = await _context.Borrows.FindAsync(id);
            if (borrow == null) return NotFound();

            // Nếu xóa đơn mượn khi chưa trả, phải trả lại trạng thái 'Available' cho sách
            var book = await _context.Books.FindAsync(borrow.BookId);
            if (book != null && borrow.ReturnDate == null)
            {
                book.Status = "Available";
            }

            _context.Borrows.Remove(borrow);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BorrowExists(int id)
        {
            return _context.Borrows.Any(e => e.Id == id);
        }
    }
}