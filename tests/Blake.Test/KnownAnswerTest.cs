// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System;
using System.Linq;
using System.Json;
using System.Runtime.InteropServices;

using Xunit;
using Blake2Fast;

namespace BlakeTest;

public struct KatEntry2(JsonObject o)
{
	public string Alg = o["hash"];
	public byte[] Key = Util.ConvertHexToBytes(o["key"]);
	public byte[] Data = Util.ConvertHexToBytes(o["in"]);
	public byte[] Digest = Util.ConvertHexToBytes(o["out"]);

	public static readonly KatEntry2[] Values = getKatValues();

	public static TheoryData<KatEntry2> Blake2b => new(Values.Where(k => k.Alg == "blake2b" && k.Key.Length == 0));

	public static TheoryData<KatEntry2> Blake2bKeyed => new(Values.Where(k => k.Alg == "blake2b" && k.Key.Length != 0));

	public static TheoryData<KatEntry2> Blake2s => new(Values.Where(k => k.Alg == "blake2s" && k.Key.Length == 0));

	public static TheoryData<KatEntry2> Blake2sKeyed => new(Values.Where(k => k.Alg == "blake2s" && k.Key.Length != 0));

	private static KatEntry2[] getKatValues()
	{
		using var stm = typeof(KatEntry2).Assembly.GetManifestResourceStream("Blake.Test.blake2-kat.json");
		return ((JsonArray)JsonValue.Load(stm)).Cast<JsonObject>().Select(o => new KatEntry2(o)).ToArray();
	}
}

public struct KatEntry3(JsonObject o)
{
	public int InputLen = o["input_len"];
	public byte[] Hash = Util.ConvertHexToBytes(o["hash"]);
	public byte[] KeyedHash = Util.ConvertHexToBytes(o["keyed_hash"]);
	public byte[] DeriveKey = Util.ConvertHexToBytes(o["derive_key"]);

	private static readonly byte[] data = getData();

	private static readonly KatEntry3[] values = getKatValues();

	public static ReadOnlySpan<byte> Key => "whats the Elvish word for friend"u8;

	public static ReadOnlySpan<byte> GetData(int len) => data.AsSpan(0, len);

	public static TheoryData<KatEntry3> Values => new(values);

	private static byte[] getData()
	{
		byte[] data = new byte[102400];
		for (int i = 0; i < data.Length; i++)
			data[i] = (byte)(i % 251);

		return data;
	}

	private static KatEntry3[] getKatValues()
	{
		using var stm = typeof(KatEntry3).Assembly.GetManifestResourceStream("Blake.Test.blake3-kat.json");
		return ((JsonArray)JsonValue.Load(stm)["cases"]).Cast<JsonObject>().Select(o => new KatEntry3(o)).ToArray();
	}
}

public class KnownAnswerTest
{
	// DateTime is Auto layout, so this struct size differs between runtimes with default padding (net7+ is 12 bytes, older is 16 bytes)
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct TestStruct
	{
		public static readonly KnownAnswerTest Test = new();

		private readonly DateTime when;
		public int HowMany;
	}

	private static byte[] compute(IBlakeIncremental impl, ReadOnlySpan<byte> data)
	{
		static unsafe bool tryReadAndAdvance<T>(ref ReadOnlySpan<byte> span, out T val) where T : unmanaged
		{
			bool res = MemoryMarshal.TryRead(span, out val);
			if (res)
				span = span[sizeof(T)..];

			return res;
		}

		// every read and write after this will be unaligned
		if (tryReadAndAdvance(ref data, out byte byteVal))
			impl.Update(byteVal);

		// an enum (int) value
		if (tryReadAndAdvance(ref data, out DateTimeKind enumVal))
			impl.Update(enumVal);

		// advance to just shy of a block boundary to make sure we split a value across blocks sometimes
		if (data.Length >= sizeof(long) * 15)
		{
			impl.Update(MemoryMarshal.Cast<byte, long>(data[..(sizeof(long) * 15)]));
			data = data[(sizeof(long) * 15)..];
		}

		// a non-blittable value type
		if (tryReadAndAdvance(ref data, out Guid guidVal))
			impl.Update(guidVal);

		// a custom struct with a static ref field
		if (tryReadAndAdvance(ref data, out TestStruct testVal))
			impl.Update(testVal);

		impl.Update(data);

		return impl.Finish();
	}

	private static byte[] compute3(int inlen, int outlen = Blake3.DefaultDigestLength, bool keyed = false)
	{
		byte[] hash = new byte[outlen];
		var impl = Blake3.CreateIncrementalHasher(keyed ? KatEntry3.Key : default);
		impl.Update(KatEntry3.GetData(inlen));
		impl.Finish(hash);

		return hash;
	}

	[Theory]
	[MemberData(nameof(KatEntry2.Blake2b), MemberType = typeof(KatEntry2))]
	public void KatBlake2b(KatEntry2 ka)
	{
		var hash = compute(Blake2b.CreateIncrementalHasher(), ka.Data);
		Assert.Equal(ka.Digest, hash);
	}

	[Theory]
	[MemberData(nameof(KatEntry2.Blake2s), MemberType = typeof(KatEntry2))]
	public void KatBlake2s(KatEntry2 ka)
	{
		var hash = compute(Blake2s.CreateIncrementalHasher(), ka.Data);
		Assert.Equal(ka.Digest, hash);
	}

	[Theory]
	[MemberData(nameof(KatEntry2.Blake2bKeyed), MemberType = typeof(KatEntry2))]
	public void KatBlake2bKeyed(KatEntry2 ka)
	{
		var hash = compute(Blake2b.CreateIncrementalHasher(ka.Key), ka.Data);
		Assert.Equal(ka.Digest, hash);
	}

	[Theory]
	[MemberData(nameof(KatEntry2.Blake2sKeyed), MemberType = typeof(KatEntry2))]
	public void KatBlake2sKeyed(KatEntry2 ka)
	{
		var hash = compute(Blake2s.CreateIncrementalHasher(ka.Key), ka.Data);
		Assert.Equal(ka.Digest, hash);
	}

	[Theory]
	[MemberData(nameof(KatEntry3.Values), MemberType = typeof(KatEntry3))]
	public void KatBlake3(KatEntry3 ka)
	{
		var hash = compute(Blake3.CreateIncrementalHasher(), KatEntry3.GetData(ka.InputLen));
		Assert.Equal(ka.Hash.AsSpan(0, Blake3.DefaultDigestLength).ToArray(), hash);
	}

	[Theory]
	[MemberData(nameof(KatEntry3.Values), MemberType = typeof(KatEntry3))]
	public void KatBlake3Keyed(KatEntry3 ka)
	{
		var hash = compute3(ka.InputLen, ka.KeyedHash.Length, true);
		Assert.Equal(ka.KeyedHash, hash);
	}

	[Fact]
	public void UpdateThrowsOnRefContainingT()
	{
		Assert.Throws<NotSupportedException>(() => Blake2b.CreateIncrementalHasher().Update(KatEntry2.Values.First()));
	}

	[Fact]
	public void UpdateThrowsOnRefContainingSpanT()
	{
		Assert.Throws<NotSupportedException>(() => Blake2b.CreateIncrementalHasher().Update(KatEntry2.Values));
	}
}
