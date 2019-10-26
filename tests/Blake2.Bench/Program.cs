using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

static class Paths
{
	public const string DotNetCLIx64 = @"c:\program files\dotnet\dotnet.exe";
	public const string DotNetCLIx86 = @"c:\program files (x86)\dotnet\dotnet.exe";
}

static class BenchConfig
{
	public const int HashBytes = 5;
	public static readonly byte[] Key = Array.Empty<byte>();
	//public static readonly byte[] Key = new[] { (byte)'a', (byte)'b', (byte)'c' };

	public static readonly List<byte[]> Data = new List<byte[]>
	{
		new[] { (byte)'a', (byte)'b', (byte)'c' },
		new byte[3268].RandomFill(), // size of Windows 10 srgb.icm
		new byte[1024 * 1024 * 3].RandomFill()
	};

	public static string ToHexString(this byte[] a) => string.Concat(a.Select(x => x.ToString("X2")));

	public static byte[] RandomFill(this byte[] a)
	{
		new Random(42).NextBytes(a);
		return a;
	}
}

class Program
{
	public static void Main()
	{
		if (!Blake2Rfc.SelftTest.blake2b_selftest() || !Blake2Rfc.SelftTest.blake2s_selftest())
		{
			Console.WriteLine("Error: RFC self-test failed");
			return;
		}

		Console.WriteLine(@"Choose a benchmark:

0. Just test each BLAKE2 library once, don't benchmark
1. Blake2Fast vs .NET in-box algorithms (MD5 and SHA2)
2. Blake2Fast BLAKE2b vs 3rd party libraries
3. Blake2Fast BLAKE2s vs 3rd party libraries
4. Blake2Fast performance on multiple runtimes (check SDK paths in Program.cs)
");
		switch (Console.ReadKey().Key)
		{
			case ConsoleKey.D0:
				singleRun();
				break;
			case ConsoleKey.D1:
				BenchmarkRunner.Run<Blake2Bench>(new AllowNonOptimizedConfig(false).With(new AllCategoriesFilter(new[] { "OtherHash" })));
				break;
			case ConsoleKey.D2:
				BenchmarkRunner.Run<Blake2Bench>(new AllowNonOptimizedConfig().With(new AllCategoriesFilter(new[] { "Blake2b" })));
				break;
			case ConsoleKey.D3:
				BenchmarkRunner.Run<Blake2Bench>(new AllowNonOptimizedConfig().With(new AllCategoriesFilter(new[] { "Blake2s" })));
				break;
			case ConsoleKey.D4:
				BenchmarkRunner.Run<Blake2Bench>(new MultipleJitConfig().With(new AllCategoriesFilter(new[] { "JitTest" })));
				break;
			default:
				Console.WriteLine("Unrecognized command.");
				break;
		}
	}

	private static void singleRun()
	{
		var bench = new Blake2Bench();

		// BLAKE2B
		Console.WriteLine();
		Console.WriteLine(bench.GetHashBlake2bRfc(BenchConfig.Data.Last()).ToHexString());
		Console.WriteLine(bench.GetHashBlake2bFast(BenchConfig.Data.Last()).ToHexString());

		Console.WriteLine(bench.GetHashBlake2Sharp(BenchConfig.Data.Last()).ToHexString());
		Console.WriteLine(bench.GetHashByteTerrace2b(BenchConfig.Data.Last()).ToHexString());
		Console.WriteLine(bench.GetHashDHB2(BenchConfig.Data.Last()).ToHexString());
		Console.WriteLine(bench.GetHashICB2(BenchConfig.Data.Last()).ToHexString());
		Console.WriteLine(bench.GetHashKSB2(BenchConfig.Data.Last()).ToHexString());

#if !NETFRAMEWORK
		Console.WriteLine(bench.GetHashBlake2Core(BenchConfig.Data.Last()).ToHexString());
		Console.WriteLine(bench.GetHashNSec(BenchConfig.Data.Last()).ToHexString()); // not RFC-compliant -- result will be all 0s when digest size < default
#endif

		// BLAKE2S
		Console.WriteLine();
		Console.WriteLine(bench.GetHashBlake2sRfc(BenchConfig.Data.Last()).ToHexString());
		Console.WriteLine(bench.GetHashBlake2sFast(BenchConfig.Data.Last()).ToHexString());

		Console.WriteLine(bench.GetHash2snet(BenchConfig.Data.Last()).ToHexString());
		Console.WriteLine(bench.GetHashByteTerrace2s(BenchConfig.Data.Last()).ToHexString());
	}
}

public class Blake2Bench
{
	public static IEnumerable<byte[]> Data() => BenchConfig.Data;

	/*
	 *
	 * BLAKE2B
	 *
	 */
	//[Benchmark(Description = "Blake2bRFC"), BenchmarkCategory("Blake2b")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashBlake2bRfc(byte[] data)
	{
		return Blake2Rfc.Blake2b.ComputeHash(BenchConfig.HashBytes, BenchConfig.Key, data);
	}

