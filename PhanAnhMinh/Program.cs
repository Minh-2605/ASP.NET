using Microsoft.EntityFrameworkCore;
using PhanAnhMinh.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Port cho Render (Phải để TRƯỚC builder.Build)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. QUAN TRỌNG: Đưa Swagger ra ngoài dấu ngoặc "if"
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhanAnhMinh API V1");
    c.RoutePrefix = string.Empty; // Để khi vào link Render là thấy API ngay
});

// app.UseHttpsRedirection(); // Tạm thời comment nếu Render báo lỗi vòng lặp chuyển hướng
app.UseAuthorization();
app.MapControllers();

app.Run();