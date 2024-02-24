namespace Open.Text.Benchmarks;

[AttributeUsage(AttributeTargets.Field)]
public class LetterAttribute(char upper, char lower) : Attribute
{
	public char Upper { get; } = upper;
	public char Lower { get; } = lower;

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
