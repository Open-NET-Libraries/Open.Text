﻿using System.Text;

namespace Open.Text.Tests;

[ExcludeFromCodeCoverage]
public static class StringBuilderTests
{
	[Theory]
	[InlineData("Hello there.")]
	[InlineData("ABC")]
	[InlineData("A")]
	[InlineData("")]
	[InlineData(null)]
	public static void ToStringBuilder(string? source)
	{
		if (source is null)
		{
			Assert.Throws<ArgumentNullException>(() => source!.ToStringBuilder());
			Assert.Throws<ArgumentNullException>(() => default(StringBuilder)!.AppendAll(source));
			Assert.Throws<ArgumentNullException>(() => default(StringBuilder)!.AppendAll(source, 'x'));
			Assert.Throws<ArgumentNullException>(() => default(StringBuilder)!.AppendAll(source, "xx"));
			Assert.Throws<ArgumentNullException>(() => default(StringBuilder)!.AppendAll(ReadOnlySpan<char>.Empty));
			Assert.Throws<ArgumentNullException>(() => default(StringBuilder)!.AppendAll(ReadOnlySpan<char>.Empty, 'x'));
			Assert.Throws<ArgumentNullException>(() => default(StringBuilder)!.AppendAll(ReadOnlySpan<char>.Empty, "xx"));
			Assert.Throws<ArgumentNullException>(() => default(StringBuilder)!.AppendWithSeparator('x', 1));
			Assert.Throws<ArgumentNullException>(() => default(StringBuilder)!.AppendWithSeparator("xx", "Yes"));
			Assert.Equal("hello", new StringBuilder("hello").AppendAll(source).ToString());
			return;
		}

		var span = source.AsSpan();
		var a = span.ToArray();
		{
			var sb = a.ToStringBuilder();
			Assert.Equal(source.Length, sb.Length);
			Assert.Equal(source, sb.ToString());

			sb.Clear();
			sb.AppendAll(a);
			Assert.Equal(source, sb.ToString());

			sb.Clear();
			sb.AppendAll(a);
			Assert.Equal(source, sb.ToString());
		}

		{
			var sb = span.ToStringBuilder();
			Assert.Equal(source.Length, sb.Length);
			Assert.Equal(source, sb.ToString());
			sb.Clear();
			sb.AppendAll(a);
			Assert.Equal(source, sb.ToString());
		}

		{
			var sb = a.AsSpan().ToStringBuilder();
			Assert.Equal(source.Length, sb.Length);
			Assert.Equal(source, sb.ToString());
			sb.Clear();
			sb.AppendAll(a);
			Assert.Equal(source, sb.ToString());
		}
	}

	[Theory]
	[InlineData("Hello there.", ',')]
	[InlineData("ABC", '-')]
	[InlineData("A", ',')]
	[InlineData("", ',')]
	[InlineData(null, 'x')]
	public static void ToStringBuilderSeparatedChar(string? source, char separator)
	{
		if (source is null)
		{
			Assert.Throws<ArgumentNullException>(() => source!.ToStringBuilder(separator));
			Assert.Throws<ArgumentNullException>(() => default(StringBuilder)!.AppendAll(source, separator!));
			Assert.Equal("hello", new StringBuilder("hello").AppendAll(source, separator).ToString());
			return;
		}

		var span = source.AsSpan();
		var a = source.AsSpan().ToArray();
		var joined = string.Join(""+separator, a);
		var list = a.ToList();
		list.Insert(0, 'X');
		var xValue = string.Join("" + separator, list);
		{
			var sb = a.ToStringBuilder(separator);
			Assert.Equal(joined, sb.ToString());

			sb.Clear();
			sb.AppendAll(a, separator);
			Assert.Equal(joined, sb.ToString());

			sb.Clear();
			sb.Append('X');
			sb.AppendAll(a, separator);
			Assert.Equal(xValue, sb.ToString());
		}

		{
			var sb = span.ToStringBuilder(separator);
			Assert.Equal(joined, sb.ToString());

			sb.Clear();
			sb.AppendAll(a, separator);
			Assert.Equal(joined, sb.ToString());

			sb.Clear();
			sb.AppendAll(span, separator);
			Assert.Equal(joined, sb.ToString());

			sb.Clear();
			sb.Append('X');
			sb.AppendAll(a, separator);
			Assert.Equal(xValue, sb.ToString());

			sb.Clear();
			sb.Append('X');
			sb.AppendAll(span, separator);
			Assert.Equal(xValue, sb.ToString());
		}

		{
			var sb = a.AsSpan().ToStringBuilder(separator);
			Assert.Equal(joined, sb.ToString());
		}

		{
			var sb = new StringBuilder(source);
			sb.AppendWithSeparator(separator, 1, "Yes");
			var suffix = $"1{separator}Yes";
			if (source.Length == 0) Assert.Equal(suffix, sb.ToString());
			else Assert.Equal(source + separator + suffix, sb.ToString());
		}
	}

