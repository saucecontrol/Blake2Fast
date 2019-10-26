[![NuGet](https://buildstats.info/nuget/SauceControl.Blake2Fast)](https://www.nuget.org/packages/SauceControl.Blake2Fast/) [![Build Status](https://dev.azure.com/saucecontrol/Blake2Fast/_apis/build/status/saucecontrol.Blake2Fast?branchName=master)](https://dev.azure.com/saucecontrol/Blake2Fast/_build/latest?definitionId=3&branchName=master) [![Test Results](https://img.shields.io/azure-devops/tests/saucecontrol/Blake2Fast/3?logo=azure-devops)](https://dev.azure.com/saucecontrol/Blake2Fast/_build/latest?definitionId=3&branchName=master) [![CI NuGet](https://img.shields.io/badge/nuget-CI%20builds-4da2db?logo=azure-devops)](https://dev.azure.com/saucecontrol/Blake2Fast/_packaging?_a=feed&feed=blake2fast_ci)

Blake2Fast
==========

These [RFC 7693](https://tools.ietf.org/html/rfc7693)-compliant BLAKE2 implementations have been tuned for high speed and low memory usage.  The .NET Core 2.1 and 3.0 builds use the new x86 SIMD Intrinsics for even greater speed.  `Span<byte>` is used throughout for lower memory overhead compared to `byte[]` based APIs.

On .NET Core 2.1, Blake2Fast uses an SSE4.1 SIMD-accelerated implementation for both BLAKE2b and BLAKE2s.

On .NET Core 3.0, a faster AVX2 implementation of BLAKE2b is available (with SSE4.1 fallback for older processors), while BLAKE2s uses the same SSE4.1 implementation.


Installation
------------

Blake2Fast is available on [NuGet](https://www.nuget.org/packages/SauceControl.Blake2Fast/)

```
PM> Install-Package SauceControl.Blake2Fast
```

Usage
-----

### All-at-Once Hashing

The simplest way to calculate a hash is the all-at-once `ComputeHash` method.

```C#
var hash = Blake2b.ComputeHash(data);
```

BLAKE2 supports variable digest lengths from 1 to 32 bytes for BLAKE2s or 1 to 64 bytes for BLAKE2b.

```C#
var hash = Blake2b.ComputeHash(42, data);
```

BLAKE2 also natively supports keyed hashing.

```C#
var hash = Blake2b.ComputeHash(key, data);
```

### Incremental Hashing

BLAKE2 hashes can be incrementally updated if you do not have the data available all at once.

```C#
async Task<byte[]> ComputeHashAsync(Stream data)
{
    var hasher = Blake2b.CreateIncrementalHasher();
    var buffer = ArrayPool<byte>.Shared.Rent(4096);

    int bytesRead;
    while ((bytesRead = await data.ReadAsync(buffer, 0, buffer.Length)) > 0)
        hasher.Update(new Span<byte>(buffer, 0, bytesRead));

    ArrayPool<byte>.Shared.Return(buffer);
    return hasher.Finish();
}
```

### Allocation-Free Hashing

The output hash digest can be written to an existing buffer to avoid allocating a new array each time.

```C#
Span<byte> buffer = stackalloc byte[Blake2b.DefaultDigestLength];
Blake2b.ComputeAndWriteHash(data, buffer);
```

This is especially useful when performing an iterative hash, as might be used in a [key derivation function](https://en.wikipedia.org/wiki/Key_derivation_function).

```C#
byte[] DeriveBytes(string password, ReadOnlySpan<byte> salt)
{
    // Create key from password, then hash the salt using the key
    var pwkey = Blake2b.ComputeHash(Encoding.UTF8.GetBytes(password));
    var hbuff = Blake2b.ComputeHash(pwkey, salt);

    // Hash the hash lots of times, re-using the same buffer
    for (int i = 0; i < 999_999; i++)
        Blake2b.ComputeAndWriteHash(pwkey, hbuff, hbuff);

    return hbuff;
}
```

### System.Security.Cryptography Interop

For interoperating with code that uses `System.Security.Cryptography` primitives, Blake2Fast can create a `HashAlgorithm` wrapper.  The wrapper inherits from `HMAC` in case keyed hashing is required.

`HashAlgorithm` is less efficient than the above methods, so use it only when necessary for compatibility.

```C#
byte[] WriteDataAndCalculateHash(byte[] data, string outFile)
{
    using (var hashAlg = Blake2b.CreateHashAlgorithm())
    using (var fileStream = new FileStream(outFile, FileMode.Create))
    using (var cryptoStream = new CryptoStream(fileStream, hashAlg, CryptoStreamMode.Write))
    {
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();
        return hashAlg.Hash;
    }
}
```

SIMD Intrinsics Warning
-----------------------

**This warning applies only to .NET Core 2.1**; the older build targets use only the scalar code, and SIMD intrinsics are fully supported on .NET Core 3.0.

The x86 SIMD Intrinsics used in the .NET Core 2.1 build are not officially supported by Microsoft.  Although the specific SSE Intrinsics used by Blake2Fast have been well-tested, the JIT support for the x86 Intrinsics in general is experimental in .NET Core 2.1.

If you are uncomfortable using unsupported functionality, you can make a custom build of Blake2Fast by removing the `HWINTRINSICS` define constant for the `netcoreapp2.1` target in the [project file](src/Blake2Fast/Blake2Fast.csproj).


Benchmarks
----------

Sample results from the [Blake2.Bench](tests/Blake2.Bench) project.  Benchmarks were run on the .NET Core 3.0-preview7 x64 runtime.  Configuration below:

``` ini
BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100
  [Host]   : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
  ShortRun : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  WarmupCount=3
```

### Blake2Fast vs .NET in-box algorithms (MD5 and SHA2)

```
|     Method | Data Length |            Mean |          Error |         StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------- |------------:|----------------:|---------------:|---------------:|-------:|------:|------:|----------:|
| BLAKE2-256 |           3 |        115.2 ns |       4.012 ns |      0.2199 ns | 0.0134 |     - |     - |      56 B |
| BLAKE2-512 |           3 |        158.2 ns |       6.010 ns |      0.3295 ns | 0.0210 |     - |     - |      88 B |
|        MD5 |           3 |        594.4 ns |     125.147 ns |      6.8597 ns | 0.0496 |     - |     - |     208 B |
|    SHA-256 |           3 |        750.7 ns |      12.582 ns |      0.6897 ns | 0.0572 |     - |     - |     240 B |
|    SHA-512 |           3 |        793.2 ns |     145.418 ns |      7.9708 ns | 0.0725 |     - |     - |     304 B |
|            |             |                 |                |                |        |       |       |           |
| BLAKE2-256 |        3268 |      4,365.6 ns |     162.196 ns |      8.8905 ns | 0.0076 |     - |     - |      56 B |
| BLAKE2-512 |        3268 |      2,816.7 ns |     237.838 ns |     13.0367 ns | 0.0191 |     - |     - |      88 B |
|        MD5 |        3268 |      6,138.1 ns |     551.232 ns |     30.2149 ns | 0.0458 |     - |     - |     208 B |
|    SHA-256 |        3268 |     13,939.6 ns |  18,730.949 ns |  1,026.7065 ns | 0.0458 |     - |     - |     240 B |
|    SHA-512 |        3268 |      7,986.6 ns |   1,738.739 ns |     95.3062 ns | 0.0610 |     - |     - |     304 B |
|            |             |                 |                |                |        |       |       |           |
| BLAKE2-256 |     3145728 |  4,068,222.1 ns | 464,154.358 ns | 25,441.8666 ns |      - |     - |     - |      56 B |
| BLAKE2-512 |     3145728 |  2,603,172.5 ns |  14,066.567 ns |    771.0360 ns |      - |     - |     - |      88 B |
|        MD5 |     3145728 |  5,346,016.5 ns | 707,026.849 ns | 38,754.5273 ns |      - |     - |     - |     208 B |
|    SHA-256 |     3145728 | 11,527,220.8 ns | 688,897.951 ns | 37,760.8213 ns |      - |     - |     - |     240 B |
|    SHA-512 |     3145728 |  6,920,789.6 ns | 963,166.916 ns | 52,794.4287 ns |      - |     - |     - |     304 B |
```

Note that the built-in cryptographic hash algorithms in .NET Core forward to platform-native libraries for their implementations.  On Windows, this means the implementations are provided by [Windows CNG](https://docs.microsoft.com/en-us/windows/desktop/seccng/cng-portal).  Performance may differ on Linux.

On .NET Framework, only scalar (not SIMD) implementations are available for both BLAKE2 algorithms.  The scalar implementations outperform the built-in .NET algorithms in 64-bit applications, but they are slower for large input data on 32-bit.  The SIMD implementations available in .NET Core are faster than the built-in algorithms on either processor architecture.

### Blake2Fast vs other BLAKE2b implementations available on NuGet

```
|              Method | Data Length |            Mean |             Error |            StdDev |          Median |     Gen 0 |     Gen 1 |     Gen 2 | Allocated |
|-------------------- |------------:|----------------:|------------------:|------------------:|----------------:|----------:|----------:|----------:|----------:|
| *Blake2Fast.Blake2b |           3 |        154.6 ns |         11.265 ns |         0.6174 ns |        154.3 ns |    0.0076 |         - |         - |      32 B |
|         Blake2Sharp |           3 |        399.8 ns |          3.924 ns |         0.2151 ns |        399.8 ns |    0.2065 |         - |         - |     864 B |
|         ByteTerrace |           3 |        465.2 ns |         53.384 ns |         2.9262 ns |        464.5 ns |    0.1087 |         - |         - |     456 B |
|    S.D.HashFunction |           3 |      1,925.3 ns |        228.902 ns |        12.5469 ns |      1,930.8 ns |    0.4158 |         - |         - |    1744 B |
|           Konscious |           3 |      1,312.6 ns |         65.560 ns |         3.5936 ns |      1,310.7 ns |    0.2289 |         - |         - |     960 B |
|              Isopoh |           3 |  5,575,764.3 ns | 29,643,293.882 ns | 1,624,848.9645 ns |  6,245,297.4 ns | 1446.2891 | 1435.0586 | 1435.0586 |   38830 B |
|          Blake2Core |           3 |      1,466.0 ns |         18.951 ns |         1.0388 ns |      1,466.1 ns |    0.2060 |         - |         - |     864 B |
|                NSec |           3 |        195.9 ns |         37.838 ns |         2.0740 ns |        194.9 ns |    0.0267 |         - |         - |     112 B |
|                     |             |                 |                   |                   |                 |           |           |           |           |
| *Blake2Fast.Blake2b |        3268 |      2,816.3 ns |        196.974 ns |        10.7968 ns |      2,817.9 ns |    0.0076 |         - |         - |      32 B |
|         Blake2Sharp |        3268 |      4,607.2 ns |      1,289.570 ns |        70.6857 ns |      4,570.6 ns |    0.2060 |         - |         - |     864 B |
|         ByteTerrace |        3268 |      4,283.0 ns |        155.465 ns |         8.5215 ns |      4,286.1 ns |    0.1068 |         - |         - |     456 B |
|    S.D.HashFunction |        3268 |     30,997.7 ns |        365.501 ns |        20.0344 ns |     30,993.8 ns |    2.1973 |         - |         - |    9344 B |
|           Konscious |        3268 |     17,838.6 ns |      5,256.549 ns |       288.1292 ns |     17,810.2 ns |    0.2136 |         - |         - |     960 B |
|              Isopoh |        3268 |  4,900,439.9 ns | 69,196,704.000 ns | 3,792,904.8400 ns |  3,818,492.5 ns | 1665.5273 | 1652.5879 | 1652.5879 |   44453 B |
|          Blake2Core |        3268 |     21,460.8 ns |      1,331.275 ns |        72.9717 ns |     21,430.9 ns |    0.1831 |         - |         - |     864 B |
|                NSec |        3268 |      2,926.2 ns |         44.080 ns |         2.4162 ns |      2,927.0 ns |    0.0267 |         - |         - |     112 B |
|                     |             |                 |                   |                   |                 |           |           |           |           |
| *Blake2Fast.Blake2b |     3145728 |  2,601,242.8 ns |    106,368.445 ns |     5,830.4134 ns |  2,598,229.7 ns |         - |         - |         - |      32 B |
|         Blake2Sharp |     3145728 |  4,133,410.5 ns |    558,771.342 ns |    30,628.1427 ns |  4,116,710.5 ns |         - |         - |         - |     864 B |
|         ByteTerrace |     3145728 |  3,791,728.8 ns |     15,569.103 ns |       853.3951 ns |  3,791,369.1 ns |         - |         - |         - |     456 B |
|    S.D.HashFunction |     3145728 | 28,587,428.1 ns |  1,431,805.370 ns |    78,482.0838 ns | 28,593,243.8 ns | 1781.2500 |         - |         - | 7472544 B |
|           Konscious |     3145728 | 16,070,360.4 ns |  2,271,284.235 ns |   124,496.7530 ns | 16,009,746.9 ns |         - |         - |         - |     960 B |
|              Isopoh |     3145728 |  4,222,685.9 ns |  1,711,996.562 ns |    93,840.3084 ns |  4,170,207.8 ns |         - |         - |         - |     984 B |
|          Blake2Core |     3145728 | 19,492,347.4 ns |  2,974,640.838 ns |   163,050.1018 ns | 19,413,667.2 ns |         - |         - |         - |     864 B |
|                NSec |     3145728 |  2,681,916.7 ns |    217,008.582 ns |    11,894.9727 ns |  2,675,534.4 ns |         - |         - |         - |     112 B |
```

* (1) `Blake2Sharp` is the reference C# BLAKE2b implementation from the [official BLAKE2 repo](https://github.com/BLAKE2/BLAKE2).  This version is not published to NuGet, so the source is included in the benchmark project directly.
* (2) `ByteTerrace.Maths.Cryptography.Blake2` version 0.0.6.
* (3) `System.Data.HashFunction.Blake2` version 2.0.0.  BLAKE2b only.
* (4) `Konscious.Security.Cryptography.Blake2` version 1.0.9.  BLAKE2b only.
* (5) `Isopoh.Cryptography.Blake2b` version 1.1.2.  Yes, it really is that slow on incomplete block lengths.
* (6) `Blake2Core` version 1.0.0.  This package contains the reference Blake2Sharp code compiled as a debug (unoptimized) build.  BenchmarkDotNet errors in such cases, so the settings were overridden to allow this library to run.
* (7) `NSec.Cryptography` 19.5.0.  This implementation of BLAKE2b is not RFC-compliant in that it does not support digest sizes less than 32 bytes or keyed hashing.  This library forwards to a referenced native library (libsodium), which contains an AVX2 implementation of BLAKE2b.

### Blake2Fast vs other BLAKE2s implementations available on NuGet

```
|              Method | Data Length |           Mean |           Error |         StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------- |------------:|---------------:|----------------:|---------------:|-------:|------:|------:|----------:|
| *Blake2Fast.Blake2s |           3 |       113.2 ns |       0.6107 ns |      0.0335 ns | 0.0076 |     - |     - |      32 B |
|         Blake2s-net |           3 |       275.8 ns |      11.9290 ns |      0.6539 ns | 0.1278 |     - |     - |     536 B |
|         ByteTerrace |           3 |       317.5 ns |      60.3902 ns |      3.3102 ns | 0.0763 |     - |     - |     320 B |
|                     |             |                |                 |                |        |       |       |           |
| *Blake2Fast.Blake2s |        3268 |     4,387.3 ns |     874.4187 ns |     47.9298 ns | 0.0076 |     - |     - |      32 B |
|         Blake2s-net |        3268 |     6,267.0 ns |      71.3469 ns |      3.9108 ns | 0.1221 |     - |     - |     536 B |
|         ByteTerrace |        3268 |     6,522.3 ns |      46.1384 ns |      2.5290 ns | 0.0763 |     - |     - |     320 B |
|                     |             |                |                 |                |        |       |       |           |
| *Blake2Fast.Blake2s |     3145728 | 3,953,215.9 ns |   6,863.9457 ns |    376.2360 ns |      - |     - |     - |      32 B |
|         Blake2s-net |     3145728 | 5,873,522.9 ns | 996,716.8203 ns | 54,633.4122 ns |      - |     - |     - |     536 B |
|         ByteTerrace |     3145728 | 5,980,544.8 ns | 138,455.1551 ns |  7,589.1942 ns |      - |     - |     - |     320 B |
```

* (1) `blake2s-net` version 0.1.0.  This is a conversion of the reference Blake2Sharp code to support BLAKE2s.
* (2) `ByteTerrace.Maths.Cryptography.Blake2` version 0.0.6.

You can find more detailed comparisons between Blake2Fast and other .NET BLAKE2 implementations starting [here](https://photosauce.net/blog/post/fast-hashing-with-blake2-part-1-nuget-is-a-minefield).  The short version is that Blake2Fast is the fastest and lowest-memory version of RFC-compliant BLAKE2 available for .NET.

