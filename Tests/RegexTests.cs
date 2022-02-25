using FluentAssertions;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Open.Text.Tests;

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

	[Fact]
	public static void GroupNameTest()
	{
		var pattern = new Regex("(?<test>hello)");
		var validGroups = pattern.Match("   hello ").Groups;
		var invalidGroups = pattern.Match(" xxx goodbye ").Groups;
		validGroups.GetValue("test")
			.Should().Be("hello");

		validGroups.GetValue("unknown")
			.Should().Be(null);

		invalidGroups.GetValue("test")
			.Should().Be(null);

		validGroups.GetValueSpan("unknown").Length
			.Should().Be(0);

		invalidGroups.GetValueSpan("test").Length
			.Should().Be(0);

		validGroups.GetValueSpan("test").ToString()
			.Should().Be("hello");

		Assert.Throws<ArgumentNullException>(() => default(GroupCollection)!.GetValue("test"));
		Assert.Throws<ArgumentNullException>(() => validGroups.GetValue(null!));
		Assert.Throws<ArgumentNullException>(() => default(GroupCollection)!.GetValueSpan("test"));
		Assert.Throws<ArgumentNullException>(() => validGroups.GetValueSpan(null!));
	}
}
