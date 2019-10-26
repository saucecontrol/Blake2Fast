using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Blake2Fast
{
	internal static class ThrowHelper
	{
#if !BUILTIN_SPAN
		private static class TypeCache<T>
		{
			internal static readonly bool IsReferenceOrContainsReferences = checkRefs(typeof(T));

			private static bool checkRefs(Type t)
			{
				if (t.IsPointer)
					return false;

				var ti = t.GetTypeInfo();
				if (!ti.IsValueType)
					return true;

				return ti.DeclaredFields.Any(fi => !fi.IsStatic && fi.FieldType != t && checkRefs(fi.FieldType));
			}
		}
#endif

		public static void ThrowIfIsRefOrContainsRefs<T>()
		{
			if (
#if BUILTIN_SPAN
				RuntimeHelpers.IsReferenceOrContainsReferences<T>()
#else
				TypeCache<T>.IsReferenceOrContainsReferences
#endif
			)
				throw new NotSupportedException("This method may only be used with value types that do not contain reference type fields.");
		}

		public static void HashFinalized() => throw new InvalidOperationException("Hash has already been finalized.");

		public static void NoBigEndian() => throw new PlatformNotSupportedException("Big-endian platforms not supported");

		public static void DigestInvalidLength(int max) => throw new ArgumentOutOfRangeException("digestLength", $"Value must be between 1 and {max}");

		public static void KeyTooLong(int max) => throw new ArgumentException($"Key must be between 0 and {max} bytes in length", "key");
	}
}