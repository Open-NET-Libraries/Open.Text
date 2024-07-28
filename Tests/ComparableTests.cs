namespace Open.Text.Tests;

[ExcludeFromCodeCoverage]
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
		var cBi = b.AsCaseInsensitive();
		var seg = a.AsSegment();

		Assert.False(cA == cB); Assert.True(cA != cB);
		Assert.True(cA == a); Assert.False(cA != a);
		Assert.True(a == cA); Assert.False(a != cA);
		Assert.True(seg == cA); Assert.False(seg != cA);
		Assert.False(cA == b); Assert.True(cA != b);
		Assert.False(b == cA); Assert.True(b != cA);
		Assert.False(cB == a); Assert.True(cB != a);
		Assert.True(cB == b); Assert.False(cB != b);

		Assert.True(cA == cBi); Assert.False(cA != cBi);
		Assert.True(cBi == cA); Assert.False(cBi != cA);

		var span = a.AsSpan();
		var sA = span.AsComparable(StringComparison.Ordinal);
		var sAi = span.AsCaseInsensitive();
		var sB = b.AsSpan().AsComparable(StringComparison.Ordinal);
		var sBi = b.AsSpan().AsCaseInsensitive();

		Assert.True(sA.Equals(cBi));
		Assert.True(sAi.Equals(cB));

		Assert.True(sAi == sBi); Assert.False(sAi != sBi);
		Assert.True(sBi == sAi); Assert.False(sBi != sAi);
		Assert.True(sAi == sB); Assert.False(sAi != sB);
		Assert.True(sB == sAi); Assert.False(sB != sAi);
		Assert.True(sBi == sA); Assert.False(sBi != sA);
		Assert.True(sA == sBi); Assert.False(sA != sBi);

		Assert.True(span == cA); Assert.False(span != cA);
		Assert.False(sA == sB); Assert.True(sA != sB);
		Assert.True(sA == a); Assert.False(sA != a);
		Assert.True(a == sA); Assert.False(a != sA);
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

		Assert.True(cA == sBi); Assert.False(cA != sBi);
		Assert.True(sBi == cA); Assert.False(sBi != cA);
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
