using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if USE_INTRINSICS
using System.Runtime.Intrinsics;
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

		private static readonly ulong[] iv;

#if USE_INTRINSICS
		private static readonly Vector128<ulong> v128iv0;
		private static readonly Vector128<ulong> v128iv1;
		private static readonly Vector128<ulong> v128iv2;
		private static readonly Vector128<ulong> v128iv3;
		private static readonly Vector128<sbyte> v128rm0;
		private static readonly Vector128<sbyte> v128rm1;

#if USE_AVX2
		private static readonly Vector256<ulong> v256iv0;
		private static readonly Vector256<ulong> v256iv1;
		private static readonly Vector256<sbyte> v256rm0;
		private static readonly Vector256<sbyte> v256rm1;
#endif
#endif

		private fixed byte b[BlockBytes];
		private fixed ulong h[HashWords];
		private fixed ulong t[2];
		private fixed ulong f[2];
		private uint c;
		private uint outlen;

		unsafe static Blake2bContext()
		{
			iv = new[] {
				0x6A09E667F3BCC908ul, 0xBB67AE8584CAA73Bul,
				0x3C6EF372FE94F82Bul, 0xA54FF53A5F1D36F1ul,
				0x510E527FADE682D1ul, 0x9B05688C2B3E6C1Ful,
				0x1F83D9ABFB41BD6Bul, 0x5BE0CD19137E2179ul
			};

#if USE_INTRINSICS
			fixed (ulong* p = &iv[0])
			{
				v128iv0 = Sse2.LoadVector128(p);
				v128iv1 = Sse2.LoadVector128(p + 2);
				v128iv2 = Sse2.LoadVector128(p + 4);
				v128iv3 = Sse2.LoadVector128(p + 6);
			}

			sbyte* rormask = stackalloc sbyte[] {
				3, 4, 5, 6, 7, 0, 1, 2, 11, 12, 13, 14, 15, 8, 9, 10, //r24
				2, 3, 4, 5, 6, 7, 0, 1, 10, 11, 12, 13, 14, 15, 8, 9  //r16
			};

			v128rm0 = Sse2.LoadVector128(rormask);
			v128rm1 = Sse2.LoadVector128(rormask + 16);

#if USE_AVX2
			v256iv0 = Vector256.Create(v128iv0, v128iv1);
			v256iv1 = Vector256.Create(v128iv2, v128iv3);

			v256rm0 = Vector256.Create(v128rm0, v128rm0);
			v256rm1 = Vector256.Create(v128rm1, v128rm1);
#endif
#endif
		}

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
#if USE_AVX2
			if (Avx2.IsSupported)
				mixAvx2(s, m);
			else
#endif
			if (Sse41.IsSupported)
				mixSse41(s, m);
			else
#endif
				mixScalar(s, m);
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
				fixed (byte* pinput = &input[0])
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
			if (f[0] != 0)
				throw new InvalidOperationException("Hash has already been finalized.");

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
