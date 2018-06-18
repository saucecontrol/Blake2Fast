using System;

#if !NETSTANDARD1_0
using System.Security.Cryptography;
#endif

#if FAST_SPAN
using ByteSpan = System.ReadOnlySpan<byte>;
using WriteableByteSpan = System.Span<byte>;
#else
using ByteSpan = System.ArraySegment<byte>;
using WriteableByteSpan = System.ArraySegment<byte>;
#endif

namespace SauceControl.Blake2Fast
{
	/// <summary>Static helper methods for BLAKE2b hashing.</summary>
	public static class Blake2b
	{
		/// <summary>The default hash digest length in bytes.  For BLAKE2b, this value is 64.</summary>
		public const int DefaultDigestLength = Blake2bContext.HashBytes;

		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static byte[] ComputeHash(ByteSpan input) => ComputeHash(DefaultDigestLength, default, input);

		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static byte[] ComputeHash(int digestLength, ByteSpan input) => ComputeHash(digestLength, default, input);

		/// <inheritdoc cref="ComputeHash(int, ByteSpan, ByteSpan)"/>
		public static byte[] ComputeHash(ByteSpan key, ByteSpan input) => ComputeHash(DefaultDigestLength, key, input);

		/// <summary>Perform an all-at-once BLAKE2b hash computation.</summary>
		/// <remarks>If you have all the input available at once, this is the most efficient way to calculate the hash.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 64.</param>
		/// <param name="key">0 to 64 bytes of input for initializing a keyed hash.</param>
		/// <param name="input">The message bytes to hash.</param>
		/// <returns>The computed hash digest from the message bytes in <paramref name="input" />.</returns>
		public static byte[] ComputeHash(int digestLength, ByteSpan key, ByteSpan input)
		{
			var ctx = default(Blake2bContext);
			ctx.Init(digestLength, key);
			ctx.Update(input);
			return ctx.Finish();
		}

		/// <inheritdoc cref="ComputeAndWriteHash(ByteSpan, ByteSpan, WriteableByteSpan)" />
		public static void ComputeAndWriteHash(ByteSpan input, WriteableByteSpan output) => ComputeAndWriteHash(DefaultDigestLength, default, input, output);

		/// <inheritdoc cref="ComputeAndWriteHash(int, ByteSpan, ByteSpan, WriteableByteSpan)" />
		public static void ComputeAndWriteHash(int digestLength, ByteSpan input, WriteableByteSpan output) => ComputeAndWriteHash(digestLength, default, input, output);

		/// <summary>Perform an all-at-once BLAKE2b hash computation and write the hash digest to <paramref name="output" />.</summary>
		/// <remarks>If you have all the input available at once, this is the most efficient way to calculate the hash.</remarks>
		/// <param name="key">0 to 64 bytes of input for initializing a keyed hash.</param>
		/// <param name="input">The message bytes to hash.</param>
		/// <param name="output">Destination buffer into which the hash digest is written.  The buffer must have a capacity of at least <see cref="DefaultDigestLength"/>(64) /> bytes.</param>
		public static void ComputeAndWriteHash(ByteSpan key, ByteSpan input, WriteableByteSpan output) => ComputeAndWriteHash(DefaultDigestLength, key, input, output);

		/// <summary>Perform an all-at-once BLAKE2b hash computation and write the hash digest to <paramref name="output" />.</summary>
		/// <remarks>If you have all the input available at once, this is the most efficient way to calculate the hash.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 64.</param>
		/// <param name="key">0 to 64 bytes of input for initializing a keyed hash.</param>
		/// <param name="input">The message bytes to hash.</param>
		/// <param name="output">Destination buffer into which the hash digest is written.  The buffer must have a capacity of at least <paramref name="digestLength" /> bytes.</param>
		public static void ComputeAndWriteHash(int digestLength, ByteSpan key, ByteSpan input, WriteableByteSpan output)
		{
#if FAST_SPAN
			if (output.Length < digestLength)
#else
			if (output.Count < digestLength)
#endif
				throw new ArgumentException($"Output buffer must have a capacity of at least {digestLength} bytes.", nameof(output));

			var ctx = default(Blake2bContext);
			ctx.Init(digestLength, key);
			ctx.Update(input);
			ctx.TryFinish(output, out int _);
		}

		/// <inheritdoc cref="CreateIncrementalHasher(int, ByteSpan)" />
		public static IBlake2Incremental CreateIncrementalHasher() => CreateIncrementalHasher(DefaultDigestLength, default(ByteSpan));

		/// <inheritdoc cref="CreateIncrementalHasher(int, ByteSpan)" />
		public static IBlake2Incremental CreateIncrementalHasher(int digestLength) => CreateIncrementalHasher(digestLength, default(ByteSpan));

