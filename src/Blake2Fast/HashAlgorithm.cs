#if USE_CRYPTOGRAPHY
using System;
using System.Security.Cryptography;

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
		private readonly int HashSizeValue;
		public sealed override int HashSize => HashSizeValue;
#endif

		private readonly Blake2Algorithm alg;
		private IBlake2Incremental impl;

#nullable disable // until the analyzer understands `Initialize()` sets `impl`
		public Blake2HMAC(Blake2Algorithm hashAlg, int hashBytes, ReadOnlySpan<byte> key)
		{
			alg = hashAlg;
			HashSizeValue = hashBytes * 8;
			HashName = alg.ToString();

			if (key.Length > 0)
#if NETSTANDARD1_3
				base.Key = key.ToArray();
#else
				KeyValue = key.ToArray();
#endif

			Initialize();
		}
#nullable enable

		public override byte[] Key
		{
			get => base.Key;
			set => throw new InvalidOperationException($"{nameof(Key)} cannot be changed.  You must create a new {nameof(HMAC)} instance.");
		}

		public sealed override void Initialize()
		{
#if NETSTANDARD1_3
			var key = new ReadOnlySpan<byte>(base.Key);
#else
			var key = new ReadOnlySpan<byte>(KeyValue);
#endif

			impl = alg == Blake2Algorithm.Blake2b ?
				Blake2b.CreateIncrementalHasher(HashSizeValue / 8, key) :
				Blake2s.CreateIncrementalHasher(HashSizeValue / 8, key);
		}

		protected sealed override void HashCore(byte[] array, int ibStart, int cbSize) => impl.Update(new ReadOnlySpan<byte>(array, ibStart, cbSize));

		protected sealed override byte[] HashFinal() => impl.Finish();

#if FAST_SPAN
		protected sealed override void HashCore(ReadOnlySpan<byte> source) => impl.Update(source);

		protected sealed override bool TryHashFinal(Span<byte> destination, out int bytesWritten) => impl.TryFinish(destination, out bytesWritten);
#endif
	}
}
#endif
