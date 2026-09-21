namespace Ch03_DependencyInjectionDemo.Services;

public class EmailNotificationService : INotificationService
{
    private readonly IAppLogger _logger;

    public EmailNotificationService(IAppLogger logger) => _logger = logger;

    public Guid InstanceId { get; } = Guid.NewGuid();

    public async Task SendAsync(string to, string subject, string body)
    {
        await Task.Delay(100);   // gia lap goi SMTP
        _logger.Log($"Email -> {to} | {subject}");
    }
}
