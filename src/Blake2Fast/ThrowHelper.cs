using System;

#if !BUILTIN_SPAN
using System.Linq;
using System.Reflection;
#else
using System.Runtime.CompilerServices;
#endif

namespace Blake2Fast
{
	internal static class ThrowHelper
	{
#if !BUILTIN_SPAN
		private static class TypeCache<T>
		{
			public static readonly bool IsReferenceOrContainsReferences = isOrContainsRef(typeof(T));

			private static bool isOrContainsRef(Type t)
			{
				if (t.IsPointer)
					return false;

				var ti = t.GetTypeInfo();
				if (!ti.IsValueType)
					return true;

				return ti.DeclaredFields.Any(fi => !fi.IsStatic && fi.FieldType != t && isOrContainsRef(fi.FieldType));
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
			) throw new NotSupportedException("This method may only be used with value types that do not contain reference type fields.");
		}

		public static void HashFinalized() => throw new InvalidOperationException("Hash has already been finalized.");

		public static void HashNotInitialized() => throw new InvalidOperationException("Hash not initialized.  Do not create the state struct instance directly; use CreateIncrementalHasher.");

		public static void NoBigEndian() => throw new PlatformNotSupportedException("Big-endian platforms not supported");

		public static void DigestInvalidLength(int max) => throw new ArgumentOutOfRangeException("digestLength", $"Value must be between 1 and {max}");

		public static void KeyTooLong(int max) => throw new ArgumentException($"Key must be between 0 and {max} bytes in length", "key");

		public static void OutputTooSmall(int min) => throw new ArgumentException($"Output must be at least {min} bytes in length", "output");
	}
}