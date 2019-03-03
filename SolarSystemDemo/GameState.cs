using System.Collections.Generic;
using System.Linq;
using SharpDX;
using SharpDXCommons;
using SolarSystemDemo.GeoMath;
using SolarSystemDemo.Graphics;
using SolarSystemDemo.MineCraft.Structures.Ships;
using SolarSystemDemo.Objects.Base;
using SolarSystemDemo.Objects.Demo.PlayerControls;
using SolarSystemDemo.Objects.SolarSystem;

namespace SolarSystemDemo
{
	public class GameState
	{
		public const float VisionRange = 200000;

		public long GameTime;
		private float TimeBuffer;
		private float TimeStep = 0.001f; /* it's "const" actually */

		public RenderWindow RenderWindow { get; private set; }

		private Dictionary<int, BaseObject> AllObjects;
		private List<BaseInteractiveObject> InteractiveObjects;
		private List<int> CelestialObjectsIds;

		public int SkySphereId;
		public Vector3 SunPointLightPosition;

		public Dictionary<int, BaseSceneObject> ShipMarks;

		#region Player's Ship

		private int _CurrentShipId;
		public int CurrentShipId
		{
			get { return _CurrentShipId; }
			set
			{
				if (_CurrentShipId != -1)
				{
					PlayerShip.Controls.RemoveControls(RenderWindow);
				}

				_CurrentShipId = value;

				PlayerShip.Controls.InstallControls(RenderWindow);
			}
		}

		public ShipMainStructure PlayerShip
		{
			get
			{
				if (CurrentShipId != -1) { return GetObject(CurrentShipId) as ShipMainStructure; }
				else { return null; }
			}
		}

		public ShipCamera ShipCamera;

		#endregion Player's Ship

		public GameState(RenderWindow renderWindow)
		{
			GameTime = 0;
			TimeBuffer = 0;

			RenderWindow = renderWindow;

			AllObjects = new Dictionary<int, BaseObject>();

			ShipCamera = new ShipCamera(10, 1000, 20);
			ShipCamera.InstallControls(RenderWindow);

			InitTestObjects();

			SkySphereId = AllObjects.Where(o => (o.Value is Planet) && (o.Value as Planet).Name == "SkySphere").First().Key;
			CelestialObjectsIds = AllObjects.Where(o => o.Value is Planet).Select(o => o.Key).ToList();
			CelestialObjectsIds.Remove(SkySphereId);
		}


		#region Init Test Objects

		private void InitTestObjects()
		{
			foreach (ShipMainStructure ship in ShipMainStructure.CreateShipyardAndShips(SpaceVector.FromCurrentChunk(10000000, 10000000, 10000000)))
			{
				AddObjectToGame(ship);
			}

			CurrentShipId = 2;

			foreach (BaseSceneObject planet in Planet.GenerateSolarSystem())
			{
				AddObjectToGame(planet);
			}


			ShipMarks = new Dictionary<int, BaseSceneObject>();
			foreach (int shipId in AllObjects.Keys.Where(k => AllObjects[k] is ShipMainStructure && k != CurrentShipId))
			{
				BaseSceneObject mark = new BaseSceneObject();
				mark.MeshDataId = StaticGraphicsResources.EngineDirectionDotMeshDataId;
				mark.MaterialId = StaticGraphicsResources.WhiteMaterialId;

				ShipMarks.Add(shipId, mark);
			}
			foreach (BaseSceneObject mark in ShipMarks.Values)
			{
				AddObjectToGame(mark);
			}
		}

		#endregion Init Test Objects

		#region Work with Objects

		public int AddObjectToGame(BaseObject obj)
		{
			int key = AllObjects.Keys.Count == 0 ? 0 : AllObjects.Keys.Max() + 1;
			AllObjects.Add(key, obj);
			return key;
		}

		public List<BaseObject> GetAllObjects()
		{
			return AllObjects.Values.ToList();
		}

		public BaseObject GetObject(int id)
		{
			if (AllObjects.ContainsKey(id))
			{
				return AllObjects[id];
			}
			else
			{
				return null;
			}
		}

