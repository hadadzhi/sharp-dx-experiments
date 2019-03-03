using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Device = SharpDX.Direct3D11.Device;

namespace SharpDXCommons
{
	public class SharpDXApplication
	{
		private const DeviceCreationFlags DEVICE_CREATION_FLAGS = DeviceCreationFlags.None;
		private const SwapChainFlags SWAP_CHAIN_FLAGS = SwapChainFlags.AllowModeSwitch;
		private const Format DEPTH_STENCIL_FORMAT = Format.D24_UNorm_S8_UInt;

		protected String ApplicationTitle;

		protected GraphicsConfiguration Configuration;

		protected RenderWindow RenderWindow;

		protected Device Device;
		protected DeviceContext Context;

		protected SwapChain SwapChain;

		private Texture2D DepthStencilBuffer;
		protected DepthStencilView DepthStencilView;

		private Texture2D RenderTarget;
		protected RenderTargetView RenderTargetView;

		protected ViewportF DefaultViewport;

		private Clock Clock;

		private bool Fullscreen;

		// Used to calculate frame statistics
		private int FrameCount = 0;
		private double ElapsedTime = 0;

		protected delegate void BuffersResizedCallback(int newWidth, int newHeight);
		protected event BuffersResizedCallback TargetsResized = delegate { };

		protected PipelineStates PipelineStates;

		public SharpDXApplication(GraphicsConfiguration configuration, String title)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException();
			}

			Configuration = configuration;
			ApplicationTitle = title;
			Fullscreen = !configuration.Windowed;

			Clock = new Clock();

			Init();

