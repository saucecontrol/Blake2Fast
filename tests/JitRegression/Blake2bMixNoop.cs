using System.Runtime.CompilerServices;

unsafe internal partial struct Blake2bContext
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	unsafe private void mixNoop(ulong* m)
	{
		ulong v00 = h[0];
		ulong v01 = h[1];
		ulong v02 = h[2];
		ulong v03 = h[3];
		ulong v04 = h[4];
		ulong v05 = h[5];
		ulong v06 = h[6];
		ulong v07 = h[7];

		ulong v08 = 0x6A09E667F3BCC908ul;
		ulong v09 = 0xBB67AE8584CAA73Bul;
		ulong v10 = 0x3C6EF372FE94F82Bul;
		ulong v11 = 0xA54FF53A5F1D36F1ul;
		ulong v12 = 0x510E527FADE682D1ul;
		ulong v13 = 0x9B05688C2B3E6C1Ful;
		ulong v14 = 0x1F83D9ABFB41BD6Bul;
		ulong v15 = 0x5BE0CD19137E2179ul;

		v12 ^= t[0];
		v13 ^= t[1];
		v14 ^= f[0];

		//Skip mixing

		h[0] ^= v00 ^ v08;
		h[1] ^= v01 ^ v09;
		h[2] ^= v02 ^ v10;
		h[3] ^= v03 ^ v11;
		h[4] ^= v04 ^ v12;
		h[5] ^= v05 ^ v13;
		h[6] ^= v06 ^ v14;
		h[7] ^= v07 ^ v15;
	}
}
