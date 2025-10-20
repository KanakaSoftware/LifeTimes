using LifeTimes;

namespace Web.Services;

public interface IConditionedService : IConditional
{
    int GetValue(int count, CancellationToken ct = default);
}
