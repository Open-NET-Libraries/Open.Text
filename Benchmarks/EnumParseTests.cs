using BenchmarkDotNet.Attributes;
using FastEnumUtility;
using System.Security;

namespace Open.Text.Benchmarks;

/*
|                  Method | UseValid | IgnoreCase |     Mean |    Error |   StdDev | Ratio | RatioSD | Allocated |
|------------------------ |--------- |----------- |---------:|---------:|---------:|------:|--------:|----------:|
| CompiledSwitchByLengths |     True |      False | 12.63 ns | 0.285 ns | 0.776 ns |  1.00 |    0.00 |         - |
|          EnumValueParse |     True |      False | 50.22 ns | 0.543 ns | 0.481 ns |  3.98 |    0.17 |         - |
|           FastEnumParse |     True |      False | 46.10 ns | 0.780 ns | 0.730 ns |  3.62 |    0.22 |         - |
|                         |          |            |          |          |          |       |         |           |
| CompiledSwitchByLengths |     True |       True | 17.29 ns | 0.185 ns | 0.173 ns |  1.00 |    0.00 |         - |
|          EnumValueParse |     True |       True | 59.01 ns | 0.340 ns | 0.284 ns |  3.41 |    0.04 |         - |
|           FastEnumParse |     True |       True | 72.12 ns | 0.451 ns | 0.352 ns |  4.17 |    0.05 |         - |
*/

[MemoryDiagnoser]
public class EnumParseTests
{
	private bool useValid;
	private bool ignoreCase;

	Tests _tests = new InvalidTests();

	void SetTests()
		=> _tests = useValid
			? (ignoreCase ? new ValidTestsIC() : new ValidTests())
			: (ignoreCase ? new InvalidTestsIC() : new InvalidTests());

	[Params(true/*, false*/)]
	public bool UseValid
	{
		get => useValid;
		set
		{
			useValid = value;
			SetTests();
		}
	}

	[Params(false/*, true*/)]
	public bool IgnoreCase
	{
		get => ignoreCase;
		set
		{
			ignoreCase = value;
			SetTests();
		}
	}

	static readonly string[] ValidValues = [nameof(Greek.Alpha), nameof(Greek.Epsilon), nameof(Greek.Phi), nameof(Greek.Beta), nameof(Greek.Gamma)];
	static readonly string[] InvalidValues = ["Apple", "Orange", "Pineapple", "Grapefruit", "Lemon"];

	// To avoid branching overhead when benchmarking.
	abstract class Tests
	{
		public abstract Greek EnumParse();

		public abstract Greek EnumValueParse();

		public abstract Greek FastEnumParse();

		public abstract Greek CompiledSwitch();

		public abstract Greek CompiledSwitchByLength();

		static readonly IDictionary<string, Greek> LookupD
			= Enum
			.GetValues<Greek>()
			.ToDictionary(e => Enum.GetName(e)!, e => e, StringComparer.Ordinal);

		protected virtual bool Lookup(string value, out Greek e)
			=> LookupD.TryGetValue(value, out e);

		public Greek DictionaryLookup()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!Lookup(s, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}
	}

	class ValidTests : Tests
	{
		public override Greek EnumParse()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!Enum.TryParse(s, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}

		public override Greek CompiledSwitch()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!TryParseBySwitch(s, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}

		public override Greek CompiledSwitchByLength()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!TryParseByLengthSwitch(s, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}

		public override Greek EnumValueParse()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!EnumValue.TryParse(s, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}

		public override Greek FastEnumParse()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!FastEnum.TryParse(s, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}
	}

	class InvalidTests : Tests
	{
		public override Greek EnumParse()
		{
			Greek e = default;
			foreach (string s in InvalidValues)
			{
				if (Enum.TryParse(s, out e))
					throw new Exception("Valid.");
			}
			return e;
		}

		public override Greek CompiledSwitch()
		{
			Greek e = default;
			foreach (string s in InvalidValues)
			{
				if (TryParseBySwitch(s, out e))
					throw new Exception("Valid.");
			}
			return e;
		}

		public override Greek CompiledSwitchByLength()
		{
			Greek e = default;
			foreach (string s in InvalidValues)
			{
				if (TryParseByLengthSwitch(s, out e))
					throw new Exception("Valid.");
			}
			return e;
		}

		public override Greek EnumValueParse()
		{
			Greek e = default;
			foreach (string s in InvalidValues)
			{
				if (EnumValue.TryParse(s, out e))
					throw new Exception("Valid.");
			}
			return e;
		}

		public override Greek FastEnumParse()
		{
			Greek e = default;
			foreach (string s in InvalidValues)
			{
				if (FastEnum.TryParse(s, out e))
					throw new Exception("Valid.");
			}
			return e;
		}
	}

