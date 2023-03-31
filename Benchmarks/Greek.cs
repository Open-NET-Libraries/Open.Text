namespace Open.Text.Benchmarks;

[AttributeUsage(AttributeTargets.Field)]
public class LetterAttribute : Attribute
{
	public LetterAttribute(char upper, char lower)
	{
		Upper = upper;
		Lower = lower;
	}

	public char Upper { get; }
	public char Lower { get; }

	public bool EqualsLetter(char letter)
		=> letter == Upper || letter == Lower;
}

public enum Greek
{
	[Letter('Α', 'α')]
	Alpha,
	[Letter('Β', 'β')]
	Beta,
	[Letter('b', 'b')]
	Beta2,
	[Letter('Κ', 'κ')]
	Kappa,
	[Letter('Δ', 'δ')]
	Delta,
	[Letter('Ε', 'ε')]
	Epsilon,
	[Letter('Γ', 'γ')]
	Gamma,
	[Letter('Ω', 'ω')]
	Omega,
	[Letter('Φ', 'φ')]
	Phi,
	[Letter('Θ', 'θ')]
	Theta,
	None
}
