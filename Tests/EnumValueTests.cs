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

public enum LongEnum
{
	In_the_glowing_moonlight_the_winds_caress_and_serenade_the_silent_ocean_waves,
	Amidst_the_twinkling_stars_dreams_weave_through_the_dancing_cosmic_ribbon,
	As_autumn_leaves_whisper_secrets_winds_carry_them_to_mystic_destinations,
	Underneath_the_willow_tree_souls_unite_in_soft_melodies_of_unspoken_love
}

public enum LargeEnum
{
	Item001,
	Item002,
	Item003,
	Item004,
	Item005,
	Item006,
	Item007,
	Item008,
	Item009,
	Item010,
	Item011,
	Item012,
	Item013,
	Item014,
	Item015,
	Item016,
	Item017,
	Item018,
	Item019,
	Item020,
	Item021,
	Item022,
	Item023,
	Item024,
	Item025,
	Item026,
	Item027,
	Item028,
	Item029,
	Item030,
	Item031,
	Item032,
	Item033,
	Item034,
	Item035,
	Item036,
	Item037,
	Item038,
	Item039,
	Item040,
	Item041,
	Item042,
	Item043,
	Item044,
	Item045,
	Item046,
	Item047,
	Item048,
	Item049,
	Item050,
	Item051,
	Item052,
	Item053,
	Item054,
	Item055,
	Item056,
	Item057,
	Item058,
	Item059,
	Item060,
	Item061,
	Item062,
	Item063,
	Item064,
	Item065
}

public static class EnumValueTests
{
	[Fact]
	public static void IsIntType()
	{
		var tInt = typeof(int);
		var t = EnumValue<Greek>.UnderlyingType;
		Assert.Equal(tInt, t);
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
		Assert.True(EnumValue.TryParse<Greek>(lower, true, out var e));
		Assert.False(EnumValue.TryParse<Greek>(lower, out _));
		Assert.Equal(e, EnumValue.Parse<Greek>(value));
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

	static void TryParseTestsCore<T>() where T : Enum
	{
		var values = EnumValue<T>.Values;
		foreach (var e in values)
		{
			string toString = e.GetName();

			{
				var x = new EnumValue<T>(e);
				Assert.Equal(e, x);
				Assert.Equal(e.GetHashCode(), x.GetHashCode());
				Assert.Equal(e.ToString(), x.ToString());
			}

			{
				var x = new EnumValueCaseIgnored<T>(e);
				Assert.Equal(e, x);
				Assert.Equal(e.GetHashCode(), x.GetHashCode());
				Assert.Equal(e.ToString(), x.ToString());
			}

			Assert.True(EnumValue.TryParse(toString, out T v));
			Assert.Equal(e, v);
			Assert.True(EnumValue.TryParse(toString, true, out v));
			Assert.Equal(e, v);
			Assert.True(EnumValue.TryParse(toString, false, out v));
			Assert.Equal(e, v);

			string toStringLower = toString.ToLower();
			Assert.False(EnumValue.TryParse(toStringLower, out T _));
			Assert.True(EnumValue.TryParse(toStringLower, true, out v));
			Assert.Equal(e, v);
			Assert.False(EnumValue.TryParse(toStringLower, false, out v));

			Assert.Equal(default, v);
			Assert.False(EnumValue.TryParse("XXX", out v));
			Assert.Equal(default, v);
			Assert.False(EnumValue.TryParse("XXX", true, out v));
			Assert.Equal(default, v);
			Assert.False(EnumValue.TryParse("XXX", false, out v));
			Assert.Equal(default, v);
		}

		// Final tests that cover values that don't exist.
		Assert.False(EnumValue.TryParse("Item000", out T _));
		Assert.False(EnumValue.TryParse("Item100", out T _));
		Assert.False(EnumValue.TryParse("Item1000", out T _));
	}

	[Fact]
	public static void TryParseTests()
		=> TryParseTestsCore<Greek>();

	[Fact]
	public static void LongEnumTests()
		=> TryParseTestsCore<LongEnum>();

	[Fact]
	public static void LargeEnumTests()
		=> TryParseTestsCore<LargeEnum>();
}