	[Theory]
	[InlineData("Hello there.", ", ")]
	[InlineData("ABC", "--")]
	[InlineData("A", ", ")]
	[InlineData("", ", ")]
	[InlineData(null, "  ")]
	[InlineData("xyz", null)]
	public static void ToStringBuilderSeparatedString(string? source, string? separator)
	{
		if (source is null)
		{
			Assert.Throws<ArgumentNullException>(() => source!.ToStringBuilder(separator));
			Assert.Throws<ArgumentNullException>(() => default(StringBuilder)!.AppendAll(source, separator));
			Assert.Equal("hello", new StringBuilder("hello").AppendAll(source, separator).ToString());
			return;
		}

		var span = source.AsSpan();
		var a = source.AsSpan().ToArray();
		var joined = string.Join(separator ?? string.Empty, a);
		var list = a.ToList();
		list.Insert(0, 'X');
		var xValue = string.Join(separator, list);

		{
			var sb = a.ToStringBuilder(separator);
			Assert.Equal(joined, sb.ToString());

			sb.Clear();
			sb.AppendAll(a, separator);
			Assert.Equal(joined, sb.ToString());

			sb.Clear();
			sb.Append('X');
			sb.AppendAll(a, separator);
			Assert.Equal(xValue, sb.ToString());
		}

		{
			var sb = span.ToStringBuilder(separator);
			Assert.Equal(joined, sb.ToString());

			sb.Clear();
			sb.AppendAll(a, separator);
			Assert.Equal(joined, sb.ToString());

			sb.Clear();
			sb.AppendAll(span, separator);
			Assert.Equal(joined, sb.ToString());

			sb.Clear();
			sb.Append('X');
			sb.AppendAll(a, separator);
			Assert.Equal(xValue, sb.ToString());

			sb.Clear();
			sb.Append('X');
			sb.AppendAll(span, separator);
			Assert.Equal(xValue, sb.ToString());
		}

		{
			var sb = a.AsSpan().ToStringBuilder(separator);
			Assert.Equal(joined, sb.ToString());
		}

		{
			var sb = new StringBuilder(source);
			sb.AppendWithSeparator(separator, 1, "Yes");
			var suffix = $"1{separator}Yes";
			if (source.Length == 0) Assert.Equal(suffix, sb.ToString());
			else Assert.Equal(source + separator + suffix, sb.ToString());
		}
	}

	// Unit Tests: Theories for StringBuilder.Trim() extension that have multiple values added.
	[Theory]
	// Expected, A, B, C, D
	[InlineData("Hello there.", "Hello", " ", "there.", "")]
	[InlineData("Hello there.", " Hello", " ", "there.", "")]
	[InlineData("Hello there.", "  Hello", " ", "there.", " ")]
	[InlineData("Hello there.", "Hello", " ", "there.", " ")]
	[InlineData("", "", " ", "", " ")]
	[InlineData("", "", "", "", "")]
	public static void Trim(string expected, string a, string b, string c, string d)
	{
		var sb = new StringBuilder();
		sb.Append(a).Append(b).Append(c).Append(d).Trim();
		Assert.Equal(expected, sb.ToString());
	}

	// Tests that cover the StringBuilder Trim but with multiple characters to trim using a ReadOnlySpan<char>
	[Theory]
	// Expected, A, B, C, D
	[InlineData("Hello there.", "!Hello", " ", "there.", "")]
	[InlineData("Hello there.", " Hello", " ", "there.", "!")]
	[InlineData("Hello there.", " ! Hello", " ", "there.!", " ")]
	[InlineData("Hello there.", "Hello", " ", "there.", " ")]
	[InlineData("", "", " ", "", " ")]
	[InlineData("", "", "", "", "")]
	public static void TrimChars(string expected, string a, string b, string c, string d)
	{
		var sb = new StringBuilder();
		sb.Append(a).Append(b).Append(c).Append(d).Trim(" !".AsSpan());
		Assert.Equal(expected, sb.ToString());
	}

	[Fact]
	public static void StringBuilderHelperTest()
	{
		StringBuilderHelper sb = new();
		sb += "Hello";
		sb += " ";
		sb += "World";
		sb.ToString().Should().Be("Hello World");
	}
}
