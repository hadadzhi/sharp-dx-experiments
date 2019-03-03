using SharpDX;
using System;
using System.Windows.Forms;

namespace SharpDXCommons.Cameras
{
	public class SimpleFirstPersonCamera
	{
		#region Properties
		public Vector3 Position;
		public float Speed { get; set; }
		public float SpeedMultiplier { get; set; }
		public Matrix ViewMatrix { get; private set; }
		public Vector3 ViewDirection { get; private set; }
		public Vector3 RightHandDirection { get; private set; }
		#endregion

		#region Fields
		private int LastMouseX;
		private int LastMouseY;

		private bool keysW = false;
		private bool keysA = false;
		private bool keysS = false;
		private bool keysD = false;
		private bool keysQ = false;
		private bool keysE = false;
		private bool keysF = false;

		private float Phi = MathUtil.PiOverTwo;
		private float Theta = MathUtil.PiOverTwo;
		#endregion

		#region Constants

		// Each pixel of mouse movement corresponds to this angle
		private const float CameraStep = 0.1f * MathUtil.Pi / 180.0f;

		#endregion

		public SimpleFirstPersonCamera(Vector3 position, float speed = 1.0f, float speedMultiplier = 10.0f)
		{
			Position = position;
			Speed = speed;
			SpeedMultiplier = speedMultiplier;

			Rotate(0, 0);
		}

		/// <summary>
		/// Walk/strafe/raise on WASDQE, fast forward on F, rotate on mouse movement with RMB pressed.
		/// </summary>
		/// <param name="control">The windows form control on which to listen for keyboard/mouse events</param>
		public void InstallControls(Control control)
		{
			control.MouseMove += OnMouseMove;
			control.KeyDown += OnKeyDown;
			control.KeyUp += OnKeyUp;
			control.LostFocus += OnLostFocus;
		}

		public void RemoveControls(Control control)
		{
			control.MouseMove -= OnMouseMove;
			control.KeyDown -= OnKeyDown;
			control.KeyUp -= OnKeyUp;
			control.LostFocus -= OnLostFocus;
		}

		public void Walk(float d)
		{
			Position = Vector3.Add(Position, Vector3.Multiply(ViewDirection, d));
		}

		public void Strafe(float d)
		{
			Position = Vector3.Add(Position, Vector3.Multiply(RightHandDirection, d));
		}

		public void Raise(float d)
		{
			Position = Vector3.Add(Position, Vector3.Multiply(Vector3.Up, d));
		}

		public void Rotate(float deltaPhi, float deltaTheta)
		{
			Phi = MathUtil.Wrap(Phi + deltaPhi, 0f, 2 * MathUtil.Pi);
			Theta = MathUtil.Clamp(Theta + deltaTheta, 0.001f, MathUtil.Pi - 0.001f);

			ViewDirection = new Vector3(
				(float) (Math.Sin(Theta) * Math.Cos(Phi)),
				(float) (Math.Cos(Theta)),
				(float) (Math.Sin(Theta) * Math.Sin(Phi))
			);

			RightHandDirection = new Vector3(
				(float) (Math.Cos(Phi - (MathUtil.PiOverTwo))),
				0.0f,
				(float) (Math.Sin(Phi - (MathUtil.PiOverTwo)))
			);
		}

		/// <summary>
		/// Must be called each frame to update the view matrix.
		/// </summary>
		/// <param name="delta">Seconds since the last frame</param>
		public void Update(float delta)
		{
			if (keysF) { delta *= SpeedMultiplier; }

			Vector3 deltaPos = Vector3.Zero;

			if (keysD) { deltaPos.X += 1.0f; }
			if (keysA) { deltaPos.X -= 1.0f; }
			if (keysW) { deltaPos.Z += 1.0f; }
			if (keysS) { deltaPos.Z -= 1.0f; }
			if (keysQ) { deltaPos.Y += 1.0f; }
			if (keysE) { deltaPos.Y -= 1.0f; }

			deltaPos.Normalize();
			deltaPos = Vector3.Multiply(deltaPos, Speed * delta);

			Strafe(deltaPos.X);
			Raise(deltaPos.Y);
			Walk(deltaPos.Z);

			ViewMatrix = Matrix.LookAtLH(Position, Position + ViewDirection, Vector3.Up);
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				Rotate((LastMouseX - e.X) * CameraStep, (e.Y - LastMouseY) * CameraStep);
			}
			LastMouseX = e.X;
			LastMouseY = e.Y;
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.W: { keysW = true; break; }
				case Keys.A: { keysA = true; break; }
				case Keys.S: { keysS = true; break; }
				case Keys.D: { keysD = true; break; }
				case Keys.Q: { keysQ = true; break; }
				case Keys.E: { keysE = true; break; }
				case Keys.F: { keysF = true; break; }
			}
		}

		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.W: { keysW = false; break; }
				case Keys.A: { keysA = false; break; }
				case Keys.S: { keysS = false; break; }
				case Keys.D: { keysD = false; break; }
				case Keys.Q: { keysQ = false; break; }
				case Keys.E: { keysE = false; break; }
				case Keys.F: { keysF = false; break; }
			}
		}

		private void OnLostFocus(object sender, EventArgs e)
		{
			keysW = false;
			keysA = false;
			keysS = false;
			keysD = false;
			keysQ = false;
			keysE = false;
			keysF = false;
		}
	}
}
