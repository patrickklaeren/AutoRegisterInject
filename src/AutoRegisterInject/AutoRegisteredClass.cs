namespace AutoRegisterInject;

internal record AutoRegisteredClass(string ClassName, AutoRegistrationType RegistrationType, string[] Interfaces, string? Key);
