# AutoRegisterInject

<img src="https://github.com/patrickklaeren/AutoRegisterInject/blob/main/Icon.png?raw=true" alt="Responsive Image" style="width:40%; height:auto;">

AutoRegisterInject, also referred to as ARI, is a C# source generator that will automatically create Microsoft.Extensions.DependencyInjection registrations for types marked with attributes.

This is a compile time alternative to reflection/assembly scanning for your injections or manually adding to the `ServiceCollection` every time a new type needs to be registered.

For example:

```cs
namespace MyProject;

[RegisterScoped]
public class Foo { }
```

will automatically generate an extension method called `AutoRegister()` for `IServiceProvider`, that registers `Foo`, as scoped.

```cs
internal IServiceCollection AutoRegister(this IServiceCollection serviceCollection)
{
    serviceCollection.AddScoped<Foo>();
    return serviceCollection;
}
```

In larger projects, dependency injection registration becomes tedious and in team situations can lead to merge conflicts which can be easily avoided.

AutoRegisterInject moves the responsibility of service registration to the owning type rather than external service collection configuration, giving control and oversight of the type that is going to be registered with the container.

## Installation

Install the [Nuget](https://www.nuget.org/packages/AutoRegisterInject) package, and start decorating classes with ARI attributes.

Use `dotnet add package AutoRegisterInject` or add a package reference manually:

```xml
<PackageReference Include="AutoRegisterInject" />
```

## Usage

Classes should be decorated with one of four attributes:
- `[RegisterScoped]`
- `[RegisterSingleton]`
- `[RegisterTransient]`
- `[RegisterHostedService]`

Variants for keyed and the service `Try` register pattern are also available:
- `[TryRegisterScoped]`
- `[TryRegisterSingleton]`
- `[TryRegisterTransient]`
- `[RegisterKeyedScoped]`
- `[RegisterKeyedSingleton]`
- `[RegisterKeyedTransient]`

Each keyed attribute has a `Try` counterpart.

Register a class:

```cs
[RegisterScoped]
class Foo;
```

and get the following output:

```cs
serviceCollection.AddScoped<Foo>();
```

Update the service collection by invoking:

```cs
var serviceCollection = new ServiceCollection();
serviceCollection.AutoRegister();
serviceCollection.BuildServiceProvider();
```

You can now inject `Foo` as a dependency and have this resolved as scoped.

Alternatively, you can register hosted services by:

```cs
[RegisterHostedService]
class Foo;
```

and get:

```cs
serviceCollection.AddHostedService<Foo>();
```

### Register as interface

Implement one or many interfaces on your target class:

```cs
[RegisterTransient]
class Bar : IBar;
```

and get the following output:

```cs
serviceCollection.AddTransient<IBar, Bar>();
```

**Important note:** AutoRegisterInject is opinionated and `Bar` will only be registered with its implemented interface. ARI will **not** register `Bar`. `Bar` will always need to be resolved from `IBar` in your code.

Implementing multiple interfaces will have the implementing type be registered for each distinct interface.

```cs
[RegisterTransient]
class Bar : IBar, IFoo, IBaz;
```

will output the following:

```cs
serviceCollection.AddTransient<IBar, Bar>();
serviceCollection.AddTransient<IFoo, Bar>();
serviceCollection.AddTransient<IBaz, Bar>();
```

**Important note:** AutoRegisterInject is opinionated and `Bar` will only be registered with its implemented interfaces. ARI will **not** register `Bar`. `Bar` will always need to be resolved from `IBar`, `IFoo` or `IBaz` in your code.

### Generate interfaces

ARI can also generate an interface for a type by decorating it with `[AutoInterface]` and declaring the interface on the class:

```cs
[AutoInterface]
[RegisterScoped]
public class Foo : IFoo
{
    public string Name { get; set; }
    public void Run() { }
}
```

will generate:

```cs
public interface IFoo
{
    string Name { get; set; }
    void Run();
}
```

and because `Foo` implements `IFoo`, ARI will register it as an interface registration:

```cs
serviceCollection.AddScoped<IFoo, Foo>();
```

This works with namespaced types and the standard registration attributes.

### Multiple assemblies

In addition to the `AutoRegister` extension method, every assembly that AutoRegisterInject is a part of, a `AutoRegisterFromAssemblyName` will be generated. This allows you to configure your service collection from one, main, executing assembly.

Given 3 assemblies, `MyProject.Main`, `MyProject.Services`, `MyProject.Data`, you can configure the `ServiceCollection` as such:

```cs
var serviceCollection = new ServiceCollection();
serviceCollection.AutoRegisterFromMyProjectMain();
serviceCollection.AutoRegisterFromMyProjectServices();
serviceCollection.AutoRegisterFromMyProjectData();
serviceCollection.BuildServiceProvider();
```

AutoRegisterInject will remove illegal characters from assembly names in order to generate legal C# method names. `,`, `.` and ` ` will be removed.

### Ignoring interfaces

By default ARI will register a type with all the interfaces it implements, however this excludes `System.IDisposable` and its `IAsyncDisposable` counterpart.

You can ignore interfaces by telling ARI to *explicitly* register with only declared interfaces in the given attributes:

```cs
public interface IFoo { }
public interface IBar { }
[RegisterScoped(typeof(IBar))]
public class Foo;
```

this will result in `Foo` ONLY being registered as `IBar` and the following output:

```cs
serviceCollection.AddTransient<IBar, Foo>();
```

`IFoo` will be ignored entirely.

Where you want to register as multiple interfaces, you can pass multiple types.

```cs
[RegisterScoped(typeof(IBar), typeof(IFoo))]
public class Foo;
```

This works for all applicable attributes.

## License

AutoRegisterInject is MIT licensed. Do with it what you please under the terms of MIT.
