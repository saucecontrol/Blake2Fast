// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System;

using Blake2Fast.Implementation;

namespace Blake2Fast;

/// <summary>Static helper methods for BLAKE3 hashing.</summary>
#if BLAKE_PUBLIC
public
#else
internal
#endif
static class Blake3
{
	/// <summary>The default hash digest length in bytes.  For BLAKE3, this value is 32.</summary>
	public const int DefaultDigestLength = Blake3HashState.HashBytes;

	/// <inheritdoc cref="HashData(int, ReadOnlySpan{byte}, ReadOnlySpan{byte})" />
	public static byte[] HashData(ReadOnlySpan<byte> source) => HashData(DefaultDigestLength, default, source);

	/// <inheritdoc cref="HashData(int, ReadOnlySpan{byte}, ReadOnlySpan{byte})" />
	public static byte[] HashData(int digestLength, ReadOnlySpan<byte> source) => HashData(digestLength, default, source);

	/// <inheritdoc cref="HashData(int, ReadOnlySpan{byte}, ReadOnlySpan{byte})" />
	public static byte[] HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source) => HashData(DefaultDigestLength, key, source);

	/// <summary>Perform a one-shot BLAKE3 hash computation.</summary>
	/// <remarks>If you have all the input available at once, this is the most efficient way to calculate the hash.</remarks>
	/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to Array.MaxLength />.</param>
	/// <param name="key">0 or 32 bytes of input for initializing a keyed hash.</param>
	/// <param name="source">The message bytes to hash.</param>
	/// <returns>The computed hash digest from the message bytes in <paramref name="source" />.</returns>
	public static byte[] HashData(int digestLength, ReadOnlySpan<byte> key, ReadOnlySpan<byte> source)
	{
		byte[] hash = new byte[digestLength];

		var hs = default(Blake3HashState);
		hs.Init(key);
		hs.Update(source);
		hs.Finish(hash);

		return hash;
	}

	/// <inheritdoc cref="HashData(ReadOnlySpan{byte}, ReadOnlySpan{byte}, Span{byte})" />
	public static void HashData(ReadOnlySpan<byte> source, Span<byte> destination) => HashData(default, source, destination);

	/// <summary>Perform a one-shot BLAKE3 hash computation and write the hash digest to <paramref name="destination" />.</summary>
	/// <remarks>If you have all the input available at once, this is the most efficient way to calculate the hash.</remarks>
	/// <param name="key">0 or 32 bytes of input for initializing a keyed hash.</param>
	/// <param name="source">The message bytes to hash.</param>
	/// <param name="destination">Destination buffer into which the hash digest is written.  The buffer must have a capacity of at least 1 byte.</param>
	public static void HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination)
	{
		if (destination.IsEmpty)
			throw new ArgumentException($"Output buffer must have a capacity of at least 1 byte.", nameof(destination));

		var hs = default(Blake3HashState);
		hs.Init(key);
		hs.Update(source);
		hs.Finish(destination);
	}

	/// <inheritdoc cref="CreateIncrementalHasher(ReadOnlySpan{byte})" />
	public static Blake3HashState CreateIncrementalHasher() => CreateIncrementalHasher(default);

	/// <summary>Create and initialize an incremental BLAKE3 hash computation.</summary>
	/// <remarks>If you will receive the input in segments rather than all at once, this is the most efficient way to calculate the hash.</remarks>
	/// <param name="key">0 to 32 bytes of input for initializing a keyed hash.</param>
	/// <returns>An <see cref="Blake3HashState" /> instance for updating and finalizing the hash.</returns>
	public static Blake3HashState CreateIncrementalHasher(ReadOnlySpan<byte> key)
	{
		var hs = default(Blake3HashState);
		hs.Init(key);
		return hs;
	}

#if BLAKE_PUBLIC
	/// <inheritdoc cref="HashData(int, ReadOnlySpan{byte}, ReadOnlySpan{byte})" />
	public static byte[] HashData(byte[] source) => HashData(DefaultDigestLength, default, new ReadOnlySpan<byte>(source));

	/// <inheritdoc cref="HashData(int, ReadOnlySpan{byte}, ReadOnlySpan{byte})" />
	public static byte[] HashData(int digestLength, byte[] source) => HashData(digestLength, default, new ReadOnlySpan<byte>(source));

	/// <inheritdoc cref="HashData(int, ReadOnlySpan{byte}, ReadOnlySpan{byte})" />
	public static byte[] HashData(byte[] key, byte[] source) => HashData(DefaultDigestLength, new ReadOnlySpan<byte>(key), new ReadOnlySpan<byte>(source));

	/// <inheritdoc cref="HashData(int, ReadOnlySpan{byte}, ReadOnlySpan{byte})" />
	public static byte[] HashData(int digestLength, byte[] key, byte[] source) => HashData(digestLength, new ReadOnlySpan<byte>(key), new ReadOnlySpan<byte>(source));
#endif
}
