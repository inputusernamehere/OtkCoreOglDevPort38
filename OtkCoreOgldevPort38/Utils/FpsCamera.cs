using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common.Input;
using System;

namespace OtkCoreOgldevPort38.Utils
{
	public class FpsCamera
	{
		public Vector2 MouseLastPosition;
		public float MouseSensitivity = 0.2f;

		public float CameraYaw = MathHelper.PiOver2 * -1;
		public float CameraPitch;

		public float CameraFov => MathUtils.FovxToFovy(MathHelper.DegreesToRadians(90), Width, Height);

		public float AspectRatio { get => Height == 0 ? 1 : ((float)Width) / ((float)Height); }
		// set in constructor
		public int Width = 0;
		public int Height = 0;

		public Vector3 CameraPosition;
		public Vector3 CameraFront = Vector3.UnitZ * -1;
		public Vector3 CameraUp = Vector3.UnitY;
		public Vector3 CameraRight = Vector3.UnitX;
		public float CameraSpeed = 0.05f;

		public Matrix4 View { get => Matrix4.LookAt(CameraPosition, CameraPosition + CameraFront, CameraUp); }
		public Matrix4 Projection { get => Matrix4.CreatePerspectiveFieldOfView(CameraFov, AspectRatio, 0.01f, 100f); }

		public FpsCamera(int width, int height)
		{
			Width = width;
			Height = height;

			RecalculateCamera(0, 0);
		}

		public void MoveByMouse(MouseState mouse)
		{
			float deltaX = mouse.X - MouseLastPosition.X;
			float deltaY = mouse.Y - MouseLastPosition.Y;
			MouseLastPosition = new Vector2(mouse.X, mouse.Y);

			if (!mouse.IsButtonDown(MouseButton.Middle))
			{
				return;
			}

			RecalculateCamera(deltaX, deltaY);
		}

		public void RecalculateCamera(float deltaX, float deltaY)
		{
			CameraYaw += deltaX * MouseSensitivity;
			CameraPitch -= deltaY * MouseSensitivity;
			CameraPitch = MathHelper.Clamp(CameraPitch, -89f, 89f);

			CameraFront.X = (float)Math.Cos(MathHelper.DegreesToRadians(CameraPitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(CameraYaw));
			CameraFront.Y = (float)Math.Sin(MathHelper.DegreesToRadians(CameraPitch));
			CameraFront.Z = (float)Math.Cos(MathHelper.DegreesToRadians(CameraPitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(CameraYaw));
			CameraFront = Vector3.Normalize(CameraFront);

			CameraRight = Vector3.Normalize(Vector3.Cross(CameraFront, Vector3.UnitY));
			CameraUp = Vector3.Normalize(Vector3.Cross(CameraRight, CameraFront));
		}

		public void MoveByKeyboard(KeyboardState keyboard, float deltaTime)
		{
			if (keyboard.IsKeyDown(Key.W))
			{
				CameraPosition += CameraFront * CameraSpeed * deltaTime;
			}

			if (keyboard.IsKeyDown(Key.S))
			{
				CameraPosition -= CameraFront * CameraSpeed * deltaTime;
			}

			if (keyboard.IsKeyDown(Key.A))
			{
				CameraPosition -= Vector3.Normalize(Vector3.Cross(CameraFront, CameraUp)) * CameraSpeed * deltaTime;
			}

			if (keyboard.IsKeyDown(Key.D))
			{
				CameraPosition += Vector3.Normalize(Vector3.Cross(CameraFront, CameraUp)) * CameraSpeed * deltaTime;
			}

			if (keyboard.IsKeyDown(Key.Space))
			{
				CameraPosition += CameraUp * CameraSpeed * deltaTime;
			}

			if (keyboard.IsKeyDown(Key.LShift))
			{
				CameraPosition -= CameraUp * CameraSpeed * deltaTime;
			}
		}
	}
}
