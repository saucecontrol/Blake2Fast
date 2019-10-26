using System;
using System.Linq;

using Xunit;
using Blake2Fast;

public class RfcSelfTest
{
	private static readonly byte[] blake2bCheck = new byte[] {
		0xC2, 0x3A, 0x78, 0x00, 0xD9, 0x81, 0x23, 0xBD,
		0x10, 0xF5, 0x06, 0xC6, 0x1E, 0x29, 0xDA, 0x56,
		0x03, 0xD7, 0x63, 0xB8, 0xBB, 0xAD, 0x2E, 0x73,
		0x7F, 0x5E, 0x76, 0x5A, 0x7B, 0xCC, 0xD4, 0x75
	};

	private static readonly byte[] blake2sCheck = new byte[] {
		0x6A, 0x41, 0x1F, 0x08, 0xCE, 0x25, 0xAD, 0xCD,
		0xFB, 0x02, 0xAB, 0xA6, 0x41, 0x45, 0x1C, 0xEC,
		0x53, 0xC5, 0x98, 0xB2, 0x4F, 0x4F, 0xC7, 0x87,
		0xFB, 0xDC, 0x88, 0x79, 0x7F, 0x4C, 0x1D, 0xFE
	};

	private static byte[] getTestSequence(int len)
	{
		var buff = new byte[len];
		uint a = 0xDEAD4BADu * (uint)buff.Length;
		uint b = 1;

		for (int i = 0; i < buff.Length; i++)
		{
			uint t = a + b;
			a = b;
			b = t;
			buff[i] = (byte)(t >> 24);
		}

		return buff;
	}

	private static bool blake2bSelfTest()
	{
		var inc = Blake2b.CreateIncrementalHasher(blake2bCheck.Length);

		foreach (int diglen in new[] { 20, 32, 48, 64 })
		foreach (int msglen in new[] { 0, 3, 128, 129, 255, 1024 })
		{
			var msg = getTestSequence(msglen);
			var key = getTestSequence(diglen);

			inc.Update(Blake2b.ComputeHash(diglen, msg));
			inc.Update(Blake2b.ComputeHash(diglen, key, msg));
		}

		return inc.Finish().SequenceEqual(blake2bCheck);
	}

	private static bool blake2sSelfTest()
	{
		var inc = Blake2s.CreateIncrementalHasher(blake2sCheck.Length);

		foreach (int diglen in new[] { 16, 20, 28, 32 })
		foreach (int msglen in new[] { 0, 3, 64, 65, 255, 1024 })
		{
			var msg = getTestSequence(msglen);
			var key = getTestSequence(diglen);

			inc.Update(Blake2s.ComputeHash(diglen, msg));
			inc.Update(Blake2s.ComputeHash(diglen, key, msg));
		}

		return inc.Finish().SequenceEqual(blake2sCheck);
	}

	private static bool blake2bNoAllocSelfTest()
	{
		Span<byte> buff = stackalloc byte[Blake2b.DefaultDigestLength];
		var inc = Blake2b.CreateIncrementalHasher(blake2bCheck.Length);

		foreach (int diglen in new[] { 20, 32, 48, 64 })
		foreach (int msglen in new[] { 0, 3, 128, 129, 255, 1024 })
		{
			var msg = getTestSequence(msglen);
			var key = getTestSequence(diglen);

			Blake2b.ComputeAndWriteHash(diglen, msg, buff);
			inc.Update(buff.Slice(0, diglen));

			Blake2b.ComputeAndWriteHash(diglen, key, msg, buff);
			inc.Update(buff.Slice(0, diglen));
		}

		return inc.TryFinish(buff, out int len) && buff.Slice(0, len).SequenceEqual(blake2bCheck);
	}

