using SharpDX.DXGI;
using SharpDXCommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDRDemo
{
	class Program
	{
		static void Main(string[] args)
		{
			GraphicsConfiguration conf = new GraphicsConfiguration();
			conf.DisplayMode.Width = 1200;
			conf.DisplayMode.Height = 803;
			conf.DisplayMode.Format = Format.R8G8B8A8_UNorm_SRgb;
			new HDRDemo(conf).Run();
		}
	}
}
