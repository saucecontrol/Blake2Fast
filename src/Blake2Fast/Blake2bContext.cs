using System;
using System.Runtime.CompilerServices;

#if FAST_SPAN
using System.Buffers.Binary;
using System.Runtime.InteropServices;
#endif
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
#if USE_INTRINSICS
		private fixed ulong viv[HashWords];
		private fixed byte vrm[32];
#endif
		private uint c;
		private uint outlen;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void addLength(uint len)
		{
			this.t[0] += len;
			if (this.t[0] < len)
				this.t[1]++;
		}

		unsafe private static void compress(Blake2bContext* s, byte* data)
		{
			ulong* m = (ulong*)data;

#if FAST_SPAN
			if (!BitConverter.IsLittleEndian)
			{
				var span = new ReadOnlySpan<byte>(data, BlockBytes);
				m = (ulong*)s->b;
				for (int i = 0; i < BlockWords; i++)
					m[i] = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(i * WordSize, WordSize));
			}
#endif

#if USE_INTRINSICS
			if (Sse.IsSupported && Sse2.IsSupported && Ssse3.IsSupported && Sse41.IsSupported)
			mixSse41(s, m);
			else
#endif
			mixScalar(s, m);
		}

#if FAST_SPAN
		public void Init(int outlen = HashBytes, ReadOnlySpan<byte> key = default)
#else
		public void Init(int outlen = HashBytes, byte[] key = null)
#endif
		{
#if !FAST_SPAN
			if (!BitConverter.IsLittleEndian)
				throw new PlatformNotSupportedException("Big-endian platforms not supported");
#endif

			if (outlen == 0 || (uint)outlen > HashBytes)
				throw new ArgumentOutOfRangeException($"Value must be between 1 and {HashBytes}", nameof(outlen));

#if !FAST_SPAN
			key = key ?? Array.Empty<byte>();
#endif
			uint keylen = (uint)key.Length;
			if (keylen > MaxKeyBytes)
				throw new ArgumentException($"Key must be between 0 and {MaxKeyBytes} bytes in length", nameof(key));

			Unsafe.CopyBlock(ref Unsafe.As<ulong, byte>(ref this.h[0]), ref Unsafe.As<ulong, byte>(ref iv[0]), HashBytes);
			this.h[0] ^= 0x01010000u ^ (keylen << 8) ^ (uint)outlen;
			this.outlen = (uint)outlen;

#if USE_INTRINSICS
			Unsafe.CopyBlock(ref Unsafe.As<ulong, byte>(ref this.viv[0]), ref Unsafe.As<ulong, byte>(ref iv[0]), HashBytes);
			Unsafe.CopyBlock(ref this.vrm[0], ref rormask[0], 32);
#endif

			if (keylen > 0)
			{
#if FAST_SPAN
				Unsafe.CopyBlock(ref this.b[0], ref MemoryMarshal.GetReference(key), keylen);
#else
				Unsafe.CopyBlock(ref this.b[0], ref key[0], keylen);
#endif
				c = BlockBytes;
			}
		}

#if FAST_SPAN
		public void Update(ReadOnlySpan<byte> data)
#else
		public void Update(byte[] data)
