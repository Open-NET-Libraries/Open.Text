using BenchmarkDotNet.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace Open.Text.Benchmarks;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class IsDefinedTests
{
	static readonly IReadOnlyList<int> Values = Enumerable.Range(-2, 30).ToArray();

	[Benchmark(Baseline = true)]
	public void EnumIsDefined()
	{
		var type = typeof(Greek);
		foreach (var i in Values)
			_ = Enum.IsDefined(type, i);
	}

	[Benchmark] // 10x faster with EnumValue.TryGetValue(i, out Greek value) as an option.
	public void EnumValueIsDefined()
	{
		foreach (var i in Values)
			_ = EnumValue<Greek>.IsDefined(i);
	}
}
