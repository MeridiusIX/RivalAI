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
using RivalAI.Helpers;
using RivalAI.Behavior.Subsystems.Profiles;
using RivalAI.Sync;

namespace RivalAI.Behavior.Subsystems {

	public class TriggerSystem{

		public IMyRemoteControl RemoteControl;
		public List<IMyRadioAntenna> AntennaList = new List<IMyRadioAntenna>();
		public List<IMyLargeTurretBase> TurretList = new List<IMyLargeTurretBase>();

		private IBehavior _behavior;

		private NewAutoPilotSystem _autopilot;
		private BroadcastSystem _broadcast;
		private DespawnSystem _despawn;
		private ExtrasSystem _extras;
		private OwnerSystem _owner;
		private StoredSettings _settings;

		public List<TriggerProfile> Triggers;
		public List<TriggerProfile> DamageTriggers;
		public List<TriggerProfile> CommandTriggers;
		public List<TriggerProfile> CompromisedTriggers;
		public List<string> ExistingTriggers;

		public bool RemoteControlCompromised;

		public bool TimedTriggersProcessed;

		public bool CommandListenerRegistered;
		public bool DamageHandlerRegistered;
		public MyDamageInformation DamageInfo;
		public bool PendingDamage;

		public Action BehaviorEventA;
		public Action BehaviorEventB;
		public Action BehaviorEventC;
		public Action BehaviorEventD;
		public Action BehaviorEventE;
		public Action BehaviorEventF;
		public Action BehaviorEventG;
		public Action BehaviorEventH;

		public DateTime LastTriggerRun;

		public Action OnComplete;


		public TriggerSystem(IMyRemoteControl remoteControl){

			RemoteControl = null;
			AntennaList = new List<IMyRadioAntenna>();
			TurretList = new List<IMyLargeTurretBase>();

			Triggers = new List<TriggerProfile>();
			DamageTriggers = new List<TriggerProfile>();
			CommandTriggers = new List<TriggerProfile>();
			CompromisedTriggers = new List<TriggerProfile>();
			ExistingTriggers = new List<string>();

			RemoteControlCompromised = false;

			TimedTriggersProcessed = false;

			CommandListenerRegistered = false;

			LastTriggerRun = MyAPIGateway.Session.GameDateTime;

			Setup(remoteControl);

		}

		public void ProcessTriggerWatchers() {

			var timeDifference = MyAPIGateway.Session.GameDateTime - this.LastTriggerRun;

			if (timeDifference.TotalMilliseconds < 500) {

				Logger.MsgDebug("Triggers Not Ready (total ms elapsed: "+ timeDifference.TotalMilliseconds.ToString() + "), Handing Off to Next Action", DebugTypeEnum.Dev);
				//OnComplete?.Invoke();
				return;
			
			}

			Logger.MsgDebug("Checking Triggers", DebugTypeEnum.Dev);
			this.LastTriggerRun = MyAPIGateway.Session.GameDateTime;

			for (int i = 0; i < this.Triggers.Count; i++) {

				var trigger = this.Triggers[i];

				//Timer
				if (trigger.Type == "Timer") {

					//Logger.MsgDebug("Checking Timer Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger == true) {

						trigger.ActivateTrigger();

					}

					continue;

				}

				//PlayerNear
				if (trigger.Type == "PlayerNear") {

					//Logger.MsgDebug("Checking PlayerNear Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger == true) {

						if (IsPlayerNearby(trigger)) {

							trigger.ActivateTrigger();

						}

					}

					continue;

				}

				//TurretTarget
				if (trigger.Type == "TurretTarget") {

					//Logger.MsgDebug("Checking TurretTarget Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger == true) {

						var turretTarget = _autopilot.Weapons.GetTurretTarget();

						if (turretTarget != 0) {

							trigger.ActivateTrigger();

							if (trigger.Triggered == true) {

								trigger.DetectedEntityId = turretTarget;

							}

						}

					}

					continue;

				}

