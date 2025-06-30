# Hệ Thống SSO với KeyCloak

Ứng dụng ASP.NET Core MVC tích hợp với KeyCloak để thực hiện Single Sign-On (SSO).

## Tính năng

- 🏠 **Landing Page**: Trang chủ giới thiệu hệ thống
- 🔐 **KeyCloak Authentication**: Đăng nhập qua KeyCloak  
- 📊 **Dashboard**: Bảng điều khiển hiển thị thông tin người dùng
- 🚪 **SSO Flow**: Luồng đăng nhập một lần

## Luồng ứng dụng

```
Landing Page → KeyCloak Login → Dashboard
```

## Cài đặt

1. **Clone repository:**
   ```bash
   git clone <repository-url>
   cd KeyCloakSSO
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

## Cấu hình KeyCloak

### 1. Cài đặt KeyCloak

```bash
# Sử dụng Docker
docker run -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin quay.io/keycloak/keycloak:latest start-dev
```

### 2. Cấu hình Realm và Client

1. Truy cập KeyCloak Admin Console: `http://localhost:8080`
2. Đăng nhập với admin/admin
3. Tạo Realm mới (ví dụ: `my-realm`)
4. Tạo Client:
   - Client ID: `keycloak-sso-client`
   - Client Type: `OpenID Connect`
   - Valid Redirect URIs: `https://localhost:7xxx/signin-oidc`
   - Valid Post Logout Redirect URIs: `https://localhost:7xxx/`
   - Web Origins: `https://localhost:7xxx`

### 3. Cấu hình appsettings.json

Cập nhật file `appsettings.json`:

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/my-realm",
    "ClientId": "keycloak-sso-client", 
    "ClientSecret": "your-client-secret",
    "RequireHttpsMetadata": false,
    "ResponseType": "code"
  }
}
```

**Lưu ý:** Thay đổi các giá trị sau:
- `my-realm`: Tên realm bạn đã tạo  
- `keycloak-sso-client`: Client ID bạn đã tạo
- `your-client-secret`: Client Secret từ KeyCloak

## Chạy ứng dụng

```bash
dotnet run
```

Hoặc sử dụng Visual Studio/VS Code để chạy project.

## Cấu trúc thư mục

```
KeyCloakSSO/
├── Controllers/
│   └── HomeController.cs          # Controller chính
├── Models/
│   ├── DashboardViewModel.cs      # Model cho Dashboard
│   └── ErrorViewModel.cs          # Model cho Error
├── Views/
│   ├── Home/
│   │   ├── LandingPage.cshtml     # Trang chủ
│   │   └── Dashboard.cshtml       # Dashboard
│   └── Shared/
│       └── _Layout.cshtml         # Layout chung
├── Program.cs                     # Cấu hình ứng dụng
├── appsettings.json              # Cấu hình KeyCloak
└── README.md                     # Hướng dẫn
```

## Tính năng chính

### Landing Page
- Giao diện đẹp với Bootstrap 5
- Hiển thị trạng thái đăng nhập
- Nút đăng nhập/chuyển Dashboard

### Dashboard  
- Hiển thị thông tin người dùng
- Thống kê trạng thái hệ thống
- Nút đăng xuất
- Responsive design

### Authentication Flow
- Tích hợp OpenID Connect
- Lưu trữ token và claims
- Xử lý đăng xuất an toàn

## Lưu ý bảo mật

- Trong production, bật `RequireHttpsMetadata: true`
- Sử dụng HTTPS cho tất cả endpoint
- Cấu hình CORS phù hợp
- Quản lý Client Secret an toàn

## Troubleshooting

### Lỗi redirect_uri không hợp lệ
- Kiểm tra Valid Redirect URIs trong KeyCloak Client
- Đảm bảo URL khớp chính xác với ứng dụng

### Lỗi kết nối KeyCloak
- Kiểm tra KeyCloak đang chạy trên port 8080
- Kiểm tra Authority URL trong appsettings.json

### Lỗi Client Secret
- Lấy Client Secret từ KeyCloak Admin Console
- Cập nhật vào appsettings.json

## License

MIT License 