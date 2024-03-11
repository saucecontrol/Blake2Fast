// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Blake2Fast.Implementation;

/// <summary>Defines the state associated with an incremental BLAKE3 hashing operation.</summary>
//// <remarks>Instances of this struct must be created by <see cref="Blake3.CreateIncrementalHasher()" />.  An instance created directly will be unusable.</remarks>
#if BLAKE_PUBLIC
public
#else
internal
#endif
unsafe partial struct Blake3HashState : IBlakeIncremental
{
	[Flags]
	private enum Flags : uint
	{
		ChunkStart         = 1u << 0,
		ChunkEnd           = 1u << 1,
		Parent             = 1u << 2,
		Root               = 1u << 3,
		KeyedHash          = 1u << 4,
		DeriveKeyContext   = 1u << 5,
		DeriveKeyMaterial  = 1u << 6,
		StateFlags         = 0b_111u
	}

	internal const int WordSize = sizeof(uint);
	internal const int BlockWords = 16;
	internal const int BlockBytes = BlockWords * WordSize;
	internal const int ChunkBlocks = 16;
	internal const int ChunkBytes = ChunkBlocks * BlockBytes;
	internal const int HashWords = 8;
	internal const int HashBytes = HashWords * WordSize;
	internal const int KeyBytes = HashBytes;

 	private fixed byte b[BlockBytes];
	private fixed uint k[HashWords];
	private fixed uint h[HashWords];
	private ulong t;
	private uint c;
	private uint f;
	private uint bc;
	private uint cc;
	private uint sc;
	private uint _pad;
	private fixed uint cv[8 * 54];

	private static ReadOnlySpan<byte> ivle => [
		0x67, 0xe6, 0x09, 0x6a,
		0x85, 0xae, 0x67, 0xbb,
		0x72, 0xf3, 0x6e, 0x3c,
		0x3a, 0xf5, 0x4f, 0xa5,
		0x7f, 0x52, 0x0e, 0x51,
		0x8c, 0x68, 0x05, 0x9b,
		0xab, 0xd9, 0x83, 0x1f,
		0x19, 0xcd, 0xe0, 0x5b
	];

	/// <inheritdoc />
	public readonly int DigestLength => HashBytes;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void compress(ref byte input, ref uint output, bool trunc)
	{
		fixed (byte* pin = &input)
		fixed (uint* pout = &output)
		fixed (Blake3HashState* s = &this)
		{
			uint* sh = s->h;
			uint* m = (uint*)pin;

			mixScalar(sh, m, pout, trunc);
		}

		f &= ~(uint)Flags.StateFlags;
	}

	internal void Init(ReadOnlySpan<byte> key = default)
	{
		uint keylen = (uint)key.Length;

		if (!BitConverter.IsLittleEndian) ThrowHelper.NoBigEndian();
		if ((keylen & ~KeyBytes) != 0) ThrowHelper.KeyInvalid();

		ref byte keyref = ref *(byte*)null;
		if (keylen != 0)
		{
			keyref = ref MemoryMarshal.GetReference(key);
			f = (uint)(Flags.KeyedHash | Flags.ChunkStart);
		}
		else
		{
			keyref = ref MemoryMarshal.GetReference(ivle);
			f = (uint)Flags.ChunkStart;
		}

		Unsafe.CopyBlockUnaligned(ref Unsafe.As<uint, byte>(ref k[0]), ref keyref, KeyBytes);
		Unsafe.CopyBlockUnaligned(ref Unsafe.As<uint, byte>(ref h[0]), ref keyref, KeyBytes);
		c = BlockBytes;
	}

	private void updateChunk(ReadOnlySpan<byte> input)
	{
		uint remaining = (uint)input.Length;
		ref byte rinput = ref MemoryMarshal.GetReference(input);

		uint blockrem;
		if ((bc != 0) && (remaining > (blockrem = BlockBytes - bc)))
		{
			if (blockrem != 0)
			{
				Unsafe.CopyBlockUnaligned(ref b[bc], ref rinput, blockrem);
				rinput = ref Unsafe.Add(ref rinput, (nint)blockrem);
			}

			compress(ref b[0], ref h[0], true);

			remaining -= blockrem;
			cc += BlockBytes;
			bc = 0;
		}

		if (remaining > BlockBytes)
		{
			uint cb = (remaining - 1) & ~((uint)BlockBytes - 1);
			for (uint i = 0; i < cb / BlockBytes; i++)
			{
				compress(ref rinput, ref h[0], true);
				rinput = ref Unsafe.Add(ref rinput, BlockBytes);
			}

			remaining -= cb;
			cc += cb;
		}

		if (remaining != 0)
		{
			Unsafe.CopyBlockUnaligned(ref b[bc], ref rinput, remaining);
			bc += remaining;
		}
	}

