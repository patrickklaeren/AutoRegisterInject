using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AutoRegisterInject.IntegrationTest.Project1
{
    public static class Project1
    {
        public static void Init()
        {
            var serviceCollection = new ServiceCollection()
                .AutoRegister()
                .AutoRegisterFromAutoRegisterInjectIntegrationTestProject1();

            serviceCollection.BuildServiceProvider();
        }
    }

    [RegisterScoped]
    public partial class PartialClassTest
    {

    }

    public partial class PartialClassTest
    {

    }

    [
        RegisterTransient, 
        RegisterSingleton, 
        RegisterScoped, 
        TryRegisterScoped, 
        TryRegisterSingleton, 
        TryRegisterTransient, 
        TryRegisterKeyedScoped(serviceKey: "TryRegisterKeyedScoped"),
        TryRegisterKeyedSingleton(serviceKey: "TryRegisterKeyedSingleton"),
        TryRegisterKeyedTransient(serviceKey: "TryRegisterKeyedScoped"),
        RegisterKeyedScoped(serviceKey: "RegisterKeyedScoped"),
        RegisterKeyedSingleton(serviceKey: "RegisterKeyedSingleton"),
        RegisterKeyedTransient(serviceKey: "RegisterKeyedTransient")
    ]
    public class MultipleRegisterTest
    {

    }

    [RegisterScoped]
    public class ScopedTest
    {

    }

    [RegisterSingleton]
    public class SingletonTest
    {

    }

    [RegisterTransient]
    public class TransientTest
    {

    }
    
    [TryRegisterScoped]
    public class TryScopedTest
    {

    }

    [TryRegisterSingleton]
    public class TrySingletonTest
    {

    }

    [TryRegisterTransient]
    public class TryTransientTest
    {

    }
    
    [TryRegisterKeyedScoped(serviceKey: "TryRegisterKeyedScoped")]
    public class TryKeyedScopedTest
    {

    }

    [TryRegisterKeyedSingleton(serviceKey: "TryRegisterKeyedSingleton")]
    public class TryKeyedSingletonTest
    {

    }

    [TryRegisterKeyedTransient(serviceKey: "TryRegisterKeyedScoped")]
    public class TryKeyedTransientTest
    {

    }
    
    [RegisterKeyedScoped(serviceKey: "RegisterKeyedScoped")]
    public class KeyedScopedTest
    {

    }

    [RegisterKeyedSingleton(serviceKey: "RegisterKeyedSingleton")]
    public class KeyedSingletonTest
    {

    }

    [RegisterKeyedTransient(serviceKey: "RegisterKeyedTransient")]
    public class KeyedTransientTest
    {

    }

    public interface IInterfaceTest
    {

    }

    [RegisterScoped]
    public class RegisterScopedInterfaceTest : IInterfaceTest
    {

    }
    
    [TryRegisterScoped]
    public class TryRegisterScopedInterfaceTest : IInterfaceTest
    {

    }
    
    [TryRegisterKeyedScoped(serviceKey: "TryRegisterKeyedScopedInterface")]
    public class TryRegisterKeyedScopedInterfaceTest : IInterfaceTest
    {

    }
    
    [RegisterKeyedScoped(serviceKey: "RegisterKeyedScopedInterface")]
    public class RegisterKeyedScopedInterfaceTest : IInterfaceTest
    {

    }

    public interface IMultiInterfaceTest
    {

    }

    [RegisterScoped]
    public class RegisterMultiInterfaceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
    
    [TryRegisterScoped]
    public class TryRegisterMultiInterfaceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
    
    [TryRegisterKeyedScoped(serviceKey: "TryRegisterKeyedScopedMultipleInterface")]
    public class TryRegisterKeyedMultiInterfaceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
    
    [RegisterKeyedScoped(serviceKey: "RegisterKeyedScopedMultipleInterface")]
    public class RegisterKeyedMultiInterfaceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }

    public interface IIgnore
    {
        
    }

    // Multiple Interface Single Ignorance
    [RegisterScoped(onlyRegisterAs: typeof(IIgnore))]
    public class RegisterScopedMultiInterfaceIgnoranceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable, IIgnore
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
    
    [TryRegisterScoped(onlyRegisterAs: typeof(IIgnore))]
    public class TryRegisterScopedMultiInterfaceIgnoranceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable, IIgnore
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
    
    [TryRegisterKeyedScoped(serviceKey: "TryRegisterKeyedScopedMultipleInterfaceSingleIgnore", onlyRegisterAs: typeof(IIgnore))]
    public class TryRegisterKeyedScopedMultiInterfaceIgnoranceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable, IIgnore
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
    
    [RegisterKeyedScoped(serviceKey: "RegisterKeyedScopedMultipleInterfaceSingleIgnore", onlyRegisterAs: typeof(IIgnore))]
    public class RegisterKeyedScopedMultiInterfaceIgnoranceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable, IIgnore
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
    
    
    // Multiple Interface Multiple Ignorance
    [RegisterScoped(typeof(IIgnore), typeof(IInterfaceTest))]
    public class RegisterScopedMultiInterfaceMultiIgnoranceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable, IIgnore
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
    
    [TryRegisterScoped(typeof(IIgnore), typeof(IInterfaceTest))]
    public class TryRegisterScopedMultiInterfaceMultiIgnoranceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable, IIgnore
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
    
    [TryRegisterKeyedScoped(serviceKey: "TryRegisterKeyedScopedMultipleInterfaceMultipleIgnore", typeof(IIgnore), typeof(IInterfaceTest))]
    public class TryRegisterKeyedScopedMultiInterfaceMultiIgnoranceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable, IIgnore
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
    
    [RegisterKeyedScoped(serviceKey: "RegisterKeyedScopedMultipleInterfaceMultipleIgnore", typeof(IIgnore), typeof(IInterfaceTest))]
    public class RegisterKeyedScopedMultiInterfaceMultiIgnoranceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable, IIgnore
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}