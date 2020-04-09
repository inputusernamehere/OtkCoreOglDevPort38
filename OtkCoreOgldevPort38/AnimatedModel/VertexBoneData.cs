using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OtkCoreOgldevPort38.AnimatedModel
{
	// I had to use fixed-size buffers to make the code (almost) identical to the C++ version.

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct VertexBoneData
	{
		public static readonly int NumBonesPerVertex = 4;

		[FieldOffset(0)]
		public fixed int Ids[4];
		[FieldOffset(sizeof(int) * 4)]
		public fixed float Weights[4];

		public static void AddBoneData(ref VertexBoneData vbd, int boneId, float weight)
		{
			for (int i = 0; i < NumBonesPerVertex; i++)
			{
				if (vbd.Weights[i] == 0.0f)
				{
					vbd.Ids[i] = boneId;
					vbd.Weights[i] = weight;

					return;
				}
			}
		}
	}
}
