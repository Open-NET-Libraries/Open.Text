using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Open.Text.Tests
{
	public class EqualityTests
	{
		[Theory]
		[InlineData("")]
		[InlineData("y")]
		[InlineData("hello")]
		public void EqualsSpan(string value)
		{
			var span = value.AsSpan();
			Assert.True(value.Equals(span, StringComparison.Ordinal));
		}

		[Theory]
		[InlineData("", null)]
		[InlineData("y", "n")]
		[InlineData("y", " y")]
		[InlineData("y", "Y")]
		[InlineData("hello", "hell")]
		public void NotEqualsSpan(string value, string? other)
		{
			var span = value.AsSpan();
			Assert.False(span.Equals(other, StringComparison.Ordinal));
		}
	}
}
