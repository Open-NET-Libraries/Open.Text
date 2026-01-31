using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System.Text.RegularExpressions;
using ZLinq;

namespace Open.Text.Benchmarks;

/// <summary>
/// Comprehensive benchmarks measuring the ZLinq integration improvements.
/// Compares allocation behavior across different operations.
/// </summary>
[Config(typeof(Config))]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ZLinqImprovementsBenchmark
{
	private class Config : ManualConfig
	{
		public Config()
		{
			AddDiagnoser(MemoryDiagnoser.Default);
			AddColumn(StatisticColumn.Mean);
			AddColumn(StatisticColumn.Median);
			AddColumn(RankColumn.Arabic);
			AddJob(Job.ShortRun.WithId("ZLinq"));
		}
	}

	// Test data
	private const string SmallCsv = "apple,banana,cherry,date,elderberry";
	private const string MediumCsv = "apple,banana,cherry,date,elderberry,fig,grape,honeydew,kiwi,lemon,mango,nectarine,orange,papaya,quince";
	private static readonly string LargeCsv = string.Join(",", Enumerable.Range(1, 1000).Select(i => $"item{i}"));

	private static readonly Regex CommaRegex = new(",", RegexOptions.Compiled);
	private static readonly Regex WordRegex = new(@"\w+", RegexOptions.Compiled);

	// =====================================================================
	// CATEGORY: Char Split - Foreach Only (Pure enumeration, no materialization)
	// =====================================================================

	[BenchmarkCategory("CharSplit-Foreach"), Benchmark(Baseline = true, Description = "BCL String.Split")]
	public int CharSplit_Foreach_BCL()
	{
		int count = 0;
		foreach (var s in SmallCsv.Split(','))
			count += s.Length;
		return count;
	}

	[BenchmarkCategory("CharSplit-Foreach"), Benchmark(Description = "SplitAsSegments (ZLinq)")]
	public int CharSplit_Foreach_ZLinq()
	{
		int count = 0;
		foreach (var s in SmallCsv.SplitAsSegments(','))
			count += s.Length;
		return count;
	}

	// =====================================================================
	// CATEGORY: Char Split - With LINQ Count()
	// =====================================================================

	[BenchmarkCategory("CharSplit-Count"), Benchmark(Baseline = true, Description = "BCL Split + LINQ Count")]
	public int CharSplit_Count_BCL()
#pragma warning disable CA1829 // Use Length/Count property instead of Count() when available
#pragma warning disable RCS1077 // Optimize LINQ method call
		=> SmallCsv.Split(',').Count();
#pragma warning restore RCS1077 // Optimize LINQ method call
#pragma warning restore CA1829 // Use Length/Count property instead of Count() when available

	[BenchmarkCategory("CharSplit-Count"), Benchmark(Description = "SplitAsSegments + ZLinq Count")]
	public int CharSplit_Count_ZLinq()
		=> SmallCsv.SplitAsSegments(',').Count();

	// =====================================================================
	// CATEGORY: Char Split - Large String Foreach
	// =====================================================================

	[BenchmarkCategory("CharSplit-Large"), Benchmark(Baseline = true, Description = "BCL Split (1000 items)")]
	public int CharSplit_Large_BCL()
	{
		int count = 0;
		foreach (var s in LargeCsv.Split(','))
			count++;
		return count;
	}

	[BenchmarkCategory("CharSplit-Large"), Benchmark(Description = "SplitAsSegments (1000 items)")]
	public int CharSplit_Large_ZLinq()
	{
		int count = 0;
		foreach (var s in LargeCsv.SplitAsSegments(','))
			count++;
		return count;
	}

	// =====================================================================
	// CATEGORY: String Sequence Split
	// =====================================================================

	[BenchmarkCategory("SeqSplit"), Benchmark(Baseline = true, Description = "BCL Split(string)")]
	public int SeqSplit_BCL()
	{
		int count = 0;
		foreach (var s in MediumCsv.Split(","))
			count += s.Length;
		return count;
	}

	[BenchmarkCategory("SeqSplit"), Benchmark(Description = "SplitAsSegments(string)")]
	public int SeqSplit_ZLinq()
	{
		int count = 0;
		foreach (var s in MediumCsv.SplitAsSegments(","))
			count += s.Length;
		return count;
	}

	// =====================================================================
	// CATEGORY: Regex Split
	// =====================================================================

	[BenchmarkCategory("RegexSplit"), Benchmark(Baseline = true, Description = "Regex.Split")]
	public int RegexSplit_BCL()
	{
		int count = 0;
		foreach (var s in CommaRegex.Split(MediumCsv))
			count += s.Length;
		return count;
	}

	[BenchmarkCategory("RegexSplit"), Benchmark(Description = "SplitAsSegments(Regex)")]
	public int RegexSplit_ZLinq()
	{
		int count = 0;
		foreach (var s in MediumCsv.SplitAsSegments(CommaRegex))
			count += s.Length;
		return count;
	}

	// =====================================================================
	// CATEGORY: Regex Match Enumeration
	// =====================================================================

	[BenchmarkCategory("RegexMatch"), Benchmark(Baseline = true, Description = "Regex.Matches")]
	public int RegexMatch_BCL()
	{
		int count = 0;
		foreach (Match m in WordRegex.Matches(MediumCsv))
			count += m.Length;
		return count;
	}

	[BenchmarkCategory("RegexMatch"), Benchmark(Description = "AsSegments(Regex)")]
	public int RegexMatch_ZLinq()
	{
		int count = 0;
		foreach (var s in WordRegex.AsSegments(MediumCsv))
			count += s.Length;
		return count;
	}

	// =====================================================================
	// CATEGORY: Replace Operation
	// =====================================================================

	[BenchmarkCategory("Replace"), Benchmark(Baseline = true, Description = "BCL String.Replace")]
	public string Replace_BCL() => MediumCsv.Replace(",", " | ");

	[BenchmarkCategory("Replace"), Benchmark(Description = "ReplaceToString (ZLinq)")]
	public string Replace_ZLinq() => MediumCsv.AsSegment().ReplaceToString(",", " | ");

	// =====================================================================
	// CATEGORY: LINQ Chain - Where + Select + Count
	// =====================================================================

	[BenchmarkCategory("LinqChain"), Benchmark(Baseline = true, Description = "BCL + LINQ Chain")]
	public int LinqChain_BCL()
		=> SmallCsv.Split(',')
			.Where(s => s.Length > 4)
			.Sum(s => s.Length);

	[BenchmarkCategory("LinqChain"), Benchmark(Description = "ZLinq Chain")]
	public int LinqChain_ZLinq()
		=> SmallCsv.SplitAsSegments(',')
			.Where(s => s.Length > 4)
			.Select(s => s.Length)
			.Sum();

	// =====================================================================
	// CATEGORY: ToArray Materialization
	// =====================================================================

	[BenchmarkCategory("ToArray"), Benchmark(Baseline = true, Description = "BCL Split (already array)")]
	public int ToArray_BCL()
		=> SmallCsv.Split(',').Length;

	[BenchmarkCategory("ToArray"), Benchmark(Description = "SplitAsSegments.ToArray()")]
	public int ToArray_ZLinq()
		=> SmallCsv.SplitAsSegments(',').ToArray().Length;
}
