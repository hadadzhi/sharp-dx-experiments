using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpDX;

namespace SharpDXCommons.Cameras
{
	public class OrbitalControls
	{
		private RenderWindow Window;

		private int LastMouseX;
		private int LastMouseY;

		public Vector3 Position;

		private float Radius;
		private float Phi = (float) -Math.PI / 3;
		private float Theta = (float) Math.PI / 3;

		private float RadiusMin;
		private float RadiusMax;

		public OrbitalControls(RenderWindow window, Vector3 pos, float radiusMin, float radiusMax, float radius)
		{
			Window = window;
			Position = pos;
			Radius = radius;
			RadiusMin = radiusMin;
			RadiusMax = radiusMax;
		}

		public void Install()
		{
			Window.MouseMove += OnMouseMove;
			Window.MouseWheel += OnMouseWheel;
		}

		public void Remove()
		{
			Window.MouseMove -= OnMouseMove;
			Window.MouseWheel -= OnMouseWheel;
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				// Make each pixel correspond to a quarter of a degree.
				Phi -= 0.25f * (e.X - LastMouseX) * (float) Math.PI / 180;
				Theta -= 0.25f * (e.Y - LastMouseY) * (float) Math.PI / 180;

				Theta = NiceFunctions.Clamp(Theta, 0.01f, (float) Math.PI - 0.01f);
				Phi = NiceFunctions.Wrap(Phi, 0f, (float) (2 * Math.PI));
			}
			else if (e.Button == (MouseButtons.Left | MouseButtons.Right))
			{
				Radius += (e.Y - LastMouseY) * (RadiusMax - RadiusMin) * 0.001f;
				Radius = NiceFunctions.Clamp(Radius, RadiusMin, RadiusMax);
			}

			LastMouseX = e.X;
			LastMouseY = e.Y;
		}

		private void OnMouseWheel(object sender, MouseEventArgs e)
		{
			Radius -= (e.Delta / Math.Abs(e.Delta)) * (RadiusMax - RadiusMin) * 0.02f;
			Radius = NiceFunctions.Clamp(Radius, RadiusMin, RadiusMax);
		}

		public Matrix GetViewMatrix()
		{
			Vector3 eye = GetCameraPosition();
			Vector3 target = Position;
			Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);

			return Matrix.LookAtLH(eye, target, up);
		}

		public Vector3 GetCameraPosition()
		{
			float x = Position.X + (float) (Radius * Math.Sin(Theta) * Math.Cos(Phi));
			float z = Position.Z + (float) (Radius * Math.Sin(Theta) * Math.Sin(Phi));
			float y = Position.Y + Radius * (float) Math.Cos(Theta);

			return new Vector3(x, y, z);
		}

		public Vector3 GetCameraDirection()
		{
			return Position - GetCameraPosition();
		}

		public Vector3 GetOrigin()
		{
			return Position;
		}
	}
}
