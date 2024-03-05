using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoRegisterInject.IntegrationTest.ThirdParty
{
    public static class Project3
    {
        public static void Init()
        {
            var serviceCollection = new ServiceCollection()
                .AutoRegister();

            serviceCollection.BuildServiceProvider();
        }
    }

    [RegisterScoped]
    public class Baseline { }

    [RegisterScoped]
    public class FluentValidator : AbstractValidator<Baseline> { }

    [RegisterScoped(typeof(IValidator<Baseline>))]
    public class FluentValidator2 : AbstractValidator<Baseline> { }
}
