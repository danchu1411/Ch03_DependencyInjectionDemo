using System.Globalization;
using System.Text;
using Ch03_DependencyInjectionDemo.Data;
using Ch03_DependencyInjectionDemo.Entities;
using Ch03_DependencyInjectionDemo.Repositories;
using Ch03_DependencyInjectionDemo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ch03_DependencyInjectionDemo;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "PRN222 - Product Management";

        var builder = Host.CreateApplicationBuilder(args);

        var connectionString =
            builder.Configuration.GetConnectionString("DiDemoDB")
            ?? throw new InvalidOperationException(
                "Thiếu connection string 'DiDemoDB'.");

        // =========================
        // ĐĂNG KÝ DEPENDENCY INJECTION
        // =========================
        builder.Services.AddDbContext<AppDbContext>(
            options => options.UseSqlServer(connectionString));

        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();

        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IProductService, ProductService>();

        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderService, OrderService>();

        var host = builder.Build();

        // =========================
        // KIỂM TRA DATABASE
        // =========================
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            Console.WriteLine(
                $"[DB] Kết nối thành công. Hiện có " +
                $"{await db.Categories.CountAsync()} Categories, " +
                $"{await db.Products.CountAsync()} Products.");
        }

        // =========================
        // MENU CHÍNH
        // =========================
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("          QUẢN LÝ BÁN HÀNG");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Quản lý Categories");
            Console.WriteLine("2. Quản lý Product");
            Console.WriteLine("3. Thống kê");
            Console.WriteLine("0. Thoát");
            Console.Write("Chọn chức năng: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await CategoryMenuAsync(host.Services);
                    break;

                case "2":
                    await ProductMenuAsync(host.Services);
                    break;

                case "3":
                    await StatisticsMenuAsync(host.Services);
                    break;

                case "0":
                    Console.WriteLine("Đã thoát chương trình.");
                    return;

                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }
    }

    // =========================================================
    // CATEGORY MENU
    // =========================================================
    private static async Task CategoryMenuAsync(
        IServiceProvider services)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== QUẢN LÝ CATEGORIES =====");
            Console.WriteLine("1. Danh sách Categories");
            Console.WriteLine("2. Thêm Category");
            Console.WriteLine("3. Sửa Category");
            Console.WriteLine("4. Xóa Category");
            Console.WriteLine("0. Quay lại");
            Console.Write("Chọn chức năng: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await ShowCategoriesAsync(services);
                    break;

                case "2":
                    await AddCategoryAsync(services);
                    break;

                case "3":
                    await UpdateCategoryAsync(services);
                    break;

                case "4":
                    await DeleteCategoryAsync(services);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }
    }

    // =========================================================
    // SHOW CATEGORY
    // =========================================================
    private static async Task ShowCategoriesAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<ICategoryService>();

        var categories = await service.GetAllAsync();

        Console.WriteLine();
        Console.WriteLine("===== DANH SÁCH CATEGORIES =====");
        Console.WriteLine("--------------------------------");
        Console.WriteLine($"{"ID",-5} {"Name",-30}");
        Console.WriteLine("--------------------------------");

        if (categories.Count == 0)
        {
            Console.WriteLine("Chưa có Category.");
            return;
        }

        foreach (var category in categories)
        {
            Console.WriteLine(
                $"{category.Id,-5} {category.Name,-30}");
        }
    }

    // =========================================================
    // ADD CATEGORY
    // =========================================================
    private static async Task AddCategoryAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<ICategoryService>();

        Console.WriteLine();
        Console.WriteLine("===== THÊM CATEGORY =====");

        string name = ReadRequiredString(
            "Tên Category: ",
            100);

        var category = new Category
        {
            Name = name
        };

        await service.AddAsync(category);

        Console.WriteLine(
            $"Thêm Category thành công. ID = {category.Id}");
    }

    // =========================================================
    // UPDATE CATEGORY
    // =========================================================
    private static async Task UpdateCategoryAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<ICategoryService>();

        await ShowCategoriesWithServiceAsync(service);

        Console.WriteLine();
        Console.WriteLine("===== SỬA CATEGORY =====");

        int id = ReadPositiveInt("Nhập Category ID: ");

        var category = await service.GetByIdAsync(id);

        if (category == null)
        {
            Console.WriteLine("Không tìm thấy Category.");
            return;
        }

        Console.WriteLine($"Tên hiện tại: {category.Name}");

        string newName = ReadRequiredString(
            "Tên mới: ",
            100);

        category.Name = newName;

        await service.UpdateAsync(category);

        Console.WriteLine("Sửa Category thành công.");
    }

    // =========================================================
    // DELETE CATEGORY
    // =========================================================
    private static async Task DeleteCategoryAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<ICategoryService>();

        await ShowCategoriesWithServiceAsync(service);

        Console.WriteLine();
        Console.WriteLine("===== XÓA CATEGORY =====");

        int id = ReadPositiveInt("Nhập Category ID: ");

        var category = await service.GetByIdAsync(id);

        if (category == null)
        {
            Console.WriteLine("Không tìm thấy Category.");
            return;
        }

        Console.WriteLine($"Category: {category.Name}");

        if (!Confirm("Bạn có chắc muốn xóa? (Y/N): "))
        {
            Console.WriteLine("Đã hủy.");
            return;
        }

        try
        {
            await service.DeleteAsync(id);
            Console.WriteLine("Xóa Category thành công.");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (DbUpdateException)
        {
            Console.WriteLine(
                "Không thể xóa Category vì đang được dữ liệu khác sử dụng.");
        }
    }

    // =========================================================
    // PRODUCT MENU
    // =========================================================
    private static async Task ProductMenuAsync(
        IServiceProvider services)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== QUẢN LÝ PRODUCT =====");
            Console.WriteLine("1. Danh sách Products");
            Console.WriteLine("2. Thêm Product");
            Console.WriteLine("3. Sửa Product");
            Console.WriteLine("4. Xóa Product");
            Console.WriteLine("5. Tạo Order");
            Console.WriteLine("0. Quay lại");
            Console.Write("Chọn chức năng: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await ShowProductsAsync(services);
                    break;

                case "2":
                    await AddProductAsync(services);
                    break;

                case "3":
                    await UpdateProductAsync(services);
                    break;

                case "4":
                    await DeleteProductAsync(services);
                    break;

                case "5":
                    await CreateOrderAsync(services);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }
    }

    // =========================================================
    // SHOW PRODUCT
    // =========================================================
    private static async Task ShowProductsAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IProductService>();

        var products = await service.GetAllAsync();

        PrintProducts(products);
    }

    private static void PrintProducts(
        List<Product> products)
    {
        Console.WriteLine();
        Console.WriteLine("===== DANH SÁCH PRODUCTS =====");

        Console.WriteLine(
            $"{"ID",-4} " +
            $"{"Name",-28} " +
            $"{"Price",15} " +
            $"{"Qty",6} " +
            $"{"Category",-20}");

        Console.WriteLine(
            new string('-', 80));

        if (products.Count == 0)
        {
            Console.WriteLine("Chưa có Product.");
            return;
        }

        foreach (var product in products)
        {
            Console.WriteLine(
                $"{product.Id,-4} " +
                $"{product.Name,-28} " +
                $"{product.Price,15:N0} " +
                $"{product.Quantity,6} " +
                $"{product.Category?.Name ?? "N/A",-20}");
        }
    }

    // =========================================================
    // ADD PRODUCT
    // =========================================================
    private static async Task AddProductAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var productService = scope.ServiceProvider
            .GetRequiredService<IProductService>();

        var categoryService = scope.ServiceProvider
            .GetRequiredService<ICategoryService>();

        var categories = await categoryService.GetAllAsync();

        if (categories.Count == 0)
        {
            Console.WriteLine(
                "Chưa có Category. Hãy thêm Category trước.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("===== THÊM PRODUCT =====");

        PrintCategories(categories);

        string name = ReadRequiredString(
            "Tên Product: ",
            150);

        decimal price = ReadNonNegativeDecimal(
            "Giá: ");

        int quantity = ReadNonNegativeInt(
            "Số lượng tồn kho: ");

        int categoryId;

        while (true)
        {
            categoryId = ReadPositiveInt(
                "Category ID: ");

            bool exists = categories.Any(
                c => c.Id == categoryId);

            if (exists)
                break;

            Console.WriteLine(
                "Category ID không tồn tại.");
        }

        var product = new Product
        {
            Name = name,
            Price = price,
            Quantity = quantity,
            CategoryId = categoryId
        };

        await productService.AddAsync(product);

        Console.WriteLine(
            $"Thêm Product thành công. ID = {product.Id}");
    }

    // =========================================================
    // UPDATE PRODUCT
    // =========================================================
    private static async Task UpdateProductAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var productService = scope.ServiceProvider
            .GetRequiredService<IProductService>();

        var categoryService = scope.ServiceProvider
            .GetRequiredService<ICategoryService>();

        var products = await productService.GetAllAsync();

        PrintProducts(products);

        if (products.Count == 0)
            return;

        Console.WriteLine();
        Console.WriteLine("===== SỬA PRODUCT =====");

        int id = ReadPositiveInt(
            "Product ID: ");

        var product = await productService.GetByIdAsync(id);

        if (product == null)
        {
            Console.WriteLine("Không tìm thấy Product.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Tên hiện tại       : {product.Name}");
        Console.WriteLine($"Giá hiện tại       : {product.Price:N0}");
        Console.WriteLine($"Tồn kho hiện tại   : {product.Quantity}");
        Console.WriteLine(
            $"Category hiện tại  : {product.Category?.Name}");

        var categories = await categoryService.GetAllAsync();

        Console.WriteLine();
        PrintCategories(categories);

        string name = ReadRequiredString(
            "Tên mới: ",
            150);

        decimal price = ReadNonNegativeDecimal(
            "Giá mới: ");

        int quantity = ReadNonNegativeInt(
            "Số lượng mới: ");

        int categoryId;

        while (true)
        {
            categoryId = ReadPositiveInt(
                "Category ID mới: ");

            if (categories.Any(c => c.Id == categoryId))
                break;

            Console.WriteLine(
                "Category ID không tồn tại.");
        }

        product.Name = name;
        product.Price = price;
        product.Quantity = quantity;
        product.CategoryId = categoryId;

        await productService.UpdateAsync(product);

        Console.WriteLine("Sửa Product thành công.");
    }

    // =========================================================
    // DELETE PRODUCT
    // =========================================================
    private static async Task DeleteProductAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IProductService>();

        var products = await service.GetAllAsync();

        PrintProducts(products);

        if (products.Count == 0)
            return;

        Console.WriteLine();
        Console.WriteLine("===== XÓA PRODUCT =====");

        int id = ReadPositiveInt(
            "Product ID: ");

        var product = await service.GetByIdAsync(id);

        if (product == null)
        {
            Console.WriteLine("Không tìm thấy Product.");
            return;
        }

        Console.WriteLine(
            $"Bạn đang chọn: {product.Name}");

        if (!Confirm("Bạn có chắc muốn xóa? (Y/N): "))
        {
            Console.WriteLine("Đã hủy.");
            return;
        }

        try
        {
            await service.DeleteAsync(id);
            Console.WriteLine("Xóa Product thành công.");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (DbUpdateException)
        {
            Console.WriteLine(
                "Không thể xóa Product vì đang được Order sử dụng.");
        }
    }

    // =========================================================
    // CREATE ORDER
    // =========================================================
    private static async Task CreateOrderAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var productService = scope.ServiceProvider
            .GetRequiredService<IProductService>();

        var orderService = scope.ServiceProvider
            .GetRequiredService<IOrderService>();

        var products = await productService.GetAllAsync();

        PrintProducts(products);

        if (products.Count == 0)
            return;

        Console.WriteLine();
        Console.WriteLine("===== TẠO ORDER =====");

        int productId = ReadPositiveInt(
            "Product ID: ");

        var product = await productService
            .GetByIdAsync(productId);

        if (product == null)
        {
            Console.WriteLine("Không tìm thấy Product.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Product : {product.Name}");
        Console.WriteLine($"Giá     : {product.Price:N0} VND");
        Console.WriteLine($"Tồn kho : {product.Quantity}");

        if (product.Quantity <= 0)
        {
            Console.WriteLine("Product đã hết hàng.");
            return;
        }

        int quantity;

        while (true)
        {
            quantity = ReadPositiveInt(
                "Số lượng mua: ");

            if (quantity <= product.Quantity)
                break;

            Console.WriteLine(
                $"Không đủ hàng. Tồn kho hiện tại: " +
                $"{product.Quantity}");
        }

        decimal total = product.Price * quantity;

        Console.WriteLine();
        Console.WriteLine("----- THÔNG TIN ORDER -----");
        Console.WriteLine($"Product   : {product.Name}");
        Console.WriteLine($"Đơn giá   : {product.Price:N0} VND");
        Console.WriteLine($"Số lượng  : {quantity}");
        Console.WriteLine($"Thành tiền: {total:N0} VND");

        if (!Confirm("Xác nhận tạo Order? (Y/N): "))
        {
            Console.WriteLine("Đã hủy tạo Order.");
            return;
        }

        bool success = await orderService
            .CreateOrderAsync(productId, quantity);

        if (success)
        {
            Console.WriteLine("Tạo Order thành công.");
            Console.WriteLine(
                $"Tồn kho còn lại: " +
                $"{product.Quantity - quantity}");
        }
        else
        {
            Console.WriteLine(
                "Tạo Order thất bại. " +
                "Product không tồn tại hoặc không đủ hàng.");
        }
    }

    // =========================================================
    // STATISTICS MENU
    // =========================================================
    private static async Task StatisticsMenuAsync(
        IServiceProvider services)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== THỐNG KÊ =====");
            Console.WriteLine("1. Tổng doanh thu các sản phẩm đã bán");
            Console.WriteLine(
                "2. Số lượng đơn hàng trong khoảng thời gian");
            Console.WriteLine("0. Quay lại");
            Console.Write("Chọn chức năng: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await ShowRevenueAsync(services);
                    break;

                case "2":
                    await ShowOrderCountByDateAsync(services);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }
    }

    // =========================================================
    // TOTAL REVENUE
    // =========================================================
    private static async Task ShowRevenueAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IOrderService>();

        decimal revenue =
            await service.GetTotalRevenueAsync();

        Console.WriteLine();
        Console.WriteLine("===== TỔNG DOANH THU =====");
        Console.WriteLine(
            $"Tổng doanh thu: {revenue:N0} VND");
    }

    // =========================================================
    // ORDER COUNT BY DATE
    // =========================================================
    private static async Task ShowOrderCountByDateAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IOrderService>();

        Console.WriteLine();
        Console.WriteLine(
            "===== SỐ ĐƠN HÀNG THEO THỜI GIAN =====");

        DateTime from =
            ReadDate("Từ ngày (dd/MM/yyyy): ");

        DateTime to;

        while (true)
        {
            to = ReadDate(
                "Đến ngày (dd/MM/yyyy): ");

            if (to.Date >= from.Date)
                break;

            Console.WriteLine(
                "Đến ngày phải >= Từ ngày.");
        }

        // +1 ngày để lấy toàn bộ ngày kết thúc.
        DateTime endExclusive =
            to.Date.AddDays(1);

        int count = await service.CountOrdersAsync(
            from.Date,
            endExclusive);

        Console.WriteLine();
        Console.WriteLine(
            $"Từ {from:dd/MM/yyyy} " +
            $"đến {to:dd/MM/yyyy}");

        Console.WriteLine(
            $"Số lượng đơn hàng: {count}");
    }

    // =========================================================
    // HELPER: PRINT CATEGORY
    // =========================================================
    private static void PrintCategories(
        List<Category> categories)
    {
        Console.WriteLine();
        Console.WriteLine("Danh sách Categories:");

        Console.WriteLine("-------------------------------");
        Console.WriteLine($"{"ID",-5} {"Name",-25}");
        Console.WriteLine("-------------------------------");

        foreach (var category in categories)
        {
            Console.WriteLine(
                $"{category.Id,-5} {category.Name,-25}");
        }
    }

    private static async Task ShowCategoriesWithServiceAsync(
        ICategoryService service)
    {
        var categories = await service.GetAllAsync();

        PrintCategories(categories);
    }

    // =========================================================
    // INPUT VALIDATION
    // =========================================================
    private static string ReadRequiredString(
        string message,
        int maxLength)
    {
        while (true)
        {
            Console.Write(message);

            string? value = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine(
                    "Không được để trống.");
                continue;
            }

            if (value.Length > maxLength)
            {
                Console.WriteLine(
                    $"Tối đa {maxLength} ký tự.");
                continue;
            }

            return value;
        }
    }

    private static int ReadPositiveInt(
        string message)
    {
        while (true)
        {
            Console.Write(message);

            if (int.TryParse(
                    Console.ReadLine(),
                    out int value)
                && value > 0)
            {
                return value;
            }

            Console.WriteLine(
                "Vui lòng nhập số nguyên > 0.");
        }
    }

    private static int ReadNonNegativeInt(
        string message)
    {
        while (true)
        {
            Console.Write(message);

            if (int.TryParse(
                    Console.ReadLine(),
                    out int value)
                && value >= 0)
            {
                return value;
            }

            Console.WriteLine(
                "Vui lòng nhập số nguyên >= 0.");
        }
    }

    private static decimal ReadNonNegativeDecimal(
        string message)
    {
        while (true)
        {
            Console.Write(message);

            string input =
                Console.ReadLine()?.Trim() ?? "";

            bool success =
                decimal.TryParse(
                    input,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out decimal value)
                ||
                decimal.TryParse(
                    input,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out value);

            if (success && value >= 0)
                return value;

            Console.WriteLine(
                "Giá không hợp lệ. Ví dụ: 24990000");
        }
    }

    private static DateTime ReadDate(
        string message)
    {
        while (true)
        {
            Console.Write(message);

            string input =
                Console.ReadLine()?.Trim() ?? "";

            if (DateTime.TryParseExact(
                    input,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime date))
            {
                return date;
            }

            Console.WriteLine(
                "Ngày không hợp lệ. " +
                "Nhập theo định dạng dd/MM/yyyy.");
        }
    }

    private static bool Confirm(
        string message)
    {
        while (true)
        {
            Console.Write(message);

            string? input =
                Console.ReadLine()?.Trim().ToUpper();

            if (input == "Y")
                return true;

            if (input == "N")
                return false;

            Console.WriteLine(
                "Chỉ nhập Y hoặc N.");
        }
    }
}