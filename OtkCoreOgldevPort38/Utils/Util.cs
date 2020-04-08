using OpenToolkit.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace OtkCoreOgldevPort38.Utils
{
	public class Util
	{
		public static bool GLCheckError()
		{
			return GL.GetError() == ErrorCode.NoError;
		}
	}
}
