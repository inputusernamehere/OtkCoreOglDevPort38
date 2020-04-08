using System;

namespace OtkCoreOgldevPort38.Utils
{
	public static class MathUtils
	{
		public static float FovyToFovx(float fovyRads, float width, float height)
		{
			var ret = 2f * (float)Math.Atan(Math.Tan(fovyRads * 0.5) * (width / height));
			return ret;
		}

		public static float FovxToFovy(float fovxRads, float width, float height)
		{
			var ret = 2f * (float)Math.Atan(Math.Tan(fovxRads * 0.5) * (height / width));
			return ret;
		}
	}
}
