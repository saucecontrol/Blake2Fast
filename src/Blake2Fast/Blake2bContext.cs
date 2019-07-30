using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if USE_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif

namespace SauceControl.Blake2Fast
{
	unsafe internal partial struct Blake2bContext : IBlake2Incremental
	{
		public const int WordSize = sizeof(ulong);
		public const int BlockWords = 16;
		public const int BlockBytes = BlockWords * WordSize;
		public const int HashWords = 8;
		public const int HashBytes = HashWords * WordSize;
		public const int MaxKeyBytes = HashBytes;

		private static readonly ulong[] iv = new[] {
			0x6A09E667F3BCC908ul, 0xBB67AE8584CAA73Bul,
			0x3C6EF372FE94F82Bul, 0xA54FF53A5F1D36F1ul,
			0x510E527FADE682D1ul, 0x9B05688C2B3E6C1Ful,
			0x1F83D9ABFB41BD6Bul, 0x5BE0CD19137E2179ul
		};

		private fixed byte b[BlockBytes];
		private fixed ulong h[HashWords];
		private fixed ulong t[2];
		private fixed ulong f[2];
		private uint c;
		private uint outlen;

#if FAST_SPAN
		private static ReadOnlySpan<byte> ivle => new byte[] {
			0x08, 0xC9, 0xBC, 0xF3, 0x67, 0xE6, 0x09, 0x6A,
			0x3B, 0xA7, 0xCA, 0x84, 0x85, 0xAE, 0x67, 0xBB,
			0x2B, 0xF8, 0x94, 0xFE, 0x72, 0xF3, 0x6E, 0x3C,
			0xF1, 0x36, 0x1D, 0x5F, 0x3A, 0xF5, 0x4F, 0xA5,
			0xD1, 0x82, 0xE6, 0xAD, 0x7F, 0x52, 0x0E, 0x51,
			0x1F, 0x6C, 0x3E, 0x2B, 0x8C, 0x68, 0x05, 0x9B,
			0x6B, 0xBD, 0x41, 0xFB, 0xAB, 0xD9, 0x83, 0x1F,
			0x79, 0x21, 0x7E, 0x13, 0x19, 0xCD, 0xE0, 0x5B
		};
#endif

#if USE_INTRINSICS
		private static ReadOnlySpan<byte> rormask => new byte[] {
			3, 4, 5, 6, 7, 0, 1, 2, 11, 12, 13, 14, 15, 8, 9, 10, //r24
			2, 3, 4, 5, 6, 7, 0, 1, 10, 11, 12, 13, 14, 15, 8, 9  //r16
		};
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void addLength(uint len)
		{
			t[0] += len;
			if (t[0] < len)
				t[1]++;
		}

		private void ensureValidState()
		{
			if (f[0] != 0)
				throw new InvalidOperationException("Hash has already been finalized.");
		}

		private static void compress(Blake2bContext* s, byte* input)
		{
			ulong* m = (ulong*)input;

#if FAST_SPAN
			if (!BitConverter.IsLittleEndian)
			{
				var span = new ReadOnlySpan<byte>(input, BlockBytes);
				m = (ulong*)s->b;
				for (int i = 0; i < BlockWords; i++)
					m[i] = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(i * WordSize, WordSize));
			}
#endif

#if USE_INTRINSICS
#if USE_AVX2
			if (Avx2.IsSupported)
				mixAvx2(s->h, m);
			else
#endif
			if (Sse41.IsSupported)
				mixSse41(s->h, m);
			else
#endif
				mixScalar(s->h, m);
		}

		public void Init(int digestLength = HashBytes, ReadOnlySpan<byte> key = default)
		{
#if !FAST_SPAN
			if (!BitConverter.IsLittleEndian)
				throw new PlatformNotSupportedException("Big-endian platforms not supported");
#endif

			if (digestLength == 0 || (uint)digestLength > HashBytes)
				throw new ArgumentOutOfRangeException(nameof(digestLength), $"Value must be between 1 and {HashBytes}");

			uint keylen = (uint)key.Length;
			if (keylen > MaxKeyBytes)
				throw new ArgumentException($"Key must be between 0 and {MaxKeyBytes} bytes in length", nameof(key));

			outlen = (uint)digestLength;

#if FAST_SPAN
			if (BitConverter.IsLittleEndian)
				Unsafe.CopyBlock(ref Unsafe.As<ulong, byte>(ref h[0]), ref MemoryMarshal.GetReference(ivle), HashBytes);
			else
#endif
				Unsafe.CopyBlock(ref Unsafe.As<ulong, byte>(ref h[0]), ref Unsafe.As<ulong, byte>(ref iv[0]), HashBytes);

			h[0] ^= 0x01010000u ^ (keylen << 8) ^ outlen;

			if (keylen > 0)
			{
				Unsafe.CopyBlock(ref b[0], ref MemoryMarshal.GetReference(key), keylen);
				c = BlockBytes;
			}
		}

		public void Update(ReadOnlySpan<byte> input)
		{
			ensureValidState();

			uint inlen = (uint)input.Length;
			uint clen = 0u;
			uint blockrem = BlockBytes - c;

			if ((c > 0u) && (inlen > blockrem))
			{
				if (blockrem > 0)
					Unsafe.CopyBlockUnaligned(ref b[c], ref MemoryMarshal.GetReference(input), blockrem);

				addLength(BlockBytes);
				fixed (Blake2bContext* s = &this)
					compress(s, s->b);

				clen += blockrem;
				inlen -= blockrem;
				c = 0u;
			}

			if (inlen + clen > BlockBytes)
			{
				fixed (byte* pinput = input)
				fixed (Blake2bContext* s = &this)
				while (inlen > BlockBytes)
				{
					addLength(BlockBytes);
					compress(s, pinput + clen);

					clen += BlockBytes;
					inlen -= BlockBytes;
				}
				c = 0u;
			}

			if (inlen > 0)
			{
				Unsafe.CopyBlockUnaligned(ref b[c], ref MemoryMarshal.GetReference(input.Slice((int)clen)), inlen);
				c += inlen;
			}
		}

		private void finish(Span<byte> hash)
		{
			ensureValidState();

			if (c < BlockBytes)
				Unsafe.InitBlockUnaligned(ref b[c], 0, BlockBytes - c);

			addLength(c);
			f[0] = ~0ul;
			fixed (Blake2bContext* s = &this)
				compress(s, s->b);

#if FAST_SPAN
			if (!BitConverter.IsLittleEndian)
			{
				var span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ulong, byte>(ref h[0]), HashBytes);
				for (int i = 0; i < HashWords; i++)
					h[i] = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(i * WordSize, WordSize));
			}
#endif

			Unsafe.CopyBlock(ref hash[0], ref Unsafe.As<ulong, byte>(ref h[0]), outlen);
		}

		public byte[] Finish()
		{
			byte[] hash = new byte[outlen];
			finish(new Span<byte>(hash));

			return hash;
		}

		public bool TryFinish(Span<byte> output, out int bytesWritten)
		{
			if (output.Length < outlen)
			{
				bytesWritten = 0;
				return false;
			}

			finish(output);
			bytesWritten = (int)outlen;
			return true;
		}
	}
}