#endif
		{
#if !FAST_SPAN
			data = data ?? Array.Empty<byte>();
#endif
			uint inlen = (uint)data.Length;
			uint clen = 0u;
			uint blockrem = BlockBytes - c;

			if ((c > 0u) && (inlen > blockrem))
			{
				if (blockrem > 0)
				{
#if FAST_SPAN
					Unsafe.CopyBlockUnaligned(ref this.b[c], ref MemoryMarshal.GetReference(data), blockrem);
#else
					Unsafe.CopyBlockUnaligned(ref this.b[c], ref data[0], blockrem);
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
				fixed (byte* pdata = &data[0])
				fixed (Blake2bContext* s = &this)
				while (inlen > BlockBytes)
				{
					addLength(BlockBytes);
					compress(s, pdata + clen);

					clen += BlockBytes;
					inlen -= BlockBytes;
				}
				c = 0u;
			}

			if (inlen > 0)
			{
#if FAST_SPAN
				Unsafe.CopyBlockUnaligned(ref this.b[c], ref MemoryMarshal.GetReference(data.Slice((int)clen)), inlen);
#else
				Unsafe.CopyBlockUnaligned(ref this.b[c], ref data[clen], inlen);
#endif
				c += inlen;
			}
		}

		public byte[] Finish()
		{
			if (this.f[0] != 0)
				throw new InvalidOperationException(nameof(Finish) + " has already been used.  It cannot be called again on this instance.");

			if (c < BlockBytes)
				Unsafe.InitBlockUnaligned(ref this.b[c], 0, BlockBytes - c);

			addLength(c);
			this.f[0] = unchecked((ulong)~0);
			fixed (Blake2bContext* s = &this)
				compress(s, s->b);

#if FAST_SPAN
			if (!BitConverter.IsLittleEndian)
			{
				var span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ulong, byte>(ref this.h[0]), HashBytes);
				for (int i = 0; i < HashWords; i++)
					this.h[i] = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(i * WordSize, WordSize));
			}
#endif

			var hash = new byte[outlen];
			Unsafe.CopyBlock(ref hash[0], ref Unsafe.As<ulong, byte>(ref this.h[0]), outlen);

			this = default;
			return hash;
		}
	}

	public static class Blake2b
	{
#if FAST_SPAN
		unsafe public static byte[] ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(Blake2bContext.HashBytes, ReadOnlySpan<byte>.Empty, data);

		unsafe public static byte[] ComputeHash(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data) => ComputeHash(Blake2bContext.HashBytes, key, data);

		unsafe public static byte[] ComputeHash(int outlen, ReadOnlySpan<byte> data) => ComputeHash(outlen, ReadOnlySpan<byte>.Empty, data);

		unsafe public static byte[] ComputeHash(int outlen, ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
#else
		unsafe public static byte[] ComputeHash(byte[] data) => ComputeHash(Blake2bContext.HashBytes, Array.Empty<byte>(), data);

		unsafe public static byte[] ComputeHash(byte[] key, byte[] data) => ComputeHash(Blake2bContext.HashBytes, key, data);

		unsafe public static byte[] ComputeHash(int outlen, byte[] data) => ComputeHash(outlen, Array.Empty<byte>(), data);

		unsafe public static byte[] ComputeHash(int outlen, byte[] key, byte[] data)
#endif
		{
			var ctx = default(Blake2bContext);
			ctx.Init(outlen, key);
			ctx.Update(data);
			return ctx.Finish();
		}

#if FAST_SPAN
		unsafe public static IBlake2Incremental CreateIncrementalHasher() => CreateIncrementalHasher(Blake2bContext.HashBytes, ReadOnlySpan<byte>.Empty);

		unsafe public static IBlake2Incremental CreateIncrementalHasher(ReadOnlySpan<byte> key) => CreateIncrementalHasher(Blake2bContext.HashBytes, key);

		unsafe public static IBlake2Incremental CreateIncrementalHasher(int outlen) => CreateIncrementalHasher(outlen, ReadOnlySpan<byte>.Empty);

		unsafe public static IBlake2Incremental CreateIncrementalHasher(int outlen, ReadOnlySpan<byte> key)
#else
		unsafe public static IBlake2Incremental CreateIncrementalHasher() => CreateIncrementalHasher(Blake2bContext.HashBytes, Array.Empty<byte>());

		unsafe public static IBlake2Incremental CreateIncrementalHasher(byte[] key) => CreateIncrementalHasher(Blake2bContext.HashBytes, key);

		unsafe public static IBlake2Incremental CreateIncrementalHasher(int outlen) => CreateIncrementalHasher(outlen, Array.Empty<byte>());

		unsafe public static IBlake2Incremental CreateIncrementalHasher(int outlen, byte[] key)
#endif
		{
			var ctx = default(Blake2bContext);
			ctx.Init(outlen, key);
			return ctx;
		}
	}
}
