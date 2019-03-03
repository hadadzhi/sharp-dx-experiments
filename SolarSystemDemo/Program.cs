using System.Windows.Forms;
using SharpDXCommons;

namespace SolarSystemDemo
{
	class Program
	{
		public static void Main()
		{
			//Application.EnableVisualStyles();

			//GraphicsSettingsDialog settings = new GraphicsSettingsDialog();

			//if (settings.ShowDialog() == DialogResult.OK)
			//{
			//	new SolarSystemDemo(settings.Configuration).Run();
			//}

			new SolarSystemDemo(new GraphicsConfiguration()).Run();
		}
	}
}
