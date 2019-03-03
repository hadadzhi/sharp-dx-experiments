using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SharpDXCommons
{
	public class RenderWindow : Form
	{
		public delegate void VoidCallback();
		public delegate void ResizeCallback(int newClientWidth, int newClientHeight);

		/// <summary>
		/// Should render the next frame, called on WM_PAINT.
		/// </summary>
		public VoidCallback RenderCallback = delegate { };

		/// <summary>
		/// Fired when this window's ClientSize is changed.
		/// </summary>
		/// <remarks>
		/// This event will not be fired when the window enters a state which is inappropriate for rendering buffers resize,
		/// such as minimized or empty client area, or in the middle of the user manual resizing process.
		/// </remarks>
		public event ResizeCallback ClientResized = delegate { };

		private FormWindowState LastState;
		private Rectangle LastBounds;

		public bool Fullscreen
		{
			get { return _Fullscreen; }
			set
			{
				if (_Fullscreen != value)
				{
					if (value == true)
					{
						_Fullscreen = true;

						LastState = WindowState;
						LastBounds = Bounds;

						FormBorderStyle = FormBorderStyle.None;
						WindowState = FormWindowState.Normal;
						Bounds = Screen.FromControl(this).Bounds;
					}
					else
					{
						_Fullscreen = false;

						FormBorderStyle = FormBorderStyle.Sizable;
						WindowState = LastState;
						Bounds = LastBounds;
					}
				}
			}
		}

		public void ResizeClientArea(int width, int height)
		{
			WindowState = FormWindowState.Normal;
			ClientSize = new Size(width, height);
			FireClientResized();
		}

		private bool _Fullscreen = false;

		private bool IsManualResizing = false;

		public RenderWindow() : this(null, null) { }

		public RenderWindow(String title) : this(title, null) { }

		public RenderWindow(VoidCallback renderCallback) : this(null, renderCallback) { }

		public RenderWindow(String title, VoidCallback renderCallback) : base()
		{
			Text = title;
			RenderCallback = renderCallback;
			LastState = WindowState;
			LastBounds = Bounds;
		}

		protected override void WndProc(ref Message m)
		{
			switch(m.Msg)
			{
				case WindowsConstants.WM_ERASEBKGND:
				{
					// Drop this message to prevent flicker during window dragging
					return;
				}
				case WindowsConstants.WM_PAINT:
				{
					RenderCallback();
					return;
				}
				case WindowsConstants.WM_ENTERSIZEMOVE:
				{
					IsManualResizing = true;
					break;
				}
				case WindowsConstants.WM_EXITSIZEMOVE:
				{
					IsManualResizing = false;
					FireClientResized();
					break;
				}
				case WindowsConstants.WM_SIZE:
				{
					FireClientResized();					
					return;
				}
			}

			base.WndProc(ref m);
		}

		private void FireClientResized()
		{
			if (!IsManualResizing && WindowState != FormWindowState.Minimized && ClientSize.Width > 0 && ClientSize.Height > 0)
			{
				ClientResized(ClientSize.Width, ClientSize.Height);
			}
		}
	}
}
