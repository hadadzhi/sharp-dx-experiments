using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;

namespace FeatureLevelTest
{
	class Program
	{
		static void Main(string[] args)
		{
			foreach (FeatureLevel fl in Enum.GetValues(typeof(FeatureLevel)))
			{
				Console.WriteLine(fl.ToString() + " - " + Device.IsSupportedFeatureLevel(fl));
			}
		}
	}
}
