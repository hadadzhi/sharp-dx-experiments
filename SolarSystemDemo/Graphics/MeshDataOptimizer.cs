using System.Collections.Generic;
using SharpDX;
using SharpDXCommons;
using SolarSystemDemo.MineCraft;
using SolarSystemDemo.MineCraft.Cubes;

namespace SolarSystemDemo.Graphics
{
	public static class MeshDataOptimizer
	{
		public static MeshData OptimizeCubeMeshData(List<BaseStructureBlock> cubes, Vector3 massCenterShift)
		{
			List<Vertex> vertices = new List<Vertex>();
			List<uint> indices = new List<uint>();
			uint indexShift = 0;

			foreach (Cube cube in cubes)
			{
				Vector3 shift = massCenterShift + cube.RelativePosition;

				MeshData meshData = GenerateCubeMeshData(
					shift.X,
					shift.Y,
					shift.Z,
					indexShift
				);

				vertices.AddRange(meshData.Vertices);
				indices.AddRange(meshData.Indices);
				indexShift += (uint) meshData.Vertices.Length;
			}

			return new MeshData
			{
				Vertices = vertices.ToArray(),
				Indices = indices.ToArray()
			};
		}

		public static MeshData GenerateCubeMeshData(float shiftX, float shiftY, float shiftZ, uint indexShift)
		{
			float width = 1;
			float height = 1;
			float depth = 1;

			float w2 = 0.5f * width;
			float h2 = 0.5f * height;
			float d2 = 0.5f * depth;

			Vertex[] v = new Vertex[]
			{
				// Fill in the back face vertex data.
				new Vertex(-w2 + shiftX, -h2 + shiftY, -d2 + shiftZ, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f),
				new Vertex(-w2 + shiftX, +h2 + shiftY, -d2 + shiftZ, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f),
				new Vertex(+w2 + shiftX, +h2 + shiftY, -d2 + shiftZ, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f),
				new Vertex(+w2 + shiftX, -h2 + shiftY, -d2 + shiftZ, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f),

				// Fill in the front face vertex data.
				new Vertex(-w2 + shiftX, -h2 + shiftY, +d2 + shiftZ, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f),
				new Vertex(+w2 + shiftX, -h2 + shiftY, +d2 + shiftZ, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f),
				new Vertex(+w2 + shiftX, +h2 + shiftY, +d2 + shiftZ, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f),
				new Vertex(-w2 + shiftX, +h2 + shiftY, +d2 + shiftZ, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f),

				// Fill in the top face vertex data.
				new Vertex(-w2 + shiftX, +h2 + shiftY, -d2 + shiftZ, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f),
				new Vertex(-w2 + shiftX, +h2 + shiftY, +d2 + shiftZ, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f),
				new Vertex(+w2 + shiftX, +h2 + shiftY, +d2 + shiftZ, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f),
				new Vertex(+w2 + shiftX, +h2 + shiftY, -d2 + shiftZ, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f),

				// Fill in the bottom face vertex data.
				new Vertex(-w2 + shiftX, -h2 + shiftY, -d2 + shiftZ, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f),
				new Vertex(+w2 + shiftX, -h2 + shiftY, -d2 + shiftZ, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f),
				new Vertex(+w2 + shiftX, -h2 + shiftY, +d2 + shiftZ, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f),
				new Vertex(-w2 + shiftX, -h2 + shiftY, +d2 + shiftZ, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f),

				// Fill in the left face vertex data.
				new Vertex(-w2 + shiftX, -h2 + shiftY, +d2 + shiftZ, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f),
				new Vertex(-w2 + shiftX, +h2 + shiftY, +d2 + shiftZ, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f),
				new Vertex(-w2 + shiftX, +h2 + shiftY, -d2 + shiftZ, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f),
				new Vertex(-w2 + shiftX, -h2 + shiftY, -d2 + shiftZ, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f),

				// Fill in the right face vertex data.
				new Vertex(+w2 + shiftX, -h2 + shiftY, -d2 + shiftZ, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f),
				new Vertex(+w2 + shiftX, +h2 + shiftY, -d2 + shiftZ, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f),
				new Vertex(+w2 + shiftX, +h2 + shiftY, +d2 + shiftZ, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f),
				new Vertex(+w2 + shiftX, -h2 + shiftY, +d2 + shiftZ, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f)
			};

			//
			// Create the indices.
			//
			uint[] i = new uint[]
			{
				// Fill in the back face index data
				0 + indexShift,
				1 + indexShift,
				2 + indexShift,
				0 + indexShift,
				2 + indexShift,
				3 + indexShift,

				// Fill in the front face index data
				4 + indexShift,
				5 + indexShift,
				6 + indexShift,
				4 + indexShift,
				6 + indexShift,
				7 + indexShift,

				// Fill in the top face index data
				8 + indexShift,
				9 + indexShift,
				10 + indexShift,
				8 + indexShift,
				10 + indexShift,
				11 + indexShift,

				// Fill in the bottom face index data
				12 + indexShift,
				13 + indexShift,
				14 + indexShift,
				12 + indexShift,
				14 + indexShift,
				15 + indexShift,

				// Fill in the left face index data
				16 + indexShift,
				17 + indexShift,
				18 + indexShift,
				16 + indexShift,
				18 + indexShift,
				19 + indexShift,

				// Fill in the right face index data
				20 + indexShift,
				21 + indexShift,
				22 + indexShift,
				20 + indexShift,
				22 + indexShift,
				23 + indexShift
			};

			return new MeshData()
			{
				Vertices = v,
				Indices = i
			};
		}
	}
}
