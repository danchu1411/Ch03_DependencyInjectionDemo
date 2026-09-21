# Chương 03 — Dependency Injection in .NET

Demo đi hết chuỗi khái niệm của chương: **SOLID → IoC → DI container → Service Lifetimes → 4 kiểu injection**. Tầng dữ liệu dùng **EF Core Code First** trên **SQL Server**, và chính `DbContext` là ví dụ sống động nhất về lifetime `Scoped`.

## 1. Bản đồ slide → demo

| Slide | Nội dung | Menu |
|---|---|---|
| Single Responsibility Principle Demo | Tách hoá đơn thành 3 lớp trách nhiệm | 1 |
| Open/Closed Principle Demo | `IDiscountPolicy` — thêm loại KH không sửa code cũ | 1 |
| Liskov Substitution Principle Demo | Rectangle/Square sai và cách sửa | 1 |
| Interface Segregation Principle Demo | `IPrinter` / `IScanner` / `IFax` | 1 |
| Dependency Inversion Principle | `IOrderStore` + `OrderProcessor` | 1 |
| Understanding IoC, IoC Pattern Demo | Có và không có container | 2 |
| Service Lifetimes | Transient / Scoped / Singleton qua `InstanceId` | 3 |
| ServiceCollection Class Demo | `ServiceCollection`, `BuildServiceProvider`, `CreateScope` | 3 |
| DI - Constructor Injection Pattern | Phụ thuộc bắt buộc | 4 |
| DI - Property Injection Pattern | Phụ thuộc tuỳ chọn | 4 |
| DI - Method Injection Pattern | Phụ thuộc cho một thao tác | 4 |
| DI - Ambient Context Pattern | `AsyncLocal` + `AuditContext` | 4 |

## 2. Yêu cầu môi trường

- .NET SDK 8.0 trở lên, Visual Studio 2022 17.8+ hoặc Visual Studio 2026
- SQL Server (mặc định `localhost\SQLEXPRESS_Tam`)

## 3. Cấu trúc project

```
Ch03_DependencyInjectionDemo/
├── Ch03_DependencyInjectionDemo.sln
└── Ch03_DependencyInjectionDemo/
    ├── Entities/Category.cs, Product.cs
    ├── Data/AppDbContext.cs             # Code First + HasData seeding
    ├── Data/AppDbContextFactory.cs
    ├── Repositories/IProductRepository.cs, ProductRepository.cs
    ├── Services/IAppLogger.cs, ConsoleLogger.cs
    ├── Services/INotificationService.cs, EmailNotificationService.cs, SmsNotificationService.cs
    ├── Services/ProductService.cs       # 3 kiểu injection trong một lớp
    ├── Services/AuditContext.cs         # Ambient Context
    ├── Solid/SolidExamples.cs           # 5 nguyên tắc, mỗi nguyên tắc có bản SAI và bản ĐÚNG
    ├── Demos/LifetimeDemo.cs
    ├── Demos/SolidAndPatternDemo.cs
    ├── appsettings.json
    └── Program.cs
```

## 4. Cài đặt

**Bước 1.** Mở `Ch03_DependencyInjectionDemo.sln` → **Restore NuGet Packages**.

**Bước 2.** Sửa connection string trong `appsettings.json` nếu cần:

```json
"DiDemoDB": "Server=localhost\\SQLEXPRESS_Tam;Database=PRN222_Ch03_DiDemo;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

**Bước 3.** Tạo database (Package Manager Console):

```powershell
Add-Migration InitialCreate
Update-Database
```

Migration này tạo hai bảng `Categories`, `Products` kèm dữ liệu gốc khai báo bằng `HasData` trong `OnModelCreating` — đúng tinh thần Code First.

## 5. Hướng dẫn chạy

F5 rồi chọn menu 1–4, hoặc 5 để chạy tất cả.

Kiểm tra dữ liệu sau khi chạy menu 4:

```sql
USE PRN222_Ch03_DiDemo;
SELECT p.Id, p.Name, p.Price, c.Name AS Category
FROM Products p JOIN Categories c ON c.Id = p.CategoryId;
```

## 6. Điểm giảng dạy nên nhấn mạnh

**Menu 3 là phần trực quan nhất.** Mỗi service in ra 8 ký tự đầu của `Guid`:

```
--- SCOPE 1 ---
  Lan lay #1
    Transient : 3f2a91c4
    Scoped    : 8b17de02
    Singleton : c04e7a55
  Lan lay #2
    Transient : 7d5c1e88   <-- ĐỔI
    Scoped    : 8b17de02   <-- GIỮ NGUYÊN
    Singleton : c04e7a55   <-- GIỮ NGUYÊN
--- SCOPE 2 ---
    Scoped    : e91b0f77   <-- ĐỔI vì sang scope khác
    Singleton : c04e7a55   <-- VẪN GIỮ NGUYÊN
```

Từ đây liên hệ sang ASP.NET Core: mỗi HTTP request là một scope, nên `DbContext` sống đúng một request rồi bị huỷ.

**Bài tập tại lớp gợi ý.** Trong `Program.cs`, đổi một dòng:

```csharp
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
// thành
builder.Services.AddScoped<INotificationService, SmsNotificationService>();
```

Chạy lại menu 4: kênh thông báo đổi hoàn toàn mà `ProductService` không bị sửa một ký tự nào. Đó là câu trả lời cụ thể cho câu hỏi "DI để làm gì".

**Captive dependency.** Cho sinh viên thử đổi `AddScoped<IProductRepository, …>` thành `AddSingleton<…>`. Ứng dụng sẽ ném `InvalidOperationException` vì một Singleton không được giữ `DbContext` (Scoped). Đây là lỗi rất hay gặp trong đồ án.

## 7. Xử lý sự cố

| Triệu chứng | Cách xử lý |
|---|---|
| `Cannot consume scoped service … from singleton` | Đúng như mong đợi — đây là captive dependency. Đổi lại đăng ký về `AddScoped`. |
| `Unable to resolve service for type …` | Quên đăng ký interface trong `Program.cs`. Container chỉ tạo được cái đã được đăng ký. |
| `Add-Migration` báo không tìm được DbContext | `AppDbContext` không có constructor không tham số, nên bắt buộc phải có `AppDbContextFactory`. Kiểm tra file này còn nguyên và `appsettings.json` được copy sang thư mục output. |
| Login failed for user | Sai thông tin đăng nhập SQL Server. Kiểm tra lại chế độ xác thực của instance. |
