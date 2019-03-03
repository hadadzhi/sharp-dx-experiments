using System.Collections.Generic;
using SharpDX;
using SolarSystemDemo.Graphics;

namespace SolarSystemDemo.Objects.Base
{
	public class BaseSceneObject : BaseObject
	{
		#region Constructors

		public BaseSceneObject()
			: base()
		{
			IsVisible = true;
			ScalingVector = Vector3.One;

			MeshDataId = -1;
			MaterialId = -1;
			TextureId = -1;
		}

		#endregion Constructors

		public virtual Vector3 ScalingVector { get; set; }

		public virtual bool IsVisible { get; set; }

		public virtual int MeshDataId { get; set; }
		public virtual int MaterialId { get; set; }
		public virtual int TextureId { get; set; }

		public virtual List<GraphicsData> GetGraphicsData()
		{
			return new List<GraphicsData> {
				new GraphicsData
				{
					IsVisible = this.IsVisible,

					RealPosition = this.WorldPosition,
					RotationQuaternion = this.WorldRotationQuaternion,
					ScalingVector = this.ScalingVector,

					MeshDataId = this.MeshDataId,
					MaterialId = this.MaterialId,
					TextureId = this.TextureId
				}
			};
		}
	}
}
