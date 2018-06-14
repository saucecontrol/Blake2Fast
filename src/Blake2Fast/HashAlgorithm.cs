#if !NETSTANDARD1_0
using System;
using System.Security.Cryptography;

#if FAST_SPAN
using ByteSpan = System.ReadOnlySpan<byte>;
#else
using ByteSpan = System.ArraySegment<byte>;
#endif

namespace SauceControl.Blake2Fast
{
	internal enum Blake2Algorithm
	{
		Blake2b,
		Blake2s
	}

	internal sealed class Blake2HMAC : HMAC
	{
#if NETSTANDARD1_3
		private int HashSizeValue;
		public sealed override int HashSize => HashSizeValue;
#endif

		private Blake2Algorithm alg;
		private IBlake2Incremental impl;

		public Blake2HMAC(Blake2Algorithm hashAlg, int hashBytes, ByteSpan key)
		{
			alg = hashAlg;
			HashSizeValue = hashBytes * 8;
			HashName = alg.ToString();

#if FAST_SPAN
			if (key.Length > 0)
				KeyValue = key.ToArray();
#else
			if (key.Count > 0)
			{
				var keyCopy = new byte[key.Count];
				Buffer.BlockCopy(key.Array, key.Offset, keyCopy, 0, key.Count);
#if NETSTANDARD1_3
				base.Key = keyCopy;
#else
				KeyValue = keyCopy;
#endif
			}
#endif

				Initialize();
		}

		public override byte[] Key
		{
			get => base.Key;
			set => throw new InvalidOperationException($"{nameof(Key)} cannot be changed.  You must create a new {nameof(HMAC)} instance.");
		}

		public sealed override void Initialize()
		{
			impl = alg == Blake2Algorithm.Blake2b ?
#if NETSTANDARD1_3
				Blake2b.CreateIncrementalHasher(HashSizeValue / 8, base.Key.AsByteSpan()) :
				Blake2s.CreateIncrementalHasher(HashSizeValue / 8, base.Key.AsByteSpan());
#else
				Blake2b.CreateIncrementalHasher(HashSizeValue / 8, KeyValue.AsByteSpan()) :
				Blake2s.CreateIncrementalHasher(HashSizeValue / 8, KeyValue.AsByteSpan());
#endif
		}

		protected sealed override void HashCore(byte[] array, int ibStart, int cbSize) => impl.Update(new ByteSpan(array, ibStart, cbSize));

		protected sealed override byte[] HashFinal() => impl.Finish();

#if FAST_SPAN
		protected sealed override void HashCore(ReadOnlySpan<byte> source) => impl.Update(source);

		protected sealed override bool TryHashFinal(Span<byte> destination, out int bytesWritten) => impl.TryFinish(destination, out bytesWritten);
#endif
	}
}
#endif
