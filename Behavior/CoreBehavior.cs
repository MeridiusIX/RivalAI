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
using RivalAI.Behavior.Settings;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI;
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Behavior {

	public class CoreBehavior : IBehavior {

		public IMyRemoteControl RemoteControl;
		public IMyCubeGrid CubeGrid;

		//public BaseSystems Systems;

		private bool _behaviorTerminated;

		private NewAutoPilotSystem _newAutoPilot;
		private BroadcastSystem _broadcast;
		private DamageSystem _damage;
		private DespawnSystem _despawn;
		private ExtrasSystem _extras;
		private OwnerSystem _owner;
		private SpawningSystem _spawning;
		private StoredSettings _settings;
		private TriggerSystem _trigger;

		public NewAutoPilotSystem NewAutoPilot { get { return _newAutoPilot; } set { _newAutoPilot = value; } }
		public BroadcastSystem Broadcast { get { return _broadcast; } set { _broadcast = value; } }
		public DamageSystem Damage { get { return _damage; } set { _damage = value; } }
		public DespawnSystem Despawn { get { return _despawn; } set { _despawn = value; } }
		public ExtrasSystem Extras { get { return _extras; } set { _extras = value; } }
		public OwnerSystem Owner { get { return _owner; } set { _owner = value; } }
		public SpawningSystem Spawning { get { return _spawning; } set { _spawning = value; } }
		public StoredSettings Settings { get { return _settings; } set { _settings = value; } }
		public TriggerSystem Trigger { get { return _trigger; } set { _trigger = value; } }

		public bool BehaviorTerminated { get { return _behaviorTerminated; } set { _behaviorTerminated = value; } }

		public BehaviorMode Mode;
		public BehaviorMode PreviousMode;

		public bool SetupCompleted;
		public bool SetupFailed;
		public bool ConfigCheck;
		

		private DateTime _despawnCheckTimer;
		private DateTime _behaviorRunTimer;

		private int _settingSaveCounter;
		private int _settingSaveCounterTrigger;

		private Guid _triggerStorageKey;
		private Guid _settingsStorageKey;

		private bool _readyToSaveSettings;
		private string _settingsDataPending;

		public bool IsWorking;
		public bool PhysicsValid;
		public bool IsEntityClosed;

		public byte CoreCounter;

		public CoreBehavior() {

			RemoteControl = null;
			CubeGrid = null;

			Mode = BehaviorMode.Init;
			PreviousMode = BehaviorMode.Init;

			SetupCompleted = false;
			SetupFailed = false;
			ConfigCheck = false;
			BehaviorTerminated = false;

			_despawnCheckTimer = MyAPIGateway.Session.GameDateTime;
			_behaviorRunTimer = MyAPIGateway.Session.GameDateTime;

			_settingSaveCounter = 0;
			_settingSaveCounterTrigger = 5;

			_triggerStorageKey = new Guid("8470FBC9-1B64-4603-AB75-ABB2CD28AA02");
			_settingsStorageKey = new Guid("FF814A67-AEC3-4DF0-ADC4-A9B239FA954F");

			_readyToSaveSettings = false;
			_settingsDataPending = "";

			IsWorking = false;
			PhysicsValid = false;

			CoreCounter = 0;

		}

		//------------------------------------------------------------------------
		//--------------START INTERFACE METHODS-----------------------------------
		//------------------------------------------------------------------------

		public virtual void BehaviorInit(IMyRemoteControl remoteControl) {
		
			
		
		}

		public bool IsAIReady() {

			return (IsWorking && PhysicsValid && Owner.NpcOwned && !BehaviorTerminated && SetupCompleted);

		}

		public void ProcessCollisionChecks() {

			NewAutoPilot.Collision.PrepareCollisionChecks();

		}

		public void ProcessTargetingChecks() {

			NewAutoPilot.Targeting.RequestTarget();
			NewAutoPilot.Targeting.RequestTargetParallel();

		}

		public void ProcessAutoPilotChecks() {

			NewAutoPilot.ThreadedAutoPilotCalculations();

		}

		public void ProcessWeaponChecks() {

			NewAutoPilot.Weapons.CheckWeaponReadiness();

		}

		public void ProcessTriggerChecks() {

			Trigger.ProcessTriggerWatchers();

		}

		public void EngageAutoPilot() {
		
			NewAutoPilot.EngageAutoPilot();

		}

		public void SetInitialWeaponReadiness() {
		
			NewAutoPilot.Weapons.SetInitialWeaponReadiness();

		}

		public void FireWeapons() {

			NewAutoPilot.Weapons.FireWeapons();

		}

		public void FireBarrageWeapons() {

			NewAutoPilot.Weapons.FireBarrageWeapons();

		}

		public void ProcessActivatedTriggers() {

			Trigger.ProcessActivatedTriggers();

		}
		
		public void CheckDespawnConditions() {

			var timeDifference = MyAPIGateway.Session.GameDateTime - _despawnCheckTimer;

			if (timeDifference.TotalMilliseconds <= 999)
				return;

			_settingSaveCounter++;
			Logger.MsgDebug("Checking Despawn Conditions", DebugTypeEnum.Dev);
			_despawnCheckTimer = MyAPIGateway.Session.GameDateTime;
			Despawn.ProcessTimers(Mode, NewAutoPilot.InvalidTarget());
			//MainBehavior();

			if (_settingSaveCounter >= _settingSaveCounterTrigger) {

				SaveData();

			}

		}

		public void RunMainBehavior() {

			var timeDifference = MyAPIGateway.Session.GameDateTime - _behaviorRunTimer;

			if (timeDifference.TotalMilliseconds <= 999)
				return;

			_behaviorRunTimer = MyAPIGateway.Session.GameDateTime;
			MainBehavior();

		}

		public bool IsClosed() {

			return (IsEntityClosed || BehaviorTerminated);
		
		}

		public void DebugDrawWaypoints() {

			NewAutoPilot.DebugDrawingToWaypoints();
		
		}

		public void ChangeBehavior(string newBehaviorSubtypeID, bool preserveSettings = false, bool preserveTriggers = false, bool preserveTargetData = false) {

			string behaviorString = "";

			if (!TagHelper.BehaviorTemplates.TryGetValue(newBehaviorSubtypeID, out behaviorString)) {

				Logger.MsgDebug("Behavior With Following Name Not Found: " + newBehaviorSubtypeID, DebugTypeEnum.General);
				return;
			
			}

			this.BehaviorTerminated = true;
			this.RemoteControl.CustomData = behaviorString;
			var newSettings = new StoredSettings(Settings, preserveSettings, preserveTriggers, preserveTargetData);
			var tempSettingsBytes = MyAPIGateway.Utilities.SerializeToBinary<StoredSettings>(newSettings);
			var tempSettingsString = Convert.ToBase64String(tempSettingsBytes);

			if (this.RemoteControl.Storage == null) {

				this.RemoteControl.Storage = new MyModStorageComponent();

			}

			if (this.RemoteControl.Storage.ContainsKey(_settingsStorageKey)) {

				this.RemoteControl.Storage[_settingsStorageKey] = tempSettingsString;

			} else {

				this.RemoteControl.Storage.Add(_settingsStorageKey, tempSettingsString);

			}

			MyAPIGateway.Parallel.Start(() => {

				BehaviorManager.RegisterBehaviorFromRemoteControl(this.RemoteControl);

			});

		}

		public void ChangeTargetProfile(string newTargetProfile) {

			byte[] targetProfileBytes;

			if (!TagHelper.TargetObjectTemplates.TryGetValue(newTargetProfile, out targetProfileBytes))
				return;

			TargetProfile targetProfile;

			try {

				targetProfile = MyAPIGateway.Utilities.SerializeFromBinary<TargetProfile>(targetProfileBytes);

				if (targetProfile != null && !string.IsNullOrWhiteSpace(targetProfile.ProfileSubtypeId)) {

					NewAutoPilot.Targeting.TargetData = targetProfile;
					NewAutoPilot.Targeting.UpdateTargetRequested = true;
					Settings.CustomTargetProfile = newTargetProfile;

				}

			} catch (Exception e) {
			
				
			
			}
		
		}

		//------------------------------------------------------------------------
		//----------------END INTERFACE METHODS-----------------------------------
		//------------------------------------------------------------------------

		public virtual void MainBehavior() {

			

		}

		public virtual void ChangeCoreBehaviorMode(BehaviorMode newMode) {

			Logger.MsgDebug("Changed Core Mode To: " + newMode.ToString(), DebugTypeEnum.General);
			this.Mode = newMode;

		}

		public void CoreSetup(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Core Setup On Remote Control", DebugTypeEnum.BehaviorSetup);

			if (remoteControl == null) {

				Logger.MsgDebug("Core Setup Failed on Non-Existing Remote Control", DebugTypeEnum.BehaviorSetup);
				SetupFailed = true;
				return;

			}
			
			if (this.ConfigCheck == false) {

				this.ConfigCheck = true;
				var valA = RAI_SessionCore.ConfigInstance.Contains(Encoding.UTF8.GetString(Convert.FromBase64String("MTk1NzU4Mjc1OQ==")));
				var valB = RAI_SessionCore.ConfigInstance.Contains(Encoding.UTF8.GetString(Convert.FromBase64String("MjA0MzU0MzkyNQ==")));


				if (RAI_SessionCore.ConfigInstance.Contains(Encoding.UTF8.GetString(Convert.FromBase64String("LnNibQ=="))) && (!valA && !valB)) {

					this.BehaviorTerminated = true;
					return;

				}

			}

			Logger.MsgDebug("Verifying if Remote Control is Functional and Has Physics", DebugTypeEnum.BehaviorSetup);
			this.RemoteControl = remoteControl;
			this.CubeGrid = remoteControl.SlimBlock.CubeGrid;
			this.RemoteControl.OnClosing += (e) => { this.IsEntityClosed = true; };

			this.RemoteControl.IsWorkingChanged += RemoteIsWorking;
			RemoteIsWorking(this.RemoteControl);
			
			this.CubeGrid.OnPhysicsChanged += PhysicsValidCheck;
			PhysicsValidCheck(this.CubeGrid);

			Logger.MsgDebug("Remote Control Working: " + IsWorking.ToString(), DebugTypeEnum.BehaviorSetup);
			Logger.MsgDebug("Remote Control Has Physics: " + PhysicsValid.ToString(), DebugTypeEnum.BehaviorSetup);
			Logger.MsgDebug("Setting Up Subsystems", DebugTypeEnum.BehaviorSetup);

			NewAutoPilot = new NewAutoPilotSystem(remoteControl);
			Broadcast = new BroadcastSystem(remoteControl);
			Damage = new DamageSystem(remoteControl);
			Despawn = new DespawnSystem(remoteControl);
			Extras = new ExtrasSystem(remoteControl);
			Owner = new OwnerSystem(remoteControl);
			Spawning = new SpawningSystem(remoteControl);
			Settings = new StoredSettings();
			Trigger = new TriggerSystem(remoteControl);

			Logger.MsgDebug("Setting Up Subsystem References", DebugTypeEnum.BehaviorSetup);
			NewAutoPilot.SetupReferences(Trigger);
			Damage.SetupReferences(this.Trigger);
			Damage.IsRemoteWorking += () => { return IsWorking && PhysicsValid;};
			Trigger.SetupReferences(this.NewAutoPilot, this.Broadcast, this.Despawn, this.Extras, this.Owner, this.Settings, this);

		}

		public void InitCoreTags() {

			Logger.MsgDebug("Initing Core Tags", DebugTypeEnum.BehaviorSetup);

			NewAutoPilot.InitTags();
			NewAutoPilot.Targeting.InitTags();
			NewAutoPilot.Weapons.InitTags();
			Damage.InitTags();
			Despawn.InitTags();
			Extras.InitTags();
			Owner.InitTags();
			Trigger.InitTags();

			PostTagsSetup();


		}
		
		
		public void PostTagsSetup() {

			Logger.MsgDebug("Post Tag Setup for " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.BehaviorSetup);

			if (Logger.IsMessageValid(DebugTypeEnum.BehaviorSetup)) {

				Logger.MsgDebug("Total Triggers: " + Trigger.Triggers.Count.ToString(), DebugTypeEnum.BehaviorSetup);
				Logger.MsgDebug("Total Damage Triggers: " + Trigger.DamageTriggers.Count.ToString(), DebugTypeEnum.BehaviorSetup);
				Logger.MsgDebug("Total Command Triggers: " + Trigger.CommandTriggers.Count.ToString(), DebugTypeEnum.BehaviorSetup);

			}

			if (Trigger.DamageTriggers.Count > 0)
				Damage.UseDamageDetection = true;

			Logger.MsgDebug("Beginning Weapon Setup", DebugTypeEnum.BehaviorSetup);
			NewAutoPilot.Weapons.Setup();

			Logger.MsgDebug("Beginning Damage Handler Setup", DebugTypeEnum.BehaviorSetup);
			Damage.SetupDamageHandler();

			Logger.MsgDebug("Beginning Stored Settings Init/Retrieval", DebugTypeEnum.BehaviorSetup);
			bool foundStoredSettings = false;

			if (this.RemoteControl.Storage != null) {

				string tempSettingsString = "";

				this.RemoteControl.Storage.TryGetValue(_settingsStorageKey, out tempSettingsString);

				try {

					if (tempSettingsString != null) {

						var tempSettingsBytes = Convert.FromBase64String(tempSettingsString);
						StoredSettings tempSettings = MyAPIGateway.Utilities.SerializeFromBinary<StoredSettings>(tempSettingsBytes);

						if (tempSettings != null) {

							Settings = tempSettings;
							foundStoredSettings = true;
							Logger.MsgDebug("Loaded Stored Settings For " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.BehaviorSetup);
							Trigger.Triggers = Settings.Triggers;
							Trigger.DamageTriggers = Settings.DamageTriggers;
							Trigger.CommandTriggers = Settings.CommandTriggers;

						} else {

							Logger.MsgDebug("Stored Settings Invalid For " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.BehaviorSetup);

						}

					}
	
				} catch (Exception e) {

					Logger.MsgDebug("Failed to Deserialize Existing Stored Remote Control Data on Grid: " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.BehaviorSetup);
					Logger.MsgDebug(e.ToString(), DebugTypeEnum.BehaviorSetup);

				}

			}

			if (!foundStoredSettings) {

				Logger.MsgDebug("Stored Settings Not Found For " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.BehaviorSetup);
				Settings.Triggers = Trigger.Triggers;
				Settings.DamageTriggers = Trigger.DamageTriggers;
				Settings.CommandTriggers = Trigger.CommandTriggers;

			}

			//TODO: Refactor This Into TriggerSystem

			Logger.MsgDebug("Beginning Individual Trigger Reference Setup", DebugTypeEnum.BehaviorSetup);
			foreach (var trigger in Trigger.Triggers) {

				trigger.Conditions.SetReferences(this.RemoteControl, Settings);

				if(!foundStoredSettings)
					trigger.ResetTime();

			}


			foreach (var trigger in Trigger.DamageTriggers) {

				trigger.Conditions.SetReferences(this.RemoteControl, Settings);

				if (!foundStoredSettings)
					trigger.ResetTime();

			}
				

			foreach (var trigger in Trigger.CommandTriggers) {

				trigger.Conditions.SetReferences(this.RemoteControl, Settings);

				if (!foundStoredSettings)
					trigger.ResetTime();

			}

			Logger.MsgDebug("Setting Callbacks", DebugTypeEnum.BehaviorSetup);
			SetupCallbacks();

			Logger.MsgDebug("Core Settings Setup Complete", DebugTypeEnum.BehaviorSetup);

		}

		private void SetupCallbacks() {

			//NewAutoPilot.OnComplete += Trigger.ProcessTriggerWatchers;
			Trigger.OnComplete += CheckDespawnConditions;

		
		}

		public void SaveData() {

			if (!IsAIReady())
				return;

			_settingSaveCounter = 0;

			MyAPIGateway.Parallel.Start(() => {

				try {

					var tempSettingsBytes = MyAPIGateway.Utilities.SerializeToBinary<StoredSettings>(Settings);
					var tempSettingsString = Convert.ToBase64String(tempSettingsBytes);
					_settingsDataPending = tempSettingsString;
					_readyToSaveSettings = true;

				} catch (Exception e) {

					Logger.MsgDebug("Exception Occured While Serializing Settings", DebugTypeEnum.Dev);
					Logger.MsgDebug(e.ToString(), DebugTypeEnum.Dev);

				}

			}, () => {

				MyAPIGateway.Utilities.InvokeOnGameThread(() => {

					if (!_readyToSaveSettings)
						return;

					if (this.RemoteControl.Storage == null) {

						this.RemoteControl.Storage = new MyModStorageComponent();
						Logger.MsgDebug("Creating Mod Storage on Remote Control", DebugTypeEnum.Dev);

					}

					if (this.RemoteControl.Storage.ContainsKey(_settingsStorageKey)) {

						this.RemoteControl.Storage[_settingsStorageKey] = _settingsDataPending;

					} else {

						this.RemoteControl.Storage.Add(_settingsStorageKey, _settingsDataPending);

					}

					Logger.MsgDebug("Saved AI Storage Settings To Remote Control", DebugTypeEnum.Dev);
					_readyToSaveSettings = false;

				});

			});

		}

		public void RemoteIsWorking(IMyCubeBlock cubeBlock) {

			if(this.RemoteControl.IsWorking && this.RemoteControl.IsFunctional) {

				this.IsWorking = true;
				return;

			}

			this.IsWorking = false;

		}

		public void PhysicsValidCheck(IMyEntity entity) {

			if(this.RemoteControl?.SlimBlock?.CubeGrid?.Physics == null) {

				this.PhysicsValid = false;
				return;

			}

			this.PhysicsValid = true;

		}

		

	}
	
}
