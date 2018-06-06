using System;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

static class Paths
{
	public const string DotNetCLIx64 = @"c:\program files\dotnet\dotnet.exe";
	public const string DotNetCLIx86 = @"c:\program files (x86)\dotnet\dotnet.exe";
	public const string BlakeNativeDLLx64 = @"c:\gitlocal\blake2fast\tests\blake2.bench\nativebin\x64";
	public const string BlakeNativeDLLx86 = @"c:\gitlocal\blake2fast\tests\blake2.bench\nativebin\x86";
}

static class BenchConfig
{
	public const int HashBytes = 5;

	public static readonly byte[] Key = Array.Empty<byte>();
	//public static readonly byte[] Key = System.Text.Encoding.ASCII.GetBytes("abc");

	//public static readonly byte[] Data = Array.Empty<byte>();
	//public static readonly byte[] Data = System.Text.Encoding.ASCII.GetBytes("abc");
	//public static readonly byte[] Data = System.IO.File.ReadAllBytes(@"c:\windows\system32\spool\drivers\color\srgb.icm");
	public static readonly byte[] Data = new byte[1024 * 1024 * 10].RandomFill();

	public static string ToHexString(this byte[] a) => String.Concat(a.Select(x => x.ToString("X2")));

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
		//Console.WriteLine(Blake2Rfc.SelftTest.blake2b_selftest());
		//Console.WriteLine(Blake2Rfc.SelftTest.blake2s_selftest());
		//singleRuns();

		BenchmarkRunner.Run<Blake2Bench>(new MultipleJitConfig().With(new CategoryFilter("JitTest")));
		//BenchmarkRunner.Run<Blake2Bench>(new AllowNonOptimizedConfig().With(new CategoryFilter("OtherHash")));
		//BenchmarkRunner.Run<Blake2Bench>(new AllowNonOptimizedConfig().With(new CategoryFilter("Blake2b")));
		//BenchmarkRunner.Run<Blake2Bench>(new AllowNonOptimizedConfig().With(new CategoryFilter("Blake2s")));
	}

	static void singleRuns()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			Console.WriteLine(Blake2Bench.GetHashNativeRef().ToHexString());
			Console.WriteLine(Blake2Bench.GetHashNativeSSE().ToHexString());
			//Console.WriteLine(Blake2Bench.GetHashNativeAVX().ToHexString()); //will crash if processor doesn't support AVX2
		}
		Console.WriteLine(Blake2Bench.GetHashBlake2bRfc().ToHexString());
		Console.WriteLine();
#if !NETFRAMEWORK
		Console.WriteLine(Blake2Bench.GetHashBlake2Core().ToHexString());
#endif
		Console.WriteLine(Blake2Bench.GetHashKSB2().ToHexString());
#if NETCOREAPP2_1
		Console.WriteLine(Blake2Bench.GetHashNSec().ToHexString());
#endif
		Console.WriteLine();

		Console.WriteLine(Blake2Bench.GetHashBlake2Sharp().ToHexString());
		Console.WriteLine(Blake2Bench.GetHashBlake2bFast().ToHexString());
		Console.WriteLine(Blake2Bench.GetHashDHB2().ToHexString());
		//Console.WriteLine(Blake2Bench.GetHashICB2().ToHexString()); //disabled due to rogue Console.WriteLine in library

		Console.WriteLine();

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			Console.WriteLine(Blake2Bench.GetHashsNativeRef().ToHexString());
			Console.WriteLine(Blake2Bench.GetHashsNativeSSE().ToHexString());
		}
		Console.WriteLine(Blake2Bench.GetHashBlake2sRfc().ToHexString());
		Console.WriteLine();

		Console.WriteLine(Blake2Bench.GetHashBlake2sFast().ToHexString());
#if !NETCOREAPP1_1
		Console.WriteLine(Blake2Bench.GetHash2snet().ToHexString());
#endif
	}
}

