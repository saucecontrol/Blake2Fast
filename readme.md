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

The simplest and lightest-weight way to calculate a hash is the all-at-once `ComputeHash` method.

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

The output hash digest can be written to an existing buffer to avoid allocating a new array each time.  This is especially useful when performing an iterative hash, as might be used in a [key derivation function](https://en.wikipedia.org/wiki/Key_derivation_function).

```C#
byte[] DeriveBytes(string password, byte[] salt)
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

If you are uncomfortable using unsupported functionality, you can make a custom build of Blake2Fast by removing the `USE_INTRINSICS` define constant in the [project file](src/Blake2Fast/Blake2Fast.csproj).


Benchmarks
----------

Sample results from the [Blake2.Bench](tests/Blake2.Bench) project.  Benchmarks were run on the .NET Core 3.0-preview7 x64 runtime.  Configuration below:

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100-preview7-012821
  [Host]   : .NET Core 3.0.0-preview7-27912-14 (CoreCLR 4.700.19.32702, CoreFX 4.700.19.36209), 64bit RyuJIT
  ShortRun : .NET Core 3.0.0-preview7-27912-14 (CoreCLR 4.700.19.32702, CoreFX 4.700.19.36209), 64bit RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  WarmupCount=3

```

### Blake2Fast vs .NET in-box algorithms (MD5 and SHA2)

```
|     Method | Data Length |            Mean |          Error |         StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------- |------------:|----------------:|---------------:|---------------:|-------:|------:|------:|----------:|
| BLAKE2-256 |           3 |        111.6 ns |       5.079 ns |      0.2784 ns | 0.0134 |     - |     - |      56 B |
| BLAKE2-512 |           3 |        138.5 ns |       8.805 ns |      0.4826 ns | 0.0210 |     - |     - |      88 B |
|        MD5 |           3 |        544.1 ns |      48.366 ns |      2.6511 ns | 0.0496 |     - |     - |     208 B |
|    SHA-256 |           3 |        711.8 ns |       8.934 ns |      0.4897 ns | 0.0572 |     - |     - |     240 B |
|    SHA-512 |           3 |        734.7 ns |      35.255 ns |      1.9324 ns | 0.0725 |     - |     - |     304 B |
|            |             |                 |                |                |        |       |       |           |
| BLAKE2-256 |        3268 |      4,174.0 ns |     139.581 ns |      7.6509 ns | 0.0076 |     - |     - |      56 B |
| BLAKE2-512 |        3268 |      2,693.9 ns |       1.073 ns |      0.0588 ns | 0.0191 |     - |     - |      88 B |
|        MD5 |        3268 |      5,840.9 ns |     187.058 ns |     10.2533 ns | 0.0458 |     - |     - |     208 B |
|    SHA-256 |        3268 |     12,563.8 ns |     271.360 ns |     14.8742 ns | 0.0458 |     - |     - |     240 B |
|    SHA-512 |        3268 |      7,532.9 ns |      98.917 ns |      5.4220 ns | 0.0687 |     - |     - |     304 B |
|            |             |                 |                |                |        |       |       |           |
| BLAKE2-256 |     3145728 |  3,909,347.1 ns | 120,876.614 ns |  6,625.6551 ns |      - |     - |     - |      56 B |
| BLAKE2-512 |     3145728 |  2,497,492.3 ns |  50,301.798 ns |  2,757.2113 ns |      - |     - |     - |      88 B |
|        MD5 |     3145728 |  5,085,250.3 ns |  95,827.863 ns |  5,252.6485 ns |      - |     - |     - |     208 B |
|    SHA-256 |     3145728 | 10,936,735.2 ns | 674,402.898 ns | 36,966.2985 ns |      - |     - |     - |     240 B |
|    SHA-512 |     3145728 |  6,620,802.9 ns |  32,556.339 ns |  1,784.5228 ns |      - |     - |     - |     304 B |
```

Note that the built-in cryptographic hash algorithms in .NET Core forward to platform-native libraries for their implementations.  On Windows, this means the implementations are provided by [Windows CNG](https://docs.microsoft.com/en-us/windows/desktop/seccng/cng-portal).  Performance may differ on Linux.

On .NET Framework, only scalar (not SIMD) implementations are available for both BLAKE2 algorithms.  The scalar implementations outperform the built-in .NET algorithms in 64-bit applications, but they are slower for large input data on 32-bit.  The SIMD implementations available in .NET Core are faster than the built-in algorithms on either processor architecture.

### Blake2Fast vs other BLAKE2b implementations available on Nuget

```
|              Method | Data Length |            Mean |             Error |            StdDev |     Gen 0 |     Gen 1 |     Gen 2 |   Allocated |
|-------------------- |------------:|----------------:|------------------:|------------------:|----------:|----------:|----------:|------------:|
| *Blake2Fast.Blake2b |           3 |        141.0 ns |         11.192 ns |         0.6135 ns |    0.0076 |         - |         - |        32 B |
|      Blake2Sharp(1) |           3 |        380.5 ns |         30.801 ns |         1.6883 ns |    0.2065 |         - |         - |       864 B |
|      ByteTerrace(2) |           3 |        455.3 ns |          4.572 ns |         0.2506 ns |    0.1087 |         - |         - |       456 B |
| S.D.HashFunction(3) |           3 |      1,819.3 ns |         45.298 ns |         2.4829 ns |    0.4158 |         - |         - |      1744 B |
|        Konscious(4) |           3 |      1,282.5 ns |         58.913 ns |         3.2292 ns |    0.2289 |         - |         - |       960 B |
|           Isopoh(5) |           3 |  4,920,916.8 ns | 54,306,897.991 ns | 2,976,744.3293 ns | 1753.6621 | 1740.2344 | 1740.2344 | 527448084 B |
|       Blake2Core(6) |           3 |      1,394.8 ns |         44.357 ns |         2.4314 ns |    0.2060 |         - |         - |       864 B |
|             NSec(7) |           3 |        189.6 ns |          4.810 ns |         0.2636 ns |    0.0267 |         - |         - |       112 B |
|                     |             |                 |                   |                   |           |           |           |             |
| *Blake2Fast.Blake2b |        3268 |      2,686.8 ns |         16.774 ns |         0.9195 ns |    0.0076 |         - |         - |        32 B |
|      Blake2Sharp(1) |        3268 |      4,338.0 ns |        173.013 ns |         9.4834 ns |    0.2060 |         - |         - |       864 B |
|      ByteTerrace(2) |        3268 |      4,090.6 ns |        158.552 ns |         8.6908 ns |    0.1068 |         - |         - |       456 B |
| S.D.HashFunction(3) |        3268 |     29,381.7 ns |        261.868 ns |        14.3539 ns |    2.2278 |         - |         - |      9344 B |
|        Konscious(4) |        3268 |     16,620.0 ns |      1,402.499 ns |        76.8757 ns |    0.2136 |         - |         - |       960 B |
|           Isopoh(5) |        3268 |  3,392,905.6 ns | 24,844,814.249 ns | 1,361,828.1041 ns | 2203.3691 | 2186.0352 | 2186.0352 | 670057939 B |
|       Blake2Core(6) |        3268 |     20,614.0 ns |        200.856 ns |        11.0096 ns |    0.1831 |         - |         - |       864 B |
|             NSec(7) |        3268 |      2,819.8 ns |         79.142 ns |         4.3380 ns |    0.0267 |         - |         - |       112 B |
|                     |             |                 |                   |                   |           |           |           |             |
| *Blake2Fast.Blake2b |     3145728 |  2,503,472.0 ns |     71,056.282 ns |     3,894.8346 ns |         - |         - |         - |        32 B |
|      Blake2Sharp(1) |     3145728 |  3,954,441.7 ns |    169,463.338 ns |     9,288.8574 ns |         - |         - |         - |       864 B |
|      ByteTerrace(2) |     3145728 |  3,639,843.0 ns |     92,425.183 ns |     5,066.1361 ns |         - |         - |         - |       456 B |
| S.D.HashFunction(3) |     3145728 | 27,317,234.4 ns |    711,323.445 ns |    38,990.0383 ns | 1781.2500 |         - |         - |   7472544 B |
|        Konscious(4) |     3145728 | 15,110,314.6 ns |    305,461.330 ns |    16,743.3662 ns |         - |         - |         - |       960 B |
|           Isopoh(5) |     3145728 |  3,968,873.4 ns |     86,677.772 ns |     4,751.1011 ns |         - |         - |         - |       984 B |
|       Blake2Core(6) |     3145728 | 18,638,068.8 ns |  1,356,570.399 ns |    74,358.2011 ns |         - |         - |         - |       864 B |
|             NSec(7) |     3145728 |  2,561,597.0 ns |     25,735.378 ns |     1,410.6429 ns |         - |         - |         - |       112 B |
```

(1) `Blake2Sharp` is the reference C# BLAKE2b implementation from the [official BLAKE2 repo](https://github.com/BLAKE2/BLAKE2).  This version is not published to Nuget, so the source is included in the benchmark project directly.
(2) `ByteTerrace.Maths.Cryptography.Blake2` version 0.0.4.  This package also includes a BLAKE2s implementation, but it crashed on the 3268 byte and 3KiB inputs, so it is included only in the BLAKE2b benchmark.
(3) `System.Data.HashFunction.Blake2` version 2.0.0.  BLAKE2b only.
(4) `Konscious.Security.Cryptography.Blake2` version 1.0.9.  BLAKE2b only.
(5) `Isopoh.Cryptography.Blake2b` version 1.1.2.
(6) `Blake2Core` version 1.0.0.  This package contains the reference Blake2Sharp code compiled as a debug (unoptimized) build.  BenchmarkDotNet errors in such cases, so the settings were overridden to allow this library to run.
(7) `NSec.Cryptography` 19.5.0.  This implementation of BLAKE2 is not RFC-compliant in that it does not allow digest sizes less than 16 bytes.  This library forwards to a referenced native library (libsodium), which contains an AVX2 implementation of BLAKE2b.

### Blake2Fast vs other BLAKE2s implementations available on Nuget

```
|              Method | Data Length |           Mean |          Error |         StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------- |------------:|---------------:|---------------:|---------------:|-------:|------:|------:|----------:|
| *Blake2Fast.Blake2s |           3 |       108.9 ns |       3.378 ns |      0.1852 ns | 0.0076 |     - |     - |      32 B |
|      Blake2s-net(1) |           3 |       255.3 ns |      10.771 ns |      0.5904 ns | 0.1278 |     - |     - |     536 B |
|                     |             |                |                |                |        |       |       |           |
| *Blake2Fast.Blake2s |        3268 |     4,169.5 ns |     176.109 ns |      9.6531 ns | 0.0076 |     - |     - |      32 B |
|      Blake2s-net(1) |        3268 |     5,964.5 ns |     165.185 ns |      9.0544 ns | 0.1221 |     - |     - |     536 B |
|                     |             |                |                |                |        |       |       |           |
| *Blake2Fast.Blake2s |     3145728 | 3,906,812.0 ns |  73,568.528 ns |  4,032.5393 ns |      - |     - |     - |      32 B |
|      Blake2s-net(1) |     3145728 | 5,469,015.9 ns | 194,030.194 ns | 10,635.4497 ns |      - |     - |     - |     536 B |
```

(1) blake2s-net version 0.1.0.  This is a conversion of the reference Blake2Sharp code to support BLAKE2s.  It is the only other properly working BLAKE2s implementation I could find on Nuget.

You can find more detailed comparisons between Blake2Fast and other .NET BLAKE2 implementations starting [here](https://photosauce.net/blog/post/fast-hashing-with-blake2-part-1-nuget-is-a-minefield).  The short version is that Blake2Fast is the fastest and lowest-memory version of RFC-compliant BLAKE2 available for .NET.

