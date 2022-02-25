using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Open.Text.Tests;

public static class TrimTests
{
	[Fact]
	public static void NullReferences()
	{
		Assert.Throws<ArgumentNullException>(() => default(string)!.TrimStartPattern(string.Empty));
		Assert.Throws<ArgumentNullException>(() => default(string)!.TrimEndPattern(string.Empty));

		var rgx = new Regex("");
		Assert.Throws<ArgumentNullException>(() => default(string)!.TrimStartPattern(rgx));
		Assert.Throws<ArgumentNullException>(() => default(string)!.TrimEndPattern(rgx));

		Assert.Throws<ArgumentNullException>(() => string.Empty.TrimStartPattern(default(Regex)!));
		Assert.Throws<ArgumentNullException>(() => string.Empty.TrimEndPattern(default(Regex)!));
	}

	[Fact]
	public static void MaxOutOfRange()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlySpan<char>.Empty.TrimStartPattern(ReadOnlySpan<char>.Empty, max: -2));
		Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlySpan<char>.Empty.TrimStartPattern(string.Empty, max: -2));
		Assert.Throws<ArgumentOutOfRangeException>(() => string.Empty.TrimStartPattern(string.Empty, max: -2));
		Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlySpan<char>.Empty.TrimEndPattern(ReadOnlySpan<char>.Empty, max: -2));
		Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlySpan<char>.Empty.TrimEndPattern(string.Empty, max: -2));
		Assert.Throws<ArgumentOutOfRangeException>(() => string.Empty.TrimEndPattern(string.Empty, max: -2));

		var rgx = new Regex("");
		Assert.Throws<ArgumentOutOfRangeException>(() => string.Empty.TrimStartPattern(rgx, max: -2));
		Assert.Throws<ArgumentOutOfRangeException>(() => string.Empty.TrimEndPattern(rgx, max: -2));
	}

	[Theory]
	[InlineData("xyzxyzABCxyz", "xyz", "ABCxyz")]
	[InlineData("xyzxyzxyzABCxyz", "xyz", "xyzxyzxyzABCxyz", 0)]
	[InlineData("xyzxyzxyzABCxyz", "", "xyzxyzxyzABCxyz")]
	[InlineData("", "xyz", "")]
	[InlineData("xy", "xyz", "xy")]
	[InlineData("xyzxyzABCxyz", "xyz", "xyzABCxyz", 1)]
	[InlineData("xyzxyzxyzABCxyz", "xyz", "xyzABCxyz", 2)]
	[InlineData("xyzxyzABCxyz", "XyZ", "ABCxyz", -1, StringComparison.OrdinalIgnoreCase)]
	[InlineData("xyzxYzABCxyz", "XyZ", "xYzABCxyz", 1, StringComparison.OrdinalIgnoreCase)]
	[InlineData("xyzxyzxyzABCxyz", "XyZ", "xyzABCxyz", 2, StringComparison.OrdinalIgnoreCase)]
	public static void TrimStartPattern(
		string source,
		string pattern,
		string expected,
		int max = -1,
		StringComparison comparison = StringComparison.Ordinal)
	{
		Assert.Equal(expected, source.TrimStartPattern(pattern, comparison, max));
		Assert.Equal(expected, source.AsSpan().TrimStartPattern(pattern, comparison, max).ToString());
		Assert.Equal(expected, source.AsSpan().TrimStartPattern(pattern.AsSpan(), comparison, max).ToString());
	}

	[Theory]
	[InlineData("xyzABCxyzxyz", "xyz", "xyzABC")]
	[InlineData("xyzABCxyzxyz", "xyz", "xyzABCxyzxyz", 0)]
	[InlineData("xyzABCxyzxyz", "", "xyzABCxyzxyz")]
	[InlineData("", "xyz", "")]
	[InlineData("xy", "xyz", "xy")]
	[InlineData("xyzABCxyzxyz", "xyz", "xyzABCxyz", 1)]
	[InlineData("xyzABCxyzxyz", "xyz", "xyzABC", 2)]
	[InlineData("xyzABCxyzxyz", "XyZ", "xyzABC", -1, StringComparison.OrdinalIgnoreCase)]
	[InlineData("xYzABCxyzxyz", "XyZ", "xYzABCxyz", 1, StringComparison.OrdinalIgnoreCase)]
	[InlineData("xyzABCxyzxyzxyz", "XyZ", "xyzABCxyz", 2, StringComparison.OrdinalIgnoreCase)]
	public static void TrimEndPattern(
		string source,
		string pattern,
		string expected,
		int max = -1,
		StringComparison comparison = StringComparison.Ordinal)
	{
		Assert.Equal(expected, source.TrimEndPattern(pattern, comparison, max));
		Assert.Equal(expected, source.AsSpan().TrimEndPattern(pattern, comparison, max).ToString());
		Assert.Equal(expected, source.AsSpan().TrimEndPattern(pattern.AsSpan(), comparison, max).ToString());
	}

	[Fact]
	public static void TrimStartPatternRegex()
	{
		Assert.Equal("", "".TrimStartPattern(new Regex("[a-z]")));
		Assert.Equal("xyzxyzABCxyz", "xyzxyzABCxyz".TrimStartPattern(new Regex("[a-z]"), 0));
		Assert.Equal("ABCxyz", "xyzxyzABCxyz".TrimStartPattern(new Regex("[a-z]")));
		Assert.Equal("ABCxyz", "xyzxyzABCxyz".TrimStartPattern(new Regex("xyz")));
		Assert.Equal("ABCxyz", "xyzABCxyz".TrimStartPattern(new Regex("xyz"), 2));
		Assert.Equal("yzABCxyz", "xyzxyzABCxyz".TrimStartPattern(new Regex("[a-z]"), 4));
		Assert.Equal("xyzABCxyz", "XYZxyzABCxyz".TrimStartPattern(new Regex("xyz", RegexOptions.IgnoreCase | RegexOptions.RightToLeft), 1));
		Assert.Equal("ABCxyz", "xyzxyzABCxyz".TrimStartPattern(new Regex("[a-z]+")));
		Assert.Equal("ABCxyz", "xyzxyzABCxyz".TrimStartPattern(new Regex("[a-z]", RegexOptions.RightToLeft)));
		Assert.Equal("ABCxyz", "xyzxyzABCxyz".TrimStartPattern(new Regex("[a-z]+", RegexOptions.RightToLeft)));
	}

	[Fact]
	public static void TrimEndPatternRegex()
	{
		Assert.Equal("", "".TrimEndPattern(new Regex("[a-z]")));
		Assert.Equal("xyzxyzABCxyz", "xyzxyzABCxyz".TrimEndPattern(new Regex("[a-z]"), 0));
		Assert.Equal("xyzABC", "xyzABCxyzxyz".TrimEndPattern(new Regex("[a-z]", RegexOptions.RightToLeft)));
		Assert.Equal("xyzABC", "xyzABCxyzxyz".TrimEndPattern(new Regex("[a-z]+", RegexOptions.RightToLeft)));
		Assert.Equal("xyzABC", "xyzABCxyzxyz".TrimEndPattern(new Regex("[a-z]")));
		Assert.Equal("xyzABC", "xyzABCxyzxyz".TrimEndPattern(new Regex("[a-z]+")));
		Assert.Equal("xyzABCxyzx", "xyzABCxyzxyz".TrimEndPattern(new Regex("yz"), 2));
		Assert.Equal("xyzABCxyzx", "xyzABCxyzxyz".TrimEndPattern(new Regex("yz", RegexOptions.RightToLeft), 2));
	}

	[Fact]
	public static void TrimmedEquals()
	{
		Assert.True(string.Empty.TrimmedEquals(string.Empty));
		Assert.True(" ".AsSegment().TrimmedEquals(string.Empty));
		Assert.True(" ".AsSegment().TrimmedEquals(string.Empty, new[] { ' ' }.AsSpan()));
		Assert.True(" ".AsSegment().TrimmedEquals(ReadOnlySpan<char>.Empty));
		Assert.True(" ".AsSegment().TrimmedEquals(ReadOnlySpan<char>.Empty, new[] {' '}.AsSpan()));
		Assert.True(" ".TrimmedEquals(string.Empty));
		Assert.False("AB ".TrimmedEquals("ABC"));
		Assert.False("A ".TrimmedEquals("ABC"));
	}
}
