using SharpDX;

namespace SharpDXCommons.Cameras
{
	public enum CameraMode
	{
		/// <summary>
		/// The camera's Position and Focus and the world's Up vector is used to calculate a conventional Look-At View matrix
		/// </summary>
		Focus,

		/// <summary>
		/// The camera's Position and Direction and the world's Up vector is used to calculate a conventional Look-At View matrix
		/// </summary>
		Direction,
		
		/// <summary>
		/// The camera behaves like a real-world camera. Like for any other SceneObject, you can specify its Position and Rotation.
		/// Scaling is used as a zoom factor.
		/// </summary>
		Object
	}

	public abstract class Camera : SceneObject
	{
		public Matrix ViewMatrix { get; protected set; }
		public Matrix ProjectionMatrix { get; protected set; }

		public bool NeedProjectionMatrixUpdate { get; protected set; }

		/// <summary>
		/// A vector representing a point in space at which this camera is looking. Used only if this camera's Mode is CameraMode.Focus
		/// </summary>
		public Vector3 Focus;

		/// <summary>
		/// A vector representing a direction in which this camera is looking. Used only if this camera's Mode is CameraMode.Direction
		/// </summary>
		public Vector3 Direction;

		/// <summary>
		/// Determines how the View matrix is calculated. Default is CameraMode.Focus
		/// </summary>
		public CameraMode Mode = CameraMode.Focus;

		internal override void UpdateWorldMatrix()
		{
			UpdateViewMatrix();
			NeedWorldMatrixUpdate = false;
		}

		internal void UpdateViewMatrix()
		{
			switch (Mode)
			{
				// TODO: Optimize matrix calculations
				case CameraMode.Object:
				{
					base.UpdateWorldMatrix();
					ViewMatrix = Matrix.Invert(WorldMatrix);
					break;
				}
				case CameraMode.Focus:
				{
					ViewMatrix = Matrix.LookAtLH(Position, Focus, Vector3.Up);
					break;
				}
				case CameraMode.Direction:
				{
					ViewMatrix = Matrix.LookAtLH(Position, Position + Direction, Vector3.Up);
					break;
				}
			}
		}

		internal abstract void UpdateProjectionMatrix();
	}
}
