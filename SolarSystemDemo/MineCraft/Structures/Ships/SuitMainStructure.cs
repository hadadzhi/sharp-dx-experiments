using SolarSystemDemo.Objects.Demo.PlayerControls;

namespace SolarSystemDemo.MineCraft.Structures.Ships
{
	public class SuitMainStructure : ShipMainStructure
	{
		public CubicalStructure LeftHandRef;
		public CubicalStructure RightHandRef;
		public CubicalStructure LeftLegRef;
		public CubicalStructure RightLegRef;

		#region Constructors

		public SuitMainStructure(ShipControls controls)
			: base(controls) { }

		#endregion Constructors

		//bool Walking = true;
		//bool Walking = false;

		//float WalkingTime = 0;
		//float Speed = 2.5f;
		//float sign = 1;

		// Инерционные двигатели
		//public override void UpdateState(float timeDelta)
		//{
		//    #region Walking

		//    if (Walking)
		//    {
		//        WalkingTime += timeDelta * sign * Speed;

		//        if (WalkingTime > 1)
		//        {
		//            sign = -1;
		//        }
		//        else if (WalkingTime < -1)
		//        {
		//            sign = 1;
		//        }

		//        float a = WalkingTime * MathUtil.Pi / 7;
		//        Quaternion r = Quaternion.RotationYawPitchRoll(0, a, 0);

		//        LeftHand.LocalRotationQuaternion = r;
		//        RightHand.LocalRotationQuaternion = Quaternion.Invert(r);

		//        LeftLeg.LocalRotationQuaternion = Quaternion.Invert(r);
		//        RightLeg.LocalRotationQuaternion = r;
		//    }

		//    #endregion Walking

		//    Vector3 lineVelocity = Vector3.Zero;
		//    Vector3 rotationAxis = Vector3.Zero;

		//    foreach (InertiaEngineCube engine in InertiaEngines)
		//    {
		//        rotationAxis += engine.AngleVelocity;
		//        //lineVelocity += MathUtil2.RotateVector(engine.LineVelocity, Structure.WorldRotationQuaternion); // !!!
		//        lineVelocity += MathUtil2.RotateVector(engine.LineVelocity, engine.WorldRotationQuaternion);
		//    }

		//    WorldPosition = WorldPosition + lineVelocity * timeDelta;

		//    if (!MathUtil2.NearEqual(rotationAxis, Vector3.Zero))
		//    {
		//        rotationAxis = MathUtil2.RotateVector(rotationAxis, Quaternion.Invert(Structure.WorldRotationQuaternion));

		//        RotateAxis(Vector3.Normalize(rotationAxis), rotationAxis.Length() * timeDelta);
		//        Structure.LocalRotationQuaternion = LocalRotationQuaternion;
		//    }

		//    Structure.WorldPosition = WorldPosition;
		//    Structure.UpdateState(timeDelta);
		//}


		//public override void FinishConstruction()
		//{
		//	//LeftHand.RotatePitch(MathUtil.Pi / 16);
		//	//RightHand.RotatePitch(MathUtil.Pi / 16);
		//	//LeftLeg.RotatePitch(MathUtil.Pi / 16);
		//	//RightLeg.RotatePitch(MathUtil.Pi / 16);


		//	Controls.BindOnKeyDownOperation(Keys.W, () =>
		//		{
		//			InertiaAllDirEngine e = InertiaEngines[0] as InertiaAllDirEngine;
		//			e.EngineDirection = Directions.Toward;
		//			e.Activate();

		//			e = InertiaEngines[1] as InertiaAllDirEngine;
		//			e.EngineDirection = Directions.Toward;
		//			e.Activate();
		//		}
		//	);
		//	Controls.BindOnKeyUpOperation(Keys.W, () =>
		//		{
		//			InertiaAllDirEngine e = InertiaEngines[0] as InertiaAllDirEngine;
		//			e.Deactivate();
		//			e = InertiaEngines[1] as InertiaAllDirEngine;
		//			e.Deactivate();
		//		}
		//	);


		//	base.FinishConstruction();
		//}
	}
}
