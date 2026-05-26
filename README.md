# LifeTimes

**LifeTimes** is a lightweight, high-performance extension library for the built-in `Microsoft.Extensions.DependencyInjection` framework. It empowers developers to define dynamic, state-aware, and time-bound lifetimes for service resolutions in .NET.

---

## ⚡ The DI Gap: Why Do We Need LifeTimes?

The default Microsoft DI container is simple, fast, and robust. However, it only supports three rigid, static lifetimes:

1. **Transient**: Recreated on *every* resolution. (Can result in excessive allocations for heavy objects).
2. **Scoped**: Recreated once per *request scope*. (Tied directly to an HTTP request or manually created scope).
3. **Singleton**: Created *once* and lives for the entire application life cycle. (Impossible to refresh without restarts or complex factory wrappers).

### What standard .NET DI is missing:
* **No Cache-Like Timed Lifetimes:** You cannot configure a service to live for 10 minutes, expire, and automatically recreate on the next resolution (e.g., self-refreshing API clients, currency converters, or configuration readers).
* **No Contextual/Conditional Lifetimes:** You cannot resolve one instance or destroy/recreate instances dynamically based on runtime changes, tenant context, database state, or feature flags.
* **Synchronous-Only Resolution:** Built-in `IServiceProvider` resolves everything synchronously (`GetService<T>`). Resolving services that require asynchronous initialization or remote fetching usually forces developers into dangerous sync-over-async (`.GetAwaiter().GetResult()`) anti-patterns.

**LifeTimes** bridges these gaps by adding **Timed** and **Conditional** lifetimes with a fully **asynchronous resolution engine** (`GetServiceAsync<T>`).

---

## 📊 Comparison Table

| Lifetime | Default .NET DI | `Kanaka.LifeTimes` | Lifecycle Behavior | Primary Use Case |
|---|---|---|---|---|
| **Transient** | ✅ Yes | ✅ Yes (Standard) | Recreated every time it is resolved. | Stateless utility services. |
| **Scoped** | ✅ Yes | ✅ Yes (Standard) | Bound to an HTTP request or boundary scope. | Database contexts (like EF Core `DbContext`). |
| **Singleton** | ✅ Yes | ✅ Yes (Standard) | Created once and lives forever. | Loggers, immutable configuration. |
| **Timed** 🆕 | ❌ No | ✅ **Yes** | Persists for a specified duration, then automatically disposes and recreates on next resolve. | Temporary caches, currency exchanges, OAuth access tokens. |
| **Conditional** 🆕 | ❌ No | ✅ **Yes** | Recreated or disposed dynamically based on a custom condition evaluation. | Feature-flagged services, context-based tenants, connection switches. |

---

## 🔨 Installation

Install the package via the .NET CLI:

```bash
dotnet add package Kanaka.LifeTimes
```

## 🏗️ Working Detail

`ILifeTime` is registered in the the application's DI container. It maintains an internal DI container to manage user-configured service objects. `ITypeLifeTime<T>`(generic) is a singleton in the internal DI, handling scope creation and disposal of it's service.

![Working Detail](https://raw.githubusercontent.com/KanakaSoftware/LifeTimes/main/images/working-detail.png)

## 💡 Inspiration

The idea for this library came from the podcast [Episode of a Lifetime](https://www.breakpoint.show/podcast/episode-036-episode-of-a-lifetime/) and a blog post by [Andrew Lock](https://andrewlock.net/going-beyond-singleton-scoped-and-transient-lifetimes/), which highlighted four additional service lifetimes beyond the standard DI scopes.

## 🤝 Getting support

If you have a specific question about this project, open a issue with *question* label. If you encounter a bug or would like to request a feature, submit an issue.
