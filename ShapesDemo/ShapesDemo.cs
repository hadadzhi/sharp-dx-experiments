using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDXCommons;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDXCommons.Cameras;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Runtime.InteropServices;

namespace ShapesDemo
{
	struct ShaderConstant
	{
		public Matrix WorldViewProj;
	}

	public class ShapesDemo : SharpDXApplication
	{
		private Matrix Proj;
		private Matrix ViewProj;

		private Buffer ConstantBuffer;

		private SimpleFirstPersonCamera CameraCamera;

		private int BoxVerticesOffset;
		private int GridVerticesOffset;
		private int SphereVerticesOffset;
		private int GeoSphereVerticesOffset;
		private int CylinderVerticesOffset;

		private int BoxIndicesOffset;
		private int GridIndicesOffset;
		private int SphereIndicesOffset;
		private int GeoSphereIndicesOffset;
		private int CylinderIndicesOffset;

		private int BoxIndexCount;
		private int GridIndexCount;
		private int SphereIndexCount;
		private int GeoSphereIndexCount;
		private int CylinderIndexCount;

		private Matrix[] SphereWorld = new Matrix[10];
		private Matrix[] CylWorld = new Matrix[10];
		private Matrix BoxWorld;
		private Matrix CenterSphereWorld;

		private Buffer VertexBuffer;
		private Buffer IndexBuffer;

		private RasterizerState WireframeState;
		private RasterizerState WireframeNoCullState;

		public ShapesDemo(GraphicsConfiguration configuration)
			: base(configuration, "Shapes Demo")
		{
			Init();
			TargetsResized += OnBuffersResized;
		}

		private void Init()
		{
			CreateGeometryBuffers();
			CreateWorldMatrices();

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

			ConstantBuffer = new Buffer(
				Device,
				Marshal.SizeOf(typeof(ShaderConstant)),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			Context.VertexShader.SetConstantBuffer(0, ConstantBuffer);

			CameraCamera = new SimpleFirstPersonCamera(new Vector3(3, 3, 3));
			CameraCamera.InstallControls(RenderWindow);
		}

		private void CreateWorldMatrices()
		{
			BoxWorld = Matrix.Multiply(Matrix.Scaling(2.0f, 1.0f, 2.0f), Matrix.Translation(0.0f, 0.5f, 0.0f));
			CenterSphereWorld = Matrix.Multiply(Matrix.Scaling(2.0f, 2.0f, 2.0f), Matrix.Translation(0.0f, 2.0f, 0.0f));

			for (int i = 0; i < 5; i++)
			{
				CylWorld[2 * i] = Matrix.Translation(-5, 1.5f, -10 + i * 5);
				CylWorld[2 * i + 1] = Matrix.Translation(5, 1.5f, -10 + i * 5);

				SphereWorld[2 * i] = Matrix.Translation(-5, 3.5f, -10 + i * 5);
				SphereWorld[2 * i + 1] = Matrix.Translation(5, 3.5f, -10 + i * 5);
			}
		}

		private void CreateGeometryBuffers()
		{
			MeshData box = GeometryGenerator.CreateBox(1, 1, 1);
			MeshData grid = GeometryGenerator.CreateGrid(20, 30, 40, 60);
			MeshData sphere = GeometryGenerator.CreateSphere(0.5f, 20, 20);
			MeshData geoSphere = GeometryGenerator.CreateGeosphere(0.5f, 2);
			MeshData cylinder = GeometryGenerator.CreateCylinder(0.5f, 0.3f, 3, 20, 20);

			BoxVerticesOffset = 0;
			GridVerticesOffset = BoxVerticesOffset + box.Vertices.Length;
			SphereVerticesOffset = GridVerticesOffset + grid.Vertices.Length;
			GeoSphereVerticesOffset = SphereVerticesOffset + sphere.Vertices.Length;
			CylinderVerticesOffset = GeoSphereVerticesOffset + geoSphere.Vertices.Length;

			BoxIndexCount = box.Indices.Length;
			GridIndexCount = grid.Indices.Length;
			SphereIndexCount = sphere.Indices.Length;
			GeoSphereIndexCount = geoSphere.Indices.Length;
			CylinderIndexCount = cylinder.Indices.Length;

			BoxIndicesOffset = 0;
			GridIndicesOffset = BoxIndicesOffset + BoxIndexCount;
			SphereIndicesOffset = GridIndicesOffset + GridIndexCount;
			GeoSphereIndicesOffset = SphereIndicesOffset + SphereIndexCount;
			CylinderIndicesOffset = GeoSphereIndicesOffset + GeoSphereIndexCount;

			int totalVertices = box.Vertices.Length + grid.Vertices.Length + sphere.Vertices.Length + geoSphere.Vertices.Length + cylinder.Vertices.Length;
			int totalIndices = BoxIndexCount + GridIndexCount + SphereIndexCount + GeoSphereIndexCount + CylinderIndexCount;

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

			vertexList.AddRange(box.Vertices);
			vertexList.AddRange(grid.Vertices);
			vertexList.AddRange(sphere.Vertices);
			vertexList.AddRange(geoSphere.Vertices);
			vertexList.AddRange(cylinder.Vertices);

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

			indexList.AddRange(box.Indices);
			indexList.AddRange(grid.Indices);
			indexList.AddRange(sphere.Indices);
			indexList.AddRange(geoSphere.Indices);
			indexList.AddRange(cylinder.Indices);

			uint[] indexArray = indexList.ToArray();

			Context.UpdateSubresource(indexArray, IndexBuffer);

			Context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
			Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Marshal.SizeOf(typeof(Vertex)), 0));
		}

