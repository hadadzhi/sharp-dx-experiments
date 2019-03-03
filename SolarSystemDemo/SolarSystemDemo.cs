using System.Linq;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDXCommons;
using SolarSystemDemo.Graphics;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SolarSystemDemo
{
	public class SolarSystemDemo : SharpDXApplication
	{
		private Matrix Proj;
		private Matrix ViewProj;

		private Buffer ConstantBufferPerObject;
		private Buffer ConstantBufferPerFrame;

		private PointLight PointLight;

		private Buffer VertexBuffer;
		private Buffer IndexBuffer;

		private RasterizerState WireframeState;
		private RasterizerState WireframeNoCullState;

		private GameState GameState;

		public SolarSystemDemo(GraphicsConfiguration configuration)
			: base(configuration, "SolarSystem Demo")
		{
			Init();
			TargetsResized += OnBuffersResized;
		}

		private void Init()
		{
			Scene.Initialize();
			StaticGraphicsResources.InitializeStaticGraphicsData(Device);

			GameState = new GameState(RenderWindow);

			#region Graphics Initialize

			CreateLights();

			PixelShader ps = new PixelShader(Device, ShaderBytecode.FromFile("pixel.shd"));
			ShaderBytecode vsbytecode = ShaderBytecode.FromFile("vertex.shd");
			VertexShader vs = new VertexShader(Device, vsbytecode);
			InputLayout layout = new InputLayout(
				Device,
				vsbytecode,
				Vertex.GetInputElements()
			);

			Context.InputAssembler.InputLayout = layout;
			Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			Context.VertexShader.Set(vs);
			Context.PixelShader.Set(ps);

			vsbytecode.Dispose();
			vs.Dispose();
			ps.Dispose();
			layout.Dispose();

			WireframeState = new RasterizerState(
				Device,
				new RasterizerStateDescription
				{
					CullMode = CullMode.Back,
					DepthBias = 0,
					DepthBiasClamp = 0.0f,
					FillMode = FillMode.Wireframe,
					IsAntialiasedLineEnabled = false,
					IsDepthClipEnabled = false,
					IsFrontCounterClockwise = false,
					IsMultisampleEnabled = false,
					IsScissorEnabled = false,
					SlopeScaledDepthBias = 0.0f
				}
			);

			WireframeNoCullState = new RasterizerState(
				Device,
				new RasterizerStateDescription
				{
					CullMode = CullMode.None,
					DepthBias = 0,
					DepthBiasClamp = 0.0f,
					FillMode = FillMode.Wireframe,
					IsAntialiasedLineEnabled = false,
					IsDepthClipEnabled = false,
					IsFrontCounterClockwise = false,
					IsMultisampleEnabled = false,
					IsScissorEnabled = false,
					SlopeScaledDepthBias = 0.0f
				}
			);

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

			Context.VertexShader.SetConstantBuffer(1, ConstantBufferPerObject);
			Context.PixelShader.SetConstantBuffers(0, 2, new Buffer[] { ConstantBufferPerFrame, ConstantBufferPerObject });

			Context.Rasterizer.State = PipelineStates.Rasterizer.DisableBackfaceCulling;

			#endregion Graphics Initialize
		}

		private void CreateLights()
		{
			PointLight = new PointLight
			{
				Ambient = new Vector4(0, 0, 0, 0),
				Diffuse = new Vector4(1.0f, 1.0f, 0.5f, 1.0f),
				Specular = new Vector4(1.0f, 1.0f, 0.3f, 1.0f),
				Att = new Vector3(0.0f, 0.0f, 0.01f),
				Range = float.MaxValue,
				Position = new Vector3(0, 0, 0)
			};
		}

		private void OnBuffersResized(int newWidth, int newHeight)
		{
			Proj = Matrix.PerspectiveFovLH(0.25f * MathUtil.Pi, (float) newWidth / newHeight, 0.1f, float.MaxValue);
		}

		protected override void UpdateScene(double delta)
		{
			GameState.UpdateState((float) delta);

			ViewProj = Matrix.Multiply(GameState.GetPlayerCameraViewMatrix(), Proj);
			Vector3 cameraPosition = GameState.GetPlayerCameraPosition();

			PointLight.Position = GameState.SunPointLightPosition;

			CBPerFrame cbPerFrame = new CBPerFrame
			{
				//DirLight = DirectionalLight,
				PointLight = PointLight,
				//SpotLight = new SpotLight(),
				ViewerPosition = cameraPosition
			};

			Context.UpdateSubresource(ref cbPerFrame, ConstantBufferPerFrame);
		}

		protected override void RenderScene()
		{
			base.RenderScene();

			if (Scene.NeedUpdateBuffers)
			{
				VertexBuffer = new Buffer(
					Device,
					Scene.Vertices.Count * Marshal.SizeOf(typeof(Vertex)),
					ResourceUsage.Default,
					BindFlags.VertexBuffer,
					CpuAccessFlags.None,
					ResourceOptionFlags.None,
					0
				);

				Context.UpdateSubresource(Scene.Vertices.ToArray(), VertexBuffer);

				IndexBuffer = new Buffer(
					Device,
					Scene.Indices.Count * Marshal.SizeOf(typeof(uint)),
					ResourceUsage.Default,
					BindFlags.IndexBuffer,
					CpuAccessFlags.None,
					ResourceOptionFlags.None,
					0
				);

				Context.UpdateSubresource(Scene.Indices.ToArray(), IndexBuffer);

				Context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
				Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Marshal.SizeOf(typeof(Vertex)), 0));

				Scene.NeedUpdateBuffers = false;
			}


			foreach (GraphicsData data in GameState.GetGraphicsData().Where(d => d.IsVisible))
			{
				Matrix translationMatrix = Matrix.Translation(data.GraphicsPosition);
				Matrix rotationMatrix = Matrix.RotationQuaternion(data.RotationQuaternion);
				Matrix scalingMatrix = Matrix.Scaling(data.ScalingVector);
				Matrix worldMatrix = Matrix.Multiply(scalingMatrix, Matrix.Multiply(rotationMatrix, translationMatrix));

				SetPerObjectState(
					worldMatrix,
					Scene.GetMaterial(data.MaterialId),
					Scene.GetTexture(data.TextureId),
					Matrix.Identity
				);

				Context.DrawIndexed(
					Scene.GetMeshData(data.MeshDataId).Indices.Length,
					Scene.GetMeshDataIndicesOffset(data.MeshDataId),
					Scene.GetMeshDataVerticesOffset(data.MeshDataId)
				);
			}
		}

		private void SetPerObjectState(Matrix world, Material material, ShaderResourceView texture, Matrix textureTransform)
		{
			CBPerObject cbPerObject = new CBPerObject
			{
				World = Matrix.Transpose(world),
				WorldInvTranspose = Matrix.Invert(world),
				WorldViewProj = Matrix.Transpose(Matrix.Multiply(world, ViewProj)),
				TextureTransform = Matrix.Transpose(textureTransform),
				Material = material
			};

			Context.UpdateSubresource(ref cbPerObject, ConstantBufferPerObject);
			Context.PixelShader.SetShaderResource(0, texture);
			Context.PixelShader.SetSampler(0, PipelineStates.Sampler.WrappedAnisotropic);
		}
	}
}
