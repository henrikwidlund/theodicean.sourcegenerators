namespace Theodicean.SourceGenerators;

public readonly record struct JsonConverterToGenerate(
    string ConverterType,
    string? ConverterNamespace,
    in bool IsPublic,
    string FullyQualifiedEnumName,
    in bool CaseSensitive,
    in bool CamelCase,
    string? PropertyName,
    List<(string EnumMember, EnumValueOption EnumValueOption)> Members);
