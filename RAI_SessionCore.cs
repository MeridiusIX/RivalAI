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
using RivalAI.Behavior;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI.Sync;
using RivalAI.Entities;

namespace RivalAI {

	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

	public class RAI_SessionCore:MySessionComponentBase {

		public static string ReleaseVersion = "0.22.6";

		//Server
		public static bool IsServer = false;
		public static bool IsDedicated = false;

		//Unstable Build
		//public static bool IsUnstable = false;
		//public static bool UnstableDetected = false;
		//public ulong StableBuildId = 0;
		//public ulong UnstableBuildId = 0;

		//ShieldApi Support
		internal static RAI_SessionCore Instance { get; private set; } // allow access from gamelogic
		public ulong ShieldModId = 1365616918;
		public bool ShieldMod { get; set; }
		public bool ShieldApiLoaded { get; set; }
		public ShieldApi SApi = new ShieldApi();
		public static string ConfigInstance = "";
		
		//WeaponCore Support
		public ulong WeaponCoreModId = 1918681825;
		public bool WeaponCoreMod { get; set; }
		public bool WeaponCoreLoaded { get; set; }
		public WcApi WeaponCore = new WcApi();

		public int Ticks = 0;

		public static bool SetupComplete = false;

		public override void LoadData() {

			if(MyAPIGateway.Multiplayer.IsServer == false)
				return;

			//if(MyAPIGateway.Utilities.GamePaths.ModScopeName.StartsWith("RivalAI (Unstable)"))

			Instance = this;

			foreach(var mod in MyAPIGateway.Session.Mods) {

				if(mod.PublishedFileId == ShieldModId) {

					Logger.WriteLog("Defense Shield Mod Detected");
					ShieldMod = true;
					continue;

				}
				
				if(mod.PublishedFileId == WeaponCoreModId || mod.Name.Contains("WeaponCore-Local")) {

					Logger.WriteLog("WeaponCore Mod Detected");
					WeaponCoreMod = true;

				}

			}

			MESApi.RegisterAPIListener();

		}

		public override void BeforeStart() {

			if (!MyAPIGateway.Multiplayer.IsServer)
				return;

			DebugTerminalControls.RegisterControls();

			//TODO: Register Shield and WeaponCore APIs
			try {

				Logger.WriteLog("Defense Shields API Loading");
				if (ShieldMod && !ShieldApiLoaded) {

					SApi.Load();

				}

			} catch (Exception exc) {

				Logger.WriteLog("Defense Shields API Failed To Load");
				Logger.WriteLog(exc.ToString());

			}
			
			try {

				if (WeaponCoreMod && !Instance.WeaponCore.IsReady) {

					Logger.WriteLog("WeaponCore API Loading");
					WeaponCore.Load(WcApiCallback, true);

				} else {

					Logger.WriteLog("WeaponCore API Failed To Load For Unknown Reason");

				}

			} catch (Exception exc) {

				Logger.WriteLog("WeaponCore API Failed To Load");
				Logger.WriteLog(exc.ToString());

			}

			
			
			Logger.LoadDebugFromSandbox();
			Utilities.GetDefinitionsAndIDs();
			TagHelper.Setup();
			DamageHelper.RegisterEntityWatchers();


		}

		public void WcApiCallback() {

			if (Instance.WeaponCore.IsReady) {

				Logger.WriteLog("WeaponCore API Successfully Loaded");
				WeaponCoreLoaded = true;
				Instance.WeaponCore.GetAllCoreWeapons(Utilities.AllWeaponCoreBlocks);
				Instance.WeaponCore.GetAllCoreStaticLaunchers(Utilities.AllWeaponCoreGuns);
				Instance.WeaponCore.GetAllCoreTurrets(Utilities.AllWeaponCoreTurrets);

			} else {

				Logger.WriteLog("WeaponCore API Failed To Load");

			}
		
		}

		public override void UpdateBeforeSimulation() {

			if(SetupComplete == false) {

				SetupComplete = true;
				Setup();
				Logger.MsgDebug("MES API Registered: " + MESApi.MESApiReady.ToString(), DebugTypeEnum.General);

			}

			Ticks++;

			if (IsServer)
				BehaviorManager.ProcessBehaviors();

			if (EffectManager.SoundsPlaying)
				EffectManager.ProcessAvatarDisplay();

			if (Ticks % 10 == 0) {

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

			if(!IsServer)
				return;

			EntityWatcher.RegisterWatcher();
			Logger.LoadDebugFromSandbox();
			MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(75, DamageHelper.DamageHandler);

		}

		protected override void UnloadData() {

			if(ShieldApiLoaded)
				SApi.Unload();

			if (WeaponCoreLoaded)
				WeaponCore.Unload();

			if (MESApi.MESApiReady)
				MESApi.UnregisterListener();

			DebugTerminalControls.RegisterControls(false);

			Instance = null;

			SyncManager.Close();
			DamageHelper.UnregisterEntityWatchers();
			BehaviorManager.Behaviors.Clear();
			EntityWatcher.UnregisterWatcher();

		}

	}

}
