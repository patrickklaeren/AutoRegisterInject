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
        TryRegisterKeyedScoped(ServiceKey: "TryRegisterKeyedScoped"),
        TryRegisterKeyedSingleton(ServiceKey: "TryRegisterKeyedSingleton"),
        TryRegisterKeyedTransient(ServiceKey: "TryRegisterKeyedScoped"),
        RegisterKeyedScoped(ServiceKey: "RegisterKeyedScoped"),
        RegisterKeyedSingleton(ServiceKey: "RegisterKeyedSingleton"),
        RegisterKeyedTransient(ServiceKey: "RegisterKeyedTransient")
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
    
    [TryRegisterKeyedScoped(ServiceKey: "TryRegisterKeyedScoped")]
    public class TryKeyedScopedTest
    {

    }

    [TryRegisterKeyedSingleton(ServiceKey: "TryRegisterKeyedSingleton")]
    public class TryKeyedSingletonTest
    {

    }

    [TryRegisterKeyedTransient(ServiceKey: "TryRegisterKeyedScoped")]
    public class TryKeyedTransientTest
    {

    }
    
    [RegisterKeyedScoped(ServiceKey: "RegisterKeyedScoped")]
    public class KeyedScopedTest
    {

    }

    [RegisterKeyedSingleton(ServiceKey: "RegisterKeyedSingleton")]
    public class KeyedSingletonTest
    {

    }

    [RegisterKeyedTransient(ServiceKey: "RegisterKeyedTransient")]
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
    
    [TryRegisterKeyedScoped(ServiceKey: "TryRegisterKeyedScopedInterface")]
    public class TryRegisterKeyedScopedInterfaceTest : IInterfaceTest
    {

    }
    
    [RegisterKeyedScoped(ServiceKey: "RegisterKeyedScopedInterface")]
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
    
    [TryRegisterKeyedScoped(ServiceKey: "TryRegisterKeyedScopedMultipleInterface")]
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
    
    [RegisterKeyedScoped(ServiceKey: "RegisterKeyedScopedMultipleInterface")]
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
    
    [TryRegisterKeyedScoped(ServiceKey: "TryRegisterKeyedScopedMultipleInterfaceSingleIgnore", onlyRegisterAs: typeof(IIgnore))]
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
    
    [RegisterKeyedScoped(ServiceKey: "RegisterKeyedScopedMultipleInterfaceSingleIgnore", onlyRegisterAs: typeof(IIgnore))]
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
    
    [TryRegisterKeyedScoped(ServiceKey: "TryRegisterKeyedScopedMultipleInterfaceMultipleIgnore", typeof(IIgnore), typeof(IInterfaceTest))]
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
    
    [RegisterKeyedScoped(ServiceKey: "RegisterKeyedScopedMultipleInterfaceMultipleIgnore", typeof(IIgnore), typeof(IInterfaceTest))]
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