using System.Text.Json.Serialization;

namespace Benchmarks.Deserialization;

public class TranslationAsClass
{
    [JsonPropertyName("success")]
    public SuccessAsClass Success { get; set; } = new();
    [JsonPropertyName("contents")]
    public ContentsAsClass Contents { get; set; } = new();
}

public class SuccessAsClass
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}
public class ContentsAsClass
{
    [JsonPropertyName("translated")]
    public string Translated { get; set; } = string.Empty;
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    [JsonPropertyName("translation")]
    public string Translation { get; set; } = string.Empty;
}
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(TranslationAsClass))]
internal sealed partial class TranslationAsClassSerializerContext : JsonSerializerContext
{

}
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(TranslationAsStruct))]
internal sealed partial class TranslationAsStructSerializerContext : JsonSerializerContext
{

}
#pragma warning disable CA1815 // Override equals and operator equals on value types
public struct TranslationAsStruct
#pragma warning restore CA1815 // Override equals and operator equals on value types
{
    [JsonPropertyName("success")]
    public SuccessAsStruct Success { get; set; }
    [JsonPropertyName("contents")]
    public ContentsAsStruct Contents { get; set; }
}
#pragma warning disable CA1815 // Override equals and operator equals on value types
public struct SuccessAsStruct
#pragma warning restore CA1815 // Override equals and operator equals on value types
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}
#pragma warning disable CA1815 // Override equals and operator equals on value types
public struct ContentsAsStruct
#pragma warning restore CA1815 // Override equals and operator equals on value types
{
    [JsonPropertyName("translated")]
    public string Translated { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("translation")]
    public string Translation { get; set; }
}