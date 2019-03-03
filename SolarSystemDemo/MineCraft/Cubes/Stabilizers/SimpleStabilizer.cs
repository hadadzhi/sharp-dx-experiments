using System.Collections.Generic;
using SharpDX;
using SolarSystemDemo.GeoMath;
using SolarSystemDemo.Graphics;
using SolarSystemDemo.Objects.Base;
using System;

namespace SolarSystemDemo.MineCraft.Cubes.Stabilizers
{
	public class SimpleStabilizer : Cube
	{
		public override StructureBlockFunctions BlockFunction
		{
			get { return StructureBlockFunctions.Stabilizer; }
		}

		public override int Id
		{
			get { return StabilizerRegistry.BaseStabilizerCubeId; }
		}


		public const float DefaultPowerOutput = 200;

		private float _PowerOutput;

		public float PowerOutput
		{
			get { return _PowerOutput; }
			protected set { _PowerOutput = value; }
		}

		#region Constructors

		public SimpleStabilizer()
			: this(DefaultPowerOutput) { }

		public SimpleStabilizer(float powerOutput)
			: base()
		{
			_PowerOutput = powerOutput;
		}

		#endregion Constructors

		public override bool CanAffect
		{
			get { return true; }
		}

		public override void Affect(BaseInteractiveObject containerObject, IEnumerable<BaseInteractiveObject> otherObjects)
		{
			if (!TwaMath.NearZero(containerObject.WorldLineVelocity))
			{
				Vector3 resistance = Vector3.Normalize(-containerObject.WorldLineVelocity) * PowerOutput;
				// Сглаживание торможения ???
				containerObject.AddForce(worldForce: resistance);
			}

			if (!TwaMath.NearZero(containerObject.LocalAngleVelocity))
			{
				float power = PowerOutput;

				// Сглаживание торможения
				if (containerObject.LocalAngleVelocity.Length() < 1)
				{
					power *= (float) Math.Pow(containerObject.LocalAngleVelocity.Length(), 0.25);
				}

				float momentOfInertia = containerObject.CalculateMomentOfInertia(Vector3.Normalize(containerObject.LocalAngleVelocity));

				containerObject.AddForce(
					localMomentOfForce: Vector3.Normalize(-containerObject.LocalAngleVelocity) * power,
					localMomentOfInertia: momentOfInertia
				);
			}
		}

		#region Graphics Overrides

		public override int MaterialId
		{
			get { return StaticGraphicsResources.StabilizerMaterialId; }
		}

		#endregion Graphics Overrides
	}
}
