using FluentAssertions;
using System;
using Xunit;

namespace Open.Text.Tests;

public static class ParseTests
{
	[Theory]
	[InlineData("hellogoodbye", "goodbye")]
	[InlineData("hellogoodbyegoodbye", "goodbye")]
	[InlineData("hellogoodbyegoodbye", "Goodbye")]
	[InlineData("hellogoodbyegoodBye", "goodbye")]
	[InlineData("hellogoodbyegoodBye", "xyz")]
	[InlineData("hellogoodbyegoodBye", "hellogoodbyegoodByehellogoodbyegoodBye")]
	public static void LastIndexOf(string input, string search)
	{
		var expected = input.LastIndexOf(search);
		var expectedIC = input.LastIndexOf(search, StringComparison.OrdinalIgnoreCase);

		var span = input.AsSpan();
		span.LastIndexOf(search, StringComparison.Ordinal)
			.Should().Be(expected);
		span.LastIndexOf(search, StringComparison.OrdinalIgnoreCase)
			.Should().Be(expectedIC);

		var segment = input.AsSegment();
		segment.LastIndexOf(search, StringComparison.Ordinal)
			.Should().Be(expected);
		segment.LastIndexOf(search, StringComparison.OrdinalIgnoreCase)
			.Should().Be(expectedIC);
		input.First(search).Should().Be(segment.First(search.AsSpan()));

		var contains = input.Contains(search);
		var containsIC = input.Contains(search, StringComparison.OrdinalIgnoreCase);
		input.Contains(search.AsSpan(), StringComparison.OrdinalIgnoreCase)
			.Should().Be(containsIC);
		input.Contains(search.AsSegment(), StringComparison.OrdinalIgnoreCase)
			.Should().Be(containsIC);
		segment.Contains(search)
			.Should().Be(contains);
		segment.Contains(search, StringComparison.OrdinalIgnoreCase)
			.Should().Be(containsIC);

		var comparable = segment.AsCaseInsensitive();
		Assert.True(comparable == segment);
		Assert.True(comparable == span);
		Assert.False(comparable != segment);
		Assert.False(comparable != span);
		input.SequenceEqual(span).Should().BeTrue();
		span.SequenceEqual(segment).Should().BeTrue();
		span.ToArray().AsSpan().SequenceEqual(segment).Should().BeTrue();
		span.ToArray().AsSpan().SequenceEqual(span.ToArray().AsSpan()).Should().BeTrue();
		comparable.Contains(search)
			.Should().Be(containsIC);
		comparable.Contains(search.AsSegment())
			.Should().Be(containsIC);
		comparable.Contains(search.AsSpan())
			.Should().Be(containsIC);

		var spanComparable = span.AsComparable(StringComparison.Ordinal);
		var spanComparablei = span.AsCaseInsensitive();
		Assert.True(spanComparable == spanComparablei);
		Assert.True(spanComparablei == spanComparable);
		Assert.False(spanComparable != spanComparablei);
		Assert.False(spanComparablei != spanComparable);

		Assert.True(spanComparablei == segment);
		Assert.True(spanComparablei == span);
		Assert.True(span == spanComparablei);
		Assert.True(segment == spanComparablei);
		Assert.True(segment == spanComparablei);
		Assert.True(spanComparablei == comparable);
		Assert.False(spanComparablei != segment);
		Assert.False(spanComparablei != span);
		Assert.False(span != spanComparablei);
		Assert.False(segment != spanComparablei);
		Assert.False(segment != spanComparablei);
		Assert.False(spanComparablei != comparable);
		Assert.False(comparable != spanComparablei);
		spanComparablei.Contains(search)
			.Should().Be(containsIC);
		spanComparablei.Contains(search.AsSegment())
			.Should().Be(containsIC);
		spanComparablei.Contains(search.AsSpan())
			.Should().Be(containsIC);
	}
}
