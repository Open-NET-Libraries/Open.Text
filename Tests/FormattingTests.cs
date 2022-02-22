using Xunit;

namespace Open.Text.Tests
{
	public static class FormattingTests
	{
		[Theory]
		[InlineData("10%", 10, 100)]
		[InlineData("50.0%", 1, 2, 1)]
		public static void ToPercentString(string expected, int value, int range, int decimals = 0)
			=> Assert.Equal(expected, value.ToPercentString(range, decimals));

		[Theory]
		[InlineData("1 byte", 1)]
		[InlineData("0 bytes", 0)]
		[InlineData("2 bytes", 2)]
		[InlineData("1,023 bytes", 1023)]
		[InlineData("1.0 KB", 1024)]
		[InlineData("1.5 KB", 1024d * 1.5)]
		[InlineData("1.5 MB", 1024d * 1024 * 1.5)]
		[InlineData("1.5 GB", 1024d * 1024 * 1024 * 1.5)]
		[InlineData("1.5 TB", 1024d * 1024 * 1024 * 1024 * 1.5)]
		[InlineData("1.5 PB", 1024d * 1024 * 1024 * 1024 * 1024 * 1.5)]
		[InlineData("1,536.0 PB", 1024d * 1024 * 1024 * 1024 * 1024 * 1024 * 1.5)]
		public static void ToByteString(string expected, double bytes)
			=> Assert.Equal(expected, bytes.ToByteString());

		[Theory]
		[InlineData("1.0", 1)]
		[InlineData("0.0", 0)]
		[InlineData("2.0", 2)]
		[InlineData("999.1", 999.1)]
		[InlineData("1.1K", 1110)]
		[InlineData("1.1M", 1110 * 1000)]
		[InlineData("1.1B", 1110 * 1000 * 1000)]
		public static void ToMetricString(string expected, double bytes)
			=> Assert.Equal(expected, bytes.ToMetricString());
	}
}
