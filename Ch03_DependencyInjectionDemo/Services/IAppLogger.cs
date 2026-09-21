namespace Ch03_DependencyInjectionDemo.Services;

public interface IAppLogger
{
    Guid InstanceId { get; }
    void Log(string message);
}
