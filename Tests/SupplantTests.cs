using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System;
using System.Xml.Schema;

namespace Open.Text.Tests;

public class StringUtilsTests
{
	[Fact]
	public void Supplant_ThrowsOnNullTags()
	{
		IDictionary<string, string> tags = null!;
		Action act = () => TextExtensions.Supplant("Test".AsSpan(), tags);
		act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("tags");
	}

	[Fact]
	public void Supplant_NoTags_ReturnsOriginal()
	{
		ReadOnlySpan<char> value = "Hello, ${name}!";
		IDictionary<string, string> tags = new Dictionary<string, string>();

		var result = TextExtensions.Supplant(value, tags);

		result.ToString().Should().Be("Hello, ${name}!");
	}

	[Fact]
	public void Supplant_OneTag_ReplacesCorrectly()
	{
		ReadOnlySpan<char> value = "Hello, ${name}!";
		IDictionary<string, string> tags = new Dictionary<string, string> { { "name", "John" } };

		var result = TextExtensions.Supplant(value, tags);

		result.ToString().Should().Be("Hello, John!");
	}

	[Fact]
	public void Supplant_MultipleTags_ReplacesCorrectly()
	{
		ReadOnlySpan<char> value = "Hello, ${firstName} ${lastName}!";
		IDictionary<string, string> tags = new Dictionary<string, string>
		{
			{ "firstName", "John" },
			{ "lastName", "Doe" }
		};

		var result = TextExtensions.Supplant(value, tags);

		result.ToString().Should().Be("Hello, John Doe!");
	}

	[Fact]
	public void Supplant_EmptyPattern_SkipsReplacement()
	{
		ReadOnlySpan<char> value = "Hello, ${name}${}!";
		IDictionary<string, string> tags = new Dictionary<string, string> { { "name", "John" } };

		var result = TextExtensions.Supplant(value, tags);

		result.ToString().Should().Be("Hello, John${}!");
	}

	[Fact]
	public void Supplant_NoClosingBrace_IgnoresTag()
	{
		ReadOnlySpan<char> value = "Hello, ${name";
		IDictionary<string, string> tags = new Dictionary<string, string> { { "name", "John" } };

		var result = TextExtensions.Supplant(value, tags);

		result.ToString().Should().Be("Hello, ${name");
	}
}
