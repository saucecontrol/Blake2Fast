// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System;

internal static class Util
{
	public static byte[] ConvertHexToBytes(string hex)
	{
		if (string.IsNullOrEmpty(hex))
			return [];

#if NET5_0_OR_GREATER
		return Convert.FromHexString(hex);
#else
		var bytes = new byte[hex.Length / 2];
		for (int i = 0; i < bytes.Length; i++)
			bytes[i] = (byte)((getDigit(hex[i * 2]) << 4) | getDigit(hex[i * 2 + 1]));

		return bytes;

		static int getDigit(char c) => c < 'a' ? c - '0' : c - 'a' + 10;
#endif
	}
}
