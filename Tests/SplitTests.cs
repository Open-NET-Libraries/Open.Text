using System.Text.RegularExpressions;
using ZLinq;

namespace Open.Text.Tests;

#if NET472
internal static class Shim
{
	[SuppressMessage("Performance", "OPENTXT002:Use SplitAsSegments to reduce string allocations", Justification = "<Pending>")]
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
		// NoAlloc variants
		Assert.Throws<ArgumentException>(() => DefaultTestString.SplitAsSegmentsNoAlloc(string.Empty));
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
		// NoAlloc variants
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitToEnumerableNoAlloc(','));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitToEnumerableNoAlloc(","));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitAsMemoryNoAlloc(','));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitAsMemoryNoAlloc(","));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitAsSegmentsNoAlloc(','));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitAsSegmentsNoAlloc(","));
		Assert.Throws<ArgumentNullException>(() => DefaultTestString.SplitAsSegmentsNoAlloc(default(string)!));
		Assert.Throws<ArgumentNullException>(() => default(StringSegment).SplitAsSegmentsNoAlloc(','));
		Assert.Throws<ArgumentNullException>(() => default(StringSegment).SplitAsSegmentsNoAlloc(","));
		Assert.Throws<ArgumentNullException>(() => default(string)!.SplitAsSegmentsNoAlloc(new Regex(",")));
		Assert.Throws<ArgumentNullException>(() => DefaultTestString.SplitAsSegmentsNoAlloc(default(Regex)!));
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
	[InlineData("Hello , there,, I,am  ,Joe")]
	[InlineData("Hello , there,, I,am  ,Joe", StringSplitOptions.RemoveEmptyEntries)]
#if NET5_0_OR_GREATER
	[InlineData("Hello , there,, I,am  ,Joe", StringSplitOptions.TrimEntries)]
	[InlineData("Hello , there,, I,am  ,Joe", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)]
#endif
	[InlineData("Hello,there,I,am,Joe,")]
	[InlineData(",Hello,there,I,am,Joe")]
	[InlineData(",Hello,there,I,am,Joe,")]
	[InlineData(",Hello,there,I,am,Joe", StringSplitOptions.RemoveEmptyEntries)]
	[InlineData(",Hello,there,I,am,Joe,", StringSplitOptions.RemoveEmptyEntries)]
#pragma warning disable IDE0079 // Use SplitAsSegments to reduce string allocations
	[SuppressMessage("Performance", "OPENTXT002:Use SplitAsSegments to reduce string allocations")]
	[SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "<Pending>")]
#pragma warning restore IDE0079 // Use SplitAsSegments to reduce string allocations
	public static void Split(string sequence, StringSplitOptions options = StringSplitOptions.None)
	{
		var segments = sequence.Split(',', options);
		Assert.Equal(segments, sequence.SplitToEnumerable(',', options).ToArray());
		Assert.Equal(segments, sequence.SplitToEnumerable(",", options: options).ToArray());
		Assert.Equal(segments, sequence.SplitAsMemory(',', options).Select(m => m.Span.ToString()).ToArray());
		Assert.Equal(segments, sequence.SplitAsMemory(",", options: options).Select(m => m.Span.ToString()).ToArray());
		Assert.Equal(segments, sequence.SplitAsSegments(',', options).Select(m => m.ToString()).ToArray());
		Assert.Equal(segments, sequence.SplitAsSegments(",", options: options).Select(m => m.ToString()).ToArray());
		Assert.Equal(segments, sequence.SplitAsSegments(new Regex(","), options: options).Select(m => m.ToString()).ToArray());
		// NoAlloc variants - should produce identical results
		Assert.Equal(segments, sequence.SplitToEnumerableNoAlloc(',', options).ToArray());
		Assert.Equal(segments, sequence.SplitToEnumerableNoAlloc(",", options: options).ToArray());
		Assert.Equal(segments, sequence.SplitAsMemoryNoAlloc(',', options).Select(m => m.Span.ToString()).ToArray());
		Assert.Equal(segments, sequence.SplitAsMemoryNoAlloc(",", options: options).Select(m => m.Span.ToString()).ToArray());
		Assert.Equal(segments, sequence.SplitAsSegmentsNoAlloc(',', options).Select(m => m.ToString()).ToArray());
		Assert.Equal(segments, sequence.SplitAsSegmentsNoAlloc(",", options: options).Select(m => m.ToString()).ToArray());
		Assert.Equal(segments, sequence.SplitAsSegmentsNoAlloc(new Regex(","), options: options).Select(m => m.ToString()).ToArray());
		var span = sequence.AsSpan();
		Assert.Equal(segments, span.Split(',', options));
		Assert.Equal(segments, span.Split(",", options));
#if NET6_0_OR_GREATER
		Assert.Equal(sequence.Split("I,", options), span.Split("i,", options, StringComparison.OrdinalIgnoreCase));
#endif

		Assert.Equal(
			RegexPatterns
			.ValidAlphaNumericOnlyPattern
			.Matches(sequence)
			.Cast<Match>()
			.Select(m => m.Value).ToArray(),
			RegexPatterns
			.ValidAlphaNumericOnlyPattern
			.AsSegments(sequence)
			.Select(m => m.Value).ToArray());

		var stringSegment = sequence.AsSegment();
		var ss = stringSegment.SplitAsSegments(",", options).Select(s => s.Value).ToArray();
		Assert.Equal(segments, ss);

		if (options != StringSplitOptions.None) return;

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
	[SuppressMessage("Performance", "OPENTXT002:Use SplitAsSegments to reduce string allocations")]
	public static void SplitIgnoreCase(string sequence, string split, string expected, StringSplitOptions options = StringSplitOptions.None)
	{
		var segments = expected.Split(',');
		Assert.Equal(segments, sequence.SplitToEnumerable(split, options, StringComparison.OrdinalIgnoreCase).ToArray());
		Assert.Equal(segments, sequence.SplitAsMemory(split, options, StringComparison.OrdinalIgnoreCase).Select(m => m.Span.ToString()).ToArray());
		Assert.Equal(segments, sequence.SplitAsSegments(split, options, StringComparison.OrdinalIgnoreCase).Select(m => m.ToString()).ToArray());
		// NoAlloc variants
		Assert.Equal(segments, sequence.SplitToEnumerableNoAlloc(split, options, StringComparison.OrdinalIgnoreCase).ToArray());
		Assert.Equal(segments, sequence.SplitAsMemoryNoAlloc(split, options, StringComparison.OrdinalIgnoreCase).Select(m => m.Span.ToString()).ToArray());
		Assert.Equal(segments, sequence.SplitAsSegmentsNoAlloc(split, options, StringComparison.OrdinalIgnoreCase).Select(m => m.ToString()).ToArray());
		var span = sequence.AsSpan();
		Assert.Equal(segments, span.Split(split, options, StringComparison.OrdinalIgnoreCase));
	}
}
