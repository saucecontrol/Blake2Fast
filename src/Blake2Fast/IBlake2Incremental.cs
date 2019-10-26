using System;

namespace Blake2Fast
{
	/// <summary>Defines an incremental BLAKE2 hashing operation.</summary>
	/// <remarks>Allows the hash to be computed as portions of the message become available, rather than all at once.</remarks>
#if BLAKE2_PUBLIC
	public
#else
	internal
#endif
	interface IBlake2Incremental
	{
		/// <summary>The hash digest length for this instance, in bytes.</summary>
		int DigestLength { get; }

		/// <summary>Update the hash state with the bytes contained in <paramref name="input" />.</summary>
		/// <param name="input">The message bytes to add to the hash state.</param>
		void Update(ReadOnlySpan<byte> input);

		/// <inheritdoc cref="Update(ReadOnlySpan{byte})" />
		/// <typeparam name="T">The type of the data that will be added to the hash state.  It must be a value type and must not contain any reference type fields.</typeparam>
		/// <remarks>
		///   The <typeparamref name="T" /> value will be added to the hash state in memory layout order, including any padding bytes.
		///   Use caution when using this overload with non-primitive structs or when hash values are to be compared across machines with different struct layouts or byte orders.
		/// </remarks>
		/// <exception cref="NotSupportedException">Thrown when <typeparamref name="T"/> is a reference type or contains any fields of reference types.</exception>
		void Update<T>(ReadOnlySpan<T> input) where T : struct;

		/// <summary>Update the hash state with the <paramref name="input" /> value.</summary>
		/// <inheritdoc cref="Update{T}(ReadOnlySpan{T})" />
		void Update<T>(T input) where T : struct;

		/// <summary>Finalize the hash, and return the computed digest.</summary>
		/// <returns>The computed hash digest.</returns>
		byte[] Finish();

		/// <summary>Finalize the hash, and copy the computed digest to <paramref name="output" />.</summary>
		/// <param name="output">The buffer into which the hash digest should be written.  The buffer must have a capacity of at least <see cref="DigestLength" /> bytes for the method to succeed.</param>
		/// <param name="bytesWritten">On return, contains the number of bytes written to <paramref name="output" />.</param>
		/// <returns>True if the <paramref name="output" /> buffer was large enough to hold the digest, otherwise False.</returns>
		bool TryFinish(Span<byte> output, out int bytesWritten);
	}
}
