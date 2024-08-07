﻿using BenchmarkDotNet.Attributes;

namespace Open.Text.Benchmarks;

public class EnumAttributeTests
{
	public static IReadOnlyList<Attribute> GetAttribute(Greek value)
	{
		var memInfo = typeof(Greek).GetMember(value.GetName());
		var attributes = memInfo[0].GetCustomAttributes(false);
		return attributes.Length is 0
			? Array.Empty<Attribute>()
			: Array.AsReadOnly(attributes.Cast<Attribute>().ToArray());
	}

	[Benchmark(Baseline = true)]
	public void Reflection()
	{
		foreach (var greek in EnumValue<Greek>.Values)
			_ = GetAttribute(greek);
	}

	[Benchmark]
	public void Cached()
	{
		foreach (var greek in EnumValue<Greek>.Values)
			_ = greek.GetAttributes();
	}
}
