namespace Ch03_DependencyInjectionDemo.Services;

public interface INotificationService
{
    Guid InstanceId { get; }
    Task SendAsync(string to, string subject, string body);
}
