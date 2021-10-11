using BenchmarkDotNet.Attributes;
using FastEnumUtility;

namespace Open.Text.Benchmarks;

[MemoryDiagnoser]
public class EnumParseTests
{
	[Params(true, false)]
	public bool UseValid { get; set; }

	[Params(false, true)]
	public bool IgnoreCase { get; set; }


	static readonly string[] ValidValues = new string[] { Greek.Alpha.ToString(), Greek.Epsilon.ToString(), Greek.Phi.ToString() };
	static readonly string[] InvalidValues = new string[] { "Apple", "Orange", "Pineapple" };

	[Benchmark(Baseline = true)]
	public Greek EnumParse()
	{
		Greek e = Greek.None;
		if (UseValid)
		{
			foreach (string s in ValidValues)
			{
				if (!Enum.TryParse(s, IgnoreCase, out e))
					throw new Exception("Invalid.");
			}
		}
		else
		{
			foreach (string s in InvalidValues)
			{
				if (Enum.TryParse(s, IgnoreCase, out e))
					throw new Exception("Valid.");
			}

		}
		return e;
	}

	private static bool TryParseBySwitch(string value, bool ignoreCase, out Greek e)
	{
		if (ignoreCase)
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

			if (value.Equals(nameof(Greek.Cappa), StringComparison.OrdinalIgnoreCase))
			{
				e = Greek.Cappa;
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

		switch (value)
		{
			case nameof(Greek.Alpha):
				e = Greek.Alpha;
				return true;
			case nameof(Greek.Beta):
				e = Greek.Beta;
				return true;
			case nameof(Greek.Cappa):
				e = Greek.Cappa;
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

	private static bool TryParseByLengthSwitch(string value, bool ignoreCase, out Greek e)
	{
		var len = value.Length;
		switch (len)
		{
			case 3:
				if (value.Equals(nameof(Greek.Phi), ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
				{
					e = Greek.Phi;
					return true;
				}
				e = default!;
				return false;

			case 4:
				if (ignoreCase)
				{
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
				}

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

				if (ignoreCase)
				{
					if (value.Equals(nameof(Greek.Alpha), StringComparison.OrdinalIgnoreCase))
					{
						e = Greek.Alpha;
						return true;
					}

					if (value.Equals(nameof(Greek.Cappa), StringComparison.OrdinalIgnoreCase))
					{
						e = Greek.Cappa;
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
				}

				switch (value)
				{
					case nameof(Greek.Alpha):
						e = Greek.Alpha;
						return true;
					case nameof(Greek.Cappa):
						e = Greek.Cappa;
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
				if (value.Equals(nameof(Greek.Epsilon), ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
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


	[Benchmark]
	public Greek CompiledSwitch()
	{
		Greek e = Greek.None;
		if (UseValid)
		{
			foreach (string s in ValidValues)
			{
				if (!TryParseBySwitch(s, IgnoreCase, out e))
					throw new Exception("Invalid.");
			}
		}
		else
		{
			foreach (string s in InvalidValues)
			{
				if (TryParseBySwitch(s, IgnoreCase, out e))
					throw new Exception("Valid.");
			}

		}
		return e;
	}

	[Benchmark]
	public Greek CompiledSwitchWithLengths()
	{
		Greek e = Greek.None;
		if (UseValid)
		{
			foreach (string s in ValidValues)
			{
				if (!TryParseByLengthSwitch(s, IgnoreCase, out e))
					throw new Exception("Invalid.");
			}
		}
		else
		{
			foreach (string s in InvalidValues)
			{
				if (TryParseByLengthSwitch(s, IgnoreCase, out e))
					throw new Exception("Valid.");
			}

		}
		return e;
	}

	[Benchmark]
	public Greek EnumValueParse()
	{
		Greek e = Greek.None;
		if (UseValid)
		{
			foreach (string s in ValidValues)
			{
				if (!EnumValue.TryParse(s, IgnoreCase, out e))
					throw new Exception("Invalid.");
			}
		}
		else
		{
			foreach (string s in InvalidValues)
			{
				if (EnumValue.TryParse(s, IgnoreCase, out e))
					throw new Exception("Valid.");
			}

		}
		return e;
	}


	[Benchmark]
	public Greek FastEnumParse()
	{
		Greek e = Greek.None;
		if (UseValid)
		{
			foreach (string s in ValidValues)
			{
				if (!FastEnum.TryParse(s, IgnoreCase, out e))
					throw new Exception("Invalid.");
			}
		}
		else
		{
			foreach (string s in InvalidValues)
			{
				if (FastEnum.TryParse(s, IgnoreCase, out e))
					throw new Exception("Valid.");
			}

		}
		return e;
	}
}
