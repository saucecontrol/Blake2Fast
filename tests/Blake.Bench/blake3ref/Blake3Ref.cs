using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// Ported from https://github.com/oconnor663/blake3_reference_impl_c
public static class Blake3Ref {
  const int BLAKE3_OUT_LEN   = 32;
  const int BLAKE3_KEY_LEN   = 32;
  const int BLAKE3_BLOCK_LEN = 64;
  const int BLAKE3_CHUNK_LEN = 1024;

  // Flags values
  const int CHUNK_START         = 1 << 0;
  const int CHUNK_END           = 1 << 1;
  const int PARENT              = 1 << 2;
  const int ROOT                = 1 << 3;
  const int KEYED_HASH          = 1 << 4;
  const int DERIVE_KEY_CONTEXT  = 1 << 5;
  const int DERIVE_KEY_MATERIAL = 1 << 6;

  // This struct is private.
  unsafe struct _blake3_chunk_state
  {
    public fixed uint chaining_value[8];
    public ulong chunk_counter;
    public fixed byte block[BLAKE3_BLOCK_LEN];
    public byte block_len;
    public byte blocks_compressed;
    public uint flags;
  }

  // An incremental hasher that can accept any number of writes.
  unsafe struct blake3_hasher
  {
    public _blake3_chunk_state chunk_state;
    public fixed uint key_words[8];
    public fixed uint cv_stack[8 * 54]; // Space for 54 subtree chaining values:
    public byte cv_stack_len;           // 2^54 * CHUNK_LEN = 2^64
    public uint flags;
  }

  private static ReadOnlySpan<byte> IV => [
    0x67, 0xe6, 0x09, 0x6a,
    0x85, 0xae, 0x67, 0xbb,
    0x72, 0xf3, 0x6e, 0x3c,
    0x3a, 0xf5, 0x4f, 0xa5,
    0x7f, 0x52, 0x0e, 0x51,
    0x8c, 0x68, 0x05, 0x9b,
    0xab, 0xd9, 0x83, 0x1f,
    0x19, 0xcd, 0xe0, 0x5b
  ];

  static ReadOnlySpan<byte> MSG_PERMUTATION => [ 2, 6,  3,  10, 7, 0,  4,  13,
                                                 1, 11, 12, 5,  9, 14, 15, 8 ];

  static uint rotate_right(uint x, int n) {
    return (x >> n) | (x << (32 - n));
  }

  // The mixing function, G, which mixes either a column or a diagonal.
  static void g(Span<uint> state, int a, int b, int c, int d,
                uint mx, uint my) {
    state[a] = state[a] + state[b] + mx;
    state[d] = rotate_right(state[d] ^ state[a], 16);
    state[c] = state[c] + state[d];
    state[b] = rotate_right(state[b] ^ state[c], 12);
    state[a] = state[a] + state[b] + my;
    state[d] = rotate_right(state[d] ^ state[a], 8);
    state[c] = state[c] + state[d];
    state[b] = rotate_right(state[b] ^ state[c], 7);
  }

  static void round_function(Span<uint> state, ReadOnlySpan<uint> m) {
    // Mix the columns.
    g(state, 0, 4, 8, 12, m[0], m[1]);
    g(state, 1, 5, 9, 13, m[2], m[3]);
    g(state, 2, 6, 10, 14, m[4], m[5]);
    g(state, 3, 7, 11, 15, m[6], m[7]);
    // Mix the diagonals.
    g(state, 0, 5, 10, 15, m[8], m[9]);
    g(state, 1, 6, 11, 12, m[10], m[11]);
    g(state, 2, 7, 8, 13, m[12], m[13]);
    g(state, 3, 4, 9, 14, m[14], m[15]);
  }

  static void permute(Span<uint> m) {
    var permuted = (Span<uint>)stackalloc uint[16];
    for (int i = 0; i < 16; i++) {
      permuted[i] = m[MSG_PERMUTATION[i]];
    }
    permuted.CopyTo(m);
  }

