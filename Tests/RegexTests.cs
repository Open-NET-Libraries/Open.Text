using System.Text.RegularExpressions;
using Xunit;

namespace Open.Text.Tests
{
	public static class RegexTests
	{
		[Fact]
		public static void CaptureSpanTest()
		{
			var pattern = new Regex("you");
			var match = pattern.Match("hello how are you Dr. Bob.");
			var span = match.AsSpan();
			Assert.Equal("you", span.ToString());
		}
	}
}