		/// <inheritdoc cref="CreateIncrementalHasher(int, ByteSpan)" />
		public static IBlake2Incremental CreateIncrementalHasher(ByteSpan key) => CreateIncrementalHasher(DefaultDigestLength, key);

		/// <summary>Create and initialize an incremental BLAKE2b hash computation.</summary>
		/// <remarks>If you will recieve the input in segments rather than all at once, this is the most efficient way to calculate the hash.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 64.</param>
		/// <param name="key">0 to 64 bytes of input for initializing a keyed hash.</param>
		/// <returns>An <see cref="IBlake2Incremental" /> interface for updating and finalizing the hash.</returns>
		public static IBlake2Incremental CreateIncrementalHasher(int digestLength, ByteSpan key)
		{
			var ctx = default(Blake2bContext);
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

		/// <inheritdoc cref="ComputeAndWriteHash(ByteSpan, ByteSpan, WriteableByteSpan)" />
		public static void ComputeAndWriteHash(byte[] input, byte[] output) => ComputeAndWriteHash(DefaultDigestLength, default, input.AsByteSpan(), output.AsByteSpan());

		/// <inheritdoc cref="ComputeAndWriteHash(int, ByteSpan, ByteSpan, WriteableByteSpan)" />
		public static void ComputeAndWriteHash(int digestLength, byte[] input, byte[] output) => ComputeAndWriteHash(digestLength, default, input.AsByteSpan(), output.AsByteSpan());

		/// <inheritdoc cref="ComputeAndWriteHash(ByteSpan, ByteSpan, WriteableByteSpan)" />
		public static void ComputeAndWriteHash(byte[] key, byte[] input, byte[] output) => ComputeAndWriteHash(DefaultDigestLength, key.AsByteSpan(), input.AsByteSpan(), output.AsByteSpan());

		/// <inheritdoc cref="ComputeAndWriteHash(int, ByteSpan, ByteSpan, WriteableByteSpan)" />
		public static void ComputeAndWriteHash(int digestLength, byte[] key, byte[] input, byte[] output) => ComputeAndWriteHash(digestLength, key.AsByteSpan(), input.AsByteSpan(), output.AsByteSpan());

		/// <inheritdoc cref="CreateIncrementalHasher(int, ByteSpan)" />
		public static IBlake2Incremental CreateIncrementalHasher(byte[] key) => CreateIncrementalHasher(key.AsByteSpan());

		/// <inheritdoc cref="CreateIncrementalHasher(int, ByteSpan)" />
		public static IBlake2Incremental CreateIncrementalHasher(int digestLength, byte[] key) => CreateIncrementalHasher(digestLength, key.AsByteSpan());
#endif

#if !NETSTANDARD1_0
		/// <inheritdoc cref="CreateHashAlgorithm(int)" />
		public static HashAlgorithm CreateHashAlgorithm() => CreateHashAlgorithm(DefaultDigestLength);

		/// <summary>Creates and initializes a <see cref="HashAlgorithm" /> instance that implements BLAKE2b hashing.</summary>
		/// <remarks>Use this only if you require an implementation of <see cref="HashAlgorithm" />.  It is less efficient than the direct methods.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 64.</param>
		/// <returns>A <see cref="HashAlgorithm" /> instance.</returns>
		public static HashAlgorithm CreateHashAlgorithm(int digestLength) => new Blake2HMAC(Blake2Algorithm.Blake2b, digestLength, default);

		/// <inheritdoc cref="CreateHMAC(int, ByteSpan)" />
		public static HMAC CreateHMAC(ByteSpan key) => CreateHMAC(DefaultDigestLength, key);

		/// <summary>Creates and initializes an <see cref="HMAC" /> instance that implements BLAKE2b keyed hashing.  Uses BLAKE2's built-in support for keyed hashing rather than the normal 2-pass approach.</summary>
		/// <remarks>Use this only if you require an implementation of <see cref="HMAC" />.  It is less efficient than the direct methods.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 64.</param>
		/// <param name="key">0 to 64 bytes of input for initializing the keyed hash.</param>
		/// <returns>An <see cref="HMAC" /> instance.</returns>
		public static HMAC CreateHMAC(int digestLength, ByteSpan key) => new Blake2HMAC(Blake2Algorithm.Blake2b, digestLength, key);

#if !IMPLICIT_BYTESPAN
		/// <inheritdoc cref="CreateHMAC(int, ByteSpan)" />
		public static HMAC CreateHMAC(byte[] key) => CreateHMAC(key.AsByteSpan());

		/// <inheritdoc cref="CreateHMAC(int, ByteSpan)" />
		public static HMAC CreateHMAC(int digestLength, byte[] key) => CreateHMAC(digestLength, key.AsByteSpan());
#endif
#endif
	}
}
