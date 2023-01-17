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
    public class MultiInterfaceTest : IInterfaceTest, IMultiInterfaceTest
    {

    }
}
