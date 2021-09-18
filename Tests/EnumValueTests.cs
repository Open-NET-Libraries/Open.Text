using System;
using Xunit;

namespace Open.Text.Tests
{
	public static class EnumValueTests
	{
		enum Greek
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
		}

		[Theory]
		[InlineData("Cappa")]
		[InlineData("Theta")]
		public static void EvalValueParseFail(string value)
		{
			Assert.Throws<ArgumentException>(()=> Enum.Parse(typeof(Greek), value));
			Assert.Throws<ArgumentException>(() => _ = new EnumValue<Greek>(value));
			Assert.Throws<ArgumentException>(() => _ = new EnumValueCaseIgnored<Greek>(value));
		}

		static void CheckImplicit(EnumValue<Greek> value, Greek expected)
		{
			Assert.Equal(expected, value);
			Assert.True(value==expected);
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
}
