using LifeTimes;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
    .AddLifeTime((p, o) =>
        {
            o.AddTimed<ITimedService, TimedService>(p, TimeSpan.FromSeconds(10));
            o.AddConditional<IConditionedService, ConditionedService>(p);
        }
    );

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapGet("/timed-service", GetTimedServiceAsync);

app.MapGet("/timed-service-delay", GetTimedServiceDelayAsync);

app.MapGet("/conditional-service", GetConditionalServiceAsync);

app.MapGet("/gc-clean", () =>
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
    return TypedResults.Ok();
});

app.Run();

static async Task<IResult> GetTimedServiceAsync(CancellationToken cancellationToken, ILifeTime lifeTime, int? count = 1)
{
    var service = await lifeTime.GetServiceAsync<ITimedService>(cancellationToken);
    if (service == null)
    {
        return TypedResults.NotFound();
    }
    var code = service.GetValue(count!.Value);
    return TypedResults.Ok(code);
}

static async Task<IResult> GetTimedServiceDelayAsync(CancellationToken cancellationToken, ILifeTime lifeTime, int? delay = 1)
{
    var service = await lifeTime.GetRequiredServiceAsync<ITimedService>(cancellationToken);
    var ct = lifeTime.GetCancellationToken<ITimedService>();
    await Task.Delay(TimeSpan.FromSeconds(delay!.Value), ct);
    var code = service.GetValue(1);
    return TypedResults.Ok(code);
}

static async Task<IResult> GetConditionalServiceAsync(CancellationToken cancellationToken, ILifeTime lifeTime, int? count = 1)
{
    var service = await lifeTime.GetServiceAsync<IConditionedService>();
    var ct = lifeTime.GetCancellationToken<IConditionedService>();
    if (service == null)
    {
        return TypedResults.NotFound();
    }
    var code = service.GetValue(count!.Value, ct);
    return TypedResults.Ok(code);
}
