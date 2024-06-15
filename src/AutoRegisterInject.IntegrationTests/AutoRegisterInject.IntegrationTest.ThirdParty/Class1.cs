using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoRegisterInject.IntegrationTest.ThirdParty
{
    /// <summary>
    /// Main class for project
    /// </summary>
    public static class Project3
    {
        /// <summary>
        /// Init entry point for project
        /// </summary>
        public static void Init()
        {
            var serviceCollection = new ServiceCollection()
                .AutoRegister();

            serviceCollection.BuildServiceProvider();
        }
    }

    /// <summary>
    /// Baseline
    /// </summary>
    [RegisterScoped]
    public class Baseline { }

    /// <summary>
    /// Fluent validator
    /// </summary>
    [RegisterScoped]
    public class FluentValidator : AbstractValidator<Baseline> { }

    /// <summary>
    /// Fluent validator 2
    /// </summary>
    [RegisterScoped(typeof(IValidator<Baseline>))]
    public class FluentValidator2 : AbstractValidator<Baseline> { }
}