public class Blake2Bench
{
	/*
	 *
	 * BLAKE2B
	 *
	 */
	//[Benchmark(Description = "Blake2bRefNative"), BenchmarkCategory("Blake2b")]
	public static byte[] GetHashNativeRef()
	{
		var h = new byte[BenchConfig.HashBytes];
		Interop.Blake2bNativeRef(h, new IntPtr(h.Length), BenchConfig.Data, new IntPtr(BenchConfig.Data.Length), BenchConfig.Key, new IntPtr(BenchConfig.Key.Length));
		return h;
	}

	//[Benchmark(Description = "Blake2bSseNative"), BenchmarkCategory("Blake2b")]
	public static byte[] GetHashNativeSSE()
	{
		var h = new byte[BenchConfig.HashBytes];
		Interop.Blake2bNativeSSE41(h, new IntPtr(h.Length), BenchConfig.Data, new IntPtr(BenchConfig.Data.Length), BenchConfig.Key, new IntPtr(BenchConfig.Key.Length));
		return h;
	}

	//[Benchmark(Description = "Blake2AvxNative"), BenchmarkCategory("Blake2b")]
	//public unsafe static byte[] GetHashNativeAVX()
	//{
	//	var h = new byte[BenchConfig.HashBytes];
	//	Interop.Blake2bNativeAVX2(h, new IntPtr(h.Length), BenchConfig.Data, new IntPtr(BenchConfig.Data.Length));
	//	return h;
	//}

	//[Benchmark(Description = "Blake2bRFC"), BenchmarkCategory("Blake2b")]
	public static byte[] GetHashBlake2bRfc()
	{
		return Blake2Rfc.Blake2b.ComputeHash(BenchConfig.HashBytes, BenchConfig.Key, BenchConfig.Data);
	}

	[Benchmark(Description = "Blake2Sharp"), BenchmarkCategory("Blake2b")]
	public static byte[] GetHashBlake2Sharp()
	{
		var cfg = new Blake2Sharp.Blake2BConfig() {
			OutputSizeInBytes = BenchConfig.HashBytes,
			Key = BenchConfig.Key
		};
		return Blake2Sharp.Blake2B.ComputeHash(BenchConfig.Data, cfg);
	}

	[Benchmark(Description = "Blake2bFast"), BenchmarkCategory("Blake2b", "JitTest", "OtherHash")]
	public static byte[] GetHashBlake2bFast()
	{
		return SauceControl.Blake2Fast.Blake2b.ComputeHash(BenchConfig.HashBytes, BenchConfig.Key, BenchConfig.Data);
	}

#if !NETFRAMEWORK
	//[Benchmark(Description = "Blake2Core"), BenchmarkCategory("Blake2b")]
	public static byte[] GetHashBlake2Core()
	{
		var cfg = new Blake2Core.Blake2BConfig() {
			OutputSizeInBytes = BenchConfig.HashBytes,
			Key = BenchConfig.Key
		};
		return Blake2Core.Blake2B.ComputeHash(BenchConfig.Data, cfg);
	}
#endif

	//[Benchmark(Description = "S.D.HashFunction"), BenchmarkCategory("Blake2b")]
	public static byte[] GetHashDHB2()
	{
		var cfg = new System.Data.HashFunction.Blake2.Blake2BConfig() {
			HashSizeInBits = BenchConfig.HashBytes * 8,
			Key = BenchConfig.Key
		};
		var b2 = System.Data.HashFunction.Blake2.Blake2BFactory.Instance.Create(cfg);
		return b2.ComputeHash(BenchConfig.Data).Hash;
	}

	//[Benchmark(Description = "Konscious"), BenchmarkCategory("Blake2b")]
	public static byte[] GetHashKSB2()
	{
		var b2 = new Konscious.Security.Cryptography.HMACBlake2B(BenchConfig.Key, BenchConfig.HashBytes *  8);
		return b2.ComputeHash(BenchConfig.Data);
	}

