using System;
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
		public static void InvalidStartChar()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => DefaultTestString.FirstSplit(',', out _, 2000));
		}

		[Fact]
		public static void InvalidStartString()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => DefaultTestString.FirstSplit(",", out _, 2000));
		}

		[Fact]
		public static void ThrowIfNullChar()
		{
			Assert.Throws<ArgumentNullException>(() => Extensions.FirstSplit(default(string)!, ',', out _));
		}

		[Fact]
		public static void ThrowIfNullString()
		{
			Assert.Throws<ArgumentNullException>(() => Extensions.FirstSplit(default(string)!, ",", out _));
			Assert.Throws<ArgumentNullException>(() => Extensions.FirstSplit(DefaultTestString, default(string)!, out _));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitToEnumerable(default(string)!, ','));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitToEnumerable(default(string)!, ","));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitToEnumerable(DefaultTestString, default(string)!));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitAsMemory(default(string)!, ','));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitAsMemory(default(string)!, ","));
			Assert.Throws<ArgumentNullException>(() => Extensions.SplitAsMemory(DefaultTestString, default(string)!));

		}

		[Theory]
		[InlineData("")]
		[InlineData(",")]
		[InlineData("Hello")]
		[InlineData("Hello,there")]
		[InlineData("Hello,there,I")]
		public static void FirstSplitChar(string sequence)
		{
			Assert.Equal(sequence.FirstSplit(',', out _).ToString(), sequence.AsSpan().FirstSplit(',', out _).ToString());
		}

		[Theory]
		[InlineData("")]
		[InlineData(",")]
		[InlineData("Hello")]
		[InlineData("Hello,there")]
		[InlineData("Hello,there,I")]
		public static void FirstSplitString(string sequence)
		{
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
		public static void SplitToEnumerableChar(string sequence, StringSplitOptions options = StringSplitOptions.None)
		{
			var segments = sequence.Split(',', options);
			var r = sequence.SplitToEnumerable(',', options).ToArray();
			Assert.Equal(segments, r);
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
		public static void SplitToEnumerableString(string sequence, StringSplitOptions options = StringSplitOptions.None)
		{
			var segments = sequence.Split(",", options);
			var r = sequence.SplitToEnumerable(",", options: options).ToArray();
			Assert.Equal(segments, r);
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
		public static void SplitAsMemoryChar(string sequence, StringSplitOptions options = StringSplitOptions.None)
		{
			var segments = sequence.Split(',', options);
			var r = sequence.SplitAsMemory(',', options).Select(m=>m.Span.ToString()).ToArray();
			Assert.Equal(segments, r);
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
		public static void SplitAsMemoryString(string sequence, StringSplitOptions options = StringSplitOptions.None)
		{
			var segments = sequence.Split(",", options);
			var r = sequence.SplitAsMemory(",", options: options).Select(m => m.Span.ToString()).ToArray();
			Assert.Equal(segments, r);
		}

	}
}
