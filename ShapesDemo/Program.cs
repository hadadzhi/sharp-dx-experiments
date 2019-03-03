using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpDXCommons;

namespace ShapesDemo
{
	class Program
	{
		public static void Main()
		{
			Application.EnableVisualStyles();

			GraphicsSettingsDialog settings = new GraphicsSettingsDialog();

			if (settings.ShowDialog() == DialogResult.OK)
			{
				new ShapesDemo(settings.Configuration).Run();
			}

			//new ShapesDemo(new GraphicsConfiguration()).Run();
		}
	}
}
