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
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LẤY TẤT CẢ ĐÁNH GIÁ (Kèm thông tin User và Book)
        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            // Sử dụng Include để lấy thông tin User đã đánh giá theo yêu cầu
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // 2. LẤY CHI TIẾT 1 ĐÁNH GIÁ
        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound("Không tìm thấy đánh giá.");

            return review;
        }

        // 3. LẤY DANH SÁCH ĐÁNH GIÁ THEO CUỐN SÁCH
        // GET: api/Reviews/book/5
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsByBook(int bookId)
        {
            return await _context.Reviews
                .Where(r => r.BookId == bookId)
                .Include(r => r.User)
                .ToListAsync();
        }

        // 4. NGHIỆP VỤ THÊM ĐÁNH GIÁ (POST)
        // POST: api/Reviews
        [HttpPost]
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
            // Kiểm tra sự tồn tại của Sách và User trước khi lưu
            var bookExists = await _context.Books.AnyAsync(b => b.Id == review.BookId);
            var userExists = await _context.Users.AnyAsync(u => u.Id == review.UserId);

            if (!bookExists || !userExists)
            {
                return BadRequest("Thông tin Sách hoặc Người dùng không tồn tại trong hệ thống.");
            }

            // Nghiệp vụ: Điểm đánh giá phải từ 1-5 sao
            if (review.Rating < 1 || review.Rating > 5)
            {
                return BadRequest("Rating phải nằm trong khoảng từ 1 đến 5.");
            }

            // Tự động gán ngày tạo đánh giá
            review.CreatedAt = DateTime.Now;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReview", new { id = review.Id }, review);
        }

        // 5. CẬP NHẬT ĐÁNH GIÁ (Chỉ sửa điểm và nội dung)
        // PUT: api/Reviews/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, Review review)
        {
            if (id != review.Id) return BadRequest();

            var existingReview = await _context.Reviews.FindAsync(id);
            if (existingReview == null) return NotFound();

            // Cập nhật các trường được phép sửa
            existingReview.Rating = review.Rating;
            existingReview.Comment = review.Comment;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // 6. XÓA ĐÁNH GIÁ
        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}