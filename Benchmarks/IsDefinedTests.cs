using BenchmarkDotNet.Attributes;

namespace Open.Text.Benchmarks;

public class IsDefinedTests
{
	static readonly IReadOnlyList<int> Values = [.. Enumerable.Range(-2, 30)];

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
