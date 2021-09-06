using System;
using Xunit;

namespace Open.Text.Tests
{
	public static class BeforeAfterTests
	{
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
			else Assert.Equal(expected, first.Preceding().ToString());
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
			Assert.Equal("well", segment.OffsetLength(-4).ToString());
			Assert.Equal("how", segment.OffsetIndex(+5).ToString());
			Assert.Equal("well how are", segment.OffsetLength(+4).ToString());
			Assert.Equal("Hello well how", segment.OffsetIndex(-6).ToString());
			Assert.Throws<ArgumentOutOfRangeException>(() => segment.OffsetIndex(-7));
			Assert.Throws<ArgumentOutOfRangeException>(() => segment.OffsetLength(9));
		}
	}
}
