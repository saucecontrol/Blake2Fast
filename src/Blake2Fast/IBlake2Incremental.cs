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
		/// <summary>Update the hash state with the message bytes contained in <paramref name="input" />.</summary>
		/// <param name="input">The message bytes to add to the hash state.</param>
		void Update(ReadOnlySpan<byte> input);

		/// <summary>Finalize the hash, and return the computed digest.</summary>
		/// <returns>The computed hash digest.</returns>
		byte[] Finish();

		/// <summary>Finalize the hash, and copy the computed digest to <paramref name="output" />.</summary>
		/// <param name="output">The buffer into which the hash digest should be written.</param>
		/// <param name="bytesWritten">On return, contains the number of bytes written to <paramref name="output" />.</param>
		/// <returns>True if the <paramref name="output" /> buffer was large enough to hold the digest, otherwise False.</returns>
		bool TryFinish(Span<byte> output, out int bytesWritten);
	}
}
