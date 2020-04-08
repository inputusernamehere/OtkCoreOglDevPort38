using OpenToolkit.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtkCoreOgldevPort38.Utils
{
	public class ShaderUtils
	{
		private static int CompileShader(ShaderType type, string path)
		{
			var shader = GL.CreateShader(type);
			var src = File.ReadAllText(path);

			GL.ShaderSource(shader, src);
			GL.CompileShader(shader);

			var info = GL.GetShaderInfoLog(shader);
			if (!string.IsNullOrWhiteSpace(info))
			{
				Console.WriteLine($"GL.CompileShader [{type}] had info log: {info}");
			}

			return shader;
		}

		public static int CreateProgram(string vertexPath, string fragmentPath)
		{
			var program = GL.CreateProgram();
			var shaders = new List<int>();

			shaders.Add(CompileShader(ShaderType.VertexShader, vertexPath));
			shaders.Add(CompileShader(ShaderType.FragmentShader, fragmentPath));

			foreach (var shader in shaders)
			{
				GL.AttachShader(program, shader);
			}

			GL.LinkProgram(program);

			var info = GL.GetProgramInfoLog(program);
			if (!string.IsNullOrWhiteSpace(info))
			{
				Console.WriteLine($"GL.LinkProgram had info log: {info}");
			}

			foreach (var shader in shaders)
			{
				GL.DetachShader(program, shader);
				GL.DeleteShader(shader);
			}

			return program;
		}
	}
}
