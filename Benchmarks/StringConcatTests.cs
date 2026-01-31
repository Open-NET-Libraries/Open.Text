using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Primitives;
using System.Text;
using ZLinq;

namespace Open.Text.Benchmarks;

[MemoryDiagnoser]
public class StringConcatTests
{
	public static readonly string Phrase
		= "Hello I'm from andromeda. Take me to your leader. There is nothing to fear.";

	private static readonly ReadOnlyMemory<string> Words = Phrase.Split(' ');

	private static readonly ReadOnlyMemory<StringSegment> Segments = Phrase.SplitAsSegments(' ').ToArray();

	[Benchmark(Baseline = true)]
	public string StringOperator()
	{
		var s = "";
		{
			var len = Words.Length;
			var span = Words.Span;
			for (var i = 0; i < len; i++)
				s += span[i];
		}

		{
			var len = Segments.Length;
			var span = Segments.Span;
			for (var i = 0; i < len; i++)
				s += span[i].ToString();
		}

		return s;
	}

	[Benchmark]
	public string StringBuilder()
	{
		var sb = new StringBuilder();
		{
			var len = Words.Length;
			var span = Words.Span;
			for (var i = 0; i < len; i++)
				sb.Append(span[i]);
		}

		{
			var len = Segments.Length;
			var span = Segments.Span;
			for (var i = 0; i < len; i++)
				sb.AppendSegment(span[i]);
		}

		return sb.ToString();
	}

	[Benchmark]
	public string StringBuilderHelper()
	{
		StringBuilderHelper s = "";
		{
			var len = Words.Length;
			var span = Words.Span;
			for (var i = 0; i < len; i++)
				s += span[i];
		}

		{
			var len = Segments.Length;
			var span = Segments.Span;
			for (var i = 0; i < len; i++)
				s += span[i];
		}

		return s;
	}
}
