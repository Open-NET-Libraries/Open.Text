using ZLinq;

namespace Open.Text.Benchmarks;

/// <summary>
/// Quick test to demonstrate current allocation behavior of SplitAsSegments
/// Run with: dotnet run -c Release --project Benchmarks/Open.Text.Benchmarks.csproj --quicktest
/// </summary>
public static class SplitAsSegmentsQuickTest
{
	public static void Run()
	{
		Console.WriteLine("=== SplitAsSegments Allocation Test ===\n");

		const string testString = "apple,banana,cherry,date,elderberry";

		// Test 1: Simple foreach
		Console.WriteLine("Test 1: Simple foreach (only counting)");
		long gen0Before = GC.CollectionCount(0);
		long memBefore = GC.GetTotalMemory(forceFullCollection: true);

		int count = 0;
		for (int i = 0; i < 10000; i++)
		{
			foreach (var segment in testString.AsSegment().SplitAsSegments(','))
			{
				count += segment.Length;
			}
		}

		long memAfter = GC.GetTotalMemory(forceFullCollection: false);
		long gen0After = GC.CollectionCount(0);

		Console.WriteLine($"  Result: {count}");
		Console.WriteLine($"  Memory allocated: ~{(memAfter - memBefore) / 1024}KB");
		Console.WriteLine($"  Gen0 collections: {gen0After - gen0Before}");
		Console.WriteLine($"  Avg per iteration: ~{(memAfter - memBefore) / 10000} bytes\n");

		// Test 2: With LINQ Count()
		Console.WriteLine("Test 2: With LINQ Count()");
		gen0Before = GC.CollectionCount(0);
		memBefore = GC.GetTotalMemory(forceFullCollection: true);

		int totalCount = 0;
		for (int i = 0; i < 10000; i++)
		{
			totalCount += testString.AsSegment().SplitAsSegments(',').Count();
		}

		memAfter = GC.GetTotalMemory(forceFullCollection: false);
		gen0After = GC.CollectionCount(0);

		Console.WriteLine($"  Result: {totalCount}");
		Console.WriteLine($"  Memory allocated: ~{(memAfter - memBefore) / 1024}KB");
		Console.WriteLine($"  Gen0 collections: {gen0After - gen0Before}");
		Console.WriteLine($"  Avg per iteration: ~{(memAfter - memBefore) / 10000} bytes\n");

		// Test 3: ToArray()
		Console.WriteLine("Test 3: With ToArray()");
		gen0Before = GC.CollectionCount(0);
		memBefore = GC.GetTotalMemory(forceFullCollection: true);

		int arrayCount = 0;
		for (int i = 0; i < 10000; i++)
		{
			var arr = testString.AsSegment().SplitAsSegments(',').ToArray();
			arrayCount += arr.Length;
		}

		memAfter = GC.GetTotalMemory(forceFullCollection: false);
		gen0After = GC.CollectionCount(0);

		Console.WriteLine($"  Result: {arrayCount}");
		Console.WriteLine($"  Memory allocated: ~{(memAfter - memBefore) / 1024}KB");
		Console.WriteLine($"  Gen0 collections: {gen0After - gen0Before}");
		Console.WriteLine($"  Avg per iteration: ~{(memAfter - memBefore) / 10000} bytes\n");

		// Show expected baseline
		Console.WriteLine("Expected baseline:");
		Console.WriteLine("  - yield return creates ~32-64 byte enumerator object per call");
		Console.WriteLine("  - 10,000 iterations = ~320-640KB minimum allocation");
		Console.WriteLine("  - Additional allocations for LINQ operations\n");

		Console.WriteLine("Goal: Reduce to ZERO allocations for direct foreach iteration");
	}
}
