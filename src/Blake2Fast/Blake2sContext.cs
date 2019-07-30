using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if USE_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif

namespace SauceControl.Blake2Fast
{
	unsafe internal partial struct Blake2sContext : IBlake2Incremental
	{
		public const int WordSize = sizeof(uint);
		public const int BlockWords = 16;
		public const int BlockBytes = BlockWords * WordSize;
		public const int HashWords = 8;
		public const int HashBytes = HashWords * WordSize;
		public const int MaxKeyBytes = HashBytes;

		private static readonly uint[] iv = new[] {
			0x6A09E667u, 0xBB67AE85u,
			0x3C6EF372u, 0xA54FF53Au,
			0x510E527Fu, 0x9B05688Cu,
			0x1F83D9ABu, 0x5BE0CD19u
		};

		private fixed byte b[BlockBytes];
		private fixed uint h[HashWords];
		private fixed uint t[2];
		private fixed uint f[2];
		private uint c;
		private uint outlen;

#if FAST_SPAN
		private static ReadOnlySpan<byte> ivle => new byte[] {
			0x67, 0xE6, 0x09, 0x6A,
			0x85, 0xAE, 0x67, 0xBB,
			0x72, 0xF3, 0x6E, 0x3C,
			0x3A, 0xF5, 0x4F, 0xA5,
			0x7F, 0x52, 0x0E, 0x51,
			0x8C, 0x68, 0x05, 0x9B,
			0xAB, 0xD9, 0x83, 0x1F,
			0x19, 0xCD, 0xE0, 0x5B
		};
#endif

#if USE_INTRINSICS
		private static ReadOnlySpan<byte> rormask => new byte[] {
			2, 3, 0, 1, 6, 7, 4, 5, 10, 11, 8, 9, 14, 15, 12, 13, //r16
			1, 2, 3, 0, 5, 6, 7, 4, 9, 10, 11, 8, 13, 14, 15, 12  //r8
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

		private static void compress(Blake2sContext* s, byte* input)
		{
			uint* m = (uint*)input;

#if FAST_SPAN
			if (!BitConverter.IsLittleEndian)
			{
				var span = new ReadOnlySpan<byte>(input, BlockBytes);
				m = (uint*)s->b;
				for (int i = 0; i < BlockWords; i++)
					m[i] = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(i * WordSize, WordSize));
			}
#endif

#if USE_INTRINSICS
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
				Unsafe.CopyBlock(ref Unsafe.As<uint, byte>(ref h[0]), ref MemoryMarshal.GetReference(ivle), HashBytes);
			else
#endif
				Unsafe.CopyBlock(ref Unsafe.As<uint, byte>(ref h[0]), ref Unsafe.As<uint, byte>(ref iv[0]), HashBytes);

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
				fixed (Blake2sContext* s = &this)
					compress(s, s->b);

				clen += blockrem;
				inlen -= blockrem;
				c = 0u;
			}

			if (inlen + clen > BlockBytes)
			{
				fixed (byte* pinput = input)
				fixed (Blake2sContext* s = &this)
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
			f[0] = ~0u;
			fixed (Blake2sContext* s = &this)
				compress(s, s->b);

#if FAST_SPAN
			if (!BitConverter.IsLittleEndian)
			{
				var span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<uint, byte>(ref h[0]), HashBytes);
				for (int i = 0; i < HashWords; i++)
					h[i] = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(i * WordSize, WordSize));
			}
#endif

			Unsafe.CopyBlock(ref hash[0], ref Unsafe.As<uint, byte>(ref h[0]), outlen);
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
