using Ch03_DependencyInjectionDemo.Entities;

namespace Ch03_DependencyInjectionDemo.Solid;

// =====================================================================
// S - SINGLE RESPONSIBILITY PRINCIPLE
// Slide "Single Responsibility Principle Demo"
// Moi lop chi co MOT ly do de thay doi.
// =====================================================================

/// <summary>SAI: mot lop vua tinh tien, vua ghi file, vua gui mail.</summary>
public class InvoiceBad
{
    public decimal CalculateTotal(IEnumerable<Product> items) => items.Sum(i => i.Price * i.Quantity);
    public void SaveToFile(string path) { /* ghi file */ }
    public void SendEmail(string to) { /* gui mail */ }
}

/// <summary>DUNG: tach thanh ba trach nhiem doc lap.</summary>
public class InvoiceCalculator
{
    public decimal CalculateTotal(IEnumerable<Product> items) => items.Sum(i => i.Price * i.Quantity);
}

public class InvoiceFileWriter
{
    public string Write(decimal total) => $"Da ghi hoa don tong {total:N0} ra file.";
}

public class InvoiceMailer
{
    public string Send(string to, decimal total) => $"Da gui hoa don {total:N0} toi {to}.";
}

// =====================================================================
// O - OPEN/CLOSED PRINCIPLE
// Slide "Open/Closed Principle Demo"
// Mo cho mo rong, dong voi sua doi.
// =====================================================================

/// <summary>SAI: them loai khach hang moi phai sua switch, tuc la sua lop cu.</summary>
public class DiscountCalculatorBad
{
    public decimal GetDiscount(string customerType, decimal amount) => customerType switch
    {
        "Regular" => amount * 0.05m,
        "Vip" => amount * 0.10m,
        _ => 0m
    };
}

/// <summary>DUNG: them loai khach hang = them mot lop moi, khong dong vao code cu.</summary>
public interface IDiscountPolicy
{
    string Name { get; }
    decimal GetDiscount(decimal amount);
}

public class RegularCustomerDiscount : IDiscountPolicy
{
    public string Name => "Regular";
    public decimal GetDiscount(decimal amount) => amount * 0.05m;
}

public class VipCustomerDiscount : IDiscountPolicy
{
    public string Name => "VIP";
    public decimal GetDiscount(decimal amount) => amount * 0.10m;
}

public class StudentDiscount : IDiscountPolicy      // lop MOI, khong sua lop nao ca
{
    public string Name => "Student";
    public decimal GetDiscount(decimal amount) => amount * 0.15m;
}

// =====================================================================
// L - LISKOV SUBSTITUTION PRINCIPLE
// Slide "Liskov Substitution Principle Demo"
// Lop con phai thay the duoc lop cha ma khong lam hong chuong trinh.
// =====================================================================

/// <summary>SAI: Square ke thua Rectangle nhung pha vo hop dong cua lop cha.</summary>
public class RectangleBad
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
    public int Area => Width * Height;
}

public class SquareBad : RectangleBad
{
    public override int Width
    {
        get => base.Width;
        set { base.Width = value; base.Height = value; }   // tac dung phu ngoai du kien
    }

    public override int Height
    {
        get => base.Height;
        set { base.Width = value; base.Height = value; }
    }
}

/// <summary>DUNG: tach abstraction chung, khong ep quan he ke thua khong dung.</summary>
public interface IShape
{
    string Name { get; }
    int Area { get; }
}

public class Rectangle : IShape
{
    public Rectangle(int width, int height) { Width = width; Height = height; }
    public int Width { get; }
    public int Height { get; }
    public string Name => "Rectangle";
    public int Area => Width * Height;
}

public class Square : IShape
{
    public Square(int side) => Side = side;
    public int Side { get; }
    public string Name => "Square";
    public int Area => Side * Side;
}

// =====================================================================
// I - INTERFACE SEGREGATION PRINCIPLE
// Slide "Interface Segregation Principle Demo"
// Khong ep lop phai cai dat phuong thuc no khong dung.
// =====================================================================

/// <summary>SAI: may in don gian buoc phai cai dat Fax va Scan.</summary>
public interface IMultiFunctionDeviceBad
{
    void Print(string content);
    void Scan(string path);
    void Fax(string number);
}

/// <summary>DUNG: tach interface nho theo dung nang luc.</summary>
public interface IPrinter { string Print(string content); }
public interface IScanner { string Scan(string path); }
public interface IFax { string Fax(string number); }

public class SimplePrinter : IPrinter
{
    public string Print(string content) => $"[SimplePrinter] In: {content}";
}

public class OfficeMachine : IPrinter, IScanner, IFax
{
    public string Print(string content) => $"[OfficeMachine] In: {content}";
    public string Scan(string path) => $"[OfficeMachine] Quet: {path}";
    public string Fax(string number) => $"[OfficeMachine] Fax toi: {number}";
}

// =====================================================================
// D - DEPENDENCY INVERSION PRINCIPLE
// Slide "Dependency Inversion Principle (DIP)"
// Module cap cao KHONG phu thuoc module cap thap; ca hai phu thuoc abstraction.
// =====================================================================

/// <summary>SAI: OrderProcessorBad tu tao SqlOrderStore - khong the thay the khi test.</summary>
public class SqlOrderStore
{
    public string Save(string order) => $"[SQL] Da luu don: {order}";
}

public class OrderProcessorBad
{
    private readonly SqlOrderStore _store = new();   // phu thuoc cung
    public string Process(string order) => _store.Save(order);
}

/// <summary>DUNG: phu thuoc vao abstraction, cai dat cu the duoc tiem tu ngoai vao.</summary>
public interface IOrderStore
{
    string Save(string order);
}

public class SqlServerOrderStore : IOrderStore
{
    public string Save(string order) => $"[SQL Server] Da luu don: {order}";
}

public class InMemoryOrderStore : IOrderStore    // dung cho unit test
{
    public List<string> Orders { get; } = new();
    public string Save(string order)
    {
        Orders.Add(order);
        return $"[InMemory] Da luu don: {order}";
    }
}

public class OrderProcessor
{
    private readonly IOrderStore _store;
    public OrderProcessor(IOrderStore store) => _store = store;   // Constructor Injection
    public string Process(string order) => _store.Save(order);
}
