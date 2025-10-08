# LifeTimes

**LifeTimes** is a lightweight library that extends the built-in *Microsoft.DependencyInjection* lifetimes. It provides additional lifetimes for your services, which includes:

**Conditional Lifetime** – Automatically creates or disposes services based on given condition.

**Timed Lifetime** – Services that automatically expire after a specified duration or interval.

*LifeTimes* seamlessly integrates with *Microsoft.Extensions.DependencyInjection* and follows the same familiar patterns, making it easy to adopt in ASP.NET Core, console apps, or any DI-enabled .NET project.

## Installation

LifeTimes is available on [NuGet](https://www.nuget.org/packages/KanakaSoftware.LifeTimes).

```text
dotnet add package KanakaSoftware.LifeTimes
```

## Usage

The following code demonstrates basic usage of LifeTimes. For a full tutorial see sample [Web](src/Web) project in the repository.

```csharp
ServiceCollection services = new();
services
    .AddLifeTime((p, o) =>
        {
            o.AddTimed<CurrencyService>(p, TimeSpan.FromMinutes(10));
            o.AddConditional<TokenService>(p);
        }
    );
using ServiceProvider provider = services.BuildServiceProvider();
var lifetime = provider.GetService<ILifeTime>();
var currencyService = lifetime.GetService<CurrencyService>();
var rate = currencyService.GetRate("INR");
var tokenService = lifetime.GetService<TokenService>();
var token = tokenService.GetToken();


class CurrencyService
{
    private readonly Dictionary<string, decimal> _rates = new();

    public CurrencyService()
    {
        // initialize/update _rates
    }

    public decimal GetRate(string currency)
    {
        return _rates.GetValueOrDefault(currency);
    }
}

class TokenService : IConditional
{
    private readonly string token = string.Empty;
    public string GetToken()
    {
        return token;
    }
    public bool Condition()
    {
        var expired = false; // check token expire, for demonstration it's set to false
        return expired;
    }
}
```

## Getting support

If you have a specific question about this project, open a issue with *question* label. If you encounter a bug or would like to request a feature, submit an issue.

## Workitems

- [ ] #4
- [ ] #1
- [ ] #2
- [ ] #3
