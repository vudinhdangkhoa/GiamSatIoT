#  Hệ thống thu nhận và cảnh báo dữ liệu từ cảm biến 

> Hệ thống dùng backend là .NET 8 để để xử lý yêu cầu phía server và frontend là ReactJS để làm phần giao diện. Người dùng tải dữ liệu cảm biến từ file csv và có thể thiết lập các ngưỡng báo cho các dữ liệu 

##  Yêu cầu môi trường

Trước khi chạy được hệ thống, cần đảm bảo các công cụ sau đã được cài đặt trên máy:

- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- [npm](https://www.npmjs.com/)
- [SQL Server 2020](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) – dùng làm cơ sở dữ liệu

---

##  Hướng dẫn chạy hệ thống

###  Backend (`/be` folder)

1. **Clone** source code từ GitHub:
   ```bash
   git clone https://github.com/vudinhdangkhoa/QuanLySaoLuu
2. Mở thư mục `be` bằng Visual Studio Code.
   

3. **Khởi tạo cơ sở dữ liệu**:  
   Mở SQL Server 2020 và **nhập dữ liệu từ file backup hoặc script SQL đi kèm dự án**.

4. **Cấu hình chuỗi kết nối**:  
   Mở file `MyDbContext.cs` trong thư mục `Models` và file `Program.cs`.  
   Tại các dòng có chứa `"connectionString"`, hãy thay `Server=...` bằng **tên Server thực tế trên máy**.

5. Chạy ứng dụng:
   ```bash
   dotnet watch run 
   ```
   Giao diện Swagger UI sẽ được mở trong trình duyệt để bạn kiểm thử các API.

---

###  Frontend (`/fe` folder)

1. Mở một cửa sổ VS Code mới và truy cập thư mục `fe`:
   

2 Mở terminal (chế độ Command Prompt - CMD) và chạy lệnh:
  ```bash
  npm i
```
để cài đặt các dependencies.

3 Tiếp theo, chạy:
  ```bash
  npm start
  ```
để khởi động frontend. Ứng dụng web sẽ được mở trong trình duyệt mặc định.