		private void OnBuffersResized(int newWidth, int newHeight)
		{
			Proj = Matrix.PerspectiveFovLH(0.25f * (float) Math.PI, (float) newWidth / newHeight, 0.1f, 10000f);
		}

		protected override void UpdateScene(double delta)
		{
			CameraCamera.Update((float) delta);
			ViewProj = Matrix.Multiply(CameraCamera.ViewMatrix, Proj);
		}

		protected override void RenderScene()
		{
			base.RenderScene();

			ShaderConstant shConst = new ShaderConstant();

			Context.Rasterizer.State = WireframeNoCullState;

			// Grid
			shConst.WorldViewProj = Matrix.Transpose(ViewProj);
			Context.UpdateSubresource(ref shConst, ConstantBuffer);
			Context.DrawIndexed(GridIndexCount, GridIndicesOffset, GridVerticesOffset);

			Context.Rasterizer.State = WireframeState;

			// Box
			shConst.WorldViewProj = Matrix.Transpose(Matrix.Multiply(BoxWorld, ViewProj));
			Context.UpdateSubresource(ref shConst, ConstantBuffer);
			Context.DrawIndexed(BoxIndexCount, BoxIndicesOffset, BoxVerticesOffset);

			// Center sphere
			shConst.WorldViewProj = Matrix.Transpose(Matrix.Multiply(CenterSphereWorld, ViewProj));
			Context.UpdateSubresource(ref shConst, ConstantBuffer);
			Context.DrawIndexed(SphereIndexCount, SphereIndicesOffset, SphereVerticesOffset);

			for (int i = 0; i < CylWorld.Length; i++)
			{
				// Cylinder
				shConst.WorldViewProj = Matrix.Transpose(Matrix.Multiply(CylWorld[i], ViewProj));
				Context.UpdateSubresource(ref shConst, ConstantBuffer);
				Context.DrawIndexed(CylinderIndexCount, CylinderIndicesOffset, CylinderVerticesOffset);

				// Cylinder sphere
				shConst.WorldViewProj = Matrix.Transpose(Matrix.Multiply(SphereWorld[i], ViewProj));
				Context.UpdateSubresource(ref shConst, ConstantBuffer);
				Context.DrawIndexed(GeoSphereIndexCount, GeoSphereIndicesOffset, GeoSphereVerticesOffset);
			}
		}
	}
}
