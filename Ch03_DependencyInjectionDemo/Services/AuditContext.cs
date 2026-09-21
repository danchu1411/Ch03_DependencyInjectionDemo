namespace Ch03_DependencyInjectionDemo.Services;

/// <summary>
/// AMBIENT CONTEXT PATTERN (slide "DI - Ambient Context Pattern").
///
/// Phu thuoc duoc truy cap qua mot thuoc tinh tinh thay vi truyen qua constructor.
/// Uu diem : khong lam "o nhiem" chu ky ham cho nhung thu dung o rat nhieu noi
///           (thoi gian he thong, nguoi dung hien tai, ngon ngu, don vi tien te).
/// Nhuoc diem: la mot dang bien toan cuc tra hinh - kho unit test, de bi lam dung.
///           Chi nen dung cho cross-cutting concern, KHONG dung cho nghiep vu.
///
/// Dung AsyncLocal de moi luong / moi request co gia tri rieng.
/// </summary>
public abstract class AuditContext
{
    private static readonly AsyncLocal<AuditContext?> CurrentContext = new();

    public static AuditContext Current
    {
        get => CurrentContext.Value ?? DefaultAuditContext.Instance;
        set => CurrentContext.Value = value;
    }

    public abstract string UserName { get; }

    public abstract void Write(string message);
}

/// <summary>Cai dat mac dinh - khong ai gan Current thi dung cai nay.</summary>
public sealed class DefaultAuditContext : AuditContext
{
    public static readonly DefaultAuditContext Instance = new();

    private DefaultAuditContext() { }

    public override string UserName => "anonymous";

    public override void Write(string message)
        => Console.WriteLine($"    [AUDIT] {message}");
}

/// <summary>Cai dat cho moi truong test: khong in ra man hinh, chi ghi vao bo nho.</summary>
public sealed class FakeAuditContext : AuditContext
{
    public List<string> Entries { get; } = new();

    public FakeAuditContext(string userName) => UserName = userName;

    public override string UserName { get; }

    public override void Write(string message) => Entries.Add(message);
}
