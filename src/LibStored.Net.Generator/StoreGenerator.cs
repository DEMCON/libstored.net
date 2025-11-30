using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using YamlDotNet.Core;

namespace LibStored.Net.Generator;

[Generator]
public class StoreGenerator : IIncrementalGenerator
{
    private record struct StoreMetadataFile(string Path, string? Source);
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<StoreMetadataFile>> stores = context.AdditionalTextsProvider
            .Where(text => text.Path.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)
                           || text.Path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            .Select((text, token) => new StoreMetadataFile(text.Path, text.GetText(token)?.ToString()))
            .Where(file => file.Source is not null)!
            .Collect();

        context.RegisterSourceOutput(stores, GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context, ImmutableArray<StoreMetadataFile> stores)
    {
        string version = typeof(StoreGenerator)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";

        foreach (StoreMetadataFile store in stores)
        {
            string fileName = Path.GetFileName(store.Path);

            StoreModel model;
            try
            {
                model = StoreYaml.Deserializer.Deserialize<StoreModel>(store.Source!);
            }
            catch (YamlException jex)
            {
                var error = Diagnostic.Create(StoreDiagnosticsDescriptors.DeserializationError, null, jex.Start.Line,
                    jex.Start.Column);
                context.ReportDiagnostic(error);
                continue;
            }

            try
            {
                StoreModelValidator.Validate(model);
            }
            catch (ArgumentException aex)
            {
                var error = Diagnostic.Create(StoreDiagnosticsDescriptors.ValidationError, null, store.Path, aex.Message);
                context.ReportDiagnostic(error);
                continue;
            }

            try
            {
                string source = ScribanGenerator.GenerateSource(model, fileName, version);
                context.AddSource($"{model.Name}.g.cs", source);
            }
            catch (Exception ex)
            {
                var error = Diagnostic.Create(StoreDiagnosticsDescriptors.GenerationError, null, store.Path, ex.Message);
                context.ReportDiagnostic(error);
            }
        }
    }
}
