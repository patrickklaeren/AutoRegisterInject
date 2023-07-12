using Xunit;

namespace AutoRegisterInject.Tests;

public partial class GenerationTests
{
    [Fact]
    public async Task ShouldRegisterScoped()
    {
        const string INPUT = @"[RegisterScoped]
public class Foo { }";

        const string EXPECTED = @"// <auto-generated>
//     Automatically generated by AutoRegisterInject.
//     Changes made to this file may be lost and may cause undesirable behaviour.
// </auto-generated>
using Microsoft.Extensions.DependencyInjection;
public static class AutoRegisterInjectServiceCollectionExtension
{
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AutoRegisterFromTestProject(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
    {
        return AutoRegister(serviceCollection);
    }

    internal static Microsoft.Extensions.DependencyInjection.IServiceCollection AutoRegister(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<Foo>();
        return serviceCollection;
    }
}";

        await RunGenerator(INPUT, EXPECTED);
    }

    [Fact]
    public async Task ShouldRegisterScopedNoInterface()
    {
        const string INPUT = @"[RegisterScopedNoInterface]
public class Foo : IFoo { }
public interface IFoo { }
";
        const string EXPECTED = @"// <auto-generated>
//     Automatically generated by AutoRegisterInject.
//     Changes made to this file may be lost and may cause undesirable behaviour.
// </auto-generated>
using Microsoft.Extensions.DependencyInjection;
public static class AutoRegisterInjectServiceCollectionExtension
{
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AutoRegisterFromTestProject(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
    {
        return AutoRegister(serviceCollection);
    }

    internal static Microsoft.Extensions.DependencyInjection.IServiceCollection AutoRegister(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<Foo>();
        return serviceCollection;
    }
}";
        await RunGenerator(INPUT, EXPECTED);
    }

    [Fact]
    public async Task ShouldRegisterScopedFromInterface()
    {
        const string INPUT = @"[RegisterScoped]
public class Foo : IFoo { }
public interface IFoo { }
";

        const string EXPECTED = @"// <auto-generated>
//     Automatically generated by AutoRegisterInject.
//     Changes made to this file may be lost and may cause undesirable behaviour.
// </auto-generated>
using Microsoft.Extensions.DependencyInjection;
public static class AutoRegisterInjectServiceCollectionExtension
{
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AutoRegisterFromTestProject(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
    {
        return AutoRegister(serviceCollection);
    }

    internal static Microsoft.Extensions.DependencyInjection.IServiceCollection AutoRegister(this Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IFoo, Foo>();
        return serviceCollection;
    }
}";

        await RunGenerator(INPUT, EXPECTED);
    }
}