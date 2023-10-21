Blake2Fast
==========

These [RFC 7693](https://tools.ietf.org/html/rfc7693)-compliant BLAKE2 implementations have been tuned for high speed and low memory usage.  `Span<byte>` is used throughout for lower memory overhead compared to `byte[]` based APIs.

On modern .NET, Blake2Fast includes SIMD-accelerated (SSE2-AVX2) implementations of both BLAKE2b and BLAKE2s.


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
