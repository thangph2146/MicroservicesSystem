# Hướng Dẫn Chạy Hệ Thống Backend (Local Development cho Front-end)

Tài liệu này hướng dẫn đội ngũ front-end cách khởi chạy các dịch vụ backend cần thiết trên máy local để phục vụ cho việc phát triển.

## 1. Yêu cầu

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) đã được cài đặt và đang chạy.
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) đã được cài đặt.

## 2. Quy trình Khởi chạy

Để hệ thống hoạt động, bạn cần khởi chạy 3 thành phần trong 3 terminal riêng biệt.

**Bước 1: Chạy Keycloak**

Mở terminal tại thư mục gốc của dự án keycloak-server và chạy lệnh:

```bash
.\bin\kc.bat start-dev
```
Lệnh này sẽ khởi tạo Keycloak tại `http://localhost:8080`.

**Bước 2: Chạy Kong API Gateway**

Mở một terminal khác, di chuyển vào thư mục `kong-api-gateway` và chạy lệnh:
```bash
docker-compose up -d
```
Lệnh này sẽ khởi tạo Kong API Gateway.

**Bước 3: Chạy Backend API (DataManagementApi)**

Mở terminal thứ ba, di chuyển vào thư mục `DataManagementApi` và chạy lệnh:

```bash
dotnet run --launch-profile http
```

API sẽ khởi chạy và sẵn sàng nhận yêu cầu từ Kong.

## 3. Cấu hình Kong lần đầu

Sau khi Kong khởi chạy, bạn cần chỉ cho nó biết cách trỏ đến Backend API. Chạy lệnh sau trong một terminal bất kỳ:

```bash
# Tạo một Service trong Kong trỏ đến DataManagementApi
curl -i -X POST http://localhost:8001/services/ \
  --data name=data-management-service \
  --data url='http://host.docker.internal:5100'

# Tạo một Route trên Service đó, để các request đến /api được chuyển tiếp
curl -i -X POST http://localhost:8001/services/data-management-service/routes \
  --data 'paths[]=/api' \
  --data strip_path=true
```
Bạn chỉ cần làm điều này một lần. Kong sẽ lưu cấu hình này trong database của nó.

## 4. Các URL Quan trọng

- **Giao diện quản trị Keycloak**: `http://localhost:8080`
  - **Tài khoản**: `admin` / `admin`
- **API Gateway (dùng để gọi từ Front-end)**: `http://localhost:8000`
- **Swagger UI (để xem tài liệu API)**: `http://localhost:5100/swagger`
- **Konga UI (Giao diện quản lý Kong)**: `http://localhost:1337`

## 5. Quy Trình Xác Thực và Gọi API

**Thông tin cấu hình OIDC cho Front-end:**
- **Authority/Issuer URL**: `http://localhost:8080/realms/{ten-realm-cua-ban}`
- **Client ID**: (Client ID bạn tạo trong Keycloak)
- **API Base URL**: `http://localhost:8000/api`

**Lưu ý**: Cần tạo một **Realm** và một **Client** trong Keycloak. Đặt `Valid Redirect URIs` trỏ về địa chỉ của ứng dụng front-end (ví dụ: `http://localhost:3000/*`).

### Ví dụ gọi API từ Front-end

Sau khi lấy được `access_token` từ Keycloak, hãy đính kèm nó vào header `Authorization` khi gọi API qua Kong.

```javascript
// Giả sử bạn đã có accessToken sau khi đăng nhập
const accessToken = "ey..."; 

// URL đã được cập nhật để trỏ đến Kong Gateway local
fetch('http://localhost:8000/api/AcademicYears', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  }
})
.then(response => response.json())
.then(data => console.log(data))
.catch(error => console.error('Error:', error));
```

Tất cả các endpoint được liệt kê trong Swagger đều có thể được gọi thông qua `http://localhost:8000/api/`. 