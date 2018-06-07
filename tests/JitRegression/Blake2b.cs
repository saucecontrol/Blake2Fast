using System;
using System.Runtime.CompilerServices;

public enum MixMode
{
	Noop,
	Inlined,
	Preferred
}

unsafe internal partial struct Blake2bContext
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong ror64(ulong x, byte y) => (x >> y) ^ (x << (64 - y));

	private fixed byte b[BlockBytes];
	private fixed ulong h[HashWords];
	private fixed ulong t[2];
	private fixed ulong f[2];
	private uint c;
	private uint outlen;
	internal MixMode mode;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void addLength(uint len)
	{
		this.t[0] += len;
		if (this.t[0] < len)
			this.t[1]++;
	}

	private void compress(byte* data)
	{
		ulong* m = (ulong*)data;

		switch (mode)
		{
			case MixMode.Noop:
				mixNoop(m);
				break;
			case MixMode.Inlined:
				mixManualInline(m);
				break;
			case MixMode.Preferred:
				mixPreferred(m);
				break;
		}
	}

	public void Init(int outlen = HashBytes, byte[] key = null)
	{
		if (!BitConverter.IsLittleEndian)
			throw new PlatformNotSupportedException("Big-endian platforms not supported");

		if (outlen == 0 || (uint)outlen > HashBytes)
			throw new ArgumentOutOfRangeException($"Value must be between 1 and {HashBytes}", nameof(outlen));

		key = key ?? Array.Empty<byte>();
		uint keylen = (uint)key.Length;
		if (keylen > MaxKeyBytes)
			throw new ArgumentException($"Key must be between 0 and {MaxKeyBytes} bytes in length", nameof(key));

		Unsafe.CopyBlock(ref Unsafe.As<ulong, byte>(ref this.h[0]), ref Unsafe.As<ulong, byte>(ref iv[0]), HashBytes);
		this.h[0] ^= 0x01010000u ^ (keylen << 8) ^ (uint)outlen;
		this.outlen = (uint)outlen;

		if (keylen > 0)
		{
			Unsafe.CopyBlock(ref this.b[0], ref key[0], keylen);
			c = BlockBytes;
		}
	}

	public void Update(byte[] data)
	{
		data = data ?? Array.Empty<byte>();
		uint inlen = (uint)data.Length;
		uint clen = 0u;
		uint blockrem = BlockBytes - c;

		if ((c > 0u) && (inlen > blockrem))
		{
			if (blockrem > 0)
				Unsafe.CopyBlockUnaligned(ref this.b[c], ref data[0], blockrem);

			addLength(BlockBytes);
			fixed (Blake2bContext* s = &this)
				compress(s->b);

			clen += blockrem;
			inlen -= blockrem;
			c = 0u;
		}

		if (inlen + clen > BlockBytes)
		{
			while (inlen > BlockBytes)
			{
				addLength(BlockBytes);
				fixed (byte* pdata = &data[0])
					compress(pdata + clen);

				clen += BlockBytes;
				inlen -= BlockBytes;
			}
			c = 0u;
		}

		if (inlen > 0)
		{
			Unsafe.CopyBlockUnaligned(ref this.b[c], ref data[clen], inlen);
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
			compress(s->b);

		var hash = new byte[outlen];
		Unsafe.CopyBlock(ref hash[0], ref Unsafe.As<ulong, byte>(ref this.h[0]), outlen);

		this = default;
		return hash;
	}
}

public static class Blake2b
{
	unsafe public static byte[] ComputeHash(int outlen, byte[] key, byte[] data, MixMode mode)
	{
		var ctx = default(Blake2bContext);
		ctx.mode = mode;
		ctx.Init(outlen, key);
		ctx.Update(data);
		return ctx.Finish();
	}
}
