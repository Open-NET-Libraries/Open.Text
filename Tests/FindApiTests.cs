using Microsoft.Extensions.Primitives;

namespace Open.Text.Tests;

[ExcludeFromCodeCoverage]
public static class FindAPITests
{
	[Fact]
	public static void Exists()
	{
		StringSegment segment = "Hello World!";
		{
			var e = segment.Find("Hello");
			e.Exists().Should().BeTrue();
			e.First().Success.Should().BeTrue();
			e.First().Index.Should().Be(0);
		}
		{
			var e = segment.Find("xxx");
			e.Exists().Should().BeFalse();
			e.First().Success.Should().BeFalse();
		}

		{
			var e = segment.Find("l").First();
			e.Success.Should().BeTrue();
			e.Index.Should().Be(2);

			e = e.Next();
			e.Success.Should().BeTrue();
			e.Index.Should().Be(3);

			e = e.Next();
			e.Success.Should().BeTrue();
			e.Index.Should().Be(9);

			e = e.Next();
			e.Success.Should().BeFalse();
			e.Index.Should().Be(-1);
		}
	}
}
