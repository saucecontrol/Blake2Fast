unsafe internal partial struct Blake2bContext
{
	private static void mixSimplified(Blake2bContext* s, ulong* m)
	{
		ulong m00 = m[00];
		ulong m01 = m[01];
		ulong m02 = m[02];
		ulong m03 = m[03];
		ulong m04 = m[04];
		ulong m05 = m[05];
		ulong m06 = m[06];
		ulong m07 = m[07];
		ulong m08 = m[08];
		ulong m09 = m[09];
		ulong m10 = m[10];
		ulong m11 = m[11];
		ulong m12 = m[12];
		ulong m13 = m[13];
		ulong m14 = m[14];
		ulong m15 = m[15];

		ulong v00 = s->h[0];
		ulong v01 = s->h[1];
		ulong v02 = s->h[2];
		ulong v03 = s->h[3];
		ulong v04 = s->h[4];
		ulong v05 = s->h[5];
		ulong v06 = s->h[6];
		ulong v07 = s->h[7];

		ulong v08 = 0x6A09E667F3BCC908ul;
		ulong v09 = 0xBB67AE8584CAA73Bul;
		ulong v10 = 0x3C6EF372FE94F82Bul;
		ulong v11 = 0xA54FF53A5F1D36F1ul;
		ulong v12 = 0x510E527FADE682D1ul;
		ulong v13 = 0x9B05688C2B3E6C1Ful;
		ulong v14 = 0x1F83D9ABFB41BD6Bul;
		ulong v15 = 0x5BE0CD19137E2179ul;

		v12 ^= s->t[0];
		v13 ^= s->t[1];
		v14 ^= s->f[0];

		//ROUND 1 (first half)
		v00 += m00;
		v00 += v04;
		v12 ^= v00;
		v08 += v12;
		v04 ^= v08;

		v01 += m02;
		v01 += v05;
		v13 ^= v01;
		v09 += v13;
		v05 ^= v09;

		v02 += m04;
		v02 += v06;
		v14 ^= v02;
		v10 += v14;
		v06 ^= v10;

		v03 += m06;
		v03 += v07;
		v15 ^= v03;
		v11 += v15;
		v07 ^= v11;

		v00 += m08;
		v00 += v05;
		v15 ^= v00;
		v10 += v15;
		v05 ^= v10;

		v01 += m10;
		v01 += v06;
		v12 ^= v01;
		v11 += v12;
		v06 ^= v11;

		v02 += m12;
		v02 += v07;
		v13 ^= v02;
		v08 += v13;
		v07 ^= v08;

		v03 += m14;
		v03 += v04;
		v14 ^= v03;
		v09 += v14;
		v04 ^= v09;

		s->h[0] ^= v00 ^ v08;
		s->h[1] ^= v01 ^ v09;
		s->h[2] ^= v02 ^ v10;
		s->h[3] ^= v03 ^ v11;
		s->h[4] ^= v04 ^ v12;
		s->h[5] ^= v05 ^ v13;
		s->h[6] ^= v06 ^ v14;
		s->h[7] ^= v07 ^ v15;
	}
}
