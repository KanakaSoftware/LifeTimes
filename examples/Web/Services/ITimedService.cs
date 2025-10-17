namespace Web.Services;

public interface ITimedService
{
    int GetValue(int count, CancellationToken ct = default);
}
