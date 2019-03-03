using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace SharpDXCommons
{
	public class Waves
	{
		public int RowCount { get; private set; }
		public int ColumnCount { get; private set; }

		public int VertexCount { get; private set; }
		public int TriangleCount { get; private set; }

		// Simulation constants we can precompute.
		private float k1;
		private float k2;
		private float k3;

		private float timeStep;
		private float spatialStep;

		private Vector3[] previousSolution;
		private Vector3[] currentSolution;

		public Vertex[] vertices;

		private float time = 0;

		public Waves(int m, int n, float dx, float dt, float speed, float damping)
		{
			RowCount = m;
			ColumnCount = n;

			VertexCount = n * m;
			TriangleCount = (m - 1) * (n - 1) * 2;

			timeStep = dt;
			spatialStep = dx;

			float d = damping * dt + 2.0f;
			float e = (speed * speed) * (dt * dt) / (dx * dx);
			k1 = (damping * dt - 2.0f) / d;
			k2 = (4.0f - 8.0f * e) / d;
			k3 = (2.0f * e) / d;

			previousSolution = new Vector3[n * m];
			currentSolution = new Vector3[n * m];

			vertices = new Vertex[VertexCount];

			for (int i = 0; i < VertexCount; i++)
			{
				vertices[i] = new Vertex();
			}

			float width = (n - 1) * dx;
			float depth = (m - 1) * dx;
			float halfWidth = width * 0.5f;
			float halfDepth = depth * 0.5f;

			for (int i = 0; i < m; i++)
			{
				float z = halfDepth - i * dx;

				for (int j = 0; j < n; j++)
				{
					float x = -halfWidth + j * dx;

					previousSolution[i * n + j] = new Vector3(x, 0.0f, z);
					currentSolution[i * n + j] = new Vector3(x, 0.0f, z);

					vertices[i * n + j].Position = currentSolution[i * n + j];
					vertices[i * n + j].Normal = Vector3.Up;
					vertices[i * n + j].TexCoord = new Vector2(j * dx / width, i * dx / depth);
				}
			}
		}

		public void Update(float dt)
		{
			// Accumulate time.
			time += dt;

			// Only update the simulation at the specified time step.
			if ( time >= timeStep )
			{
				// Only update interior points; we use zero boundary conditions.
				for (int i = 1; i < RowCount - 1; i++)
				{
					for (int j = 1; j < ColumnCount - 1; j++)
					{
						// After this update we will be discarding the old previous
						// buffer, so overwrite that buffer with the new update.
						// Note how we can do this inplace (read/write to same element) 
						// because we won't need prev_ij again and the assignment happens last.

						// Note j indexes x and i indexes z: h(x_j, z_i, t_k)
						// Moreover, our +z axis goes "down"; this is just to 
						// keep consistent with our row indices going down.

						float a1 = k1 * previousSolution[i * ColumnCount + j].Y;
						float a2 = k2 * currentSolution[i * ColumnCount + j].Y;
						float b1 = currentSolution[(i + 1) * ColumnCount + j].Y;
						float b2 = currentSolution[(i - 1) * ColumnCount + j].Y;
						float b3 = currentSolution[i * ColumnCount + j + 1].Y;
						float b4 = currentSolution[i * ColumnCount + j - 1].Y;

						previousSolution[i * ColumnCount + j].Y = a1 + a2 + k3 * (b1 + b2 + b3 + b4);

						vertices[i * ColumnCount + j].Position = previousSolution[i * ColumnCount + j];
					}
				}

				// We just overwrote the previous buffer with the new data, so
				// this data needs to become the current solution and the old
				// current solution becomes the new previous solution.
				Vector3[] work = previousSolution;
				previousSolution = currentSolution;
				currentSolution = work;
				work = null;

				// reset time
				time = 0;

				//
				// Compute normals using finite difference scheme.
				//
				for (int i = 1; i < RowCount - 1; ++i)
				{
					for (int j = 1; j < ColumnCount - 1; ++j)
					{
						float l = currentSolution[i * ColumnCount + j - 1].Y;
						float r = currentSolution[i * ColumnCount + j + 1].Y;
						float t = currentSolution[(i - 1) * ColumnCount + j].Y;
						float b = currentSolution[(i + 1) * ColumnCount + j].Y;

						Vector3 n = new Vector3(-r + l, 2.0f *  spatialStep, b - t);

						vertices[i * ColumnCount + j].Normal = Vector3.Normalize(n);
					}
				}
			}
		}

		public void Disturb(int i, int j, float magnitude)
		{
			// Don't disturb boundaries.
			if (!((i > 1 && i < RowCount - 2) || (j > 1 && j < ColumnCount - 2)))
				throw new Exception("Don't disturb boundaries.");

			float halfMag = 0.5f * magnitude;
			
			// Disturb the ijth vertex height and its neighbors.
			currentSolution[i * ColumnCount + j].Y += magnitude;
			currentSolution[i * ColumnCount + j + 1].Y += halfMag;
			currentSolution[i * ColumnCount + j - 1].Y += halfMag;
			currentSolution[(i + 1) * ColumnCount + j].Y += halfMag;
			currentSolution[(i - 1) * ColumnCount + j].Y += halfMag;
		}

		
		public Vector3 this[int index]
		{
			get { return currentSolution[index]; }
		}
	}
}
