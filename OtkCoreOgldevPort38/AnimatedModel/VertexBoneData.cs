using System;
using System.Collections.Generic;
using System.Text;

namespace OtkCoreOgldevPort38.AnimatedModel
{
	public struct VertexBoneData
	{
		public const int NumBonesPerVertex = 4;

		public int[] Ids;
		public float[] Weights;

		public void AddBoneData(int boneId, float weight)
		{
			if (Ids == null)
			{
				Ids = new int[NumBonesPerVertex];
				Weights = new float[NumBonesPerVertex];
			}

			for (int i = 0; i < NumBonesPerVertex; i++)
			{
				if (Weights[i] == 0.0f)
				{
					Ids[i] = boneId;
					Weights[i] = weight;

					return;
				}
			}
		}
	}
}
