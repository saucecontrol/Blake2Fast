using System;
using System.Runtime.InteropServices;

static class Interop
{
	static Interop() => SetDllDirectory(IntPtr.Size == 4 ? Paths.BlakeNativeDLLx86 : Paths.BlakeNativeDLLx64);

	[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	public extern static bool SetDllDirectory(string lpPathName);

	[DllImport("Blake2SSE.dll", EntryPoint = "blake2b")]
	public extern static void Blake2bNativeSSE41([Out] byte[] hash, IntPtr cbHash, [In] byte[] data, IntPtr cbData, [In] byte[] key, IntPtr cbKey);
	[DllImport("Blake2SSE.dll", EntryPoint = "blake2s")]
	public extern static void Blake2sNativeSSE41([Out] byte[] hash, IntPtr cbHash, [In] byte[] data, IntPtr cbData, [In] byte[] key, IntPtr cbKey);

	[DllImport("Blake2Ref.dll", EntryPoint = "blake2b")]
	public extern static void Blake2bNativeRef([Out] byte[] hash, IntPtr cbHash, [In] byte[] data, IntPtr cbData, [In] byte[] key, IntPtr cbKey);
	[DllImport("Blake2Ref.dll", EntryPoint = "blake2s")]
	public extern static void Blake2sNativeRef([Out] byte[] hash, IntPtr cbHash, [In] byte[] data, IntPtr cbData, [In] byte[] key, IntPtr cbKey);

	[DllImport("Blake2AVX.dll", EntryPoint = "blake2b")]
	public extern static void Blake2bNativeAVX2([Out] byte[] hash, IntPtr cbHash, [In] byte[] data, IntPtr cbData);
}
