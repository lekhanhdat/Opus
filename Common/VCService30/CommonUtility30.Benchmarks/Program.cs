using AvePoint.Common.FilterEngine;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<PropertyGetterBenchmarks>();

[MemoryDiagnoser]
[ShortRunJob]
public class PropertyGetterBenchmarks
{
    private readonly CommonInfoBase uncheckedInfo = new() { Title = "assigned" };
    private readonly CommonInfoBase checkedInfo = new() { Title = "assigned" };
    private IDisposable? checkScope;

    [GlobalSetup]
    public void EnableChecking()
    {
        checkScope = checkedInfo.BeginPropertyCheck();
    }

    [GlobalCleanup]
    public void DisableChecking()
    {
        checkScope?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public string DisabledGetter()
    {
        return uncheckedInfo.Title;
    }

    [Benchmark]
    public string EnabledGetter()
    {
        return checkedInfo.Title;
    }
}