using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX;
using SolarSystemDemo.GeoMath;
using SolarSystemDemo.Graphics;
using SolarSystemDemo.MineCraft.Cubes.Engines;
using SolarSystemDemo.MineCraft.Cubes.Hulls;
using SolarSystemDemo.MineCraft.Cubes.Stabilizers;
using SolarSystemDemo.Objects.Demo.PlayerControls;

namespace SolarSystemDemo.MineCraft.Structures.Ships
{
	public class ShipMainStructure : ShipStructure
	{
		public ShipControls Controls { get; protected set; }

		public override float Size
		{
			get { return 15; }
		}

		#region Constructors

		public ShipMainStructure(ShipControls controls)
			: base()
		{
			Controls = controls;
		}

		#endregion Constructors

		#region Shipyard & Ships Creation

		public static List<ShipMainStructure> CreateShipyardAndShips(SpaceVector shipyardLocation)
		{
			List<ShipMainStructure> objects = new List<ShipMainStructure>() {
				CreateShipyard(),
				CreateCruiserWithInertias(),
				CreateWarpCruiser(),
				//CreateDefaultSuit()
			};

			objects[0].WorldPosition = shipyardLocation + new Vector3(0, 0, 0);
			objects[1].WorldPosition = shipyardLocation + new Vector3(-15, 25, 0);
			objects[2].WorldPosition = shipyardLocation + new Vector3(15, 25, 0);

			return objects;
		}

		#region Ship Creation

		private static ShipMainStructure CreateCruiserWithInertias()
		{
			ShipControls shipControls = new ShipControls();
			ShipMainStructure structure = new ShipMainStructure(shipControls);

			for (int y = -1; y <= 1; y++)
			{
				for (int x = -2; x <= 2; x++)
				{
					for (int z = 0; z < 15; z++)
					{
						structure[x, y, -z] = new BaseHullCube();
					}
				}

				foreach (int x in new[] { -3, 3 })
				{
					structure[x, y, -1] = new BaseHullCube();
					structure[x, y, -2] = new BaseHullCube();
					structure[x, y, -3] = new BaseHullCube();
					structure[x, y, -4] = new BaseHullCube();
					structure[x, y, -10] = new BaseHullCube();
					structure[x, y, -11] = new BaseHullCube();
				}

				foreach (int x in new[] { -4, 4 })
				{
					structure[x, y, -2] = new BaseHullCube();
					structure[x, y, -3] = new BaseHullCube();
					structure[x, y, -4] = new BaseHullCube();

					for (int z = 7; z < 15; z++)
					{
						structure[x, y, -z] = new BaseHullCube();
					}
				}

				foreach (int x in new[] { -5, 5 })
				{
					for (int z = 8; z < 14; z++)
					{
						structure[x, y, -z] = new BaseHullCube();
					}
				}
			}

			foreach (int y in new[] { -2, 2 })
			{
				foreach (int x in new[] { -1, 1 })
				{
					for (int z = 2; z < 14; z++)
					{
						structure[x, y, -z] = new BaseHullCube();
					}
				}

				for (int z = 8; z < 14; z++)
				{
					structure[0, y, -z] = new BaseHullCube();
				}
			}


			JetEngine engine = new JetEngine();
			structure[0, -1, -14] = engine;
			shipControls.Bind(Keys.W, engine);

			engine = new JetEngine();
			structure[-1, 0, -14] = engine;
			shipControls.Bind(Keys.W, engine);

			engine = new JetEngine();
			structure[1, -0, -14] = engine;
			shipControls.Bind(Keys.W, engine);

			engine = new JetEngine();
			structure[0, 1, -14] = engine;
			shipControls.Bind(Keys.W, engine);


			engine = new JetEngine();
			engine.RotateYaw(MathUtil.Pi);
			structure[0, 0, 0] = engine;
			shipControls.Bind(Keys.S, engine);


			engine = new JetEngine();
			engine.RotatePitch(MathUtil.PiOverTwo);
			structure[0, 2, -2] = engine;
			shipControls.Bind(Keys.ShiftKey, engine);

			engine = new JetEngine();
			engine.RotatePitch(MathUtil.PiOverTwo);
			structure[0, 2, -3] = engine;
			shipControls.Bind(Keys.ShiftKey, engine);


			engine = new JetEngine();
			engine.RotatePitch(-MathUtil.PiOverTwo);
			structure[0, -2, -2] = engine;
			shipControls.Bind(Keys.Space, engine);

			engine = new JetEngine();
			engine.RotatePitch(-MathUtil.PiOverTwo);
			structure[0, -2, -3] = engine;
			shipControls.Bind(Keys.Space, engine);

			foreach (int z in new[] { -7, -8, -9 })
			{
				engine = new JetEngine();
				engine.RotatePitch(MathUtil.PiOverTwo);
				structure[-2, 1, z] = engine;
				shipControls.Bind(Keys.A, engine);

				engine = new JetEngine();
				engine.RotatePitch(-MathUtil.PiOverTwo);
				structure[2, -1, z] = engine;
				shipControls.Bind(Keys.A, engine);


				engine = new JetEngine();
				engine.RotatePitch(MathUtil.PiOverTwo);
				structure[2, 1, z] = engine;
				shipControls.Bind(Keys.D, engine);

				engine = new JetEngine();
				engine.RotatePitch(-MathUtil.PiOverTwo);
				structure[-2, -1, z] = engine;
				shipControls.Bind(Keys.D, engine);
			}

			return structure;
		}

