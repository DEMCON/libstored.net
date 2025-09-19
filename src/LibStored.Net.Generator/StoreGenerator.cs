using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using System.Text.Json;

namespace LibStored.Net.Generator;

[Generator]
public class StoreGenerator : IIncrementalGenerator
{
    private record struct JsonFile(string Path, string? Source);
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<JsonFile>> stores = context.AdditionalTextsProvider
            .Where(text => text.Path.EndsWith("Store.json", StringComparison.OrdinalIgnoreCase))
            .Select((text, token) => new JsonFile(text.Path, text.GetText(token)?.ToString()))
            .Where(file => file.Source is not null)!
            .Collect();
            
        context.RegisterSourceOutput(stores, GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context, ImmutableArray<JsonFile> stores)
    {
        foreach (JsonFile store in stores)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            try
            {
                StoreModel? model = JsonSerializer.Deserialize<StoreModel>(store.Source!, options);
                if (model is null)
                {
                    continue;
                }

                string fileName = Path.GetFileName(store.Path);
                string source = ScribanGenerator.GenerateSource(model, fileName);
                context.AddSource($"{model.Name}.g.cs", source);
            }
            catch (JsonException jex)
            {
                var error = Diagnostic.Create(StoreDiagnosticsDescriptors.DeserializationError, null, jex.Path, jex.LineNumber, jex.BytePositionInLine);
                context.ReportDiagnostic(error);
            }
            catch (Exception ex)
            {
                var error = Diagnostic.Create(StoreDiagnosticsDescriptors.GenerationError, null, store.Path, ex.Message);
                context.ReportDiagnostic(error);
            }
        }
    }
}
