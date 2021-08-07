[![NuGet](https://buildstats.info/nuget/SauceControl.Blake2Fast)](https://www.nuget.org/packages/SauceControl.Blake2Fast/) [![Build Status](https://dev.azure.com/saucecontrol/Blake2Fast/_apis/build/status/saucecontrol.Blake2Fast?branchName=master)](https://dev.azure.com/saucecontrol/Blake2Fast/_build/latest?definitionId=3&branchName=master) [![Test Results](https://img.shields.io/azure-devops/tests/saucecontrol/Blake2Fast/3?logo=azure-devops)](https://dev.azure.com/saucecontrol/Blake2Fast/_build/latest?definitionId=3&branchName=master) [![Coverage](https://img.shields.io/azure-devops/coverage/saucecontrol/Blake2Fast/3?logo=azure-devops)](https://dev.azure.com/saucecontrol/Blake2Fast/_build/latest?definitionId=3&branchName=master&view=codecoverage-tab) [![CI NuGet](https://img.shields.io/badge/nuget-CI%20builds-4da2db?logo=azure-devops)](https://dev.azure.com/saucecontrol/Blake2Fast/_packaging?_a=feed&feed=blake2fast_ci)

Blake2Fast
==========

These [RFC 7693](https://tools.ietf.org/html/rfc7693)-compliant BLAKE2 implementations have been tuned for high speed and low memory usage.  The .NET Core builds use the new x86 SIMD Intrinsics for even greater speed.  `Span<byte>` is used throughout for lower memory overhead compared to `byte[]` based APIs.

On .NET Core 3+ and .NET 5+, Blake2Fast includes SIMD-accelerated (SSE2-AVX2) implementations of both BLAKE2b and BLAKE2s.


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
        hasher.Update(buffer.AsSpan(0, bytesRead));

    ArrayPool<byte>.Shared.Return(buffer);
    return hasher.Finish();
}
```

For convenience, the generic `Update<T>()` method accepts any value type that does not contain reference fields, plus arrays and Spans of compatible types.

```C#
byte[] ComputeCompositeHash()
{
    var hasher = Blake2b.CreateIncrementalHasher();

    hasher.Update(42);
    hasher.Update(Math.Pi);
    hasher.Update("I love deadlines. I like the whooshing sound they make as they fly by.".AsSpan());

    return hasher.Finish();
}
```

Be aware that the value passed to `Update` is added to the hash state in its current memory layout, which may differ based on platform (endianness) or struct layout.  Use care when calling `Update` with types other than `byte` if the computed hashes are to be used across application or machine boundaries.

For example, if you are adding a string to the hash state, you may hash the characters in memory layout as shown above, or you may use [`Encoding.GetBytes`](https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding.getbytes) to ensure the string bytes are handled consistently across platforms.

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

Benchmarks
----------

Sample results from the [Blake2.Bench](tests/Blake2.Bench) project.  Benchmarks were run on the .NET Core 3.1 x64 runtime.  Configuration below:

``` ini
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.836 (1909/November2018Update/19H2)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.301
  [Host]   : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT
  ShortRun : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  WarmupCount=3  
```

### Blake2Fast vs .NET in-box algorithms (MD5 and SHA2)

```
|     Method | Data Length |            Mean |         Error |       StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------- |------------:|----------------:|--------------:|-------------:|-------:|------:|------:|----------:|
| BLAKE2-256 |           3 |        106.2 ns |       8.01 ns |      0.44 ns | 0.0134 |     - |     - |      56 B |
| BLAKE2-512 |           3 |        144.2 ns |      30.51 ns |      1.67 ns | 0.0210 |     - |     - |      88 B |
|        MD5 |           3 |        559.2 ns |      89.97 ns |      4.93 ns | 0.0496 |     - |     - |     208 B |
|    SHA-256 |           3 |        722.7 ns |      61.84 ns |      3.39 ns | 0.0572 |     - |     - |     240 B |
|    SHA-512 |           3 |        749.2 ns |      40.06 ns |      2.20 ns | 0.0725 |     - |     - |     304 B |
|            |             |                 |               |              |        |       |       |           |
| BLAKE2-256 |        3268 |      3,933.6 ns |     148.09 ns |      8.12 ns | 0.0076 |     - |     - |      56 B |
| BLAKE2-512 |        3268 |      2,429.7 ns |     107.58 ns |      5.90 ns | 0.0191 |     - |     - |      88 B |
|        MD5 |        3268 |      5,866.8 ns |     171.88 ns |      9.42 ns | 0.0458 |     - |     - |     208 B |
|    SHA-256 |        3268 |     12,719.1 ns |     559.17 ns |     30.65 ns | 0.0458 |     - |     - |     240 B |
|    SHA-512 |        3268 |      7,577.3 ns |     555.80 ns |     30.47 ns | 0.0610 |     - |     - |     304 B |
|            |             |                 |               |              |        |       |       |           |
| BLAKE2-256 |     3145728 |  3,667,519.1 ns |  77,804.44 ns |  4,264.72 ns |      - |     - |     - |      56 B |
| BLAKE2-512 |     3145728 |  2,240,879.0 ns | 101,729.66 ns |  5,576.15 ns |      - |     - |     - |      88 B |
|        MD5 |     3145728 |  5,108,604.6 ns | 189,941.46 ns | 10,411.33 ns |      - |     - |     - |     208 B |
|    SHA-256 |     3145728 | 11,038,065.4 ns | 311,623.07 ns | 17,081.11 ns |      - |     - |     - |     240 B |
|    SHA-512 |     3145728 |  6,599,771.6 ns | 251,528.85 ns | 13,787.15 ns |      - |     - |     - |     304 B |
```

Note that the built-in cryptographic hash algorithms in .NET Core forward to platform-native libraries for their implementations.  On Windows, this means the implementations are provided by [Windows CNG](https://docs.microsoft.com/en-us/windows/desktop/seccng/cng-portal).  Performance may differ on Linux.

On .NET Framework, only scalar (not SIMD) implementations are available for both BLAKE2 algorithms.  The scalar implementations outperform the built-in .NET algorithms in 64-bit applications, but they are slower for large input data on 32-bit.  The SIMD implementations available in .NET Core are faster than the built-in algorithms on either processor architecture.

### Blake2Fast vs other BLAKE2b implementations available on NuGet

```
|              Method | Data Length |            Mean |            Error |          StdDev |     Gen 0 |     Gen 1 |     Gen 2 |   Allocated |
|-------------------- |------------:|----------------:|-----------------:|----------------:|----------:|----------:|----------:|------------:|
| *Blake2Fast.Blake2b |           3 |        139.5 ns |          2.71 ns |         0.15 ns |    0.0076 |         - |         - |        32 B |
|      Blake2Sharp(1) |           3 |        382.0 ns |         41.26 ns |         2.26 ns |    0.2065 |         - |         - |       864 B |
|      ByteTerrace(2) |           3 |        442.5 ns |         40.06 ns |         2.20 ns |    0.1087 |         - |         - |       456 B |
| S.D.HashFunction(3) |           3 |      1,818.6 ns |         28.93 ns |         1.59 ns |    0.4158 |         - |         - |      1744 B |
|        Konscious(4) |           3 |      1,234.3 ns |         23.67 ns |         1.30 ns |    0.2289 |         - |         - |       960 B |
|           Isopoh(5) |           3 | 10,403,770.2 ns | 96,909,560.25 ns | 5,311,940.00 ns | 1736.0840 | 1722.4121 | 1722.4121 | 527973075 B |
|       Blake2Core(6) |           3 |      1,407.4 ns |        137.05 ns |         7.51 ns |    0.2060 |         - |         - |       864 B |
|             NSec(7) |           3 |        170.2 ns |         17.42 ns |         0.96 ns |    0.0267 |         - |         - |       112 B |
|                     |             |                 |                  |                 |           |           |           |             |
| *Blake2Fast.Blake2b |        3268 |      2,413.4 ns |         48.19 ns |         2.64 ns |    0.0076 |         - |         - |        32 B |
|      Blake2Sharp(1) |        3268 |      4,378.4 ns |        278.87 ns |        15.29 ns |    0.2060 |         - |         - |       864 B |
|      ByteTerrace(2) |        3268 |      4,095.5 ns |        295.62 ns |        16.20 ns |    0.1068 |         - |         - |       456 B |
| S.D.HashFunction(3) |        3268 |     29,730.2 ns |      2,388.67 ns |       130.93 ns |    2.2278 |         - |         - |      9344 B |
|        Konscious(4) |        3268 |     16,682.2 ns |        997.62 ns |        54.68 ns |    0.2136 |         - |         - |       960 B |
|           Isopoh(5) |        3268 |  1,708,548.1 ns |  3,287,267.60 ns |   180,186.23 ns |  220.7031 |  218.7500 |  218.7500 |  67111641 B |
|       Blake2Core(6) |        3268 |     20,619.3 ns |      1,859.13 ns |       101.90 ns |    0.1831 |         - |         - |       864 B |
|             NSec(7) |        3268 |      2,459.1 ns |        252.85 ns |        13.86 ns |    0.0267 |         - |         - |       112 B |
|                     |             |                 |                  |                 |           |           |           |             |
| *Blake2Fast.Blake2b |     3145728 |  2,242,018.9 ns |    156,659.45 ns |     8,587.03 ns |         - |         - |         - |        32 B |
|      Blake2Sharp(1) |     3145728 |  3,955,138.2 ns |    113,166.53 ns |     6,203.04 ns |         - |         - |         - |       864 B |
|      ByteTerrace(2) |     3145728 |  3,641,689.8 ns |     58,221.45 ns |     3,191.31 ns |         - |         - |         - |       457 B |
| S.D.HashFunction(3) |     3145728 | 27,450,332.3 ns |  1,245,091.70 ns |    68,247.68 ns | 1781.2500 |         - |         - |   7472544 B |
|        Konscious(4) |     3145728 | 15,179,139.1 ns |    668,577.20 ns |    36,646.97 ns |         - |         - |         - |       960 B |
|           Isopoh(5) |     3145728 |  4,011,376.3 ns |    477,836.99 ns |    26,191.86 ns |         - |         - |         - |       984 B |
|       Blake2Core(6) |     3145728 | 18,704,691.7 ns |  1,247,107.98 ns |    68,358.20 ns |         - |         - |         - |       864 B |
|             NSec(7) |     3145728 |  2,247,392.2 ns |     13,390.91 ns |       734.00 ns |         - |         - |         - |       112 B |
```

* (1) `Blake2Sharp` is the reference C# BLAKE2b implementation from the [official BLAKE2 repo](https://github.com/BLAKE2/BLAKE2).  This version is not published to NuGet, so the source is included in the benchmark project directly.
* (2) `ByteTerrace.Maths.Cryptography.Blake2` version 0.0.6.
* (3) `System.Data.HashFunction.Blake2` version 2.0.0.  BLAKE2b only.
* (4) `Konscious.Security.Cryptography.Blake2` version 1.0.9.  BLAKE2b only.
* (5) `Isopoh.Cryptography.Blake2b` version 1.1.3.  Yes, it really is that slow on incomplete block lengths.
* (6) `Blake2Core` version 1.0.0.  This package contains the reference Blake2Sharp code compiled as a debug (unoptimized) build.  BenchmarkDotNet errors in such cases, so the settings were overridden to allow this library to run.
* (7) `NSec.Cryptography` 20.2.0.  This implementation of BLAKE2b is not RFC-compliant in that it does not support digest sizes less than 32 bytes or keyed hashing.  NSec.Cryptography wraps the native `libsodium` library, which contains an AVX2 implementation of BLAKE2b.

### Blake2Fast vs other BLAKE2s implementations available on NuGet

```
|              Method | Data Length |           Mean |         Error |       StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------- |------------:|---------------:|--------------:|-------------:|-------:|------:|------:|----------:|
| *Blake2Fast.Blake2s |           3 |       106.5 ns |       2.30 ns |      0.13 ns | 0.0076 |     - |     - |      32 B |
|      Blake2s-net(1) |           3 |       274.4 ns |      39.08 ns |      2.14 ns | 0.1278 |     - |     - |     536 B |
|      ByteTerrace(2) |           3 |       303.6 ns |       5.69 ns |      0.31 ns | 0.0763 |     - |     - |     320 B |
|                     |             |                |               |              |        |       |       |           |
| *Blake2Fast.Blake2s |        3268 |     3,941.2 ns |     388.64 ns |     21.30 ns | 0.0076 |     - |     - |      32 B |
|      Blake2s-net(1) |        3268 |     6,044.0 ns |     251.18 ns |     13.77 ns | 0.1221 |     - |     - |     536 B |
|      ByteTerrace(2) |        3268 |     6,287.7 ns |     715.20 ns |     39.20 ns | 0.0763 |     - |     - |     320 B |
|                     |             |                |               |              |        |       |       |           |
| *Blake2Fast.Blake2s |     3145728 | 3,669,570.7 ns | 308,040.39 ns | 16,884.73 ns |      - |     - |     - |      32 B |
|      Blake2s-net(1) |     3145728 | 5,549,277.3 ns | 171,690.31 ns |  9,410.93 ns |      - |     - |     - |     536 B |
|      ByteTerrace(2) |     3145728 | 5,754,080.2 ns |  75,019.78 ns |  4,112.09 ns |      - |     - |     - |     320 B |
```

* (1) `blake2s-net` version 0.1.0.  This is a conversion of the reference Blake2Sharp code to support BLAKE2s.
* (2) `ByteTerrace.Maths.Cryptography.Blake2` version 0.0.6.

You can find more detailed comparisons between Blake2Fast and other .NET BLAKE2 implementations starting [here](https://photosauce.net/blog/post/fast-hashing-with-blake2-part-1-nuget-is-a-minefield).  The short version is that Blake2Fast is the fastest and lowest-memory version of RFC-compliant BLAKE2 available for .NET.

