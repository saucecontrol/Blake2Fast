#if FAST_SPAN
using ByteSpan = System.ReadOnlySpan<byte>;
using WriteableByteSpan = System.Span<byte>;
#else
using ByteSpan = System.ArraySegment<byte>;
#endif

namespace SauceControl.Blake2Fast
{
	/// <summary>Defines an incremental Blake2 hashing operation.</summary>
	/// <remarks>Allows the hash to be computed as portions of the message become available, rather than all at once.</remarks>
	public interface IBlake2Incremental
	{
#if !IMPLICIT_BYTESPAN
		/// <inheritdoc cref="Update(ByteSpan)" />
		void Update(byte[] input);
#endif

		/// <summary>Update the hash state with the message bytes contained in <paramref name="input" />.</summary>
		/// <param name="input">The message bytes to add to the hash state.</param>
		void Update(ByteSpan input);

		/// <summary>Finalize the hash, and return the computed digest.</summary>
		/// <returns>The computed hash digest.</returns>
		byte[] Finish();

#if FAST_SPAN
		/// <summary>Finalize the hash, and copy the computed digest to <paramref name="output" />.</summary>
		/// <param name="output">The buffer into which the hash digest should be written.</param>
		/// <param name="bytesWritten">On return, contains the number of bytes written to <paramref name="output" />.</param>
		/// <returns>True if the <paramref name="output" /> buffer was large enough to hold the digest, otherwise False.</returns>
		bool TryFinish(WriteableByteSpan output, out int bytesWritten);
#endif
	}

	internal static class ByteSpanExtension
	{
		public static ByteSpan AsByteSpan(this byte[] a) => a is null ? default : new ByteSpan(a);
	}
}
