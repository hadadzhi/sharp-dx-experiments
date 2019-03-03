using System.Collections.Generic;
using SolarSystemDemo.Graphics;
using SolarSystemDemo.Objects.Base;
using SolarSystemDemo.GeoMath;
using SharpDX;

namespace SolarSystemDemo.MineCraft.Cubes.Engines
{
	public class JetEngine : SimpleEngine
	{
		public override int Id
		{
			get { return EngineRegistry.JetEngineId; }
		}

        public const float DefaultPowerOutput = 100;
        public const float DefaultLiquidSpaceResistance = 0.2f;

        private float _PowerOutput;
        private float _LiquidSpaceResistance;

        public override float PowerOutput
        {
            get { return _PowerOutput; }
            protected set { _PowerOutput = value; }
        }

        public virtual float LiquidSpaceResistance
        {
            get { return _LiquidSpaceResistance; }
            protected set { _LiquidSpaceResistance = value; }
        }
		
		#region Constructors

        public JetEngine()
            : this(DefaultPowerOutput, DefaultLiquidSpaceResistance) { }

        public JetEngine(float powerOutput, float liquidSpaceResistance)
			: base()
		{
            _PowerOutput = powerOutput;
            _LiquidSpaceResistance = liquidSpaceResistance;

			ActiveMaterialID = StaticGraphicsResources.WhiteMaterialId;
			PassiveMaterialID = StaticGraphicsResources.InertiaEngineMaterialId;
		}

		#endregion Constructors

		public override void Affect(BaseInteractiveObject containerObject, IEnumerable<BaseInteractiveObject> otherObjects)
		{
            //Vector3 engineWorldLineVelocity = containerObject.WorldLineVelocity;

            //if (!TwaMath.NearZero(containerObject.LocalAngleVelocity))
            //{
            //    Vector3 r = BaseRelPosition - Overstructure.LocalMassCenterShift;
            //    Vector3 proj = TwaMath.GetProjectionToPlane(r, containerObject.LocalAngleVelocity);
            //    engineWorldLineVelocity += TwaMath.RotateVector(Vector3.Cross(containerObject.LocalAngleVelocity, proj), WorldRotationQuaternion);
            //}

            //if (TwaMath.GetCosBetweenVectors(engineWorldLineVelocity, WorldDirection) > 0)
            //{
            //    Vector3 worldVelocityProj = TwaMath.GetProjectionToVector(containerObject.WorldLineVelocity, WorldDirection);
            //    float worldVelocityLimited = TwaMath.Clamp(worldVelocityProj.Length(), 0, MaxVelocity);

            //    K = 1 - worldVelocityLimited / MaxVelocity;
            //}
            //else
            //{
            //    K = 1;
            //}

			base.Affect(containerObject, otherObjects);

            if (!TwaMath.NearZero(containerObject.WorldLineVelocity)
                && TwaMath.GetCosBetweenVectors(WorldDirection, containerObject.WorldLineVelocity) > 0)
            {
                float k = TwaMath.GetProjectionToVector(
                    WorldDirection,
                    Vector3.Normalize(containerObject.WorldLineVelocity)
                ).Length();

                containerObject.AddForce(
                    localForce: -containerObject.WorldLineVelocity * LiquidSpaceResistance * k
                );
            }

            if (!TwaMath.NearZero(containerObject.LocalAngleVelocity)
                && TwaMath.GetCosBetweenVectors(BaseRelMomentOfForce, containerObject.LocalAngleVelocity) > 0)
            {
                float k = TwaMath.GetProjectionToVector(
                    Vector3.Normalize(BaseRelMomentOfForce),
                    Vector3.Normalize(containerObject.LocalAngleVelocity)
                ).Length();

                containerObject.AddForce(
                    localMomentOfForce: -containerObject.LocalAngleVelocity * LiquidSpaceResistance * k,
                    localMomentOfInertia: 1
                );
            }
		}

		#region Graphics Overrides

		public static int ActiveMaterialID;
		public static int PassiveMaterialID;

		public override int MaterialId
		{
			get { return Active ? ActiveMaterialID : PassiveMaterialID; }
			set { base.MaterialId = value; }
		}

		#endregion Graphics Overrides
	}
}
