using Microsoft.Extensions.DependencyInjection;

namespace AutoRegisterInject.IntegrationTest.Project2
{
    public static class Project2
    {
        public static void Init()
        {
            var serviceCollection = new ServiceCollection()
                .AutoRegister()
                .AutoRegisterFromAutoRegisterInjectIntegrationTestProject1()
                .AutoRegisterFromAutoRegisterInjectIntegrationTestProject2();

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
}
