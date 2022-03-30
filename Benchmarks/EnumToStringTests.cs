﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Open.Text.Benchmarks;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Benchmarking.")]
public class EnumToStringTests
{
	static readonly IReadOnlyList<Greek> Values = Enum.GetValues(typeof(Greek)).Cast<Greek>().ToArray();

	[Benchmark(Baseline = true)]
	public void EnumToString()
	{
		foreach (var g in Values)
			_ = g.ToString();
	}

	static string Switch(Greek value) => value switch
	{
		Greek.Alpha => nameof(Greek.Alpha),
		Greek.Beta => nameof(Greek.Beta),
		Greek.Kappa => nameof(Greek.Kappa),
		Greek.Delta => nameof(Greek.Delta),
		Greek.Epsilon => nameof(Greek.Epsilon),
		Greek.Gamma => nameof(Greek.Gamma),
		Greek.Omega => nameof(Greek.Omega),
		Greek.Phi => nameof(Greek.Phi),
		Greek.Theta => nameof(Greek.Theta),
		Greek.None => nameof(Greek.None),
		_ => throw new Exception(),
	};

	[Benchmark]
	public void CompiledSwitch()
	{
		foreach (var g in Values)
			_ = Switch(g);
	}

	[Benchmark]
	public void EnumValueGetName()
	{
		foreach (var g in Values)
			_ = g.GetName();
	}
}