  static unsafe void compress(uint* chaining_value,
                              uint* block_words, ulong counter,
                              uint block_len, uint flags,
                              Span<uint> @out) {
    uint* iv = (uint*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(IV));
    var state = (Span<uint>)stackalloc uint[] {
        chaining_value[0],
        chaining_value[1],
        chaining_value[2],
        chaining_value[3],
        chaining_value[4],
        chaining_value[5],
        chaining_value[6],
        chaining_value[7],
        iv[0],
        iv[1],
        iv[2],
        iv[3],
        (uint)counter,
        (uint)(counter >> 32),
        block_len,
        flags,
    };
    var block = new Span<uint>(block_words, 16);

    round_function(state, block); // round 1
    permute(block);
    round_function(state, block); // round 2
    permute(block);
    round_function(state, block); // round 3
    permute(block);
    round_function(state, block); // round 4
    permute(block);
    round_function(state, block); // round 5
    permute(block);
    round_function(state, block); // round 6
    permute(block);
    round_function(state, block); // round 7

    for (int i = 0; i < 8; i++) {
      state[i] ^= state[i + 8];
      state[i + 8] ^= chaining_value[i]; // Feed forward is optional if not length extending
    }

    state.Slice(0, @out.Length).CopyTo(@out);
  }

  // Each chunk or parent node can produce either an 8-word chaining value or, by
  // setting the ROOT flag, any number of final output bytes. The Output struct
  // captures the state just prior to choosing between those two possibilities.
  unsafe struct _output {
    public fixed uint input_chaining_value[8];
    public fixed uint block_words[16];
    public ulong counter;
    public uint block_len;
    public uint flags;
  }

  static unsafe void output_chaining_value(_output* self, Span<uint> @out) {
    compress(self->input_chaining_value, self->block_words, self->counter,
             self->block_len, self->flags, @out);
  }

  static unsafe void output_root_bytes(_output* self, void* @out,
                                       nuint out_len) {
    byte* out_u8 = (byte*)@out;
    ulong output_block_counter = 0;
    var words = (Span<uint>)stackalloc uint[16];
    while (out_len > 0) {
      compress(self->input_chaining_value, self->block_words,
               output_block_counter, self->block_len, self->flags | ROOT, words);
      for (int word = 0; word < 16; word++) {
        for (int @byte = 0; @byte < 4; @byte++) {
          if (out_len == 0) {
            return;
          }
          *out_u8 = (byte)(words[word] >> (8 * @byte));
          out_u8++;
          out_len--;
        }
      }
      output_block_counter++;
    }
  }

  static unsafe void chunk_state_init(_blake3_chunk_state* self,
                                      uint* key_words,
                                      ulong chunk_counter, uint flags) {
    Unsafe.CopyBlockUnaligned(self->chaining_value, key_words, 8 * sizeof(uint));
    self->chunk_counter = chunk_counter;
    Unsafe.InitBlock(self->block, 0, BLAKE3_BLOCK_LEN);
    self->block_len = 0;
    self->blocks_compressed = 0;
    self->flags = flags;
  }

  static unsafe nuint chunk_state_len(_blake3_chunk_state* self) {
    return BLAKE3_BLOCK_LEN * (nuint)self->blocks_compressed +
           (nuint)self->block_len;
  }

  static unsafe uint chunk_state_start_flag(_blake3_chunk_state* self) {
    if (self->blocks_compressed == 0) {
      return CHUNK_START;
    } else {
      return 0;
    }
  }

  static unsafe void chunk_state_update(_blake3_chunk_state* self,
                                        void* input, nuint input_len) {
    byte* input_u8 = (byte*)input;
    while (input_len > 0) {
      // If the block buffer is full, compress it and clear it. More input is
      // coming, so this compression is not CHUNK_END.
      if (self->block_len == BLAKE3_BLOCK_LEN) {
        uint* block_words = (uint*)self->block;
        compress(self->chaining_value, block_words, self->chunk_counter,
                 BLAKE3_BLOCK_LEN, self->flags | chunk_state_start_flag(self),
                 new Span<uint>(self->chaining_value, 8));
        self->blocks_compressed++;
        Unsafe.InitBlock(self->block, 0, BLAKE3_BLOCK_LEN);
        self->block_len = 0;
      }

      // Copy input bytes into the block buffer.
      nuint want = BLAKE3_BLOCK_LEN - (nuint)self->block_len;
      nuint take = want;
      if (input_len < want) {
        take = input_len;
      }
      Unsafe.CopyBlockUnaligned(&self->block[(nuint)self->block_len], input_u8, (uint)take);
      self->block_len += (byte)take;
      input_u8 += take;
      input_len -= take;
    }
  }

