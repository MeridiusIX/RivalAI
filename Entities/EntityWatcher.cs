using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace RivalAI.Entities {
	public static class EntityWatcher {

		public static List<MyDefinitionId> NanobotBlockIds = new List<MyDefinitionId>();
		public static List<MyDefinitionId> RivalAiBlockIds = new List<MyDefinitionId>();
		public static List<MyDefinitionId> ShieldBlockIds = new List<MyDefinitionId>();

		public static List<PlanetEntity> Planets = new List<PlanetEntity>();
		public static List<SafeZoneEntity> SafeZones = new List<SafeZoneEntity>();

		public static bool NewPlayerConnected = false;

		public static Action UnloadEntities;

		public static void RegisterWatcher() {

			var entityList = new HashSet<IMyEntity>();
			MyAPIGateway.Entities.GetEntities(entityList);

			foreach (var entity in entityList) {

				NewEntityDetected(entity);

			}

			PlayerManager.RefreshAllPlayers();

			UnloadEntities += GridManager.UnloadEntities;
			UnloadEntities += PlayerManager.UnloadEntities;

			MyAPIGateway.Entities.OnEntityAdd += NewEntityDetected;

			//Register One-Off Blocks
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "LWTSX_DamageAbsorber"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "LargeShipSmallShieldGeneratorBase"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "LargeShipLargeShieldGeneratorBase"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "SmallShipSmallShieldGeneratorBase"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "SmallShipMicroShieldGeneratorBase"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "EmitterST"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "EmitterL"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "EmitterS"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "EmitterLA"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "EmitterSA"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "NPCEmitterLB"));
			ShieldBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "NPCEmitterSB"));

			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "RivalAIRemoteControlSmall"));
			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "RivalAIRemoteControlLarge"));
			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "K_Imperial_Dropship_Guild_RC"));
			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "K_TIE_Fighter_RC"));
			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "K_Imperial_SpeederBike_FakePilot"));
			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "K_Imperial_ProbeDroid_Top_II"));
			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "K_Imperial_DroidCarrier_DroidBrain"));
			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "K_Imperial_DroidCarrier_DroidBrain_Aggressor"));
			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "K_NewRepublic_EWing_RC"));
			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "K_Imperial_RC_Largegrid"));
			RivalAiBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_RemoteControl), "K_TIE_Drone_Core"));

			NanobotBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_ShipWelder), "SELtdSmallNanobotBuildAndRepairSystem"));
			NanobotBlockIds.Add(new MyDefinitionId(typeof(MyObjectBuilder_ShipWelder), "SELtdLargeNanobotBuildAndRepairSystem"));


		}

		public static void NewEntityDetected(IMyEntity entity) {

			var cubeGrid = entity as IMyCubeGrid;

			if (cubeGrid != null) {

				lock (GridManager.Grids) {

					var gridEntity = new GridEntity(entity);
					UnloadEntities += gridEntity.Unload;
					GridManager.Grids.Add(gridEntity);

				}
				
				return;
			
			}

			var planet = entity as MyPlanet;

			if (planet != null) {

				var planetEntity = new PlanetEntity(entity);
				UnloadEntities += planetEntity.Unload;
				Planets.Add(planetEntity);
				return;

			}

			var safezone = entity as MySafeZone;

			if (safezone != null) {

				var safezoneEntity = new SafeZoneEntity(entity);
				UnloadEntities += safezoneEntity.Unload;
				SafeZones.Add(safezoneEntity);
				return;

			}

		}

		public static void UnregisterWatcher() {

			MyAPIGateway.Entities.OnEntityAdd -= NewEntityDetected;

			UnloadEntities?.Invoke();

			UnloadEntities = null;
			GridManager.Grids.Clear();
			PlayerManager.Players.Clear();
			Planets.Clear();
			SafeZones.Clear();
			ShieldBlockIds.Clear();
			NanobotBlockIds.Clear();

		}

	}

}
