using System;
using Xunit;

namespace Open.Text.Tests
{
	public static class ComparableTests
	{
		[Theory]
		[InlineData("ABC", "abc")]
		[InlineData("XyZ", "xYz")]
		public static void CaseInsensitive(string a, string b)
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

			Assert.False(cA == cB);
			Assert.True(cA == a);
			Assert.False(cA == b);
			Assert.False(cB == a);
			Assert.True(cB == b);

			var sA = a.AsSpan().AsComparable(StringComparison.Ordinal);
			var sB = b.AsSpan().AsComparable(StringComparison.Ordinal);

			Assert.False(sA == sB);
			Assert.True(sA == a);
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

	}
}
