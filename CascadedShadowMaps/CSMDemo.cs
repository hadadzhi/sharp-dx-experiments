using SharpDXCommons;
using SharpDXCommons.Cameras;
using System;
using SharpDX;
using SharpDX.Direct3D11;

using Buffer = SharpDX.Direct3D11.Buffer;

using System.Runtime.InteropServices;
using System.Collections.Generic;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System.Windows.Forms;
using SharpDXCommons.Helpers;

using SharpSMAA;

namespace CascadedShadowMaps
{
	struct OverlayVertex
	{
		public Vector3 pos;
		public Vector3 tex;
	}

	class CSMDemo : SharpDXApplication
	{
		#region Fields
		private Matrix Proj;
		private Matrix CurrentViewProj;

		private Matrix LightView;
		private Matrix LightProj;
		private Matrix LightViewProj;

		private SimpleFirstPersonCamera Camera;
		private OrbitalControls LightControls;

		private Buffer ConstantBufferPerObject;
		private Buffer ConstantBufferPerFrame;
		private Buffer ConstantBufferConstants;

		private CBPerFrame CBPerFrame;
		private CBConstants CBConstants;

		private int FloorBaseVertex;
		private int FloorBaseIndex;
		private int FloorIndexCount;
		private Matrix FloorWorld;
		private Material FloorMaterial;
		private ShaderResourceView FloorTexture;

		private int CylBaseVertex;
		private int CylBaseIndex;
		private int CylIndexCount;
		private Matrix[][] CylWorld;
		private Material CylMaterial;
		private ShaderResourceView CylTexture;

		private int SkullBaseVertex;
		private int SkullBaseIndex;
		private int SkullIndexCount;
		private Matrix SkullWorld;
		private Material SkullMaterial;

		private int SphereBaseVertex;
		private int SphereBaseIndex;
		private int SphereIndexCount;
		private Matrix[][] SphereWorld;
		private Material SphereMaterial;
		private ShaderResourceView SphereTexture;

		private int BoxBaseVertex;
		private int BoxBaseIndex;
		private int BoxIndexCount;
		private Matrix[][] BoxWorld;
		private Material BoxMaterial;
		private ShaderResourceView BoxTexture;

		private InputLayout MainInputLayout;
		private Buffer MainIB;
		private Buffer MainVB;
		private VertexBufferBinding MainVBB;

		private DirectionalLight DirLight;

		private PixelShader BasicPS;
		private VertexShader BasicVS;
		private VertexShader ShadowMapVS;
		private GeometryShader CloningGS;
		private GeometryShader InstancingGS;

		private ShadowMap CSMShadowMapArray;
		private ShadowMap FullSceneShadowMap;

		private Matrix[] CSMCropMatrices;
		private CSMCameraFrustum SplitFrustum;
		private float AspectRatio;
		#endregion

		#region Parameters
		private int CSMNumSplits = CSMMaxSplits;
		private float CSMLambda = 0.9f;

		private bool UseInstancig = false;
		private bool UseFrontCulling = false;

		private float CameraVerticalFOV = MathUtil.Pi / 3.0f;
		private float CameraNear = 1e-3f; // 1mm
		private float CameraFar = 30 * 1.5e11f; // 30 AU -- Diameter of the Solar System

		private float LightNear = 0.0f;
		private float LightFar = SceneDiameter;

		private InputLayout OverlayInputLayout;
		private Buffer OverlayIB;
		private Buffer OverlayVB;
		private VertexBufferBinding OverlayVBB;
		private VertexShader OverlayVS;
		private PixelShader OverlayPS;
		#endregion

		#region Constants
		private const float Scale = 1f;

		private const int ColumnsX = 16;
		private const int ColumnsZ = 16;

		private const float SpacingX = 32.0f * Scale;
		private const float SpacingZ = 32.0f * Scale;

		private static readonly float SceneDiameter = (float) Math.Sqrt(Math.Pow(ColumnsX * SpacingX, 2.0) + Math.Pow(ColumnsZ * SpacingZ, 2.0)) + 1.0f;

		internal const float CSMShadowMapSize = 4096;
		internal const float FullSceneShadowMapSize = 16384;

		private const float GeometryDetail = 1.0f;

		private const int CSMMaxSplits = 9; // Maximum splits defined in Common.hlsl

		private float ShadowsDistance = SceneDiameter;
		#endregion

		#region SMAA Integration
		private SMAA SMAA;
		private ScreenTriangle Tri;
		private SMAARenderTarget SMAATarget;
		private Image SMAADepth;
		private Image SMAAVelocity;

		private enum ViewModes { Color, Depth, Edges, BlendWeights }

		private bool SMAAEnabled = true;
		private ViewModes ViewMode = ViewModes.Color;

		private SMAA.Inputs SMAAInput = SMAA.Inputs.Color;
		private SMAA.Modes SMAAMode = SMAA.Modes.SMAA_T2x;
		private SMAA.Presets SMAAPreset = SMAA.Presets.Ultra;

		private bool SMAAPredication = false;
		private bool SMAAReprojection = true;
		private Matrix PreviousViewProj;
		private bool RenderOverlay;
		#endregion

