using SharpDX.DXGI;
using SharpDXCommons;

namespace GaussianFilter
{
	class Program
	{
		static void Main(string[] args)
		{
			GraphicsConfiguration conf = new GraphicsConfiguration();
			conf.DisplayMode.Width = 1920;
			conf.DisplayMode.Height = 1080;
			conf.DisplayMode.Format = Format.R8G8B8A8_UNorm_SRgb;
			conf.FullscreenWindow = true;
			new GaussianFilter(conf).Run();
		}
	}
}
