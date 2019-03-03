using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;

namespace SharpDXCommons
{
	public abstract class SceneObject
	{
		public Matrix WorldMatrix { get; protected set; }

		public Vector3 Position = Vector3.Zero;
		public Vector3 Rotation = Vector3.Zero;
		public Vector3 Scaling = Vector3.One;

		public bool AutoUpdateWorldMatrix = true;
		public bool NeedWorldMatrixUpdate = false;

		internal virtual void UpdateWorldMatrix()
		{
			NeedWorldMatrixUpdate = false;
			WorldMatrix = 
				Matrix.Scaling(Scaling) * 
				Matrix.RotationYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) * 
				Matrix.Translation(Position);
		}
	}
}
