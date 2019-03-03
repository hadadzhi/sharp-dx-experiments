using SharpDX;
using System;

namespace CascadedShadowMaps
{
	public class CSMCameraFrustum
	{
		private Vector3[] Points;

		public float[] SplitPositions { get; private set; }

		public void Build(float near, float far, float verticalfov, float aspectRatio, Vector3 cameraPosition, Vector3 cameraViewDirection, int numSplits, float lambda)
		{
			Points = new Vector3[(numSplits + 1) * 4];
			CalculateSplitPositions(near, far, numSplits, lambda);

			Vector3 vZ = cameraViewDirection;
			Vector3 vX = Vector3.Normalize(Vector3.Cross(Vector3.Up, vZ));
			Vector3 vY = Vector3.Normalize(Vector3.Cross(vZ, vX));

			float tanHalfVFov = (float) Math.Tan(verticalfov * 0.5f);

			for (int i = 0, j = 0; i < numSplits + 1; i++)
			{
				float SplitPlaneHalfHeight = tanHalfVFov * SplitPositions[i];
				float SplitPlaneHalfWidth = SplitPlaneHalfHeight * aspectRatio;

				Vector3 SplitPlaneCenter = Vector3.Add(cameraPosition, Vector3.Multiply(vZ, SplitPositions[i]));

				Points[j++] = (SplitPlaneCenter - (vX * SplitPlaneHalfWidth)) - (vY * SplitPlaneHalfHeight);
				Points[j++] = (SplitPlaneCenter - (vX * SplitPlaneHalfWidth)) + (vY * SplitPlaneHalfHeight);
				Points[j++] = (SplitPlaneCenter + (vX * SplitPlaneHalfWidth)) + (vY * SplitPlaneHalfHeight);
				Points[j++] = (SplitPlaneCenter + (vX * SplitPlaneHalfWidth)) - (vY * SplitPlaneHalfHeight);
			}
		}

		public Matrix[] CalculateCropMatrices(Matrix lightViewProj, float lightNear)
		{
			Matrix[] result = new Matrix[Points.Length / 4 - 1];
			Vector3[] lightSpacePoints = new Vector3[Points.Length];

			// Transform points to light's projection space
			for (int i = 0; i < Points.Length; i++)
			{
				Vector4 t = Vector4.Transform(new Vector4(Points[i], 1.0f), lightViewProj);

				t.X /= t.W;
				t.Y /= t.W;
				t.Z /= t.W;

				lightSpacePoints[i] = new Vector3(t.X, t.Y, t.Z);
			}
			
			for (int i = 0; i < result.Length; i++)
			{
				AABB aabb = new AABB(lightSpacePoints, 4 * i, 8);

				// Override cropped light near plane to include possible off-screen casters
				aabb.Min.Z = lightNear;

				result[i] = BuildCropMatrix(aabb);
			}

			return result;
		}
		
		/// <param name="bb">Frustum slice's AABB in light's projection space</param>
		private Matrix BuildCropMatrix(AABB bb)
		{
			float left = bb.Min.X;
			float right = bb.Max.X;
			float bottom = bb.Min.Y;
			float top = bb.Max.Y;
			float near = bb.Min.Z;
			float far = bb.Max.Z;

			float width = bb.Max.X - bb.Min.X;
			float height = bb.Max.Y - bb.Min.Y;
			float texelSizeX = width / CSMDemo.CSMShadowMapSize;
			float texelSizeY = height / CSMDemo.CSMShadowMapSize;

			left /= texelSizeX;
			left = (float) Math.Floor(left);
			left *= texelSizeX;
			
			right /= texelSizeX;
			right = (float) Math.Floor(right);
			right *= texelSizeX;
			
			top /= texelSizeY;
			top = (float) Math.Floor(top);
			top *= texelSizeY;
			
			bottom /= texelSizeY;
			bottom = (float) Math.Floor(bottom);
			bottom *= texelSizeY;

			return Matrix.OrthoOffCenterLH(left, right, bottom, top, near, far);
		}
		
		/// <param name="m">Number of splits</param>
		/// <param name="lambda">Controls the scaling between uniform and logarithmic split distributions. 1 -- purely logarithmic, 0 -- purely uniform</param>
		/// <returns>(m + 1) split positions -- camera near and far planes included</returns>
		private void CalculateSplitPositions(float near, float far, int m, float lambda)
		{
			SplitPositions = new float[m + 1];

			// From 1-st to (m - 1)-th -- calculated split positions
			for (int i = 1; i < m; i++)
			{
				float idm = (float) i / m;
				float log = near * (float) Math.Pow(far / near, idm);
				float uni = near + (far - near) * idm;

				SplitPositions[i] = log * lambda + uni * (1 - lambda);
			}

			// 0-th and last split positions -- camera near and far planes
			SplitPositions[0] = near;
			SplitPositions[m] = far;
		}
	}
}
