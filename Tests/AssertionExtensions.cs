namespace Open.Text.Tests;

/// <summary>
/// Lightweight shim providing FluentAssertions-like API using xUnit assertions.
/// </summary>
public static class AssertionExtensions
{
	public static AssertionResult<T> Should<T>(this T actual) => new(actual);

	public readonly struct AssertionResult<T>
	{
		private readonly T _actual;

		public AssertionResult(T actual) => _actual = actual;

		public void Be(T expected) => Assert.Equal(expected, _actual);
		
		public void BeEquivalentTo(T expected) => Assert.Equal(expected, _actual);
	}

	public static BoolAssertionResult Should(this bool actual) => new(actual);

	public readonly struct BoolAssertionResult
	{
		private readonly bool _actual;

		public BoolAssertionResult(bool actual) => _actual = actual;

		public void Be(bool expected) => Assert.Equal(expected, _actual);
		
		public void BeTrue() => Assert.True(_actual);
		
		public void BeFalse() => Assert.False(_actual);
	}

	public static StringAssertionResult Should(this string? actual) => new(actual);

	public readonly struct StringAssertionResult
	{
		private readonly string? _actual;

		public StringAssertionResult(string? actual) => _actual = actual;

		public void Be(string? expected) => Assert.Equal(expected, _actual);
		
		public void BeEquivalentTo(string? expected) => Assert.Equal(expected, _actual);
	}
}
