using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace WavesDemo
{
	public class Waves
	{
		public uint RowCount { get; private set; }
		public uint ColumnCount { get; private set; }

		public uint VertexCount { get; private set; }
		public uint TriangleCount { get; private set; }

		// Simulation constants we can precompute.
		private double k1;
		private double k2;
		private double k3;

		private double timeStep;
		private double spatialStep;

		private Vector3[] previousSolution;
		private Vector3[] currentSolution;

		private double time = 0;

		public Waves(uint m, uint n, float dx, float dt, float speed, float damping)
		{
			RowCount = m;
			ColumnCount = n;

			VertexCount = n * m;
			TriangleCount = (m - 1) * (n - 1) * 2;

			timeStep = dt;
			spatialStep = dx;

			double d = damping * dt + 2.0f;
			double e = (speed * speed) * (dt * dt) / (dx * dx);
			k1 = (damping * dt - 2.0f) / d;
			k2 = (4.0f - 8.0f * e) / d;
			k3 = (2.0f * e) / d;

			previousSolution = new Vector3[n * m];
			currentSolution = new Vector3[n * m];

			double halfWidth = (n - 1) * dx * 0.5f;
			double halfDepth = (m - 1) * dx * 0.5f;

			for (uint i = 0; i < m; i++)
			{
				double z = halfDepth - i * dx;

				for (uint j = 0; j < n; j++)
				{
					double x = -halfWidth + j * dx;

					previousSolution[i * n + j] = new Vector3((float)x, 0.0f, (float)z);
					currentSolution[i * n + j] = new Vector3((float)x, 0.0f, (float)z);
				}
			}
		}

		public void Update(double dt)
		{
			// Accumulate time.
			time += dt;

			// Only update the simulation at the specified time step.
			if ( time >= timeStep )
			{
				// Only update interior points; we use zero boundary conditions.
				for (ulong i = 1; i < RowCount - 1; i++)
				{
					for (ulong j = 1; j < ColumnCount - 1; j++)
					{
						// After this update we will be discarding the old previous
						// buffer, so overwrite that buffer with the new update.
						// Note how we can do this inplace (read/write to same element) 
						// because we won't need prev_ij again and the assignment happens last.

						// Note j indexes x and i indexes z: h(x_j, z_i, t_k)
						// Moreover, our +z axis goes "down"; this is just to 
						// keep consistent with our row indices going down.

						double a1 = k1 * previousSolution[i * ColumnCount + j].Y;
						double a2 = k2 * currentSolution[i * ColumnCount + j].Y;
						double b1 = currentSolution[(i + 1) * ColumnCount + j].Y;
						double b2 = currentSolution[(i - 1) * ColumnCount + j].Y;
						double b3 = currentSolution[i * ColumnCount + j + 1].Y;
						double b4 = currentSolution[i * ColumnCount + j - 1].Y;

						previousSolution[i * ColumnCount + j].Y = 
							(float)(a1 + a2 + k3 * (b1 + b2 + b3 + b4));
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
				for (ulong i = 1; i < RowCount - 1; ++i)
				{
					for (ulong j = 1; j < ColumnCount - 1; ++j)
					{
						float l = currentSolution[i * ColumnCount + j - 1].Y;
						float r = currentSolution[i * ColumnCount + j + 1].Y;
						float t = currentSolution[(i - 1) * ColumnCount + j].Y;
						float b = currentSolution[(i + 1) * ColumnCount + j].Y;
					}
				}
			}
		}

		public void Disturb(uint i, uint j, float magnitude)
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