			PipelineStates = new PipelineStates(Device);
		}

		private void Init()
		{
			InitWindow();
			InitDevice();
			InitTargets();
			RegisterEventHandlers();
		}

		private void InitDevice()
		{
			SwapChainDescription SwapChainDesc = new SwapChainDescription
			{
				BufferCount = Configuration.BufferCount,
				Flags = SWAP_CHAIN_FLAGS,
				IsWindowed = true, // Fullscreen mode will be set later, when rendering window is properly set
				ModeDescription = Configuration.DisplayMode,
				OutputHandle = RenderWindow.Handle,
				SampleDescription = Configuration.SampleDescription,
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput
			};

			Device.CreateWithSwapChain(Configuration.Adapter, DEVICE_CREATION_FLAGS, SwapChainDesc, out Device, out SwapChain);

			Context = Device.ImmediateContext;

			// Prevent DXGI handling of Windows messages, which doesn't work properly with Windows Forms
			SwapChain.GetParent<Factory>().MakeWindowAssociation(RenderWindow.Handle, WindowAssociationFlags.IgnoreAll);
		}

		private void InitWindow()
		{
			RenderWindow = new RenderWindow(ApplicationTitle, RenderCallback);

			RenderWindow.ClientSize = new System.Drawing.Size
			{
				Width = Configuration.DisplayMode.Width,
				Height = Configuration.DisplayMode.Height
			};
		}

		private void OnUnhandledException(Exception e)
		{
			Log.Error("Unhandled exception: " + e.ToString());

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();

			Application.Exit();
			Environment.Exit(1);
		}

		private void RegisterEventHandlers()
		{
			AppDomain.CurrentDomain.UnhandledException += (o, e) =>
			{
				OnUnhandledException((Exception) e.ExceptionObject);
			};

			Application.ThreadException += (o, e) =>
			{
				OnUnhandledException(e.Exception);
			};

			Application.ApplicationExit += (sender, e) =>
			{
				DisposeResources();
			};

			RenderWindow.ResizeCallback windowResizer = (width, height) =>
			{
				if (!Fullscreen)
				{
					ResizeTargets(width, height);
				}
			};

			RenderWindow.ClientResized += windowResizer;

			// Toggle Fullscreen Window mode on F11
			RenderWindow.KeyDown += (sender, e) =>
			{
				if (e.KeyCode == Keys.F11 && e.Modifiers == Keys.None && !SwapChain.IsFullScreen)
				{
					RenderWindow.ClientResized -= windowResizer;

					RenderWindow.Fullscreen = !RenderWindow.Fullscreen;
					ResizeTargets(RenderWindow.ClientSize.Width, RenderWindow.ClientSize.Height);

					if (!RenderWindow.Fullscreen)
					{
						RenderWindow.ClientResized += windowResizer;
					}
				}
			};

			// Toggle Fullscreen on Alt+Enter
			RenderWindow.KeyDown += (sender, e) =>
			{
				if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.Alt)
				{
					RenderWindow.ClientResized -= windowResizer;

					SetFullscreenState(!Fullscreen);

					if (!Fullscreen)
					{
						RenderWindow.ClientResized += windowResizer;
					}
				}
			};
		}

		private void SetFullscreenState(bool state)
		{
			if (state)
			{
				Fullscreen = true;
				ResizeTargets();
				SwapChain.IsFullScreen = true;
			}
			else
			{
				Fullscreen = false;
				SwapChain.IsFullScreen = false;
				ResizeTargets(RenderWindow.ClientSize.Width, RenderWindow.ClientSize.Height);
			}
		}

		/// <summary>
		/// Resizes the SwapChain buffers and DepthStencil buffer to the size specified by the GraphicsConfiguration.DisplayMode
		/// </summary>
		private void ResizeTargets()
		{
			ResizeTargets(Configuration.DisplayMode.Width, Configuration.DisplayMode.Height);
		}

		/// <summary>
		/// Resizes the SwapChain buffers and DepthStencil buffer to the specified size.
		/// </summary>
		private void ResizeTargets(int width, int height)
		{
			Context.OutputMerger.SetRenderTargets(null, (RenderTargetView) null);

			RenderTargetView.Dispose();
			RenderTarget.Dispose();

			DepthStencilView.Dispose();
			DepthStencilBuffer.Dispose();

			SwapChain.ResizeBuffers(Configuration.BufferCount, width, height, Configuration.DisplayMode.Format, SWAP_CHAIN_FLAGS);

			InitTargets(width, height);

			TargetsResized(width, height);
		}

		/// <summary>
		/// Creates RenderTarget and DepthStencil views of size specified in the GraphicsConfiguration and binds them to the OutputMerger.
		/// </summary>
		private void InitTargets()
		{
			InitTargets(Configuration.DisplayMode.Width, Configuration.DisplayMode.Height);
		}

		/// <summary>
		/// Creates RenderTarget and DepthStencil views of specified size and binds them to the OutputMerger.
		/// </summary>
		private void InitTargets(int width, int height)
		{
			RenderTarget = Texture2D.FromSwapChain<Texture2D>(SwapChain, 0);
			RenderTargetView = new RenderTargetView(Device, RenderTarget);

			DepthStencilBuffer = new Texture2D(
				Device,
				new Texture2DDescription
				{
					ArraySize = 1,
					BindFlags = BindFlags.DepthStencil,
					CpuAccessFlags = CpuAccessFlags.None,
					Format = DEPTH_STENCIL_FORMAT,
					MipLevels = 1,
					OptionFlags = ResourceOptionFlags.None,
					SampleDescription = Configuration.SampleDescription,
					Usage = ResourceUsage.Default,
					Width = width,
					Height = height
				}
			);

			DepthStencilView = new DepthStencilView(Device, DepthStencilBuffer);

			Context.OutputMerger.SetRenderTargets(DepthStencilView, RenderTargetView);

			DefaultViewport = new ViewportF(0f, 0f, width, height);

			Context.Rasterizer.SetViewport(DefaultViewport);
		}

		private void RenderCallback()
		{
			double delta = Clock.Delta();

			UpdateFrameStatistics(delta);

			UpdateScene(delta);

			if (Fullscreen || (RenderWindow.WindowState != FormWindowState.Minimized && RenderWindow.ClientSize.Width > 0 && RenderWindow.ClientSize.Height > 0))
			{
				RenderScene();
			}

			SwapChain.Present(Configuration.SyncInterval, PresentFlags.None);
		}

		public void Run()
		{
			RenderWindow.Fullscreen = Configuration.FullscreenWindow;
			SetFullscreenState(Fullscreen);

			Clock.Start();

			Application.Run(RenderWindow);
		}

		private void UpdateFrameStatistics(double delta)
		{
			++FrameCount;
			ElapsedTime += delta;

			if (ElapsedTime >= 0.5)
			{
				double fps = FrameCount / ElapsedTime;
				double frameTime = 1000 / fps;

				RenderWindow.Text = String.Format(
					"{0}  -  FPS: {1} Frame time: {2} ms",
					ApplicationTitle,
					fps.ToString("F0").PadRight(10, ' '),
					frameTime.ToString("F4").PadRight(10, ' ')
				);

				FrameCount = 0;
				ElapsedTime = 0;
			}
		}

		private FieldInfo[] GetDisposableFields(Type rootType)
		{
			List<FieldInfo> fieldList = new List<FieldInfo>();

			foreach (FieldInfo field in rootType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
			{
				if ((typeof(IDisposable)).IsAssignableFrom(field.FieldType))
				{
					fieldList.Add(field);
				}
			}

			if (rootType.BaseType != typeof(System.Object))
			{
				fieldList.AddRange(GetDisposableFields(rootType.BaseType));
			}

			return fieldList.ToArray();
		}

		private void DisposeResources()
		{
			Log.Debug("DisposeResources started");

			int disposed = 0;

			foreach (FieldInfo field in GetDisposableFields(this.GetType()))
			{
				IDisposable resource = (IDisposable) field.GetValue(this);

				if (resource != null)
				{
					resource.Dispose();
					disposed++;
				}
			}

			Log.Debug("DisposeResources disposed " + disposed + " resources");
		}

		/// <summary>
		/// Override with custom scene update logic.
		/// </summary>
		/// <param name="delta">
		/// Amount of time since last update, in seconds.
		/// </param>
		protected virtual void UpdateScene(double delta)
		{
		}

		/// <summary>
		/// Override with custom rendering logic.
		/// </summary>
		protected virtual void RenderScene()
		{
			Context.ClearRenderTargetView(RenderTargetView, new Color4(0.4f, 0.5f, 0.7f, 1.0f));
			Context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
		}
	}
}
