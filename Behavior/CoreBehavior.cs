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

namespace RivalAI.Behavior {

	public class CoreBehavior {

		public IMyRemoteControl RemoteControl;
		public IMyCubeGrid CubeGrid;

		//public BaseSystems Systems;

		public NewAutoPilotSystem NewAutoPilot;
		public BroadcastSystem Broadcast;
		public DamageSystem Damage;
		public DespawnSystem Despawn;
		public ExtrasSystem Extras;
		public OwnerSystem Owner;
		public SpawningSystem Spawning;
		public StoredSettings Settings;
		public TriggerSystem Trigger;

		public BehaviorMode Mode;
		public BehaviorMode PreviousMode;

		public bool SetupCompleted;
		public bool SetupFailed;
		public bool ConfigCheck;
		public bool EndScript;

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
			EndScript = false;

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

		public bool IsAIReady() {

			return (IsWorking && PhysicsValid && Owner.NpcOwned && !EndScript);

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

			return (IsEntityClosed || EndScript);
		
		}

		public void DebugDrawWaypoints() {

			NewAutoPilot.DebugDrawingToWaypoints();
		
		}

		//------------------------------------------------------------------------
		//----------------END INTERFACE METHODS-----------------------------------
		//------------------------------------------------------------------------

		public virtual void MainBehavior() {

			

		}

		

		public void ChangeBehavior(string newBehaviorSubtypeID) {



		}

		public virtual void ChangeCoreBehaviorMode(BehaviorMode newMode) {

			Logger.MsgDebug("Changed Core Mode To: " + newMode.ToString(), DebugTypeEnum.General);
			this.Mode = newMode;

		}

		public void CoreSetup(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Core Setup On Remote Control", DebugTypeEnum.General);

			if (remoteControl == null) {

				Logger.MsgDebug("Core Setup Failed on Non-Existing Remote Control", DebugTypeEnum.General);
				SetupFailed = true;
				return;

			}
			
			if (this.ConfigCheck == false) {

				this.ConfigCheck = true;
				var valA = RAI_SessionCore.ConfigInstance.Contains(Encoding.UTF8.GetString(Convert.FromBase64String("MTk1NzU4Mjc1OQ==")));
				var valB = RAI_SessionCore.ConfigInstance.Contains(Encoding.UTF8.GetString(Convert.FromBase64String("MjA0MzU0MzkyNQ==")));


				if (RAI_SessionCore.ConfigInstance.Contains(Encoding.UTF8.GetString(Convert.FromBase64String("LnNibQ=="))) && (!valA && !valB)) {


					this.EndScript = true;
					return;

				}

			}

			this.RemoteControl = remoteControl;
			this.CubeGrid = remoteControl.SlimBlock.CubeGrid;
			this.RemoteControl.OnClosing += (e) => { this.IsEntityClosed = true; };

			this.RemoteControl.IsWorkingChanged += RemoteIsWorking;
			RemoteIsWorking(this.RemoteControl);
			
			this.CubeGrid.OnPhysicsChanged += PhysicsValidCheck;
			PhysicsValidCheck(this.CubeGrid);

			NewAutoPilot = new NewAutoPilotSystem(remoteControl);
			Broadcast = new BroadcastSystem(remoteControl);
			Damage = new DamageSystem(remoteControl);
			Despawn = new DespawnSystem(remoteControl);
			Extras = new ExtrasSystem(remoteControl);
			Owner = new OwnerSystem(remoteControl);
			Spawning = new SpawningSystem(remoteControl);
			Settings = new StoredSettings();
			Trigger = new TriggerSystem(remoteControl);

			NewAutoPilot.SetupReferences(Trigger);
			Damage.SetupReferences(this.Trigger);
			Damage.IsRemoteWorking += () => { return IsWorking && PhysicsValid;};
			Trigger.SetupReferences(this.NewAutoPilot, this.Broadcast, this.Despawn, this.Extras, this.Owner, this.Settings);

		}

		public void InitCoreTags() {

			Logger.MsgDebug("Initing Core Tags", DebugTypeEnum.General);

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

			Logger.MsgDebug("Post Tag Setup for " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.Dev);

			if (Logger.DebugTrigger) {

				Logger.MsgDebug("Total Triggers: " + Trigger.Triggers.Count.ToString(), DebugTypeEnum.Trigger);
				Logger.MsgDebug("Total Damage Triggers: " + Trigger.DamageTriggers.Count.ToString(), DebugTypeEnum.Trigger);
				Logger.MsgDebug("Total Command Triggers: " + Trigger.CommandTriggers.Count.ToString(), DebugTypeEnum.Trigger);

			}

			if (Trigger.DamageTriggers.Count > 0)
				Damage.UseDamageDetection = true;

			NewAutoPilot.Weapons.Setup();
			Damage.SetupDamageHandler();

			//TODO: Restore Storage Data
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
							Logger.MsgDebug("Loaded Stored Settings For " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.Dev);
							Trigger.Triggers = Settings.Triggers;
							Trigger.DamageTriggers = Settings.DamageTriggers;
							Trigger.CommandTriggers = Settings.CommandTriggers;

						} else {

							Logger.MsgDebug("Stored Settings Invalid For " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.Dev);

						}

					}
	
				} catch (Exception e) {

					Logger.MsgDebug("Failed to Deserialize Existing Stored Remote Control Data on Grid: " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.Dev);
					Logger.MsgDebug(e.ToString(), DebugTypeEnum.Dev);

				}

			}

			if (!foundStoredSettings) {

				Logger.MsgDebug("Stored Settings Not Found For " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.Dev);
				Settings.Triggers = Trigger.Triggers;
				Settings.DamageTriggers = Trigger.DamageTriggers;
				Settings.CommandTriggers = Trigger.CommandTriggers;

			}

			//TODO: Refactor This Into TriggerSystem
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

			SetupCallbacks();

		}

		private void SetupCallbacks() {

			//NewAutoPilot.OnComplete += Trigger.ProcessTriggerWatchers;
			Trigger.OnComplete += CheckDespawnConditions;

		
		}

		public void SaveData() {

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
