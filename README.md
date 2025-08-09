## Theodicean.SourceGenerators

Source generator for fast, allocation-light System.Text.Json converters for enums, driven by attributes.

Enables serialization/deserialization of arbitrary string tokens for enums (including tokens that are not valid C# identifiers, e.g., values with spaces, hyphens, or starting digits) while avoiding runtime reflection for performance and NativeAOT friendliness.

### Why

- **Non-identifier tokens**: Many wire formats use names that aren’t valid C# enum identifiers (e.g., "not-found", "401", "connection refused"). Use `Display(Name = "...")` or `Description("...")` to map those tokens to legal enum members.
- **Reflection-free and AOT-friendly**: The converter is fully source-generated. No runtime reflection or metadata scanning, making it safe for trimming and NativeAOT.
- **Performance**: Uses `Utf8JsonReader.CopyString`, `stackalloc`, and `ArrayPool<char>` with ordinal comparisons for low-allocation, fast parsing.

### Install

- **Package (project using the generator):**
  - Add a PackageReference to `Theodicean.SourceGenerators`
  - Example `csproj` snippet:

    ```xml
    <ItemGroup>
      <PackageReference Include="Theodicean.SourceGenerators" Version="x.y.z" PrivateAssets="all" ExcludeAssets="runtime" />
    </ItemGroup>
    ```

- The generated code uses features that require dotnet 9 or later.

- **Attributes:** Follows the same logic as [NetEscapades.EnumGenerators](https://github.com/andrewlock/NetEscapades.EnumGenerators?tab=readme-ov-file#embedding-the-attributes-in-your-project) - just replace `NETESCAPADES_ENUMGENERATORS_EMBED_ATTRIBUTES` with `THEODICEAN_GENERATORS_EMBED_ATTRIBUTES`.

Example:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <!--  Define the MSBuild constant    -->
    <DefineConstants>$(DefineConstants);THEODICEAN_GENERATORS_EMBED_ATTRIBUTES</DefineConstants>
  </PropertyGroup>

</Project>
```

### What it generates

Annotate a partial class with `EnumJsonConverterAttribute`, pointing to the enum you want to serialize/deserialize. The generator emits a sealed `JsonConverter<TEnum>` that:

- **Read:** Parses a JSON string into the enum value using a high-performance `ReadOnlySpan<char>` path and optional case sensitivity.
- **Write:** Emits the chosen string representation for each enum value.
- **Names:** Uses the enum field name by default, or the `Display(Name = "...")`/`Description("...")` value if present (both the attribute value and the enum member name will be matched).
- **Errors:** Throws `JsonException` on invalid input. If `PropertyName` is set, it is included in the exception for better diagnostics.
- **Reflection-free**: No runtime reflection; code is generated at build time.
- **NativeAOT/Trimming friendly**: Works under trimming and NativeAOT scenarios.

### Usage

1) Define your enum. You can optionally decorate members with `Display(Name = "...")` (or `Description("...")`) to control the serialized string.

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public enum ExampleEnum
{
    None = 1,
    [Display(Name = "NOT_FOUND")] NotFound,
    [Display(Name = "CONNECTION_REFUSED")] ConnectionRefused
}
```

2) Declare a partial converter class and annotate it with `EnumJsonConverter`:

```csharp
using Theodicean.SourceGenerators;

[EnumJsonConverter(typeof(ExampleEnum), CaseSensitive = false, PropertyName = "error")]
public partial class ExampleEnumJsonConverter;
```

3) Annotate your enum so that the converter is used.

```csharp
[JsonConverter(typeof(ExampleEnumJsonConverter))]
public enum ExampleEnum
{
    None = 1,
    [Display(Name = "NOT_FOUND")] NotFound,
    [Display(Name = "CONNECTION_REFUSED")] ConnectionRefused
}
```

Or add it as a converter in your `JsonSerializerOptions`

```csharp
new JsonSerializerOptions { Converters = { new ExampleEnumJsonConverter() } }
```

NOTE: If you've added the `JsonNumberEnumConverter` to your `JsonSerializerOptions`, the custom converter must be added before `JsonNumberEnumConverter` or the custom converter will not be called.`


### Attribute options

- **EnumJsonConverter(Type enumType)**: Required. The enum type to generate a converter for.
- **CaseSensitive (bool)**: Default `false`. If `true`, only exact (ordinal) case matches are accepted when reading JSON.
- **CamelCase (bool)**: Default `false`. If `PropertyName` is supplied and `CamelCase = true`, the property name included in thrown `JsonException` is camel-cased.
- **PropertyName (string?)**: If set, included in `JsonException` to identify the logical field name when invalid input is encountered.

### Display/Description name mapping

- If an enum member has `[Display(Name = "VALUE")]`, that string is used for both serialization and deserialization.
- If no `Display` is present but `[Description("VALUE")]` exists, the description string is used.
- If neither is present, the enum member name is used.
- When `CaseSensitive = false` (default), comparisons use `StringComparison.OrdinalIgnoreCase`.
- Deserialization of default enum member name will always work, regardless of `Display`/`Description` attributes.

### Example of serialized values

Given the `ExampleEnum` enum above, JSON will contain strings like `"NONE"`, `"NOT_FOUND"`, etc. If a value is not recognized, a `JsonException` is thrown. When `PropertyName = "error"`, the exception includes that property name for better error messages.

### Limitations

- **.NET version**: Requires .NET 9 or later to use the generated converters.
- **System.Text.Json only**: Generates converters specifically for `System.Text.Json`.
- **Enums only**: This generator is for enums. Other types are not supported.
- **Public vs internal**: The emitted converter matches the visibility of your partial class (`public` or `internal`). Ensure accessibility aligns with your usage.
- **String tokens only**: The converter expects JSON string tokens for enum values. Numeric tokens aren’t supported by this converter.
- **Name collisions**: If multiple enum members map to the same string (via `Display`/`Description` or names), deserialization picks the first match by declaration order; serialization will produce the shared string.
