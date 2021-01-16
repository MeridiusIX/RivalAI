using RivalAI.Behavior.Subsystems;
using RivalAI.Behavior.Subsystems.AutoPilot;
using RivalAI.Behavior.Subsystems.Trigger;
using RivalAI.Helpers;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace RivalAI.Behavior {

	public class CoreBehavior : IBehavior {

		public IMyRemoteControl RemoteControl { get { return _remoteControl; } set { _remoteControl = value; } }
		public IMyCubeGrid CubeGrid;

		public string RemoteControlCode;

		//public BaseSystems Systems;

		private bool _behaviorTerminated;

		private bool _registeredRemoteCode;
		private bool _despawnTriggersRegistered;

		private AutoPilotSystem _newAutoPilot;
		private BroadcastSystem _broadcast;
		private DamageSystem _damage;
		private DespawnSystem _despawn;
		private GridSystem _extras;
		private OwnerSystem _owner;
		private StoredSettings _settings;
		private TriggerSystem _trigger;

		private bool _behaviorTriggerA;
		private bool _behaviorTriggerB;
		private bool _behaviorTriggerC;
		private bool _behaviorTriggerD;
		private bool _behaviorTriggerE;
		private bool _behaviorTriggerF;
		private bool _behaviorTriggerG;
		private bool _behaviorTriggerH;

		private bool _behaviorActionA;
		private bool _behaviorActionB;
		private bool _behaviorActionC;
		private bool _behaviorActionD;
		private bool _behaviorActionE;
		private bool _behaviorActionF;
		private bool _behaviorActionG;
		private bool _behaviorActionH;

		internal string _behaviorType;

		private List<IMyCubeGrid> _currentGrids;
		private List<IMyCockpit> _debugCockpits;

		private IMyRemoteControl _remoteControl;

		public AutoPilotSystem AutoPilot { get { return _newAutoPilot; } set { _newAutoPilot = value; } }
		public BroadcastSystem Broadcast { get { return _broadcast; } set { _broadcast = value; } }
		public DamageSystem Damage { get { return _damage; } set { _damage = value; } }
		public DespawnSystem Despawn { get { return _despawn; } set { _despawn = value; } }
		public GridSystem Grid { get { return _extras; } set { _extras = value; } }
		public OwnerSystem Owner { get { return _owner; } set { _owner = value; } }
		public StoredSettings Settings { get { return _settings; } set { _settings = value; } }
		public TriggerSystem Trigger { get { return _trigger; } set { _trigger = value; } }

		public BehaviorMode Mode { 
			
			get {
				
				if(this.Settings != null)
					return this.Settings.Mode;

				return BehaviorMode.Init;
			
			}
			
			set {

				if (this.Settings != null)
					this.Settings.Mode = value;

			}
		
		}

		public bool BehaviorTerminated { get { return _behaviorTerminated; } set { _behaviorTerminated = value; } }
		public bool BehaviorTriggerA { get { return _behaviorTriggerA; } set { _behaviorTriggerA = value; } }
		public bool BehaviorTriggerB { get { return _behaviorTriggerB; } set { _behaviorTriggerB = value; } }
		public bool BehaviorTriggerC { get { return _behaviorTriggerC; } set { _behaviorTriggerC = value; } }
		public bool BehaviorTriggerD { get { return _behaviorTriggerD; } set { _behaviorTriggerD = value; } }
		public bool BehaviorTriggerE { get { return _behaviorTriggerE; } set { _behaviorTriggerE = value; } }
		public bool BehaviorTriggerF { get { return _behaviorTriggerF; } set { _behaviorTriggerF = value; } }
		public bool BehaviorTriggerG { get { return _behaviorTriggerG; } set { _behaviorTriggerG = value; } }
		public bool BehaviorTriggerH { get { return _behaviorTriggerH; } set { _behaviorTriggerH = value; } }

		public bool BehaviorActionA { get { return _behaviorActionA; } set { _behaviorActionA = value; } }
		public bool BehaviorActionB { get { return _behaviorActionB; } set { _behaviorActionB = value; } }
		public bool BehaviorActionC { get { return _behaviorActionC; } set { _behaviorActionC = value; } }
		public bool BehaviorActionD { get { return _behaviorActionD; } set { _behaviorActionD = value; } }
		public bool BehaviorActionE { get { return _behaviorActionE; } set { _behaviorActionE = value; } }
		public bool BehaviorActionF { get { return _behaviorActionF; } set { _behaviorActionF = value; } }
		public bool BehaviorActionG { get { return _behaviorActionG; } set { _behaviorActionG = value; } }
		public bool BehaviorActionH { get { return _behaviorActionH; } set { _behaviorActionH = value; } }

		public string BehaviorType { get { return _behaviorType; } }
		public long GridId { get { return RemoteControl?.SlimBlock?.CubeGrid == null ? 0 : RemoteControl.SlimBlock.CubeGrid.EntityId; } }
		public string GridName { get { return RemoteControl?.SlimBlock?.CubeGrid?.CustomName == null ? "N/A" : RemoteControl.SlimBlock.CubeGrid.CustomName; } }

		public List<IMyCubeGrid> CurrentGrids { get { return _currentGrids; } }

		public List<IMyCockpit> DebugCockpits { get { return _debugCockpits; } }

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
		public bool HasBeenWorking; //block was alive at one point
		public bool PhysicsValid;
		public bool HasHasValidPhysics;
		public bool IsEntityClosed;

		public bool IsParentGridClosed;

		public byte CoreCounter;

		public CoreBehavior() {

			RemoteControl = null;
			CubeGrid = null;

			RemoteControlCode = "";

			SetupCompleted = false;
			SetupFailed = false;
			ConfigCheck = false;
			BehaviorTerminated = false;

			_behaviorType = "";

			_despawnCheckTimer = MyAPIGateway.Session.GameDateTime;
			_behaviorRunTimer = MyAPIGateway.Session.GameDateTime;

			_settingSaveCounter = 0;
			_settingSaveCounterTrigger = 5;

			_currentGrids = new List<IMyCubeGrid>();
			_debugCockpits = new List<IMyCockpit>();

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

			AutoPilot.Collision.PrepareCollisionChecks();

		}

		public void ProcessTargetingChecks() {

			AutoPilot.Targeting.CheckForTarget();

		}

		public void ProcessAutoPilotChecks() {

			AutoPilot.ThreadedAutoPilotCalculations();
			AutoPilot.PrepareAutopilot();

		}

		public void ProcessWeaponChecks() {

			AutoPilot.Weapons.PrepareWeapons();

		}

		public void ProcessTriggerChecks() {

			Trigger.ProcessTriggerWatchers();

		}

		public void EngageAutoPilot() {
		
			AutoPilot.EngageAutoPilot();

		}

		public void SetDebugCockpit(IMyCockpit block, bool addMode = false) {

			if(addMode)
				_debugCockpits.Add(block);	
			else
				_debugCockpits.Remove(block);

		}

		public void SetInitialWeaponReadiness() {
			
			//Attempt Weapon Reloads
			AutoPilot.Weapons.ProcessWeaponReloads();

		}

		public void FireWeapons() {

			AutoPilot.Weapons.FireWeapons();

		}

		public void FireBarrageWeapons() {

			AutoPilot.Weapons.FireBarrageWeapons();

		}

		public void ProcessActivatedTriggers() {

			Trigger.ProcessActivatedTriggers();

		}
		
		public void CheckDespawnConditions() {

			var timeDifference = MyAPIGateway.Session.GameDateTime - _despawnCheckTimer;

			if (timeDifference.TotalMilliseconds <= 999)
				return;

			_settingSaveCounter++;
			//Logger.MsgDebug("Checking Despawn Conditions", DebugTypeEnum.Dev);
			_despawnCheckTimer = MyAPIGateway.Session.GameDateTime;
			Despawn.ProcessTimers(Mode, AutoPilot.InvalidTarget());
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

			AutoPilot.DebugDrawingToWaypoints();
		
		}

		public void ChangeBehavior(string newBehaviorSubtypeID, bool preserveSettings = false, bool preserveTriggers = false, bool preserveTargetData = false) {

			string behaviorString = "";

			if (!TagHelper.BehaviorTemplates.TryGetValue(newBehaviorSubtypeID, out behaviorString)) {

				Logger.MsgDebug("Behavior With Following Name Not Found: " + newBehaviorSubtypeID, DebugTypeEnum.General);
				return;
			
			}

			this.BehaviorTerminated = true;
			this.RemoteControl.CustomData = behaviorString;

			if (this.RemoteControl.Storage == null) {

				this.RemoteControl.Storage = new MyModStorageComponent();

			}

			if (preserveSettings) {

				Settings.State.DataMode = AutoPilotDataMode.Primary;
				Settings.State.AutoPilotFlags = NewAutoPilotMode.None;
				Settings.Mode = BehaviorMode.Init;
				var newSettings = new StoredSettings(Settings, preserveSettings, preserveTriggers, preserveTargetData);
				var tempSettingsBytes = MyAPIGateway.Utilities.SerializeToBinary<StoredSettings>(newSettings);
				var tempSettingsString = Convert.ToBase64String(tempSettingsBytes);

				if (this.RemoteControl.Storage.ContainsKey(_settingsStorageKey)) {

					this.RemoteControl.Storage[_settingsStorageKey] = tempSettingsString;

				} else {

					this.RemoteControl.Storage.Add(_settingsStorageKey, tempSettingsString);

				}

			} else {

				this.RemoteControl.Storage[_settingsStorageKey] = "";

			}

			MyAPIGateway.Parallel.Start(() => {

				BehaviorManager.RegisterBehaviorFromRemoteControl(this.RemoteControl);

			});

		}

		public void ChangeTargetProfile(string newTargetProfile) {

			AutoPilot.Targeting.UseNewTargetProfile = true;
			AutoPilot.Targeting.NewTargetProfileName = newTargetProfile;


		}

		//------------------------------------------------------------------------
		//----------------END INTERFACE METHODS-----------------------------------
		//------------------------------------------------------------------------

		public virtual void MainBehavior() {

			if (!_registeredRemoteCode) {

				_registeredRemoteCode = true;

				if (MESApi.MESApiReady && !string.IsNullOrWhiteSpace(RemoteControlCode)) {

					MESApi.RegisterRemoteControlCode(this.RemoteControl, RemoteControlCode);

				}
			
			}

			if (!_despawnTriggersRegistered) {

				_despawnTriggersRegistered = true;

				foreach (var trigger in Trigger.Triggers) {

					if (!MESApi.MESApiReady)
						break;

					if (trigger.Type == "DespawnMES") {

						MESApi.RegisterDespawnWatcher(this.RemoteControl?.SlimBlock?.CubeGrid, Trigger.DespawnFromMES);
						break;
					
					}
								
				}

			}

		}

		public virtual void ChangeCoreBehaviorMode(BehaviorMode newMode) {

			Logger.MsgDebug("Changed Core Mode To: " + newMode.ToString(), DebugTypeEnum.BehaviorMode);
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

			this.RemoteControl.OnClosing += RemoteIsClosing;
			
			this.CubeGrid.OnPhysicsChanged += PhysicsValidCheck;
			PhysicsValidCheck(this.CubeGrid);

			this.CubeGrid.OnMarkForClose += GridIsClosing;

			Logger.MsgDebug("Remote Control Working: " + IsWorking.ToString(), DebugTypeEnum.BehaviorSetup);
			Logger.MsgDebug("Remote Control Has Physics: " + PhysicsValid.ToString(), DebugTypeEnum.BehaviorSetup);
			Logger.MsgDebug("Setting Up Subsystems", DebugTypeEnum.BehaviorSetup);

			Settings = new StoredSettings();
			AutoPilot = new AutoPilotSystem(remoteControl, this);
			Broadcast = new BroadcastSystem(remoteControl);
			Damage = new DamageSystem(remoteControl);
			Despawn = new DespawnSystem(this, remoteControl);
			Grid = new GridSystem(remoteControl);
			Owner = new OwnerSystem(remoteControl);
			//Spawning = new SpawningSystem(remoteControl);
			Trigger = new TriggerSystem(remoteControl);

			Logger.MsgDebug("Setting Up Subsystem References", DebugTypeEnum.BehaviorSetup);
			AutoPilot.SetupReferences(this, Settings, Trigger);
			Damage.SetupReferences(this.Trigger);
			Damage.IsRemoteWorking += () => { return IsWorking && PhysicsValid;};
			Trigger.SetupReferences(this.AutoPilot, this.Broadcast, this.Despawn, this.Grid, this.Owner, this.Settings, this);

		}

		public void InitCoreTags() {

			Logger.MsgDebug("Initing Core Tags", DebugTypeEnum.BehaviorSetup);

			CoreTags();
			AutoPilot.InitTags();
			AutoPilot.Weapons.InitTags();
			Damage.InitTags();
			Despawn.InitTags();
			Owner.InitTags();
			Trigger.InitTags();

			PostTagsSetup();


		}

		public void CoreTags() {

			if (string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach (var tag in descSplit) {

					//RemoteControlCode
					if (tag.Contains("[RemoteControlCode:") == true) {

						this.RemoteControlCode = TagHelper.TagStringCheck(tag);

					}

				}

			}

		}
		
		
		public void PostTagsSetup() {


			if (BehaviorType != "Passive") {

				Logger.MsgDebug("Setting Inertia Dampeners: " + (AutoPilot.Data.DisableInertiaDampeners ? "False" : "True"), DebugTypeEnum.BehaviorSetup);
				RemoteControl.DampenersOverride = !AutoPilot.Data.DisableInertiaDampeners;

			}
	
			Logger.MsgDebug("Post Tag Setup for " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.BehaviorSetup);

			if (Logger.IsMessageValid(DebugTypeEnum.BehaviorSetup)) {

				Logger.MsgDebug("Total Triggers: " + Trigger.Triggers.Count.ToString(), DebugTypeEnum.BehaviorSetup);
				Logger.MsgDebug("Total Damage Triggers: " + Trigger.DamageTriggers.Count.ToString(), DebugTypeEnum.BehaviorSetup);
				Logger.MsgDebug("Total Command Triggers: " + Trigger.CommandTriggers.Count.ToString(), DebugTypeEnum.BehaviorSetup);
				Logger.MsgDebug("Total Compromised Triggers: " + Trigger.CompromisedTriggers.Count.ToString(), DebugTypeEnum.BehaviorSetup);

			}

			if (Trigger.DamageTriggers.Count > 0)
				Damage.UseDamageDetection = true;

			Logger.MsgDebug("Beginning Weapon Setup", DebugTypeEnum.BehaviorSetup);
			AutoPilot.Weapons.Setup();

			Logger.MsgDebug("Beginning Damage Handler Setup", DebugTypeEnum.BehaviorSetup);
			Damage.SetupDamageHandler();

			Logger.MsgDebug("Beginning Stored Settings Init/Retrieval", DebugTypeEnum.BehaviorSetup);
			bool foundStoredSettings = false;

			if (this.RemoteControl.Storage != null) {

				string tempSettingsString = "";

				this.RemoteControl.Storage.TryGetValue(_settingsStorageKey, out tempSettingsString);

				try {

					if (!string.IsNullOrWhiteSpace(tempSettingsString)) {

						var tempSettingsBytes = Convert.FromBase64String(tempSettingsString);
						StoredSettings tempSettings = MyAPIGateway.Utilities.SerializeFromBinary<StoredSettings>(tempSettingsBytes);

						if (tempSettings != null) {

							Settings = tempSettings;
							foundStoredSettings = true;
							Logger.MsgDebug("Loaded Stored Settings For " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.BehaviorSetup);
							Logger.MsgDebug("Stored Settings BehaviorMode: " + Settings.Mode.ToString(), DebugTypeEnum.BehaviorSetup);

							if (!Settings.IgnoreTriggers) {

								Trigger.Triggers = Settings.Triggers;
								Trigger.DamageTriggers = Settings.DamageTriggers;
								Trigger.CommandTriggers = Settings.CommandTriggers;
								Trigger.CompromisedTriggers = Settings.CompromisedTriggers;

							} else {

								Settings.IgnoreTriggers = false;

							}

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
				Settings.CompromisedTriggers = Trigger.CompromisedTriggers;

			} else {

				var sb = new StringBuilder();
				sb.Append("Checking and Displaying Existing Stored Booleans and Counters").AppendLine();

				if (Settings.StoredCustomBooleans != null && Settings.StoredCustomBooleans.Keys.Count > 0) {

					sb.Append("Stored Custom Booleans:").AppendLine();

					foreach (var name in Settings.StoredCustomBooleans.Keys) {

						if (string.IsNullOrWhiteSpace(name))
							continue;

						bool result = false;

						if (Settings.StoredCustomBooleans.TryGetValue(name, out result)) {

							sb.Append(string.Format(" - [{0}] == [{1}]", name, result)).AppendLine();

						}
					
					}

				}

				if (Settings.StoredCustomCounters != null && Settings.StoredCustomCounters.Keys.Count > 0) {

					sb.Append("Stored Custom Counters:").AppendLine();

					foreach (var name in Settings.StoredCustomCounters.Keys) {

						if (string.IsNullOrWhiteSpace(name))
							continue;

						int result = 0;

						if (Settings.StoredCustomCounters.TryGetValue(name, out result)) {

							sb.Append(string.Format(" - [{0}] == [{1}]", name, result)).AppendLine();

						}

					}

				}

				Logger.MsgDebug(sb.ToString(), DebugTypeEnum.BehaviorSetup);

			}

			//TODO: Refactor This Into TriggerSystem

			Logger.MsgDebug("Beginning Individual Trigger Reference Setup", DebugTypeEnum.BehaviorSetup);
			foreach (var trigger in Trigger.Triggers) {

				trigger.Conditions.SetReferences(this.RemoteControl, Settings);

				if (!string.IsNullOrWhiteSpace(trigger.ActionsDefunct?.ProfileSubtypeId))
					trigger.Actions.Add(trigger.ActionsDefunct);

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

			foreach (var trigger in Trigger.CompromisedTriggers) {

				trigger.Conditions.SetReferences(this.RemoteControl, Settings);

				if (!foundStoredSettings)
					trigger.ResetTime();

			}

			Logger.MsgDebug("Setting Callbacks", DebugTypeEnum.BehaviorSetup);
			SetupCallbacks();

			Logger.MsgDebug("Setting Grid Split Check", DebugTypeEnum.BehaviorSetup);
			RemoteControl.SlimBlock.CubeGrid.OnGridSplit += GridSplit;
			_currentGrids = MyAPIGateway.GridGroups.GetGroup(RemoteControl.SlimBlock.CubeGrid, GridLinkTypeEnum.Physical);

			Logger.MsgDebug("Behavior Mode Set To: " + Mode.ToString(), DebugTypeEnum.BehaviorSetup); 
			Logger.MsgDebug("Core Settings Setup Complete", DebugTypeEnum.BehaviorSetup);

			if (Settings.CurrentTargetEntityId != 0) {

				AutoPilot.Targeting.ForceTargetEntityId = Settings.CurrentTargetEntityId;

			}


		}

		internal void SetDefaultTargeting() {

			var savedTarget = !string.IsNullOrWhiteSpace(Settings.CustomTargetProfile);
			var targetProfileName = !savedTarget ? "RivalAI-GenericTargetProfile-EnemyPlayer" : Settings.CustomTargetProfile;

			if (savedTarget || string.IsNullOrWhiteSpace(AutoPilot.Targeting.NormalData.ProfileSubtypeId)) {

				byte[] byteData = { };

				if (TagHelper.TargetObjectTemplates.TryGetValue(targetProfileName, out byteData) == true) {

					try {

						var profile = MyAPIGateway.Utilities.SerializeFromBinary<TargetProfile>(byteData);

						if (profile != null) {

							AutoPilot.Targeting.NormalData = profile;

						}

					} catch (Exception) {



					}

				}

			}

			if (string.IsNullOrWhiteSpace(AutoPilot.Targeting.OverrideData.ProfileSubtypeId)) {

				byte[] byteData = { };

				if (TagHelper.TargetObjectTemplates.TryGetValue("RivalAI-GenericTargetProfile-EnemyOverride", out byteData) == true) {

					try {

						var profile = MyAPIGateway.Utilities.SerializeFromBinary<TargetProfile>(byteData);

						if (profile != null) {

							AutoPilot.Targeting.OverrideData = profile;

						}

					} catch (Exception) {



					}

				}

			}

		}

		private void GridSplit(IMyCubeGrid a, IMyCubeGrid b) {

			a.OnGridSplit -= GridSplit;
			b.OnGridSplit -= GridSplit;
			_currentGrids.Clear();

			if (RemoteControl == null || RemoteControl.MarkedForClose)
				return;

			RemoteControl.SlimBlock.CubeGrid.OnGridSplit += GridSplit;

			_currentGrids = MyAPIGateway.GridGroups.GetGroup(RemoteControl.SlimBlock.CubeGrid, GridLinkTypeEnum.Physical);

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

					Logger.MsgDebug("Exception Occured While Serializing Settings", DebugTypeEnum.General);
					Logger.MsgDebug(e.ToString(), DebugTypeEnum.General);

				}

			}, () => {

				MyAPIGateway.Utilities.InvokeOnGameThread(() => {

					if (!_readyToSaveSettings)
						return;

					if (this.RemoteControl.Storage == null) {

						this.RemoteControl.Storage = new MyModStorageComponent();
						Logger.MsgDebug("Creating Mod Storage on Remote Control", DebugTypeEnum.General);

					}

					if (this.RemoteControl.Storage.ContainsKey(_settingsStorageKey)) {

						this.RemoteControl.Storage[_settingsStorageKey] = _settingsDataPending;

					} else {

						this.RemoteControl.Storage.Add(_settingsStorageKey, _settingsDataPending);

					}

					Logger.MsgDebug("Saved AI Storage Settings To Remote Control", DebugTypeEnum.General);
					_readyToSaveSettings = false;

				});

			});

		}

		public void RemoteIsWorking(IMyCubeBlock cubeBlock) {

			if (this.RemoteControl == null || this.RemoteControl.MarkedForClose) {

				this.IsWorking = false;

				if(Trigger != null)
					Trigger.ProcessCompromisedTriggerWatcher(RemoteCompromiseCheck());

			}

			if(this.RemoteControl.IsWorking && this.RemoteControl.IsFunctional) {

				this.HasBeenWorking = true;
				this.IsWorking = true;
				return;

			}

			this.IsWorking = false;

			if (Trigger != null)
				Trigger.ProcessCompromisedTriggerWatcher(RemoteCompromiseCheck());

		}

		public void RemoteIsClosing(IMyEntity entity) {

			if (Trigger != null)
				Trigger.ProcessCompromisedTriggerWatcher(RemoteCompromiseCheck());

		}

		public void GridIsClosing(IMyEntity entity) {

			IsParentGridClosed = true;

		}

		public bool RemoteCompromiseCheck() {

			return !IsWorking && HasBeenWorking && !IsParentGridClosed;
		
		}

		public void PhysicsValidCheck(IMyEntity entity) {

			if(this.RemoteControl?.SlimBlock?.CubeGrid?.Physics == null) {

				this.PhysicsValid = false;
				return;

			}

			this.HasHasValidPhysics = true;
			this.PhysicsValid = true;

		}

		

	}
	
}
