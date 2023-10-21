// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System.Linq;

namespace Blake2Rfc;

static class SelftTest
{
    public static void selftest_seq(byte[] buff)
    {
        uint a = 0xDEAD4BADu * (uint)buff.Length;             // prime
        uint b = 1;

        for (int i = 0; i < buff.Length; i++)                 // fill the buf
        {
            uint t = a + b;
            a = b;
            b = t;
            buff[i] = (byte)(t >> 24);
        }
    }

    public static readonly byte[] blake2b_res = new byte[] {
        0xC2, 0x3A, 0x78, 0x00, 0xD9, 0x81, 0x23, 0xBD,
        0x10, 0xF5, 0x06, 0xC6, 0x1E, 0x29, 0xDA, 0x56,
        0x03, 0xD7, 0x63, 0xB8, 0xBB, 0xAD, 0x2E, 0x73,
        0x7F, 0x5E, 0x76, 0x5A, 0x7B, 0xCC, 0xD4, 0x75
    };

    public static readonly byte[] blake2s_res = new byte[] {
        0x6A, 0x41, 0x1F, 0x08, 0xCE, 0x25, 0xAD, 0xCD,
        0xFB, 0x02, 0xAB, 0xA6, 0x41, 0x45, 0x1C, 0xEC,
        0x53, 0xC5, 0x98, 0xB2, 0x4F, 0x4F, 0xC7, 0x87,
        0xFB, 0xDC, 0x88, 0x79, 0x7F, 0x4C, 0x1D, 0xFE
    };

    unsafe public static bool blake2b_selftest()
    {
        // 256-bit hash for testing
        var ctx = default(Blake2b.Blake2Context);
        Blake2b.init(ref ctx, (uint)blake2b_res.Length, null, 0);

        // grand hash of hash results
        // parameter sets
        foreach (int haslen in new[] { 20, 32, 48, 64 })
        foreach (int msglen in new[] { 0, 3, 128, 129, 255, 1024 })
        {
            var msg = new byte[msglen];

            selftest_seq(msg);                                // unkeyed hash
            var hash = Blake2b.ComputeHash(haslen, null, msg);
            fixed (byte* phash = hash)
                Blake2b.update(ref ctx, phash, (uint)haslen); // hash the hash

            selftest_seq(hash);                               // keyed hash
            hash = Blake2b.ComputeHash(haslen, hash, msg);
            fixed (byte* phash = hash)
                Blake2b.update(ref ctx, phash, (uint)haslen); // hash the hash
        }

        // compute and compare the hash of hashes
        var finhash = new byte[blake2b_res.Length];
        fixed (byte* phash = finhash)
            Blake2b.final(ref ctx, phash);

        return finhash.SequenceEqual(blake2b_res);
    }

    unsafe public static bool blake2s_selftest()
    {
        // 256-bit hash for testing
        var ctx = default(Blake2s.Blake2Context);
        Blake2s.init(ref ctx, (uint)blake2s_res.Length, null, 0);

        // grand hash of hash results
        // parameter sets
        foreach (int haslen in new[] { 16, 20, 28, 32 })
        foreach (int msglen in new[] { 0, 3, 64, 65, 255, 1024 })
        {
            var msg = new byte[msglen];

            selftest_seq(msg);                                // unkeyed hash
            var hash = Blake2s.ComputeHash(haslen, null, msg);
            fixed (byte* phash = hash)
                Blake2s.update(ref ctx, phash, (uint)haslen); // hash the hash

            selftest_seq(hash);                               // keyed hash
            hash = Blake2s.ComputeHash(haslen, hash, msg);
            fixed (byte* phash = hash)
                Blake2s.update(ref ctx, phash, (uint)haslen); // hash the hash
        }

        // compute and compare the hash of hashes
        var finhash = new byte[blake2s_res.Length];
        fixed (byte* phash = finhash)
            Blake2s.final(ref ctx, phash);

        return finhash.SequenceEqual(blake2s_res);
    }
}