		private static ShipMainStructure CreateWarpCruiser()
		{
			ShipControls shipControls = new ShipControls();
			ShipMainStructure structure = new ShipMainStructure(shipControls);

			for (int y = -1; y <= 1; y++)
			{
				for (int x = -2; x <= 2; x++)
				{
					for (int z = 0; z < 15; z++)
					{
						structure[x, y, -z] = new BaseHullCube();
					}
				}

				foreach (int x in new[] { -3, 3 })
				{
					structure[x, y, -1] = new BaseHullCube();
					structure[x, y, -2] = new BaseHullCube();
					structure[x, y, -3] = new BaseHullCube();
					structure[x, y, -4] = new BaseHullCube();
					structure[x, y, -10] = new BaseHullCube();
					structure[x, y, -11] = new BaseHullCube();
				}

				foreach (int x in new[] { -4, 4 })
				{
					structure[x, y, -2] = new BaseHullCube();
					structure[x, y, -3] = new BaseHullCube();
					structure[x, y, -4] = new BaseHullCube();

					for (int z = 7; z < 15; z++)
					{
						structure[x, y, -z] = new BaseHullCube();
					}
				}

				foreach (int x in new[] { -5, 5 })
				{
					for (int z = 8; z < 14; z++)
					{
						structure[x, y, -z] = new BaseHullCube();
					}
				}
			}

			foreach (int y in new[] { -2, 2 })
			{
				foreach (int x in new[] { -1, 1 })
				{
					for (int z = 2; z < 14; z++)
					{
						structure[x, y, -z] = new BaseHullCube();
					}
				}

				for (int z = 8; z < 14; z++)
				{
					structure[0, y, -z] = new BaseHullCube();
				}
			}

			JetEngine engine;

			for (int x = -1; x <= 1; x++)
			{
				foreach (int y in new[] { -1, 0, 1 })
				{
					engine = new JetEngine();
					structure[x, y, -14] = engine;
					shipControls.Bind(Keys.W, engine);
				}
			}
			

			engine = new JetEngine();
			engine.RotateYaw(MathUtil.Pi);
			structure[0, 0, 0] = engine;
			shipControls.Bind(Keys.S, engine);


			engine = new JetEngine();
			engine.RotatePitch(MathUtil.PiOverTwo);
			structure[0, 2, -2] = engine;
			shipControls.Bind(Keys.ShiftKey, engine);

			engine = new JetEngine();
			engine.RotatePitch(MathUtil.PiOverTwo);
			structure[0, 2, -3] = engine;
			shipControls.Bind(Keys.ShiftKey, engine);

			engine = new JetEngine();
			engine.RotatePitch(MathUtil.PiOverTwo);
			structure[0, 2, -4] = engine;
			shipControls.Bind(Keys.ShiftKey, engine);

			engine = new JetEngine();
			engine.RotatePitch(-MathUtil.PiOverTwo);
			structure[0, -2, -11] = engine;
			shipControls.Bind(Keys.ShiftKey, engine);

			engine = new JetEngine();
			engine.RotatePitch(-MathUtil.PiOverTwo);
			structure[0, -2, -12] = engine;
			shipControls.Bind(Keys.ShiftKey, engine);

			engine = new JetEngine();
			engine.RotatePitch(-MathUtil.PiOverTwo);
			structure[0, -2, -13] = engine;
			shipControls.Bind(Keys.ShiftKey, engine);


			engine = new JetEngine();
			engine.RotatePitch(-MathUtil.PiOverTwo);
			structure[0, -2, -2] = engine;
			shipControls.Bind(Keys.Space, engine);

			engine = new JetEngine();
			engine.RotatePitch(-MathUtil.PiOverTwo);
			structure[0, -2, -3] = engine;
			shipControls.Bind(Keys.Space, engine);

			engine = new JetEngine();
			engine.RotatePitch(-MathUtil.PiOverTwo);
			structure[0, -2, -4] = engine;
			shipControls.Bind(Keys.Space, engine);

			engine = new JetEngine();
			engine.RotatePitch(MathUtil.PiOverTwo);
			structure[0, 2, -11] = engine;
			shipControls.Bind(Keys.Space, engine);

			engine = new JetEngine();
			engine.RotatePitch(MathUtil.PiOverTwo);
			structure[0, 2, -12] = engine;
			shipControls.Bind(Keys.Space, engine);

			engine = new JetEngine();
			engine.RotatePitch(MathUtil.PiOverTwo);
			structure[0, 2, -13] = engine;
			shipControls.Bind(Keys.Space, engine);


			foreach (int z in new[] { -7, -8, -9 })
			{
				engine = new JetEngine();
				engine.RotatePitch(MathUtil.PiOverTwo);
				structure[-2, 1, z] = engine;
				shipControls.Bind(Keys.A, engine);

				engine = new JetEngine();
				engine.RotatePitch(-MathUtil.PiOverTwo);
				structure[2, -1, z] = engine;
				shipControls.Bind(Keys.A, engine);


				engine = new JetEngine();
				engine.RotatePitch(MathUtil.PiOverTwo);
				structure[2, 1, z] = engine;
				shipControls.Bind(Keys.D, engine);

				engine = new JetEngine();
				engine.RotatePitch(-MathUtil.PiOverTwo);
				structure[-2, -1, z] = engine;
				shipControls.Bind(Keys.D, engine);
			}

			structure[0, 2, -7] = new SimpleStabilizer();

			WarpDrive hyperDrive = new WarpDrive();
			structure[0, 0, -10] = hyperDrive;
			shipControls.Bind(Keys.X, hyperDrive);

			return structure;
		}