				//NoWeapon
				if (trigger.Type == "NoWeapon") {

					//Logger.MsgDebug("Checking NoWeapon Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger && !_autopilot.Weapons.HasWorkingWeapons()) {

						trigger.ActivateTrigger();

					}

					continue;

				}

				//NoTarget
				if (trigger.Type == "NoTarget") {

					//Logger.MsgDebug("Checking NoTarget Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger) {

						if (!_autopilot.Targeting.HasTarget()) {

							trigger.ActivateTrigger();

						}

					}

					continue;

				}

				//HasTarget
				if (trigger.Type == "HasTarget") {

					//Logger.MsgDebug("Checking HasTarget Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger) {

						if (_autopilot.Targeting.HasTarget()) {

							trigger.ActivateTrigger();

						}

					}

					continue;

				}

				//AcquiredTarget
				if (trigger.Type == "AcquiredTarget") {

					//Logger.MsgDebug("Checking NoTarget Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger) {

						if (_autopilot.Targeting.TargetAcquired) {

							_autopilot.Targeting.TargetAcquired = false;
							trigger.ActivateTrigger();

						}

					}

					continue;

				}

				//LostTarget
				if (trigger.Type == "LostTarget") {

					//Logger.MsgDebug("Checking HasTarget Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger) {

						if (_autopilot.Targeting.TargetLost) {

							_autopilot.Targeting.TargetLost = false;
							trigger.ActivateTrigger();

						}

					}

					continue;

				}

				//TargetInSafezone
				if (trigger.Type == "TargetInSafezone") {

					//Logger.MsgDebug("Checking TargetInSafezone Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger == true) {

						if (_autopilot.Targeting.HasTarget() && _autopilot.Targeting.Target.InSafeZone()) {

							trigger.ActivateTrigger();

						}

					}

					continue;

				}

				//Grounded
				if (trigger.Type == "Grounded") {

					if (trigger.UseTrigger == true) {

						//Check if Grounded
						trigger.ActivateTrigger();

					}

					continue;

				}

				//BehaviorTriggerA
				if (trigger.Type == "BehaviorTriggerA") {

					if (trigger.UseTrigger == true && _behavior.BehaviorTriggerA) {

						_behavior.BehaviorTriggerA = false;
						trigger.ActivateTrigger();

					}

					continue;

				}

				//BehaviorTriggerB
				if (trigger.Type == "BehaviorTriggerB") {

					if (trigger.UseTrigger == true && _behavior.BehaviorTriggerB) {

						_behavior.BehaviorTriggerB = false;
						trigger.ActivateTrigger();

					}

					continue;

				}

			}

			TimedTriggersProcessed = true;

		}

		public void ProcessActivatedTriggers() {

			if (!TimedTriggersProcessed)
				return;

			TimedTriggersProcessed = false;

			for (int i = 0; i < this.Triggers.Count; i++) {

				ProcessTrigger(this.Triggers[i]);

			}

			//Logger.MsgDebug("Trigger Actions Complete", DebugTypeEnum.Actions);
			//this.OnComplete?.Invoke();

		}

		public void ProcessDamageTriggerWatchers(object target, MyDamageInformation info) {

			if (!_behavior.IsAIReady())
				return;

			Logger.MsgDebug("Damage Trigger Count: " + this.DamageTriggers.Count.ToString(), DebugTypeEnum.Trigger);
			if (info.Amount <= 0)
				return;

			_settings.LastDamageTakenTime = MyAPIGateway.Session.GameDateTime;
			_settings.TotalDamageAccumulated += info.Amount;

			for (int i = 0;i < this.DamageTriggers.Count;i++) {

				//Logger.AddMsg("Got Trigger Profile", true);

				var trigger = this.DamageTriggers[i];

				if(trigger.DamageTypes.Contains(info.Type.ToString()) || trigger.DamageTypes.Contains("Any")) {

					if(trigger.UseTrigger == true) {

						trigger.ActivateTrigger();

						if(trigger.Triggered == true) {

							Logger.MsgDebug("Process Damage Actions", DebugTypeEnum.Trigger);
							ProcessTrigger(trigger, info.AttackerId, true);

						}

					}

				}

			}

		}

		public void ProcessCommandReceiveTriggerWatcher(string commandCode, IMyRemoteControl senderRemote, double radius, long entityId) {

			if (!_behavior.IsAIReady())
				return;

			if (senderRemote?.SlimBlock?.CubeGrid == null || this.RemoteControl?.SlimBlock?.CubeGrid == null)
				return;
			
			var antenna = BlockHelper.GetActiveAntenna(this.AntennaList);
			
			if(antenna == null)
				return;
			
			if(Vector3D.Distance(this.RemoteControl.GetPosition(), senderRemote.GetPosition()) > radius)
				return;
			
			for(int i = 0;i < this.CommandTriggers.Count;i++) {

				var trigger = this.CommandTriggers[i];

				if(trigger.UseTrigger == true && commandCode == trigger.CommandReceiveCode) {

					trigger.ActivateTrigger();

					if(trigger.Triggered == true) {

						ProcessTrigger(trigger, entityId, true);

					}

				}
 
			}

		}

		public void ProcessCompromisedTriggerWatcher(bool validToProcess) {

			if (!validToProcess || RemoteControlCompromised)
				return;

			this.RemoteControlCompromised = true;

			for (int i = 0; i < this.CompromisedTriggers.Count; i++) {

				var trigger = this.CompromisedTriggers[i];

				if (trigger.UseTrigger == true) {

					trigger.ActivateTrigger();

					if (trigger.Triggered == true) {

						ProcessTrigger(trigger);

					}

				}

			}

		}

		public void ProcessTrigger(TriggerProfile trigger, long attackerEntityId = 0, bool skipOnComplete = false) {

			if (this.RemoteControl?.SlimBlock?.CubeGrid == null)
				return;

			if(trigger.Triggered == false || trigger.Actions == null) {

				return;

			}

			long detectedEntity = attackerEntityId;

			if (trigger.DetectedEntityId != 0 && detectedEntity == 0) {

				detectedEntity = trigger.DetectedEntityId;

			}

			trigger.DetectedEntityId = 0;
			trigger.Triggered = false;
			trigger.CooldownTime = trigger.Rnd.Next((int)trigger.MinCooldownMs, (int)trigger.MaxCooldownMs);
			trigger.LastTriggerTime = MyAPIGateway.Session.GameDateTime;
			trigger.TriggerCount++;

			Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Performing Eligible Actions", DebugTypeEnum.Action);

			//ChatBroadcast
			if (trigger.Actions.UseChatBroadcast == true) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Chat Broadcast", DebugTypeEnum.Action);
				_broadcast.BroadcastRequest(trigger.Actions.ChatData);

			}

			//BarrellRoll - Implement Post Release
			if(trigger.Actions.BarrelRoll == true) {

				//_autopilot.ChangeAutoPilotMode(AutoPilotMode.BarrelRoll);

			}

			//Strafe - Implement Post Release
			if(trigger.Actions.Strafe == true) {

				//_autopilot.ChangeAutoPilotMode(AutoPilotMode.Strafe);

			}

			//ChangeAutopilotSpeed
			if(trigger.Actions.ChangeAutopilotSpeed == true) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Changing AutoPilot Speed To: " + trigger.Actions.NewAutopilotSpeed.ToString(), DebugTypeEnum.Action);
				_autopilot.IdealMaxSpeed = trigger.Actions.NewAutopilotSpeed;
				var blockList = TargetHelper.GetAllBlocks(RemoteControl.SlimBlock.CubeGrid);

				foreach (var block in blockList.Where(x => x.FatBlock != null)) {

					var tBlock = block.FatBlock as IMyRemoteControl;

					if (tBlock != null) {

						tBlock.SpeedLimit = trigger.Actions.NewAutopilotSpeed;

					}

				}

			}

			//SpawnReinforcements
			if (trigger.Actions.SpawnEncounter == true && trigger.Actions.Spawner.UseSpawn) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Spawn", DebugTypeEnum.Spawn);
				if (trigger.Actions.Spawner.IsReadyToSpawn()) {

					//Logger.AddMsg("Do Spawn", true);
					trigger.Actions.Spawner.CurrentPositionMatrix = this.RemoteControl.WorldMatrix;
					trigger.Actions.Spawner.CurrentFactionTag = (trigger.Actions.Spawner.ForceSameFactionOwnership && !string.IsNullOrWhiteSpace(_owner.Faction?.Tag)) ? _owner.Faction.Tag : "";
					SpawnHelper.SpawnRequest(trigger.Actions.Spawner);

				}

			} else {



			}

			//SelfDestruct
			if(trigger.Actions.SelfDestruct == true) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting SelfDestruct", DebugTypeEnum.Action);
				var blockList = TargetHelper.GetAllBlocks(RemoteControl.SlimBlock.CubeGrid);
				int totalWarheads = 0;
				
				foreach(var block in blockList.Where(x => x.FatBlock != null)) {

					var tBlock = block.FatBlock as IMyWarhead;

					if(tBlock != null) {

						if (!trigger.Actions.StaggerWarheadDetonation) {

							tBlock.IsArmed = true;
							tBlock.DetonationTime = 0;
							tBlock.Detonate();
							totalWarheads++;

						} else {

							tBlock.DetonationTime = totalWarheads + 1;
							tBlock.StartCountdown();
							totalWarheads++;

						}
						
					}

				}

				//Logger.AddMsg("TotalBlocks:  " + blockList.Count.ToString(), true);
				//Logger.AddMsg("TotalWarheads: " + totalWarheads.ToString(), true);

				//TODO: Shield EMP

			}

			//Retreat
			if(trigger.Actions.Retreat) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Retreat", DebugTypeEnum.Action);
				_despawn.Retreat();

			}

			//ForceDespawn
			if (trigger.Actions.ForceDespawn) {

				_despawn.DespawnGrid();

			}

			//TerminateBehavior
			if (trigger.Actions.TerminateBehavior) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Termination Of Behavior", DebugTypeEnum.Action);
				_autopilot.ActivateAutoPilot(AutoPilotType.None, NewAutoPilotMode.None, Vector3D.Zero);
				_behavior.BehaviorTerminated = true;

			}

			//BroadcastGenericCommand
			if (trigger.Actions.BroadcastGenericCommand == true) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Broadcast of Generic Command", DebugTypeEnum.Action);
				var antenna = BlockHelper.GetAntennaWithHighestRange(this.AntennaList);

				if (antenna != null)
					CommandHelper.CommandTrigger?.Invoke(trigger.Actions.BroadcastSendCode, this.RemoteControl, (double)antenna.Radius, 0);

			}

			//BroadcastCurrentTarget
			if (trigger.Actions.BroadcastCurrentTarget == true && detectedEntity != 0) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Broadcast of Current Target", DebugTypeEnum.Action);
				var antenna = BlockHelper.GetAntennaWithHighestRange(this.AntennaList);
				
				if(antenna != null)
					CommandHelper.CommandTrigger?.Invoke(trigger.Actions.BroadcastSendCode, this.RemoteControl, (double)antenna.Radius, detectedEntity);

			}

			//SwitchToReceivedTarget
			if (trigger.Actions.SwitchToReceivedTarget == true && detectedEntity != 0) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Switch to Received Target Data", DebugTypeEnum.Action);
				_autopilot.Targeting.ForceTargetEntity = detectedEntity;
				_autopilot.Targeting.ForceRefresh = true;

			}

			//SwitchToBehavior
			if(trigger.Actions.SwitchToBehavior == true) {

				_behavior.ChangeBehavior(trigger.Actions.NewBehavior, trigger.Actions.PreserveSettingsOnBehaviorSwitch, trigger.Actions.PreserveTriggersOnBehaviorSwitch, trigger.Actions.PreserveTargetDataOnBehaviorSwitch);

			}

			//RefreshTarget
			if(trigger.Actions.RefreshTarget == true) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Target Refresh", DebugTypeEnum.Action);
				_autopilot.Targeting.ForceRefresh = true;

			}

			//ChangeTargetProfile
			if (trigger.Actions.ChangeTargetProfile == true) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Target Change", DebugTypeEnum.Action);
				_autopilot.Targeting.UseNewTargetProfile = true;
				_autopilot.Targeting.NewTargetProfileName = trigger.Actions.NewTargetProfileId;

			}

			//ChangeReputationWithPlayers
			if (trigger.Actions.ChangeReputationWithPlayers == true) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Reputation Change With Players In Radius", DebugTypeEnum.Action);
				OwnershipHelper.ChangeReputationWithPlayersInRadius(this.RemoteControl, trigger.Actions.ReputationChangeRadius, trigger.Actions.ReputationChangeAmount, trigger.Actions.ReputationChangeFactions, trigger.Actions.ReputationChangesForAllRadiusPlayerFactionMembers);

			}

			//ChangeAttackerReputation
			if (trigger.Actions.ChangeAttackerReputation == true && detectedEntity != 0) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Reputation Change for Attacker", DebugTypeEnum.Action);
				OwnershipHelper.ChangeDamageOwnerReputation(trigger.Actions.ChangeAttackerReputationFaction, detectedEntity, trigger.Actions.ChangeAttackerReputationAmount, trigger.Actions.ReputationChangesForAllAttackPlayerFactionMembers);

			}


			//TriggerTimerBlock
			if (trigger.Actions.TriggerTimerBlocks == true) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Trigger of Timer Blocks", DebugTypeEnum.Action);
				var blockList = BlockHelper.GetBlocksWithNames(RemoteControl.SlimBlock.CubeGrid, trigger.Actions.TimerBlockNames);

				foreach(var block in blockList) {

					var tBlock = block as IMyTimerBlock;

					if(tBlock != null) {

						tBlock.Trigger();

					}

				}

			}

			//ChangeBlockNames
			if (trigger.Actions.ChangeBlockNames) {

				BlockHelper.RenameBlocks(RemoteControl.CubeGrid, trigger.Actions.ChangeBlockNamesFrom, trigger.Actions.ChangeBlockNamesTo, trigger.Actions.ProfileSubtypeId);

			}

			//ChangeAntennaRanges
			if (trigger.Actions.ChangeAntennaRanges) {

				BlockHelper.SetGridAntennaRanges(AntennaList, trigger.Actions.AntennaNamesForRangeChange, trigger.Actions.AntennaRangeChangeType, trigger.Actions.AntennaRangeChangeAmount);

			}

			//ChangeAntennaOwnership
			if (trigger.Actions.ChangeAntennaOwnership == true) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Antenna Ownership Change", DebugTypeEnum.Action);
				OwnershipHelper.ChangeAntennaBlockOwnership(AntennaList, trigger.Actions.AntennaFactionOwner);

			}

			//CreateKnownPlayerArea
			if (trigger.Actions.CreateKnownPlayerArea == true) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Creation of Known Player Area in MES", DebugTypeEnum.Action);
				MESApi.AddKnownPlayerLocation(this.RemoteControl.GetPosition(), _owner.Faction?.Tag, trigger.Actions.KnownPlayerAreaRadius, trigger.Actions.KnownPlayerAreaTimer, trigger.Actions.KnownPlayerAreaMaxSpawns);

			}

			//DamageAttacker
			if (trigger.Actions.DamageToolAttacker == true && detectedEntity != 0) {

				Logger.MsgDebug(trigger.Actions.ProfileSubtypeId + ": Attempting Damage to Tool User", DebugTypeEnum.Action);
				DamageHelper.ApplyDamageToTarget(attackerEntityId, trigger.Actions.DamageToolAttackerAmount, trigger.Actions.DamageToolAttackerParticle, trigger.Actions.DamageToolAttackerSound);

			}

			//PlayParticleEffectAtRemote
			if (trigger.Actions.PlayParticleEffectAtRemote == true) {

				EffectManager.SendParticleEffectRequest(trigger.Actions.ParticleEffectId, this.RemoteControl.WorldMatrix, trigger.Actions.ParticleEffectOffset, trigger.Actions.ParticleEffectScale, trigger.Actions.ParticleEffectMaxTime, trigger.Actions.ParticleEffectColor);
					
			}

			//ResetCooldownTimeOfTriggers
			if (trigger.Actions.ResetCooldownTimeOfTriggers) {

				foreach (var resetTrigger in Triggers) {

					if (trigger.Actions.ResetTriggerCooldownNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.LastTriggerTime = MyAPIGateway.Session.GameDateTime;

				}

				foreach (var resetTrigger in DamageTriggers) {

					if (trigger.Actions.ResetTriggerCooldownNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.LastTriggerTime = MyAPIGateway.Session.GameDateTime;

				}

				foreach (var resetTrigger in CommandTriggers) {

					if (trigger.Actions.ResetTriggerCooldownNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.LastTriggerTime = MyAPIGateway.Session.GameDateTime;

				}

			}

			if (trigger.Actions.EnableTriggers) {

				foreach (var resetTrigger in Triggers) {

					if (trigger.Actions.EnableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = true;

				}

				foreach (var resetTrigger in DamageTriggers) {

					if (trigger.Actions.EnableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = true;

				}

				foreach (var resetTrigger in CommandTriggers) {

					if (trigger.Actions.EnableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = true;

				}

			}

			if (trigger.Actions.DisableTriggers) {

				foreach (var resetTrigger in Triggers) {

					if (trigger.Actions.DisableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = false;

				}

				foreach (var resetTrigger in DamageTriggers) {

					if (trigger.Actions.DisableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = false;

				}

				foreach (var resetTrigger in CommandTriggers) {

					if (trigger.Actions.DisableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = false;

				}

			}

			//ChangeInertiaDampeners
			if (trigger.Actions.ChangeInertiaDampeners) {

				RemoteControl.DampenersOverride = trigger.Actions.InertiaDampenersEnable;

			}

			//ChangeRotationDirection
			if (trigger.Actions.ChangeRotationDirection) {

				_behavior.Settings.SetRotation(trigger.Actions.RotationDirection);

			}

			//GenerateExplosion
			if (trigger.Actions.GenerateExplosion) {

				var coords = Vector3D.Transform(trigger.Actions.ExplosionOffsetFromRemote, RemoteControl.WorldMatrix);
				MyVisualScriptLogicProvider.CreateExplosion(coords, trigger.Actions.ExplosionRange, trigger.Actions.ExplosionDamage);

			}

			//SetBooleansTrue
			foreach (var variable in trigger.Actions.SetBooleansTrue)
				_settings.SetCustomBool(variable, true);

			//SetBooleansFalse
			foreach (var variable in trigger.Actions.SetBooleansFalse)
				_settings.SetCustomBool(variable, false);

			//IncreaseCounters
			foreach (var variable in trigger.Actions.IncreaseCounters)
				_settings.SetCustomCounter(variable, 1);

			//DecreaseCounters
			foreach (var variable in trigger.Actions.DecreaseCounters)
				_settings.SetCustomCounter(variable, -1);

			//ResetCounters
			foreach (var variable in trigger.Actions.ResetCounters)
				_settings.SetCustomCounter(variable, 0, true);

			//SetSandboxBooleansTrue
			foreach (var variable in trigger.Actions.SetSandboxBooleansTrue)
				SetSandboxBool(variable, true);

			//SetSandboxBooleansFalse
			foreach (var variable in trigger.Actions.SetSandboxBooleansFalse)
				SetSandboxBool(variable, false);

			//IncreaseSandboxCounters
			foreach (var variable in trigger.Actions.IncreaseSandboxCounters)
				SetSandboxCounter(variable, 1);

			//DecreaseSandboxCounters
			foreach (var variable in trigger.Actions.DecreaseSandboxCounters)
				SetSandboxCounter(variable, -1);

			//ResetSandboxCounters
			foreach (var variable in trigger.Actions.ResetSandboxCounters)
				SetSandboxCounter(variable, 0);

			//BehaviorSpecificEventA
			if (trigger.Actions.BehaviorSpecificEventA)
				BehaviorEventA?.Invoke();

			//BehaviorSpecificEventB
			if (trigger.Actions.BehaviorSpecificEventB)
				BehaviorEventB?.Invoke();

			//BehaviorSpecificEventC
			if (trigger.Actions.BehaviorSpecificEventC)
				BehaviorEventC?.Invoke();

			//BehaviorSpecificEventD
			if (trigger.Actions.BehaviorSpecificEventD)
				BehaviorEventD?.Invoke();

			//BehaviorSpecificEventE
			if (trigger.Actions.BehaviorSpecificEventE)
				BehaviorEventE?.Invoke();

			//BehaviorSpecificEventF
			if (trigger.Actions.BehaviorSpecificEventF)
				BehaviorEventF?.Invoke();

			//BehaviorSpecificEventG
			if (trigger.Actions.BehaviorSpecificEventG)
				BehaviorEventG?.Invoke();

			//BehaviorSpecificEventH
			if (trigger.Actions.BehaviorSpecificEventH)
				BehaviorEventH?.Invoke();

		}

		public void SetSandboxBool(string boolName, bool mode) {

			MyAPIGateway.Utilities.SetVariable<bool>(boolName, mode);
		
		}

		public void SetSandboxCounter(string counterName, int amount) {

			int existingCounter = 0;

			MyAPIGateway.Utilities.GetVariable<int>(counterName, out existingCounter);

			if (amount == 0) {

				MyAPIGateway.Utilities.SetVariable<int>(counterName, 0);
				return;

			}

			if (amount == 1) {

				MyAPIGateway.Utilities.SetVariable<int>(counterName, existingCounter++);
				return;

			}

			if (amount == -1) {

				existingCounter--;
				MyAPIGateway.Utilities.SetVariable<int>(counterName, existingCounter < 0 ? 0 : existingCounter);
				return;

			}


		}

		public bool IsPlayerNearby(TriggerProfile control) {

			IMyPlayer player = null;

			var remotePosition = Vector3D.Transform(control.PlayerNearPositionOffset, this.RemoteControl.WorldMatrix);

			if(control.MinPlayerReputation != -1501 || control.MaxPlayerReputation != 1501) {

				player = TargetHelper.GetClosestPlayerWithReputation(remotePosition, _owner.FactionId, control);

			} else {

				player = TargetHelper.GetClosestPlayer(remotePosition);

			}

			if(player == null) {

				//Logger.MsgDebug(control.ProfileSubtypeId + ": No Eligible Player for PlayerNear Check", DebugTypeEnum.Trigger);
				return false;

			}

			var playerDist = Vector3D.Distance(player.GetPosition(), remotePosition);

			if(playerDist > control.TargetDistance) {

				//Logger.MsgDebug(control.ProfileSubtypeId + ": Nearest Player Too Far: " + playerDist.ToString(), DebugTypeEnum.Trigger);
				return false;

			}

			if(control.InsideAntenna == true) {

				var antenna = BlockHelper.GetAntennaWithHighestRange(this.AntennaList, control.InsideAntennaName);

				if(antenna != null) {

					playerDist = Vector3D.Distance(player.GetPosition(), antenna.GetPosition());
					if(playerDist > antenna.Radius) {

						return false;

					}

				} else {

					return false;

				}

			}

			return true;

		}

		public void Setup(IMyRemoteControl remoteControl) {

			if(remoteControl?.SlimBlock == null) {

				return;

			}

			this.RemoteControl = remoteControl;

			this.AntennaList = BlockHelper.GetGridAntennas(this.RemoteControl.SlimBlock.CubeGrid);

		}

		public void SetupReferences(NewAutoPilotSystem autopilot, BroadcastSystem broadcast, DespawnSystem despawn, ExtrasSystem extras, OwnerSystem owners, StoredSettings settings, IBehavior behavior) {

			this._autopilot = autopilot;
			this._broadcast = broadcast;
			this._despawn = despawn;
			this._extras = extras;
			this._owner = owners;
			this._settings = settings;
			this._behavior = behavior;

		}
		
		public void RegisterCommandListener(){
		
			if(this.CommandListenerRegistered)
				return;
			
			this.CommandListenerRegistered = true;
			CommandHelper.CommandTrigger += ProcessCommandReceiveTriggerWatcher;

		}

		public bool AddTrigger(TriggerProfile trigger) {

			if (ExistingTriggers.Contains(trigger.ProfileSubtypeId)) {

				Logger.MsgDebug("Trigger Already Added: " + trigger.ProfileSubtypeId, DebugTypeEnum.BehaviorSetup);
				return false;

			}
				

			ExistingTriggers.Add(trigger.ProfileSubtypeId);

			if (trigger.Type == "Damage") {

				this.DamageTriggers.Add(trigger);
				return true;

			}

			if (trigger.Type == "CommandReceived") {

				this.CommandTriggers.Add(trigger);
				RegisterCommandListener();
				return true;

			}

			if (trigger.Type == "Compromised") {

				this.CompromisedTriggers.Add(trigger);
				return true;

			}

			this.Triggers.Add(trigger);
			return true;

		}

		public void InitTags() {

			//TODO: Try To Get Triggers From Block Storage At Start

			//Start With This Class
			if(string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == true) {

				return;

			}

			var descSplit = this.RemoteControl.CustomData.Split('\n');

			foreach(var tag in descSplit) {

				//Triggers
				if(tag.Contains("[Triggers:") == true) {

					bool gotTrigger = false;
					var tempValue = TagHelper.TagStringCheck(tag);

					if(string.IsNullOrWhiteSpace(tempValue) == false) {

						byte[] byteData = { };

						if(TagHelper.TriggerObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

							try {

								var profile = MyAPIGateway.Utilities.SerializeFromBinary<TriggerProfile>(byteData);

								if(profile != null) {

									gotTrigger = AddTrigger(profile);

								}

							} catch(Exception e) {

								Logger.MsgDebug("Exception In Trigger Setup for Tag: " + tag, DebugTypeEnum.BehaviorSetup);
								Logger.MsgDebug(e.ToString(), DebugTypeEnum.BehaviorSetup);

							}

						}

					}

					if (!gotTrigger)
						Logger.MsgDebug("Could Not Find Trigger Profile Associated To Tag: " + tag, DebugTypeEnum.BehaviorSetup);

				}

				//TriggerGroups
				if (tag.Contains("[TriggerGroups:") == true) {

					bool gotTrigger = false;
					var tempValue = TagHelper.TagStringCheck(tag);

					if (string.IsNullOrWhiteSpace(tempValue) == false) {

						byte[] byteData = { };

						if (TagHelper.TriggerGroupObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

							try {

								var profile = MyAPIGateway.Utilities.SerializeFromBinary<TriggerGroupProfile>(byteData);

								if (profile != null) {

									foreach (var trigger in profile.Triggers) {

										AddTrigger(trigger);

									}

								}

							} catch (Exception) {



							}

						}

					}

					if (!gotTrigger)
						Logger.WriteLog("Could Not Find Trigger Profile Associated To Tag: " + tag);

				}

			}

		}

	}

}
