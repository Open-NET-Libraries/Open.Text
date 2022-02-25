using System;
using Xunit;

namespace Open.Text.Tests;

public static class EnumValueTests
{
	public enum Greek
	{
		Alpha, Beta, Gamma
	}

	[Theory]
	[InlineData("Alpha")]
	[InlineData("Beta")]
	[InlineData("Gamma")]
	public static void EvalValueParse(string value)
	{
		CheckImplicit(value, (Greek)Enum.Parse(typeof(Greek), value));
		var lower = value.ToLower();
		CheckImplicitCaseIgnored(lower, (Greek)Enum.Parse(typeof(Greek), lower, true));
		Assert.True(EnumValue.TryParse<Greek>(lower, true, out _));
		Assert.False(EnumValue.TryParse<Greek>(lower, out _));
	}

	[Theory]
	[InlineData("Cappa")]
	[InlineData("Theta")]
	public static void EvalValueParseFail(string value)
	{
		Assert.Throws<ArgumentException>(() => Enum.Parse(typeof(Greek), value));
		Assert.False(EnumValue.TryParse<Greek>(value, out _));
		Assert.Throws<ArgumentException>(() => _ = new EnumValue<Greek>(value));
		Assert.Throws<ArgumentException>(() => _ = new EnumValueCaseIgnored<Greek>(value));
		Assert.False(EnumValue.TryParse<Greek>(value, true, out _));
	}

	[Theory]
	[InlineData(Greek.Alpha)]
	[InlineData(Greek.Beta)]
	[InlineData(Greek.Gamma)]

	public static void GetName(Greek expected)
	{
		var s = expected.ToString();
		Assert.Equal(s, expected.GetName());
	}

	static void CheckImplicit(EnumValue<Greek> value, Greek expected)
	{
		Assert.Equal(expected, value);
		Assert.True(value == expected);
		Assert.True(value.Equals(expected));
		Assert.True(value == new EnumValueCaseIgnored<Greek>(expected));
		Assert.False(value != expected);
		Assert.False(value != new EnumValueCaseIgnored<Greek>(expected));
	}

	static void CheckImplicitCaseIgnored(EnumValueCaseIgnored<Greek> value, Greek expected)
	{
		Assert.Equal(expected, value);
		Assert.True(value == expected);
		Assert.True(value.Equals(expected));
		Assert.True(value == new EnumValue<Greek>(expected));
		Assert.False(value != expected);
		Assert.False(value != new EnumValue<Greek>(expected));
	}
}
