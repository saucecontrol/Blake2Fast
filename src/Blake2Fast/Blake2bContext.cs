using System;
using System.Runtime.CompilerServices;

#if USE_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif

#if FAST_SPAN
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using ByteSpan = System.ReadOnlySpan<byte>;
using WriteableByteSpan = System.Span<byte>;
#else
using ByteSpan = System.ArraySegment<byte>;
using WriteableByteSpan = System.ArraySegment<byte>;
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
#if USE_INTRINSICS
		private fixed ulong viv[HashWords];
		private fixed byte vrm[32];
#endif
		private uint c;
		private uint outlen;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void addLength(uint len)
		{
			t[0] += len;
			if (t[0] < len)
				t[1]++;
		}

		unsafe private static void compress(Blake2bContext* s, byte* input)
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
			if (Sse41.IsSupported)
				mixSse41(s, m);
			else
#endif
				mixScalar(s, m);
		}

		public void Init(int digestLength = HashBytes, ByteSpan key = default)
		{
#if !FAST_SPAN
			if (!BitConverter.IsLittleEndian)
				throw new PlatformNotSupportedException("Big-endian platforms not supported");
#endif

			if (digestLength == 0 || (uint)digestLength > HashBytes)
				throw new ArgumentOutOfRangeException(nameof(digestLength), $"Value must be between 1 and {HashBytes}");

#if FAST_SPAN
			uint keylen = (uint)key.Length;
#else
			uint keylen = (uint)key.Count;
#endif
			if (keylen > MaxKeyBytes)
				throw new ArgumentException($"Key must be between 0 and {MaxKeyBytes} bytes in length", nameof(key));

			outlen = (uint)digestLength;
			Unsafe.CopyBlock(ref Unsafe.As<ulong, byte>(ref h[0]), ref Unsafe.As<ulong, byte>(ref iv[0]), HashBytes);
			h[0] ^= 0x01010000u ^ (keylen << 8) ^ outlen;

#if USE_INTRINSICS
			Unsafe.CopyBlock(ref Unsafe.As<ulong, byte>(ref viv[0]), ref Unsafe.As<ulong, byte>(ref iv[0]), HashBytes);
			Unsafe.CopyBlock(ref vrm[0], ref rormask[0], 32);
#endif

			if (keylen > 0)
			{
#if FAST_SPAN
				Unsafe.CopyBlock(ref b[0], ref MemoryMarshal.GetReference(key), keylen);
#else
				Unsafe.CopyBlock(ref b[0], ref key.Array[key.Offset], keylen);
#endif
				c = BlockBytes;
			}
		}

		public void Update(ByteSpan input)
		{
#if FAST_SPAN
			uint inlen = (uint)input.Length;
#else
			uint inlen = (uint)input.Count;
#endif
			uint clen = 0u;
			uint blockrem = BlockBytes - c;

			if ((c > 0u) && (inlen > blockrem))
			{
				if (blockrem > 0)
				{
#if FAST_SPAN
					Unsafe.CopyBlockUnaligned(ref b[c], ref MemoryMarshal.GetReference(input), blockrem);
#else
					Unsafe.CopyBlockUnaligned(ref b[c], ref input.Array[input.Offset], blockrem);
#endif
				}
				addLength(BlockBytes);
				fixed (Blake2bContext* s = &this)
					compress(s, s->b);

				clen += blockrem;
				inlen -= blockrem;
				c = 0u;
			}

			if (inlen + clen > BlockBytes)
			{
#if FAST_SPAN
				fixed (byte* pinput = &input[0])
#else
				fixed (byte* pinput = &input.Array[input.Offset])
#endif
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
#if FAST_SPAN
				Unsafe.CopyBlockUnaligned(ref b[c], ref MemoryMarshal.GetReference(input.Slice((int)clen)), inlen);
#else
				Unsafe.CopyBlockUnaligned(ref b[c], ref input.Array[input.Offset + clen], inlen);
#endif
				c += inlen;
			}
		}

		private void finish(WriteableByteSpan hash)
		{
			if (f[0] != 0)
				throw new InvalidOperationException("Hash has already been finalized.");

			if (c < BlockBytes)
				Unsafe.InitBlockUnaligned(ref b[c], 0, BlockBytes - c);

			addLength(c);
			f[0] = unchecked((ulong)~0);
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

#if FAST_SPAN
			Unsafe.CopyBlock(ref hash[0], ref Unsafe.As<ulong, byte>(ref h[0]), outlen);
#else
			Unsafe.CopyBlock(ref hash.Array[hash.Offset], ref Unsafe.As<ulong, byte>(ref h[0]), outlen);
#endif
		}

		public byte[] Finish()
		{
			byte[] hash = new byte[outlen];
			finish(new WriteableByteSpan(hash));

			return hash;
		}

		public bool TryFinish(WriteableByteSpan output, out int bytesWritten)
		{
#if FAST_SPAN
			if (output.Length < outlen)
#else
			if (output.Count < outlen)
#endif
			{
				bytesWritten = 0;
				return false;
			}

			finish(output);
			bytesWritten = (int)outlen;
			return true;
		}

#if !IMPLICIT_BYTESPAN
		public void Update(byte[] input) => Update(input.AsByteSpan());

		public bool TryFinish(byte[] output, out int bytesWritten) => TryFinish(output.AsByteSpan(), out bytesWritten);
#endif
	}
}
