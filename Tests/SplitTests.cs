﻿using System;
using System.Linq;
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

		}

		[Fact]
		public static void InvalidStart()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => DefaultTestString.FirstSplit(',', out _, 2000));
			Assert.Throws<ArgumentOutOfRangeException>(() => DefaultTestString.FirstSplit(",", out _, 2000));
		}

		[Fact]
		public static void ThrowIfNull()
		{
			Assert.Throws<ArgumentNullException>(() => Extensions.FirstSplit(default!, ',', out _));
			Assert.Throws<ArgumentNullException>(() => Extensions.FirstSplit(default!, ",", out _));
			Assert.Throws<ArgumentNullException>(() => Extensions.FirstSplit(DefaultTestString, default(string)!, out _));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitToEnumerable(default!, ','));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitToEnumerable(default!, ","));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitToEnumerable(DefaultTestString, default(string)!));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitAsMemory(default!, ','));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitAsMemory(default!, ","));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitAsMemory(DefaultTestString, default(string)!));

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
			var span = sequence.AsSpan();
			Assert.Equal(segments, span.Split(',', options));
			Assert.Equal(segments, span.Split(",", options));
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
			var span = sequence.AsSpan();
			Assert.Equal(segments, span.Split(split, options, StringComparison.OrdinalIgnoreCase));
		}

	}
}
