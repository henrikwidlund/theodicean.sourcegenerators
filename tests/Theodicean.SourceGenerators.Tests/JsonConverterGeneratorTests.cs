using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using VerifyTUnit;

namespace Theodicean.SourceGenerators.Tests;

public class JsonConverterGeneratorTests
{
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Attribute_Usage_Generates_Proper_Converter(bool separateConverterNameSpace)
    {
        var input = $$"""
                             using System.Text.Json.Serialization;
                             using Theodicean.SourceGenerators;

                             namespace MyTestNameSpace
                             {
                                 [EnumJsonConverterAttribute<MyEnum>(CamelCase = false, CaseSensitive = false)]
                                 [JsonConverter(typeof({{(separateConverterNameSpace ? "MyConverterNameSpace." : "")}}MyEnumConverter))]
                                 internal enum MyEnum
                                 {
                                     First = 0,
                                     Second = 1
                                 }
                             }
                             
                             namespace MyConverterNameSpace
                             {
                                 public partial class MyEnumConverter;
                             }
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<JsonConverterGenerator>(input);

        await Assert.That(diagnostics).IsEmpty();
        await Verifier.Verify(output).UseTextForParameters(separateConverterNameSpace.ToString()).UseDirectory("Snapshots");
    }
}

internal static class TestHelpers
{
    public static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput<T>(string source)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(static assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat([
                MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(EnumJsonConverterAttribute<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Text.Json.Serialization.JsonConverterAttribute).Assembly.Location)
            ]);

        var compilation = CSharpCompilation.Create(
            "generator",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation,
            out var outputCompilation,
            out var diagnostics);

        var trees = outputCompilation.SyntaxTrees.ToList();

        return (diagnostics, trees.Count != originalTreeCount ? trees[^1].ToString() : string.Empty);
    }
}