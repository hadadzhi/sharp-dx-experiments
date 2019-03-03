using System;
using System.Collections.Generic;
using SharpDX;
using SolarSystemDemo.GeoMath;
using SolarSystemDemo.Graphics;
using SolarSystemDemo.Objects.Base;

namespace SolarSystemDemo.Objects.SolarSystem
{
	public class Planet : BaseInteractiveObject
	{
		private const int OrbitalDotDistance = 200;
		private const int OrbitalDotCount = 200;
		private List<BaseSceneObject> OrbitalDots;

		public string Name { get; private set; }

		public float Radius { get; private set; }
		public override float Size
		{
			get { return Radius; }
		}

		public List<Planet> Satellites { get; protected set; }

		private EllipticalTrajectory _Orbit;
		public virtual EllipticalTrajectory Orbit
		{
			get { return _Orbit; }
			protected set { _Orbit = value; }
		}

		public float Angle { get; set; }
		public float AngleVelocity { get; set; }
		public Vector3 RotationAxis { get; set; }

		#region Contructors

		public Planet(string name, float radius)
			: this(name, radius, null) { }

		public Planet(string name, float radius, EllipticalTrajectory trajectory)
			: base(trajectory)
		{
			Name = name;
			Radius = radius;
			Satellites = new List<Planet>();
			Orbit = trajectory;

			Angle = 0;
			AngleVelocity = 0;
			RotationAxis = Vector3.Up;

			#region OrbitalDots

			OrbitalDots = new List<BaseSceneObject>();

			if (trajectory != null)
			{
				// Так как обновление позиции на эллиптической траектории происходит по своей собственной частоте
				// для больших длин траекторий это занимает большое время // !!!

				//if (Orbit != null)
				//{
				//    //int dotsCount = (int)(Orbit.Length / OrbitalDotDistanceStep);
				//    //float timeStep = Orbit.CircleTime / dotsCount;
				//    float timeStep = Orbit.CircleTime / OrbitalDotCount;

				//    //foreach (Vector3 point in Orbit.CalculateOrbitalPoints(timeStep, dotsCount))
				//    foreach (Vector3 point in Orbit.CalculateOrbitalPoints(timeStep, OrbitalDotCount))
				//    {
				//        BaseSceneObject dot = new BaseSceneObject();
				//        dot.WorldPosition = point;
				//        dot.MeshDataId = StaticGraphicsResources.OrbitalDotMeshDataId;
				//        dot.MaterialId = StaticGraphicsResources.WhiteMaterialId;
				//        dot.IsVisible = false;

				//        OrbitalDots.Add(dot);
				//    }
				//}
			}

			#endregion OrbitalDots

			MeshDataId = StaticGraphicsResources.PlanetIdentitySphereMeshDataId;
			MaterialId = StaticGraphicsResources.PlanetMaterialId;

			ScalingVector = new Vector3(Radius);
		}

		#endregion Contructors

		public void AddSatellite(Planet satellite)
		{
			Satellites.Add(satellite);

			foreach (BaseSceneObject orbitalDot in satellite.OrbitalDots)
			{
				orbitalDot.IsVisible = false;
			}
		}

		public override void UpdateState(float timeDelta)
		{
			base.UpdateState(timeDelta);

			Angle = TwaMath.Mod2Pi(Angle + AngleVelocity * timeDelta);
			LocalRotationQuaternion = Quaternion.RotationAxis(RotationAxis, Angle);

			foreach (Planet satellite in Satellites)
			{
				SpaceVector trajectoryCenter = satellite.Orbit.Center;
				satellite.Orbit.Center = WorldPosition + trajectoryCenter;
				satellite.UpdateState(timeDelta);
				satellite.Orbit.Center = trajectoryCenter;
			}
		}

		#region Graphics Overrides

