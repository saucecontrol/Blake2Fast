#if !NETSTANDARD1_0
using System.Security.Cryptography;
#endif

#if FAST_SPAN
using ByteSpan = System.ReadOnlySpan<byte>;
#else
using ByteSpan = System.ArraySegment<byte>;
#endif

namespace SauceControl.Blake2Fast
{
	/// <summary>Static helper methods for Blake2s hashing.</summary>
	public static class Blake2s
	{
		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static byte[] ComputeHash(ByteSpan input) => ComputeHash(Blake2sContext.HashBytes, default, input);

		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static byte[] ComputeHash(int digestLength, ByteSpan input) => ComputeHash(digestLength, default, input);

		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static byte[] ComputeHash(ByteSpan key, ByteSpan input) => ComputeHash(Blake2sContext.HashBytes, key, input);

		/// <summary>Perform an all-at-once Blake2s hash computation.</summary>
		/// <remarks>If you have all the input available at once, this is the most efficient way to calculate the hash.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 32.</param>
		/// <param name="key">0 to 32 bytes of input for initializing a keyed hash.</param>
		/// <param name="input">The message bytes to hash.</param>
		/// <returns>The computed hash digest from the message bytes in <paramref name="input" />.</returns>
		public static byte[] ComputeHash(int digestLength, ByteSpan key, ByteSpan input)
		{
			var ctx = default(Blake2sContext);
			ctx.Init(digestLength, key);
			ctx.Update(input);
			return ctx.Finish();
		}

		/// <inheritdoc cref="CreateIncrementalHasher(int, ByteSpan)" />
		public static IBlake2Incremental CreateIncrementalHasher() => CreateIncrementalHasher(Blake2sContext.HashBytes, default(ByteSpan));

		/// <inheritdoc cref="CreateIncrementalHasher(int, ByteSpan)" />
		public static IBlake2Incremental CreateIncrementalHasher(int digestLength) => CreateIncrementalHasher(digestLength, default(ByteSpan));

		/// <inheritdoc cref="CreateIncrementalHasher(int, ByteSpan)" />
		public static IBlake2Incremental CreateIncrementalHasher(ByteSpan key) => CreateIncrementalHasher(Blake2sContext.HashBytes, key);

		/// <summary>Create and initialize an incremental Blake2s hash computation.</summary>
		/// <remarks>If you will recieve the input in segments rather than all at once, this is the most efficient way to calculate the hash.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 32.</param>
		/// <param name="key">0 to 32 bytes of input for initializing a keyed hash.</param>
		/// <returns>An <see cref="IBlake2Incremental" /> interface for updating and finalizing the hash.</returns>
		public static IBlake2Incremental CreateIncrementalHasher(int digestLength, ByteSpan key)
		{
			var ctx = default(Blake2sContext);
			ctx.Init(digestLength, key);
			return ctx;
		}

#if !IMPLICIT_BYTESPAN
		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static byte[] ComputeHash(byte[] input) => ComputeHash(input.AsByteSpan());

		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static byte[] ComputeHash(int digestLength, byte[] input) => ComputeHash(digestLength, input.AsByteSpan());

		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static byte[] ComputeHash(byte[] key, byte[] input) => ComputeHash(key.AsByteSpan(), input.AsByteSpan());

		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static byte[] ComputeHash(int digestLength, byte[] key, byte[] input) => ComputeHash(digestLength, key.AsByteSpan(), input.AsByteSpan());

		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static IBlake2Incremental CreateIncrementalHasher(byte[] key) => CreateIncrementalHasher(key.AsByteSpan());

		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static IBlake2Incremental CreateIncrementalHasher(int digestLength, byte[] key) => CreateIncrementalHasher(digestLength, key.AsByteSpan());
#endif

#if !NETSTANDARD1_0
		/// <inheritdoc cref="CreateHashAlgorithm(int)" />
		public static HashAlgorithm CreateHashAlgorithm() => CreateHashAlgorithm(Blake2sContext.HashBytes);

		/// <summary>Creates and initializes a <see cref="HashAlgorithm" /> instance that implements Blake2s hashing.</summary>
		/// <remarks>Use this only if you require an implementation of <see cref="HashAlgorithm" />.  It is less efficient than the direct methods.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 32.</param>
		/// <returns>A <see cref="HashAlgorithm" /> instance.</returns>
		public static HashAlgorithm CreateHashAlgorithm(int digestLength) => new Blake2HMAC(Blake2Algorithm.Blake2s, digestLength, default);

		/// <inheritdoc cref="CreateHMAC(int, ByteSpan)" />
		public static HMAC CreateHMAC(ByteSpan key) => CreateHMAC(Blake2sContext.HashBytes, key);

		/// <summary>Creates and initializes an <see cref="HMAC" /> instance that implements Blake2s keyed hashing.</summary>
		/// <remarks>Use this only if you require an implementation of <see cref="HMAC" />.  It is less efficient than the direct methods.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 32.</param>
		/// <param name="key">0 to 32 bytes of input for initializing the keyed hash.</param>
		/// <returns>An <see cref="HMAC" /> instance.</returns>
		public static HMAC CreateHMAC(int digestLength, ByteSpan key) => new Blake2HMAC(Blake2Algorithm.Blake2s, digestLength, key);

#if !IMPLICIT_BYTESPAN
		/// <inheritdoc cref="CreateHMAC(int, ByteSpan)" />
		public static HMAC CreateHMAC(byte[] key) => CreateHMAC(key.AsByteSpan());

		/// <inheritdoc cref="CreateHMAC(int, ByteSpan)" />
		public static HMAC CreateHMAC(int digestLength, byte[] key) => CreateHMAC(digestLength, key.AsByteSpan());
#endif
#endif
	}
}
