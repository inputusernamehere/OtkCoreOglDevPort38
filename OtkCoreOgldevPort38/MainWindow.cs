using Assimp;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using OtkCoreOgldevPort38.AnimatedModel;
using OtkCoreOgldevPort38.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace OtkCoreOgldevPort38
{
	class MainWindow : GameWindow
	{
		FpsCamera Camera;

		SkinnedMesh Mesh = new SkinnedMesh();

		int ShaderProgram;
		double RunningTime = 0;

		public MainWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
			: base(gameWindowSettings, nativeWindowSettings)
		{
			Camera = new FpsCamera(Size.X, Size.Y);

			Camera.CameraPosition = new OpenToolkit.Mathematics.Vector3(0, -80, 40);
			Camera.CameraYaw = -90;
			Camera.CameraPitch = 90;
			Camera.RecalculateCamera(0, 0);
		}


		// Tutorial38::Init()
		protected override void OnLoad()
		{
			GL.LoadBindings(new GLFWBindingsContext());

			if (!Mesh.LoadMesh("Assets/boblampclean.md5mesh"))
			{
				Console.WriteLine("Mesh load failed");
			}

			ShaderProgram = ShaderUtils.CreateProgram("Shaders/skinning.vert", "Shaders/skinning.frag");

			base.OnLoad();
		}

		protected override void OnUpdateFrame(FrameEventArgs args)
		{
			RunningTime += args.Time;

			Camera.MoveByKeyboard(KeyboardState, (float)args.Time);
			Camera.MoveByMouse(MouseState);

			base.OnUpdateFrame(args);
		}

		// Tutorial38::RenderSceneCB()
		protected override void OnRenderFrame(FrameEventArgs args)
		{
			RunningTime += args.Time;

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

			GL.UseProgram(ShaderProgram);

			// Camera MVP

			var model = OpenToolkit.Mathematics.Matrix4.Identity;
			var view = Camera.View;
			var projection = Camera.Projection;

			var wvp = model * view * projection;

			var wvpLocation = GL.GetUniformLocation(ShaderProgram, "gWVP");
			GL.UniformMatrix4(wvpLocation, false, ref wvp);

			// Mesh transforms

			var transforms = new List<OpenToolkit.Mathematics.Matrix4>();
			for (int i = 0; i < Mesh.NumBones; i++)
			{
				transforms.Add(new OpenToolkit.Mathematics.Matrix4());
			}

			Mesh.BoneTransforms((float)RunningTime / 1000f, ref transforms);

			for (int i = 0; i < transforms.Count; i++)
			{
				var m = transforms[i];
				var location = GL.GetUniformLocation(ShaderProgram, $"gBones[{i}]");
				GL.UniformMatrix4(location, false, ref m);
			}

			Mesh.Render();

			SwapBuffers();

			base.OnRenderFrame(args);
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			Camera.Width = Size.X;
			Camera.Height = Size.Y;

			GL.Viewport(0, 0, Size.X, Size.Y);

			base.OnResize(e);
		}
	}
}
