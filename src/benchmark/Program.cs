// See https://aka.ms/new-console-template for more information
using benchmark.CallingResult;

using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<CallingResultBenchmark>();