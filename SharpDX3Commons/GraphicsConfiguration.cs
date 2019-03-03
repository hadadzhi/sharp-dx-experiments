using SharpDX.DXGI;

namespace SharpDXCommons
{
	public class GraphicsConfiguration
	{
		public Adapter Adapter;
		public ModeDescription DisplayMode;

		/// <summary>
		/// MSAA settings.
		/// </summary>
		public SampleDescription SampleDescription;

		/// <summary>
		/// 1 for VSync ON, 0 for VSync OFF.
		/// </summary>
		public int SyncInterval;

		public int BufferCount;

		public bool Windowed;
		public bool FullscreenWindow;

		public GraphicsConfiguration()
		{
			Adapter = DXGIFactory.Instance.GetAdapter(0);
			
			DisplayMode = new ModeDescription
			{
				Format = Format.R8G8B8A8_UNorm,
				Width = 800,
				Height = 600,
				RefreshRate = new Rational(0, 1),
				Scaling = DisplayModeScaling.Unspecified,
				ScanlineOrdering = DisplayModeScanlineOrder.Unspecified
			};
			
			// Force 4x MSAA by default
			SampleDescription = new SampleDescription
			{
				Count = 4,
				Quality = 0
			};

			SyncInterval = 0;
			BufferCount = 2;

			Windowed = true;
			FullscreenWindow = false;
		}
	}
}
