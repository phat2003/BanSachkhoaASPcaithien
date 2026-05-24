using BanSach.DataAccess.Data;
using BanSach.DataAccess.Repository;
using BanSach.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using BanSach.Utility;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(//Đăng ký ApplicationDbContext với DI container và cấu hình nó để sử dụng SQL Server với chuỗi kết nối được lấy từ tệp cấu hình.
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ));

builder.Services.AddIdentity<IdentityUser,IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Bắt buộc người dùng phải xác thực account
}).AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();//Đăng ký dịch vụ IUnitOfWork với triển khai UnitOfWork trong DI container với thời gian sống là Scoped.
builder.Services.AddScoped<IEmailSender, EmailSender>();//Đăng ký dịch vụ IUnitOfWork với triển khai UnitOfWork trong DI container với thời gian sống là Scoped.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();// cho phép biên dịch lại các trang Razor trong thời gian chạy mà không cần khởi động lại ứng dụng.
builder.Services.ConfigureApplicationCookie(options =>//Cấu hình cookie xác thực ứng dụng.
{
    options.LoginPath = $"/Identity/Account/Login";//Đặt đường dẫn đến trang đăng nhập khi người dùng chưa xác thực.
    options.LogoutPath = $"/Identity/Account/Logout";//Đặt đường dẫn đến trang đăng xuất.
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";//Đặt đường dẫn đến trang từ chối truy cập khi người dùng không có quyền truy cập vào tài nguyên.
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
