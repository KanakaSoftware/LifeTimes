namespace Web.Services;

public class TimedService : ITimedService, IAsyncDisposable
{
    private readonly List<Customer> _customers = new List<Customer>();
    public int GetValue(int count, CancellationToken ct = default)
    {
        for (int i = 0; i < count; i++)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }
            _customers.Add(new Customer
            {
                message = $"Hello {i}",
                value = i
            });
        }
        return GetHashCode();
    }

    public ValueTask DisposeAsync()
    {
        Console.WriteLine($"{nameof(TimedService)} {GetHashCode()} Destorying...");
        return default;
    }
}

class Customer
{
    public string? message;
    public decimal value;
}
