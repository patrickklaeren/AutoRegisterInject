namespace AutoRegisterInject;

internal readonly record struct AutoInterface(
    string Namespace,
    string InterfaceName,
    string Accessibility,
    string MembersSource,
    int AttributeStart);
