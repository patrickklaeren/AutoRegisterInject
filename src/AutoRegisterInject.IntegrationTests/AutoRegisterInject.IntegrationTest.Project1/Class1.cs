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

    [RegisterTransient, RegisterSingleton, RegisterScoped]
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

    public interface IInterfaceTest
    {

    }

    [RegisterScoped]
    public class InterfaceTest : IInterfaceTest
    {

    }

    public interface IMultiInterfaceTest
    {

    }

    [RegisterScoped]
    public class MultiInterfaceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable
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

    [RegisterScoped(typeof(IIgnore))]
    public class MultiInterfaceIgnoranceTest : IInterfaceTest, IMultiInterfaceTest, IDisposable, IAsyncDisposable, IIgnore
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
