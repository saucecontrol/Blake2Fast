using System.Runtime.CompilerServices;

unsafe internal partial struct Blake2bContext
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void gfunc(ref ulong va, ref ulong vb, ref ulong vc, ref ulong vd, ulong* m, uint x, uint y)
	{
		va += vb + m[x];
		vd ^= va;
		vd = ror64(vd, 32);
		vc += vd;
		vb ^= vc;
		vb = ror64(vb, 24);
		va += vb + m[y];
		vd ^= va;
		vd = ror64(vd, 16);
		vc += vd;
		vb ^= vc;
		vb = ror64(vb, 63);
	}

	unsafe private void mixPreferred(ulong* m)
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

		//ROUND 1
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 00, 01);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 02, 03);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 04, 05);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 06, 07);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 08, 09);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 10, 11);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 12, 13);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 14, 15);

		//ROUND 2
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 14, 10);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 04, 08);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 09, 15);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 13, 06);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 01, 12);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 00, 02);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 11, 07);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 05, 03);

		//ROUND 3
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 11, 08);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 12, 00);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 05, 02);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 15, 13);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 10, 14);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 03, 06);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 07, 01);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 09, 04);

		//ROUND 4
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 07, 09);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 03, 01);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 13, 12);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 11, 14);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 02, 06);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 05, 10);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 04, 00);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 15, 08);

		//ROUND 5
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 09, 00);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 05, 07);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 02, 04);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 10, 15);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 14, 01);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 11, 12);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 06, 08);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 03, 13);

		//ROUND 6
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 02, 12);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 06, 10);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 00, 11);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 08, 03);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 04, 13);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 07, 05);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 15, 14);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 01, 09);

		//ROUND 7
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 12, 05);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 01, 15);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 14, 13);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 04, 10);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 00, 07);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 06, 03);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 09, 02);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 08, 11);

		//ROUND 8
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 13, 11);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 07, 14);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 12, 01);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 03, 09);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 05, 00);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 15, 04);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 08, 06);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 02, 10);

		//ROUND 9
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 06, 15);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 14, 09);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 11, 03);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 00, 08);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 12, 02);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 13, 07);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 01, 04);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 10, 05);

		//ROUND 10
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 10, 02);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 08, 04);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 07, 06);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 01, 05);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 15, 11);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 09, 14);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 03, 12);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 13, 00);

		//ROUND 11
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 00, 01);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 02, 03);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 04, 05);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 06, 07);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 08, 09);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 10, 11);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 12, 13);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 14, 15);

		//ROUND 12
		gfunc(ref v00, ref v04, ref v08, ref v12, m, 14, 10);
		gfunc(ref v01, ref v05, ref v09, ref v13, m, 04, 08);
		gfunc(ref v02, ref v06, ref v10, ref v14, m, 09, 15);
		gfunc(ref v03, ref v07, ref v11, ref v15, m, 13, 06);
		gfunc(ref v00, ref v05, ref v10, ref v15, m, 01, 12);
		gfunc(ref v01, ref v06, ref v11, ref v12, m, 00, 02);
		gfunc(ref v02, ref v07, ref v08, ref v13, m, 11, 07);
		gfunc(ref v03, ref v04, ref v09, ref v14, m, 05, 03);

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
