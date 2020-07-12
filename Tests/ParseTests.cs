using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Open.Text.Tests
{
	public static class ParseTests
	{
		[Theory]
		[InlineData("hellogoodbye", "goodbye")]
		[InlineData("hellogoodbyegoodbye", "goodbye")]
		[InlineData("hellogoodbyegoodbye", "Goodbye")]
		[InlineData("hellogoodbyegoodBye", "goodbye")]
		public static void LastIndexOf(string input, string search)
		{
			var expected = input.LastIndexOf(search, StringComparison.OrdinalIgnoreCase);
			var result = input.AsSpan().LastIndexOf(search, StringComparison.OrdinalIgnoreCase);
			Assert.Equal(expected, result);
		}
	}
}
