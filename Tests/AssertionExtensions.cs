namespace Open.Text.Tests;

/// <summary>
/// Lightweight shim providing FluentAssertions-like API using xUnit assertions.
/// </summary>
public static class AssertionExtensions
{
	public static AssertionResult<T> Should<T>(this T actual) => new(actual);

	public readonly struct AssertionResult<T>(T actual)
	{
		public void Be(T expected) => Assert.Equal(expected, actual);

		public void BeEquivalentTo(T expected) => Assert.Equal(expected, actual);
	}

	public static BoolAssertionResult Should(this bool actual) => new(actual);

	public readonly struct BoolAssertionResult(bool actual)
	{
		public void Be(bool expected) => Assert.Equal(expected, actual);

		public void BeTrue() => Assert.True(actual);

		public void BeFalse() => Assert.False(actual);
	}

	public static StringAssertionResult Should(this string? actual) => new(actual);

	public readonly struct StringAssertionResult(string? actual)
	{
		public void Be(string? expected) => Assert.Equal(expected, actual);

		public void BeEquivalentTo(string? expected) => Assert.Equal(expected, actual);
	}
}
