using SharpDXCommons;
using System;
using System.Windows.Forms;

namespace SMAADemo1
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();

			GraphicsConfiguration conf = new GraphicsConfiguration();

			conf.DisplayMode.Width = 1280;
			conf.DisplayMode.Height = 720;

			new SMAADemo1(conf).Run();
		}
	}
}