		public override List<GraphicsData> GetGraphicsData()
		{
			List<GraphicsData> data = base.GetGraphicsData();
			//	= new List<GraphicsData> {
			//	new GraphicsData
			//	{
			//		IsVisible = this.IsVisible,
			//		WorldMatrix = this.WorldMatrix,
			//		MeshDataId = this.MeshDataId,
			//		MaterialId = this.MaterialId,
			//		TextureId = this.TextureId
			//	}
			//};

			foreach (Planet satellite in Satellites)
			{
				data.AddRange(satellite.GetGraphicsData());
			}

			foreach (BaseSceneObject orbitalDot in OrbitalDots)
			{
				data.AddRange(orbitalDot.GetGraphicsData());
			}

			return data;
		}

		#endregion Graphics Overrides

		#region Solar System Generation

		/*
			https://ru.wikipedia.org/wiki/%D0%A1%D0%B8%D0%B4%D0%B5%D1%80%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B8%D0%B9_%D0%BF%D0%B5%D1%80%D0%B8%D0%BE%D0%B4
			https://ru.wikipedia.org/wiki/%D0%9F%D0%B5%D1%80%D0%B8%D0%BE%D0%B4_%D0%B2%D1%80%D0%B0%D1%89%D0%B5%D0%BD%D0%B8%D1%8F
			https://ru.wikipedia.org/wiki/%D0%9D%D0%B0%D0%BA%D0%BB%D0%BE%D0%BD_%D0%BE%D1%81%D0%B8_%D0%B2%D1%80%D0%B0%D1%89%D0%B5%D0%BD%D0%B8%D1%8F
		
			- наклонение i считается относительно солнечного экватора
		*/

		// направление на точку весеннего равноденствия - ось Ox
		public static Vector3 VernalEquinox { get { return Vector3.Right; } }

		public const int Day = 5184000; // 24 * 60 * 60;

		public const float OrbitalDistanceMultiplier = 0.0016f;

		public const float OrbitalSpeedMultiplier = Day / 96; // Множитель для угловой скорости вращения по эллипсу орбиты
		public const float AngularSpeedMultiplier = OrbitalSpeedMultiplier;


		public static readonly float EarthMass = 5.9726f * (float) Math.Pow(10, 24);
		public static readonly float EarthRadius = 6371000f; // 6371000 m

		public static readonly float SunMass = 1.9891f * (float) Math.Pow(10, 30);
		public static readonly float SunRadius = 109 * EarthRadius / 100f;

		public static readonly float MercuryMass = 3.33022f * (float) Math.Pow(10, 23);
		public static readonly float MercuryRadius = 0.3829f * EarthRadius;

		public static readonly float VenusMass = 4.8685f * (float) Math.Pow(10, 24);
		public static readonly float VenusRadius = 0.9499f * EarthRadius;

		public static readonly float MoonMass = 7.3477f * (float) Math.Pow(10, 22);
		public static readonly float MoonRadius = 0.273f * EarthRadius;

		public static readonly float MarsMass = 6.4135f * (float) Math.Pow(10, 23);
		public static readonly float MarsRadius = 0.533f * EarthRadius;


		#region Planet Creation

		private static Planet CreateEarth()
		{
			float orbitalPerios = 365.25f * Day;
			float T = Day;
			float axialTilt = TwaMath.FromDegrees(23.439281);

			float perihelion = 147098290000;
			float aphelion = 152098232000;

			float a = 149598261000;
			float e = 0.01671123f;

			float ascendingNode = TwaMath.FromDegrees(348.73936);
			float i = TwaMath.FromDegrees(7.155);
			float argumentOfPeriapsis = TwaMath.FromDegrees(114.20783);
			float meanAnomaly = TwaMath.FromDegrees(357.51716);

			return CreatePlanet(
				"Earth",
				EarthRadius,
				perihelion,
				aphelion,
				a,
				e,
				ascendingNode,
				i,
				argumentOfPeriapsis,
				meanAnomaly,
				orbitalPerios,
				T,
				0 // !!! axialTilt
			);
		}

