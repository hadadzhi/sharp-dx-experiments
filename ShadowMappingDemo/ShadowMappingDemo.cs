using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDXCommons;
using SharpDXCommons.Cameras;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ShadowMappingDemo
{
	class ShadowMappingDemo : SharpDXApplication
	{
		#region Fields

		private Matrix Proj;
		private Matrix ViewProj;

		private OrbitalControls CameraControls;

		private Buffer ConstantBufferPerObject;
		private Buffer ConstantBufferPerFrame;
		private Buffer ConstantBufferConstants;

		private CBPerFrame CBPerFrame;
		private CBPerFrame CBPerFrameReflected;

		private int FloorVerticesOffset;
		private int FloorIndicesOffset;
		private int FloorIndexCount;
		private Matrix FloorWorld;
		private Material FloorMaterial;
		private ShaderResourceView FloorTexture;

		private int WallVerticesOffset;
		private int WallIndicesOffset;
		private int WallIndexCount;
		private Matrix WallWorld;
		private Material WallMaterial;
		private ShaderResourceView WallTexture;
		private Matrix MirrorReflectionMatrix;

		private int MirrorVerticesOffset;
		private int MirrorIndicesOffset;
		private int MirrorIndexCount;
		private Matrix MirrorWorld;
		private Material MirrorMaterial;
		private ShaderResourceView MirrorTexture;

		private int SkullVerticesOffset;
		private int SkullIndicesOffset;
		private int SkullIndexCount;
		private Matrix SkullWorld;
		private Material SkullMaterial;

		private Buffer VertexBuffer;
		private Buffer IndexBuffer;
		private VertexBufferBinding VertexBinding;

		private DirectionalLight DirLight;
		private PointLight PointLight;
		private SpotLight SpotLight;

		private DepthStencilState MarkMirrorDSS;
		private DepthStencilState DrawReflectionDSS;

		private PixelShader BasicPS;
		private VertexShader BasicVS;

		private ShadowMap DirLightSM;
		private Matrix DirLightViewProj;

		#endregion

		private const float ShadowMapSize = 4096;

		public ShadowMappingDemo(GraphicsConfiguration conf)
			: base(conf, "ShadowMappingDemo")
		{	
			CameraControls = new OrbitalControls(RenderWindow, new Vector3(0f, 1f, 0f), 1f, 50f, 5f);

			CameraControls.Install();

			InitDirect3D();

			CreateGeometryBuffers();
			CreateMatrices();
			CreateMaterials();
			CreateLights();

			TargetsResized += (int newWidth, int newHeight) =>
			{
				Proj = Matrix.PerspectiveFovLH(0.25f * (float) Math.PI, (float) newWidth / newHeight, 0.1f, 10000f);
			};

			float lastMouseX = 0.0f;
			float lastMouseY = 0.0f;

			RenderWindow.MouseMove += (object sender, MouseEventArgs e) =>
			{
				if (e.Button == MouseButtons.Left)
				{
					// Make each pixel correspond to a quarter of a degree.
					float dPhi = -0.25f * (e.X - lastMouseX) * (float) Math.PI / 180;
					float dTheta = -0.25f * (e.Y - lastMouseY) * (float) Math.PI / 180;

					DirLight.Direction = Vector3.Transform(
						DirLight.Direction,
						Matrix3x3.Multiply(
							Matrix3x3.RotationY(-dPhi),
							Matrix3x3.RotationX(-dTheta)
						)
					);
					DirLight.Direction.Normalize();
				}

				lastMouseX = e.X;
				lastMouseY = e.Y;
			};
		}

		private void InitDirect3D()
		{	
			ShaderBytecode vsbytecode = ShaderBytecode.FromFile("BasicVertex.shd");

			BasicVS = new VertexShader(Device, vsbytecode);
			BasicPS = new PixelShader(Device, ShaderBytecode.FromFile("BasicPixel.shd"));

			InputLayout layout = new InputLayout(
				Device,
				vsbytecode,
				Vertex.GetInputElements()
			);

			Context.InputAssembler.InputLayout = layout;
			Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			vsbytecode.Dispose();
			layout.Dispose();

			ConstantBufferPerFrame = new Buffer(
				Device,
				Marshal.SizeOf(typeof(CBPerFrame)),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
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
				ResourceUsage.Dynamic,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.Write,
				ResourceOptionFlags.None,
				0
			);

			DataStream cbConstDS;
			Context.MapSubresource(ConstantBufferConstants, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out cbConstDS);

			cbConstDS.Write(ShadowMapSize);

			Context.UnmapSubresource(ConstantBufferConstants, 0);
			cbConstDS.Dispose();

			Context.VertexShader.SetConstantBuffers(0, 3, new Buffer[] { ConstantBufferPerFrame, ConstantBufferPerObject, ConstantBufferConstants });
			Context.PixelShader.SetConstantBuffers(0, 3, new Buffer[] { ConstantBufferPerFrame, ConstantBufferPerObject, ConstantBufferConstants });

			FloorTexture = ShaderResourceView.FromFile(Device, "Textures/checkboard.dds");
			WallTexture = ShaderResourceView.FromFile(Device, "Textures/brick01.dds");
			MirrorTexture = ShaderResourceView.FromFile(Device, "Textures/ice.dds");

			DepthStencilOperationDescription dssOpDesc = new DepthStencilOperationDescription
			{
				Comparison = Comparison.Always,
				DepthFailOperation = StencilOperation.Keep,
				FailOperation = StencilOperation.Keep,
				PassOperation = StencilOperation.Replace
			};

			DepthStencilStateDescription dssDesc = new DepthStencilStateDescription
			{
				IsDepthEnabled = true,
				DepthWriteMask = DepthWriteMask.Zero,
				DepthComparison = Comparison.Less,
				IsStencilEnabled = true,
				StencilReadMask = 0xFF,
				StencilWriteMask = 0xFF,
				FrontFace = dssOpDesc,
				BackFace = dssOpDesc
			};

			MarkMirrorDSS = new DepthStencilState(Device, dssDesc);

			Context.OutputMerger.DepthStencilReference = 1;

			dssDesc.FrontFace.PassOperation = StencilOperation.Keep;
			dssDesc.FrontFace.Comparison = Comparison.Equal;
			dssDesc.BackFace = dssDesc.FrontFace;
			dssDesc.DepthWriteMask = DepthWriteMask.All;

			DrawReflectionDSS = new DepthStencilState(Device, dssDesc);

			DirLightSM = new ShadowMap(Device, ShadowMapSize, ShadowMapSize);
		}

		private void CreateLights()
		{
			DirLight = new DirectionalLight
			{
				Ambient = new Vector4(0.2f, 0.2f, 0.2f, 1.0f),
				Diffuse = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
				Specular = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
				Direction = Vector3.Normalize(new Vector3(0.001f, -1.0f, 0.001f))
			};

			SpotLight = new SpotLight
			{
				Ambient = new Vector4(0.0f, 0.0f, 0.1f, 1.0f),
				Diffuse = new Vector4(0.2f, 0.2f, 0.5f, 1.0f),
				Specular = new Vector4(0.2f, 0.2f, 0.5f, 1.0f),
				Att = new Vector3(0.5f, 0.0f, 0.0f),
				Spot = 100,
				Range = 10000.0f
			};

			PointLight = new PointLight
			{
				Ambient = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
				Diffuse = new Vector4(0.125f, 0.0f, 0.125f, 1.0f),
				Specular = new Vector4(0.125f, 0.0f, 0.125f, 1.0f),
				Att = new Vector3(0.0f, 0.0f, 1.0f),
				Range = 10f,
				Position = new Vector3(0, 1.5f, 1.4f)
			};
		}

		private void CreateMaterials()
		{
			FloorMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = Vector4.One,
				Specular = new Vector4(0.2f, 0.2f, 0.2f, 1.0f),
				Textured = true
			};

			WallMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = Vector4.One,
				Specular = new Vector4(0.1f, 0.1f, 0.1f, 1.0f),
				Textured = true
			};

			MirrorMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = new Vector4(Vector3.One, 0.5f),
				Specular = new Vector4(0.1f, 0.1f, 0.1f, 1000.0f),
				Textured = true
			};

			SkullMaterial = new Material
			{
				Ambient = new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
				Diffuse = new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
				Specular = new Vector4(0.8f, 0.8f, 0.8f, 1000.0f),
				Textured = false
			};
		}

		private void CreateGeometryBuffers()
		{
			MeshData floor = GeometryGenerator.CreateBox(3f, 0.01f, 3f);
			MeshData wall = GeometryGenerator.CreateBox(3f, 3f, 0.01f);
			MeshData mirror = GeometryGenerator.CreateGrid(2.25f, 2.25f, 2, 2, false);
			MeshData skull = GeometryGenerator.LoadModel("Models/skull.txt");

			FloorIndexCount = floor.Indices.Length;
			WallIndexCount = wall.Indices.Length;
			MirrorIndexCount = mirror.Indices.Length;
			SkullIndexCount = skull.Indices.Length;

			FloorIndicesOffset = 0;
			WallIndicesOffset = FloorIndicesOffset + FloorIndexCount;
			MirrorIndicesOffset = WallIndicesOffset + WallIndexCount;
			SkullIndicesOffset = MirrorIndicesOffset + MirrorIndexCount;

			FloorVerticesOffset = 0;
			WallVerticesOffset = FloorVerticesOffset + floor.Vertices.Length;
			MirrorVerticesOffset = WallVerticesOffset + wall.Vertices.Length;
			SkullVerticesOffset = MirrorVerticesOffset + mirror.Vertices.Length;

			int totalVertices =
				floor.Vertices.Length +
				wall.Vertices.Length +
				mirror.Vertices.Length +
				skull.Vertices.Length;

			int totalIndices =
				floor.Indices.Length +
				wall.Indices.Length +
				mirror.Indices.Length +
				skull.Indices.Length;

			VertexBuffer = new Buffer(
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
			vertexList.AddRange(wall.Vertices);
			vertexList.AddRange(mirror.Vertices);
			vertexList.AddRange(skull.Vertices);

			Vertex[] vertexArray = vertexList.ToArray();

			Context.UpdateSubresource(vertexArray, VertexBuffer);

			IndexBuffer = new Buffer(
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
			indexList.AddRange(wall.Indices);
			indexList.AddRange(mirror.Indices);
			indexList.AddRange(skull.Indices);

			uint[] indexArray = indexList.ToArray();

			Context.UpdateSubresource(indexArray, IndexBuffer);

			VertexBinding = new VertexBufferBinding(VertexBuffer, Marshal.SizeOf(typeof(Vertex)), 0);
		}

		private void CreateMatrices()
		{
			FloorWorld = Matrix.Identity;
			WallWorld = Matrix.Translation(0f, 1.505f, 1.505f);
			MirrorWorld = Matrix.Multiply(Matrix.RotationX((float) (-Math.PI / 2)), Matrix.Translation(0f, 1.5f, 1.49f));
			SkullWorld = Matrix.Multiply(Matrix.Scaling(0.2f), Matrix.Translation(0f, 1f, 0f));

			Plane mirrorPlane = new Plane(new Vector3(0f, 1.5f, 1.5f), Vector3.UnitZ);
			MirrorReflectionMatrix = Matrix.Reflection(mirrorPlane);
		}

		private void SetPerObjectState(Matrix world, Matrix viewProj, Material material, ShaderResourceView texture, Matrix textureTransform)
		{
			CBPerObject cbPerObject = new CBPerObject
			{
				World = Matrix.Transpose(world),
				WorldInvTranspose = Matrix.Invert(world),
				WorldViewProj = Matrix.Transpose(Matrix.Multiply(world, viewProj)),
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
			ViewProj = Matrix.Multiply(CameraControls.GetViewMatrix(), Proj);

			Matrix lightView = Matrix.LookAtLH(Vector3.Negate(DirLight.Direction), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f));
			Matrix lightProj = Matrix.OrthoLH(8, 8, -5, 5);

			DirLightViewProj = Matrix.Multiply(lightView, lightProj);

			PointLight.Position = Vector3.Transform(PointLight.Position, Matrix3x3.RotationY((float) (-Math.PI * delta / 4)));

			Vector3 cameraPosition = CameraControls.GetCameraPosition();

			SpotLight.Position = cameraPosition;
			SpotLight.Direction = Vector3.Normalize(Vector3.Subtract(CameraControls.GetOrigin(), cameraPosition));

			CBPerFrame = new CBPerFrame
			{
				DirLight = DirLight,
				PointLight = PointLight,
				SpotLight = SpotLight,
				ViewerPosition = cameraPosition,
				LightViewProj = Matrix.Transpose(DirLightViewProj)
			};

			DirectionalLight dirLightReflected = DirLight;
			PointLight pointLightReflected = PointLight;
			SpotLight spotLightReflected = SpotLight;
			Vector3 cameraPositionReflected = cameraPosition;
			Vector3 cameraOriginReflected = CameraControls.GetOrigin();

			Vector3.TransformCoordinate(ref PointLight.Position, ref MirrorReflectionMatrix, out pointLightReflected.Position);
			Vector3.TransformCoordinate(ref SpotLight.Position, ref MirrorReflectionMatrix, out spotLightReflected.Position);
			Vector3.TransformCoordinate(ref cameraPosition, ref MirrorReflectionMatrix, out cameraPositionReflected);
			Vector3.TransformCoordinate(ref cameraOriginReflected, ref MirrorReflectionMatrix, out cameraOriginReflected);
			Vector3.TransformNormal(ref DirLight.Direction, ref MirrorReflectionMatrix, out dirLightReflected.Direction);

			spotLightReflected.Direction = Vector3.Normalize(Vector3.Subtract(cameraOriginReflected, cameraPositionReflected));

			CBPerFrameReflected = new CBPerFrame
			{
				DirLight = dirLightReflected,
				PointLight = pointLightReflected,
				SpotLight = spotLightReflected,
				ViewerPosition = cameraPosition
			};
		}

		protected override void RenderScene()
		{
			base.RenderScene();

			Context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
			Context.InputAssembler.SetVertexBuffers(0, VertexBinding);

			Context.UpdateSubresource(ref CBPerFrame, ConstantBufferPerFrame);

			RenderToShadowMap(DirLightSM, DirLightViewProj);

			Context.OutputMerger.SetTargets(DepthStencilView, BackbufferRTV);
			Context.Rasterizer.SetViewport(DefaultViewport);

			Context.PixelShader.Set(BasicPS);
			Context.VertexShader.Set(BasicVS);

			Context.PixelShader.SetShaderResource(1, DirLightSM.ShaderResourceView);
			Context.PixelShader.SetSampler(1, PipelineStates.Sampler.ShadowMapPCF);

			// Draw everything but the mirror and the wall as normal
			SetPerObjectState(FloorWorld, ViewProj, FloorMaterial, FloorTexture, Matrix.Scaling(4f));
			Context.DrawIndexed(FloorIndexCount, FloorIndicesOffset, FloorVerticesOffset);

			SetPerObjectState(SkullWorld, ViewProj, SkullMaterial, null, Matrix.Identity);
			Context.DrawIndexed(SkullIndexCount, SkullIndicesOffset, SkullVerticesOffset);

			// Draw the mirror to the stencil buffer only
			Context.OutputMerger.BlendState = PipelineStates.Blend.DisableRTWrites;
			Context.OutputMerger.DepthStencilState = MarkMirrorDSS;

			SetPerObjectState(MirrorWorld, ViewProj, MirrorMaterial, MirrorTexture, Matrix.Identity);
			Context.DrawIndexed(MirrorIndexCount, MirrorIndicesOffset, MirrorVerticesOffset);

			Context.OutputMerger.BlendState = PipelineStates.Blend.Default;
			Context.OutputMerger.DepthStencilState = PipelineStates.DepthStencil.Default;

			// Draw the reflection with stenciling
			Context.UpdateSubresource(ref CBPerFrameReflected, ConstantBufferPerFrame);

			Context.Rasterizer.State = PipelineStates.Rasterizer.InverseWindingRule;
			Context.OutputMerger.DepthStencilState = DrawReflectionDSS;

			SetPerObjectState(Matrix.Multiply(SkullWorld, MirrorReflectionMatrix), ViewProj, SkullMaterial, null, Matrix.Identity);
			Context.DrawIndexed(SkullIndexCount, SkullIndicesOffset, SkullVerticesOffset);

			SetPerObjectState(Matrix.Multiply(FloorWorld, MirrorReflectionMatrix), ViewProj, FloorMaterial, FloorTexture, Matrix.Scaling(4f));
			Context.DrawIndexed(FloorIndexCount, FloorIndicesOffset, FloorVerticesOffset);

			Context.OutputMerger.DepthStencilState = PipelineStates.DepthStencil.Default;
			Context.Rasterizer.State = PipelineStates.Rasterizer.Default;
			Context.UpdateSubresource(ref CBPerFrame, ConstantBufferPerFrame);

			// Draw the mirror with transparency and depth writes, so it occludes the wall
			Context.OutputMerger.BlendState = PipelineStates.Blend.AlphaBlend;

			SetPerObjectState(MirrorWorld, ViewProj, MirrorMaterial, MirrorTexture, Matrix.Identity);
			Context.DrawIndexed(MirrorIndexCount, MirrorIndicesOffset, MirrorVerticesOffset);

			Context.OutputMerger.BlendState = PipelineStates.Blend.Default;

			// Draw the wall last, so it does not occlude the reflection
			SetPerObjectState(WallWorld, ViewProj, WallMaterial, WallTexture, Matrix.Scaling(4f));
			Context.DrawIndexed(WallIndexCount, WallIndicesOffset, WallVerticesOffset);
		}

		private void RenderToShadowMap(ShadowMap shadowMap, Matrix lightViewProj)
		{
			Context.VertexShader.Set(BasicVS);
			Context.PixelShader.Set(null);

			Context.OutputMerger.SetRenderTargets(shadowMap.DepthStencilView, (RenderTargetView) null);
			Context.Rasterizer.SetViewport(0f, 0f, shadowMap.Width, shadowMap.Height, 0f, 1f);
			Context.Rasterizer.State = PipelineStates.Rasterizer.FrontFaceCulling;

			Context.ClearDepthStencilView(shadowMap.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1f, 0);

			SetPerObjectState(SkullWorld, lightViewProj, SkullMaterial, null, Matrix.Identity);
			Context.DrawIndexed(SkullIndexCount, SkullIndicesOffset, SkullVerticesOffset);

			SetPerObjectState(WallWorld, lightViewProj, WallMaterial, WallTexture, Matrix.Scaling(4f));
			Context.DrawIndexed(WallIndexCount, WallIndicesOffset, WallVerticesOffset);

			SetPerObjectState(FloorWorld, lightViewProj, FloorMaterial, FloorTexture, Matrix.Scaling(4f));
			Context.DrawIndexed(FloorIndexCount, FloorIndicesOffset, FloorVerticesOffset);

			Context.Rasterizer.State = PipelineStates.Rasterizer.Default;
		}
	}
}
