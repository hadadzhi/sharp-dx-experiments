using SharpDX;
using SharpDXCommons;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Experiments
{
	[StructLayout(LayoutKind.Explicit, Size = 32)]
	struct TestStruct
	{
		[FieldOffset(0)]
		public Vector3 v1;

		[FieldOffset(16)]
		public Vector3 v2;
	}

	class Program
	{
		//	static void Main(string[] args)
		//	{
		//		TestStruct t = new TestStruct
		//		{
		//			v1 = Vector3.Zero,
		//			v2 = Vector3.One
		//		};

		//		Console.WriteLine("Testing speed");

		//		Clock c = new Clock();
		//		double delta;
		//		const int MAX_CALLS = 100000;
		//		int calls;
		//		bool returnVal = false;

		//		// 1
		//		calls = 0;
		//		c.Start();

		//		for (int i = 0; i < MAX_CALLS; i++)
		//		{
		//			unsafe
		//			{
		//				returnVal = NativeWithVoid(&t);
		//			}

		//			calls++;

		//			if (!returnVal)
		//			{
		//				break;
		//			}
		//		}

		//		delta = c.Delta();
		//		Console.WriteLine(String.Format("{0} calls through unsafe function with void* parameter: {1,0:F1} ms", calls, delta * 1000));

		//		// 2
		//		calls = 0;
		//		c.Start();

		//		for (int i = 0; i < MAX_CALLS; i++)
		//		{
		//			returnVal = NativeWithStruct(ref t);
		//			calls++;

		//			if (!returnVal)
		//			{
		//				break;
		//			}
		//		}

		//		delta = c.Delta();
		//		Console.WriteLine(String.Format("{0} calls through safe function with struct parameter: {1,0:F1} ms", calls, delta * 1000));

		//		// 3
		//		calls = 0;
		//		c.Start();

		//		for (int i = 0; i < MAX_CALLS; i++)
		//		{
		//			returnVal = NativeGeneric(ref t);

		//			calls++;

		//			if (!returnVal)
		//			{
		//				break;
		//			}
		//		}

		//		delta = c.Delta();
		//		Console.WriteLine(String.Format("{0} calls through generic function with manual marshalling: {1,0:F1} ms", calls, delta * 1000));

		//		// 4
		//		calls = 0;
		//		c.Start();

		//		for (int i = 0; i < MAX_CALLS; i++)
		//		{
		//			unsafe
		//			{
		//				returnVal = NativeWithIntPtr(new IntPtr(&t));
		//			}

		//			calls++;

		//			if (!returnVal)
		//			{
		//				break;
		//			}
		//		}

		//		delta = c.Delta();
		//		Console.WriteLine(String.Format("{0} calls through safe function with IntPtr parameter, creating IntPtr from void*: {1,0:F1} ms", calls, delta * 1000));

		//		// 5
		//		calls = 0;
		//		c.Start();

		//		for (int i = 0; i < MAX_CALLS; i++)
		//		{
		//			returnVal = ManagedFunction(ref t);

		//			calls++;

		//			if (!returnVal)
		//			{
		//				break;
		//			}
		//		}

		//		delta = c.Delta();
		//		Console.WriteLine(String.Format("{0} calls through managed function: {1,0:F1} ms", calls, delta * 1000));

		//		// 6
		//		calls = 0;
		//		c.Start();

		//		for (int i = 0; i < MAX_CALLS; i++)
		//		{
		//			returnVal = NativeGenericWithGCHandle(ref t);

		//			calls++;

		//			if (!returnVal)
		//			{
		//				break;
		//			}
		//		}

		//		delta = c.Delta();
		//		Console.WriteLine(String.Format("{0} calls through generic function using GCHandle: {1,0:F1} ms", calls, delta * 1000));

		//		// 6
		//		calls = 0;
		//		c.Start();

		//		for (int i = 0; i < MAX_CALLS; i++)
		//		{
		//			PrimitiveNativeFunction(1, 1, 1);
		//			calls++;
		//		}

		//		delta = c.Delta();
		//		Console.WriteLine(String.Format("{0} calls through native function with primitive parameters: {1,0:F1} ms", calls, delta * 1000));

		//		Console.WriteLine("Press any key to exit");
		//		Console.ReadKey();
		//	}

		//	[DllImport("ExperimentsNative", EntryPoint = "NativeFunction")]
		//	private unsafe extern static bool NativeWithVoid(void* ptr);

		//	[DllImport("ExperimentsNative", EntryPoint = "NativeFunction")]
		//	private extern static bool NativeWithStruct(ref TestStruct t);

		//	[DllImport("ExperimentsNative", EntryPoint = "NativeFunction")]
		//	private extern static bool NativeWithIntPtr(IntPtr ptr);

		//	private static bool NativeGeneric<T>(ref T t) where T : struct
		//	{
		//		IntPtr tPtr = Marshal.AllocHGlobal(Marshal.SizeOf(t));
		//		Marshal.StructureToPtr(t, tPtr, true);

		//		bool result = NativeWithIntPtr(tPtr);

		//		Marshal.FreeHGlobal(tPtr);

		//		return result;
		//	}

		//	private static bool NativeGenericWithGCHandle<T>(ref T t) where T : struct
		//	{
		//		GCHandle gch = GCHandle.Alloc(t, GCHandleType.Pinned);

		//		bool result = NativeWithIntPtr(gch.AddrOfPinnedObject());

		//		gch.Free();

		//		return result;
		//	}

		//	private static bool ManagedFunction(ref TestStruct t)
		//	{
		//		return t.v1.X == 0 && t.v2.Y == 1;
		//	}

		//[DllImport("ExperimentsNative", EntryPoint = "NativeFunctionWithPrimitiveParameters")]
		//private static extern void PrimitiveNativeFunction(float arg1, double arg2, int arg3);

		static void Main(String[] args)
		{
			Random r = new Random();
			bool success = false;
			float a;
			float b;

			const int iterations = int.MaxValue;
			
			for (int i = 0; i < iterations; i++)
			{
				a = r.NextFloat(float.MinValue, float.MaxValue);
				b = a;

				if (a == b)
				{
					success = true;
				}
				else
				{
					success = false;
					break;
				}
			}

			Console.WriteLine(success);
		}
	}
}
