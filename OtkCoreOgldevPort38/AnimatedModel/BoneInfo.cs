using Assimp;
using System;
using System.Collections.Generic;
using System.Text;

namespace OtkCoreOgldevPort38.AnimatedModel
{
	public struct BoneInfo
	{
		public Matrix4x4 BoneOffset;
		public Matrix4x4 FinalTransformation;
	}
}
