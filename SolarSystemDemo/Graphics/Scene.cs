using System.Collections.Generic;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDXCommons;

namespace SolarSystemDemo.Graphics
{
	public class Scene
	{
		public static bool NeedUpdateBuffers;

		public static List<Vertex> Vertices { get; private set; }
		public static List<uint> Indices { get; private set; }

		private static int LastMeshDataID;
		private static Dictionary<int, MeshData> MeshDataResources { get; set; }
		private static Dictionary<int, int> MeshDataVerticesOffsets { get; set; }
		private static Dictionary<int, int> MeshDataIndicesOffsets { get; set; }

		private static int LastMaterialID;
		private static Dictionary<int, Material> MaterialResources { get; set; }

		private static int LastTextureID;
		private static Dictionary<int, ShaderResourceView> TextureResources { get; set; }

		public static void Initialize()
		{
			NeedUpdateBuffers = false;

			Vertices = new List<Vertex>();
			Indices = new List<uint>();

			LastMeshDataID = 0;
			MeshDataResources = new Dictionary<int, MeshData>();
			MeshDataVerticesOffsets = new Dictionary<int, int>();
			MeshDataIndicesOffsets = new Dictionary<int, int>();

			LastMaterialID = 0;
			MaterialResources = new Dictionary<int, Material>();

			LastTextureID = 0;
			TextureResources = new Dictionary<int, ShaderResourceView>();
		}

		//public int AddObjectToScene(ITWASceneObject sceneObject, MeshData meshData, Material material, ShaderResourceView texture = null)
		//{
		//    int meshDataID = AddMeshData(meshData);
		//    int materialID = AddMaterial(material);

		//    int textureID = -1;
		//    if (texture != null)
		//    {
		//        textureID = AddTexture(texture);
		//    }

		//    return AddObjectToScene(sceneObject, meshDataID, materialID, textureID);
		//}

		///// <summary>
		///// Не хватает проверок на правильность аргументов
		///// </summary>
		//public int AddObjectToScene(ITWASceneObject sceneObject, int meshDataID, int materialID, int textureID = -1)
		//{
		//    sceneObject.MeshDataID = meshDataID;
		//    sceneObject.MaterialID = materialID;

		//    if (textureID >= 0)
		//    {
		//        sceneObject.TextureID = textureID;
		//    }

		//    LastSceneObjectID += 1;
		//    SceneObjects.Add(LastSceneObjectID, sceneObject);

		//    return LastSceneObjectID;
		//}

		//public void RemoveObjectFromScene(int sceneObjectID, bool removeMeshData = false)
		//{
		//    // Todo: обработать removeMeshData

		//    SceneObjects.Remove(sceneObjectID);
		//}

		#region Work with MeshData

		public static int AddMeshData(MeshData meshData)
		{
			LastMeshDataID += 1;
			MeshDataResources.Add(LastMeshDataID, meshData);
			MeshDataVerticesOffsets.Add(LastMeshDataID, Vertices.Count);
			MeshDataIndicesOffsets.Add(LastMeshDataID, Indices.Count);

			Vertices.AddRange(meshData.Vertices);
			Indices.AddRange(meshData.Indices);

			NeedUpdateBuffers = true;

			return LastMeshDataID;
		}

		public static MeshData GetMeshData(int meshDataID)
		{
			if (MeshDataResources.ContainsKey(meshDataID))
			{
				return MeshDataResources[meshDataID];
			}
			else
			{
				return new MeshData();
			}
		}

		public static int GetMeshDataIndicesOffset(int meshDataID)
		{
			if (MeshDataIndicesOffsets.ContainsKey(meshDataID))
			{
				return MeshDataIndicesOffsets[meshDataID];
			}
			else
			{
				return -1;
			}
		}

		public static int GetMeshDataVerticesOffset(int meshDataID)
		{
			if (MeshDataVerticesOffsets.ContainsKey(meshDataID))
			{
				return MeshDataVerticesOffsets[meshDataID];
			}
			else
			{
				return -1;
			}
		}

		public static void RemoveMeshData(int meshDataID)
		{
			int vertexCount = MeshDataResources[meshDataID].Vertices.Length;
			int indexCount = MeshDataResources[meshDataID].Indices.Length;

			foreach (int meshID in MeshDataResources.Keys.Where(k => k > meshDataID))
			{
				MeshDataVerticesOffsets[meshID] -= vertexCount;
				MeshDataIndicesOffsets[meshID] -= indexCount;
			}

			MeshDataResources.Remove(meshDataID);
			MeshDataVerticesOffsets.Remove(meshDataID);
			MeshDataIndicesOffsets.Remove(meshDataID);

			NeedUpdateBuffers = true;
		}

		#endregion Work with MeshData

		#region Work with Materials

		public static int AddMaterial(Material material)
		{
			LastMaterialID += 1;
			MaterialResources.Add(LastMaterialID, material);
			return LastMaterialID;
		}

		public static Material GetMaterial(int materialID)
		{
			if (MaterialResources.ContainsKey(materialID))
			{
				return MaterialResources[materialID];
			}
			else
			{
				return new Material
				{
					Ambient = Vector4.One,
					Diffuse = Vector4.One,
					Specular = Vector4.One,
					Textured = false
				};
			}
		}

		public static void RemoveMaterial(int materialID)
		{
			MaterialResources.Remove(materialID);
		}

		#endregion Work with Materials

		#region Work with Textures

		public static int AddTexture(ShaderResourceView texture)
		{
			LastTextureID += 1;
			TextureResources.Add(LastTextureID, texture);
			return LastTextureID;
		}

		public static ShaderResourceView GetTexture(int textureID)
		{
			if (TextureResources.ContainsKey(textureID))
			{
				return TextureResources[textureID];
			}
			else
			{
				return null;
			}
		}

		public static void RemoveTexture(int textureID)
		{
			TextureResources.Remove(textureID);
		}

		#endregion Work with Textures
	}
}
