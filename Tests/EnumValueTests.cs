using System;
using System.Linq;
using Xunit;

namespace Open.Text.Tests;

[AttributeUsage(AttributeTargets.Field)]
public class LetterAttribute : Attribute
{
	public LetterAttribute(char upper, char lower)
	{
		Upper = upper;
		Lower = lower;
	}

	public char Upper { get; }
	public char Lower { get; }

	public bool EqualsLetter(char letter)
		=> letter == Upper || letter == Lower;
}

public enum Greek
{
	[Letter('Α', 'α')]
	Alpha,
	[Letter('Β', 'β')]
	Beta,
	[Letter('Κ', 'κ')]
	Kappa,
	[Letter('Δ', 'δ')]
	Delta,
	[Letter('Ε', 'ε')]
	Epsilon,
	[Letter('Γ', 'γ')]
	Gamma,
	[Letter('Ω', 'ω')]
	Omega,
	[Letter('Φ', 'φ')]
	Phi,
	[Letter('Θ', 'θ')]
	Theta,
	None
}

public static class EnumValueTests
{
	[Fact]
	public static void IsIntType()
	{
		var tInt = typeof(int);
		var t = EnumValue<Greek>.UnderlyingType;
		Assert.Equal(tInt,t);
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
	[InlineData("XXX")]
	[InlineData("YYY")]
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

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(2)]

	public static void GetValue(int expected)
	{
		Assert.True(EnumValue<Greek>.IsDefined(expected));
		var found = EnumValue.TryGetValue(expected, out Greek e);
		Assert.True(found);
		Assert.Equal(expected, (int)e);
	}


	[Theory]
	[InlineData(Greek.Alpha, 'Α')]
	[InlineData(Greek.Beta, 'Β')]
	[InlineData(Greek.Gamma, 'Γ')]

	public static void GetLetter(Greek expected, char letter)
	{
		var a = (LetterAttribute)expected.GetAttributes().Single();
		Assert.Equal(letter, a.Upper);
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