	class ValidTestsIC : Tests
	{
		public override Greek EnumParse()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!Enum.TryParse(s, true, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}

		public override Greek CompiledSwitch()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!TryParseBySwitchIgnoreCase(s, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}

		public override Greek CompiledSwitchByLength()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!TryParseByLengthSwitchCaseIgnored(s, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}

		public override Greek EnumValueParse()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!EnumValue.TryParse(s, true, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}

		public override Greek FastEnumParse()
		{
			Greek e = default;
			foreach (string s in ValidValues)
			{
				if (!FastEnum.TryParse(s, true, out e))
					throw new Exception("Invalid.");
			}
			return e;
		}

		static readonly IDictionary<string, Greek> LookupD
			= Enum
			.GetValues<Greek>()
			.ToDictionary(e => Enum.GetName(e)!, e => e, StringComparer.OrdinalIgnoreCase);

		protected override bool Lookup(string value, out Greek e)
			=> LookupD.TryGetValue(value, out e);
	}

	class InvalidTestsIC : Tests
	{
		public override Greek EnumParse()
		{
			Greek e = default;
			foreach (string s in InvalidValues)
			{
				if (Enum.TryParse(s, true, out e))
					throw new Exception("Valid.");
			}
			return e;
		}

		public override Greek CompiledSwitch()
		{
			Greek e = default;
			foreach (string s in InvalidValues)
			{
				if (TryParseBySwitchIgnoreCase(s, out e))
					throw new Exception("Valid.");
			}
			return e;
		}

		public override Greek CompiledSwitchByLength()
		{
			Greek e = default;
			foreach (string s in InvalidValues)
			{
				if (TryParseByLengthSwitchCaseIgnored(s, out e))
					throw new Exception("Valid.");
			}
			return e;
		}

		public override Greek EnumValueParse()
		{
			Greek e = default;
			foreach (string s in InvalidValues)
			{
				if (EnumValue.TryParseIgnoreCase(s, out e))
					throw new Exception("Valid.");
			}
			return e;
		}

		public override Greek FastEnumParse()
		{
			Greek e = default;
			foreach (string s in InvalidValues)
			{
				if (FastEnum.TryParse(s, true, out e))
					throw new Exception("Valid.");
			}
			return e;
		}

		static readonly IDictionary<string, Greek> LookupD
			= Enum
			.GetValues<Greek>()
			.ToDictionary(e => Enum.GetName(e)!, e => e, StringComparer.OrdinalIgnoreCase);

		protected override bool Lookup(string value, out Greek e)
			=> LookupD.TryGetValue(value, out e);
	}

	private static bool TryParseBySwitch(string value, out Greek e)
	{
		switch (value)
		{
			case nameof(Greek.Alpha):
				e = Greek.Alpha;
				return true;
			case nameof(Greek.Beta):
				e = Greek.Beta;
				return true;
			case nameof(Greek.Kappa):
				e = Greek.Kappa;
				return true;
			case nameof(Greek.Delta):
				e = Greek.Delta;
				return true;
			case nameof(Greek.Epsilon):
				e = Greek.Epsilon;
				return true;
			case nameof(Greek.Gamma):
				e = Greek.Gamma;
				return true;
			case nameof(Greek.Omega):
				e = Greek.Omega;
				return true;
			case nameof(Greek.Phi):
				e = Greek.Phi;
				return true;
			case nameof(Greek.Theta):
				e = Greek.Theta;
				return true;
			case nameof(Greek.None):
				e = Greek.None;
				return true;
			default:
				e = default!;
				return false;
		}
	}

	private static bool TryParseBySwitchIgnoreCase(string value, out Greek e)
	{
		if (value.Equals(nameof(Greek.Alpha), StringComparison.OrdinalIgnoreCase))
		{
			e = Greek.Alpha;
			return true;
		}

		if (value.Equals(nameof(Greek.Beta), StringComparison.OrdinalIgnoreCase))
		{
			e = Greek.Beta;
			return true;
		}

		if (value.Equals(nameof(Greek.Kappa), StringComparison.OrdinalIgnoreCase))
		{
			e = Greek.Kappa;
			return true;
		}

		if (value.Equals(nameof(Greek.Delta), StringComparison.OrdinalIgnoreCase))
		{
			e = Greek.Delta;
			return true;
		}

		if (value.Equals(nameof(Greek.Epsilon), StringComparison.OrdinalIgnoreCase))
		{
			e = Greek.Epsilon;
			return true;
		}

		if (value.Equals(nameof(Greek.Gamma), StringComparison.OrdinalIgnoreCase))
		{
			e = Greek.Gamma;
			return true;
		}

		if (value.Equals(nameof(Greek.Omega), StringComparison.OrdinalIgnoreCase))
		{
			e = Greek.Omega;
			return true;
		}

		if (value.Equals(nameof(Greek.Phi), StringComparison.OrdinalIgnoreCase))
		{
			e = Greek.Phi;
			return true;
		}

		if (value.Equals(nameof(Greek.Theta), StringComparison.OrdinalIgnoreCase))
		{
			e = Greek.Theta;
			return true;
		}

		if (value.Equals(nameof(Greek.None), StringComparison.OrdinalIgnoreCase))
		{
			e = Greek.None;
			return true;
		}

		e = default!;
		return false;
	}

