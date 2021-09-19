using BenchmarkDotNet.Attributes;
using FastEnumUtility;

namespace Open.Text.Benchmarks
{

	public enum Greek
	{
		Alpha, Beta, Cappa, Delta, Epsilon, Gamma, Omega, Phi, Theta, None
	}

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
}
