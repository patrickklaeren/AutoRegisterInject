namespace AutoRegisterInject;

internal readonly record struct AutoRegisteredClass(
    string ClassName,
    AutoRegistrationType RegistrationType,
    string InterfaceName,
    string ServiceKey,
    int ClassDeclarationStart,
    int AttributeStart);
