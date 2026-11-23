using System.Globalization;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

namespace ContractGen;

/// <summary>Turns one contract schema into one C# file in <c>Screen.Core/Contracts</c>.</summary>
internal static class Generator
{
    private const string SchemaSuffix = ".schema.json";

    /// <summary>The namespace of every generated contract type.</summary>
    public const string Namespace = "Screen.Core.Contracts";

    /// <summary>Walks up from <paramref name="start"/> to the folder that holds <c>contracts/</c> and <c>screens/</c>.</summary>
    /// <exception cref="DirectoryNotFoundException">No such folder above <paramref name="start"/>.</exception>
    public static string FindRepoRoot(string start)
    {
        for (DirectoryInfo? dir = new(start); dir is not null; dir = dir.Parent)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "contracts")) && Directory.Exists(Path.Combine(dir.FullName, "screens")))
            {
                return dir.FullName;
            }
        }

        throw new DirectoryNotFoundException($"no repo root (contracts/ and screens/) above {start}");
    }

    /// <summary>The schema files, in a stable order.</summary>
    public static IReadOnlyList<string> Schemas(string root) =>
        [.. Directory.GetFiles(Path.Combine(root, "contracts"), "*" + SchemaSuffix).Order(StringComparer.Ordinal)];

    /// <summary>Maps <c>contracts/vision-inspect-result.schema.json</c> to <c>Screen.Core/Contracts/VisionInspectResult.cs</c>.</summary>
    public static string TargetPath(string root, string schemaPath)
    {
        string stem = Path.GetFileName(schemaPath)[..^SchemaSuffix.Length];
        return Path.Combine(root, "screens", "src", "Screen.Core", "Contracts", Pascal(stem, '-') + ".cs");
    }

    /// <summary>Returns the generated file text: LF line ends, no trailing spaces, one final newline.</summary>
    public static async Task<string> RenderAsync(string schemaPath)
    {
        JsonSchema schema = await JsonSchema.FromFileAsync(schemaPath).ConfigureAwait(false);
        var settings = new CSharpGeneratorSettings
        {
            Namespace = Namespace,
            JsonLibrary = CSharpJsonLibrary.SystemTextJson,
            ClassStyle = CSharpClassStyle.Record,
            GenerateNullableReferenceTypes = true,
            GenerateDataAnnotations = false,
            GenerateDefaultValues = false,
            ArrayType = "System.Collections.Generic.IReadOnlyList",
            ArrayInstanceType = "System.Collections.Generic.List",
            PropertyNameGenerator = new PascalPropertyNames(),
            TypeNameGenerator = new TitleTypeNames(),
        };
        string rootType = schema.Title ?? throw new InvalidOperationException($"{schemaPath} has no title");
        string code = new CSharpGenerator(schema, settings).GenerateFile(rootType);

        // Make the output independent of the host OS and clean for the whitespace hooks.
        IEnumerable<string> lines = code
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select(line => line.TrimEnd());
        return string.Join('\n', lines).Trim('\n') + "\n";
    }

    /// <summary><c>bbox_xyxy</c> → <c>BboxXyxy</c>.</summary>
    internal static string Pascal(string name, char separator) =>
        string.Concat(name
            .Split(separator, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0], CultureInfo.InvariantCulture) + word[1..]));

    /// <summary>Names a type by its schema <c>title</c>, so an array item is <c>Detection</c>, not <c>Detections</c>.</summary>
    private sealed class TitleTypeNames : DefaultTypeNameGenerator
    {
        protected override string Generate(JsonSchema schema, string? typeNameHint) =>
            string.IsNullOrEmpty(schema.ActualSchema.Title) ? base.Generate(schema, typeNameHint) : schema.ActualSchema.Title;
    }

    private sealed class PascalPropertyNames : IPropertyNameGenerator
    {
        public string Generate(JsonSchemaProperty property) => Pascal(property.Name, '_');
    }
}
