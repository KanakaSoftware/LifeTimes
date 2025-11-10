using LifeTimes;

namespace Web.Services;

public class ConditionedService : IConditionedService, IDisposable
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

    public async ValueTask<bool> ConditionAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"http://localhost:3000/condition", cancellationToken);
        response.EnsureSuccessStatusCode();
        return bool.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
