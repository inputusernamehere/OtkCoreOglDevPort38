using Assimp;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Mathematics;
using OtkCoreOgldevPort38.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace OtkCoreOgldevPort38.AnimatedModel
{
	enum VB_TYPES
	{
		INDEX_BUFFER,
		POS_VB,
		NORMAL_VB,
		TEXCOORD_VB,
		BONE_VB,

		// The number of entries in this enum.
		NUM_VBs
	}

	class SkinnedMesh
	{
		public int NumBones { get; set; }

		private int Vao { get; set; }

		private int[] Buffers = new int[(int)VB_TYPES.NUM_VBs];

		List<MeshEntry> Entries = new List<MeshEntry>();
		List<Texture> Textures = new List<Texture>();

		Dictionary<string, int> BoneMapping = new Dictionary<string, int>();
		List<BoneInfo> BoneInfo = new List<BoneInfo>();
		Matrix4 GlobalInverseTransform;

		Scene Scene;
		AssimpContext Importer;

		const PostProcessSteps ASSIMP_LOAD_FLAGS = PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.FlipUVs | PostProcessSteps.JoinIdenticalVertices;

		public SkinnedMesh()
		{
		}

		public bool LoadMesh(string fileName)
		{
			Vao = GL.GenVertexArray();
			GL.BindVertexArray(Vao);

			GL.GenBuffers((int)VB_TYPES.NUM_VBs, Buffers);

			bool ret = false;

			Importer = new AssimpContext();
			Scene = Importer.ImportFile(fileName, ASSIMP_LOAD_FLAGS); ;

			if (Scene != null)
			{
				GlobalInverseTransform = Scene.RootNode.Transform.ToOtk();
				GlobalInverseTransform.Invert();

				ret = InitFromScene(Scene, fileName);
			}
			else
			{
				Console.WriteLine($"Error parsing {fileName}: {Scene}!");
			}

			GL.BindVertexArray(0);

			return ret;
		}

		public void Render()
		{
			GL.BindVertexArray(Vao);

			for (int i = 0; i < Entries.Count; i++)
			{
				var materialIndex = Entries[i].MaterialIndex;

				if (Textures[i] != null)
				{
					Textures[i].Bind(TextureUnit.Texture0);
				}

				var indices = new IntPtr(sizeof(uint) * Entries[i].BaseIndex);

				GL.DrawElementsBaseVertex(OpenToolkit.Graphics.OpenGL.PrimitiveType.Triangles,
					Entries[i].NumIndices,
					DrawElementsType.UnsignedInt,
					indices,
					Entries[i].BaseVertex);
			}

			GL.BindVertexArray(0);
		}

		internal void BoneTransforms(float timeInSeconds, ref List<Matrix4> transforms)
		{
			float ticksPerSecond = Scene.Animations[0].TicksPerSecond != 0 ?
				(float)Scene.Animations[0].TicksPerSecond : 25f;
			float timeInTicks = timeInSeconds * ticksPerSecond;
			float animationTime = timeInTicks % (float)Scene.Animations[0].DurationInTicks;

			ReadNodeHierarchy(animationTime, Scene.RootNode, Matrix4.Identity);

			for (int i = 0; i < NumBones; i++)
			{
				transforms[i] = BoneInfo[i].FinalTransformation;
			}
		}

		private void ReadNodeHierarchy(float animationTime, Node pNode, Matrix4 parentTransform)
		{
			var nodeName = pNode.Name;

			var pAnimation = Scene.Animations[0];

			var nodeTransformation = pNode.Transform.ToOtk();

			var pNodeAnim = FindNodeAnim(pAnimation, nodeName);

			if (pNodeAnim != null)
			{
				var scaling = CalcInterpolatedScaling(animationTime, pNodeAnim);
				var scalingM = Matrix4.CreateScale(scaling);

				var rotation = CalcInterpolatedRotation(animationTime, pNodeAnim);
				var rotationM = Matrix4.CreateFromQuaternion(rotation);

				var translation = CalcInterpolatedPosition(animationTime, pNodeAnim);
				var translationM = Matrix4.CreateTranslation(translation);

				// Note the reverse multiplication order
				nodeTransformation = scalingM * rotationM * translationM;
			}

			// Note the reverse multiplication order
			var globalTransformation = nodeTransformation * parentTransform;

			if (BoneMapping.ContainsKey(nodeName))
			{
				var boneIndex = BoneMapping[nodeName];

				var bi = BoneInfo[boneIndex];
				// Note the reverse multiplication order
				bi.FinalTransformation = bi.BoneOffset * globalTransformation * GlobalInverseTransform;
				BoneInfo[boneIndex] = bi;
			}

			for (int i = 0; i < pNode.ChildCount; i++)
			{
				ReadNodeHierarchy(animationTime, pNode.Children[i], globalTransformation);
			}
		}

		private Vector3 CalcInterpolatedScaling(float animationTime, NodeAnimationChannel pNodeAnim)
		{
			// TODO add asserts

			if (pNodeAnim.ScalingKeyCount == 1)
			{
				return pNodeAnim.ScalingKeys[0].Value.ToOtk();
			}

			var scalingIndex = FindScaling(animationTime, pNodeAnim);
			var nextScalingIndex = (scalingIndex + 1);

			var deltaTime = pNodeAnim.ScalingKeys[nextScalingIndex].Time - pNodeAnim.ScalingKeys[scalingIndex].Time;
			var factor = (animationTime - pNodeAnim.ScalingKeys[scalingIndex].Time) / deltaTime;

			var start = pNodeAnim.ScalingKeys[scalingIndex].Value;
			var end = pNodeAnim.ScalingKeys[nextScalingIndex].Value;
			var delta = end - start;

			return (start + ((float)factor * delta)).ToOtk();
		}

		private int FindScaling(float animationTime, NodeAnimationChannel pNodeAnim)
		{
			// TODO add asserts

			for (int i = 0; i < pNodeAnim.ScalingKeyCount - 1; i++)
			{
				if (animationTime < pNodeAnim.ScalingKeys[i + 1].Time)
				{
					return i;
				}
			}

			return 0;
		}

		private OpenToolkit.Mathematics.Quaternion CalcInterpolatedRotation(float animationTime, NodeAnimationChannel pNodeAnim)
		{
			// TODO add asserts

			if (pNodeAnim.RotationKeyCount == 1)
			{
				return pNodeAnim.RotationKeys[0].Value.ToOtk();
			}

			var rotationIndex = FindRotation(animationTime, pNodeAnim);
			var nextRotationIndex = rotationIndex + 1;

			var deltaTime = pNodeAnim.RotationKeys[nextRotationIndex].Time - pNodeAnim.RotationKeys[rotationIndex].Time;
			var factor = (animationTime - pNodeAnim.RotationKeys[rotationIndex].Time) / deltaTime;

			var startRotationQ = pNodeAnim.RotationKeys[rotationIndex].Value;
			var endRotationQ = pNodeAnim.RotationKeys[nextRotationIndex].Value;

			var interpolated = Assimp.Quaternion.Slerp(startRotationQ, endRotationQ, (float)factor);
			interpolated.Normalize();

			return interpolated.ToOtk();
		}

		private int FindRotation(float animationTime, NodeAnimationChannel pNodeAnim)
		{
			// TODO add asserts

			for (int i = 0; i < pNodeAnim.RotationKeyCount - 1; i++)
			{
				if (animationTime < pNodeAnim.RotationKeys[i + 1].Time)
				{
					return i;
				}
			}

			return 0;
		}

		private Vector3 CalcInterpolatedPosition(float animationTime, NodeAnimationChannel pNodeAnim)
		{
			// TODO add asserts

			if (pNodeAnim.PositionKeyCount == 1)
			{
				return pNodeAnim.PositionKeys[0].Value.ToOtk();
			}

			var positionIndex = FindPosition(animationTime, pNodeAnim);
			var nextPositionIndex = (positionIndex + 1);

			var deltaTime = pNodeAnim.PositionKeys[nextPositionIndex].Time - pNodeAnim.PositionKeys[positionIndex].Time;
			var factor = (animationTime - pNodeAnim.PositionKeys[positionIndex].Time) / deltaTime;

			var start = pNodeAnim.PositionKeys[positionIndex].Value;
			var end = pNodeAnim.PositionKeys[nextPositionIndex].Value;
			var delta = end - start;

			return (start + ((float)factor * delta)).ToOtk();
		}

		private int FindPosition(float animationTime, NodeAnimationChannel pNodeAnim)
		{
			// TODO add asserts

			for (int i = 0; i < pNodeAnim.PositionKeyCount - 1; i++)
			{
				if (animationTime < pNodeAnim.PositionKeys[i + 1].Time)
				{
					return i;
				}
			}

			return 0;
		}

		private NodeAnimationChannel FindNodeAnim(Animation pAnimation, string nodeName)
		{
			for (int i = 0; i < pAnimation.NodeAnimationChannelCount; i++)
			{
				var pNodeAnim = pAnimation.NodeAnimationChannels[i];

				if (pNodeAnim.NodeName == nodeName)
				{
					return pNodeAnim;
				}
			}

			return null;
		}

		private bool InitFromScene(Scene scene, string fileName)
		{
			List<Vector3> positions = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> texCoords = new List<Vector2>();
			List<VertexBoneData> bones = new List<VertexBoneData>();
			List<int> indices = new List<int>();

			int numVertices = 0;
			int numIndices = 0;

			for (int i = 0; i < Scene.MeshCount; i++)
			{
				var entry = new MeshEntry()
				{
					MaterialIndex = Scene.Meshes[i].MaterialIndex,
					NumIndices = Scene.Meshes[i].FaceCount * 3,
					BaseVertex = numVertices,
					BaseIndex = numIndices,
				};

				Entries.Add(entry);

				numVertices += Scene.Meshes[i].VertexCount;
				numIndices += entry.NumIndices;
			}

			// these are referred to by ID later, so init them here
			for (int i = 0; i < numVertices; i++)
			{
				bones.Add(new VertexBoneData());
			}

			for (int i = 0; i < Entries.Count; i++)
			{
				Mesh paiMesh = Scene.Meshes[i];
				InitMesh(i, paiMesh, ref positions, ref normals, ref texCoords, ref bones, ref indices);
			}

			if (!InitMaterials(Scene, fileName))
			{
				return false;
			}

			int v3size = 3 * sizeof(float);
			int v2size = 2 * sizeof(float);
			int boneSize = (4 * sizeof(int)) + (4 * sizeof(float));
			int iSize = sizeof(int);

			const int POSITION_LOCATION    = 0;
			const int TEX_COORD_LOCATION   = 1;
			const int NORMAL_LOCATION      = 2;
			const int BONE_ID_LOCATION     = 3;
			const int BONE_WEIGHT_LOCATION = 4;

			GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers[(int)VB_TYPES.POS_VB]);
			GL.BufferData(BufferTarget.ArrayBuffer, v3size * positions.Count, positions.ToArray(), BufferUsageHint.StaticDraw);
			GL.EnableVertexAttribArray(POSITION_LOCATION);
			GL.VertexAttribPointer(POSITION_LOCATION, 3, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers[(int)VB_TYPES.TEXCOORD_VB]);
			GL.BufferData(BufferTarget.ArrayBuffer, v2size * texCoords.Count, texCoords.ToArray(), BufferUsageHint.StaticDraw);
			GL.EnableVertexAttribArray(TEX_COORD_LOCATION);
			GL.VertexAttribPointer(TEX_COORD_LOCATION, 2, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers[(int)VB_TYPES.NORMAL_VB]);
			GL.BufferData(BufferTarget.ArrayBuffer, v3size * normals.Count, normals.ToArray(), BufferUsageHint.StaticDraw);
			GL.EnableVertexAttribArray(NORMAL_LOCATION);
			GL.VertexAttribPointer(NORMAL_LOCATION, 3, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers[(int)VB_TYPES.BONE_VB]);
			GL.BufferData(BufferTarget.ArrayBuffer, boneSize * bones.Count, bones.ToArray(), BufferUsageHint.StaticDraw);
			GL.EnableVertexAttribArray(BONE_ID_LOCATION);
			GL.VertexAttribIPointer(BONE_ID_LOCATION, 4, VertexAttribIntegerType.Int, boneSize, IntPtr.Zero);
			GL.EnableVertexAttribArray(BONE_WEIGHT_LOCATION);
			GL.VertexAttribPointer(BONE_WEIGHT_LOCATION, 4, VertexAttribPointerType.Float, false, boneSize, new IntPtr(16));

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, Buffers[(int)VB_TYPES.INDEX_BUFFER]);
			GL.BufferData(BufferTarget.ElementArrayBuffer, iSize * indices.Count, indices.ToArray(), BufferUsageHint.StaticDraw);

			return Util.GLCheckError();
		}

		void InitMesh(int MeshIndex,
			Mesh paiMesh,
			ref List<Vector3> positions,
			ref List<Vector3> normals,
			ref List<Vector2> texCoords,
			ref List<VertexBoneData> bones,
			ref List<int> indices)
		{
			for (int i = 0; i < paiMesh.VertexCount; i++)
			{
				var position = paiMesh.Vertices[i].ToOtk();
				var normal = paiMesh.Normals[i].ToOtk();
				var texCoord = paiMesh.HasTextureCoords(0) ?
					paiMesh.TextureCoordinateChannels[0][i] :
					new Vector3D();

				positions.Add(position);
				normals.Add(normal);
				texCoords.Add(new Vector2(texCoord.X, texCoord.Y));
			}

			LoadBones(MeshIndex, paiMesh, ref bones);

			for (int i = 0; i < paiMesh.FaceCount; i++)
			{
				var face = paiMesh.Faces[i];

				if (face.IndexCount != 3)
				{
					Console.WriteLine($"Wrong number of indices on face {i}");
				}

				indices.Add(face.Indices[0]);
				indices.Add(face.Indices[1]);
				indices.Add(face.Indices[2]);
			}
		}

		void LoadBones(int meshIndex, Mesh mesh, ref List<VertexBoneData> bones)
		{
			for (int i = 0; i < mesh.BoneCount; i++)
			{
				var boneIndex = 0;
				var boneName = mesh.Bones[i].Name;

				if (!BoneMapping.ContainsKey(boneName)) {
					boneIndex = NumBones;
					NumBones++;

					BoneInfo bi = new BoneInfo();

					BoneInfo.Add(bi);
					var biToChange = BoneInfo[boneIndex];
					biToChange.BoneOffset = mesh.Bones[i].OffsetMatrix.ToOtk();
					BoneInfo[boneIndex] = biToChange;
					BoneMapping[boneName] = boneIndex;
				}
				else
				{
					boneIndex = BoneMapping[boneName];
				}

				for (int j = 0; j < mesh.Bones[i].VertexWeightCount; j++)
				{
					int vertexId = Entries[meshIndex].BaseVertex + mesh.Bones[i].VertexWeights[j].VertexID;
					float weight = mesh.Bones[i].VertexWeights[j].Weight;

					var bone = bones[vertexId];
					VertexBoneData.AddBoneData(ref bone, boneIndex, weight);
					bones[vertexId] = bone;
				}
			}
		}

		// Ignore this method as we won't be loading materials
		bool InitMaterials(Scene scene, string fileName)
		{
			var slashIndex = fileName.LastIndexOf("/");
			string dir = "";

			if (slashIndex == -1)
			{
				dir = ".";
			}
			else if (slashIndex == 0)
			{
				dir = "/";
			}
			else
			{
				dir = fileName.Substring(0, slashIndex);
			}

			bool ret = true;

			for (int i = 0; i < scene.MaterialCount; i++)
			{
				var material = scene.Materials[i];

				if (material.GetMaterialTextureCount(TextureType.Diffuse) > 0)
				{
					TextureSlot path;

					if (material.GetMaterialTexture(TextureType.Diffuse, 0, out path))
					{
						var p = path.FilePath;

						if (p.Substring(0, 2) == ".\\")
						{
							p = p.Substring(2, p.Length - 2);
						}

						string fullPath = dir + "/" + p;

						Textures.Add(new Texture(TextureTarget.Texture2D, fullPath));

						// TODO: load textures
					}
				}
			}

			return ret;
		}
	}
}
