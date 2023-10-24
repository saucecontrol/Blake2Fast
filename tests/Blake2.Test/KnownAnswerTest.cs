// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System;
using System.Linq;
using System.Json;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Xunit;
using Blake2Fast;

namespace Blake2Test;

#pragma warning disable CS0169, CS0649, CA1815, IDE0051
public struct KatEntry(JsonObject o)
{
	public string Alg = o["hash"];
	public byte[] Key = getBytes(o["key"]);
	public byte[] Data = getBytes(o["in"]);
	public byte[] Digest = getBytes(o["out"]);

	private static byte[] getBytes(string s)
	{
		if (string.IsNullOrEmpty(s))
			return [ ];

		var bytes = new byte[s.Length / 2];
		for (int i = 0; i < bytes.Length; i++)
			bytes[i] = (byte)((getDigit(s[i * 2]) << 4) | getDigit(s[i * 2 + 1]));

		return bytes;
	}

	private static int getDigit(char c) => c < 'a' ? c - '0' : c - 'a' + 10;

	private static KatEntry[] getKatValues()
	{
		using var stm = typeof(KatEntry).Assembly.GetManifestResourceStream("Blake2.Test.blake2-kat.json");
		return ((JsonArray)JsonValue.Load(stm)).Cast<JsonObject>().Select(o => new KatEntry(o)).ToArray();
	}

	public static readonly KatEntry[] Values = getKatValues();

	public static IEnumerable<object[]> Blake2b => Values.Where(k => k.Alg == "blake2b" && k.Key.Length == 0).Select(k => new object[] { k });

	public static IEnumerable<object[]> Blake2bKeyed => Values.Where(k => k.Alg == "blake2b" && k.Key.Length != 0).Select(k => new object[] { k });

	public static IEnumerable<object[]> Blake2s => Values.Where(k => k.Alg == "blake2s" && k.Key.Length == 0).Select(k => new object[] { k });

	public static IEnumerable<object[]> Blake2sKeyed => Values.Where(k => k.Alg == "blake2s" && k.Key.Length != 0).Select(k => new object[] { k });
}

public class KnownAnswerTest
{
	// DateTime is Auto layout, so this struct size differs between runtimes (net7+ is 12 bytes, older is 16 bytes)
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct TestStruct
	{
		public static readonly KnownAnswerTest Test = new();

		private readonly DateTime when;
		public int HowMany;
	}

	private static unsafe bool tryReadAndAdvance<T>(out T val, ref ReadOnlySpan<byte> span) where T : unmanaged
	{
		bool res = MemoryMarshal.TryRead(span, out val);
		if (res)
			span = span[sizeof(T)..];

		return res;
	}

	private static byte[] compute(IBlake2Incremental impl, ReadOnlySpan<byte> data)
	{
		// every read and write after this will be unaligned
		if (tryReadAndAdvance(out byte byteVal, ref data))
			impl.Update(byteVal);

		// an enum (int) value
		if (tryReadAndAdvance(out DateTimeKind enumVal, ref data))
			impl.Update(enumVal);

		// advance to just shy of a block boundary to make sure we split a value across blocks sometimes
		if (data.Length >= sizeof(long) * 15)
		{
			impl.Update(MemoryMarshal.Cast<byte, long>(data[..(sizeof(long) * 15)]));
			data = data[(sizeof(long) * 15)..];
		}

		// a non-blittable value type
		if (tryReadAndAdvance(out Guid guidVal, ref data))
			impl.Update(guidVal);

		// a custom struct with a static ref field
		if (tryReadAndAdvance(out TestStruct testVal, ref data))
			impl.Update(testVal);

		impl.Update(data);

		return impl.Finish();
	}

	[Theory]
	[MemberData(nameof(KatEntry.Blake2b), MemberType = typeof(KatEntry))]
	public void KatBlake2b(KatEntry ka)
	{
		var hash = compute(Blake2b.CreateIncrementalHasher(), ka.Data);
		Assert.Equal(ka.Digest, hash);
	}

	[Theory]
	[MemberData(nameof(KatEntry.Blake2s), MemberType = typeof(KatEntry))]
	public void KatBlake2s(KatEntry ka)
	{
		var hash = compute(Blake2s.CreateIncrementalHasher(), ka.Data);
		Assert.Equal(ka.Digest, hash);
	}

	[Theory]
	[MemberData(nameof(KatEntry.Blake2bKeyed), MemberType = typeof(KatEntry))]
	public void KatBlake2bKeyed(KatEntry ka)
	{
		var hash = compute(Blake2b.CreateIncrementalHasher(ka.Key), ka.Data);
		Assert.Equal(ka.Digest, hash);
	}

	[Theory]
	[MemberData(nameof(KatEntry.Blake2sKeyed), MemberType = typeof(KatEntry))]
	public void KatBlake2sKeyed(KatEntry ka)
	{
		var hash = compute(Blake2s.CreateIncrementalHasher(ka.Key), ka.Data);
		Assert.Equal(ka.Digest, hash);
	}

	[Fact]
	public void UpdateThrowsOnRefContainingT()
	{
		Assert.Throws<NotSupportedException>(() => Blake2b.CreateIncrementalHasher().Update(KatEntry.Values.First()));
	}

	[Fact]
	public void UpdateThrowsOnRefContainingSpanT()
	{
		Assert.Throws<NotSupportedException>(() => Blake2b.CreateIncrementalHasher().Update(KatEntry.Values));
	}
}
