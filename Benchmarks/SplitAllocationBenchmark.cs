using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using ZLinq;

namespace Open.Text.Benchmarks;

/// <summary>
/// Comprehensive benchmark comparing three allocation strategies:
/// 1. BCL String.Split() - allocates array + strings (baseline)
/// 2. SplitAsSegments() - returns IEnumerable&lt;StringSegment&gt; (minimal allocation)
/// 3. SplitAsSegmentsNoAlloc() - returns ValueEnumerable (zero allocation for enumeration)
/// </summary>
[Config(typeof(Config))]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class SplitAllocationBenchmark
{
	private class Config : ManualConfig
	{
		public Config()
		{
			AddDiagnoser(MemoryDiagnoser.Default);
			AddColumn(StatisticColumn.Mean);
			AddColumn(StatisticColumn.Median);
			AddColumn(RankColumn.Arabic);
			AddJob(Job.ShortRun.WithId("Allocation"));
		}
	}

	// Test data of varying sizes
	private const string SmallCsv = "apple,banana,cherry,date,elderberry";
	private const string MediumCsv = "apple,banana,cherry,date,elderberry,fig,grape,honeydew,kiwi,lemon,mango,nectarine,orange,papaya,quince";
	private static readonly string LargeCsv = string.Join(",", Enumerable.Range(1, 1000).Select(i => $"item{i}"));

	// =====================================================================
	// CATEGORY: Foreach Enumeration - Small String
	// Shows allocation difference for simple enumeration
	// =====================================================================

	[BenchmarkCategory("Small-Foreach"), Benchmark(Baseline = true, Description = "1. BCL Split (array + strings)")]
	public int Small_Foreach_BCL()
	{
		int count = 0;
		foreach (var s in SmallCsv.Split(','))
			count += s.Length;
		return count;
	}

	[BenchmarkCategory("Small-Foreach"), Benchmark(Description = "2. SplitAsSegments (IEnumerable)")]
	public int Small_Foreach_Segments()
	{
		int count = 0;
		foreach (var s in SmallCsv.SplitAsSegments(','))
			count += s.Length;
		return count;
	}

	[BenchmarkCategory("Small-Foreach"), Benchmark(Description = "3. SplitAsSegmentsNoAlloc (ValueEnum)")]
	public int Small_Foreach_NoAlloc()
	{
		int count = 0;
		foreach (var s in SmallCsv.SplitAsSegmentsNoAlloc(','))
			count += s.Length;
		return count;
	}

	// =====================================================================
	// CATEGORY: Foreach Enumeration - Large String (1000 items)
	// Shows scaling behavior with more items
	// =====================================================================

	[BenchmarkCategory("Large-Foreach"), Benchmark(Baseline = true, Description = "1. BCL Split (1000 items)")]
	public int Large_Foreach_BCL()
	{
		int count = 0;
		foreach (var s in LargeCsv.Split(','))
			count++;
		return count;
	}

	[BenchmarkCategory("Large-Foreach"), Benchmark(Description = "2. SplitAsSegments (1000 items)")]
	public int Large_Foreach_Segments()
	{
		int count = 0;
		foreach (var s in LargeCsv.SplitAsSegments(','))
			count++;
		return count;
	}

	[BenchmarkCategory("Large-Foreach"), Benchmark(Description = "3. SplitAsSegmentsNoAlloc (1000 items)")]
	public int Large_Foreach_NoAlloc()
	{
		int count = 0;
		foreach (var s in LargeCsv.SplitAsSegmentsNoAlloc(','))
			count++;
		return count;
	}

	// =====================================================================
	// CATEGORY: LINQ Operations (Where + Sum)
	// Shows allocation behavior with LINQ chains
	// =====================================================================

	[BenchmarkCategory("LINQ-Chain"), Benchmark(Baseline = true, Description = "1. BCL + System.Linq")]
	public int Linq_BCL()
		=> SmallCsv.Split(',')
			.Where(s => s.Length > 4)
			.Sum(s => s.Length);

	[BenchmarkCategory("LINQ-Chain"), Benchmark(Description = "2. SplitAsSegments + System.Linq")]
	public int Linq_Segments()
		=> SmallCsv.SplitAsSegments(',')
			.Where(s => s.Length > 4)
			.Sum(s => s.Length);

	[BenchmarkCategory("LINQ-Chain"), Benchmark(Description = "3. SplitAsSegmentsNoAlloc + ZLinq")]
	public int Linq_NoAlloc()
		=> SmallCsv.SplitAsSegmentsNoAlloc(',')
			.Where(s => s.Length > 4)
			.Select(s => s.Length)
			.Sum();

	// =====================================================================
	// CATEGORY: Count Operation
	// Shows allocation for simple aggregation
	// =====================================================================

	[BenchmarkCategory("Count"), Benchmark(Baseline = true, Description = "1. BCL Split + LINQ Count")]
#pragma warning disable CA1829, RCS1077
	public int Count_BCL() => SmallCsv.Split(',').Count();
#pragma warning restore CA1829, RCS1077

	[BenchmarkCategory("Count"), Benchmark(Description = "2. SplitAsSegments + LINQ Count")]
	public int Count_Segments() => SmallCsv.SplitAsSegments(',').Count();

	[BenchmarkCategory("Count"), Benchmark(Description = "3. SplitAsSegmentsNoAlloc + ZLinq Count")]
	public int Count_NoAlloc() => SmallCsv.SplitAsSegmentsNoAlloc(',').Count();

	// =====================================================================
	// CATEGORY: String Sequence Split
	// Shows allocation behavior for multi-char delimiter
	// =====================================================================

	[BenchmarkCategory("Seq-Split"), Benchmark(Baseline = true, Description = "1. BCL Split(string)")]
	public int Seq_BCL()
	{
		int count = 0;
		foreach (var s in MediumCsv.Split(","))
			count += s.Length;
		return count;
	}

	[BenchmarkCategory("Seq-Split"), Benchmark(Description = "2. SplitAsSegments(string)")]
	public int Seq_Segments()
	{
		int count = 0;
		foreach (var s in MediumCsv.SplitAsSegments(","))
			count += s.Length;
		return count;
	}

	[BenchmarkCategory("Seq-Split"), Benchmark(Description = "3. SplitAsSegmentsNoAlloc(string)")]
	public int Seq_NoAlloc()
	{
		int count = 0;
		foreach (var s in MediumCsv.SplitAsSegmentsNoAlloc(","))
			count += s.Length;
		return count;
	}
}
