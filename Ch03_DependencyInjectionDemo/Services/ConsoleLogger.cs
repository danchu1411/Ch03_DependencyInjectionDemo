namespace Ch03_DependencyInjectionDemo.Services;

public class ConsoleLogger : IAppLogger
{
    public Guid InstanceId { get; } = Guid.NewGuid();

    public void Log(string message)
        => Console.WriteLine($"    [LOG {InstanceId.ToString()[..8]}] {message}");
}
