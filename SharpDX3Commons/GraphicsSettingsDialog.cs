using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;

namespace SharpDXCommons
{
	public partial class GraphicsSettingsDialog : Form
	{
		private Adapter[] Adapters;
		private Output[] Outputs;
		private ModeDescription[] Modes;

		public GraphicsConfiguration Configuration { get; private set; }

		public GraphicsSettingsDialog()
		{
			InitializeComponent();
		}

		private void GraphicsSettingsDialog_Load(object sender, EventArgs e)
		{
			CenterToScreen();

			Adapters = DXGIFactory.Instance.Adapters;
			String[] AdapterStrings = new String[Adapters.Length];

			for (int i = 0; i < AdapterStrings.Length; i++)
			{
				AdapterStrings[i] = Adapters[i].Description.Description;
			}

			AdapterSelect.DataSource = AdapterStrings;

			LaunchButton.Enabled = Adapters.Length > 0;

			AdapterSelect_SelectedIndexChanged(null, null);
		}

		private void AdapterSelect_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Adapters.Length > 0)
			{
				Outputs = GetOutputs(AdapterSelect.SelectedIndex);
				String[] OutputStrings = new String[Outputs.Length];

				for (int i = 0; i < OutputStrings.Length; i++)
				{
					OutputStrings[i] = Outputs[i].Description.DeviceName;
				}

				OutputSelect.DataSource = OutputStrings;

				FeatureLevel maxFL = Device.GetSupportedFeatureLevel(Adapters[AdapterSelect.SelectedIndex]);

				LaunchButton.Enabled = Outputs.Length > 0 && maxFL >= FeatureLevel.Level_11_0;

				StatusLabel.Text = "Highest supported feature level: " + maxFL.ToString();
			}
			else
			{
				OutputSelect.DataSource = null;
			}

			OutputSelect_SelectedIndexChanged(null, null);
		}

		private void OutputSelect_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Outputs.Length > 0)
			{
				Modes = Outputs[OutputSelect.SelectedIndex].GetDisplayModeList(Format.R8G8B8A8_UNorm, 0);
				String[] ModeStrings = new String[Modes.Length];

				for (int i = 0; i < ModeStrings.Length; i++)
				{
					ModeStrings[i] =
						Modes[i].Width + "x" + Modes[i].Height + "@" +
						Modes[i].RefreshRate.Numerator / Modes[i].RefreshRate.Denominator + "Hz";
				}

				ModeSelect.DataSource = ModeStrings;

				if (Modes.Length > 0)
				{
					ModeSelect.SelectedIndex = Modes.Length - 1;
				}

				LaunchButton.Enabled = LaunchButton.Enabled &&  (Modes.Length > 0);
			}
			else
			{
				ModeSelect.DataSource = null;
			}
		}

		private void LaunchButton_Click(object sender, EventArgs e)
		{
			Configuration = new GraphicsConfiguration();

			Configuration.Adapter = Adapters[AdapterSelect.SelectedIndex];
			Configuration.DisplayMode = Modes[ModeSelect.SelectedIndex];

			Configuration.BufferCount = TripleBufferingCheckBox.Checked ? 3 : 2;
			Configuration.SyncInterval = VSyncCheckBox.Checked ? 1 : 0;

			Configuration.Windowed = WindowedCheckBox.Checked;
			Configuration.FullscreenWindow = FullscreenWindowCheckBox.Checked;

			DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		private Output[] GetOutputs(int adapterIndex)
		{
			Output[] outputs = new Output[Adapters[adapterIndex].GetOutputCount()];

			for (int i = 0; i < outputs.Length; i++)
			{
				outputs[i] = Adapters[adapterIndex].GetOutput(i);
			}

			return outputs;
		}
	}
}
