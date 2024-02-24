using Xunit;
using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Text;

namespace Open.Text.Tests;

public class StringConcatTests
{
	[Fact()]
	public void CharacterSequenceToString()
	{
		CharacterSequence a = ArraySegment<char>.Empty;
		a.ToString().Length.Should().Be(0);

		a = new ArraySegment<char>(new[] { 'a', 'b', 'c' });
		a.ToString().Should().Be("abc");

		var e = new ArraySegment<char>(Enumerable.Repeat('a', 1200).ToArray(), 0, 1100);
		var result = new string(e.AsSpan());
		a = e;
		a.ToString().Should().Be(result);
	}

	[Fact()]
	public void GetEnumeratorTest()
	{
		StringConcat chars = "";
		chars.Count.Should().Be(0);
		((IEnumerable)chars).GetEnumerator().MoveNext().Should().BeFalse();
		chars.ToString().Length.Should().Be(0);

		chars += "Hello";
		chars.Count().Should().Be(1);
		string result = chars;
		result.Should().Be("Hello");
	}

	[Fact()]
	public void GetLengthTest()
	{
		StringConcat chars = "Hello";
		chars += " World";
		chars.GetLength().Should().Be(11);
		chars[0].Length.Should().Be(5);
		chars[1].Length.Should().Be(6);
	}

	[Fact()]
	public void AppendTest()
	{
		StringConcat chars = "Hello";
		string result = chars.Append(" World");
		result.Should().Be("Hello World");

		chars.Append(" Again".AsMemory());
		result = chars;
		result.Should().Be("Hello World Again");

		var sb = new StringBuilder();
		sb.Append(' ').Append("12").Append(' ').Append("34");
		chars += sb;
		result = chars;
		result.Should().Be("Hello World Again 12 34");
	}

	[Fact()]
	public void AppendManyStringTest()
	{
		StringConcat chars = "Hello";
		chars.AppendLine(" World");
		chars.Append(new[] { "How", " are", " you?" });
		chars.ToString().Should().Be($"Hello World{Environment.NewLine}How are you?");
	}

	[Fact()]
	public void AppendManyStringSegmentTest()
	{
		StringConcat chars = "Hello";
		chars.AppendLine(" World");
		chars.Append(new StringSegment[] { "How", " are", " you?" });
		chars.ToString().Should().Be($"Hello World{Environment.NewLine}How are you?");
	}

	[Fact()]
	public void AppendManyOperatorTest()
	{
		StringConcat chars = "Hello";
		chars.AppendLine(" World");
		chars += new CharacterSequence[] { "How", " are", " you?" };
		chars.ToString().Should().Be($"Hello World{Environment.NewLine}How are you?");
	}

	[Fact()]
	public void ClearTest()
	{
		StringConcat chars = "Hello";
		chars.Clear();
		chars.Any().Should().BeFalse();
	}

	[Fact()]
	public void InsertTest()
	{
		StringConcat chars = "Hello";
		chars.Append(" World").Insert(1, " There");
		chars.ToString().Should().Be("Hello There World");
		chars.RemoveAt(1);
		chars.ToString().Should().Be("Hello World");
	}

	[Fact()]
	public void CloneTest()
	{
		StringConcat chars = "Hello";
		chars.Append(" World").Insert(1, " There");
		var chars2 = chars.Clone();
		chars2.ToString().Should().Be("Hello There World");
	}
}