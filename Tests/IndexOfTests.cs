namespace Open.Text.Tests;

[ExcludeFromCodeCoverage]
public static class IndexOfTests
{
	const string HELLO_WORLD_IM_HERE = "Hello World, world I'm here";

	[Theory]
	[InlineData(HELLO_WORLD_IM_HERE, "", StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, "World", StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, "World", StringComparison.CurrentCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, "World", StringComparison.InvariantCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, "World", StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "world", StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "world", StringComparison.CurrentCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "world", StringComparison.InvariantCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.CurrentCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.InvariantCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.CurrentCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.InvariantCultureIgnoreCase)]
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

		var found = expected != -1;
		source.Contains(value, comparison).Should().Be(found);
		source.AsSpan().Contains(value, comparison).Should().Be(found);
		source.Contains(value.AsSpan(), comparison).Should().Be(found);
		source.AsSpan().Contains(value.AsSpan(), comparison).Should().Be(found);
	}

	[Theory]
	[InlineData(HELLO_WORLD_IM_HERE, StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, StringComparison.CurrentCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, StringComparison.InvariantCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, StringComparison.CurrentCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, StringComparison.InvariantCultureIgnoreCase)]
	public static void IndexOfNull(string source, StringComparison comparison)
	{
		// An empty ReadOnlySpan<char> appears to be always be a valid match of index 0 because it can't be determined if null.
		// Where as with a StringSegment, null can never be found but it's not necessary to throw.

		var value = default(string).AsSegment();
		source.IndexOf(value.AsSpan(), comparison).Should().Be(0);
		source.IndexOf(value, comparison).Should().Be(-1);
		source.AsSpan().IndexOf(value, comparison).Should().Be(-1);
		source.AsSegment().IndexOf(value, comparison).Should().Be(-1);
		source.IndexOf(value.AsSpan(), 0, comparison).Should().Be(0);
		source.IndexOf(value, 0, comparison).Should().Be(-1);
		source.AsSpan().IndexOf(value, 0, comparison).Should().Be(-1);
		source.AsSegment().IndexOf(value, 0, comparison).Should().Be(-1);
		source.IndexOf(value.AsSpan(), 2, comparison).Should().Be(0);
		source.IndexOf(value, 2, comparison).Should().Be(-1);
		source.AsSpan().IndexOf(value, 2, comparison).Should().Be(-1);
		source.AsSegment().IndexOf(value, 2, comparison).Should().Be(-1);

		source.LastIndexOf(value.AsSpan(), comparison).Should().Be(HELLO_WORLD_IM_HERE.Length);
		source.LastIndexOf(value, comparison).Should().Be(-1);
		source.AsSpan().LastIndexOf(value, comparison).Should().Be(-1);
		source.AsSegment().LastIndexOf(value, comparison).Should().Be(-1);

		source.Contains(value, comparison).Should().BeFalse();
		source.AsSpan().Contains(value, comparison).Should().BeFalse();
		source.Contains(value.AsSpan(), comparison).Should().BeTrue();
		source.AsSpan().Contains(value.AsSpan(), comparison).Should().BeTrue();
	}

	[Theory]
	[InlineData(HELLO_WORLD_IM_HERE, 'W', StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, 'W', StringComparison.CurrentCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, 'W', StringComparison.InvariantCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, 'W', StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'w', StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'w', StringComparison.CurrentCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'w', StringComparison.InvariantCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'X', StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, 'X', StringComparison.CurrentCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, 'X', StringComparison.InvariantCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, 'X', StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'x', StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'x', StringComparison.CurrentCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'x', StringComparison.InvariantCultureIgnoreCase)]
	public static void IndexOfChar(string source, char value, StringComparison comparison)
	{
		var expected = source.IndexOf(value, comparison);
		source.AsSpan().IndexOf(value, comparison).Should().Be(expected);
		source.AsSegment().IndexOf(value, comparison).Should().Be(expected);
		Assert.Throws<ArgumentOutOfRangeException>(() => source.AsSpan().IndexOf(value, -1, comparison));
		source.AsSpan().IndexOf(value, 0, comparison).Should().Be(expected);
		source.AsSegment().IndexOf(value, 0, comparison).Should().Be(expected);
		source.AsSpan().IndexOf(value, 2, comparison).Should().Be(expected);
		source.AsSegment().IndexOf(value, 2, comparison).Should().Be(expected);

		var found = expected != -1;
		source.Contains(value, comparison).Should().Be(found);
		source.AsSpan().Contains(value, comparison).Should().Be(found);
		source.AsSegment().Contains(value, comparison).Should().Be(found);
	}

	// Now the LastIndexOf tests.
	[Theory]
	[InlineData(HELLO_WORLD_IM_HERE, "", StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, "World", StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, "World", StringComparison.CurrentCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, "World", StringComparison.InvariantCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, "World", StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "world", StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "world", StringComparison.CurrentCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "world", StringComparison.InvariantCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.CurrentCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.InvariantCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.CurrentCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, "foo", StringComparison.InvariantCultureIgnoreCase)]
	public static void LastIndexOf(string source, string value, StringComparison comparison)
	{
		// This reports differently for 4.72 than later versions.
		// We will retain modern behavior as it makes slighty more sense.
		var expected = source.LastIndexOf(value, comparison);
#if NET472
		if (value.Length == 0)
			expected++;
#endif
		source.LastIndexOf(value.AsSpan(), comparison).Should().Be(expected);
		source.LastIndexOf(value.AsSegment(), comparison).Should().Be(expected);
		source.AsSpan().LastIndexOf(value, comparison).Should().Be(expected);
		source.AsSegment().LastIndexOf(value, comparison).Should().Be(expected);
		source.AsSpan().LastIndexOf(value.AsSpan(), comparison).Should().Be(expected);
		source.AsSegment().LastIndexOf(value.AsSpan(), comparison).Should().Be(expected);
		source.AsSpan().LastIndexOf(value.AsSegment(), comparison).Should().Be(expected);
		source.AsSegment().LastIndexOf(value.AsSegment(), comparison).Should().Be(expected);
		if (value.Length == 0)
			string.Empty.LastIndexOf(value.AsSpan(), comparison).Should().Be(0);
	}

	[Theory]
	[InlineData(HELLO_WORLD_IM_HERE, 'W', StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, 'W', StringComparison.CurrentCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, 'W', StringComparison.InvariantCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, 'W', StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'w', StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'w', StringComparison.CurrentCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'w', StringComparison.InvariantCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'X', StringComparison.Ordinal)]
	[InlineData(HELLO_WORLD_IM_HERE, 'X', StringComparison.CurrentCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, 'X', StringComparison.InvariantCulture)]
	[InlineData(HELLO_WORLD_IM_HERE, 'X', StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'x', StringComparison.OrdinalIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'x', StringComparison.CurrentCultureIgnoreCase)]
	[InlineData(HELLO_WORLD_IM_HERE, 'x', StringComparison.InvariantCultureIgnoreCase)]
	public static void LastIndexOfChar(string source, char value, StringComparison comparison)
	{
		var expected = source.LastIndexOf(value, comparison);
		source.AsSpan().LastIndexOf(value, comparison).Should().Be(expected);
		source.AsSegment().LastIndexOf(value, comparison).Should().Be(expected);
	}
}
