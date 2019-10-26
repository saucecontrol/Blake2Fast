using System;

namespace Blake2Fast
{
	internal static class ThrowHelper
	{
		public static void HashFinalized() => throw new InvalidOperationException("Hash has already been finalized.");

		public static void NoBigEndian() => throw new PlatformNotSupportedException("Big-endian platforms not supported");

		public static void DigestInvalidLength(int max) => throw new ArgumentOutOfRangeException("digestLength", $"Value must be between 1 and {max}");

		public static void KeyTooLong(int max) => throw new ArgumentException($"Key must be between 0 and {max} bytes in length", "key");
	}
}