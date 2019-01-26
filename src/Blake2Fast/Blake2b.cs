using System;

#if USE_CRYPTOGRAPHY
using System.Security.Cryptography;
#endif

namespace SauceControl.Blake2Fast
{
	/// <summary>Static helper methods for BLAKE2b hashing.</summary>
	public static class Blake2b
	{
		/// <summary>The default hash digest length in bytes.  For BLAKE2b, this value is 64.</summary>
		public const int DefaultDigestLength = Blake2bContext.HashBytes;

		/// <inheritdoc cref="ComputeHash(int, ReadOnlySpan{byte}, ReadOnlySpan{byte})"/>
		public static byte[] ComputeHash(ReadOnlySpan<byte> input) => ComputeHash(DefaultDigestLength, default, input);

		/// <inheritdoc cref="ComputeHash(int, ReadOnlySpan{byte}, ReadOnlySpan{byte})"/>
		public static byte[] ComputeHash(int digestLength, ReadOnlySpan<byte> input) => ComputeHash(digestLength, default, input);

		/// <inheritdoc cref="ComputeHash(int, ReadOnlySpan{byte}, ReadOnlySpan{byte})"/>
		public static byte[] ComputeHash(ReadOnlySpan<byte> key, ReadOnlySpan<byte> input) => ComputeHash(DefaultDigestLength, key, input);

		/// <summary>Perform an all-at-once BLAKE2b hash computation.</summary>
		/// <remarks>If you have all the input available at once, this is the most efficient way to calculate the hash.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 64.</param>
		/// <param name="key">0 to 64 bytes of input for initializing a keyed hash.</param>
		/// <param name="input">The message bytes to hash.</param>
		/// <returns>The computed hash digest from the message bytes in <paramref name="input" />.</returns>
		public static byte[] ComputeHash(int digestLength, ReadOnlySpan<byte> key, ReadOnlySpan<byte> input)
		{
			var ctx = default(Blake2bContext);
			ctx.Init(digestLength, key);
			ctx.Update(input);
			return ctx.Finish();
		}

		/// <inheritdoc cref="ComputeAndWriteHash(ReadOnlySpan{byte}, ReadOnlySpan{byte}, Span{byte})" />
		public static void ComputeAndWriteHash(ReadOnlySpan<byte> input, Span<byte> output) => ComputeAndWriteHash(DefaultDigestLength, default, input, output);

		/// <inheritdoc cref="ComputeAndWriteHash(int, ReadOnlySpan{byte}, ReadOnlySpan{byte}, Span{byte})" />
		public static void ComputeAndWriteHash(int digestLength, ReadOnlySpan<byte> input, Span<byte> output) => ComputeAndWriteHash(digestLength, default, input, output);

		/// <summary>Perform an all-at-once BLAKE2b hash computation and write the hash digest to <paramref name="output" />.</summary>
		/// <remarks>If you have all the input available at once, this is the most efficient way to calculate the hash.</remarks>
		/// <param name="key">0 to 64 bytes of input for initializing a keyed hash.</param>
		/// <param name="input">The message bytes to hash.</param>
		/// <param name="output">Destination buffer into which the hash digest is written.  The buffer must have a capacity of at least <see cref="DefaultDigestLength"/>(64) /> bytes.</param>
		public static void ComputeAndWriteHash(ReadOnlySpan<byte> key, ReadOnlySpan<byte> input, Span<byte> output) => ComputeAndWriteHash(DefaultDigestLength, key, input, output);

		/// <summary>Perform an all-at-once BLAKE2b hash computation and write the hash digest to <paramref name="output" />.</summary>
		/// <remarks>If you have all the input available at once, this is the most efficient way to calculate the hash.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 64.</param>
		/// <param name="key">0 to 64 bytes of input for initializing a keyed hash.</param>
		/// <param name="input">The message bytes to hash.</param>
		/// <param name="output">Destination buffer into which the hash digest is written.  The buffer must have a capacity of at least <paramref name="digestLength" /> bytes.</param>
		public static void ComputeAndWriteHash(int digestLength, ReadOnlySpan<byte> key, ReadOnlySpan<byte> input, Span<byte> output)
		{
			if (output.Length < digestLength)
				throw new ArgumentException($"Output buffer must have a capacity of at least {digestLength} bytes.", nameof(output));

			var ctx = default(Blake2bContext);
			ctx.Init(digestLength, key);
			ctx.Update(input);
			ctx.TryFinish(output, out int _);
		}

		/// <inheritdoc cref="CreateIncrementalHasher(int, ReadOnlySpan{byte})" />
		public static IBlake2Incremental CreateIncrementalHasher() => CreateIncrementalHasher(DefaultDigestLength, default);

		/// <inheritdoc cref="CreateIncrementalHasher(int, ReadOnlySpan{byte})" />
		public static IBlake2Incremental CreateIncrementalHasher(int digestLength) => CreateIncrementalHasher(digestLength, default);

		/// <inheritdoc cref="CreateIncrementalHasher(int, ReadOnlySpan{byte})" />
		public static IBlake2Incremental CreateIncrementalHasher(ReadOnlySpan<byte> key) => CreateIncrementalHasher(DefaultDigestLength, key);

		/// <summary>Create and initialize an incremental BLAKE2b hash computation.</summary>
		/// <remarks>If you will recieve the input in segments rather than all at once, this is the most efficient way to calculate the hash.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 64.</param>
		/// <param name="key">0 to 64 bytes of input for initializing a keyed hash.</param>
		/// <returns>An <see cref="IBlake2Incremental" /> interface for updating and finalizing the hash.</returns>
		public static IBlake2Incremental CreateIncrementalHasher(int digestLength, ReadOnlySpan<byte> key)
		{
			var ctx = default(Blake2bContext);
			ctx.Init(digestLength, key);
			return ctx;
		}

#if USE_CRYPTOGRAPHY
		/// <inheritdoc cref="CreateHashAlgorithm(int)" />
		public static HashAlgorithm CreateHashAlgorithm() => CreateHashAlgorithm(DefaultDigestLength);

		/// <summary>Creates and initializes a <see cref="HashAlgorithm" /> instance that implements BLAKE2b hashing.</summary>
		/// <remarks>Use this only if you require an implementation of <see cref="HashAlgorithm" />.  It is less efficient than the direct methods.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 64.</param>
		/// <returns>A <see cref="HashAlgorithm" /> instance.</returns>
		public static HashAlgorithm CreateHashAlgorithm(int digestLength) => new Blake2HMAC(Blake2Algorithm.Blake2b, digestLength, default);

		/// <inheritdoc cref="CreateHMAC(int, ReadOnlySpan{byte})" />
		public static HMAC CreateHMAC(ReadOnlySpan<byte> key) => CreateHMAC(DefaultDigestLength, key);

		/// <summary>Creates and initializes an <see cref="HMAC" /> instance that implements BLAKE2b keyed hashing.  Uses BLAKE2's built-in support for keyed hashing rather than the normal 2-pass approach.</summary>
		/// <remarks>Use this only if you require an implementation of <see cref="HMAC" />.  It is less efficient than the direct methods.</remarks>
		/// <param name="digestLength">The hash digest length in bytes.  Valid values are 1 to 64.</param>
		/// <param name="key">0 to 64 bytes of input for initializing the keyed hash.</param>
		/// <returns>An <see cref="HMAC" /> instance.</returns>
		public static HMAC CreateHMAC(int digestLength, ReadOnlySpan<byte> key) => new Blake2HMAC(Blake2Algorithm.Blake2b, digestLength, key);
#endif
	}
}
