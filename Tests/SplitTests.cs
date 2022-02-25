using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Open.Text.Tests
{
	public static class SplitTests
	{
		private const string DefaultTestString = "Hello,there";

		[Fact]
		public static void EmptyLengthSplit()
		{
			Assert.Throws<ArgumentException>(() => DefaultTestString.AsSpan().FirstSplit(ReadOnlySpan<char>.Empty, out _));
			Assert.Throws<ArgumentException>(() => DefaultTestString.FirstSplit(string.Empty, out _));
			Assert.Throws<ArgumentException>(() => DefaultTestString.SplitToEnumerable(string.Empty));
			Assert.Throws<ArgumentException>(() => DefaultTestString.SplitAsMemory(string.Empty));
			Assert.Throws<ArgumentException>(() => DefaultTestString.SplitAsSegments(string.Empty));
		}

		[Fact]
		public static void InvalidStart()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => DefaultTestString.FirstSplit(',', out _, 2000));
			Assert.Throws<ArgumentOutOfRangeException>(() => DefaultTestString.FirstSplit(",", out _, 2000));
		}

		[Fact]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1196:Call extension method as instance method.", Justification = "Preferred.")]
		public static void ThrowIfNull()
		{
			Assert.Throws<ArgumentException>(() => TextExtensions.FirstSplit(default!, ',', out _));
			Assert.Throws<ArgumentException>(() => TextExtensions.FirstSplit(default!, ",", out _));
			Assert.Throws<ArgumentException>(() => TextExtensions.FirstSplit(DefaultTestString, default(string)!, out _));
			Assert.Throws<ArgumentNullException>(() => TextExtensions.SplitToEnumerable(default!, ','));
			Assert.Throws<ArgumentNullException>(() => TextExtensions.SplitToEnumerable(default!, ","));
			Assert.Throws<ArgumentNullException>(() => TextExtensions.SplitToEnumerable(DefaultTestString, default(string)!));
			Assert.Throws<ArgumentNullException>(() => TextExtensions.SplitAsMemory(default!, ','));
			Assert.Throws<ArgumentNullException>(() => TextExtensions.SplitAsMemory(default!, ","));
			Assert.Throws<ArgumentNullException>(() => TextExtensions.SplitAsMemory(DefaultTestString, default(string)!));
			Assert.Throws<ArgumentNullException>(() => TextExtensions.SplitAsSegments(default!, ','));
			Assert.Throws<ArgumentNullException>(() => TextExtensions.SplitAsSegments(default!, ","));
			Assert.Throws<ArgumentNullException>(() => TextExtensions.SplitAsSegments(DefaultTestString, default(string)!));
		}

		[Theory]
		[InlineData("")]
		[InlineData(",")]
		[InlineData("Hello")]
		[InlineData("Hello,there")]
		[InlineData("Hello,there,I")]
		public static void FirstSplit(string sequence)
		{
			Assert.Equal(sequence.FirstSplit(',', out _).ToString(), sequence.AsSpan().FirstSplit(',', out _).ToString());
			Assert.Equal(sequence.FirstSplit(",", out _).ToString(), sequence.AsSpan().FirstSplit(",", out _).ToString());
		}

		[Theory]
		[InlineData("")]
		[InlineData(",")]
		[InlineData("", StringSplitOptions.RemoveEmptyEntries)]
		[InlineData(",", StringSplitOptions.RemoveEmptyEntries)]
		[InlineData("Hello")]
		[InlineData("Hello,there")]
		[InlineData("Hello,there,I")]
		[InlineData("Hello,there,I,am")]
		[InlineData("Hello,there,I,am,Joe")]
		[InlineData("Hello,there,,I,am,Joe")]
		[InlineData("Hello,there,,I,am,Joe", StringSplitOptions.RemoveEmptyEntries)]
		[InlineData("Hello,there,I,am,Joe,")]
		[InlineData(",Hello,there,I,am,Joe")]
		[InlineData(",Hello,there,I,am,Joe,")]
		[InlineData(",Hello,there,I,am,Joe", StringSplitOptions.RemoveEmptyEntries)]
		[InlineData(",Hello,there,I,am,Joe,", StringSplitOptions.RemoveEmptyEntries)]
		public static void Split(string sequence, StringSplitOptions options = StringSplitOptions.None)
		{
			var segments = sequence.Split(',', options);
			Assert.Equal(segments, sequence.SplitToEnumerable(',', options));
			Assert.Equal(segments, sequence.SplitToEnumerable(",", options: options));
			Assert.Equal(segments, sequence.SplitAsMemory(',', options).Select(m => m.Span.ToString()));
			Assert.Equal(segments, sequence.SplitAsMemory(",", options: options).Select(m => m.Span.ToString()));
			Assert.Equal(segments, sequence.SplitAsSegments(',', options).Select(m => m.ToString()));
			Assert.Equal(segments, sequence.SplitAsSegments(",", options: options).Select(m => m.ToString()));
			Assert.Equal(segments, sequence.SplitAsSegments(new Regex(","), options: options).Select(m => m.ToString()));
			var span = sequence.AsSpan();
			Assert.Equal(segments, span.Split(',', options));
			Assert.Equal(segments, span.Split(",", options));

			Assert.Equal(
				TextExtensions
				.ValidAlphaNumericOnlyPattern
				.Matches(sequence)
				.Select(m => m.Value),
				TextExtensions
				.ValidAlphaNumericOnlyPattern
				.AsSegments(sequence)
				.Select(m => m.Value));

			var stringSegment = sequence.AsSegment();
			var ss = stringSegment.SplitAsSegments(",", options).Select(s => s.Value).ToArray();
			Assert.Equal(segments, ss);

			if (options == StringSplitOptions.RemoveEmptyEntries) return;

			const string sep = " YES ";
			Assert.Equal(
				string.Join(sep, segments),
				stringSegment.ReplaceToString(",", sep));
		}

		[Theory]
		[InlineData("Hello", "ll", "He,o")]
		[InlineData("HelLo", "Ll", "He,o")]
		[InlineData("Hello", "LL", "He,o")]
		[InlineData("Hello", "l", "He,o", StringSplitOptions.RemoveEmptyEntries)]
		[InlineData("HelLo", "L", "He,o", StringSplitOptions.RemoveEmptyEntries)]
		public static void SplitIgnoreCase(string sequence, string split, string expected, StringSplitOptions options = StringSplitOptions.None)
		{
			var segments = expected.Split(',');
			Assert.Equal(segments, sequence.SplitToEnumerable(split, options, StringComparison.OrdinalIgnoreCase));
			Assert.Equal(segments, sequence.SplitAsMemory(split, options, StringComparison.OrdinalIgnoreCase).Select(m => m.Span.ToString()));
			Assert.Equal(segments, sequence.SplitAsSegments(split, options, StringComparison.OrdinalIgnoreCase).Select(m => m.ToString()));
			var span = sequence.AsSpan();
			Assert.Equal(segments, span.Split(split, options, StringComparison.OrdinalIgnoreCase));
		}
	}
}