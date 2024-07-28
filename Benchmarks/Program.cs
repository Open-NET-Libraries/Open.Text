using BenchmarkDotNet.Running;
using Open.Text.Benchmarks;

//BenchmarkSwitcher
//	.FromAssembly(typeof(Program).Assembly)
//	.Run(args); // crucial to make it work

//BenchmarkRunner.Run<EnumParseTests>();
//BenchmarkRunner.Run<EnumToStringTests>();
//BenchmarkRunner.Run<CharAssumptionTests>();
//BenchmarkRunner.Run<EnumAttributeTests>();
//BenchmarkRunner.Run<IsDefinedTests>();
//BenchmarkRunner.Run<StringConcatTests>();
BenchmarkRunner.Run<IndexOfTests>();
