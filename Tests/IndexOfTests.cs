namespace Open.Text.Tests;

[ExcludeFromCodeCoverage]
public static class IndexOfTests
{
	const string HELLO_WORLD_IM_HERE = "Hello World, I'm here";

	[Theory]
	[InlineData(HELLO_WORLD_IM_HERE, "World", StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, "World", StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "world", StringComparison.OrdinalIgnoreCase)]
	public static void IndexOf(string source, string value, StringComparison comparison)
	{
		var expected = source.IndexOf(value, comparison);
		source.IndexOf(value.AsSpan(), comparison).Should().Be(expected);
		source.IndexOf(value.AsSegment(), comparison).Should().Be(expected);
		source.AsSpan().IndexOf(value, comparison).Should().Be(expected);
		source.AsSegment().IndexOf(value, comparison).Should().Be(expected);
		source.IndexOf(value.AsSpan(), 0, comparison).Should().Be(expected);
		source.IndexOf(value.AsSegment(), 0, comparison).Should().Be(expected);
		source.AsSpan().IndexOf(value, 0, comparison).Should().Be(expected);
		source.AsSegment().IndexOf(value, 0, comparison).Should().Be(expected);
		source.IndexOf(value.AsSpan(), 2, comparison).Should().Be(expected);
		source.IndexOf(value.AsSegment(), 2, comparison).Should().Be(expected);
		source.AsSpan().IndexOf(value, 2, comparison).Should().Be(expected);
		source.AsSegment().IndexOf(value, 2, comparison).Should().Be(expected);
	}
}
