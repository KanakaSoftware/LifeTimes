using LifeTimes;

namespace web.Services;

public class ConditionedService : IDisposable, IAsyncDisposable, IConditional
{
    private readonly List<Customer> _customers = new List<Customer>();
    private readonly HttpClient _httpClient;

    public ConditionedService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

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

    public void Dispose()
    {
        Console.WriteLine($"{nameof(ConditionedService)} {GetHashCode()} Destorying...");
    }

    public ValueTask DisposeAsync()
    {
        Console.WriteLine($"{nameof(ConditionedService)} {GetHashCode()} Async Destorying...");
        return default;
    }

    public bool Condition()
    {
        var response = _httpClient.GetAsync($"http://localhost:3000/condition").GetAwaiter()
            .GetResult();
        response.EnsureSuccessStatusCode();
        return bool.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
    }
}
