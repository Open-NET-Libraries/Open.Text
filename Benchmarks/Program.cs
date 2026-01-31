using BenchmarkDotNet.Running;
using Open.Text.Benchmarks;

// Quick allocation test if --quicktest argument provided
if (args.Length > 0 && args.Contains("--quicktest"))
{
	SplitAsSegmentsQuickTest.Run();
	return;
}

//BenchmarkSwitcher
//	.FromAssembly(typeof(Program).Assembly)
//	.Run(args); // crucial to make it work

//BenchmarkRunner.Run<EnumParseTests>();
//BenchmarkRunner.Run<EnumToStringTests>();
//BenchmarkRunner.Run<CharAssumptionTests>();
//BenchmarkRunner.Run<EnumAttributeTests>();
//BenchmarkRunner.Run<IsDefinedTests>();
//BenchmarkRunner.Run<StringConcatTests>();
//BenchmarkRunner.Run<SplitAsSegmentsBaselineBenchmark>();
BenchmarkRunner.Run<SplitAllocationBenchmark>();