	private static bool TryParseByLengthSwitch(string value, out Greek e)
	{
		switch (value.Length)
		{
			case 3:
				if (nameof(Greek.Phi) == value)
				{
					e = Greek.Phi;
					return true;
				}

				e = default!;
				return false;

			case 4:
				switch (value)
				{
					case nameof(Greek.Beta):
						e = Greek.Beta;
						return true;
					case nameof(Greek.None):
						e = Greek.None;
						return true;
					default:
						e = default!;
						return false;
				}

			case 5:

				switch (value)
				{
					case nameof(Greek.Alpha):
						e = Greek.Alpha;
						return true;
					case nameof(Greek.Kappa):
						e = Greek.Kappa;
						return true;
					case nameof(Greek.Delta):
						e = Greek.Delta;
						return true;
					case nameof(Greek.Gamma):
						e = Greek.Gamma;
						return true;
					case nameof(Greek.Omega):
						e = Greek.Omega;
						return true;
					case nameof(Greek.Theta):
						e = Greek.Theta;
						return true;
					default:
						e = default!;
						return false;
				}

			case 7:
				if (nameof(Greek.Epsilon) == value)
				{
					e = Greek.Epsilon;
					return true;
				}

				e = default!;
				return false;
		}

		e = default!;
		return false;
	}

	private static bool TryParseByLengthSwitchCaseIgnored(string value, out Greek e)
	{
		switch (value.Length)
		{
			case 3:
				if (value.Equals(nameof(Greek.Phi), StringComparison.OrdinalIgnoreCase))
				{
					e = Greek.Phi;
					return true;
				}

				e = default!;
				return false;

			case 4:
				if (value.Equals(nameof(Greek.Beta), StringComparison.OrdinalIgnoreCase))
				{
					e = Greek.Beta;
					return true;
				}

				if (value.Equals(nameof(Greek.None), StringComparison.OrdinalIgnoreCase))
				{
					e = Greek.None;
					return true;
				}

				e = default!;
				return false;

			case 5:

				if (value.Equals(nameof(Greek.Alpha), StringComparison.OrdinalIgnoreCase))
				{
					e = Greek.Alpha;
					return true;
				}

				if (value.Equals(nameof(Greek.Kappa), StringComparison.OrdinalIgnoreCase))
				{
					e = Greek.Kappa;
					return true;
				}

				if (value.Equals(nameof(Greek.Delta), StringComparison.OrdinalIgnoreCase))
				{
					e = Greek.Delta;
					return true;
				}

				if (value.Equals(nameof(Greek.Gamma), StringComparison.OrdinalIgnoreCase))
				{
					e = Greek.Gamma;
					return true;
				}

				if (value.Equals(nameof(Greek.Omega), StringComparison.OrdinalIgnoreCase))
				{
					e = Greek.Omega;
					return true;
				}

				if (value.Equals(nameof(Greek.Theta), StringComparison.OrdinalIgnoreCase))
				{
					e = Greek.Theta;
					return true;
				}

				e = default!;
				return false;

			case 7:
				if (value.Equals(nameof(Greek.Epsilon), StringComparison.OrdinalIgnoreCase))
				{
					e = Greek.Epsilon;
					return true;
				}

				e = default!;
				return false;
		}

		e = default!;
		return false;
	}

	//[Benchmark]
	public Greek EnumParse() => _tests.EnumParse();

	//[Benchmark]
	public Greek CompiledSwitch() => _tests.CompiledSwitch();

	// This tends to be the indisputable fastest and could be accomplished through a source generator.
	[Benchmark(Baseline = true)]
	public Greek CompiledSwitchByLengths() => _tests.CompiledSwitchByLength();

	[Benchmark]
	// Uses an expression tree when case sensitive.
	public Greek EnumValueParse() => _tests.EnumValueParse();

	[Benchmark]
	public Greek FastEnumParse() => _tests.FastEnumParse();

	[Benchmark]
	public Greek DictionaryLookup() => _tests.DictionaryLookup();
}
