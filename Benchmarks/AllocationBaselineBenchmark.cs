using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using ZLinq;

namespace Open.Text.Benchmarks;

/// <summary>
/// Focused benchmark for SplitAsSegments to establish baseline allocation behavior.
/// Goal: Prove current implementation allocates and measure by how much.
/// </summary>
[Config(typeof(Config))]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SplitAsSegmentsBaselineBenchmark
{
	private class Config : ManualConfig
	{
		public Config()
		{
			AddDiagnoser(MemoryDiagnoser.Default);
			AddColumn(StatisticColumn.AllStatistics);
			AddJob(Job.Default.WithId("Baseline"));
		}
	}

	private const string SmallString = "apple,banana,cherry,date";
	private const string MediumString = "apple,banana,cherry,date,elderberry,fig,grape,honeydew,kiwi,lemon";
	private readonly string _largeString = string.Join(",", Enumerable.Range(1, 1000));

	// ========== Small String Tests ==========

	[Benchmark(Description = "Small - Foreach Only (no ToString)")]
	public int Small_ForeachOnly()
	{
		int count = 0;
		foreach (var segment in SmallString.AsSegment().SplitAsSegments(','))
		{
			count += segment.Length;
		}
		return count;
	}

	[Benchmark(Description = "Small - Foreach with ToString")]
	public int Small_ForeachWithToString()
	{
		int totalLen = 0;
		foreach (var segment in SmallString.AsSegment().SplitAsSegments(','))
		{
			var str = segment.ToString();
			totalLen += str.Length;
		}
		return totalLen;
	}

	[Benchmark(Description = "Small - ToArray")]
	public int Small_ToArray()
	{
		var arr = SmallString.AsSegment().SplitAsSegments(',').ToArray();
		return arr.Length;
	}

	[Benchmark(Description = "Small - Count()")]
	public int Small_Count() => SmallString.AsSegment().SplitAsSegments(',').Count();

	// ========== Medium String Tests ==========

	[Benchmark(Description = "Medium - Foreach Only")]
	public int Medium_ForeachOnly()
	{
		int count = 0;
		foreach (var segment in MediumString.AsSegment().SplitAsSegments(','))
		{
			count += segment.Length;
		}
		return count;
	}

	[Benchmark(Description = "Medium - LINQ Where")]
	public int Medium_LinqWhere()
		=> MediumString.AsSegment()
			.SplitAsSegments(',')
			.Where(s => s.Length > 5)
			.Count();

	[Benchmark(Description = "Medium - LINQ Select")]
	public int Medium_LinqSelect()
		=> MediumString.AsSegment()
			.SplitAsSegments(',')
			.Select(s => s.Length)
			.Sum();

	// ========== Large String Tests ==========

	[Benchmark(Description = "Large (1000) - Foreach Only")]
	public int Large_ForeachOnly()
	{
		int count = 0;
		foreach (var segment in _largeString.AsSegment().SplitAsSegments(','))
		{
			count++;
		}
		return count;
	}

	[Benchmark(Description = "Large (1000) - ToList")]
	public int Large_ToList()
	{
		var list = _largeString.AsSegment().SplitAsSegments(',').ToList();
		return list.Count;
	}

	// ========== BCL Comparison (Baseline) ==========

	[Benchmark(Baseline = true, Description = "BCL String.Split (Reference)")]
	public int BclStringSplit()
	{
		var arr = SmallString.Split(',');
		return arr.Length;
	}

	[Benchmark(Description = "BCL Large String.Split")]
	public int BclStringSplit_Large()
	{
		var arr = _largeString.Split(',');
		return arr.Length;
	}
}