		private static Planet CreateSun()
		{
			float T = 25.05f * Day;

			Planet sun = new Planet("Sun", SunRadius);
			sun.SetAngles(0, MathUtil.Pi / 2, 0);

			sun.RotationAxis = Vector3.Up;
			sun.AngleVelocity = -AngularSpeedMultiplier / T;

			return sun;
		}

		private static Planet CreateMercury()
		{
			float orbitalPerios = 88 * Day;
			float T = 58.646f * Day;
			float axialTilt = TwaMath.FromDegrees(0.01);

			float perihelion = 46001009000;
			float aphelion = 69817445000;

			float a = 57909227000;
			float e = 0.20563593f;

			float ascendingNode = TwaMath.FromDegrees(48.33167);
			float i = TwaMath.FromDegrees(3.38);
			float argumentOfPeriapsis = TwaMath.FromDegrees(29.124279);
			float meanAnomaly = TwaMath.FromDegrees(174.795884);

			return CreatePlanet(
				"Mercury",
				MercuryRadius,
				perihelion,
				aphelion,
				a,
				e,
				ascendingNode,
				i,
				argumentOfPeriapsis,
				meanAnomaly,
				orbitalPerios,
				T,
				axialTilt
			);
		}

		private static Planet CreateVenus()
		{
			float orbitalPerios = 224.7f * Day;
			float T = 243f * Day;
			float axialTilt = TwaMath.FromDegrees(177.36);

			float perihelion = 107476259000;
			float aphelion = 108942109000;

			float a = 108208930000;
			float e = 0.0068f;

			float ascendingNode = TwaMath.FromDegrees(76.678);
			float i = TwaMath.FromDegrees(3.86);
			float argumentOfPeriapsis = TwaMath.FromDegrees(55.186);
			float meanAnomaly = TwaMath.FromDegrees(50.115);

			return CreatePlanet(
				"Venus",
				VenusRadius,
				perihelion,
				aphelion,
				a,
				e,
				ascendingNode,
				i,
				argumentOfPeriapsis,
				meanAnomaly,
				orbitalPerios,
				T,
				axialTilt
			);
		}

		private static Planet CreateMoon()
		{
			float orbitalPerios = 27.322f * Day;
			float T = 27.322f * Day;
			float axialTilt = TwaMath.FromDegrees(1.5424);

			float perihelion = 363104000;
			float aphelion = 405696000;

			float a = 384399000;
			float e = 0.0549f;

			float ascendingNode = TwaMath.FromDegrees(0);
			float i = TwaMath.FromDegrees(7.155 + 5.145);
			float argumentOfPeriapsis = TwaMath.FromDegrees(0);
			float meanAnomaly = TwaMath.FromDegrees(0);

			return CreatePlanet(
				"Moon",
				MoonRadius,
				perihelion,
				aphelion,
				a,
				e,
				ascendingNode,
				i,
				argumentOfPeriapsis,
				meanAnomaly,
				orbitalPerios,
				T,
				axialTilt
			);
		}

		private static Planet CreateMars()
		{
			float orbitalPerios = 686.98f * Day;
			float T = 779.94f * Day;
			float axialTilt = TwaMath.FromDegrees(1.5424);

			float perihelion = 249200000000;
			float aphelion = 206700000000;

			float a = 227900000000;
			float e = 0.0933941f;

			float ascendingNode = TwaMath.FromDegrees(49.57854);
			float i = TwaMath.FromDegrees(5.65);
			float argumentOfPeriapsis = TwaMath.FromDegrees(286.46);
			float meanAnomaly = TwaMath.FromDegrees(19.3564);

			return CreatePlanet(
				"Mars",
				MarsRadius,
				perihelion,
				aphelion,
				a,
				e,
				ascendingNode,
				i,
				argumentOfPeriapsis,
				meanAnomaly,
				orbitalPerios,
				T,
				axialTilt
			);
		}


