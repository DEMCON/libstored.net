using System.Text;
using LibStored.Net.Generator.Models;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace LibStored.Net.Generator;

public static class ScribanGenerator
{
    private const string StoreTemplate =
        """
        #nullable enable

        namespace LibStored.Net;

        /// <summary>
        /// {{store.name}} generated from {{filename}}.
        /// </summary>
        public class {{store.name}} : global::LibStored.Net.Store, global::System.ComponentModel.INotifyPropertyChanged
        {
            private static readonly byte[] InitialBuffer = [
                {{ store.init }}
            ];

            private readonly byte[] _data = new byte[{{ store.size }}];

            private readonly global::System.Collections.Generic.Dictionary<string, global::LibStored.Net.DebugVariantInfo> _debugDirectory = [];

        {{~ for o in store.variables ~}}
        {{~ if o.is_variable ~}}
            private readonly global::LibStored.Net.StoreVariable<{{o.type|cs_types}}> {{o.cname|cs_field}};
        {{~ else ~}}
            private readonly global::LibStored.Net.StoreVariant<{{o.type|cs_types}}> {{o.cname|cs_field}};
        {{~ end ~}}
        {{~ end ~}}

            public {{store.name}}()
            {
                {{store.name}}.InitialBuffer.AsSpan().CopyTo(_data.AsSpan());

        {{~ for o in store.variables ~}}
        {{~ if o.is_variable ~}}
                {{o.cname|cs_field}} = new global::LibStored.Net.StoreVariable<{{o.type|cs_types}}>({{o.offset}}, {{o.size}}, this);
        {{~ else ~}}
                {{o.cname|cs_field}} = new global::LibStored.Net.StoreVariant<{{o.type|cs_types}}>({{o.offset}}, {{o.size}}, this);
        {{~ end ~}}
        {{~ end ~}}

        {{~ for o in store.variables ~}}
                _debugDirectory.Add("/{{o.name}}", new global::LibStored.Net.DebugVariantInfo({{o.type|cse_types}}, {{o.offset}}, {{o.size}}));
        {{~ end ~}}
            }

            public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

            public override global::System.Span<byte> GetBuffer() => _data;
            public override global::System.Collections.Generic.IReadOnlyDictionary<string, global::LibStored.Net.DebugVariantInfo> GetDebugVariants() => _debugDirectory;

            public override string Name => "/{{store.name}}";
            public override string Hash => "{{store.hash}}";
            public override int VariableCount => {{store.variables|array.size}};

        {{~ for o in store.variables ~}}
            /// <summary>
            /// {{o.name}}.
            /// </summary>
            public {{o.type|cs_types}} {{o.cname|cs_prop}}
            {
        {{~ if o.is_variable ~}}
                get => {{o.cname|cs_field}}.Get();
                set => {{o.cname|cs_field}}.Set(value);
        {{~ else ~}}
                get => StoreVariantExtensions.Get({{o.cname|cs_field}});
                set => StoreVariantExtensions.Set({{o.cname|cs_field}}, value);
        {{~ end ~}}
            }

        {{~ end ~}}
            /// <summary>
            /// Notify property changed for the specific variable that changed based on the offset.
            /// </summary>
            /// <param name="offset"></param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public override void Changed(int offset)
            {
                if (PropertyChanged is null)
                {
                    return;
                }

                global::System.ComponentModel.PropertyChangedEventArgs args = new(offset switch
                {
        {{~ for o in store.variables ~}}
                    {{o.offset}} => nameof({{store.name}}.{{o.cname|cs_prop}}),
        {{~ end ~}}
                    _ => throw new ArgumentOutOfRangeException(nameof(offset), offset, "Unknown offset")
                });

                PropertyChanged?.Invoke(this, args);
            }
        }

        """;

    public static string GenerateSource(StoreModel model, string filename)
    {
        Template template = Template.Parse(StoreTemplate);
        if (template.HasErrors)
        {
            StringBuilder sb = new();
            foreach (LogMessage parsedMessage in template.Messages)
            {
                sb.AppendLine(parsedMessage.Message);
            }

            throw new Exception(sb.ToString());
        }

        ScriptObject scriptObject = new();
        scriptObject["filename"] = filename;
        scriptObject.Import(typeof(TemplateFunctions));

        TemplateVariable[] variables = model.Variables
            .OrderBy(x => x.Offset)
            .Select(Map).ToArray();

        TemplateObject store = new()
        {
            Name = model.Name,
            Hash = model.Hash,
            Size = Size(variables),
            Init = Encode(variables),
            Variables = variables,
        };

        scriptObject["store"] = store;

        TemplateContext context = new();
        context.PushGlobal(scriptObject);

        string? result = template.Render(context);
        return result;
    }

    private static int Size(TemplateVariable[] variables) => variables
        .OrderBy(x => x.Offset)
        .Select(x => x.Offset + x.Size)
        .LastOrDefault();

    private static string Encode(TemplateVariable[] variables)
    {
        List<byte> bytes = new();
        foreach (TemplateVariable variable in variables)
        {
            if (variable.Value is null)
            {
                break;
            }

            int padding = variable.Offset - bytes.Count;
            if (padding > 0)
            {
                for (int i = 0; i < padding; i++)
                {
                    bytes.Add(0);
                }
            }

            bytes.AddRange(variable.Value);
        }

        return string.Join(", ", bytes.Select(b => $"0x{b:X2}"));
    }


    private static TemplateVariable Map(Variables x) => new()
    {
        Name = x.Name,
        Cname = x.Cname,
        Type = x.Type,
        Offset = x.Offset,
        Size = x.Size,
        Value = Decode(x.Init)
    };

    // Base64 or use HexString, although convert.FromHexString is not in netstandard2.0.
    private static byte[]? Decode(string? init) => init is null ? null : Convert.FromBase64String(init);
}
