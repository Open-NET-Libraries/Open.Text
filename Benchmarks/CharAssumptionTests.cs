using BenchmarkDotNet.Attributes;

namespace Open.Text.Benchmarks;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "For benchmarking.")]
public class CharAssumptionTests
{
	const string TestString = "abcdefghijklmnopqrstuvwxyz0123456789";
	static readonly ReadOnlyMemory<char> TestMemory = TestString.AsMemory();
	const int Length = 32;
	const int Repeat = 1024 * 1024;

	[Benchmark(Baseline = true)]
	public char[] ArrayUpdate()
	{
		var a = new char[Length];
		for (var n = 0; n < Repeat; n++)
		{
			for (var i = 0; i < Length; i++)
				a[i] = TestString[i];
		}

		return a;
	}

	[Benchmark]
	public char[] ArrayToUpdate()
	{
		var a = new char[Length];
		for (var n = 0; n < Repeat; n++)
		{
			for (var i = 0; i < Length; i++)
				Update(a, i, TestString[i]);
		}

		return a;

		static void Update(char[] a, int i, char v) => a[i] = v;
	}

	[Benchmark]
	public char[] ArrayFromSpanUpdate()
	{
		var a = new char[Length];
		var s = TestMemory.Span;
		for (var n = 0; n < Repeat; n++)
		{
			for (var i = 0; i < Length; i++)
				a[i] = s[i];
		}

		return a;
	}

	[Benchmark]
	public Span<char> SpanToSpanUpdate()
	{
		var a = new char[Length].AsSpan();
		var s = TestMemory.Span;
		for (var n = 0; n < Repeat; n++)
		{
			for (var i = 0; i < Length; i++)
				a[i] = s[i];
		}

		return a;
	}

	[Benchmark]
	public Span<char> SpanToSpanRefUpdate()
	{
		var a = new char[Length].AsSpan();
		var s = TestMemory.Span;
		for (var n = 0; n < Repeat; n++)
		{
			for (var i = 0; i < Length; i++)
			{
				ref readonly var v = ref s[i];
				ref var x = ref a[i];
				x = v;
			}
		}

		return a;
	}

	[Benchmark]
	public Span<char> SpanToSpanRefInUpdate()
	{
		var a = new char[Length].AsSpan();
		var s = TestMemory.Span;
		for (var n = 0; n < Repeat; n++)
		{
			for (var i = 0; i < Length; i++)
			{
				ref readonly var v = ref s[i];
				Update(a, in i, in v);
			}
		}

		return a;

		static void Update(Span<char> a, in int i, in char v) => a[i] = v;
	}

	[Benchmark]
	public Span<char> SpanToSpanNotInUpdate()
	{
		var a = new char[Length].AsSpan();
		var s = TestMemory.Span;
		for (var n = 0; n < Repeat; n++)
		{
			for (var i = 0; i < Length; i++)
			{
				Update(a, in i, s[i]);
			}
		}

		return a;

		static void Update(Span<char> a, in int i, char v) => a[i] = v;
	}

	[Benchmark]
	public Span<char> SpanToSpanNotIn2Update()
	{
		var a = new char[Length].AsSpan();
		var s = TestMemory.Span;
		for (var n = 0; n < Repeat; n++)
		{
			for (var i = 0; i < Length; i++)
			{
				Update(a, in i, s[i]);
			}
		}

		return a;

		static void Update(Span<char> a, in int i, in char v) => a[i] = v;
	}
}