		#endregion Work with Objects

		public void UpdateState(float timeDelta)
		{
			TimeBuffer += timeDelta;

			if (TimeBuffer > 10)
			{
				return; // !!!
			}

			while (TimeBuffer >= TimeStep)
			{
				InteractiveObjects = AllObjects.Values
					.Where(o => (o is BaseInteractiveObject) && (o as BaseInteractiveObject).CanAffect)
					.Select(o => o as BaseInteractiveObject)
					.ToList();

				foreach (BaseInteractiveObject interactiveObject in InteractiveObjects)
				{
					List<BaseInteractiveObject> interactiveObjects = new List<BaseInteractiveObject>(InteractiveObjects);
					interactiveObjects.Remove(interactiveObject);
					interactiveObject.Affect(interactiveObject, interactiveObjects);
				}

				foreach (IUpdateable updateableObject in AllObjects.Values.Where(o => o is IUpdateable))
				{
					updateableObject.UpdateState(TimeStep);
				}

				ShipCamera.Update(timeDelta, PlayerShip);

				TimeBuffer -= TimeStep;
				GameTime++;
			}

			AllObjects[SkySphereId].WorldPosition = PlayerShip.WorldPosition;

			SpaceVector playerPosition = PlayerShip.WorldPosition;
			foreach (var mark in ShipMarks)
			{
				Vector3 directionToShip = SpaceVector.Normalize(AllObjects[mark.Key].WorldPosition - playerPosition);
				mark.Value.WorldPosition = playerPosition + directionToShip * 15;
			}

			//Log.Debug(PlayerShip.WorldPosition.ToString());
		}


		public Matrix GetPlayerCameraViewMatrix()
		{
			return ShipCamera.GetViewMatrix();
		}

		public Vector3 GetPlayerCameraPosition()
		{
			return ShipCamera.Position;
		}

		public List<GraphicsData> GetGraphicsData()
		{
			SpaceVector playerPosition = PlayerShip.WorldPosition;

			List<GraphicsData> data = new List<GraphicsData>();


			IEnumerable<BaseSceneObject> objectsToDraw = AllObjects
				.Where(o => SpaceVector.AreWithinLocalChunkRange(o.Value.WorldPosition, playerPosition))
				.Where(o => (o.Value.WorldPosition - playerPosition).LengthWithinGlobalChunk < VisionRange)
				.Where(o => o.Value is BaseSceneObject && !CelestialObjectsIds.Contains(o.Key))
				.Select(o => o.Value as BaseSceneObject);

			foreach (BaseSceneObject sceneObject in objectsToDraw)
			{
				foreach (GraphicsData d in sceneObject.GetGraphicsData().Where(d => d.IsVisible))
				{
					GraphicsData rd = d;

					rd.GraphicsPosition = SpaceVector.GetDeltaVectorWithinChunkRange(d.RealPosition, playerPosition);
					data.Add(rd);
				}
			}


			IEnumerable<Planet> celestialObjects = AllObjects.Where(
				o => CelestialObjectsIds.Contains(o.Key) && SpaceVector.AreWithinGlobalChunkRange(o.Value.WorldPosition, playerPosition)
			).Select(o => o.Value as Planet);

			double farthest = celestialObjects.Max(o => (o.WorldPosition - playerPosition).LengthWithinGlobalChunk);
			float k = (float) (VisionRange / farthest);

			foreach (Planet celestialObject in celestialObjects)
			{
				GraphicsData d = celestialObject.GetGraphicsData().First();

				SpaceVector delta = d.RealPosition - playerPosition;
				float length = (float) (delta.LengthWithinGlobalChunk * k);
				d.GraphicsPosition = SpaceVector.Normalize(delta) * length;

				if (celestialObject.Name == "Sun")
				{
					SunPointLightPosition = d.GraphicsPosition;
				}

				d.ScalingVector = new Vector3(celestialObject.Radius * k);

				data.Add(d);
			}

			return data;
		}
	}
}
