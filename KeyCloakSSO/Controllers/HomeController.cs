using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeyCloakSSO.Models;

namespace KeyCloakSSO.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // Landing Page - Trang chủ không cần đăng nhập
    public IActionResult LandingPage()
    {
        try
        {
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải trang Landing Page");
            return RedirectToAction("Error");
        }
    }

    // Dashboard - Trang chủ sau khi đăng nhập (yêu cầu authentication)
    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            // Lấy thông tin người dùng từ claims
            var tenNguoiDung = User.Identity?.Name ?? "Người dùng";
            var email = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value ?? "";
            
            var model = new DashboardViewModel
            {
                TenNguoiDung = tenNguoiDung,
                Email = email,
                ThoiGianDangNhap = DateTime.Now
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải Dashboard");
            return RedirectToAction("Error");
        }
    }

    // Action để đăng nhập
    public IActionResult DangNhap()
    {
        try
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Url.Action("Dashboard", "Home")
            }, OpenIdConnectDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập");
            return RedirectToAction("Error");
        }
    }

    // Action để đăng xuất - Đăng xuất hoàn toàn khỏi cả local và KeyCloak
    [Authorize]
    public async Task<IActionResult> DangXuat()
    {
        try
        {
            // Cách 1: Sử dụng SignOut với OpenIdConnect để redirect đến KeyCloak logout
            return SignOut(
                new AuthenticationProperties 
                { 
                    RedirectUri = Url.Action("LandingPage", "Home") 
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng xuất");
            return RedirectToAction("Error");
        }
    }

    // Action callback sau khi đăng xuất khỏi KeyCloak
    public IActionResult PostLogoutRedirect()
    {
        return RedirectToAction("LandingPage");
    }

    public IActionResult Index()
    {
        return RedirectToAction("LandingPage");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
} 