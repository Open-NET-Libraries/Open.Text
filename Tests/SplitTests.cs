﻿using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Open.Text.Tests;

#if NET472
internal static class Shim
{
	internal static string[] Split(this string sequence, char separator, StringSplitOptions options = StringSplitOptions.None)
		=> sequence.Split([separator], options);
}
#endif

[ExcludeFromCodeCoverage]
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
	public static void ThrowIfNull()
	{
		Assert.Throws<ArgumentException>(() => default(string)!.FirstSplit(',', out _));
		Assert.Throws<ArgumentException>(() => default(string)!.FirstSplit(",", out _));
		Assert.Throws<ArgumentException>(() => DefaultTestString.FirstSplit(default(string)!, out _));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitToEnumerable(','));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitToEnumerable(","));
		Assert.Throws<ArgumentNullException>(() => DefaultTestString.SplitToEnumerable(default(string)!));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitAsMemory(','));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitAsMemory(","));
		Assert.Throws<ArgumentNullException>(() => DefaultTestString.SplitAsMemory(default(string)!));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitAsSegments(','));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitAsSegments(","));
		Assert.Throws<ArgumentNullException>(() => DefaultTestString.SplitAsSegments(default(string)!));
	}

	[Theory]
	[InlineData("")]
	[InlineData(",")]
	[InlineData("Hello")]
	[InlineData("Hello,there")]
	[InlineData("Hello,there,I")]
	public static void FirstSplit(string sequence)
	{
		var span = sequence.AsSpan();
		Assert.Equal(sequence.FirstSplit(',', out _).ToString(), span.FirstSplit(',', out _).ToString());
		Assert.Equal(sequence.FirstSplit(",", out _).ToString(), span.FirstSplit(",", out _).ToString());
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

#pragma warning disable CS0618 // Type or member is obsolete
		// Use obsolete values to ensure they still work.
		Assert.Equal(
			TextExtensions
			.ValidAlphaNumericOnlyPattern
			.Matches(sequence)
			.Cast<Match>()
			.Select(m => m.Value),
			TextExtensions
			.ValidAlphaNumericOnlyPattern
			.AsSegments(sequence)
			.Select(m => m.Value));
#pragma warning restore CS0618 // Type or member is obsolete

		var stringSegment = sequence.AsSegment();
		var ss = stringSegment.SplitAsSegments(",", options).Select(s => s.Value).ToArray();
		Assert.Equal(segments, ss);

		if (options == StringSplitOptions.RemoveEmptyEntries) return;

		const string sep = " YES ";
		Assert.Equal(
			string.Join(sep, segments),
			stringSegment.ReplaceToString(",", sep));
	}

	[Fact]
	public static void SplitOptionsInvalid()
	{
		Assert.Throws<InvalidEnumArgumentException>(
			() => "hello there".SplitAsMemory(',', (StringSplitOptions)10000));
		Assert.Throws<InvalidEnumArgumentException>(
			() => "hello there".SplitAsMemory(",", (StringSplitOptions)10000));
		Assert.Throws<InvalidEnumArgumentException>(
			() => "hello there".SplitAsSegments(',', (StringSplitOptions)10000));
		Assert.Throws<InvalidEnumArgumentException>(
			() => "hello there".SplitAsSegments(",", (StringSplitOptions)10000));
		Assert.Throws<InvalidEnumArgumentException>(
			() => "hello there".SplitAsMemory(',', (StringSplitOptions)10000));
		Assert.Throws<InvalidEnumArgumentException>(
			() => "hello there".SplitAsMemory(",", (StringSplitOptions)10000));
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
