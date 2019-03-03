using SharpDXCommons;
using System;
using System.Windows.Forms;

namespace SMAADemo
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
				new SMAADemo(conf.Configuration).Run();
			}

			//new SMAADemo(new GraphicsConfiguration()).Run();
		}
	}
}
