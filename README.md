# Hướng Dẫn Tích Hợp Front-end với Hệ Thống Backend

Tài liệu này mô tả các bước cần thiết để kết nối một ứng dụng front-end với hệ thống backend quản lý trường học, bao gồm việc chạy môi trường local, xác thực người dùng qua Keycloak và tương tác với các API dữ liệu.

## 1. Chạy Hệ Thống Ở Môi Trường Local

Hệ thống bao gồm nhiều dịch vụ được quản lý bởi Docker và một API riêng chạy bằng .NET.

**Bước 1: Chạy các dịch vụ hạ tầng (Nginx, Keycloak, Kong)**

Mở terminal và chạy lệnh sau từ thư mục gốc của dự án:

```bash
docker-compose -f docker-compose.load-balancing.yml up -d
```

Lệnh này sẽ khởi tạo:
- **Nginx Load Balancer**: tại `http://localhost`
- **Keycloak Instances**: erver quản lý định danh, truy cập qua Nginx tại `http://localhost/auth/`
- **Kong API Gateway**: truy cập qua Nginx tại `http://localhost/`

**Bước 2: Chạy Backend API (DataManagementApi)**

Mở một terminal khác, di chuyển vào thư mục `DataManagementApi` và chạy lệnh:

```bash
dotnet run --launch-profile http
```

API sẽ khởi chạy và có thể truy cập tại `http://localhost:5100`. Bạn có thể xem danh sách các endpoint tại `http://localhost:5100/swagger`.

## 2. Quy Trình Xác Thực (Đăng nhập / Đăng xuất)

Hệ thống sử dụng Keycloak để quản lý xác thực theo chuẩn OpenID Connect (OIDC).

**Thông tin cấu hình Keycloak cho Front-end:**
- **Authority/Issuer URL**: `http://localhost/auth/realms/{ten-realm-cua-ban}`
- **Client ID**: (Tên client ID bạn tạo trong Keycloak cho ứng dụng front-end)
- **Redirect URI**: (URL của trang front-end mà Keycloak sẽ chuyển hướng về sau khi đăng nhập thành công, ví dụ: `http://localhost:3000/callback`)
- **Post Logout Redirect URI**: (URL của trang front-end mà Keycloak sẽ chuyển hướng về sau khi đăng xuất, ví dụ: `http://localhost:3000/`)

> **Lưu ý**: Bạn cần tạo một **Realm** và một **Client** trong Keycloak. Ví dụ, tạo realm tên `school-realm` và client tên `school-frontend`.

**URL Đăng nhập:**
Front-end không gọi trực tiếp URL đăng nhập. Thay vào đó, hãy sử dụng một thư viện OIDC (ví dụ: `oidc-client-ts` cho React/Angular/Vue) và cấu hình các thông tin trên. Thư viện sẽ tự động điều hướng người dùng đến trang đăng nhập của Keycloak. URL sẽ có dạng:
`http://localhost/auth/realms/school-realm/protocol/openid-connect/auth?client_id=...&redirect_uri=...&response_type=code&scope=openid profile email`

**URL Lấy Token:**
Sau khi người dùng đăng nhập thành công, Keycloak sẽ chuyển hướng về `Redirect URI` của bạn với một `authorization_code`. Thư viện OIDC sẽ tự động dùng code này để gọi đến URL sau và lấy về `access_token`:
`POST http://localhost/auth/realms/school-realm/protocol/openid-connect/token`

**URL Đăng xuất:**
Gọi hàm `logout()` từ thư viện OIDC. Thư viện sẽ điều hướng người dùng đến URL của Keycloak để kết thúc phiên làm việc.
`http://localhost/auth/realms/school-realm/protocol/openid-connect/logout?post_logout_redirect_uri=...`

## 3. Tương Tác Với API Dữ Liệu

Sau khi có `access_token` từ Keycloak, bạn phải đính kèm nó vào header của mỗi yêu cầu gửi tới API backend **thông qua Kong API Gateway**.

- **Base URL của API (thông qua Kong)**: `http://localhost/api`
- **Header xác thực**: `Authorization: Bearer <access_token>`

**Quan trọng**: Tất cả các đường dẫn API bây giờ sẽ bắt đầu bằng `/api`. Ví dụ, để lấy danh sách sinh viên, URL sẽ là `http://localhost/api/Students`.

### Cấu hình Kong (dành cho Backend Dev)
Để luồng này hoạt động, backend cần cấu hình Kong để tạo một *Service* trỏ đến `DataManagementApi` và một *Route* để ánh xạ các yêu cầu. Bạn có thể thực hiện việc này bằng cách gọi đến Admin API của Kong:

```bash
# 1. Tạo một Service trỏ đến DataManagementApi
curl -i -X POST http://localhost:8001/services/ \
  --data name=data-management-service \
  --data url='http://host.docker.internal:5100'

# 2. Tạo một Route trên Service đó, khớp với các đường dẫn bắt đầu bằng /api
curl -i -X POST http://localhost:8001/services/data-management-service/routes \
  --data 'paths[]=/api' \
  --data strip_path=true
```
Với cấu hình `strip_path=true`, Kong sẽ tự động loại bỏ `/api` khỏi đường dẫn trước khi chuyển tiếp yêu cầu. Ví dụ: một yêu cầu đến `http://localhost/api/Students` sẽ được chuyển đến `http://host.docker.internal:5100/Students`.


### Ví dụ gọi API từ Front-end (Đã sửa)

```javascript
// Giả sử bạn đã có accessToken sau khi đăng nhập
const accessToken = "ey..."; 

// URL đã được cập nhật để trỏ đến Kong Gateway
fetch('http://localhost/api/AcademicYears', {
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

**Danh sách các API endpoint cơ bản (qua Kong):**

- `GET /api/AcademicYears`
- `GET /api/Semesters`
- `GET /api/Departments`
- `GET /api/Partners`
- `GET /api/Students`
- `GET /api/Theses`
- `GET /api/Internships`

... và các phương thức `POST`, `PUT`, `DELETE` với ID tương ứng (ví dụ: `GET /api/Students/123`). 