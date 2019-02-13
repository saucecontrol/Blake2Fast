Blake2Fast
==========

These [RFC 7693](https://tools.ietf.org/html/rfc7693)-compliant BLAKE2 implementations have been tuned for high speed and low memory usage.  The .NET Core 2.1 and 3.0 builds support the new X86 SIMD Intrinsics for even greater speed.  `Span<byte>` is used throughout for lower memory overhead compared to `byte[]` based APIs.

Sample benchmark results comparing with built-in .NET algorithms, 10MiB input, .NET Core x64 and x86 runtimes:

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17134
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100-preview-010184
  [Host]        : .NET Core 2.1.7 (CoreCLR 4.6.27129.04, CoreFX 4.6.27129.04), 64bit RyuJIT
  netcoreapp1.1 : .NET Core 1.1.8 (CoreCLR 4.6.26328.01, CoreFX 4.6.24705.01), 64bit RyuJIT
  netcoreapp2.1 : .NET Core 2.1.7 (CoreCLR 4.6.27129.04, CoreFX 4.6.27129.04), 64bit RyuJIT
  netcoreapp3.0 : .NET Core 3.0.0-preview-27324-5 (CoreCLR 4.6.27322.0, CoreFX 4.7.19.7311), 64bit RyuJIT

Jit=RyuJit  Toolchain=Default

```
|      Method |           Job | Platform |      Mean |     Error |    StdDev | Allocated |
|------------ |-------------- |--------- |----------:|----------:|----------:|----------:|
| Blake2bFast | netcoreapp1.1 |      X64 | 10.753 ms | 0.0502 ms | 0.0445 ms |       0 B |
| Blake2sFast | netcoreapp1.1 |      X64 | 17.066 ms | 0.1528 ms | 0.1355 ms |       0 B |
|             |               |          |           |           |           |           |
| Blake2bFast | netcoreapp2.1 |      X64 | 10.230 ms | 0.0708 ms | 0.0662 ms |       0 B |
| Blake2sFast | netcoreapp2.1 |      X64 | 13.678 ms | 0.0258 ms | 0.0216 ms |       0 B |
|             |               |          |           |           |           |           |
| Blake2bFast | netcoreapp3.0 |      X64 |  8.792 ms | 0.0305 ms | 0.0254 ms |       0 B |
| Blake2sFast | netcoreapp3.0 |      X64 | 13.687 ms | 0.0463 ms | 0.0433 ms |       0 B |
|         MD5 | netcoreapp3.0 |      X64 | 17.894 ms | 0.0632 ms | 0.0561 ms |       0 B |
|      SHA256 | netcoreapp3.0 |      X64 | 38.607 ms | 0.2877 ms | 0.2691 ms |       0 B |
|      SHA512 | netcoreapp3.0 |      X64 | 23.498 ms | 0.1493 ms | 0.1397 ms |     304 B |

|      Method |           Job | Platform |      Mean |     Error |    StdDev | Allocated |
|------------ |-------------- |--------- |----------:|----------:|----------:|----------:|
| Blake2bFast | netcoreapp1.1 |      X86 | 68.925 ms | 0.1575 ms | 0.1315 ms |       0 B |
| Blake2sFast | netcoreapp1.1 |      X86 | 67.513 ms | 0.5069 ms | 0.4742 ms |       0 B |
|             |               |          |           |           |           |           |
| Blake2bFast | netcoreapp2.1 |      X86 | 14.208 ms | 0.0876 ms | 0.0819 ms |       0 B |
| Blake2sFast | netcoreapp2.1 |      X86 | 13.628 ms | 0.0399 ms | 0.0333 ms |       0 B |
|             |               |          |           |           |           |           |
| Blake2bFast | netcoreapp3.0 |      X86 |  8.965 ms | 0.0483 ms | 0.0452 ms |       0 B |
| Blake2sFast | netcoreapp3.0 |      X86 | 13.636 ms | 0.0474 ms | 0.0443 ms |       0 B |
|         MD5 | netcoreapp3.0 |      X86 | 16.966 ms | 0.1235 ms | 0.1155 ms |       0 B |
|      SHA256 | netcoreapp3.0 |      X86 | 44.138 ms | 0.1181 ms | 0.0986 ms |       0 B |
|      SHA512 | netcoreapp3.0 |      X86 | 37.384 ms | 0.3196 ms | 0.2989 ms |       0 B |

Duplicate results have been removed from the above tables for the sake of brevity.

Note that the built-in cryptographic hash algorithms in .NET forward to platform-native libraries for their implementations.  On Windows, this means the implementations are provided by [Windows CNG](https://docs.microsoft.com/en-us/windows/desktop/seccng/cng-portal).  Their performance is therefore identical across all .NET Core versions.

On .NET Framework and .NET Core 1.1, only scalar implementations are available for both BLAKE2 algorithms.  The scalar implementations outperform the built-in .NET algorithms on x64 platforms, but they are significantly slower on x86.

On .NET Core 2.1, Blake2Fast uses an SSE4.1 SIMD-accelerated implementation for both BLAKE2b and BLAKE2s.  On .NET Core 3.0, an AVX2 implementation of BLAKE2b is available (with SSE4.1 fallback for older processors), while BLAKE2s uses the same SSE4.1 implementation.  These are faster than the .NET built-in algorithms on either processor architecture.

You can find more detailed comparisons between Blake2Fast and other .NET BLAKE2 implementations starting [here](https://photosauce.net/blog/post/fast-hashing-with-blake2-part-1-nuget-is-a-minefield).  The short version is that Blake2Fast is the fastest and lowest-memory version of RFC-compliant BLAKE2 available for .NET.

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
    var incHash = Blake2b.CreateIncrementalHasher();
    var buffer = new byte[4096];
    int bytesRead;

    while ((bytesRead = await data.ReadAsync(buffer, 0, buffer.Length)) > 0)
        incHash.Update(new Span<byte>(buffer, 0, bytesRead));

    return incHash.Finish();
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
byte[] WriteDataAndCalculateHash(byte[] data)
{
    using (var hashAlg = Blake2b.CreateHashAlgorithm())
    using (var fileStream = new FileStream(@"c:\data\output.bin", FileMode.Create))
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

The X86 SIMD Intrinsics used in the .NET Core 2.1 build are not officially supported by Microsoft.  Although the specific SSE Intrinsics used by Blake2Fast have been well-tested, the JIT support for the X86 Intrinsics in general is experimental in .NET Core 2.1.

If you are uncomfortable using unsupported functionality, you can make a custom build of Blake2Fast by removing the `USE_INTRINSICS` define constant in the [project file](src/Blake2Fast/Blake2Fast.csproj).

This warning applies only to .NET Core 2.1; the older build targets use only the scalar code, and SIMD intrinsics will be fully supported on .NET Core 3.0+.
