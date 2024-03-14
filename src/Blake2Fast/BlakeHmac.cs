// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

#if BLAKE_CRYPTOGRAPHY
using System;
using System.Security.Cryptography;

namespace Blake2Fast;

internal sealed class BlakeHmac : HMAC
{
	internal enum Algorithm
	{
		Blake2b,
		Blake2s,
		Blake3
	}

	private readonly Algorithm alg;
	private IBlakeIncremental impl;

	public BlakeHmac(Algorithm hashAlg, int hashBytes, ReadOnlySpan<byte> key)
	{
		alg = hashAlg;
		HashSizeValue = hashBytes * 8;
		HashName = alg.ToString();

		if (key.Length > 0)
			KeyValue = key.ToArray();

		impl = createIncrementalInstance(hashBytes);
	}

	public override byte[] Key
	{
		get => base.Key;
		set => throw new InvalidOperationException($"{nameof(Key)} cannot be changed.  You must create a new {nameof(HMAC)} instance.");
	}

	public sealed override void Initialize() => impl = createIncrementalInstance(HashSizeValue / 8);

	protected sealed override void HashCore(byte[] array, int ibStart, int cbSize) => impl.Update(new ReadOnlySpan<byte>(array, ibStart, cbSize));

	protected sealed override byte[] HashFinal() => impl.Finish();

#if BUILTIN_SPAN
	protected sealed override void HashCore(ReadOnlySpan<byte> source) => impl.Update(source);

	protected sealed override bool TryHashFinal(Span<byte> destination, out int bytesWritten)
	{
		uint digestLength = (uint)HashSizeValue / 8;
		if ((uint)destination.Length < digestLength)
		{
			bytesWritten = 0;
			return false;
		}

		destination = destination.Slice(0, (int)digestLength);
		impl.Finish(destination);
		bytesWritten = destination.Length;
		return true;
	}
#endif

	private IBlakeIncremental createIncrementalInstance(int digestLength)
	{
		var key = new ReadOnlySpan<byte>(KeyValue);

		switch (alg)
		{
			case Algorithm.Blake2b:
				Blake2b.CreateIncrementalHasher(digestLength, key, out var hs2b);
				return hs2b;
			case Algorithm.Blake2s:
				Blake2s.CreateIncrementalHasher(digestLength, key, out var hs2s);
				return hs2s;
			default:
				Blake3.CreateIncrementalHasher(key, out var hs3);
				return hs3;
		};
	}
}
#endif