using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes.Types;

public class ILifeTimeTest
{
    private class TestService
    {
    }
    private class TestServiceTrueCondition : IConditional
    {
        public bool Condition() => true;
    }
    private class TestServiceDisposable : IDisposable
    {
        public bool _disposed = false;

        public void Dispose() => _disposed = true;
    }
    private interface ITestServiceInterface
    {
    }
    private class TestServiceInterface : ITestServiceInterface
    {
    }
    private interface ITestServiceTrueConditionInterface : IConditional
    {
    }
    private class TestServiceTrueConditionInterface : ITestServiceTrueConditionInterface
    {
        public bool Condition() => true;
    }

    [Fact]
    public void GetService_ReturnsNull()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) => { });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance = lifetime.GetService<TestService>();

        // assert
        Assert.Null(instance);
    }

    [Fact]
    public void GetRequiredService_ThrowsInvalidOperationException()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) => { });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();

        // assert
        Assert.Throws<InvalidOperationException>(() => lifetime.GetRequiredService<TestService>());
    }

    [Fact]
    public void GetCancellationToken_UnregisteredService_ThrowsInvalidOperationException()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();

        // assert
        Assert.Throws<InvalidOperationException>(() => lifetime.GetCancellationToken<TestService>());
    }

    [Fact]
    public void Methods_WhenDisposed_ThrowsObjectDisposedException()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        provider.Dispose();

        // assert
        Assert.Throws<ObjectDisposedException>(() => lifetime.GetService<TestService>());
        Assert.Throws<ObjectDisposedException>(() => lifetime.GetRequiredService<TestService>());
        Assert.Throws<ObjectDisposedException>(() => lifetime.GetCancellationToken<TestService>());
    }

    [Fact]
    public void GetService_ReturnsIntance()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
            {
                o.AddTimed<TestService>(p, TimeSpan.FromMilliseconds(500));
                o.AddConditional<TestServiceTrueCondition>(p);
            }
        );
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = lifetime.GetService<TestService>();
        var instance2 = lifetime.GetService<TestServiceTrueCondition>();

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
    }

    [Fact]
    public void GetService_WithInterface_ReturnsIntance()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
            {
                o.AddTimed<ITestServiceInterface, TestServiceInterface>(p, TimeSpan.FromMilliseconds(500));
                o.AddConditional<ITestServiceTrueConditionInterface, TestServiceTrueConditionInterface>(p);
            }
        );
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = lifetime.GetService<ITestServiceInterface>();
        var instance2 = lifetime.GetService<ITestServiceTrueConditionInterface>();

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
    }

    [Fact]
    public void Get_WithJustInterface_ThrowsInvalidOperationException()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
            {
                o.AddTimed<ITestServiceInterface>(p, TimeSpan.FromMilliseconds(500));
                o.AddConditional<ITestServiceTrueConditionInterface>(p);
            }
        );
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();

        // assert
        Assert.Throws<InvalidOperationException>(() => lifetime.GetRequiredService<ITestServiceInterface>());
        Assert.Throws<InvalidOperationException>(() => lifetime.GetRequiredService<ITestServiceTrueConditionInterface>());
    }

    [Fact]
    public void GetService_WithFactory_ReturnsIntance()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
            {
                o.AddTimed<TestService>(() => new TestService(), TimeSpan.FromMilliseconds(500));
                o.AddConditional<TestServiceTrueCondition>(() => new TestServiceTrueCondition());
            }
        );
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = lifetime.GetService<TestService>();
        var instance2 = lifetime.GetService<TestServiceTrueCondition>();

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
    }

    [Fact]
    public void GetService_WithFactoryAndInterface_ReturnsIntance()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
            {
                o.AddTimed<ITestServiceInterface, TestServiceInterface>(() => new TestServiceInterface(), TimeSpan.FromMilliseconds(500));
                o.AddConditional<ITestServiceTrueConditionInterface, TestServiceTrueConditionInterface>(() => new TestServiceTrueConditionInterface());
            }
        );
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = lifetime.GetService<ITestServiceInterface>();
        var instance2 = lifetime.GetService<ITestServiceTrueConditionInterface>();

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
    }
}
