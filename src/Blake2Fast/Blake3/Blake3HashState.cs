// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
#if HWINTRINSICS
using System.Runtime.Intrinsics.X86;
#endif

namespace Blake2Fast.Implementation;

/// <summary>Defines the state associated with an incremental BLAKE3 hashing operation.</summary>
//// <remarks>Instances of this struct must be created by <see cref="Blake3.CreateIncrementalHasher()" />.  An instance created directly will be unusable.</remarks>
[StructLayout(LayoutKind.Sequential)]
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
	private ulong t;
	private uint c;
	private uint f;
	private uint bc;
	private uint cc;
	private uint sc;
	private uint _pad;
	private fixed uint cv[HashWords * 54];

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
		ref uint ri = ref Unsafe.As<byte, uint>(ref input);
		ref uint rt = ref Unsafe.As<ulong, uint>(ref t);
		ref uint rh = ref k[0];
		if ((f & (uint)(Flags.Parent | Flags.ChunkStart)) == 0)
			rh = ref cv[sc * HashWords];

#if HWINTRINSICS
#if NET8_0_OR_GREATER
		if (Avx512F.VL.IsSupported)
			mixAvx512(ref rh, ref ri, ref rt, ref output, trunc);
		else
#endif
		if (Ssse3.IsSupported)
			mixSsse3(ref rh, ref ri, ref rt, ref output, trunc);
		else
#endif
			mixScalar(ref rh, ref ri, ref rt, ref output, trunc);

		f &= ~(uint)Flags.StateFlags;
	}

	internal void Init(ReadOnlySpan<byte> key = default)
	{
		uint keylen = (uint)key.Length;

		if (!BitConverter.IsLittleEndian) ThrowHelper.NoBigEndian();
		if ((keylen & ~KeyBytes) != 0) ThrowHelper.KeyInvalid();

		uint flags;
		ref byte keyref = ref *(byte*)null;
		if (keylen != 0)
		{
			keyref = ref MemoryMarshal.GetReference(key);
			flags = (uint)(Flags.KeyedHash | Flags.ChunkStart);
		}
		else
		{
			keyref = ref MemoryMarshal.GetReference(ivle);
			flags = (uint)Flags.ChunkStart;
		}

		Unsafe.InitBlockUnaligned(ref Unsafe.As<ulong, byte>(ref t), 0, KeyBytes);
		Unsafe.CopyBlockUnaligned(ref Unsafe.As<uint, byte>(ref k[0]), ref keyref, KeyBytes);
		c = BlockBytes;
		f = flags;
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

			compress(ref b[0], ref cv[sc * HashWords], true);

			remaining -= blockrem;
			cc += BlockBytes;
			bc = 0;
		}

		while (remaining > BlockBytes)
		{
			compress(ref rinput, ref cv[sc * HashWords], true);
			rinput = ref Unsafe.Add(ref rinput, BlockBytes);

			remaining -= BlockBytes;
			cc += BlockBytes;
		}

		if (remaining != 0)
		{
			Unsafe.CopyBlockUnaligned(ref b[bc], ref rinput, remaining);
			bc += remaining;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void mergeStack(ulong cc)
	{
		while ((cc & 1) == 0)
		{
			sc--;
			t = 0;
			f |= (uint)Flags.Parent;

			compress(ref Unsafe.As<uint, byte>(ref cv[sc * HashWords]), ref cv[sc * HashWords], true);
			cc >>>= 1;
		}
		sc++;
	}

	private void update(ReadOnlySpan<byte> input)
	{
		uint clen = cc + bc;
		if (clen != 0)
		{
			int crem = Math.Min(ChunkBytes - (int)clen, input.Length);
			updateChunk(input.Slice(0, crem));

			input = input.Slice(crem);
			if (!input.IsEmpty)
			{
				f |= (uint)Flags.ChunkEnd;
				compress(ref b[0], ref cv[sc * HashWords], true);

				ulong chunk = checked(t + 1);
				mergeStack(chunk);

				t = chunk;
				f |= (uint)Flags.ChunkStart;
				cc = bc = 0;
			}
		}

		while (input.Length > ChunkBytes)
		{
			ref byte rinput = ref MemoryMarshal.GetReference(input);
			for (uint i = 1; i <= ChunkBlocks; i++)
			{
				if (i == ChunkBlocks)
					f |= (uint)Flags.ChunkEnd;

				compress(ref rinput, ref cv[sc * HashWords], true);
				rinput = ref Unsafe.Add(ref rinput, BlockBytes);
			}

			ulong chunk = checked(t + 1);
			mergeStack(chunk);

			t = chunk;
			f |= (uint)Flags.ChunkStart;

			input = input.Slice(ChunkBytes);
		}

		if (!input.IsEmpty)
			updateChunk(input);
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
		ref byte rblock = ref b[0];

		while (sc > 0)
		{
			compress(ref rblock, ref cv[sc * HashWords], true);
			sc--;

			t = 0;
			c = BlockBytes;
			f |= (uint)Flags.Parent;
			rblock = ref Unsafe.As<uint, byte>(ref cv[sc * HashWords]);
		}

		f |= (uint)Flags.Root;
		uint flags = f;

		while (!hash.IsEmpty)
		{
			compress(ref rblock, ref cv[2 * HashWords], hash.Length <= HashBytes);
			t++;
			f = flags;

			int hashrem = Math.Min(hash.Length, BlockBytes);

			ref byte hout = ref MemoryMarshal.GetReference(hash);
			if (hashrem is HashBytes)
				Unsafe.CopyBlockUnaligned(ref hout, ref Unsafe.As<uint, byte>(ref cv[2 * HashWords]), HashBytes);
			else
				Unsafe.CopyBlockUnaligned(ref hout, ref Unsafe.As<uint, byte>(ref cv[2 * HashWords]), (uint)hashrem);

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
}
