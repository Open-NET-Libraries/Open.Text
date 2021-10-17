using System;
using Xunit;

namespace Open.Text.Tests
{
	public static class StringSegmentTests
	{
		[Fact]
		public static void Validity()
		{
			var s = default(StringSegment);
			Assert.False(s.IsValid);
			Assert.Equal(s, s.SourceSegment);
			Assert.True(s.Slice(0, 0) == s);
			Assert.Throws<InvalidOperationException>(() => s.Slice(1, 1));
			Assert.True(s == s.SourceSegment);
			Assert.NotEqual(0, s.ToString().Length); // base string.
			Assert.Equal(0, ((ReadOnlySpan<char>)s).Length);
			Assert.Equal(0, ((ReadOnlyMemory<char>)s).Length);
		}

		[Theory]
		[InlineData("Hello well how are you", "o", "Hell")]
		[InlineData("Hello well how are you", "ll", "He")]
		[InlineData("Hello well how are you", "Ll", "He", StringComparison.OrdinalIgnoreCase)]
		[InlineData("Hello well how are you", "Hell", "")]
		[InlineData("Hello well how are you", "xxx", null)]
		public static void BeforeFirst(string source, string pattern, string? expected, StringComparison comparisonType = StringComparison.Ordinal)
		{
			var first = source.First(pattern, comparisonType);
			if (expected is null) Assert.False(first.IsValid);
			else
			{
				var p = first.Preceding();
				Assert.Equal(expected, p.ToString());
				Assert.True(p.ToString().Equals(expected, comparisonType));
				Assert.True(p.AsSpan().Equals(expected, comparisonType));
				Assert.True(p.AsMemory().Span.Equals(expected, comparisonType));
			}
		}

		[Theory]
		[InlineData("Hello well how are you", "o", " well how are you")]
		[InlineData("Hello well how are you", "ll", "o well how are you")]
		[InlineData("Hello well how are you", "Ll", "o well how are you", StringComparison.OrdinalIgnoreCase)]
		[InlineData("Hello well how are you", "Hell", "o well how are you")]
		[InlineData("Hello well how are you", "xxx", null)]
		public static void AfterFirst(string source, string pattern, string? expected, StringComparison comparisonType = StringComparison.Ordinal)
		{
			var first = source.First(pattern, comparisonType);
			if (expected is null) Assert.False(first.IsValid);
			else Assert.Equal(expected, first.Following().ToString());
		}

		[Theory]
		[InlineData("Hello well how are you", "o", "Hello well how are y")]
		[InlineData("Hello well how are you", "ll", "Hello we")]
		[InlineData("Hello well how are you", "Ll", "Hello we", StringComparison.OrdinalIgnoreCase)]
		[InlineData("Hello well how are you", "Hell", "")]
		[InlineData("Hello well how are you", "xxx", null)]
		public static void BeforeLast(string source, string pattern, string? expected, StringComparison comparisonType = StringComparison.Ordinal)
		{
			var last = source.Last(pattern, comparisonType);
			if (expected is null) Assert.False(last.IsValid);
			else Assert.Equal(expected, last.Preceding().ToString());
		}

		[Theory]
		[InlineData("Hello well how are you", "o", "u")]
		[InlineData("Hello well how are you", "ll", " how are you")]
		[InlineData("Hello well how are you", "Ll", " how are you", StringComparison.OrdinalIgnoreCase)]
		[InlineData("Hello well how are you", "Hell", "o well how are you")]
		[InlineData("Hello well how are you", "xxx", null)]
		public static void AfterLast(string source, string pattern, string? expected, StringComparison comparisonType = StringComparison.Ordinal)
		{
			var last = source.Last(pattern, comparisonType);
			if (expected is null) Assert.False(last.IsValid);
			else Assert.Equal(expected, last.Following().ToString());
		}

		[Fact]
		public static void Offsets()
		{
			var segment = "Hello well how are you".First("well how");
			Assert.Equal("Hello ", segment.Preceding(100).ToString());
			Assert.Equal("o ", segment.Preceding(2).ToString());
			Assert.Equal(" are you", segment.Following(100).ToString());
			Assert.Equal(" a", segment.Following(2).ToString());
			Assert.Equal("Hello well how", segment.Preceding(100, true).ToString());
			Assert.Equal("o well how", segment.Preceding(2, true).ToString());
			Assert.Equal("well how are you", segment.Following(100, true).ToString());
			Assert.Equal("well how a", segment.Following(2, true).ToString());
			Assert.Equal("well how", segment.ToString());
			Assert.Equal("well", segment.OffsetLength(-4).ToString());
			Assert.Equal("how", segment.OffsetIndex(+5).ToString());
			Assert.Equal("well how are", segment.OffsetLength(+4).ToString());
			Assert.Equal("Hello well how", segment.OffsetIndex(-6).ToString());
			Assert.Throws<ArgumentOutOfRangeException>(() => segment.OffsetIndex(-7));
			Assert.Throws<ArgumentOutOfRangeException>(() => segment.OffsetLength(9));
		}

		[Fact]
		public static void Slice()
		{
			var segment = "Hello well how are you".First("well how");
			Assert.Equal("well how", segment.ToString());
			var slice = segment.Slice(0, 4);
			Assert.Equal("well", slice.ToString());
			Assert.True(slice.Equals("well".AsSpan()));
			Assert.Throws<ArgumentOutOfRangeException>(() => slice.Slice(5, 5));
			Assert.Throws<ArgumentOutOfRangeException>(() => slice.Slice(0, 5));
			Assert.True(slice.Slice(1, 6, true).Equals("ell ho"));
			Assert.True(slice.Slice(1, 7, true).Equals("ell how"));
			Assert.True(slice.Slice(1, 15, true).Equals("ell how are you"));
			Assert.Throws<ArgumentOutOfRangeException>(() => slice.Slice(1, 16, true));
		}

		[Theory]
		[InlineData("xyz")]
		[InlineData(" xyz")]
		[InlineData("  xyz ")]
		[InlineData(" \t xyz ")]
		public static void Trim(string input)
		{
			var segment = input.AsSegment();
			Assert.Equal(input.TrimEnd(), segment.TrimEnd().ToString());
			Assert.Equal(input.TrimStart(), segment.TrimStart().ToString());
			Assert.Equal(input.Trim(), segment.Trim().ToString());
		}

		[Theory]
		[InlineData("xyz", '*')]
		[InlineData("*xyz", '*')]
		[InlineData("**xyz*", '*')]
		[InlineData("*xyz**", '*')]
		public static void TrimChar(string input, char c)
		{
			var segment = input.AsSegment();
			Assert.Equal(input.TrimEnd(c), segment.TrimEnd(c).ToString());
			Assert.Equal(input.TrimStart(c), segment.TrimStart(c).ToString());
			Assert.Equal(input.Trim(c), segment.Trim(c).ToString());
		}

		[Theory]
		[InlineData("xyz", '*', '$')]
		[InlineData("*xyz", '*', '$')]
		[InlineData("**xyz*", '*', '$')]
		[InlineData("*xyz**", '*', '$')]
		[InlineData("*$*xyz*", '*', '$')]
		[InlineData("*xyz*$", '*', '$')]
		[InlineData("$*xyz*", '*', '$')]
		public static void TrimChars(string input, params char[] c)
		{
			var segment = input.AsSegment();
			Assert.Equal(input.TrimEnd(c), segment.TrimEnd(c).ToString());
			Assert.Equal(input.TrimStart(c), segment.TrimStart(c).ToString());
			Assert.Equal(input.Trim(c), segment.Trim(c).ToString());
		}
	}
}
