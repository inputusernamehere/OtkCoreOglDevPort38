using OpenToolkit.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace OtkCoreOgldevPort38.Utils
{
	public static class Util
	{
		public static bool GLCheckError()
		{
			return GL.GetError() == ErrorCode.NoError;
		}

		public static OpenToolkit.Mathematics.Matrix4 ToOtk(this Assimp.Matrix4x4 m)
		{
			var ret = new OpenToolkit.Mathematics.Matrix4();

			ret.M11 = m.A1;
			ret.M12 = m.B1;
			ret.M13 = m.C1;
			ret.M14 = m.D1;
			
			ret.M21 = m.A2;
			ret.M22 = m.B2;
			ret.M23 = m.C2;
			ret.M24 = m.D2;

			ret.M31 = m.A3;
			ret.M32 = m.B3;
			ret.M33 = m.C3;
			ret.M34 = m.D3;

			ret.M41 = m.A4;
			ret.M42 = m.B4;
			ret.M43 = m.C4;
			ret.M44 = m.D4;

			return ret;
		}

		public static OpenToolkit.Mathematics.Quaternion ToOtk(this Assimp.Quaternion q)
		{
			var ret = new OpenToolkit.Mathematics.Quaternion();

			ret.X = q.X;
			ret.Y = q.Y;
			ret.Z = q.Z;
			ret.W = q.W;

			return ret;
		}

		public static OpenToolkit.Mathematics.Vector3 ToOtk(this Assimp.Vector3D v)
		{
			var ret = new OpenToolkit.Mathematics.Vector3();

			ret.X = v.X;
			ret.Y = v.Y;
			ret.Z = v.Z;

			return ret;
		}
	}
}
