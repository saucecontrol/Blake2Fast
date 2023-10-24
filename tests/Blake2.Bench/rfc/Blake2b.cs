// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;

namespace Blake2Rfc;

static class Blake2b
{
    unsafe public struct Blake2Context {
        public fixed byte b[128];                // input buffer
        public fixed ulong h[8];                 // chained state
        public fixed ulong t[2];                 // total number of bytes
        public uint c;                           // pointer for b[]
        public uint outlen;                      // digest size
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ulong ror64(ulong x, int y) => (x >> y) ^ (x << (64 - y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe static void mix(ref ulong a, ref ulong b, ref ulong c, ref ulong d, ulong x, ulong y)
    {
        a = a + b + x;
        d = ror64(d ^ a, 32);
        c = c + d;
        b = ror64(b ^ c, 24);
        a = a + b + y;
        d = ror64(d ^ a, 16);
        c = c + d;
        b = ror64(b ^ c, 63);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe static ulong getUInt64(byte* p) =>
        ((ulong)p[0]         ) ^
        ((ulong)p[1] << 1 * 8) ^
        ((ulong)p[2] << 2 * 8) ^
        ((ulong)p[3] << 3 * 8) ^
        ((ulong)p[4] << 4 * 8) ^
        ((ulong)p[5] << 5 * 8) ^
        ((ulong)p[6] << 6 * 8) ^
        ((ulong)p[7] << 7 * 8);

    static readonly ulong[] iv = [
        0x6A09E667F3BCC908, 0xBB67AE8584CAA73B,
        0x3C6EF372FE94F82B, 0xA54FF53A5F1D36F1,
        0x510E527FADE682D1, 0x9B05688C2B3E6C1F,
        0x1F83D9ABFB41BD6B, 0x5BE0CD19137E2179
    ];

    static readonly byte[][] sigma = [
        [ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 ],
        [ 14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3 ],
        [ 11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4 ],
        [ 7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8 ],
        [ 9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13 ],
        [ 2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9 ],
        [ 12, 5, 1, 15, 14, 13, 4, 10, 0, 7, 6, 3, 9, 2, 8, 11 ],
        [ 13, 11, 7, 14, 12, 1, 3, 9, 5, 0, 15, 4, 8, 6, 2, 10 ],
        [ 6, 15, 14, 9, 11, 3, 0, 8, 12, 2, 13, 7, 1, 4, 10, 5 ],
        [ 10, 2, 8, 4, 7, 6, 1, 5, 15, 11, 9, 14, 3, 12, 13, 0 ],
        [ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 ],
        [ 14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3 ]
    ];

    // Compression function. "last" flag indicates last block.
    unsafe static void compress(ref Blake2Context ctx, bool last)
    {
        ulong* v = stackalloc ulong[16];
        ulong* m = stackalloc ulong[16];

        for (uint i = 0; i < 8; i++) {           // init work variables
            v[i] = ctx.h[i];
            v[i + 8] = iv[i];
        }

        v[12] ^= ctx.t[0];                       // low 64 bits of offset
        v[13] ^= ctx.t[1];                       // high 64 bits
        if (last)                                // last block flag set ?
            v[14] = ~v[14];

        fixed (byte* pb = &ctx.b[0])
        for (uint i = 0; i < 16; i++)            // get little-endian words
            m[i] = getUInt64(&pb[sizeof(ulong) * i]);

        for (uint i = 0; i < 12; i++) {          // twelve rounds
            mix(ref v[0], ref v[4], ref v[ 8], ref v[12], m[sigma[i][ 0]], m[sigma[i][ 1]]);
            mix(ref v[1], ref v[5], ref v[ 9], ref v[13], m[sigma[i][ 2]], m[sigma[i][ 3]]);
            mix(ref v[2], ref v[6], ref v[10], ref v[14], m[sigma[i][ 4]], m[sigma[i][ 5]]);
            mix(ref v[3], ref v[7], ref v[11], ref v[15], m[sigma[i][ 6]], m[sigma[i][ 7]]);
            mix(ref v[0], ref v[5], ref v[10], ref v[15], m[sigma[i][ 8]], m[sigma[i][ 9]]);
            mix(ref v[1], ref v[6], ref v[11], ref v[12], m[sigma[i][10]], m[sigma[i][11]]);
            mix(ref v[2], ref v[7], ref v[ 8], ref v[13], m[sigma[i][12]], m[sigma[i][13]]);
            mix(ref v[3], ref v[4], ref v[ 9], ref v[14], m[sigma[i][14]], m[sigma[i][15]]);
        }

        for(uint i = 0; i < 8; i++)
            ctx.h[i] ^= v[i] ^ v[i + 8];
    }

    // Initialize the hashing context "ctx" with optional key "key".
    //      1 <= outlen <= 64 gives the digest size in bytes.
    //      Secret key (also <= 64 bytes) is optional (keylen = 0).
    unsafe public static void init(ref Blake2Context ctx, uint outlen,
        void* key, uint keylen)                  // (keylen=0: no key)
    {
        if (outlen == 0 || outlen > 64 || keylen > 64)
            throw new ArgumentException();

        for (uint i = 0; i < 8; i++)             // state, "param block"
            ctx.h[i] = iv[i];
        ctx.h[0] ^= 0x01010000 ^ (keylen << 8) ^ outlen;

        ctx.t[0] = 0;                            // input count low word
        ctx.t[1] = 0;                            // input count high word
        ctx.c = 0;                               // pointer within buffer
        ctx.outlen = outlen;

        for (uint i = keylen; i < 128; i++)      // zero input block
            ctx.b[i] = 0;
        if (keylen > 0) {
            update(ref ctx, key, keylen);
            ctx.c = 128;                         // at the end
        }
    }

    // Add "inlen" bytes from "in" into the hash.
    unsafe public static void update(ref Blake2Context ctx,
        void* data, uint inlen)                  // data bytes
    {
        for (uint i = 0; i < inlen; i++) {
            if (ctx.c == 128) {                  // buffer full ?
                ctx.t[0] += ctx.c;               // add counters
                if (ctx.t[0] < ctx.c)            // carry overflow ?
                    ctx.t[1]++;                  // high word
                compress(ref ctx, false);        // compress (not last)
                ctx.c = 0;                       // counter to zero
            }
            ctx.b[ctx.c++] = ((byte*)data)[i];
        }
    }

    // Generate the message digest (size given in init).
    //      Result placed in "out".
    unsafe public static void final(ref Blake2Context ctx, void* hash)
    {
        ctx.t[0] += ctx.c;                       // mark last block offset
        if (ctx.t[0] < ctx.c)                    // carry overflow
            ctx.t[1]++;                          // high word

        while (ctx.c < 128)                      // fill up with zeros
            ctx.b[ctx.c++] = 0;
        compress(ref ctx, true);                 // final block flag = true

        // little endian convert and store
        for (uint i = 0; i < ctx.outlen; i++) {
            ((byte*)hash)[i] = (byte)((ctx.h[i >> 3] >> (int)(8 * (i & 7))) & 0xFF);
        }
    }

    // Convenience function for all-in-one computation.
    unsafe public static byte[] ComputeHash(int outlen, byte[] key, byte[] data)
    {
        var ctx = default(Blake2Context);
        key ??= [ ];
        data ??= [ ];

        fixed (byte* pkey = key)
            init(ref ctx, (uint)outlen, pkey, (uint)key.Length);

        fixed (byte* pdata = data)
            update(ref ctx, pdata, (uint)data.Length);

        var hash = new byte[outlen];
        fixed (byte* phash = hash)
            final(ref ctx, phash);

        return hash;
    }
}
