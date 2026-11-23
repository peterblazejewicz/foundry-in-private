using ContractGen;

// Generates the C# contract models from contracts/*.schema.json.
// Run from the repo root:
//   dotnet run --project screens/tools/ContractGen            write the models
//   dotnet run --project screens/tools/ContractGen -- --check exit 1 if a model is out of date
bool check = args.Contains("--check");
string root = Generator.FindRepoRoot(Directory.GetCurrentDirectory());

var stale = new List<string>();
foreach (string schemaPath in Generator.Schemas(root))
{
    string target = Generator.TargetPath(root, schemaPath);
    string text = await Generator.RenderAsync(schemaPath).ConfigureAwait(false);
    string? current = File.Exists(target) ? await File.ReadAllTextAsync(target).ConfigureAwait(false) : null;
    if (current == text)
    {
        continue;
    }

    if (check)
    {
        stale.Add(target);
    }
    else
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await File.WriteAllTextAsync(target, text).ConfigureAwait(false);
        Console.WriteLine($"wrote {Path.GetRelativePath(root, target).Replace('\\', '/')}");
    }
}

foreach (string target in stale)
{
    await Console.Error.WriteLineAsync($"out of date: {Path.GetRelativePath(root, target).Replace('\\', '/')}").ConfigureAwait(false);
}

return stale.Count == 0 ? 0 : 1;
