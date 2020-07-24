using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace RivalAI.Helpers {
	public static class CollisionHelper {

		public static List<IMyCubeGrid> NewGrids = new List<IMyCubeGrid>();
		public static List<IMyCubeGrid> ActiveGrids = new List<IMyCubeGrid>();
		public static List<MySafeZone> AllSafeZones = new List<MySafeZone>();

		public static void NewEntityDetected(IMyEntity entity) {

			var cubeGrid = entity as IMyCubeGrid;

			if (cubeGrid == null)
				return;

			if (cubeGrid.Physics != null) {

				if (!NewGrids.Contains(cubeGrid))
					NewGrids.Add(cubeGrid);

			} else {

				cubeGrid.OnPhysicsChanged += GridPhysicsChanged;

			}
		
		}

		public static void GridPhysicsChanged(IMyEntity entity) {

			var cubeGrid = entity as IMyCubeGrid;

			if (cubeGrid == null)
				return;

			if (cubeGrid.Physics != null) {

				if (!NewGrids.Contains(cubeGrid))
					NewGrids.Add(cubeGrid);

			}

		}

		public static void RegisterCollisionHelper() {

			MyAPIGateway.Entities.OnEntityAdd += NewEntityDetected;
			var allEntities = new HashSet<IMyEntity>();
			MyAPIGateway.Entities.GetEntities(allEntities);

			foreach (var entity in allEntities) {

				var cubeGrid = entity as IMyCubeGrid;

				if (cubeGrid == null)
					return;

				if (cubeGrid.Physics != null) {

					if (!NewGrids.Contains(cubeGrid))
						NewGrids.Add(cubeGrid);

				} else {

					cubeGrid.OnPhysicsChanged += GridPhysicsChanged;

				}

			}


		}

		public static void UnregisterCollisionHelper() {

			MyAPIGateway.Entities.OnEntityAdd -= NewEntityDetected;

		}

	}
}
