using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Entities {
	public static class EntityEvaluator {

		public static double AltitudeAtPosition(Vector3D coords) {

			foreach (var planet in EntityWatcher.Planets) {

				if (planet.Closed)
					continue;

				if (planet.IsPositionInRange(coords))
					return AltitudeAtPosition(coords, planet);

			}

			return 0;
		
		}

		public static double AltitudeAtPosition(Vector3D coords, PlanetEntity planet) {

			if (planet.Closed)
				return 0;

			var surfaceCoords = planet.Planet.GetClosestSurfacePointGlobal(coords);
			var myDistToCore = Vector3D.Distance(coords, planet.GetPosition());
			var surfaceDistToCore = Vector3D.Distance(surfaceCoords, planet.GetPosition());
			return myDistToCore - surfaceDistToCore;

		}

		public static List<GridEntity> GetAttachedGrids(IMyCubeGrid cubeGrid) {

			var gridList = new List<GridEntity>();

			if (cubeGrid == null)
				return gridList;

			GetAttachedGrids(cubeGrid, gridList);
			return gridList;


		}

		public static void GetAttachedGrids(IMyCubeGrid cubeGrid, List<GridEntity> gridList) {

			gridList.Clear();
			var gridGroup = MyAPIGateway.GridGroups.GetGroup(cubeGrid, GridLinkTypeEnum.Physical);

			foreach (var grid in GridManager.Grids) {

				if (grid.IsClosed() || !grid.HasPhysics)
					continue;

				if (gridGroup.Contains(grid.CubeGrid))
					gridList.Add(grid);

			}

		}

		public static GridEntity GetGridProfile(IMyCubeGrid cubeGrid) {

			foreach (var grid in GridManager.Grids) {

				if (grid.IsClosed() || !grid.HasPhysics)
					continue;

				if (grid.CubeGrid == cubeGrid)
					return grid;

			}

			return null;

		}

		public static int GetReputationBetweenIdentities(long ownerA, long ownerB) {

			if (IsIdentityNPC(ownerA)) {

				var factionA = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerA);

				if (factionA != null)
					return MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(ownerB, factionA.FactionId);

			}

			if (IsIdentityNPC(ownerB)) {

				var factionB = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerB);

				if (factionB != null)
					return MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(ownerA, factionB.FactionId);

			}

			var playerFactionA = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerA);
			var playerFactionB = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerB);

			if (playerFactionA != null && playerFactionB != null)
				return MyAPIGateway.Session.Factions.GetReputationBetweenFactions(playerFactionA.FactionId, playerFactionB.FactionId);

			return -1000;

		}

		public static double GravityAtPosition(Vector3D coords) {

			foreach (var planet in EntityWatcher.Planets) {

				if (planet.Closed)
					continue;

				if (planet.IsPositionInRange(coords))
					return GravityAtPosition(coords, planet);


			}

			return 0;

		}

		public static double GravityAtPosition(Vector3D coords, PlanetEntity planet) {

			return planet.Gravity.GetGravityMultiplier(coords);

		}

		public static double GridBroadcastRange(List<GridEntity> grids, bool onlyAntenna = false) {

			double result = 0;

			foreach (var grid in grids) {

				var power = GridBroadcastRange(grid);

				if (power > result)
					result = power;

			}

			return result;

		}

		public static double GridBroadcastRange(GridEntity grid, bool onlyAntenna = false) {

			double result = 0;

			foreach (var antenna in grid.Antennas) {

				if (antenna.IsClosed() || !antenna.Working || !antenna.Functional)
					continue;

				var antennaBlock = antenna.Block as IMyRadioAntenna;

				if (antennaBlock == null || !antennaBlock.IsBroadcasting)
					continue;

				if (antennaBlock.Radius > result)
					result = antennaBlock.Radius;

			}

			if (onlyAntenna)
				return result;

			foreach (var beacon in grid.Beacons) {

				if (beacon.IsClosed() || !beacon.Working || !beacon.Functional)
					continue;

				var beaconBlock = beacon.Block as IMyRadioAntenna;

				if (beaconBlock == null)
					continue;

				if (beaconBlock.Radius > result)
					result = beaconBlock.Radius;

			}

			return result;

		}

		public static bool GridPowered(List<GridEntity> grids) {

			foreach (var grid in grids) {

				if (GridPowered(grid))
					return true;

			}

			return false;

		}

		public static bool GridPowered(GridEntity grid) {

			return grid.IsPowered();

		}

		public static Vector2 GridPowerOutput(List<GridEntity> grids) {

			var result = Vector2.Zero;

			foreach (var grid in grids) {

				result += GridPowerOutput(grid);

			}

			return result;

		}

		public static Vector2 GridPowerOutput(GridEntity grid) {

			var result = Vector2.Zero;

			if (grid.IsClosed())
				return result;

			foreach (var block in grid.Power) {

				if (block.IsClosed())
					continue;

				var powerBlock = block.Block as IMyPowerProducer;

				if (powerBlock == null)
					continue;

				result.X += powerBlock.CurrentOutput;
				result.Y += powerBlock.MaxOutput;

			}

			return result;

		}

		public static bool GridShielded(List<GridEntity> grids) {

			foreach (var grid in grids) {

				if (GridShielded(grid))
					return true;
			
			}

			return false;

		}

		public static bool GridShielded(GridEntity grid) {

			if (grid.IsClosed())
				return false;

			if (EntityShielded(grid.CubeGrid))
				return true;

			foreach (var shield in grid.Shields) {

				if (shield.IsClosed() || !shield.Working || !shield.Functional)
					continue;

				return true;
			
			}

			return false;

		}

		public static float GridTargetValue(List<GridEntity> gridList) {

			float result = 0;

			foreach (var grid in gridList) {

				result += GridTargetValue(grid);

			}

			return result;

		}
		public static float GridTargetValue(GridEntity grid) {

			float result = 0;

			if (grid.IsClosed())
				return result;

			result += GetTargetValueFromBlockList(grid.Antennas, 4, 2);
			result += GetTargetValueFromBlockList(grid.Beacons, 3, 2);
			result += GetTargetValueFromBlockList(grid.Containers, 0.5f, 2, true);
			result += GetTargetValueFromBlockList(grid.Controllers, 0.5f, 2);
			result += GetTargetValueFromBlockList(grid.Guns, 5, 4, true);
			result += GetTargetValueFromBlockList(grid.JumpDrives, 10, 2);
			result += GetTargetValueFromBlockList(grid.Mechanical, 1, 2);
			result += GetTargetValueFromBlockList(grid.NanoBots, 15, 2);
			result += GetTargetValueFromBlockList(grid.Production, 2, 2, true);
			result += GetTargetValueFromBlockList(grid.Power, 0.5f, 2, true);
			result += GetTargetValueFromBlockList(grid.Shields, 15, 2);
			result += GetTargetValueFromBlockList(grid.Thrusters, 1, 2);
			result += GetTargetValueFromBlockList(grid.Tools, 2, 2, true);
			result += GetTargetValueFromBlockList(grid.Turrets, 7.5f, 4, true);

			return result;
		
		}

		public static float GetTargetValueFromBlockList(List<BlockEntity> blockList, float threatValue, float modMultiplier = 2, bool scanInventory = false) {

			float result = 0;

			foreach (var block in blockList) {

				if (block.IsClosed() || !block.Working || !block.Functional)
					continue;

				result += threatValue * (block.Modded ? modMultiplier : 1);

				if (!scanInventory)
					continue;

			}

			return result;

		}

		public static int GridWeaponCount(List<GridEntity> grids) {

			int result = 0;

			foreach (var grid in grids) {

				result += GridWeaponCount(grid);

			}

			return result;
		
		}

		public static int GridWeaponCount(GridEntity grid) {

			int result = 0;

			foreach (var gun in grid.Guns) {

				if (!gun.ActiveEntity())
					continue;

				result++;
			
			}

			foreach (var turret in grid.Turrets) {

				if (!turret.ActiveEntity())
					continue;

				result++;

			}

			return result;

		}

		public static bool IsIdentityNPC(long identityId) {

			if (MyAPIGateway.Players.TryGetSteamId(identityId) > 0)
				return false;

			return true;
		
		}

		public static bool IsPositionInSafeZone(Vector3D coords) {

			foreach (var zone in EntityWatcher.SafeZones) {

				if (zone.IsClosed() || !zone.SafeZone.Enabled)
					continue;

				if (zone.InZone(coords))
					return true;
			
			}

			return false;
		
		}

		public static bool EntityShielded(IMyEntity entity) {

			if (entity == null || entity.MarkedForClose || entity.Closed)
				return false;

			//Begin Defense Shield API Check
			if (RAI_SessionCore.Instance.SApi.ProtectedByShield(entity))
				return true;
			//End Defense Shield API Check
			
			return false;

		}

		public static double EntitySpeed(IMyEntity entity) {

			if (entity == null || entity.MarkedForClose || entity.Closed)
				return -1;

			if (entity.Physics == null)
				return -1;

			return entity.Physics.LinearVelocity.Length();
			
		}

		public static Vector3D EntityVelocity(IMyEntity entity) {

			if (entity == null || entity.MarkedForClose || entity.Closed)
				return Vector3D.Zero;

			if (entity.Physics == null)
				return Vector3D.Zero;

			return entity.Physics.LinearVelocity;

		}



	}

}
