using Microsoft.CodeAnalysis;

namespace LibStored.Net.Generator;

public static class StoreDiagnosticsDescriptors
{
    public static readonly DiagnosticDescriptor DeserializationError
        = new("STRO001",
            "Store could not be deserialized",
            "Store could not be deserialized: Path: {0} | LineNumber: {1} | BytePositionInLine: {2}",
            "Deserialization",
            DiagnosticSeverity.Error,
            true);
    
    public static readonly DiagnosticDescriptor GenerationError
        = new("STRO002",
            "Store could not be generated",
            "Store could not be deserialized: File: {0} | Message: {1}",
            "Generator",
            DiagnosticSeverity.Error,
            true);
}