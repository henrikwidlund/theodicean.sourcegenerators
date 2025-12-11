namespace Theodicean.SourceGenerators;

/// <summary>
/// Add to enums to indicate that a JsonConverter for the enum should be generated.
/// </summary>
/// <typeparam name="TEnum">The enum to generate the converter for.</typeparam>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class EnumJsonConverterAttribute<TEnum> : Attribute
    where TEnum : Enum
{
    /// <summary>
    /// Indicates if the string representation is case-sensitive when deserializing it as an enum.
    /// </summary>
    public bool CaseSensitive { get; set; }

    /// <summary>
    /// Indicates if the value of <see cref="PropertyName"/> should be camel cased. Default is <see langword="true" />.
    /// </summary>
    public bool CamelCase { get; set; } = true;

    /// <summary>
    /// If set, this value will be used in messages when there are problems with validation and/or serialization/deserialization occurs.
    /// </summary>
    public string? PropertyName { get; set; }
}