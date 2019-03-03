using SharpDXCommons;
using System.Windows.Forms;

namespace SpriteDemo
{
	class Program
	{
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();

			GraphicsSettingsDialog settings = new GraphicsSettingsDialog();

			if (settings.ShowDialog() == DialogResult.OK)
			{
				new SpriteDemo(settings.Configuration).Run();
			}
		}
	}
}
