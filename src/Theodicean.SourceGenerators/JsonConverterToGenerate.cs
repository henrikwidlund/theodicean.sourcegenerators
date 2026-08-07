namespace Theodicean.SourceGenerators;

public readonly record struct JsonConverterToGenerate(
    string ConverterType,
    string? ConverterNamespace,
    in bool IsPublic,
    string FullyQualifiedEnumName,
    in bool CaseSensitive,
    in bool CamelCase,
    string? PropertyName,
    List<(string EnumMember, EnumValueOption EnumValueOption)> Members)
{
    public bool Equals(JsonConverterToGenerate other) =>
        string.Equals(ConverterType, other.ConverterType, StringComparison.Ordinal) &&
        string.Equals(ConverterNamespace, other.ConverterNamespace, StringComparison.Ordinal) &&
        IsPublic == other.IsPublic &&
        string.Equals(FullyQualifiedEnumName, other.FullyQualifiedEnumName, StringComparison.Ordinal) &&
        CaseSensitive == other.CaseSensitive &&
        CamelCase == other.CamelCase &&
        string.Equals(PropertyName, other.PropertyName, StringComparison.Ordinal) &&
        Members.SequenceEqual(other.Members);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(ConverterType);
            hash = hash * 31 + (ConverterNamespace is null ? 0 : StringComparer.Ordinal.GetHashCode(ConverterNamespace));
            hash = hash * 31 + IsPublic.GetHashCode();
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(FullyQualifiedEnumName);
            hash = hash * 31 + CaseSensitive.GetHashCode();
            hash = hash * 31 + CamelCase.GetHashCode();
            hash = hash * 31 + (PropertyName is null ? 0 : StringComparer.Ordinal.GetHashCode(PropertyName));
            foreach (var member in Members)
                hash = hash * 31 + (StringComparer.Ordinal.GetHashCode(member.EnumMember) * 31 + member.EnumValueOption.GetHashCode());

            return hash;
        }
    }
}
