# LifeTimes

**LifeTimes** is a lightweight library that extends the built-in *Microsoft.DependencyInjection* lifetimes. It provides additional lifetimes for your services, which includes:

**Conditional Lifetime** – Automatically creates or disposes services based on given condition.

**Timed Lifetime** – Services that automatically expire after a specified duration or interval.

*LifeTimes* seamlessly integrates with *Microsoft.Extensions.DependencyInjection* and follows the same familiar patterns, making it easy to adopt in ASP.NET Core, console apps, or any DI-enabled .NET Core project.

## 🔨 Installation

LifeTimes is available on [NuGet](https://www.nuget.org/packages/Kanaka.LifeTimes).

```text
dotnet add package Kanaka.LifeTimes
```

## 🧩 Usage

The following code demonstrates basic usage of LifeTimes. For a full tutorial see sample [Web](https://github.com/KanakaSoftware/LifeTimes/blob/main/examples/Web) project in the repository.

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

## 🏗️ Working Detail

`ILifeTime` is registered in the the application's DI container. It maintains an internal DI container to manage user-configured service objects. `ITypeLifeTime<T>`(generic) is a singleton in the internal DI, handling scope creation and disposal of it's service.

![Working Detail](https://github.com/KanakaSoftware/LifeTimes/blob/main/images/working-detail.svg)

## 💡 Inspiration

The idea for this library came from the podcast [Episode of a Lifetime](https://www.breakpoint.show/podcast/episode-036-episode-of-a-lifetime/) and a blog post by [Andrew Lock](https://andrewlock.net/going-beyond-singleton-scoped-and-transient-lifetimes/), which highlighted four additional service lifetimes beyond the standard DI scopes.

## 🤝 Getting support

If you have a specific question about this project, open a issue with *question* label. If you encounter a bug or would like to request a feature, submit an issue.
