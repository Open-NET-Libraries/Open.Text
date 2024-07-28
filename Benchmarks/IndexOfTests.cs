using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Open.Text.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net472, baseline: true)]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net80)]
public class IndexOfTests
{
	public const string Text = "The quick brown fox jumps over the lazy dog The quick brown fox jumps over the lazy dog";
	public static readonly string TextUpper = Text.ToUpper();
	public const string Search = "fox";
	public const string SearchCased = "Fox";

	private StringComparison _comparison = StringComparison.Ordinal;
	private StringComparison _comparisonCaseIgnored;

	[Params(
		StringComparison.Ordinal,
		StringComparison.CurrentCulture,
		StringComparison.InvariantCulture)]
	public StringComparison Comparison
	{
		get => _comparison;
		set
		{
			_comparison = value;
			_comparisonCaseIgnored = _comparison switch
			{
				StringComparison.Ordinal => StringComparison.OrdinalIgnoreCase,
				StringComparison.CurrentCulture => StringComparison.CurrentCultureIgnoreCase,
				StringComparison.InvariantCulture => StringComparison.InvariantCultureIgnoreCase,
				_ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
			};
		}
	}

	[Benchmark(Baseline = true)]
	public int IndexOf()
		=> Text.IndexOf(Search, _comparison);

	[Benchmark]
	public int IndexOfCaseIgnored()
		=> Text.IndexOf(SearchCased, _comparisonCaseIgnored);

	[Benchmark]
	public int IndexOfSpan()
		=> Text.IndexOf(Search.AsSpan(), _comparison);

	[Benchmark]
	public int IndexOfSpanCaseIgnored()
		=> Text.IndexOf(SearchCased.AsSpan(), _comparisonCaseIgnored);

	[Benchmark]
	public int IndexOfSpanSlice()
		=> Text.IndexOf(Text.AsSpan(16, 3), _comparison);

	[Benchmark]
	public int IndexOfSpanSliceCaseIgnored()
		=> Text.IndexOf(TextUpper.AsSpan(16, 3), _comparisonCaseIgnored);
}
