using System;
using System.Windows.Forms;
using SharpDX;
using SharpDXCommons;
using SolarSystemDemo.GeoMath;

namespace SolarSystemDemo.Objects.Demo.PlayerControls
{
	public enum PlayerCameraModes
	{
		FirstPerson,
		Orbital,
		WorldOrbital
	}

	public class PlayerCamera
	{
		private int LastMouseX;
		private int LastMouseY;

		// !!! Избавиться от этого
		public virtual Vector3 Position
		{ get { return Vector3.Zero; } set { } }
		//{ get; set; }

		private Vector3 AxisX;
		private Vector3 AxisY;
		private Vector3 AxisZ;

		private float Radius;
		private float Phi = 2 * MathUtil.Pi / 3;
		private float Theta = -MathUtil.Pi / 6;

		private PlayerCameraModes CameraMode;

		public PlayerCamera(float radiusMin, float radiusMax, float radius)
		{
			Radius = radius;

			AxisX = Vector3.Right;
			AxisY = Vector3.Up;
			AxisZ = Vector3.BackwardRH;

			CameraMode = PlayerCameraModes.WorldOrbital;
		}

		#region Control Handlers

		public void InstallControls(RenderWindow renderWindow)
		{
			renderWindow.MouseMove += OnMouseMove;
			renderWindow.MouseWheel += OnMouseWheel;
			renderWindow.KeyDown += OnKeyDown;
		}

		public void RemoveControls(RenderWindow renderWindow)
		{
			renderWindow.MouseMove -= OnMouseMove;
			renderWindow.MouseWheel -= OnMouseWheel;
			renderWindow.KeyDown -= OnKeyDown;
		}

		public void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Tab)
			{
				if (CameraMode == PlayerCameraModes.WorldOrbital)
				{
					CameraMode = PlayerCameraModes.Orbital;
				}
				else
				{
					CameraMode = PlayerCameraModes.WorldOrbital;
				}
			}
		}

		public void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
			{
				Phi += -0.25f * (e.X - LastMouseX) * MathUtil.Pi / 180;
				Theta += -0.25f * (e.Y - LastMouseY) * MathUtil.Pi / 180;

				Theta = NiceFunctions.Clamp(Theta, -MathUtil.PiOverTwo + 0.05f, MathUtil.PiOverTwo - 0.05f);
				Phi = NiceFunctions.Wrap(Phi, 0f, MathUtil.TwoPi);
			}

			LastMouseX = e.X;
			LastMouseY = e.Y;
		}

		public void OnMouseWheel(object sender, MouseEventArgs e)
		{
            Radius -= (e.Delta / Math.Abs(e.Delta)) * 3;
			Radius = TwaMath.Clamp(Radius, 1, 100000); // !!!
		}

		#endregion Control Handlers

		public Matrix GetViewMatrix()
		{
			Vector3 eye = GetEyePosition();
			Vector3 target = Position;

			Vector3 axisY = AxisY;

			if (CameraMode == PlayerCameraModes.WorldOrbital)
			{
				axisY = Vector3.Up;
			}

			return Matrix.LookAtLH(eye, target, axisY);
		}

		public Vector3 GetEyePosition()
		{
			Vector3 axisX = AxisX;
			Vector3 axisY = AxisY;
			Vector3 axisZ = AxisZ;

			if (CameraMode == PlayerCameraModes.WorldOrbital)
			{
				axisX = Vector3.Right;
				axisY = Vector3.Up;
				axisZ = Vector3.BackwardRH;
			}

			Quaternion q = Quaternion.RotationAxis(axisY, -Phi) * Quaternion.RotationAxis(axisX, Theta);
			Vector3 direction = TwaMath.RotateVector(axisZ, q);

			return Position + Vector3.Normalize(direction) * Radius;
		}


		public void Update(Vector3 axisX, Vector3 axisY, Vector3 axisZ)
		{
			AxisX = axisX;
			AxisY = axisY;
			AxisZ = axisZ;
		}
	}
}
