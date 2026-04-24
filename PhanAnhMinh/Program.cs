using Microsoft.EntityFrameworkCore;
using PhanAnhMinh.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Port cho Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 2. Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. CẤU HÌNH CORS (Phải nằm TRƯỚC builder.Build)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Giúp xử lý vòng lặp giữa đơn mượn và sách
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. KÍCH HOẠT CORS (Phải nằm TRƯỚC MapControllers và TRƯỚC Authorization)
app.UseCors("AllowAll");

// 5. Cấu hình Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhanAnhMinh API V1");
    c.RoutePrefix = string.Empty;
});

// app.UseHttpsRedirection(); // Để comment như cũ để tránh lỗi trên Render
app.UseAuthorization();
app.MapControllers();

app.Run();