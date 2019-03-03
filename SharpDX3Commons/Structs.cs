using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXCommons
{
	#region Structs

	[StructLayout(LayoutKind.Explicit, Size = 320)]
	public struct CBPerFrame
	{
		[FieldOffset(0)]
		public DirectionalLight DirLight;

		[FieldOffset(64)]
		public PointLight PointLight;

		[FieldOffset(144)]
		public SpotLight SpotLight;

		[FieldOffset(240)]
		public Vector3 ViewerPosition;

		[FieldOffset(256)]
		public Matrix DirLightViewProj;
	};

	[StructLayout(LayoutKind.Explicit, Size = 336)]
	public struct CBPerObject
	{
		[FieldOffset(0)]
		public Matrix World;

		[FieldOffset(64)]
		public Matrix WorldInvTranspose;

		[FieldOffset(128)]
		public Matrix WorldViewProj;

		[FieldOffset(192)]
		public Matrix TextureTransform;

		[FieldOffset(256)]
		public Material Material;
	};

	[StructLayout(LayoutKind.Explicit, Size = 16)]
	public struct CBConstants
	{
		[FieldOffset(0)]
		public float ShadowMapSize;
	}

	[StructLayout(LayoutKind.Explicit, Size = 64)]
	public struct DirectionalLight
	{
		[FieldOffset(0)]
		public Vector4 Ambient;

		[FieldOffset(16)]
		public Vector4 Diffuse;

		[FieldOffset(32)]
		public Vector4 Specular;

		[FieldOffset(48)]
		public Vector3 Direction;
	};

	[StructLayout(LayoutKind.Explicit, Size = 80)]
	public struct PointLight
	{
		[FieldOffset(0)]
		public Vector4 Ambient;

		[FieldOffset(16)]
		public Vector4 Diffuse;

		[FieldOffset(32)]
		public Vector4 Specular;

		[FieldOffset(48)]
		public Vector3 Position;

		[FieldOffset(60)]
		public float Range;

		[FieldOffset(64)]
		public Vector3 Att;
	};

	[StructLayout(LayoutKind.Explicit, Size = 96)]
	public struct SpotLight
	{
		[FieldOffset(0)]
		public Vector4 Ambient;

		[FieldOffset(16)]
		public Vector4 Diffuse;

		[FieldOffset(32)]
		public Vector4 Specular;

		[FieldOffset(48)]
		public Vector3 Position;

		[FieldOffset(60)]
		public float Range;

		[FieldOffset(64)]
		public Vector3 Direction;

		[FieldOffset(76)]
		public float Spot;

		[FieldOffset(80)]
		public Vector3 Att;
	};

	[StructLayout(LayoutKind.Explicit, Size = 80)]
	public struct Material
	{
		[FieldOffset(0)]
		public Vector4 Ambient;

		[FieldOffset(16)]
		public Vector4 Diffuse;

		[FieldOffset(32)]
		public Vector4 Specular; // w = SpecPower

		#region unneeded

		// Will be used in later demos

#pragma warning disable 0169
#pragma warning disable 0649

		[FieldOffset(48)]
		public Vector4 Reflect;

#pragma warning restore 0169
#pragma warning restore 0649

		#endregion

		[FieldOffset(64)]
		public bool Textured;
	};

	#endregion
}