  static unsafe _output chunk_state_output(_blake3_chunk_state* self) {
    _output ret;
    Unsafe.CopyBlockUnaligned(ret.input_chaining_value, self->chaining_value, 8 * sizeof(uint));
    Unsafe.CopyBlockUnaligned(ret.block_words, self->block, BLAKE3_BLOCK_LEN);
    ret.counter = self->chunk_counter;
    ret.block_len = (uint)self->block_len;
    ret.flags = self->flags | chunk_state_start_flag(self) | CHUNK_END;
    return ret;
  }

  static unsafe _output parent_output(uint* left_child_cv,
                                     uint* right_child_cv,
                                     uint* key_words,
                                     uint flags) {
    _output ret;
    Unsafe.CopyBlockUnaligned(ret.input_chaining_value, key_words, 8 * sizeof(uint));
    Unsafe.CopyBlockUnaligned(&ret.block_words[0], left_child_cv, 8 * sizeof(uint));
    Unsafe.CopyBlockUnaligned(&ret.block_words[8], right_child_cv, 8 * sizeof(uint));
    ret.counter = 0; // Always 0 for parent nodes.
    ret.block_len =
        BLAKE3_BLOCK_LEN; // Always BLAKE3_BLOCK_LEN (64) for parent nodes.
    ret.flags = PARENT | flags;
    return ret;
  }

  static unsafe void parent_cv(uint* left_child_cv,
                               uint* right_child_cv,
                               uint* key_words, uint flags,
                               Span<uint> @out) {
    _output o = parent_output(left_child_cv, right_child_cv, key_words, flags);
    // We only write to `out` after we've read the inputs. That makes it safe for
    // `out` to alias an input, which we do below.
    output_chaining_value(&o, @out);
  }

  static unsafe void hasher_init_internal(blake3_hasher* self,
                                          uint* key_words,
                                          uint flags) {
    chunk_state_init(&self->chunk_state, key_words, 0, flags);
    Unsafe.CopyBlockUnaligned(self->key_words, key_words, BLAKE3_KEY_LEN);
    self->cv_stack_len = 0;
    self->flags = flags;
  }

  // Construct a new `Hasher` for the regular hash function.
  static unsafe void blake3_hasher_init(blake3_hasher* self) {
    hasher_init_internal(self, (uint*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(IV)), 0);
  }

  // Construct a new `Hasher` for the keyed hash function.
  static unsafe void blake3_hasher_init_keyed(blake3_hasher* self,
                                byte* key) {
    uint* key_words = (uint*)key;
    hasher_init_internal(self, key_words, KEYED_HASH);
  }

  // Construct a new `Hasher` for the key derivation function. The context
  // string should be hardcoded, globally unique, and application-specific.
  static unsafe void blake3_hasher_init_derive_key(blake3_hasher* self, byte* context) {
    blake3_hasher context_hasher;
    hasher_init_internal(&context_hasher, (uint*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(IV)), DERIVE_KEY_CONTEXT);
    blake3_hasher_update(&context_hasher, context, (uint)new ReadOnlySpan<byte>(context, int.MaxValue).IndexOf((byte)0));
    byte* context_key = stackalloc byte[BLAKE3_KEY_LEN];
    blake3_hasher_finalize(&context_hasher, context_key, BLAKE3_KEY_LEN);
    uint* context_key_words = (uint*)context_key;
    hasher_init_internal(self, context_key_words, DERIVE_KEY_MATERIAL);
  }

  static unsafe void hasher_push_stack(blake3_hasher* self,
                                       uint* cv) {
    Unsafe.CopyBlockUnaligned(&self->cv_stack[(nuint)self->cv_stack_len * 8], cv, 8 * sizeof(uint));
    self->cv_stack_len++;
  }

