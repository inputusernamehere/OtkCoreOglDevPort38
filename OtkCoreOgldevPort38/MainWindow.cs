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
			Camera.CameraPosition = new OpenToolkit.Mathematics.Vector3(-3.0f, -2.0f, -2.0f);
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

			// Camera MVP ---

			var model = OpenToolkit.Mathematics.Matrix4.Identity;
			var view = Camera.View;
			var projection = Camera.Projection;

			var wvp = model * view * projection;
			//var wvp = projection * view * model;

			// test
			var testwvp = new OpenToolkit.Mathematics.Matrix4(
				-0.1f, 0, -8.7E-9f, 0,
				-8.7E-9f, 1.1E-9f, 0.1f, 0,
				1.0E-16f, 0.1f, -1.1f, 0,
				0, 0, 6, 1);
			var testwvp2 = new OpenToolkit.Mathematics.Matrix4(
				-0.1f, -8.7E-9f, 1.0E-16f, 0,
				0, 1.1E-9f, 0.1f, 0,
				-8.7E-9f, 0.1f, -1.1f, 6,
				0, 0, 0, 1);
			var testwvp3 = new OpenToolkit.Mathematics.Matrix4(
				-0.12f, 0, -0.05f, -0.05f,
				-0.06f, 0, 0.08f, 0.08f,
				8.3E-10f, 0.17f, 0, 0,
				-4.8f, -5.3f, 4, 6);

			var wvpLocation = GL.GetUniformLocation(ShaderProgram, "gWVP");
			//GL.UniformMatrix4(wvpLocation, false, ref wvp);
			//GL.UniformMatrix4(wvpLocation, false, ref testwvp);
			//GL.UniformMatrix4(wvpLocation, false, ref testwvp2);
			GL.UniformMatrix4(wvpLocation, false, ref testwvp3);

			// Mesh transforms

			var transforms = new List<OpenToolkit.Mathematics.Matrix4>();
			for (int i = 0; i < Mesh.NumBones; i++)
			{
				transforms.Add(new OpenToolkit.Mathematics.Matrix4());
			}

			var runningTime = 0f;

			//Mesh.BoneTransforms((float)RunningTime, ref transforms);
			Mesh.BoneTransforms((float)runningTime, ref transforms);

			var identity = OpenToolkit.Mathematics.Matrix4.Identity;

			// Max number of bones in shader
			for (int i = 0; i < 100; i++)
			{
				var m = OpenToolkit.Mathematics.Matrix4.Identity;

				if (i < transforms.Count)
				{
					m = transforms[i];
				}

				var location = GL.GetUniformLocation(ShaderProgram, $"gBones[{i}]");
				GL.UniformMatrix4(location, false, ref m);

				//GL.UniformMatrix4(location, false, ref identity);
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