	private static bool blake2sNoAllocSelfTest()
	{
		Span<byte> buff = stackalloc byte[Blake2b.DefaultDigestLength];
		var inc = Blake2s.CreateIncrementalHasher(blake2sCheck.Length);

		foreach (int diglen in new[] { 16, 20, 28, 32 })
		foreach (int msglen in new[] { 0, 3, 64, 65, 255, 1024 })
		{
			var msg = getTestSequence(msglen);
			var key = getTestSequence(diglen);

			Blake2s.ComputeAndWriteHash(diglen, msg, buff);
			inc.Update(buff.Slice(0, diglen));

			Blake2s.ComputeAndWriteHash(diglen, key, msg, buff);
			inc.Update(buff.Slice(0, diglen));
		}

		return inc.TryFinish(buff, out int len) && buff.Slice(0, len).SequenceEqual(blake2sCheck);
	}

	private static bool blake2bHmacSelfTest()
	{
#if ICRYPTOTRANSFORM
		using var inc = Blake2b.CreateHashAlgorithm(blake2bCheck.Length);
#else
		var inc = Blake2b.CreateIncrementalHasher(blake2bCheck.Length);
#endif

		foreach (int diglen in new[] { 20, 32, 48, 64 })
		{
			using var halg = Blake2b.CreateHashAlgorithm(diglen);
			using var hmac = Blake2b.CreateHMAC(diglen, getTestSequence(diglen));

			foreach (int msglen in new[] { 0, 3, 128, 129, 255, 1024 })
			{
				var msg = getTestSequence(msglen);

#if ICRYPTOTRANSFORM
				inc.TransformBlock(halg.ComputeHash(msg), 0, diglen, null, 0);
				inc.TransformBlock(hmac.ComputeHash(msg), 0, diglen, null, 0);
#else
				inc.Update(halg.ComputeHash(msg));
				inc.Update(hmac.ComputeHash(msg));
#endif
			}
		}

#if ICRYPTOTRANSFORM
		inc.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		var hash = inc.Hash;
#else
		var hash = inc.Finish();
#endif

		return hash.SequenceEqual(blake2bCheck);
	}

	private static bool blake2sHmacSelfTest()
	{
#if ICRYPTOTRANSFORM
		using var inc = Blake2s.CreateHashAlgorithm(blake2bCheck.Length);
#else
		var inc = Blake2s.CreateIncrementalHasher(blake2bCheck.Length);
#endif

		foreach (int diglen in new[] { 16, 20, 28, 32 })
		{
			using var halg = Blake2s.CreateHashAlgorithm(diglen);
			using var hmac = Blake2s.CreateHMAC(diglen, getTestSequence(diglen));

			foreach (int msglen in new[] { 0, 3, 64, 65, 255, 1024 })
			{
				var msg = getTestSequence(msglen);

#if ICRYPTOTRANSFORM
				inc.TransformBlock(halg.ComputeHash(msg), 0, diglen, null, 0);
				inc.TransformBlock(hmac.ComputeHash(msg), 0, diglen, null, 0);
#else
				inc.Update(halg.ComputeHash(msg));
				inc.Update(hmac.ComputeHash(msg));
#endif
			}
		}

#if ICRYPTOTRANSFORM
		inc.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		var hash = inc.Hash;
#else
		var hash = inc.Finish();
#endif

		return hash.SequenceEqual(blake2sCheck);
	}

	[Fact]
	public void TestBlake2b()
	{
		Assert.True(blake2bSelfTest());
	}

	[Fact]
	public void TestBlake2s()
	{
		Assert.True(blake2sSelfTest());
	}

	[Fact]
	public void TestBlake2bNoAlloc()
	{
		Assert.True(blake2bNoAllocSelfTest());
	}

	[Fact]
	public void TestBlake2sNoAlloc()
	{
		Assert.True(blake2sNoAllocSelfTest());
	}

	[Fact]
	public void TestBlake2bHMAC()
	{
		Assert.True(blake2bHmacSelfTest());
	}

	[Fact]
	public void TestBlake2sHMAC()
	{
		Assert.True(blake2sHmacSelfTest());
	}
}
