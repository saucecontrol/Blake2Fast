Blake2Fast
==========

These [RFC 7693](https://tools.ietf.org/html/rfc7693)-compliant BLAKE2 implementations have been tuned for high speed and low memory usage.  The .NET Core 2.1 build supports the new X86 SIMD Intrinsics for even greater speed and `Span<T>` for even lower memory usage.

Sample benchmark results comparing with built-in .NET algorithms, 10MiB input, .NET Core 2.1 x64 and x86 runtimes:

Method | Platform |      Mean |     Error |    StdDev | Allocated |
------------ |--------- |----------:|----------:|----------:|----------:|
Blake2bFast |      X64 |  12.13 ms | 0.0870 ms | 0.0771 ms |       0 B |
Blake2sFast |      X64 |  16.27 ms | 0.1362 ms | 0.1274 ms |       0 B |
MD5 |      X64 |  21.22 ms | 0.1190 ms | 0.1113 ms |       0 B |
SHA256 |      X64 |  46.16 ms | 0.2564 ms | 0.2398 ms |       0 B |
SHA512 |      X64 |  27.89 ms | 0.0982 ms | 0.0871 ms |     304 B |
|          |           |           |           |           |
Blake2bFast |      X86 |  16.56 ms | 0.0879 ms | 0.0779 ms |       0 B |
Blake2sFast |      X86 |  16.36 ms | 0.1103 ms | 0.1032 ms |       0 B |
MD5 |      X86 |  20.06 ms | 0.0996 ms | 0.0931 ms |       0 B |
SHA256 |      X86 |  52.47 ms | 0.3252 ms | 0.3042 ms |       0 B |
SHA512 |      X86 |  44.07 ms | 0.1643 ms | 0.1372 ms |       0 B |

You can find more detailed comparison between Blake2Fast and other .NET BLAKE2 implementations starting [here](https://photosauce.net/blog/post/fast-hashing-with-blake2-part-1-nuget-is-a-minefield)

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
    {
        // Use Span<T> for .NET Core 2.1 or ArraySegment<T> for others
        incHash.Update(new ArraySegment<byte>(buffer, 0, bytesRead));
    }

    return incHash.Finish();
}
```

### Allocation-Free Hashing

The output hash digest can be written to an existing buffer to avoid allocating a new array each time.  This is especially useful when performing an iterative hash, as might be used in a [key derivation function](https://en.wikipedia.org/wiki/Key_derivation_function).

```C#
byte[] DeriveBytes(string password, byte[] salt)
{
    // Create key from password, then hash the salt using the key
    var pwkey = Blake2b.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    var hbuff = Blake2b.ComputeHash(pwkey, salt);

    // Hash the hash lots of times, re-using the same buffer
    for (int i = 0; i < 1_000_000; i++)
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

The X86 SIMD Intrinsics used in the .NET Core 2.1 build are not officially supported by Microsoft.  Although the specific SSE Intrinsics used by Blake2Fast have been well-tested, the JIT support for the X86 Intrinsics in general is experimental in .NET Core 2.1.  Please test with your specific hardware and report any issues here or in the [CoreCLR](https://github.com/dotnet/coreclr/) repo as appropriate.

If you are uncomfortable using unsupported functionality, you can make a custom build of Blake2Fast by removing the `USE_INTRINSICS` define constant in the [project file](src/Blake2Fast/Blake2Fast.csproj).  This applies only to .NET Core 2.1; the older build targets use only the scalar code.