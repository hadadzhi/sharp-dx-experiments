using SharpDX;

namespace CascadedShadowMaps
{
	public struct AABB
	{
		public Vector3 Min;
		public Vector3 Max;

		public AABB(Vector3[] sourcePoints) : this(sourcePoints, 0, sourcePoints.Length) { }

		public AABB(Vector3[] sourcePoints, int offset, int count)
		{
			Min = sourcePoints[offset];
			Max = sourcePoints[offset];

			for (int i = 0; i < count; i++)
			{
				Min = Vector3.Min(Min, sourcePoints[i + offset]);
				Max = Vector3.Max(Max, sourcePoints[i + offset]);
			}
		}
	}
}
