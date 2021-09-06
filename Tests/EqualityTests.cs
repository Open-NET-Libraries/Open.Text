using System;
using Xunit;

namespace Open.Text.Tests
{
	public class EqualityTests
	{
		[Fact]
		public void NullReference()
		{
			Assert.Throws<ArgumentNullException>(() => default(string)!.Equals(ReadOnlySpan<char>.Empty, StringComparison.Ordinal));
			Assert.Throws<ArgumentNullException>(() => default(string)!.Equals(ReadOnlySpan<char>.Empty.ToArray().AsSpan(), StringComparison.Ordinal));
		}

		[Theory]
		[InlineData("")]
		[InlineData("y")]
		[InlineData("y", StringComparison.OrdinalIgnoreCase)]
		[InlineData("hello")]
		public void EqualsSpan(string value, StringComparison comparison = StringComparison.Ordinal)
		{
			var roSpan = value.AsSpan();
			var span = roSpan.ToArray().AsSpan();
			Assert.True(roSpan.Equals(value, comparison));
			Assert.True(value.Equals(roSpan, comparison));
			Assert.True(value.Equals(roSpan.ToArray().AsSpan(), comparison));
			Assert.True(span.Equals(value, comparison));
			Assert.True(span.Equals(roSpan, comparison));
			Assert.True(roSpan.Equals(span, comparison));
			Assert.True(span.Equals(span, comparison));
		}

		[Theory]
		[InlineData("", null)]
		[InlineData("", "n")]
		[InlineData("y", "n")]
		[InlineData("y", "n", StringComparison.OrdinalIgnoreCase)]
		[InlineData("y", " y")]
		[InlineData("y", "Y")]
		[InlineData("hello", "hell")]
		public void NotEqualsSpan(string value, string? other, StringComparison comparison = StringComparison.Ordinal)
		{
			var roSpan = value.AsSpan();
			var span = roSpan.ToArray().AsSpan();
			Assert.False(roSpan.Equals(other, comparison));
			if (other is not null)
			{
				var oRoSpan = other.AsSpan();
				Assert.False(roSpan.Equals(oRoSpan, comparison));
				Assert.False(value.Equals(oRoSpan, comparison));
				Assert.False(span.Equals(oRoSpan, comparison));
				var oSpan = oRoSpan.ToArray().AsSpan();
				Assert.False(value.Equals(oSpan, comparison));
				Assert.False(span.Equals(oSpan, comparison));
			}
			Assert.False(span.Equals(other, comparison));
		}
	}
}
