using SharpDXCommons;
using System;
using System.Windows.Forms;

namespace CascadedShadowMaps
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();

			GraphicsSettingsDialog conf = new GraphicsSettingsDialog();

			if (conf.ShowDialog() == DialogResult.OK)
			{
				new CSMDemo(conf.Configuration).Run();
			}

			//new CSMDemo(new GraphicsConfiguration()).Run();
		}
	}
}
