using System;

using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

static class Paths
{
	public const string DotNetCLIx64 = @"c:\program files\dotnet\dotnet.exe";
	public const string DotNetCLIx86 = @"c:\program files (x86)\dotnet\dotnet.exe";
}

static class BenchData
{
	public const int HashBytes = 5;

	public static readonly byte[] Key = Array.Empty<byte>();
	public static readonly byte[] Data = new byte[1024 * 1024 * 10].RandomFill();

	public static byte[] RandomFill(this byte[] a)
	{
		new Random(42).NextBytes(a);
		return a;
	}
}

class Program
{
	static void Main(string[] args)
	{
		BenchmarkRunner.Run<Blake2Bench>(new MultipleJitConfig());
	}
}

public class Blake2Bench
{

	[Benchmark(Description = "MixNoop")]
	public static byte[] Blake2bNoop()
	{
		return Blake2b.ComputeHash(BenchData.HashBytes, BenchData.Key, BenchData.Data, MixMode.Noop);
	}

	[Benchmark(Description = "MixManualInline")]
	public static byte[] Blake2bManualInline()
	{
		return Blake2b.ComputeHash(BenchData.HashBytes, BenchData.Key, BenchData.Data, MixMode.Inlined);
	}

	[Benchmark(Description = "MixPreferred")]
	public static byte[] Blake2bPreferred()
	{
		return Blake2b.ComputeHash(BenchData.HashBytes, BenchData.Key, BenchData.Data, MixMode.Preferred);
	}
}
