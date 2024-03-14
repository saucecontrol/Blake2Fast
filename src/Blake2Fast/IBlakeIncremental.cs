// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System;

namespace Blake2Fast;

/// <summary>Defines an incremental BLAKE2 hashing operation.</summary>
/// <remarks>Allows the hash to be computed as portions of the message become available, rather than all at once.</remarks>
#if BLAKE_PUBLIC
public
#else
internal
#endif
interface IBlakeIncremental
{
	/// <summary>The hash digest length for this instance, in bytes.</summary>
	/// <remarks>For BLAKE3, the digest length is variable and can be chosen on <see cref="Finish(Span{byte})"/>.  This value will return the default digest length.</remarks>
	int DigestLength { get; }

	/// <summary>Update the hash state with the bytes of all values in <paramref name="input" />.</summary>
	/// <param name="input">The message bytes to add to the hash state.</param>
	/// <typeparam name="T">The type of the data that will be added to the hash state.  It must be a value type and must not contain any reference type fields.</typeparam>
	/// <remarks>
	///   The <typeparamref name="T" /> value will be added to the hash state in memory layout order, including any padding bytes.
	///   Use caution when using this overload with non-primitive structs or when hash values are to be compared across machines with different struct layouts or byte orders.
	///   <see cref="byte"/> is the only type that is guaranteed to be consistent across platforms.
	/// </remarks>
	/// <exception cref="NotSupportedException">Thrown when <typeparamref name="T"/> is a reference type or contains any fields of reference types.</exception>
	void Update<T>(ReadOnlySpan<T> input) where T : struct;

	/// <inheritdoc cref="Update{T}(ReadOnlySpan{T})" />
	void Update<T>(Span<T> input) where T : struct;

	/// <inheritdoc cref="Update{T}(ReadOnlySpan{T})" />
	void Update<T>(ArraySegment<T> input) where T : struct;

	/// <inheritdoc cref="Update{T}(ReadOnlySpan{T})" />
	void Update<T>(T[] input) where T : struct;

	/// <summary>Update the hash state with the bytes of the <paramref name="input" /> value.</summary>
	/// <inheritdoc cref="Update{T}(ReadOnlySpan{T})" />
	void Update<T>(T input) where T : struct;

	/// <summary>Finalize the hash, and return the computed digest.</summary>
	/// <returns>The computed hash digest.</returns>
	byte[] Finish();

	/// <summary>Finalize the hash, and copy the computed digest to <paramref name="destination" />.</summary>
	/// <param name="destination">The buffer into which the hash digest should be written.</param>
	/// <remarks>
	///   For BLAKE2, the buffer must have a capacity of at least <see cref="DigestLength" /> bytes, and exactly <see cref="DigestLength" /> bytes will be written.
	///   For BLAKE3, the buffer must have a capacity of at least 1 byte, and the entire buffer will be filled.
	/// </remarks>
	void Finish(Span<byte> destination);
}
