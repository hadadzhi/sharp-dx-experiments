using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDXCommons;

namespace BoxDemoComeBack
{
	class Program
	{
		public static void Main()
		{
			Application.EnableVisualStyles();

			GraphicsSettingsDialog settings = new GraphicsSettingsDialog();

			if (settings.ShowDialog() == DialogResult.OK)
			{
				new BoxDemo(settings.Configuration).Run();
			}
		}
	}
}
