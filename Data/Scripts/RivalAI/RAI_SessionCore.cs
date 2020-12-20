using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using RivalAI.Behavior;
using RivalAI.Helpers;
using RivalAI.Sync;
using RivalAI.Entities;

namespace RivalAI {

	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

	public class RAI_SessionCore:MySessionComponentBase {

		public static string ReleaseVersion = "0.39.0";

		//Server
		public static bool IsServer = false;
		public static bool IsDedicated = false;

		public static bool RivalAiEnabled = true;

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

		//Water Mod ID and API
		public ulong WaterModID = 2200451495;
		public WaterModAPI WaterMod = new WaterModAPI();

		public int Ticks = 0;

		public static bool SetupComplete = false;

		public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {
			base.Init(sessionComponent);

			if (MyAPIGateway.Multiplayer.IsServer) {

				WaterMod.Register("Modular Encounters Spawner");
				WaterMod.OnRegisteredEvent += WaterLogged;
				WaterMod.WaterCreatedEvent += WaterHelper.WaterAdded;
				WaterMod.WaterRemovedEvent += WaterHelper.WaterRemoved;

			}

		}

		public override void LoadData() {

			Logger.WriteLog("Mod Version: " + ReleaseVersion);

			if (MyAPIGateway.Multiplayer.IsServer == false)
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

			if (MyAPIGateway.Session.SessionSettings.EnableSelectivePhysicsUpdates && MyAPIGateway.Utilities.IsDedicated) {

				Logger.WriteLog("WARNING: Selective Physics Updates World Option Detected with RivalAI.");

				if (MyAPIGateway.Session.SessionSettings.SyncDistance < 10000) {

					Logger.WriteLog("Mod Disabled.");
					Logger.WriteLog("Sync Distance Must Be Set to 10000m or Higher for RivalAI to Function with Selective Physics Updates enabled.");
					Logger.WriteLog("Please Adjust World Settings and Restart Server.");
					RivalAiEnabled = false;
					return;

				}

				Logger.WriteLog("Encounters Using RivalAI May Not Work Correctly Outside of 10000m Sync Distance with Selective Physics Updates enabled.");
				Logger.WriteLog("Consider Increasing Sync Distnace if you Encounter Issues Outside Your Current Sync Distance Range.");

			}

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
			EntityWatcher.RegisterWatcher();

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

			if (!RivalAiEnabled) {

				MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
				return;

			}

			if (SetupComplete == false) {

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

			Logger.LoadDebugFromSandbox();
			MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(75, DamageHelper.DamageHandler);

		}

		public void WaterLogged() {

			Logger.WriteLog("Water Mod Detected and API Loaded.");

		}

		protected override void UnloadData() {

			if(ShieldApiLoaded)
				SApi.Unload();

			if (WeaponCoreLoaded)
				WeaponCore.Unload();

			if (MESApi.MESApiReady)
				MESApi.UnregisterListener();

			if (WaterMod.Registered) {

				WaterMod.Unregister();
				WaterMod.OnRegisteredEvent -= WaterLogged;
				WaterMod.WaterCreatedEvent -= WaterHelper.WaterAdded;
				WaterMod.WaterRemovedEvent -= WaterHelper.WaterRemoved;

			}

			DebugTerminalControls.RegisterControls(false);

			Instance = null;

			SyncManager.Close();
			DamageHelper.UnregisterEntityWatchers();
			BehaviorManager.Behaviors.Clear();
			EntityWatcher.UnregisterWatcher();

		}

	}

}