		#region HDR
		private Image HDRTarget;
		private Image HDRTargetDepth;
		private bool HDREnabled = false;
		private VertexShader ToneMappingVS;
		private PixelShader ToneMappingPS;
		private Buffer ExposureConstBuffer;
		private float Exposure = 1.0f;
		private float LightIntensity = 1.0f;
		#endregion

		public CSMDemo(GraphicsConfiguration conf)
			: base(conf, "Cascaded Shadow Maps Demo")
		{
			CBConstants = new CBConstants
			{
				ShadowMapSize = FullSceneShadowMapSize,
				FogColor = BackgroundColor,
				FogStartDist = (CameraFar - CameraNear) * 0.75f,
				FullFogDist = CameraFar,
				UseFullSceneShadowMap = false,
				HighlightCascades = false
			};

			#region Keys
			RenderWindow.KeyDown += (sender, e) =>
			{
				switch (e.KeyCode)
				{
					case Keys.Y:
					{
						HDREnabled = !HDREnabled;
						Log.Info(String.Format("HDR: {0}", HDREnabled));
						break;
					}
					case Keys.PageUp:
					{
						Exposure *= 2;
						Exposure = MathUtil.Clamp(Exposure, 1.0f / 1024, 1024);
						Log.Info(String.Format("Exposure: {0}", Exposure));
						break;
					}
					case Keys.PageDown:
					{
						Exposure /= 2;
						Exposure = MathUtil.Clamp(Exposure, 1.0f / 1024, 1024);
						Log.Info(String.Format("Exposure: {0}", Exposure));
						break;
					}
					case Keys.Back:
					{
						RenderOverlay = !RenderOverlay;
						break;
					}
					case Keys.NumPad0:
					{
						CBConstants.UseFullSceneShadowMap = true;
						CBConstants.ShadowMapSize = FullSceneShadowMapSize;
						CSMNumSplits = 0;

						Log.Info(String.Format("Using {0}*{0} full-scene shadow map", FullSceneShadowMapSize));

						break;
					}
					case Keys.NumPad1:
					{
						CBConstants.UseFullSceneShadowMap = false;
						CBConstants.ShadowMapSize = CSMShadowMapSize;
						CSMNumSplits = 1;

						Log.Info(String.Format("Using {1} shadow map cascades, {0}*{0} each", CSMShadowMapSize, CSMNumSplits));

						break;
					}
					case Keys.NumPad2:
					{
						CBConstants.UseFullSceneShadowMap = false;
						CBConstants.ShadowMapSize = CSMShadowMapSize;
						CSMNumSplits = 2;

						Log.Info(String.Format("Using {1} shadow map cascades, {0}*{0} each", CSMShadowMapSize, CSMNumSplits));

						break;
					}
					case Keys.NumPad3:
					{
						CBConstants.UseFullSceneShadowMap = false;
						CBConstants.ShadowMapSize = CSMShadowMapSize;
						CSMNumSplits = 3;

						Log.Info(String.Format("Using {1} shadow map cascades, {0}*{0} each", CSMShadowMapSize, CSMNumSplits));

						break;
					}
					case Keys.NumPad4:
					{
						CBConstants.UseFullSceneShadowMap = false;
						CBConstants.ShadowMapSize = CSMShadowMapSize;
						CSMNumSplits = 4;

						Log.Info(String.Format("Using {1} shadow map cascades, {0}*{0} each", CSMShadowMapSize, CSMNumSplits));

						break;
					}
					case Keys.NumPad5:
					{
						CBConstants.UseFullSceneShadowMap = false;
						CBConstants.ShadowMapSize = CSMShadowMapSize;
						CSMNumSplits = 5;

						Log.Info(String.Format("Using {1} shadow map cascades, {0}*{0} each", CSMShadowMapSize, CSMNumSplits));

						break;
					}
					case Keys.NumPad6:
					{
						CBConstants.UseFullSceneShadowMap = false;
						CBConstants.ShadowMapSize = CSMShadowMapSize;
						CSMNumSplits = 6;

						Log.Info(String.Format("Using {1} shadow map cascades, {0}*{0} each", CSMShadowMapSize, CSMNumSplits));

						break;
					}
					case Keys.NumPad7:
					{
						CBConstants.UseFullSceneShadowMap = false;
						CBConstants.ShadowMapSize = CSMShadowMapSize;
						CSMNumSplits = 7;

						Log.Info(String.Format("Using {1} shadow map cascades, {0}*{0} each", CSMShadowMapSize, CSMNumSplits));

						break;
					}
					case Keys.NumPad8:
					{
						CBConstants.UseFullSceneShadowMap = false;
						CBConstants.ShadowMapSize = CSMShadowMapSize;
						CSMNumSplits = 8;

						Log.Info(String.Format("Using {1} shadow map cascades, {0}*{0} each", CSMShadowMapSize, CSMNumSplits));

						break;
					}
					case Keys.NumPad9:
					{
						CBConstants.UseFullSceneShadowMap = false;
						CBConstants.ShadowMapSize = CSMShadowMapSize;
						CSMNumSplits = 9;

						Log.Info(String.Format("Using {1} shadow map cascades, {0}*{0} each", CSMShadowMapSize, CSMNumSplits));

						break;
					}
					case Keys.I:
					{
						UseInstancig = !UseInstancig;

						Log.Info(String.Format("Using {0} for cascades rendering", UseInstancig ? "instancing" : "cloning"));

						break;
					}
					case Keys.H:
					{
						CBConstants.HighlightCascades = !CBConstants.HighlightCascades;
						break;
					}
					case Keys.Add:
					{
						CSMLambda += 0.005f;
						CSMLambda = MathUtil.Clamp(CSMLambda, 0.0f, 1.0f);

						Log.Info(String.Format("CSM Lambda == {0}", CSMLambda));

						break;
					}
					case Keys.Subtract:
					{
						CSMLambda -= 0.005f;
						CSMLambda = MathUtil.Clamp(CSMLambda, 0.0f, 1.0f);

						Log.Info(String.Format("CSM Lambda == {0}", CSMLambda));

						break;
					}
					case Keys.B:
					{
						UseFrontCulling = !UseFrontCulling;

						Log.Info(String.Format("Front culling in shadow maps: {0}", UseFrontCulling));

						break;
					}
					case Keys.M:
					{
						SMAAEnabled = !SMAAEnabled;

						Log.Info(String.Format("SMAAEnabled: {0}", SMAAEnabled));

						break;
					}
					case Keys.Z:
					{
						ViewMode = ViewModes.Color;
						break;
					}
					case Keys.X:
					{
						ViewMode = ViewModes.Depth;
						break;
					}
					case Keys.C:
					{
						ViewMode = ViewModes.Edges;
						break;
					}
					case Keys.V:
					{
						ViewMode = ViewModes.BlendWeights;
						break;
					}
					case Keys.F1:
					{
						SMAAMode = SMAA.Modes.SMAA_1x;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.F2:
					{
						SMAAMode = SMAA.Modes.SMAA_T2x;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.F3:
					{
						SMAAMode = SMAA.Modes.SMAA_S2x;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.F4:
					{
						SMAAMode = SMAA.Modes.SMAA_4x;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.F5:
					{
						SMAAInput = SMAA.Inputs.Depth;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.F6:
					{
						SMAAInput = SMAA.Inputs.Luma;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.F7:
					{
						SMAAInput = SMAA.Inputs.Color;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.D1:
					{
						SMAAPreset = SMAA.Presets.Low;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.D2:
					{
						SMAAPreset = SMAA.Presets.Medium;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.D3:
					{
						SMAAPreset = SMAA.Presets.High;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.D4:
					{
						SMAAPreset = SMAA.Presets.Ultra;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.R:
					{
						SMAAReprojection = !SMAAReprojection;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
					case Keys.Space:
					{
						SMAAPredication = !SMAAPredication;
						UpdateSMAA(SMAATarget.Width, SMAATarget.Height);
						break;
					}
				}

				Context.UpdateSubresource(ref CBConstants, ConstantBufferConstants);
			};
			#endregion

			Camera = new SimpleFirstPersonCamera(new Vector3(0.0f, 1.5f, -2.0f), 5, 50);
			LightControls = new OrbitalControls(RenderWindow, Vector3.One, SceneDiameter / 2.0f, SceneDiameter / 2.0f, SceneDiameter / 2.0f, true);

			Camera.InstallControls(RenderWindow);
			LightControls.Install();

			TargetsResized += UpdateProjection;
			TargetsResized += UpdateSMAA;
			TargetsResized += UpdateHDRTarget;

			InitD3DResources();
			InitGeometryBuffers();
			InitWorldMatrices();
			InitMaterials();
			InitOverlayBuffers();

			DirLight = new DirectionalLight
			{
				Ambient = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) * LightIntensity / 100,
				Diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) * LightIntensity,
				Specular = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) * LightIntensity,
				Direction = LightControls.GetCameraDirection()
			};

			CSMShadowMapArray = new ShadowMap(Device, CSMShadowMapSize, CSMShadowMapSize, CSMMaxSplits);
			FullSceneShadowMap = new ShadowMap(Device, FullSceneShadowMapSize, FullSceneShadowMapSize, 1);

			SplitFrustum = new CSMCameraFrustum();

			Tri = new ScreenTriangle(Device);
		}

		private void UpdateHDRTarget(int newWidth, int newHeight)
		{
			if (HDRTarget != null) { HDRTarget.Dispose(); }
			if (HDRTargetDepth != null) { HDRTargetDepth.Dispose(); }
			HDRTarget = new Image(Device, newWidth, newHeight, Format.R11G11B10_Float, Format.R11G11B10_Float, Format.R11G11B10_Float, Format.Unknown);
			HDRTargetDepth = new Image(Device, newWidth, newHeight, Format.R24G8_Typeless, Format.Unknown, Format.Unknown, Format.D24_UNorm_S8_UInt);
		}

		private void UpdateSMAA(int newWidth, int newHeight)
		{
			if (SMAA != null) SMAA.Dispose();
			if (SMAATarget != null) SMAATarget.Dispose();
			if (SMAADepth != null) SMAADepth.Dispose();
			if (SMAAVelocity != null) SMAAVelocity.Dispose();

			SampleDescription sampleDesc = new SampleDescription
			{
				Count = SMAAMode == SMAA.Modes.SMAA_S2x || SMAAMode == SMAA.Modes.SMAA_4x ? 2 : 1,
				Quality = SMAAMode == SMAA.Modes.SMAA_S2x || SMAAMode == SMAA.Modes.SMAA_4x ? (int) StandardMultisampleQualityLevels.StandardMultisamplePattern : 0,
			};

			SMAA = new SMAA(Device, newWidth, newHeight, SMAAMode, SMAAPreset, SMAAInput, SMAAPredication, SMAAReprojection);
			SMAATarget = new SMAARenderTarget(Device, newWidth, newHeight, sampleDesc.Count == 2);
			SMAADepth = new Image(Device, newWidth, newHeight, Format.R24G8_Typeless, Format.R24_UNorm_X8_Typeless, Format.Unknown, Format.D24_UNorm_S8_UInt, sampleDesc);
			SMAAVelocity = new Image(Device, newWidth, newHeight, Format.R16G16_Float, Format.R16G16_Float, Format.R16G16_Float, Format.Unknown, sampleDesc);
		}

		private void UpdateProjection(int newTargetWidth, int newTargetHeight)
		{
			AspectRatio = (float) newTargetWidth / newTargetHeight;
			PerspectiveFovLH(CameraVerticalFOV, AspectRatio, CameraNear, CameraFar, out Proj);
		}

		private void InitD3DResources()
		{
			ShaderBytecode vsbytecode = ShaderBytecode.FromFile("BasicVS.shd");
			ShaderBytecode ovsbytecode = ShaderBytecode.FromFile("OverlayVS.shd");

			BasicVS = new VertexShader(Device, vsbytecode);
			OverlayVS = new VertexShader(Device, ovsbytecode);
			BasicPS = new PixelShader(Device, ShaderBytecode.FromFile("BasicPS.shd"));
			OverlayPS = new PixelShader(Device, ShaderBytecode.FromFile("OverlayPS.shd"));
			ShadowMapVS = new VertexShader(Device, ShaderBytecode.FromFile("ShadowMapVS.shd"));
			CloningGS = new GeometryShader(Device, ShaderBytecode.FromFile("CloningGS.shd"));
			InstancingGS = new GeometryShader(Device, ShaderBytecode.FromFile("InstancingGS.shd"));
			ToneMappingVS = new VertexShader(Device, ShaderBytecode.FromFile("ToneMappingVS.shd"));
			ToneMappingPS = new PixelShader(Device, ShaderBytecode.FromFile("ToneMappingPS.shd"));

			MainInputLayout = new InputLayout(
				Device,
				vsbytecode,
				Vertex.GetInputElements()
			);

			OverlayInputLayout = new InputLayout(
				Device,
				ovsbytecode,
				new InputElement[]
				{
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32B32_Float, 0)
				}
			);

			vsbytecode.Dispose();

			int cbPerFrameSize = Marshal.SizeOf(typeof(CBPerFrame)) + (Marshal.SizeOf(typeof(Matrix)) + Marshal.SizeOf(typeof(Vector4))) * CSMMaxSplits;

			ConstantBufferPerFrame = new Buffer(
				Device,
				cbPerFrameSize,
				ResourceUsage.Dynamic,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.Write,
				ResourceOptionFlags.None,
				0
			);

			ConstantBufferPerObject = new Buffer(
				Device,
				Marshal.SizeOf(typeof(CBPerObject)),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			ConstantBufferConstants = new Buffer(
				Device,
				Marshal.SizeOf(typeof(CBConstants)),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			ExposureConstBuffer = new Buffer(
				Device,
				16,
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			OverlayVB = new Buffer(
				Device,
				(CSMMaxSplits * 6) * 6 * 4,
				ResourceUsage.Default,
				BindFlags.VertexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			OverlayIB = new Buffer(
				Device,
				(CSMMaxSplits * 6) * Marshal.SizeOf(typeof(uint)),
				ResourceUsage.Default,
				BindFlags.IndexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			OverlayVBB = new VertexBufferBinding(OverlayVB, 6 * 4, 0);

			Context.UpdateSubresource(ref CBConstants, ConstantBufferConstants);

			ImageLoadInformation loadInfo = new ImageLoadInformation
			{
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				Filter = FilterFlags.SRgbIn | FilterFlags.Point,
				FirstMipLevel = 0,
				Format = Format.R8G8B8A8_UNorm_SRgb,
				OptionFlags = ResourceOptionFlags.None
			};

			CylTexture = ShaderResourceView.FromFile(Device, "Textures/wood.jpg", loadInfo);
			FloorTexture = ShaderResourceView.FromFile(Device, "Textures/floor.dds", loadInfo);
			SphereTexture = ShaderResourceView.FromFile(Device, "Textures/sphere.jpg", loadInfo);
			BoxTexture = ShaderResourceView.FromFile(Device, "Textures/wood.jpg", loadInfo);
		}

		private void InitGeometryBuffers()
		{
			MeshData floor = GeometryGenerator.CreateBox(SpacingX * ColumnsX, 0.1f, SpacingZ * ColumnsZ);//GeometryGenerator.CreateGrid(SpacingX * ColumnsX, SpacingZ * ColumnsZ, ColumnsX, ColumnsZ);
			MeshData cyl = GeometryGenerator.CreateCylinder(0.0f, 0.0f, 0.0f, (int) (16 * GeometryDetail), 1);
			MeshData sphere = GeometryGenerator.CreateSphere(0.0f, (int) (16 * GeometryDetail), (int) (16 * GeometryDetail));
			MeshData skull = GeometryGenerator.LoadModel("Models/skull.txt");
			MeshData box = GeometryGenerator.CreateBox(10, 20, 10);

			FloorBaseVertex = 0;
			CylBaseVertex = floor.Vertices.Length;
			SphereBaseVertex = CylBaseVertex + cyl.Vertices.Length;
			SkullBaseVertex = SphereBaseVertex + sphere.Vertices.Length;
			BoxBaseVertex = SkullBaseVertex + skull.Vertices.Length;

			FloorIndexCount = floor.Indices.Length;
			CylIndexCount = cyl.Indices.Length;
			SphereIndexCount = sphere.Indices.Length;
			SkullIndexCount = skull.Indices.Length;
			BoxIndexCount = box.Indices.Length;

			FloorBaseIndex = 0;
			CylBaseIndex = FloorIndexCount;
			SphereBaseIndex = CylBaseIndex + CylIndexCount;
			SkullBaseIndex = SphereBaseIndex + SphereIndexCount;
			BoxBaseIndex = SkullBaseIndex + SkullIndexCount;

			int totalVertices = floor.Vertices.Length + cyl.Vertices.Length + sphere.Vertices.Length + skull.Vertices.Length + box.Vertices.Length;
			int totalIndices = FloorIndexCount + CylIndexCount + SphereIndexCount + SkullIndexCount + BoxIndexCount;

			MainVB = new Buffer(
				Device,
				totalVertices * Marshal.SizeOf(typeof(Vertex)),
				ResourceUsage.Default,
				BindFlags.VertexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			List<Vertex> vertexList = new List<Vertex>();

			vertexList.AddRange(floor.Vertices);
			vertexList.AddRange(cyl.Vertices);
			vertexList.AddRange(sphere.Vertices);
			vertexList.AddRange(skull.Vertices);
			vertexList.AddRange(box.Vertices);

			Context.UpdateSubresource(vertexList.ToArray(), MainVB);

			MainIB = new Buffer(
				Device,
				totalIndices * Marshal.SizeOf(typeof(uint)),
				ResourceUsage.Default,
				BindFlags.IndexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			List<uint> indexList = new List<uint>();

			indexList.AddRange(floor.Indices);
			indexList.AddRange(cyl.Indices);
			indexList.AddRange(sphere.Indices);
			indexList.AddRange(skull.Indices);
			indexList.AddRange(box.Indices);

			Context.UpdateSubresource(indexList.ToArray(), MainIB);

			MainVBB = new VertexBufferBinding(MainVB, Marshal.SizeOf(typeof(Vertex)), 0);

			Log.Info(String.Format("Total vertices: {0}", (box.Vertices.Length + cyl.Vertices.Length + sphere.Vertices.Length) * ColumnsX * ColumnsZ + floor.Vertices.Length + skull.Vertices.Length));
			Log.Info(String.Format("Total triangles: {0}", (box.Indices.Length + cyl.Indices.Length + sphere.Indices.Length) / 3 * ColumnsX * ColumnsZ + (floor.Indices.Length + skull.Indices.Length) / 3));
		}

		private void InitOverlayBuffers()
		{
			float z = CameraNear / 2.0f;
			float a = 2.0f / CSMMaxSplits;

			List<OverlayVertex> vertexList = new List<OverlayVertex>();
			List<uint> indexList = new List<uint>();

			for (uint i = 0; i < CSMMaxSplits; i++)
			{
				vertexList.Add(
					new OverlayVertex
					{
						pos = new Vector3(i * a - 1, 1.0f, z),
						tex = new Vector3(0, 0, i)
					}
				);

				vertexList.Add(
					new OverlayVertex
					{
						pos = new Vector3(i * a - 1, 1.0f - a, z),
						tex = new Vector3(0, 1, i)
					}
				);

				vertexList.Add(
					new OverlayVertex
					{
						pos = new Vector3(i * a + a - 1, 1.0f, z),
						tex = new Vector3(1, 0, i)
					}
				);

				vertexList.Add(
					new OverlayVertex
					{
						pos = new Vector3(i * a + a - 1, 1.0f - a, z),
						tex = new Vector3(1, 1, i)
					}
				);

				indexList.Add(i * 4);
				indexList.Add(i * 4 + 2);
				indexList.Add(i * 4 + 1);
				indexList.Add(i * 4 + 1);
				indexList.Add(i * 4 + 2);
				indexList.Add(i * 4 + 3);
			}

			OverlayVertex[] vertexArray = vertexList.ToArray();
			uint[] indexArray = indexList.ToArray();

			Context.UpdateSubresource(vertexArray, OverlayVB);
			Context.UpdateSubresource(indexArray, OverlayIB);
		}

		private void InitWorldMatrices()
		{
			FloorWorld = Scale < 100 ? Matrix.Identity : Matrix.Zero;
			SkullWorld = Matrix.Multiply(Matrix.Scaling(0.5f, 0.5f, 0.5f), Matrix.Translation(0.0f, 5.0f, 0.0f));

			CylWorld = new Matrix[ColumnsX][];
			SphereWorld = new Matrix[ColumnsX][];
			BoxWorld = new Matrix[ColumnsX][];

			for (int i = 0; i < ColumnsX; i++)
			{
				CylWorld[i] = new Matrix[ColumnsZ];
				SphereWorld[i] = new Matrix[ColumnsZ];
				BoxWorld[i] = new Matrix[ColumnsZ];

				for (int j = 0; j < ColumnsZ; j++)
				{
					CylWorld[i][j] = Matrix.Scaling(Scale) * Matrix.Translation(i * SpacingX - (ColumnsX - 1) * SpacingX / 2.0f, 8.0f * Scale, j * SpacingZ - (ColumnsZ - 1) * SpacingZ / 2.0f);

					SphereWorld[i][j] = Matrix.Scaling(Scale) * Matrix.Translation(i * SpacingX - (ColumnsX - 1) * SpacingX / 2.0f, 14.0f * Scale, j * SpacingZ - (ColumnsZ - 1) * SpacingZ / 2.0f);

					BoxWorld[i][j] = Matrix.Scaling(Scale) * Matrix.Translation(i * SpacingX - (ColumnsX - 1) * SpacingX / 2.0f, 10.0f * Scale, j * SpacingZ - (ColumnsZ - 1) * SpacingZ / 2.0f);
				}
			}
		}

		private void InitMaterials()
		{
			FloorMaterial = new Material
			{
				Ambient = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Specular = new Vector4(0.5f, 0.5f, 0.5f, 64.0f),
				Textured = true
			};

			BoxMaterial = new Material
			{
				Ambient = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Specular = new Vector4(1.0f, 1.0f, 1.0f, 16.0f),
				Textured = true
			};

			SphereMaterial = new Material
			{
				Ambient = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Specular = new Vector4(1.0f, 1.0f, 1.0f, 32.0f),
				Textured = true
			};

			CylMaterial = new Material
			{
				Ambient = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Specular = new Vector4(1.0f, 1.0f, 1.0f, 32.0f),
				Textured = true
			};

			SkullMaterial = new Material
			{
				Ambient = new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
				Diffuse = new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
				Specular = new Vector4(1.0f, 1.0f, 1.0f, 1000.0f),
				Textured = false
			};
		}

		private void DrawSceneGeometryInstanced(Matrix viewProj, int numInstances)
		{
			// Floor
			SetPerObjectState(FloorWorld, viewProj, FloorMaterial, FloorTexture, Matrix.Scaling(ColumnsX * 4, ColumnsZ * 4, 1));
			Context.DrawIndexedInstanced(FloorIndexCount, numInstances, FloorBaseIndex, FloorBaseVertex, 0);

			// Skull
			SetPerObjectState(SkullWorld, viewProj, SkullMaterial, null, Matrix.Identity);
			Context.DrawIndexedInstanced(SkullIndexCount, numInstances, SkullBaseIndex, SkullBaseVertex, 0);

			for (int i = 0; i < ColumnsX; i++)
			{
				for (int j = 0; j < ColumnsZ; j++)
				{
					// Cylinders
					SetPerObjectState(CylWorld[i][j], viewProj, CylMaterial, CylTexture, Matrix.Identity);
					Context.DrawIndexedInstanced(CylIndexCount, numInstances, CylBaseIndex, CylBaseVertex, 0);

					// Spheres
					SetPerObjectState(SphereWorld[i][j], viewProj, SphereMaterial, SphereTexture, Matrix.Identity);
					Context.DrawIndexedInstanced(SphereIndexCount, numInstances, SphereBaseIndex, SphereBaseVertex, 0);

					// Boxes
					SetPerObjectState(BoxWorld[i][j], viewProj, BoxMaterial, BoxTexture, Matrix.Identity);
					Context.DrawIndexedInstanced(BoxIndexCount, numInstances, BoxBaseIndex, BoxBaseVertex, 0);
				}
			}
		}

		private void RenderFullSceneShadowMap()
		{
			Context.OutputMerger.SetTargets(FullSceneShadowMap.DepthStencilView, (RenderTargetView) null);

			Context.VertexShader.Set(ShadowMapVS);
			Context.GeometryShader.Set(null);
			Context.PixelShader.Set(null);

			Context.Rasterizer.State = UseFrontCulling ? PipelineStates.Rasterizer.FrontFaceCulling : PipelineStates.Rasterizer.DepthBias;
			Context.Rasterizer.SetViewport(0.0f, 0.0f, FullSceneShadowMapSize, FullSceneShadowMapSize, 0.0f, 1.0f);

			Context.ClearDepthStencilView(FullSceneShadowMap.DepthStencilView, DepthStencilClearFlags.Depth, 1f, 0);

			DrawSceneGeometryInstanced(LightViewProj, 1);
		}

		private void RenderCascadedShadowMap()
		{
			Context.OutputMerger.SetTargets(CSMShadowMapArray.DepthStencilView, (RenderTargetView) null);

			Context.VertexShader.Set(ShadowMapVS);
			Context.PixelShader.Set(null);

			Context.Rasterizer.State = UseFrontCulling ? PipelineStates.Rasterizer.FrontFaceCulling : PipelineStates.Rasterizer.DepthBias;
			Context.Rasterizer.SetViewport(0.0f, 0.0f, CSMShadowMapSize, CSMShadowMapSize, 0.0f, 1.0f);

			Context.ClearDepthStencilView(CSMShadowMapArray.DepthStencilView, DepthStencilClearFlags.Depth, 1f, 0);

			if (UseInstancig)
			{
				Context.GeometryShader.Set(InstancingGS);
				DrawSceneGeometryInstanced(LightViewProj, CSMNumSplits);
			}
			else
			{
				Context.GeometryShader.Set(CloningGS);
				DrawSceneGeometryInstanced(LightViewProj, 1);
			}
		}

		private void RenderMainView()
		{
			Context.Rasterizer.SetViewport(DefaultViewport);
			Context.Rasterizer.State = PipelineStates.Rasterizer.Default;

			Context.OutputMerger.DepthStencilState = PipelineStates.DepthStencil.Default;

			Context.VertexShader.Set(BasicVS);
			Context.GeometryShader.Set(null);
			Context.PixelShader.Set(BasicPS);

			Context.PixelShader.SetShaderResource(1, CBConstants.UseFullSceneShadowMap ? FullSceneShadowMap.ShaderResourceView : CSMShadowMapArray.ShaderResourceView);
			Context.PixelShader.SetSampler(1, PipelineStates.Sampler.ShadowMapPCF);

			DrawSceneGeometryInstanced(CurrentViewProj, 1);
		}

		private void RenderCascadesOverlay()
		{
			Context.InputAssembler.SetIndexBuffer(OverlayIB, Format.R32_UInt, 0);
			Context.InputAssembler.SetVertexBuffers(0, OverlayVBB);
			Context.InputAssembler.InputLayout = OverlayInputLayout;
			Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			Context.OutputMerger.DepthStencilState = PipelineStates.DepthStencil.DisableDepthStencil;

			Context.VertexShader.Set(OverlayVS);
			Context.PixelShader.Set(OverlayPS);
			Context.GeometryShader.Set(null);

			Context.PixelShader.SetShaderResource(1, CBConstants.UseFullSceneShadowMap ? FullSceneShadowMap.ShaderResourceView : CSMShadowMapArray.ShaderResourceView);
			Context.PixelShader.SetSampler(0, PipelineStates.Sampler.WrappedLinear);

			Context.DrawIndexed(3 * 2 * CSMMaxSplits, 0, 0);
		}

		private void SetPerObjectState(Matrix world, Matrix viewProj, Material material, ShaderResourceView texture, Matrix textureTransform)
		{
			CBPerObject cbPerObject = new CBPerObject
			{
				World = Matrix.Transpose(world),
				WorldInvTranspose = Matrix.Invert(world),
				WorldViewProj = Matrix.Transpose(Matrix.Multiply(world, viewProj)),
				PreviousWorldViewProj = Matrix.Transpose(Matrix.Multiply(world, PreviousViewProj)),
				TextureTransform = Matrix.Transpose(textureTransform),
				Material = material
			};

			Context.UpdateSubresource(ref cbPerObject, ConstantBufferPerObject);

			if (texture != null)
			{
				Context.PixelShader.SetShaderResource(0, texture);
				Context.PixelShader.SetSampler(0, PipelineStates.Sampler.WrappedAnisotropic);
			}
		}

		protected override void UpdateScene(double delta)
		{
			Camera.Update((float) delta);

			Matrix cameraView = Camera.ViewMatrix;

			PreviousViewProj = CurrentViewProj;
			CurrentViewProj = Matrix.Multiply(cameraView, Proj);

			DirLight.Direction = LightControls.GetCameraDirection();

			LightView = Matrix.LookAtLH(LightControls.GetCameraPosition(), LightControls.GetOrigin(), Vector3.Up);
			LightProj = Matrix.OrthoLH(SceneDiameter, SceneDiameter, LightNear, LightFar);
			LightViewProj = Matrix.Multiply(LightView, LightProj);

			SplitFrustum.Build(CameraNear, ShadowsDistance, CameraVerticalFOV, AspectRatio, Camera.Position, Camera.ViewDirection, CSMNumSplits, CSMLambda);
			CSMCropMatrices = SplitFrustum.CalculateCropMatrices(LightViewProj, LightNear);

			CBPerFrame = new CBPerFrame
			{
				DirLight = DirLight,
				ViewerPosition = Camera.Position,
				LightViewProj = Matrix.Transpose(LightViewProj),
				CameraView = Matrix.Transpose(cameraView),
				NumSplits = CSMNumSplits,
				CameraFar = CameraFar,
				CameraNear = CameraNear,
				Jitter = SMAAEnabled ? SMAA.CurrentJitter : Vector2.Zero
			};

			DataStream ds;
			Context.MapSubresource(ConstantBufferPerFrame, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out ds);

			ds.Write(CBPerFrame);

			// Write crop matrices
			for (int i = 0; i < CSMMaxSplits; i++)
			{
				if (i < CSMNumSplits)
				{
					ds.Write(Matrix.Transpose(CSMCropMatrices[i]));
				}
				else
				{
					ds.Write(Matrix.Identity);
				}
			}

			// Write split positions -- do not include SplitPositions[0] == CameraNear
			for (int i = 1; i <= CSMMaxSplits; i++)
			{
				if (i <= CSMNumSplits)
				{
					ds.Write(SplitFrustum.SplitPositions[i]);
					ds.Write(Vector3.Zero);
				}
				else
				{
					ds.Write(Vector4.Zero);
				}
			}

			Context.UnmapSubresource(ConstantBufferPerFrame, 0);
			ds.Dispose();
		}

		protected override void RenderScene()
		{
			Context.ClearState();

			Context.InputAssembler.SetIndexBuffer(MainIB, Format.R32_UInt, 0);
			Context.InputAssembler.SetVertexBuffers(0, MainVBB);
			Context.InputAssembler.InputLayout = MainInputLayout;
			Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			Context.VertexShader.SetConstantBuffers(0, 3, new Buffer[] { ConstantBufferPerFrame, ConstantBufferPerObject, ConstantBufferConstants });
			Context.PixelShader.SetConstantBuffers(0, 3, new Buffer[] { ConstantBufferPerFrame, ConstantBufferPerObject, ConstantBufferConstants });
			Context.GeometryShader.SetConstantBuffers(0, 3, new Buffer[] { ConstantBufferPerFrame, ConstantBufferPerObject, ConstantBufferConstants });

			if (CBConstants.UseFullSceneShadowMap)
			{
				RenderFullSceneShadowMap();
			}
			else
			{
				RenderCascadedShadowMap();
			}

			if (SMAAEnabled) // Render to off-screen target for later antialising
			{
				Context.ClearDepthStencilView(SMAADepth.DSV, DepthStencilClearFlags.Depth, 1.0f, 0);
				Context.ClearRenderTargetView(SMAATarget.RTV, BackgroundColor);
				Context.ClearRenderTargetView(SMAAVelocity.RTV, new Color4(0));

				Context.OutputMerger.SetTargets(SMAADepth.DSV, SMAATarget.RTV, SMAAVelocity.RTV);
			}
			else // Render to main target directly
			{
				Context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
				Context.ClearRenderTargetView(BackbufferRTV, BackgroundColor);

				Context.OutputMerger.SetTargets(DepthStencilView, BackbufferRTV);
			}

			//if (HDREnabled)
			//{
			//	Context.ClearDepthStencilView(HDRTargetDepth.DSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
			//	Context.ClearRenderTargetView(HDRTarget.RTV, BackgroundColor * LightIntensity);

			//	Context.OutputMerger.SetTargets(HDRTargetDepth.DSV, HDRTarget.RTV);
			//}
			//else // Render to main target directly
			//{
			//	Context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
			//	Context.ClearRenderTargetView(BackbufferRTV, BackgroundColor * LightIntensity);

			//	Context.OutputMerger.SetTargets(DepthStencilView, BackbufferRTV);
			//}

			RenderMainView();

			//if (HDREnabled)
			//{
			//	Context.OutputMerger.SetTargets(BackbufferRTV);

			//	Context.VertexShader.Set(ToneMappingVS);
			//	Context.PixelShader.Set(ToneMappingPS);

			//	Context.PixelShader.SetShaderResource(0, HDRTarget.SRV);
			//	Context.PixelShader.SetConstantBuffer(0, ExposureConstBuffer);

			//	Context.UpdateSubresource(ref Exposure, ExposureConstBuffer);

			//	Context.Rasterizer.SetViewport(DefaultViewport);

			//	Tri.Draw(Context);
			//}

			if (SMAAEnabled)
			{
				SMAA.Run(Context, SMAATarget.SRV, SMAATarget.GammaSRV, SMAADepth.SRV, SMAAVelocity.SRV);

				Context.OutputMerger.SetTargets(BackbufferRTV);
				Context.OutputMerger.DepthStencilState = PipelineStates.DepthStencil.DisableDepthStencil;
				Context.Rasterizer.SetViewport(DefaultViewport);

				if (ViewMode == ViewModes.Color)
				{
					Tri.DrawImage(Context, SMAA.Result.SRV, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
				}
				else if (ViewMode == ViewModes.Edges)
				{
					Tri.DrawImage(Context, SMAA.Edges.SRV, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
				}
				else if (ViewMode == ViewModes.BlendWeights)
				{
					Tri.DrawImage(Context, SMAA.BlendingWeights.SRV, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
				}
				else if (ViewMode == ViewModes.Depth)
				{
					Tri.DrawDepth(Context, SMAADepth.SRV, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
				}
			}

			if (RenderOverlay)
			{
				RenderCascadesOverlay();
			}
		}

		private static void PerspectiveFovLH(float vfov, float aspect, float near, float far, out Matrix result)
		{
			float cotTheta = (float) (1f / Math.Tan(vfov * 0.5f));
			float q = far / (far - near);

			result = new Matrix(0);
			result.M11 = cotTheta / aspect;
			result.M22 = cotTheta;
			result.M33 = q;
			result.M34 = 1f;
			result.M43 = -near * q;
		}
	}
}