		private static Planet CreatePlanet(
			string name,
			float radius,
			float perihelion,
			float aphelion,
			float a,
			float e,
			float ascendingNode,
			float i,
			float argumentOfPeriapsis,
			float meanAnomaly,
			float orbitalPeriod,
			float T,
			float axialTilt)
		{

			float b = (float) Math.Sqrt(a * a * (1 - e * e));

			Vector3 vectorToNode = TwaMath.RotateVector(VernalEquinox, Vector3.Up, ascendingNode);
			Vector3 normal = TwaMath.RotateVector(Vector3.Up, vectorToNode, i);

			Vector3 direction = TwaMath.RotateVector(vectorToNode, normal, argumentOfPeriapsis);

			Vector3 shiftOrbitalCenter = Vector3.Normalize(direction) * (-(aphelion - perihelion) / 2);


			a *= OrbitalDistanceMultiplier;
			b *= OrbitalDistanceMultiplier;
			shiftOrbitalCenter = shiftOrbitalCenter * OrbitalDistanceMultiplier;
			orbitalPeriod /= OrbitalSpeedMultiplier;


			EllipticalTrajectory orbit = new EllipticalTrajectory(
				a,
				b,
				SpaceVector.FromVector3(shiftOrbitalCenter),
				normal,
				direction,
				orbitalPeriod,
				meanAnomaly // Этот угол должен быть откручен от фокуса, в котором солнце, а не от центра эллипса, так что здесь неточность
			);

			// врядли northPoleDirection правильно считается
			//Vector3 moveDirection = orbit.LastPosition - TwaMath.RotateVector(orbit.LastPosition, orbit.Normal, 0.01f);
			//Vector3 r = Vector3.Cross(moveDirection, orbit.Normal);
			//Vector3 northPoleDirection = TwaMath.RotateVector(orbit.Normal, r, axialTilt);

			Planet planet = new Planet(name, radius, orbit);
			//planet.RotationAxis = Vector3.Normalize(northPoleDirection);
			planet.RotationAxis = Vector3.Normalize(Vector3.Up); // !!!
			planet.AngleVelocity = -MathUtil.Pi * AngularSpeedMultiplier / T;

			return planet;
		}

		#endregion Planet Creation

		public static List<BaseInteractiveObject> GenerateSolarSystem()
		{
			List<BaseInteractiveObject> objects = new List<BaseInteractiveObject>();

			Planet skySphere = new Planet("SkySphere", GameState.VisionRange * 1.5f);
			skySphere.MeshDataId = StaticGraphicsResources.SkySphereMeshDataId;
			skySphere.MaterialId = StaticGraphicsResources.SkySphereMaterialId;
			skySphere.TextureId = StaticGraphicsResources.SkySphereTextureId;
			objects.Add(skySphere);

			Planet sun = CreateSun();
			sun.MaterialId = StaticGraphicsResources.SunMaterialId;
			sun.TextureId = StaticGraphicsResources.SunTextureId;
			objects.Add(sun);

			Planet earth = CreateEarth();
			earth.TextureId = StaticGraphicsResources.EarthTextureId;
			objects.Add(earth);

			Planet mercury = CreateMercury();
			mercury.TextureId = StaticGraphicsResources.MercuryTextureId;
			objects.Add(mercury);

			Planet venus = CreateVenus();
			venus.TextureId = StaticGraphicsResources.VenusTextureId;
			objects.Add(venus);

			Planet moon = CreateMoon();
			moon.TextureId = StaticGraphicsResources.MoonTextureId;
			earth.AddSatellite(moon);

			Planet mars = CreateMars();
			mars.TextureId = StaticGraphicsResources.MarsTextureId;
			objects.Add(mars);

			return objects;
		}

		#endregion Solar System Generation
	}
}
