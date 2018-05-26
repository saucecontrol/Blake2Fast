using System;

namespace SauceControl.Blake2Fast
{
	public interface IBlake2Incremental
	{
#if FAST_SPAN
		void Update(ReadOnlySpan<byte> data);
#else
		void Update(byte[] data);
#endif

		byte[] Finish();
	}
}
