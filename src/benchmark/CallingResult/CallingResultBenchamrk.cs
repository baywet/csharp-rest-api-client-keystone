using benchmark.Mocks;

using BenchmarkDotNet.Attributes;

namespace benchmark.CallingResult;
[ShortRunJob]
[MemoryDiagnoser]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public class CallingResultBenchmark
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private HttpClient? _httpClient;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly Uri _targetUri = new("https://localhost");
    [GlobalSetup]
    public void Setup()
    {
        _httpClient = FunTranslationClient.GetHttpClient();
        _cancellationTokenSource = new();
    }

    [Benchmark]
    public HttpResponseMessage CallingResult()
    {
        return _httpClient!.GetAsync(_targetUri).Result;
    }
    [Benchmark]
    public HttpResponseMessage CallingGetResult()
    {
        return _httpClient!.GetAsync(_targetUri).GetAwaiter().GetResult();
    }
    [Benchmark]
    public async Task<HttpResponseMessage> CallingAsync()
    {
        return await _httpClient!.GetAsync(_targetUri).ConfigureAwait(false);
    }
    [Benchmark]
    public async Task<HttpResponseMessage> CallingWithCancellationTokenAsync()
    {
        return await _httpClient!.GetAsync(_targetUri, _cancellationTokenSource!.Token).ConfigureAwait(false);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _httpClient!.Dispose();
        _cancellationTokenSource!.Dispose();
    }
}