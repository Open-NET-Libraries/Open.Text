﻿using System;
using Xunit;

namespace Open.Text.Tests;

public static class ComparableTests
{
	[Theory]
	[InlineData(null, null)]
	[InlineData("ABC", "abc")]
	[InlineData("XyZ", "xYz")]
	public static void CaseInsensitive(string? a, string? b)
	{
		var cA = a.AsCaseInsensitive();
		var cB = b.AsCaseInsensitive();

		Assert.True(cA == cB);
		Assert.True(cA == a);
		Assert.True(cA == b);
		Assert.True(cB == a);
		Assert.True(cB == b);

		var sA = a.AsSpan().AsCaseInsensitive();
		var sB = b.AsSpan().AsCaseInsensitive();

		Assert.True(sA == sB);
		if (a is null || b is null) return;

		Assert.True(sA == a);
		Assert.True(sB == b);
		Assert.True(sB == a);
		Assert.True(sB == b);

		Assert.True(cA == sA);
		Assert.True(cA == sB);

		Assert.True(cB == sA);
		Assert.True(cB == sB);

		Assert.True(sA == cA);
		Assert.True(sA == cB);

		Assert.True(sB == cA);
		Assert.True(sB == cB);
	}

	[Theory]
	[InlineData("ABC", "abc")]
	[InlineData("XyZ", "xYz")]
	public static void CaseSensitive(string a, string b)
	{
		var cA = a.AsComparable(StringComparison.Ordinal);
		var cB = b.AsComparable(StringComparison.Ordinal);

		Assert.False(cA == cB);	Assert.True(cA != cB);
		Assert.True(cA == a);	Assert.False(cA != a);
		Assert.False(cA == b);	Assert.True(cA != b);
		Assert.False(cB == a);	Assert.True(cB != a);
		Assert.True(cB == b);	Assert.False(cB != b);

		var sA = a.AsSpan().AsComparable(StringComparison.Ordinal);
		var sB = b.AsSpan().AsComparable(StringComparison.Ordinal);

		Assert.False(sA == sB); Assert.True(sA != sB);
		Assert.True(sA == a);	Assert.False(sA != a);
		Assert.False(sA == b);
		Assert.False(sB == a);
		Assert.True(sB == b);

		Assert.True(cA == sA);
		Assert.False(cA == sB);

		Assert.False(cB == sA);
		Assert.True(cB == sB);

		Assert.True(sA == cA);
		Assert.False(sA == cB);

		Assert.False(sB == cA);
		Assert.True(sB == cB);
	}

	[Fact]
	public static void NullTests()
	{
		var cA = "ABC".AsCaseInsensitive();
		var cB = default(string).AsCaseInsensitive();
		Assert.False(cA == cB);
		Assert.True(cA != cB);
		Assert.False(cB == cA);
		Assert.True(cB != cA);

		cA = "".AsCaseInsensitive();
		cB = default(string).AsCaseInsensitive();
		Assert.False(cA == cB);
		Assert.True(cA != cB);
		Assert.False(cB == cA);
		Assert.True(cB != cA);

		cA = default(string).AsCaseInsensitive();
		cB = default(string).AsCaseInsensitive();
		Assert.True(cA == cB);
		Assert.False(cA != cB);
		Assert.True(cB == cA);
		Assert.False(cB != cA);
	}
}