	[Benchmark(Description = "*Blake2Fast.Blake2b"), BenchmarkCategory("Blake2b", "JitTest")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashBlake2bFast(byte[] data)
	{
		return Blake2Fast.Blake2b.ComputeHash(BenchConfig.HashBytes, BenchConfig.Key, data);
	}

	[Benchmark(Description = "Blake2Sharp"), BenchmarkCategory("Blake2b")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashBlake2Sharp(byte[] data)
	{
		var cfg = new Blake2Sharp.Blake2BConfig() {
			OutputSizeInBytes = BenchConfig.HashBytes,
			Key = BenchConfig.Key
		};
		return Blake2Sharp.Blake2B.ComputeHash(data, cfg);
	}

	[Benchmark(Description = "ByteTerrace"), BenchmarkCategory("Blake2b")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashByteTerrace2b(byte[] data)
	{
		var b2b = ByteTerrace.Maths.Cryptography.Blake2b.New(BenchConfig.Key, BenchConfig.HashBytes);
		return b2b.ComputeHash(data);
	}

	[Benchmark(Description = "S.D.HashFunction"), BenchmarkCategory("Blake2b")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashDHB2(byte[] data)
	{
		var cfg = new System.Data.HashFunction.Blake2.Blake2BConfig() {
			HashSizeInBits = BenchConfig.HashBytes * 8,
			Key = BenchConfig.Key
		};
		var b2 = System.Data.HashFunction.Blake2.Blake2BFactory.Instance.Create(cfg);
		return b2.ComputeHash(data).Hash;
	}

	[Benchmark(Description = "Konscious"), BenchmarkCategory("Blake2b")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashKSB2(byte[] data)
	{
		using var b2 = new Konscious.Security.Cryptography.HMACBlake2B(BenchConfig.Key, BenchConfig.HashBytes *  8);
		return b2.ComputeHash(data);
	}

	[Benchmark(Description = "Isopoh"), BenchmarkCategory("Blake2b")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashICB2(byte[] data)
	{
		var cfg = new Isopoh.Cryptography.Blake2b.Blake2BConfig() {
			OutputSizeInBytes = BenchConfig.HashBytes,
			Key = BenchConfig.Key
		};
		return Isopoh.Cryptography.Blake2b.Blake2B.ComputeHash(data, cfg, null);
	}

#if !NETFRAMEWORK
	[Benchmark(Description = "Blake2Core"), BenchmarkCategory("Blake2b")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashBlake2Core(byte[] data)
	{
		var cfg = new Blake2Core.Blake2BConfig() {
			OutputSizeInBytes = BenchConfig.HashBytes,
			Key = BenchConfig.Key
		};
		return Blake2Core.Blake2B.ComputeHash(data, cfg);
	}

	[Benchmark(Description = "NSec"), BenchmarkCategory("Blake2b")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashNSec(byte[] data)
	{
		// this implementation won't do hashes less than 256 bits, so just return 0s for shorter hashes
		var b2 = new NSec.Cryptography.Blake2b(Math.Max(BenchConfig.HashBytes, 32));
		var h = b2.Hash(data);
		return BenchConfig.HashBytes >= 32 ? h : new byte[BenchConfig.HashBytes];
	}
#endif

	/*
	 *
	 * BLAKE2S
	 *
	 */
	//[Benchmark(Description = "Blake2sRFC"), BenchmarkCategory("Blake2s")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashBlake2sRfc(byte[] data)
	{
		return Blake2Rfc.Blake2s.ComputeHash(BenchConfig.HashBytes, BenchConfig.Key, data);
	}

	[Benchmark(Description = "*Blake2Fast.Blake2s"), BenchmarkCategory("Blake2s", "JitTest")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashBlake2sFast(byte[] data)
	{
		return Blake2Fast.Blake2s.ComputeHash(BenchConfig.HashBytes, BenchConfig.Key, data);
	}

	[Benchmark(Description = "Blake2s-net"), BenchmarkCategory("Blake2s")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHash2snet(byte[] data)
	{
		var cfg = new Blake2s.Blake2sConfig() {
			OutputSizeInBytes = BenchConfig.HashBytes,
			Key = BenchConfig.Key
		};
		return Blake2s.Blake2S.ComputeHash(data, cfg);
	}

	[Benchmark(Description = "ByteTerrace"), BenchmarkCategory("Blake2s")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashByteTerrace2s(byte[] data)
	{
		var b2s = ByteTerrace.Maths.Cryptography.Blake2s.New(BenchConfig.Key, BenchConfig.HashBytes);
		return b2s.ComputeHash(data);
	}

	/*
	 *
	 * BLAKE2 vs OTHERS
	 *
	 */
	[Benchmark(Description = "BLAKE2-256"), BenchmarkCategory("OtherHash")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashBlake2s256(byte[] data)
	{
		return Blake2Fast.Blake2s.ComputeHash(data);
	}

	[Benchmark(Description = "BLAKE2-512"), BenchmarkCategory("OtherHash")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashBlake2b256(byte[] data)
	{
		return Blake2Fast.Blake2b.ComputeHash(data);
	}

	[Benchmark(Description = "MD5"), BenchmarkCategory("OtherHash")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashMD5(byte[] data)
	{
		using var md5 = MD5.Create();
		return md5.ComputeHash(data);
	}

	[Benchmark(Description = "SHA-256"), BenchmarkCategory("OtherHash")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashSha256(byte[] data)
	{
		using var sha = SHA256.Create();
		return sha.ComputeHash(data);
	}

	[Benchmark(Description = "SHA-512"), BenchmarkCategory("OtherHash")]
	[ArgumentsSource(nameof(Data))]
	public byte[] GetHashSha512(byte[] data)
	{
		using var sha = SHA512.Create();
		return sha.ComputeHash(data);
	}
}
