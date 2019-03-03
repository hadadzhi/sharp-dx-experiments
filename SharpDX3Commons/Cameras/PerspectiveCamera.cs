using SharpDX;

namespace SharpDXCommons.Cameras
{
	public class PerspectiveCamera : Camera
	{
		private float _VerticalFOV;
		private float _AspectRatio;
		private float _Near;
		private float _Far;

		public float VerticalFOV
		{
			get
			{
				return _VerticalFOV;
			}
			set
			{
				_VerticalFOV = value;
				NeedProjectionMatrixUpdate = true;
			}
		}

		public float AspectRatio
		{
			get
			{
				return _AspectRatio;
			}
			set
			{
				_AspectRatio = value;
				NeedProjectionMatrixUpdate = true;
			}
		}

		public float Near
		{
			get
			{
				return _Near;
			}
			set
			{
				_Near = value;
				NeedProjectionMatrixUpdate = true;
			}
		}

		public float Far
		{
			get
			{
				return _Far;
			}
			set
			{
				_Far = value;
				NeedProjectionMatrixUpdate = true;
			}
		}

		/// <summary>
		/// Initializes a new PerspectiveCamera instance.
		/// </summary>
		/// <param name="vfov">Vertical field of view in radians</param>
		/// <param name="aspect">Aspect ratio</param>
		/// <param name="near">Near plane coordinate</param>
		/// <param name="far">Far plane coordinate</param>
		public PerspectiveCamera(float vfov, float aspect, float near, float far)
		{
			VerticalFOV = vfov;
			AspectRatio = aspect;
			Near = near;
			Far = far;
		}

		internal override void UpdateProjectionMatrix()
		{
			ProjectionMatrix = Matrix.PerspectiveFovLH(_VerticalFOV, _AspectRatio, _Near, _Far);
		}
	}
}
