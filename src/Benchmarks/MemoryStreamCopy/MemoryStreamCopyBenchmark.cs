using System.Text;

using BenchmarkDotNet.Attributes;

using Benchmarks.Mocks;

using Microsoft.IO;

namespace Benchmarks.Deserialization;
[ShortRunJob]
[MemoryDiagnoser]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public class MemoryStreamCopyBenchmark
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private MemoryStream? _memoryStream;
    private CancellationTokenSource? _cancellationTokenSource;
    private static RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager();
    [GlobalSetup]
    public void Setup()
    {
        _memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(FunTranslationClient.ReturnJson));
        _cancellationTokenSource = new();
    }
    
    public void IterationSetup()
    {
        _memoryStream!.Seek(0, SeekOrigin.Begin);
    }

    [Benchmark]
    public async Task<MemoryStream> SingleMemoryCopyAsync()
    {
        IterationSetup();
        using var memoryStream = new MemoryStream();
        await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
        return memoryStream;
    }


    [Benchmark]
    public async Task<MemoryStream> SingleRecycableMemoryCopyAsync()
    {
        IterationSetup();
        using var memoryStream = manager.GetStream();
        await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
        return memoryStream;
    }

    [Benchmark]
    public async Task HundredMemoryCopyAsync()
    {
        IterationSetup();        
        for (var i = 0; i < 100; i++) {
            using (var memoryStream = new MemoryStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
            }
        }
        return;
    }


    [Benchmark]
    public async Task HundredRecycableMemoryCopyAsync()
    {
        IterationSetup();        
        for (var i = 0; i < 100; i++)
        {
            using (var memoryStream = manager.GetStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
            }
        }
        return;
    }

    [Benchmark]
    public async Task MillionMemoryCopyAsync()
    {
        IterationSetup();        
        for (var i = 0; i < 1000000; i++)
        {
            using (var memoryStream = new MemoryStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
            }
        }
        return;
    }


    [Benchmark]
    public async Task MillionRecycableMemoryCopyAsync()
    {
        IterationSetup();
        for (var i = 0; i < 1000000; i++)
        {
            using (var memoryStream = manager.GetStream()) { 
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
            }
        }
        return;
    }

    [Benchmark]
    public async Task<List<MemoryStream>> HundredMemoryListCopyAsync()
    {
        IterationSetup();
        var memoryStreamList = new List<MemoryStream>();
        for (var i = 0; i < 100; i++)
        {
            using (var memoryStream = new MemoryStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStreamList.Add(memoryStream);
            }
        }
        return memoryStreamList;
    }

    [Benchmark]
    public async Task<List<MemoryStream>> HundredRecycableMemoryListCopyAsync()
    {
        IterationSetup();
        var memoryStreamList = new List<MemoryStream>();
        for (var i = 0; i < 100; i++)
        {
            using (var memoryStream = manager.GetStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStreamList.Add(memoryStream);
            }
        }
        return memoryStreamList;
    }

    [Benchmark]
    public async Task<List<MemoryStream>> MillionMemoryListCopyAsync()
    {
        IterationSetup();
        var memoryStreamList = new List<MemoryStream>();
        for (var i = 0; i < 1000000; i++)
        {
            using (var memoryStream = new MemoryStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStreamList.Add(memoryStream);
            }
        }
        return memoryStreamList;
    }  

    [Benchmark]
    public async Task<List<MemoryStream>> MillionRecycableMemoryListCopyAsync()
    {
        IterationSetup();
        var memoryStreamList = new List<MemoryStream>();
        for (var i = 0; i < 1000000; i++)
        {
            using (var memoryStream = manager.GetStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStreamList.Add(memoryStream);
            }
        }
        return memoryStreamList;
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _memoryStream!.Dispose();
        _cancellationTokenSource!.Dispose();
    }
}