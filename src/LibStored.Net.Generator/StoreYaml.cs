// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LibStored.Net.Generator;

internal static class StoreYaml
{
    public static readonly ISerializer Serializer =
        new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

    public static readonly IDeserializer Deserializer =
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .WithTypeDiscriminatingNodeDeserializer(o =>
            {
                o.AddKeyValueTypeDiscriminator<Variables>("type", new Dictionary<string, Type>
                {
                    { "string", typeof(VariablesString) },
                    { "bool", typeof(Variables<bool>) },
                    { "int8", typeof(Variables<sbyte>) },
                    { "uint8", typeof(Variables<byte>) },
                    { "int16", typeof(Variables<short>) },
                    { "uint16", typeof(Variables<ushort>) },
                    { "int32", typeof(Variables<int>) },
                    { "uint32", typeof(Variables<uint>) },
                    { "int64", typeof(Variables<long>) },
                    { "uint64", typeof(Variables<ulong>) },
                    { "float", typeof(Variables<float>) },
                    { "double", typeof(Variables<double>) },
                });
            })
            .Build();
}
