using BenchmarkDotNet.Attributes;
using Microsoft.IO;

namespace Benchmarks.Deserialization;
[ShortRunJob]
[MemoryDiagnoser]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public class MemoryStreamCopyImageBenchmark
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private MemoryStream? _memoryStream;
    private CancellationTokenSource? _cancellationTokenSource;
    private static readonly RecyclableMemoryStreamManager.Options options = new RecyclableMemoryStreamManager.Options
    {
        BlockSize = 1024,
    };
    private static readonly RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager(options);
    [GlobalSetup]
    public void Setup()
    {
        var baseDirectory = System.IO.Directory.GetCurrentDirectory();
        using (var source = File.Open($"{baseDirectory}\\MemoryStreamCopy\\bear.jpg", FileMode.Open))
        {
            _memoryStream = new MemoryStream();
            source.CopyTo(_memoryStream);
        }
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
        for (var i = 0; i < 100; i++) {
            IterationSetup();
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
        for (var i = 0; i < 100; i++)
        {
            IterationSetup();
            using (var memoryStream = manager.GetStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
            }
        }
        return;
    }

    [Benchmark]
    public async Task ThousandMemoryCopyAsync()
    {              
        for (var i = 0; i < 1000; i++)
        {
            IterationSetup();
            using (var memoryStream = new MemoryStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
            }
        }
        return;
    }

    [Benchmark]
    public async Task ThousandRecycableMemoryCopyAsync()
    {        
        for (var i = 0; i < 1000; i++)
        {
            IterationSetup();
            using (var memoryStream = manager.GetStream()) { 
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
            }
        }
        return;
    }

    [Benchmark]
    public async Task<List<MemoryStream>> HundredMemoryListCopyAsync()
    {        
        var memoryStreamList = new List<MemoryStream>();
        for (var i = 0; i < 100; i++)
        {
            IterationSetup();
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
        var memoryStreamList = new List<MemoryStream>();
        for (var i = 0; i < 100; i++)
        {
            IterationSetup();
            using (var memoryStream = manager.GetStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStreamList.Add(memoryStream);
            }
        }
        return memoryStreamList;
    }

    [Benchmark]
    public async Task<List<MemoryStream>> ThousandMemoryListCopyAsync()
    {        
        var memoryStreamList = new List<MemoryStream>();
        for (var i = 0; i < 1000; i++)
        {
            IterationSetup();
            using (var memoryStream = new MemoryStream())
            {
                await _memoryStream!.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStreamList.Add(memoryStream);
            }
        }
        return memoryStreamList;
    }  

    [Benchmark]
    public async Task<List<MemoryStream>> ThousandRecycableMemoryListCopyAsync()
    {        
        var memoryStreamList = new List<MemoryStream>();
        for (var i = 0; i < 1000; i++)
        {
            IterationSetup();
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