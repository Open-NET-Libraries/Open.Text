using System.Text;

namespace Open.Text.Tests;

[ExcludeFromCodeCoverage]
public static class StringBuilderHelperTests
{
	[Fact]
	public static void Constructor_WithStringBuilder()
	{
		var sb = new StringBuilder("test");
		var helper = new StringBuilderHelper(sb);
		Assert.Equal("test", helper.ToString());
		Assert.Same(sb, helper.Builder);
	}

	[Fact]
	public static void Constructor_WithStringBuilder_Null_ThrowsArgumentNullException()
		=> Assert.Throws<ArgumentNullException>(() => new StringBuilderHelper(null!));

	[Fact]
	public static void Constructor_Default()
	{
		var helper = new StringBuilderHelper();
		Assert.NotNull(helper.Builder);
		Assert.Equal(string.Empty, helper.ToString());
	}

	[Fact]
	public static void Constructor_WithCapacity()
	{
		var helper = new StringBuilderHelper(100);
		Assert.NotNull(helper.Builder);
		Assert.True(helper.Builder.Capacity >= 100);
		Assert.Equal(string.Empty, helper.ToString());
	}

	[Fact]
	public static void OperatorPlus_WithChar()
	{
		StringBuilderHelper helper = 'H';
		helper += 'e';
		helper += 'l';
		helper += 'l';
		helper += 'o';
		Assert.Equal("Hello", helper.ToString());
	}

	[Fact]
	public static void OperatorPlus_WithString()
	{
		StringBuilderHelper helper = "Hello";
		helper += " ";
		helper += "World";
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void OperatorPlus_WithStringSegment()
	{
		StringBuilderHelper helper = "Hello";
		helper += " ".AsSegment();
		helper += "World".AsSegment();
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void OperatorPlus_WithReadOnlySpan()
	{
		StringBuilderHelper helper = "Hello";
		helper += " ".AsSpan();
		helper += "World".AsSpan();
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void OperatorPlus_WithSpan()
	{
		StringBuilderHelper helper = "Hello";
		Span<char> span1 = [' '];
		Span<char> span2 = ['W', 'o', 'r', 'l', 'd'];
		helper += span1;
		helper += span2;
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void OperatorPlus_WithCharArray()
	{
		StringBuilderHelper helper = "Hello";
		helper += [' '];
		helper += ['W', 'o', 'r', 'l', 'd'];
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void OperatorPlus_WithReadOnlyMemory()
	{
		StringBuilderHelper helper = "Hello";
		ReadOnlyMemory<char> memory1 = new char[] { ' ' }.AsMemory();
		ReadOnlyMemory<char> memory2 = new char[] { 'W', 'o', 'r', 'l', 'd' }.AsMemory();
		helper += memory1;
		helper += memory2;
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void OperatorPlus_WithMemory()
	{
		StringBuilderHelper helper = "Hello";
		Memory<char> memory1 = new char[] { ' ' }.AsMemory();
		Memory<char> memory2 = new char[] { 'W', 'o', 'r', 'l', 'd' }.AsMemory();
		helper += memory1;
		helper += memory2;
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void OperatorPlus_WithArraySegment()
	{
		StringBuilderHelper helper = "Hello";
		var array1 = new char[] { ' ' };
		var array2 = new char[] { 'W', 'o', 'r', 'l', 'd' };
		helper += new ArraySegment<char>(array1);
		helper += new ArraySegment<char>(array2);
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void OperatorPlus_WithIEnumerable()
	{
		StringBuilderHelper helper = "Hello";
		var chars1 = new List<char> { ' ' };
		var chars2 = new List<char> { 'W', 'o', 'r', 'l', 'd' };
		helper += chars1;
		helper += chars2;
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void ImplicitConversion_FromString()
	{
		StringBuilderHelper helper = "Hello World";
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void ImplicitConversion_FromString_Empty()
	{
		StringBuilderHelper helper = string.Empty;
		Assert.Equal(string.Empty, helper.ToString());
	}

	[Fact]
	public static void ImplicitConversion_FromChar()
	{
		StringBuilderHelper helper = 'X';
		Assert.Equal("X", helper.ToString());
	}

	[Fact]
	public static void ImplicitConversion_FromStringSegment()
	{
		StringBuilderHelper helper = "Hello World".AsSegment();
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void ImplicitConversion_FromReadOnlySpan()
	{
		StringBuilderHelper helper = "Hello World".AsSpan();
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void ImplicitConversion_FromReadOnlyMemory()
	{
		StringBuilderHelper helper = "Hello World".AsMemory();
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void ImplicitConversion_FromMemory()
	{
		Memory<char> memory = "Hello World".ToCharArray().AsMemory();
		StringBuilderHelper helper = memory;
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void ImplicitConversion_FromArraySegment()
	{
		var array = "Hello World".ToCharArray();
		StringBuilderHelper helper = new ArraySegment<char>(array);
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void Add_StaticMethod()
	{
		var helper = new StringBuilderHelper();
		helper = StringBuilderHelper.Add(helper, "Hello");
		helper = StringBuilderHelper.Add(helper, " World");
		Assert.Equal("Hello World", helper.ToString());
	}

	[Fact]
	public static void CombinedOperations()
	{
		StringBuilderHelper helper = "Start";
		helper += ' ';
		helper += "middle".AsSegment();
		helper += ' ';
		helper += "end".AsSpan();
		Assert.Equal("Start middle end", helper.ToString());
	}
}
