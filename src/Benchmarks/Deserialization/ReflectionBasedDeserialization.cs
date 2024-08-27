using Benchmarks.Mocks;

using BenchmarkDotNet.Attributes;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace Benchmarks.Deserialization;
[ShortRunJob]
[MemoryDiagnoser]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public class ReflectionBasedDeserialization
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private MemoryStream? _memoryStream;
    private CancellationTokenSource? _cancellationTokenSource;
    [GlobalSetup]
    public void Setup()
    {
        _memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(FunTranslationClient.ReturnJson));
        _cancellationTokenSource = new();
    }
    // [IterationSetup]
    public void IterationSetup()
    {
        _memoryStream!.Seek(0, SeekOrigin.Begin);
    }
    [Benchmark]
    public async Task<TranslationAsClass?> DeserializeAsClassWithNewtonSoftAsync()
    {
        IterationSetup();
        using var streamReader = new StreamReader(_memoryStream!, leaveOpen: true);
        var strRepresentation = await streamReader.ReadToEndAsync(_cancellationTokenSource!.Token).ConfigureAwait(false);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<TranslationAsClass>(strRepresentation);
    }
    [Benchmark]
    public async Task<TranslationAsStruct?> DeserializeAsStructWithNewtonSoftAsync()
    {
        IterationSetup();
        using var streamReader = new StreamReader(_memoryStream!, leaveOpen: true);
        var strRepresentation = await streamReader.ReadToEndAsync(_cancellationTokenSource!.Token).ConfigureAwait(false);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<TranslationAsStruct>(strRepresentation);
    }
    [Benchmark]
    public async Task<TranslationAsClass?> DeserializeAsClassWithStringAndReflectionAsync()
    {
        IterationSetup();
        using var streamReader = new StreamReader(_memoryStream!, leaveOpen: true);
        var strRepresentation = await streamReader.ReadToEndAsync(_cancellationTokenSource!.Token).ConfigureAwait(false);
        return JsonSerializer.Deserialize<TranslationAsClass>(strRepresentation);
    }
    [Benchmark]
    public async Task<TranslationAsStruct?> DeserializeAsStructWithStringAndReflectionAsync()
    {
        IterationSetup();
        using var streamReader = new StreamReader(_memoryStream!, leaveOpen: true);
        var strRepresentation = await streamReader.ReadToEndAsync(_cancellationTokenSource!.Token).ConfigureAwait(false);
        return JsonSerializer.Deserialize<TranslationAsStruct>(strRepresentation);
    }
    [Benchmark]
    public async Task<TranslationAsClass?> DeserializeAsClassWithReflectionAsync()
    {
        IterationSetup();
        return await JsonSerializer.DeserializeAsync<TranslationAsClass>(_memoryStream!, cancellationToken: _cancellationTokenSource!.Token).ConfigureAwait(false);
    }
    [Benchmark]
    public async Task<TranslationAsStruct?> DeserializeAsStructWithReflectionAsync()
    {
        IterationSetup();
        return await JsonSerializer.DeserializeAsync<TranslationAsStruct>(_memoryStream!, cancellationToken: _cancellationTokenSource!.Token).ConfigureAwait(false);
    }
    private static readonly JsonTypeInfo<TranslationAsClass> AsClassTypeInfo = TranslationAsClassSerializerContext.Default.TranslationAsClass;
    [Benchmark]
    public async Task<TranslationAsClass?> DeserializeAsClassWithTypeInfoAsync()
    {
        IterationSetup();
        return await JsonSerializer.DeserializeAsync(_memoryStream!, AsClassTypeInfo, _cancellationTokenSource!.Token).ConfigureAwait(false);
    }
    private static readonly JsonTypeInfo<TranslationAsStruct> AsStructTypeInfo = TranslationAsStructSerializerContext.Default.TranslationAsStruct;
    [Benchmark]
    public async Task<TranslationAsStruct?> DeserializeAsStructWithTypeInfoAsync()
    {
        IterationSetup();
        return await JsonSerializer.DeserializeAsync(_memoryStream!, AsStructTypeInfo, _cancellationTokenSource!.Token).ConfigureAwait(false);
    }
    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _memoryStream!.Dispose();
        _cancellationTokenSource!.Dispose();
    }
}