  // Returns a pointer to the popped CV, which is valid until the next push.
  static unsafe uint *hasher_pop_stack(blake3_hasher* self) {
    self->cv_stack_len--;
    return &self->cv_stack[(nuint)self->cv_stack_len * 8];
  }

  // Section 5.1.2 of the BLAKE3 spec explains this algorithm in more detail.
  static unsafe void hasher_add_chunk_cv(blake3_hasher* self, uint* new_cv,
                                         ulong total_chunks) {
    // This chunk might complete some subtrees. For each completed subtree, its
    // left child will be the current top entry in the CV stack, and its right
    // child will be the current value of `new_cv`. Pop each left child off the
    // stack, merge it with `new_cv`, and overwrite `new_cv` with the result.
    // After all these merges, push the final value of `new_cv` onto the stack.
    // The number of completed subtrees is given by the number of trailing 0-bits
    // in the new total number of chunks.
    while ((total_chunks & 1) == 0) {
      parent_cv(hasher_pop_stack(self), new_cv, self->key_words, self->flags,
                new Span<uint>(new_cv, 8));
      total_chunks >>= 1;
    }
    hasher_push_stack(self, new_cv);
  }

  // Add input to the hash state. This can be called any number of times.
  static unsafe void blake3_hasher_update(blake3_hasher* self, void* input,
                                   nuint input_len) {
    byte* input_u8 = (byte*)input;
    uint* chunk_cv = stackalloc uint[8];
    while (input_len > 0) {
      // If the current chunk is complete, finalize it and reset the chunk state.
      // More input is coming, so this chunk is not ROOT.
      if (chunk_state_len(&self->chunk_state) == BLAKE3_CHUNK_LEN) {
        _output chunk_output = chunk_state_output(&self->chunk_state);
        output_chaining_value(&chunk_output, new Span<uint>(chunk_cv, 8));
        ulong total_chunks = self->chunk_state.chunk_counter + 1;
        hasher_add_chunk_cv(self, chunk_cv, total_chunks);
        chunk_state_init(&self->chunk_state, self->key_words, total_chunks,
                         self->flags);
      }

      // Compress input bytes into the current chunk state.
      nuint want = BLAKE3_CHUNK_LEN - chunk_state_len(&self->chunk_state);
      nuint take = want;
      if (input_len < want) {
        take = input_len;
      }
      chunk_state_update(&self->chunk_state, input_u8, take);
      input_u8 += take;
      input_len -= take;
    }
  }

  // Finalize the hash and write any number of output bytes.
  static unsafe void blake3_hasher_finalize(blake3_hasher* self, void* @out,
                                     nuint out_len) {
    // Starting with the output from the current chunk, compute all the parent
    // chaining values along the right edge of the tree, until we have the root
    // output.
    _output current_output = chunk_state_output(&self->chunk_state);
    nuint parent_nodes_remaining = (nuint)self->cv_stack_len;
    uint* current_cv = stackalloc uint[8];
    while (parent_nodes_remaining > 0) {
      parent_nodes_remaining--;
      output_chaining_value(&current_output, new Span<uint>(current_cv, 8));
      current_output = parent_output(&self->cv_stack[parent_nodes_remaining * 8],
                                     current_cv, self->key_words, self->flags);
    }
    output_root_bytes(&current_output, @out, out_len);
  }

  const int MAX_OUT_LEN = 131;

  public static unsafe byte[] Hash(int len, ReadOnlySpan<byte> data, ReadOnlySpan<byte> key, ReadOnlySpan<byte> context)
  {
    byte* output = stackalloc byte[MAX_OUT_LEN];

    // Initialize the hasher.
    blake3_hasher hasher;
    if (!context.IsEmpty)
      fixed (byte* pctx = context)
        blake3_hasher_init_derive_key(&hasher, pctx);
    else if (!key.IsEmpty)
      fixed (byte* pkey = key)
        blake3_hasher_init_keyed(&hasher, pkey);
    else
      blake3_hasher_init(&hasher);

    // Mix the data
    fixed (byte* pdata = data)
      blake3_hasher_update(&hasher, pdata, (uint)data.Length);

    // Finalize the hash.
    blake3_hasher_finalize(&hasher, output, (uint)len);

    return new ReadOnlySpan<byte>(output, len).ToArray();
  }

