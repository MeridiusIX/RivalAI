using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Weapons;
using SpaceEngineers.Game.ModAPI;
using ProtoBuf;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Utils;
using VRageMath;
using RivalAI;
using RivalAI.Behavior;
using RivalAI.Behavior.Settings;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI.Sync;

namespace RivalAI {

	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

	public class RAI_SessionCore:MySessionComponentBase {

		public static string ReleaseVersion = "0.0.5";

		public static bool IsServer = false;
		public static bool IsDedicated = false;

		//ShieldApi Support
		internal static RAI_SessionCore Instance { get; private set; } // allow access from gamelogic
		public bool ShieldMod { get; set; }
		public bool ShieldApiLoaded { get; set; }
		public ShieldApi SApi = new ShieldApi();
		public static string ConfigInstance = "";

		public int Ticks = 0;

		public static bool SetupComplete = false;

		public override void LoadData() {

			if(MyAPIGateway.Multiplayer.IsServer == false)
				return;

			Instance = this;

			foreach(var mod in MyAPIGateway.Session.Mods) {

				if(mod.PublishedFileId == 1365616918) {

					ShieldMod = true;

				}

			}

			MESApi.RegisterAPIListener();

		}

		public override void BeforeStart() {

			if (!MyAPIGateway.Multiplayer.IsServer)
				return;

			Logger.LoadDebugFromSandbox();
			Utilities.GetAllModIDs();
			TagHelper.Setup();
			DamageHelper.RegisterEntityWatchers();

		}

		public override void UpdateBeforeSimulation() {

			if(SetupComplete == false) {

				SetupComplete = true;
				Setup();
				Logger.MsgDebug("MES API Registered: " + MESApi.MESApiReady.ToString());

			}

			if(ShieldMod && !ShieldApiLoaded && SApi.Load()) {

				ShieldApiLoaded = true;

			}

			Ticks++;

			if(Ticks % 10 == 0) {

				if(EffectManager.SoundsPending == true) {

					EffectManager.ProcessPlayerSoundEffect();

				}

			}

			if(Ticks % 60 == 0) {

				Ticks = 0;

			}

		}

		public static void Setup() {

			Logger.WriteLog("Mod Version: " + ReleaseVersion);

			IsServer = MyAPIGateway.Multiplayer.IsServer;
			IsDedicated = MyAPIGateway.Utilities.IsDedicated;
			ConfigInstance = MyAPIGateway.Utilities.GamePaths.ModScopeName;
			SyncManager.Setup();

			//Add ShieldBlocks To TargetHelper
			TargetHelper.ShieldBlockIDs.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "EmitterSA"));
			TargetHelper.ShieldBlockIDs.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "EmitterLA"));
			TargetHelper.ShieldBlockIDs.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "EmitterST"));
			TargetHelper.ShieldBlockIDs.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "EmitterS"));
			TargetHelper.ShieldBlockIDs.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "EmitterL"));
			TargetHelper.ShieldBlockIDs.Add(new MyDefinitionId(typeof(MyObjectBuilder_UpgradeModule), "DS_Supergen"));
			TargetHelper.ShieldBlockIDs.Add(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "LargeShipSmallShieldGeneratorBase"));
			TargetHelper.ShieldBlockIDs.Add(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "LargeShipLargeShieldGeneratorBase"));
			TargetHelper.ShieldBlockIDs.Add(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "SmallShipSmallShieldGeneratorBase"));
			TargetHelper.ShieldBlockIDs.Add(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "SmallShipMicroShieldGeneratorBase"));

			//LogicManager.Setup();

			if(IsServer == false) {

				return;

			}

			Logger.LoadDebugFromSandbox();
			MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(75, DamageHelper.DamageHandler);

		}

		protected override void UnloadData() {

			if(ShieldApiLoaded) {

				SApi.Unload();
				Instance = null;

			}

			SyncManager.Close();
			DamageHelper.UnregisterEntityWatchers();

		}

	}

}