	private void update(ReadOnlySpan<byte> input)
	{
		while (!input.IsEmpty)
		{
			if (cc + bc == ChunkBytes)
			{
				f |= (uint)Flags.ChunkEnd;
				compress(ref b[0], ref h[0], true);

				ulong chunk = checked(t + 1);
				ulong depth = chunk;
				while ((depth & 1) == 0)
				{
					sc--;
					Unsafe.CopyBlockUnaligned(ref b[0], ref Unsafe.As<uint, byte>(ref cv[sc * HashWords]), HashBytes);
					Unsafe.CopyBlockUnaligned(ref b[HashBytes], ref Unsafe.As<uint, byte>(ref h[0]), HashBytes);
					Unsafe.CopyBlockUnaligned(ref Unsafe.As<uint, byte>(ref h[0]), ref Unsafe.As<uint, byte>(ref k[0]), KeyBytes);
					t = 0;
					f |= (uint)Flags.Parent;

					compress(ref b[0], ref h[0], true);
					depth >>>= 1;
				}
				Unsafe.CopyBlockUnaligned(ref Unsafe.As<uint, byte>(ref cv[(nuint)sc * HashWords]), ref Unsafe.As<uint, byte>(ref h[0]), HashBytes);
				sc++;

				Unsafe.CopyBlockUnaligned(ref Unsafe.As<uint, byte>(ref h[0]), ref Unsafe.As<uint, byte>(ref k[0]), KeyBytes);
				t = chunk;
				f |= (uint)Flags.ChunkStart;
				cc = bc = 0;
			}

			int chunkrem = Math.Min(ChunkBytes - (int)(cc + bc), input.Length);
			updateChunk(input.Slice(0, chunkrem));
			input = input.Slice(chunkrem);
		}
	}

	/// <inheritdoc />
	public void Update<T>(ReadOnlySpan<T> input) where T : struct
	{
		ThrowHelper.ThrowIfIsRefOrContainsRefs<T>();
		if ((f & (uint)Flags.Root) != 0) ThrowHelper.HashFinalized();

		update(MemoryMarshal.AsBytes(input));
	}

	/// <inheritdoc />
	public void Update<T>(Span<T> input) where T : struct => Update((ReadOnlySpan<T>)input);

	/// <inheritdoc />
	public void Update<T>(ArraySegment<T> input) where T : struct => Update((ReadOnlySpan<T>)input);

	/// <inheritdoc />
	public void Update<T>(T[] input) where T : struct => Update((ReadOnlySpan<T>)input);

	/// <inheritdoc />
	public void Update<T>(T input) where T : struct
	{
		ThrowHelper.ThrowIfIsRefOrContainsRefs<T>();

		if ((int)bc > BlockBytes - sizeof(T))
		{
			Update(new ReadOnlySpan<byte>(&input, sizeof(T)));
			return;
		}

		if ((f & (uint)Flags.Root) != 0) ThrowHelper.HashFinalized();

		Unsafe.WriteUnaligned(ref b[bc], input);
		bc += (uint)sizeof(T);
	}

	private void finish(Span<byte> hash)
	{
		if ((k[0] | f & (uint)Flags.KeyedHash) == 0) ThrowHelper.HashNotInitialized();
		if ((f & (uint)Flags.Root) != 0) ThrowHelper.HashFinalized();

		if (bc < BlockBytes)
		{
			Unsafe.InitBlockUnaligned(ref b[bc], 0, BlockBytes - bc);
			c = bc;
		}

		f |= (uint)Flags.ChunkEnd;

		while (sc-- != 0)
		{
			compress(ref b[0], ref h[0], true);

			Unsafe.CopyBlockUnaligned(ref b[0], ref Unsafe.As<uint, byte>(ref cv[sc * HashWords]), HashBytes);
			Unsafe.CopyBlockUnaligned(ref b[HashBytes], ref Unsafe.As<uint, byte>(ref h[0]), HashBytes);
			Unsafe.CopyBlockUnaligned(ref Unsafe.As<uint, byte>(ref h[0]), ref Unsafe.As<uint, byte>(ref k[0]), KeyBytes);
			t = 0;
			c = BlockBytes;
			f |= (uint)Flags.Parent;
		}

		f |= (uint)Flags.Root;
		uint flags = f;

		while (!hash.IsEmpty)
		{
			compress(ref b[0], ref cv[0], hash.Length <= HashBytes);
			t++;
			f = flags;

			int hashrem = Math.Min(hash.Length, BlockBytes);

			ref byte hout = ref MemoryMarshal.GetReference(hash);
			if (hashrem is HashBytes)
				Unsafe.CopyBlockUnaligned(ref hout, ref Unsafe.As<uint, byte>(ref cv[0]), HashBytes);
			else
				Unsafe.CopyBlockUnaligned(ref hout, ref Unsafe.As<uint, byte>(ref cv[0]), (uint)hashrem);

			hash = hash.Slice(hashrem);
		}
	}

	/// <inheritdoc />
	public byte[] Finish()
	{
		byte[] hash = new byte[HashBytes];
		finish(hash);

		return hash;
	}

	/// <inheritdoc />
	public void Finish(Span<byte> output)
	{
		if (output.IsEmpty) ThrowHelper.OutputTooSmall(1);

		finish(output);
	}

	/// <inheritdoc />
	public bool TryFinish(Span<byte> output, out int bytesWritten)
	{
		if (output.IsEmpty)
		{
			bytesWritten = 0;
			return false;
		}

		finish(output);
		bytesWritten = output.Length;
		return true;
	}
}
