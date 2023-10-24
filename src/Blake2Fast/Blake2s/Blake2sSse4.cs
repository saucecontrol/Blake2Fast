﻿// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

//------------------------------------------------------------------------------
//	<auto-generated>
//		This code was generated from a template.
//		Manual changes will be overwritten if the code is regenerated.
//	</auto-generated>
//------------------------------------------------------------------------------

#if HWINTRINSICS
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Blake2Fast.Implementation;

#if BLAKE2_PUBLIC
public
#else
internal
#endif
unsafe partial struct Blake2sHashState
{
	// SIMD algorithm described in https://eprint.iacr.org/2012/275.pdf
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	private static void mixSse41(uint* sh, uint* m)
	{
		ref byte rrm = ref MemoryMarshal.GetReference(rormask);
		var r16 = Unsafe.As<byte, Vector128<byte>>(ref rrm);
		var r8  = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref rrm, Vector128<byte>.Count));

		var row1 = Sse2.LoadVector128(sh);
		var row2 = Sse2.LoadVector128(sh + Vector128<uint>.Count);

		ref byte riv = ref MemoryMarshal.GetReference(ivle);
		var row3 = Unsafe.As<byte, Vector128<uint>>(ref riv);
		var row4 = Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref riv, 16));

		row4 = Sse2.Xor(row4, Sse2.LoadVector128(sh + Vector128<uint>.Count * 2)); // t[] and f[]

		var m0 = Sse2.LoadVector128(m);
		var m1 = Sse2.LoadVector128(m + Vector128<uint>.Count);
		var m2 = Sse2.LoadVector128(m + Vector128<uint>.Count * 2);
		var m3 = Sse2.LoadVector128(m + Vector128<uint>.Count * 3);

		//ROUND 1
		var b0 = Sse.Shuffle(m0.AsSingle(), m1.AsSingle(), 0b_10_00_10_00).AsUInt32();

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		b0 = Sse.Shuffle(m0.AsSingle(), m1.AsSingle(), 0b_11_01_11_01).AsUInt32();

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//DIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_10_01_00_11);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_00_11_10_01);

		var t0 = Sse2.Shuffle(m2, 0b_11_10_00_01);
		var t1 = Sse2.Shuffle(m3, 0b_00_01_11_10);
		b0 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_11_00_00_11).AsUInt32();

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_11_11_00).AsUInt32();
		b0 = Sse2.Shuffle(t0, 0b_10_11_00_01);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//UNDIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_00_11_10_01);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_10_01_00_11);

		//ROUND 2
		t0 = Sse41.Blend(m1.AsUInt16(), m2.AsUInt16(), 0b_00_00_11_00).AsUInt32();
		t1 = Sse2.ShiftLeftLogical128BitLane(m3, 4);
		var t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_11_11_00_00).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_10_01_00_11);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.Shuffle(m2, 0b_00_00_10_00);
		t1 = Sse41.Blend(m1.AsUInt16(), m3.AsUInt16(), 0b_11_00_00_00).AsUInt32();
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_11_11_00_00).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_10_11_00_01);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//DIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_10_01_00_11);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_00_11_10_01);

		t0 = Sse2.ShiftLeftLogical128BitLane(m1, 4);
		t1 = Sse41.Blend(m2.AsUInt16(), t0.AsUInt16(), 0b_00_11_00_00).AsUInt32();
		t2 = Sse41.Blend(m0.AsUInt16(), t1.AsUInt16(), 0b_11_11_00_00).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_11_00_01_10);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.UnpackHigh(m0, m1);
		t1 = Sse2.ShiftLeftLogical128BitLane(m3, 4);
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_00_11_00).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_11_00_01_10);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//UNDIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_00_11_10_01);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_10_01_00_11);

		//ROUND 3
		t0 = Sse2.UnpackHigh(m2, m3);
		t1 = Sse41.Blend(m3.AsUInt16(), m1.AsUInt16(), 0b_00_00_11_00).AsUInt32();
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_00_11_11).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_11_01_00_10);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.UnpackLow(m2, m0);
		t1 = Sse41.Blend(t0.AsUInt16(), m0.AsUInt16(), 0b_11_11_00_00).AsUInt32();
		t2 = Sse2.ShiftLeftLogical128BitLane(m3, 8);
		b0 = Sse41.Blend(t1.AsUInt16(), t2.AsUInt16(), 0b_11_00_00_00).AsUInt32();

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//DIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_10_01_00_11);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_00_11_10_01);

		t0 = Sse41.Blend(m0.AsUInt16(), m2.AsUInt16(), 0b_00_11_11_00).AsUInt32();
		t1 = Sse2.ShiftRightLogical128BitLane(m1, 12);
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_00_00_11).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_00_11_10_01);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.ShiftLeftLogical128BitLane(m3, 4);
		t1 = Sse41.Blend(m0.AsUInt16(), m1.AsUInt16(), 0b_00_11_00_11).AsUInt32();
		t2 = Sse41.Blend(t1.AsUInt16(), t0.AsUInt16(), 0b_11_00_00_00).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_01_10_11_00);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//UNDIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_00_11_10_01);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_10_01_00_11);

		//ROUND 4
		t0 = Sse2.UnpackHigh(m0, m1);
		t1 = Sse2.UnpackHigh(t0, m2);
		t2 = Sse41.Blend(t1.AsUInt16(), m3.AsUInt16(), 0b_00_00_11_00).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_11_01_00_10);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.ShiftLeftLogical128BitLane(m2, 8);
		t1 = Sse41.Blend(m3.AsUInt16(), m0.AsUInt16(), 0b_00_00_11_00).AsUInt32();
		t2 = Sse41.Blend(t1.AsUInt16(), t0.AsUInt16(), 0b_11_00_00_00).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_10_00_01_11);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//DIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_10_01_00_11);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_00_11_10_01);

		t0 = Sse41.Blend(m0.AsUInt16(), m1.AsUInt16(), 0b_00_00_11_11).AsUInt32();
		t1 = Sse41.Blend(t0.AsUInt16(), m3.AsUInt16(), 0b_11_00_00_00).AsUInt32();
		b0 = Sse2.Shuffle(t1, 0b_00_01_10_11);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Ssse3.AlignRight(m0, m1, 4);
		b0 = Sse41.Blend(t0.AsUInt16(), m2.AsUInt16(), 0b_00_11_00_11).AsUInt32();

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//UNDIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_00_11_10_01);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_10_01_00_11);

		//ROUND 5
		t0 = Sse2.UnpackLow(m1.AsUInt64(), m2.AsUInt64()).AsUInt32();
		t1 = Sse2.UnpackHigh(m0.AsUInt64(), m2.AsUInt64()).AsUInt32();
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_11_00_11).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_10_00_01_11);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.UnpackHigh(m1.AsUInt64(), m3.AsUInt64()).AsUInt32();
		t1 = Sse2.UnpackLow(m0.AsUInt64(), m1.AsUInt64()).AsUInt32();
		b0 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_11_00_11).AsUInt32();

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//DIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_10_01_00_11);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_00_11_10_01);

		t0 = Sse2.UnpackHigh(m3.AsUInt64(), m1.AsUInt64()).AsUInt32();
		t1 = Sse2.UnpackHigh(m2.AsUInt64(), m0.AsUInt64()).AsUInt32();
		t2 = Sse41.Blend(t1.AsUInt16(), t0.AsUInt16(), 0b_00_11_00_11).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_10_01_00_11);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse41.Blend(m0.AsUInt16(), m2.AsUInt16(), 0b_00_00_00_11).AsUInt32();
		t1 = Sse2.ShiftLeftLogical128BitLane(t0, 8);
		t2 = Sse41.Blend(t1.AsUInt16(), m3.AsUInt16(), 0b_00_00_11_11).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_10_00_11_01);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//UNDIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_00_11_10_01);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_10_01_00_11);

		//ROUND 6
		t0 = Sse2.UnpackHigh(m0, m1);
		t1 = Sse2.UnpackLow(m0, m2);
		b0 = Sse2.UnpackLow(t0.AsUInt64(), t1.AsUInt64()).AsUInt32();

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.ShiftRightLogical128BitLane(m2, 4);
		t1 = Sse41.Blend(m0.AsUInt16(), m3.AsUInt16(), 0b_00_00_00_11).AsUInt32();
		b0 = Sse41.Blend(t1.AsUInt16(), t0.AsUInt16(), 0b_00_11_11_00).AsUInt32();

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//DIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_10_01_00_11);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_00_11_10_01);

		t0 = Sse41.Blend(m1.AsUInt16(), m0.AsUInt16(), 0b_00_00_11_00).AsUInt32();
		t1 = Sse2.ShiftRightLogical128BitLane(m3, 4);
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_11_00_00).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_10_11_00_01);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.UnpackLow(m2.AsUInt64(), m1.AsUInt64()).AsUInt32();
		t1 = Sse2.Shuffle(m3, 0b_10_00_01_00);
		t2 = Sse2.ShiftRightLogical128BitLane(t0, 4);
		b0 = Sse41.Blend(t1.AsUInt16(), t2.AsUInt16(), 0b_00_11_00_11).AsUInt32();

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//UNDIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_00_11_10_01);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_10_01_00_11);

		//ROUND 7
		t0 = Sse2.ShiftLeftLogical128BitLane(m1, 12);
		t1 = Sse41.Blend(m0.AsUInt16(), m3.AsUInt16(), 0b_00_11_00_11).AsUInt32();
		b0 = Sse41.Blend(t1.AsUInt16(), t0.AsUInt16(), 0b_11_00_00_00).AsUInt32();

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse41.Blend(m3.AsUInt16(), m2.AsUInt16(), 0b_00_11_00_00).AsUInt32();
		t1 = Sse2.ShiftRightLogical128BitLane(m1, 4);
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_00_00_11).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_10_01_11_00);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//DIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_10_01_00_11);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_00_11_10_01);

		t0 = Sse2.UnpackLow(m0.AsUInt64(), m2.AsUInt64()).AsUInt32();
		t1 = Sse2.ShiftRightLogical128BitLane(m1, 4);
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_00_11_00).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_11_01_00_10);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.UnpackHigh(m1, m2);
		t1 = Sse2.UnpackHigh(m0.AsUInt64(), t0.AsUInt64()).AsUInt32();
		b0 = Sse2.Shuffle(t1, 0b_00_01_10_11);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//UNDIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_00_11_10_01);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_10_01_00_11);

		//ROUND 8
		t0 = Sse2.UnpackHigh(m0, m1);
		t1 = Sse41.Blend(t0.AsUInt16(), m3.AsUInt16(), 0b_00_00_11_11).AsUInt32();
		b0 = Sse2.Shuffle(t1, 0b_10_00_11_01);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse41.Blend(m2.AsUInt16(), m3.AsUInt16(), 0b_00_11_00_00).AsUInt32();
		t1 = Sse2.ShiftRightLogical128BitLane(m0, 4);
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_00_00_11).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_01_00_10_11);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//DIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_10_01_00_11);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_00_11_10_01);

		t0 = Sse2.UnpackHigh(m0.AsUInt64(), m3.AsUInt64()).AsUInt32();
		t1 = Sse2.UnpackLow(m1.AsUInt64(), m2.AsUInt64()).AsUInt32();
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_11_11_00).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_10_11_01_00);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.UnpackLow(m0, m1);
		t1 = Sse2.UnpackHigh(m1, m2);
		t2 = Sse2.UnpackLow(t0.AsUInt64(), t1.AsUInt64()).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_10_01_00_11);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//UNDIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_00_11_10_01);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_10_01_00_11);

		//ROUND 9
		t0 = Sse2.UnpackHigh(m1, m3);
		t1 = Sse2.UnpackLow(t0.AsUInt64(), m0.AsUInt64()).AsUInt32();
		t2 = Sse41.Blend(t1.AsUInt16(), m2.AsUInt16(), 0b_11_00_00_00).AsUInt32();
		b0 = Sse2.ShuffleHigh(t2.AsUInt16(), 0b_01_00_11_10).AsUInt32();

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.UnpackHigh(m0, m3);
		t1 = Sse41.Blend(m2.AsUInt16(), t0.AsUInt16(), 0b_11_11_00_00).AsUInt32();
		b0 = Sse2.Shuffle(t1, 0b_00_10_01_11);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//DIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_10_01_00_11);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_00_11_10_01);

		t0 = Sse2.UnpackLow(m0.AsUInt64(), m3.AsUInt64()).AsUInt32();
		t1 = Sse2.ShiftRightLogical128BitLane(m2, 8);
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_00_00_11).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_01_11_10_00);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse41.Blend(m1.AsUInt16(), m0.AsUInt16(), 0b_00_11_00_00).AsUInt32();
		b0 = Sse2.Shuffle(t0, 0b_00_11_10_01);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//UNDIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_00_11_10_01);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_10_01_00_11);

		//ROUND 10
		t0 = Sse41.Blend(m0.AsUInt16(), m2.AsUInt16(), 0b_00_00_00_11).AsUInt32();
		t1 = Sse41.Blend(m1.AsUInt16(), m2.AsUInt16(), 0b_00_11_00_00).AsUInt32();
		t2 = Sse41.Blend(t1.AsUInt16(), t0.AsUInt16(), 0b_00_00_11_11).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_01_11_00_10);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse2.ShiftLeftLogical128BitLane(m0, 4);
		t1 = Sse41.Blend(m1.AsUInt16(), t0.AsUInt16(), 0b_11_00_00_00).AsUInt32();
		b0 = Sse2.Shuffle(t1, 0b_01_10_00_11);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//DIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_10_01_00_11);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_00_11_10_01);

		t0 = Sse2.UnpackHigh(m0, m3);
		t1 = Sse2.UnpackLow(m2, m3);
		t2 = Sse2.UnpackHigh(t0.AsUInt64(), t1.AsUInt64()).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_00_10_01_11);

		//G1
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r16).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 12), Sse2.ShiftLeftLogical(row2, 32 - 12));

		t0 = Sse41.Blend(m3.AsUInt16(), m2.AsUInt16(), 0b_11_00_00_00).AsUInt32();
		t1 = Sse2.UnpackLow(m0, m3);
		t2 = Sse41.Blend(t0.AsUInt16(), t1.AsUInt16(), 0b_00_00_11_11).AsUInt32();
		b0 = Sse2.Shuffle(t2, 0b_01_10_11_00);

		//G2
		row1 = Sse2.Add(Sse2.Add(row1, b0), row2);
		row4 = Sse2.Xor(row4, row1);
		row4 = Ssse3.Shuffle(row4.AsByte(), r8).AsUInt32();

		row3 = Sse2.Add(row3, row4);
		row2 = Sse2.Xor(row2, row3);
		row2 = Sse2.Xor(Sse2.ShiftRightLogical(row2, 7), Sse2.ShiftLeftLogical(row2, 32 - 7));

		//UNDIAGONALIZE
		row1 = Sse2.Shuffle(row1, 0b_00_11_10_01);
		row4 = Sse2.Shuffle(row4, 0b_01_00_11_10);
		row3 = Sse2.Shuffle(row3, 0b_10_01_00_11);

		row1 = Sse2.Xor(row1, row3);
		row2 = Sse2.Xor(row2, row4);
		row1 = Sse2.Xor(row1, Sse2.LoadVector128(sh));
		row2 = Sse2.Xor(row2, Sse2.LoadVector128(sh + Vector128<uint>.Count));
		Sse2.Store(sh, row1);
		Sse2.Store(sh + Vector128<uint>.Count, row2);
	}
}
#endif