  public static unsafe bool SelfTest()
  {
    // Results from https://github.com/BLAKE3-team/BLAKE3/blob/master/test_vectors/test_vectors.json
    var key = "whats the Elvish word for friend"u8;
    var context = "BLAKE3 2019-12-27 16:29:52 test vectors context"u8;

    var hd5121 = (ReadOnlySpan<byte>)[
      0x62, 0x8b, 0xd2, 0xcb, 0x20, 0x04, 0x69, 0x4a, 0xda, 0xab, 0x7b, 0xbd, 0x77, 0x8a, 0x25, 0xdf,
      0x25, 0xc4, 0x7b, 0x9d, 0x41, 0x55, 0xa5, 0x5f, 0x8f, 0xbd, 0x79, 0xf2, 0xfe, 0x15, 0x4c, 0xff
    ];
    var hk1024 = (ReadOnlySpan<byte>)[
      0x75, 0xc4, 0x6f, 0x6f, 0x3d, 0x9e, 0xb4, 0xf5, 0x5e, 0xca, 0xae, 0xe4, 0x80, 0xdb, 0x73, 0x2e,
      0x6c, 0x21, 0x05, 0x54, 0x6f, 0x1e, 0x67, 0x50, 0x03, 0x68, 0x7c, 0x31, 0x71, 0x9c, 0x7b, 0xa4
    ];
    var hc0000 = (ReadOnlySpan<byte>)[
      0x2c, 0xc3, 0x97, 0x83, 0xc2, 0x23, 0x15, 0x4f, 0xea, 0x8d, 0xfb, 0x7c, 0x1b, 0x16, 0x60, 0xf2,
      0xac, 0x2d, 0xcb, 0xd1, 0xc1, 0xde, 0x82, 0x77, 0xb0, 0xb0, 0xdd, 0x39, 0xb7, 0xe5, 0x0d, 0x7d,
      0x90, 0x56, 0x30, 0xc8, 0xbe, 0x29, 0x0d, 0xfc, 0xf3, 0xe6, 0x84, 0x2f, 0x13, 0xbd, 0xdd, 0x57,
      0x3c, 0x09, 0x8c, 0x3f, 0x17, 0x36, 0x1f, 0x1f, 0x20, 0x6b, 0x8c, 0xad, 0x9d, 0x08, 0x8a, 0xa4,
      0xa3, 0xf7, 0x46, 0x75, 0x2c, 0x6b, 0x0c, 0xe6, 0xa8, 0x3b, 0x0d, 0xa8, 0x1d, 0x59, 0x64, 0x92,
      0x57, 0xcd, 0xf8, 0xeb, 0x3e, 0x9f, 0x7d, 0x49, 0x98, 0xe4, 0x10, 0x21, 0xfa, 0xc1, 0x19, 0xde,
      0xef, 0xb8, 0x96, 0x22, 0x4a, 0xc9, 0x9f, 0x86, 0x00, 0x11, 0xf7, 0x36, 0x09, 0xe6, 0xe0, 0xe4,
      0x54, 0x0f, 0x93, 0xb2, 0x73, 0xe5, 0x65, 0x47, 0xdf, 0xd3, 0xaa, 0x1a, 0x03, 0x5b, 0xa6, 0x68,
      0x9d, 0x89, 0xa0
    ];

    byte[] data = new byte[5121];
    for (int i = 0; i < data.Length; i++)
      data[i] = (byte)(i % 251);

    var check = Hash(BLAKE3_OUT_LEN, data, default, default).AsSpan();
    if (!check.SequenceEqual(hd5121))
      return false;

    check = Hash(BLAKE3_OUT_LEN, data.AsSpan(0, 1024), key, default).AsSpan();
    if (!check.SequenceEqual(hk1024))
      return false;

    check = Hash(MAX_OUT_LEN, default, default, context).AsSpan();
    if (!check.SequenceEqual(hc0000))
      return false;

    return true;
  }
}
