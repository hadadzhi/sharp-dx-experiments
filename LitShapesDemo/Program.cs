using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpDXCommons;

namespace LitShapesDemo
{
	class Program
	{
		public static void Main()
		{
			Application.EnableVisualStyles();

			GraphicsSettingsDialog settings = new GraphicsSettingsDialog();

			if (settings.ShowDialog() == DialogResult.OK)
			{
				new LitShapesDemo(settings.Configuration).Run();
			}

			//new LitShapesDemo(new GraphicsConfiguration()).Run();
		}
	}
}
