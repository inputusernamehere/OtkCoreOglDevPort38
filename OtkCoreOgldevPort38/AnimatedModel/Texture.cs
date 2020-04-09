using System;
using System.Collections.Generic;
using System.Text;
using OpenToolkit.Graphics.OpenGL;

namespace OtkCoreOgldevPort38.AnimatedModel
{
	public class Texture
	{
		private TextureTarget TextureTarget;
		private int TextureObj;
		private string FileName;

		public Texture(TextureTarget textureTarget, string fileName)
		{
			TextureTarget = textureTarget;
			FileName = fileName;
		}

		bool Load()
		{
			// I won't bother loading textures in this example.
			return true;
		}

		internal void Bind(TextureUnit texture)
		{
			GL.ActiveTexture(texture);

			GL.BindTexture(TextureTarget, TextureObj);
		}
	}
}
