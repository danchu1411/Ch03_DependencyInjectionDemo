namespace Ch03_DependencyInjectionDemo.Services;

/// <summary>
/// Cai dat thu hai cua cung mot abstraction. Doi kenh gui thong bao
/// chi can doi MOT dong dang ky trong Program.cs - khong sua ProductService.
/// </summary>
public class SmsNotificationService : INotificationService
{
    private readonly IAppLogger _logger;

    public SmsNotificationService(IAppLogger logger) => _logger = logger;

    public Guid InstanceId { get; } = Guid.NewGuid();

    public async Task SendAsync(string to, string subject, string body)
    {
        await Task.Delay(50);
        _logger.Log($"SMS -> {to} | {subject}");
    }
}