		#endregion Ship Creation

		#region Suit Creation

		//private static Suit CreateDefaultSuit()
		//{
		//	CubicalStructure structure = new CubicalStructure();
		//	ShipControls shipControls = new ShipControls();

		//	// Голова, шея и туловище
		//	for (int y = 0; y >= -7; y--)
		//	{
		//		structure[0, y, 0] = new BaseHullCube();
		//	}
		//	//structure[0, 0, 1] = new Cube();

		//	CubicalStructure member;
		//	InertiaAllDirEngine engine;

		//	Suit suit = new Suit(structure, shipControls);

		//	// Добавляем левую руку
		//	member = CreateSuitMember(4, out engine);
		//	suit.LeftHandRef = member;
		//	structure[-1, -2, 0] = member;


		//	// Добавляем правую руку
		//	member = CreateSuitMember(4, out engine);
		//	suit.RightHandRef = member;
		//	structure[1, -2, 0] = member;


		//	// Добавляем левую ногу
		//	member = CreateSuitMember(5, out engine);
		//	suit.LeftLegRef = member;
		//	structure[-1, -7, 0] = member;

		//	// Добавляем правую ногу
		//	member = CreateSuitMember(5, out engine);
		//	suit.RightLegRef = member;
		//	structure[1, -7, 0] = member;

		//	suit.WorldPosition = SpaceVector.FromVector3(new Vector3(-450, -15, -150));
		//	//suit.FinishConstruction();

		//	return suit;
		//}

		//private static ShipStructure CreateSuitMember(int length, out InertiaAllDirEngine engine)
		//{
		//	ShipStructure structure = new ShipStructure();

		//	for (int y = 0; y > -length + 1; y--)
		//	{
		//		BaseHullCube cube = new BaseHullCube();
		//		structure[0, y, 0] = cube;
		//	}

		//	engine = new InertiaAllDirEngine();
		//	structure[0, -length + 1, 0] = engine;

		//	return structure;
		//}

		#endregion Suit Creation

		#region Shipyard Creation

		private static ShipMainStructure CreateShipyard()
		{
			ShipControls shipControls = new ShipControls();
			ShipMainStructure structure = new ShipMainStructure(shipControls);

			for (int z = 0; z < 100; z++)
			{
				for (int x = -15; x <= 15; x++)
				{
					structure[x, 0, z] = new BaseHullCube();
				}
			}

			for (int y = 0; y <= 50; y++)
			{
				structure[0, y, 0] = new BaseHullCube();
			}

			ShipStructure room = GenerateShipRoom();
			//room.RotateYaw(MathUtil.PiOverTwo);
			structure[0, 50, -1] = room;

			return structure;
		}

		private static ShipStructure GenerateShipRoom()
		{
			ShipStructure structure = new ShipStructure();

			BaseHullCube hull0 = new BaseHullCube();
			hull0.MaterialId = StaticGraphicsResources.WhiteMaterialId;
			structure[0, 0, 0] = hull0;

			int x, y, z;
			x = -15;
			for (z = 0; z <= 30; z++)
			{
				for (y = -10; y <= 10; y++)
				{
					structure[x, y, z] = new BaseHullCube();
				}
			}

			x = 15;
			for (z = 0; z <= 30; z++)
			{
				for (y = -10; y <= 10; y++)
				{
					structure[x, y, z] = new BaseHullCube();
				}
			}

			z = 0;
			for (x = -14; x <= 14; x++)
			{
				for (y = -10; y <= -1; y++)
				{
					structure[x, y, z] = new BaseHullCube();
				}
			}

			y = 11;
			for (z = 0; z <= 30; z++)
			{
				for (x = -15; x <= -12; x++)
				{
					structure[x, y, z] = new BaseHullCube();
				}
			}

			y = 11;
			for (z = 0; z <= 30; z++)
			{
				for (x = 12; x <= 15; x++)
				{
					structure[x, y, z] = new BaseHullCube();
				}
			}

			return structure;
		}

		#endregion Shipyard Creation

		#endregion Shipyard & Ships Creation
	}
}