	//[Benchmark(Description = "Isopoh"), BenchmarkCategory("Blake2b")]
	public static byte[] GetHashICB2()
	{
		var cfg = new Isopoh.Cryptography.Blake2b.Blake2BConfig() {
			OutputSizeInBytes = BenchConfig.HashBytes,
			Key = BenchConfig.Key
		};
		return Isopoh.Cryptography.Blake2b.Blake2B.ComputeHash(BenchConfig.Data, cfg, null);
	}

#if NETCOREAPP2_1
	//[Benchmark(Description = "NSec"), BenchmarkCategory("Blake2b")]
	public static byte[] GetHashNSec()
	{
		// this implementation won't do hashes less than 256 bits, so just return 0's for shorter hashes
		var b2 = new NSec.Cryptography.Blake2b(Math.Max(BenchConfig.HashBytes, 32));
		var h = b2.Hash(BenchConfig.Data);
		return BenchConfig.HashBytes >= 32 ? h : new byte[BenchConfig.HashBytes];
	}
#endif

/*
 *
 * BLAKE2S
 *
 */
	//[Benchmark(Description = "Blake2sRefNative"), BenchmarkCategory("Blake2s")]
	public static byte[] GetHashsNativeRef()
	{
		var h = new byte[BenchConfig.HashBytes];
		Interop.Blake2sNativeRef(h, new IntPtr(h.Length), BenchConfig.Data, new IntPtr(BenchConfig.Data.Length), BenchConfig.Key, new IntPtr(BenchConfig.Key.Length));
		return h;
	}

	//[Benchmark(Description = "Blake2sSseNative"), BenchmarkCategory("Blake2s")]
	public static byte[] GetHashsNativeSSE()
	{
		var h = new byte[BenchConfig.HashBytes];
		Interop.Blake2sNativeSSE41(h, new IntPtr(h.Length), BenchConfig.Data, new IntPtr(BenchConfig.Data.Length), BenchConfig.Key, new IntPtr(BenchConfig.Key.Length));
		return h;
	}

	//[Benchmark(Description = "Blake2sRFC"), BenchmarkCategory("Blake2s")]
	public static byte[] GetHashBlake2sRfc()
	{
		return Blake2Rfc.Blake2s.ComputeHash(BenchConfig.HashBytes, BenchConfig.Key, BenchConfig.Data);
	}

#if !NETCOREAPP1_1
	[Benchmark(Description = "Blake2s-net"), BenchmarkCategory("Blake2s")]
	public static byte[] GetHash2snet()
	{
		var cfg = new Blake2s.Blake2sConfig() {
			OutputSizeInBytes = BenchConfig.HashBytes,
			Key = BenchConfig.Key
		};
		return Blake2s.Blake2S.ComputeHash(BenchConfig.Data, cfg);
	}
#endif

	[Benchmark(Description = "Blake2sFast"), BenchmarkCategory("Blake2s", "JitTest", "OtherHash")]
	public static byte[] GetHashBlake2sFast()
	{
		return SauceControl.Blake2Fast.Blake2s.ComputeHash(BenchConfig.HashBytes, BenchConfig.Key, BenchConfig.Data);
	}

/*
 *
 * OTHERS
 *
 */
	[Benchmark(Description = "MD5"), BenchmarkCategory("OtherHash")]
	public static byte[] GetHashMD5()
	{
		using (var md5 = MD5.Create())
			return md5.ComputeHash(BenchConfig.Data);
	}

	[Benchmark(Description = "SHA256"), BenchmarkCategory("OtherHash")]
	public static byte[] GetHashSha256()
	{
		using (var sha = SHA256.Create())
			return sha.ComputeHash(BenchConfig.Data);
	}

	[Benchmark(Description = "SHA512"), BenchmarkCategory("OtherHash")]
	public static byte[] GetHashSha512()
	{
		using (var sha = SHA512.Create())
			return sha.ComputeHash(BenchConfig.Data);
	}
}
