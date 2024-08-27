using benchmark.Mocks;

using BenchmarkDotNet.Attributes;

namespace benchmark.CallingResult;
[MemoryDiagnoser]
public class CallingResultBenchmark : IDisposable
{
    private readonly HttpClient _httpClient = FunTranslationClient.GetHttpClient();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [Benchmark]
    public void CallingResult()
    {
        var result = _httpClient.GetAsync(new Uri("https://localhost")).Result;
    }
    [Benchmark]
    public async Task CallingAsync()
    {
        var result = await _httpClient.GetAsync(new Uri("https://localhost")).ConfigureAwait(false);
    }
    [Benchmark]
    public async Task CallingWithCancellationTokenAsync()
    {
        var result = await _httpClient.GetAsync(new Uri("https://localhost"), _cancellationTokenSource.Token).ConfigureAwait(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        _httpClient.Dispose();
        _cancellationTokenSource.Dispose();
    }
}