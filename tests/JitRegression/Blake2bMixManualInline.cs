﻿unsafe internal partial struct Blake2bContext
{
	private void mixManualInline(ulong* m)
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
		v00 += v04 + m[0];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[1];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[2];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[3];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[4];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[5];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[6];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[7];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[8];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[9];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[10];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[11];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[12];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[13];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[14];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[15];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 2
		v00 += v04 + m[14];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[10];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[4];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[8];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[9];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[15];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[13];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[6];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[1];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[12];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[0];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[2];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[11];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[7];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[5];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[3];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 3
		v00 += v04 + m[11];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[8];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[12];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[0];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[5];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[2];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[15];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[13];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[10];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[14];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[3];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[6];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[7];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[1];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[9];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[4];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 4
		v00 += v04 + m[7];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[9];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[3];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[1];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[13];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[12];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[11];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[14];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[2];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[6];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[5];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[10];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[4];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[0];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[15];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[8];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 5
		v00 += v04 + m[9];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[0];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[5];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[7];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[2];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[4];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[10];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[15];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[14];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[1];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[11];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[12];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[6];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[8];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[3];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[13];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 6
		v00 += v04 + m[2];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[12];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[6];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[10];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[0];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[11];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[8];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[3];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[4];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[13];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[7];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[5];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[15];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[14];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[1];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[9];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 7
		v00 += v04 + m[12];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[5];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[1];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[15];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[14];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[13];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[4];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[10];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[0];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[7];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[6];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[3];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[9];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[2];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[8];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[11];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 8
		v00 += v04 + m[13];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[11];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[7];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[14];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[12];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[1];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[3];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[9];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[5];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[0];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[15];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[4];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[8];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[6];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[2];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[10];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 9
		v00 += v04 + m[6];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[15];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[14];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[9];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[11];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[3];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[0];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[8];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[12];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[2];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[13];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[7];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[1];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[4];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[10];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[5];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 10
		v00 += v04 + m[10];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[2];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[8];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[4];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[7];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[6];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[1];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[5];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[15];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[11];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[9];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[14];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[3];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[12];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[13];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[0];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 11
		v00 += v04 + m[0];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[1];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[2];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[3];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[4];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[5];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[6];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[7];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[8];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[9];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[10];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[11];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[12];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[13];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[14];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[15];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

		//ROUND 12
		v00 += v04 + m[14];
		v12 ^= v00;
		v12 = ror64(v12, 32);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 24);
		v00 += v04 + m[10];
		v12 ^= v00;
		v12 = ror64(v12, 16);
		v08 += v12;
		v04 ^= v08;
		v04 = ror64(v04, 63);

		v01 += v05 + m[4];
		v13 ^= v01;
		v13 = ror64(v13, 32);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 24);
		v01 += v05 + m[8];
		v13 ^= v01;
		v13 = ror64(v13, 16);
		v09 += v13;
		v05 ^= v09;
		v05 = ror64(v05, 63);

		v02 += v06 + m[9];
		v14 ^= v02;
		v14 = ror64(v14, 32);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 24);
		v02 += v06 + m[15];
		v14 ^= v02;
		v14 = ror64(v14, 16);
		v10 += v14;
		v06 ^= v10;
		v06 = ror64(v06, 63);

		v03 += v07 + m[13];
		v15 ^= v03;
		v15 = ror64(v15, 32);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 24);
		v03 += v07 + m[6];
		v15 ^= v03;
		v15 = ror64(v15, 16);
		v11 += v15;
		v07 ^= v11;
		v07 = ror64(v07, 63);

		v00 += v05 + m[1];
		v15 ^= v00;
		v15 = ror64(v15, 32);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 24);
		v00 += v05 + m[12];
		v15 ^= v00;
		v15 = ror64(v15, 16);
		v10 += v15;
		v05 ^= v10;
		v05 = ror64(v05, 63);

		v01 += v06 + m[0];
		v12 ^= v01;
		v12 = ror64(v12, 32);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 24);
		v01 += v06 + m[2];
		v12 ^= v01;
		v12 = ror64(v12, 16);
		v11 += v12;
		v06 ^= v11;
		v06 = ror64(v06, 63);

		v02 += v07 + m[11];
		v13 ^= v02;
		v13 = ror64(v13, 32);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 24);
		v02 += v07 + m[7];
		v13 ^= v02;
		v13 = ror64(v13, 16);
		v08 += v13;
		v07 ^= v08;
		v07 = ror64(v07, 63);

		v03 += v04 + m[5];
		v14 ^= v03;
		v14 = ror64(v14, 32);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 24);
		v03 += v04 + m[3];
		v14 ^= v03;
		v14 = ror64(v14, 16);
		v09 += v14;
		v04 ^= v09;
		v04 = ror64(v04, 63);

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
