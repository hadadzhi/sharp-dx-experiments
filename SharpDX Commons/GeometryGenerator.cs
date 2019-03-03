using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpDXCommons
{
	public struct Vertex
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector3 TangentU;
		public Vector2 TexCoord;
		public Color4 Color;

		public Vertex(
			float px, float py, float pz,
			float nx, float ny, float nz,
			float tx, float ty, float tz,
			float u, float v)
		{
			Position = new Vector3(px, py, pz);
			Normal = new Vector3(nx, ny, nz);
			TangentU = new Vector3(tx, ty, tz);
			TexCoord = new Vector2(u, v);
			Color = Color4.Black;
		}

		public static InputElement[] GetInputElements()
		{
			return new InputElement[]
			{
				new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
				new InputElement("NORMAL", 0, Format.R32G32B32_Float, 0),
				new InputElement("TANGENT_U", 0, Format.R32G32B32_Float, 0),
				new InputElement("TEX_COORD", 0, Format.R32G32_Float, 0),
				new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0)
			};
		}
	};

	public struct MeshData
	{
		public Vertex[] Vertices;
		public uint[] Indices;
	};

	internal class Scanner
	{
		private StreamReader reader;

		public Scanner(string filename)
		{
			reader = new StreamReader(filename);
		}

		private string NextWord()
		{
			StringBuilder sb = new StringBuilder();
			bool inside = false;

			while (true)
			{
				int ci = reader.Read();

				if (ci < 0)
				{
					break;
				}

				char c = (char) ci;

				if (char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '+' || c == 'e' || c == 'E')
				{
					inside = true;
					sb.Append(c);
				}
				else if (inside)
				{
					break;
				}
			}

			return sb.Length > 0 ? sb.ToString() : null;
		}

		public uint NextUint()
		{
			while (true)
			{
				string s = NextWord();

				if (s == null)
				{
					throw new InvalidOperationException("No more ints in the stream");
				}

				uint v;

				if (uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out v))
				{
					return v;
				}
			}
		}

		public int NextInt()
		{
			while (true)
			{
				string s = NextWord();

				if (s == null)
				{
					throw new InvalidOperationException("No more ints in the stream");
				}

				int v;

				if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out v))
				{
					return v;
				}
			}
		}

		public float NextFloat()
		{
			while (true)
			{
				string s = NextWord();

				if (s == null)
				{
					throw new InvalidOperationException("No more floats in the stream");
				}

				float v;

				if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out v))
				{
					return v;
				}
			}
		}

		public void Close()
		{
			reader.Close();
		}
	}

	public static class GeometryGenerator
	{
		public static MeshData LoadModel(string filename)
		{
			Scanner fin = new Scanner(filename);
			MeshData md = new MeshData();

			try
			{
				int verticesCount = fin.NextInt();
				int indicesCount = 3 * fin.NextInt();

				Log.Debug("Reading " + verticesCount + " vertices");

				Vertex[] vertices = new Vertex[verticesCount];

				for (int i = 0; i < verticesCount; i++)
				{
					vertices[i] = new Vertex(
						fin.NextFloat(), fin.NextFloat(), fin.NextFloat(), // Position
						fin.NextFloat(), fin.NextFloat(), fin.NextFloat(), // Normal
						0, 0, 0, 0, 0 // Unused
					);
				}

				uint[] indices = new uint[indicesCount];

				Log.Debug("Reading " + indicesCount + " indices");

				for (int i = 0; i < indicesCount; i++)
				{
					indices[i] = fin.NextUint();
				}

				md.Vertices = vertices;
				md.Indices = indices;
			}
			finally
			{
				fin.Close();
			}

			return md;
		}

		public static MeshData CreateGrid(float width, float depth, uint resx, uint resy, bool GIVE_ME_THE_HILLS = false)
		{
			uint vertexCount = resy * resx;
			uint faceCount = (resy - 1) * (resx - 1) * 2;

			//
			// Create the vertices.
			//

			float halfWidth = 0.5f * width;
			float halfDepth = 0.5f * depth;

			float dx = width / (resx - 1);
			float dz = depth / (resy - 1);

			float du = 1.0f / (resx - 1);
			float dv = 1.0f / (resy - 1);

			MeshData meshData;
			meshData.Vertices = new Vertex[vertexCount];

			for (uint i = 0; i < resy; i++)
			{
				float z = halfDepth - i * dz;
				for (uint j = 0; j < resx; j++)
				{
					float x = -halfWidth + j * dx;

					meshData.Vertices[i * resx + j].Position = new Vector3(x, 0.0f, z);
					meshData.Vertices[i * resx + j].Normal = new Vector3(0.0f, 1.0f, 0.0f);
					meshData.Vertices[i * resx + j].TangentU = new Vector3(1.0f, 0.0f, 0.0f);

					// Stretch texture over grid.
					meshData.Vertices[i * resx + j].TexCoord.X = j * du;
					meshData.Vertices[i * resx + j].TexCoord.Y = i * dv;

					if (GIVE_ME_THE_HILLS)
					{
						meshData.Vertices[i * resx + j].Position.Y = GetHillHeight(x, z);
						meshData.Vertices[i * resx + j].Normal = GetHillNormal(x, z);
					}
				}
			}

			//
			// Create the indices.
			//

			meshData.Indices = new uint[faceCount * 3]; // 3 indices per face

			// Iterate over each quad and compute indices.
			uint k = 0;
			for (uint i = 0; i < resy - 1; i++)
			{
				for (uint j = 0; j < resx - 1; j++)
				{
					meshData.Indices[k] = i * resx + j;
					meshData.Indices[k + 1] = i * resx + j + 1;
					meshData.Indices[k + 2] = (i + 1) * resx + j;

					meshData.Indices[k + 3] = (i + 1) * resx + j;
					meshData.Indices[k + 4] = i * resx + j + 1;
					meshData.Indices[k + 5] = (i + 1) * resx + j + 1;

					k += 6; // next quad
				}
			}

			return meshData;
		}

		public static float GetHillHeight(float x, float z)
		{
			// Commented is a function to better demonstrate depth pre-pass effect
			return 0.3f * (z * (float) Math.Sin(0.1f * x) + x * (float) Math.Cos(0.1f * z)); //-40 * (float) Math.Sin(0.001f * x * x + 0.001f * z * z);
		}

		public static Vector3 GetHillNormal(float x, float z)
		{
			// n = (-df/dx, 1, -df/dz)
			Vector3 n = new Vector3(
				-0.03f * z * (float) Math.Cos(0.1f * x) - 0.3f * (float) Math.Cos(0.1f * z),
				1.0f,
				-0.3f * (float) Math.Sin(0.1f * x) + 0.03f * x * (float) Math.Sin(0.1f * z)
			);

			// Commented is a function to better demonstrate depth pre-pass effect
			//Vector3 n = new Vector3(
			//    0.08f * x * (float) Math.Cos(0.001f * x * x + 0.001f * z * z),
			//    1,
			//    0.08f * z * (float) Math.Cos(0.001f * x * x + 0.001f * z * z)
			//);

			return Vector3.Normalize(n);
		}

		public static MeshData CreateBox(float width, float height, float depth)
		{
			//
			// Create the vertices.
			//
			Vertex[] v = new Vertex[24];

			float w2 = 0.5f * width;
			float h2 = 0.5f * height;
			float d2 = 0.5f * depth;

			// Fill in the front face vertex data.
			v[0] = new Vertex(-w2, -h2, -d2, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
			v[1] = new Vertex(-w2, +h2, -d2, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
			v[2] = new Vertex(+w2, +h2, -d2, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f);
			v[3] = new Vertex(+w2, -h2, -d2, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f);

			// Fill in the back face vertex data.
			v[4] = new Vertex(-w2, -h2, +d2, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f);
			v[5] = new Vertex(+w2, -h2, +d2, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
			v[6] = new Vertex(+w2, +h2, +d2, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
			v[7] = new Vertex(-w2, +h2, +d2, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f);

			// Fill in the top face vertex data.
			v[8] = new Vertex(-w2, +h2, -d2, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
			v[9] = new Vertex(-w2, +h2, +d2, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
			v[10] = new Vertex(+w2, +h2, +d2, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f);
			v[11] = new Vertex(+w2, +h2, -d2, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f);

			// Fill in the bottom face vertex data.
			v[12] = new Vertex(-w2, -h2, -d2, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f);
			v[13] = new Vertex(+w2, -h2, -d2, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
			v[14] = new Vertex(+w2, -h2, +d2, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
			v[15] = new Vertex(-w2, -h2, +d2, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f);

			// Fill in the left face vertex data.
			v[16] = new Vertex(-w2, -h2, +d2, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f);
			v[17] = new Vertex(-w2, +h2, +d2, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f);
			v[18] = new Vertex(-w2, +h2, -d2, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f);
			v[19] = new Vertex(-w2, -h2, -d2, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f);

			// Fill in the right face vertex data.
			v[20] = new Vertex(+w2, -h2, -d2, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f);
			v[21] = new Vertex(+w2, +h2, -d2, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
			v[22] = new Vertex(+w2, +h2, +d2, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f);
			v[23] = new Vertex(+w2, -h2, +d2, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f);

			//
			// Create the indices.
			//
			uint[] i = new uint[36];

			// Fill in the front face index data
			i[0] = 0;
			i[1] = 1;
			i[2] = 2;
			i[3] = 0;
			i[4] = 2;
			i[5] = 3;

			// Fill in the back face index data
			i[6] = 4;
			i[7] = 5;
			i[8] = 6;
			i[9] = 4;
			i[10] = 6;
			i[11] = 7;

			// Fill in the top face index data
			i[12] = 8;
			i[13] = 9;
			i[14] = 10;
			i[15] = 8;
			i[16] = 10;
			i[17] = 11;

			// Fill in the bottom face index data
			i[18] = 12;
			i[19] = 13;
			i[20] = 14;
			i[21] = 12;
			i[22] = 14;
			i[23] = 15;

			// Fill in the left face index data
			i[24] = 16;
			i[25] = 17;
			i[26] = 18;
			i[27] = 16;
			i[28] = 18;
			i[29] = 19;

			// Fill in the right face index data
			i[30] = 20;
			i[31] = 21;
			i[32] = 22;
			i[33] = 20;
			i[34] = 22;
			i[35] = 23;

			MeshData meshData = new MeshData();

			meshData.Vertices = v;
			meshData.Indices = i;

			return meshData;
		}

		public static MeshData CreateSphere(float radius, uint sliceCount, uint stackCount)
		{
			List<Vertex> vertices = new List<Vertex>();
			List<uint> indices = new List<uint>();

			//
			// Compute the vertices stating at the top pole and moving down the stacks.
			//
			// Poles: note that there will be texture coordinate distortion as there is
			// not a unique point on the texture map to assign to the pole when mapping
			// a rectangular texture onto a sphere.
			Vertex topVertex = new Vertex(0.0f, +radius, 0.0f, 0.0f, +1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
			Vertex bottomVertex = new Vertex(0.0f, -radius, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f);

			vertices.Add(topVertex);

			float phiStep = (float) Math.PI / stackCount;
			float thetaStep = 2 * (float) Math.PI / sliceCount;

			// Compute vertices for each stack ring (do not count the poles as rings).
			for (uint i = 1; i <= stackCount - 1; i++)
			{
				float phi = i * phiStep;

				// Vertices of ring.
				for (uint j = 0; j <= sliceCount; j++)
				{
					float theta = j * thetaStep;

					Vertex v = new Vertex();

					// spherical to cartesian
					v.Position.X = radius * (float) Math.Sin(phi) * (float) Math.Cos(theta);
					v.Position.Y = radius * (float) Math.Cos(phi);
					v.Position.Z = radius * (float) Math.Sin(phi) * (float) Math.Sin(theta);

					// Partial derivative of P with respect to theta
					v.TangentU.X = -radius * (float) Math.Sin(phi) * (float) Math.Sin(theta);
					v.TangentU.Y = 0.0f;
					v.TangentU.Z = radius * (float) Math.Sin(phi) * (float) Math.Cos(theta);

					v.TangentU.Normalize();
					v.Normal = Vector3.Normalize(v.Position);

					v.TexCoord.X = theta / ((float) Math.PI * 2);
					v.TexCoord.Y = phi / (float) Math.PI;

					vertices.Add(v);
				}
			}

			vertices.Add(bottomVertex);

			//
			// Compute indices for top stack.  The top stack was written first to the vertex buffer
			// and connects the top pole to the first ring.
			//
			for (uint i = 1; i <= sliceCount; i++)
			{
				indices.Add(0);
				indices.Add(i + 1);
				indices.Add(i);
			}

			//
			// Compute indices for inner stacks (not connected to poles).
			//
			// Offset the indices to the index of the first vertex in the first ring.
			// This is just skipping the top pole vertex.
			uint baseIndex = 1;
			uint ringVertexCount = sliceCount + 1;

			for (uint i = 0; i < stackCount - 2; i++)
			{
				for (uint j = 0; j < sliceCount; j++)
				{
					indices.Add(baseIndex + i * ringVertexCount + j);
					indices.Add(baseIndex + i * ringVertexCount + j + 1);
					indices.Add(baseIndex + (i + 1) * ringVertexCount + j);

					indices.Add(baseIndex + (i + 1) * ringVertexCount + j);
					indices.Add(baseIndex + i * ringVertexCount + j + 1);
					indices.Add(baseIndex + (i + 1) * ringVertexCount + j + 1);
				}
			}

			//
			// Compute indices for bottom stack.  The bottom stack was written last to the vertex buffer
			// and connects the bottom pole to the bottom ring.
			//
			// South pole vertex was added last.
			uint southPoleIndex = (uint) vertices.Count - 1;

			// Offset the indices to the index of the first vertex in the last ring.
			baseIndex = southPoleIndex - ringVertexCount;

			for (uint i = 0; i < sliceCount; i++)
			{
				indices.Add(southPoleIndex);
				indices.Add(baseIndex + i);
				indices.Add(baseIndex + i + 1);
			}

			MeshData meshData = new MeshData();

			meshData.Vertices = vertices.ToArray();
			meshData.Indices = indices.ToArray();

			return meshData;
		}

		public static MeshData CreateGeosphere(float radius, int numSubdivisions)
		{
			numSubdivisions = Math.Min(numSubdivisions, 5);

			// Approximate a sphere by tessellating an icosahedron.

			const float X = 0.525731f;
			const float Z = 0.850651f;

			Vector3[] pos = new Vector3[]
			{
				new Vector3(-X, 0.0f, Z),
				new Vector3(X, 0.0f, Z),
				new Vector3(-X, 0.0f, -Z),
				new Vector3(X, 0.0f, -Z),
				new Vector3(0.0f, Z, X),
				new Vector3(0.0f, Z, -X),
				new Vector3(0.0f, -Z, X),
				new Vector3(0.0f, -Z, -X),
				new Vector3(Z, X, 0.0f),
				new Vector3(-Z, X, 0.0f),
				new Vector3(Z, -X, 0.0f),
				new Vector3(-Z, -X, 0.0f)
			};

			uint[] k = new uint[]
			{
				1,4,0,  4,9,0,  4,5,9,  8,5,4,  1,8,4,
				1,10,8, 10,3,8, 8,3,5,  3,2,5,  3,7,2,
				3,10,7, 10,6,7, 6,11,7, 6,0,11, 6,1,0,
				10,1,6, 11,0,9, 2,11,9, 5,2,9,  11,2,7
			};

			MeshData meshData = new MeshData
			{
				Vertices = new Vertex[12],
				Indices = new uint[60]
			};

			for (int i = 0; i < 12; i++)
			{
				meshData.Vertices[i].Position = pos[i];
			}

			for (int i = 0; i < 60; i++)
			{
				meshData.Indices[i] = k[i];
			}

			for (int i = 0; i < numSubdivisions; i++)
				Subdivide(ref meshData);

			// Project vertices onto sphere and scale.
			for (int i = 0; i < meshData.Vertices.Length; i++)
			{
				// Project onto unit sphere.
				Vector3 n = Vector3.Normalize(meshData.Vertices[i].Position);

				// Project onto sphere.
				meshData.Vertices[i].Position = Vector3.Multiply(n, radius);
				meshData.Vertices[i].Normal = n;

				// Derive texture coordinates from spherical coordinates.
				float theta = AngleFromXY(meshData.Vertices[i].Position.X, meshData.Vertices[i].Position.Z);
				float phi = (float) Math.Acos(meshData.Vertices[i].Position.Y / radius);

				meshData.Vertices[i].TexCoord.X = theta / (2 * (float) Math.PI);
				meshData.Vertices[i].TexCoord.Y = phi / (float) Math.PI;

				// Partial derivative of P with respect to theta
				meshData.Vertices[i].TangentU.X = -radius * (float) Math.Sin(phi) * (float) Math.Sin(theta);
				meshData.Vertices[i].TangentU.Y = 0;
				meshData.Vertices[i].TangentU.Z = radius * (float) Math.Sin(phi) * (float) Math.Cos(theta);

				meshData.Vertices[i].TangentU.Normalize();
			}

			return meshData;
		}

		private static float AngleFromXY(float x, float y)
		{
			float theta = 0;

			// Quadrant I or IV
			if (x >= 0)
			{
				// If x = 0, then atanf(y/x) = +pi/2 if y > 0
				//                atanf(y/x) = -pi/2 if y < 0
				theta = (float) Math.Atan(y / x); // in [-pi/2, +pi/2]

				if (theta < 0)
					theta += 2 * (float) Math.PI; // in [0, 2*pi).
			}
			else // Quadrant II or III
			{
				theta = (float) (Math.Atan(y / x) + Math.PI); // in [0, 2*pi).
			}

			return theta;
		}

		private static void Subdivide(ref MeshData meshData)
		{
			List<Vertex> vertices = new List<Vertex>();
			List<uint> indices = new List<uint>();

			//       v1
			//       *
			//      / \
			//     /   \
			//  m0*-----*m1
			//   / \   / \
			//  /   \ /   \
			// *-----*-----*
			// v0    m2     v2

			int numTris = meshData.Indices.Length / 3;

			for (int i = 0; i < numTris; i++)
			{
				Vertex v0 = meshData.Vertices[meshData.Indices[i * 3 + 0]];
				Vertex v1 = meshData.Vertices[meshData.Indices[i * 3 + 1]];
				Vertex v2 = meshData.Vertices[meshData.Indices[i * 3 + 2]];

				//
				// Generate the midpoints.
				//
				Vertex m0 = new Vertex();
				Vertex m1 = new Vertex();
				Vertex m2 = new Vertex();

				// For subdivision, we just care about the position component.  We derive the other
				// vertex components in CreateGeosphere.
				m0.Position = new Vector3(
					0.5f * (v0.Position.X + v1.Position.X),
					0.5f * (v0.Position.Y + v1.Position.Y),
					0.5f * (v0.Position.Z + v1.Position.Z));

				m1.Position = new Vector3(
					0.5f * (v1.Position.X + v2.Position.X),
					0.5f * (v1.Position.Y + v2.Position.Y),
					0.5f * (v1.Position.Z + v2.Position.Z));

				m2.Position = new Vector3(
					0.5f * (v0.Position.X + v2.Position.X),
					0.5f * (v0.Position.Y + v2.Position.Y),
					0.5f * (v0.Position.Z + v2.Position.Z));

				//
				// Add new geometry.
				//
				vertices.Add(v0); // 0
				vertices.Add(v1); // 1
				vertices.Add(v2); // 2
				vertices.Add(m0); // 3
				vertices.Add(m1); // 4
				vertices.Add(m2); // 5

				indices.Add((uint) (i * 6 + 0));
				indices.Add((uint) (i * 6 + 3));
				indices.Add((uint) (i * 6 + 5));

				indices.Add((uint) (i * 6 + 3));
				indices.Add((uint) (i * 6 + 4));
				indices.Add((uint) (i * 6 + 5));

				indices.Add((uint) (i * 6 + 5));
				indices.Add((uint) (i * 6 + 4));
				indices.Add((uint) (i * 6 + 2));

				indices.Add((uint) (i * 6 + 3));
				indices.Add((uint) (i * 6 + 1));
				indices.Add((uint) (i * 6 + 4));
			}

			meshData.Vertices = vertices.ToArray();
			meshData.Indices = indices.ToArray();
		}

		public static MeshData CreateFullscreenQuad()
		{
			MeshData meshData = new MeshData
			{
				Vertices = new Vertex[4],
				Indices = new uint[6]
			};

			// Position coordinates specified in NDC space.
			meshData.Vertices[0] = new Vertex(
				-1.0f, -1.0f, 0.0f,
				0.0f, 0.0f, -1.0f,
				1.0f, 0.0f, 0.0f,
				0.0f, 1.0f);

			meshData.Vertices[1] = new Vertex(
				-1.0f, +1.0f, 0.0f,
				0.0f, 0.0f, -1.0f,
				1.0f, 0.0f, 0.0f,
				0.0f, 0.0f);

			meshData.Vertices[2] = new Vertex(
				+1.0f, +1.0f, 0.0f,
				0.0f, 0.0f, -1.0f,
				1.0f, 0.0f, 0.0f,
				1.0f, 0.0f);

			meshData.Vertices[3] = new Vertex(
				+1.0f, -1.0f, 0.0f,
				0.0f, 0.0f, -1.0f,
				1.0f, 0.0f, 0.0f,
				1.0f, 1.0f);

			meshData.Indices[0] = 0;
			meshData.Indices[1] = 1;
			meshData.Indices[2] = 2;

			meshData.Indices[3] = 0;
			meshData.Indices[4] = 2;
			meshData.Indices[5] = 3;

			return meshData;
		}

		public static MeshData CreateCylinder(float bottomRadius, float topRadius, float height, uint sliceCount, uint stackCount)
		{
			List<Vertex> vertices = new List<Vertex>();
			List<uint> indices = new List<uint>();

			//
			// Build Stacks.
			//
			float stackHeight = height / stackCount;

			// Amount to increment radius as we move up each stack level from bottom to top.
			float radiusStep = (topRadius - bottomRadius) / stackCount;
			uint ringCount = stackCount + 1;

			// Compute vertices for each stack ring starting at the bottom and moving up.
			for (uint i = 0; i < ringCount; i++)
			{
				float y = -0.5f * height + i * stackHeight;
				float r = bottomRadius + i * radiusStep;

				// vertices of ring
				float dTheta = 2.0f * (float) Math.PI / sliceCount;
				for (uint j = 0; j <= sliceCount; j++)
				{
					Vertex vertex = new Vertex();

					float c = (float) Math.Cos(j * dTheta);
					float s = (float) Math.Sin(j * dTheta);

					vertex.Position = new Vector3(r * c, y, r * s);

					vertex.TexCoord.X = (float) j / sliceCount;
					vertex.TexCoord.Y = 1.0f - (float) i / stackCount;

					// Cylinder can be parameterized as follows, where we introduce v
					// parameter that goes in the same direction as the v tex-coord
					// so that the bitangent goes in the same direction as the v tex-coord.
					//   Let r0 be the bottom radius and let r1 be the top radius.
					//   y(v) = h - hv for v in [0,1].
					//   r(v) = r1 + (r0-r1)v
					//
					//   x(t, v) = r(v)*cos(t)
					//   y(t, v) = h - hv
					//   z(t, v) = r(v)*sin(t)
					// 
					//  dx/dt = -r(v)*sin(t)
					//  dy/dt = 0
					//  dz/dt = +r(v)*cos(t)
					//
					//  dx/dv = (r0-r1)*cos(t)
					//  dy/dv = -h
					//  dz/dv = (r0-r1)*sin(t)

					// This is unit length.
					vertex.TangentU = new Vector3(-s, 0.0f, c);

					float dr = bottomRadius - topRadius;
					Vector3 bitangent = new Vector3(dr * c, -height, dr * s);

					vertex.Normal = Vector3.Normalize(Vector3.Cross(vertex.TangentU, bitangent));

					vertices.Add(vertex);
				}
			}

			// Add one because we duplicate the first and last vertex per ring
			// since the texture coordinates are different.
			uint ringVertexCount = sliceCount + 1;

			// Compute indices for each stack.
			for (uint i = 0; i < stackCount; ++i)
			{
				for (uint j = 0; j < sliceCount; ++j)
				{
					indices.Add(i * ringVertexCount + j);
					indices.Add((i + 1) * ringVertexCount + j);
					indices.Add((i + 1) * ringVertexCount + j + 1);

					indices.Add(i * ringVertexCount + j);
					indices.Add((i + 1) * ringVertexCount + j + 1);
					indices.Add(i * ringVertexCount + j + 1);
				}
			}

			BuildCylinderTopCap(bottomRadius, topRadius, height, sliceCount, stackCount, vertices, indices);
			BuildCylinderBottomCap(bottomRadius, topRadius, height, sliceCount, stackCount, vertices, indices);

			MeshData meshData = new MeshData
			{
				Vertices = vertices.ToArray(),
				Indices = indices.ToArray()
			};

			return meshData;
		}

		private static void BuildCylinderTopCap(float bottomRadius, float topRadius, float height, uint sliceCount, uint stackCount, List<Vertex> vertices, List<uint> indices)
		{
			uint baseIndex = (uint) vertices.Count;

			float y = 0.5f * height;
			float dTheta = 2 * (float) Math.PI / sliceCount;

			// Duplicate cap ring vertices because the texture coordinates and normals differ.
			for (uint i = 0; i <= sliceCount; i++)
			{
				float x = topRadius * (float) Math.Cos(i * dTheta);
				float z = topRadius * (float) Math.Sin(i * dTheta);

				// Scale down by the height to try and make top cap texture coord area
				// proportional to base.
				float u = x / height + 0.5f;
				float v = z / height + 0.5f;

				vertices.Add(new Vertex(x, y, z, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, u, v));
			}

			// Cap center vertex.
			vertices.Add(new Vertex(0.0f, y, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.5f, 0.5f));

			// Index of center vertex.
			uint centerIndex = (uint) vertices.Count - 1;

			for (uint i = 0; i < sliceCount; i++)
			{
				indices.Add(centerIndex);
				indices.Add(baseIndex + i + 1);
				indices.Add(baseIndex + i);
			}
		}

		private static void BuildCylinderBottomCap(float bottomRadius, float topRadius, float height, uint sliceCount, uint stackCount, List<Vertex> vertices, List<uint> indices)
		{
			// 
			// Build bottom cap.
			//
			uint baseIndex = (uint) vertices.Count;
			float y = -0.5f * height;

			// vertices of ring
			float dTheta = 2.0f * (float) Math.PI / sliceCount;

			for (uint i = 0; i <= sliceCount; i++)
			{
				float x = bottomRadius * (float) Math.Cos(i * dTheta);
				float z = bottomRadius * (float) Math.Sin(i * dTheta);

				// Scale down by the height to try and make top cap texture coord area
				// proportional to base.
				float u = x / height + 0.5f;
				float v = z / height + 0.5f;

				vertices.Add(new Vertex(x, y, z, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f, 0.0f, u, v));
			}

			// Cap center vertex.
			vertices.Add(new Vertex(0.0f, y, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.5f, 0.5f));

			// Cache the index of center vertex.
			uint centerIndex = (uint) vertices.Count - 1;

			for (uint i = 0; i < sliceCount; ++i)
			{
				indices.Add(centerIndex);
				indices.Add(baseIndex + i);
				indices.Add(baseIndex + i + 1);
			}
		}
	}
}
