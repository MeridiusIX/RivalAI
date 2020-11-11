using RivalAI.Behavior.Subsystems.AutoPilot;
using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace RivalAI.Behavior.Subsystems.Trigger {

	public partial class TriggerSystem {

		public IMyRemoteControl RemoteControl;
		public List<IMyRadioAntenna> AntennaList = new List<IMyRadioAntenna>();
		public List<IMyLargeTurretBase> TurretList = new List<IMyLargeTurretBase>();

		private IBehavior _behavior;

		private AutoPilotSystem _autopilot;
		private BroadcastSystem _broadcast;
		private DespawnSystem _despawn;
		private GridSystem _extras;
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

		public bool PaymentSuccessTriggered;
		public bool PaymentFailureTriggered;

		public DateTime LastTriggerRun;

		public Action OnComplete;


		public TriggerSystem(IMyRemoteControl remoteControl) {

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

			var timeDifference = MyAPIGateway.Session.GameDateTime - LastTriggerRun;

			if (timeDifference.TotalMilliseconds < 500) {

				//Logger.MsgDebug("Triggers Not Ready (total ms elapsed: "+ timeDifference.TotalMilliseconds.ToString() + "), Handing Off to Next Action", DebugTypeEnum.Dev);
				//OnComplete?.Invoke();
				return;

			}

			//Logger.MsgDebug("Checking Triggers", DebugTypeEnum.Dev);
			LastTriggerRun = MyAPIGateway.Session.GameDateTime;

			for (int i = 0; i < Triggers.Count; i++) {

				var trigger = Triggers[i];

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

				//PlayerFar
				if (trigger.Type == "PlayerFar") {

					//Logger.MsgDebug("Checking PlayerNear Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger == true) {

						if (IsPlayerNearby(trigger, true)) {

							trigger.ActivateTrigger();

						}

					}

					continue;

				}

				//TargetNear
				if (trigger.Type == "TargetNear") {

					//Logger.MsgDebug("Checking PlayerNear Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger == true) {

						if (_behavior.AutoPilot.Targeting.HasTarget() && Vector3D.Distance(RemoteControl.GetPosition(), _behavior.AutoPilot.Targeting.TargetLastKnownCoords) < trigger.TargetDistance) {

							trigger.ActivateTrigger();

						}

					}

					continue;

				}

				//TargetFar
				if (trigger.Type == "TargetFar") {

					//Logger.MsgDebug("Checking PlayerNear Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger == true) {

						if (_behavior.AutoPilot.Targeting.HasTarget() && Vector3D.Distance(RemoteControl.GetPosition(), _behavior.AutoPilot.Targeting.TargetLastKnownCoords) > trigger.TargetDistance) {

							trigger.ActivateTrigger();

						}

					}

					continue;

				}

				//DespawnNear
				if (trigger.Type == "DespawnNear") {

					//Logger.MsgDebug("Checking DespawnNear Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger == true) {

						if (_behavior.Settings.DespawnCoords != Vector3D.Zero && Vector3D.Distance(RemoteControl.GetPosition(), _behavior.Settings.DespawnCoords) < trigger.TargetDistance) {

							trigger.ActivateTrigger();

						}

					}

					continue;

				}

				//DespawnFar
				if (trigger.Type == "DespawnFar") {

					//Logger.MsgDebug("Checking DespawnFar Trigger: " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);
					if (trigger.UseTrigger == true) {

						if (_behavior.Settings.DespawnCoords != Vector3D.Zero && Vector3D.Distance(RemoteControl.GetPosition(), _behavior.Settings.DespawnCoords) > trigger.TargetDistance) {

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

						
						trigger.ActivateTrigger();

					}

					continue;

				}

				//BehaviorTriggerB
				if (trigger.Type == "BehaviorTriggerB") {

					if (trigger.UseTrigger == true && _behavior.BehaviorTriggerB) {

						
						trigger.ActivateTrigger();

					}

					continue;

				}

				//BehaviorTriggerC
				if (trigger.Type == "BehaviorTriggerC") {

					if (trigger.UseTrigger == true && _behavior.BehaviorTriggerC) {

						trigger.ActivateTrigger();

					}

					continue;

				}

				//BehaviorTriggerD
				if (trigger.Type == "BehaviorTriggerD") {

					if (trigger.UseTrigger == true && _behavior.BehaviorTriggerD) {

						trigger.ActivateTrigger();

					}

					continue;

				}

				//BehaviorTriggerE
				if (trigger.Type == "BehaviorTriggerE") {

					if (trigger.UseTrigger == true && _behavior.BehaviorTriggerE) {

						trigger.ActivateTrigger();

					}

					continue;

				}

				//BehaviorTriggerF
				if (trigger.Type == "BehaviorTriggerF") {

					if (trigger.UseTrigger == true && _behavior.BehaviorTriggerF) {


						trigger.ActivateTrigger();

					}

					continue;

				}

				//BehaviorTriggerG
				if (trigger.Type == "BehaviorTriggerG") {

					if (trigger.UseTrigger == true && _behavior.BehaviorTriggerG) {


						trigger.ActivateTrigger();

					}

					continue;

				}

				//PaymentSuccess
				if (trigger.Type == "PaymentSuccess") {

					if (trigger.UseTrigger == true && PaymentSuccessTriggered) {


						trigger.ActivateTrigger();

					}

					continue;

				}

				//PaymentFailure
				if (trigger.Type == "PaymentFailure") {

					if (trigger.UseTrigger == true && PaymentFailureTriggered) {

						trigger.ActivateTrigger();

					}

					continue;

				}

			}

			_behavior.BehaviorTriggerA = false;
			_behavior.BehaviorTriggerB = false;
			_behavior.BehaviorTriggerC = false;
			_behavior.BehaviorTriggerD = false;
			_behavior.BehaviorTriggerE = false;
			_behavior.BehaviorTriggerF = false;
			_behavior.BehaviorTriggerG = false;
			PaymentSuccessTriggered = false;
			PaymentFailureTriggered = false;
			TimedTriggersProcessed = true;

		}

		public void ProcessActivatedTriggers() {

			if (!TimedTriggersProcessed)
				return;

			TimedTriggersProcessed = false;

			for (int i = 0; i < Triggers.Count; i++) {

				ProcessTrigger(Triggers[i]);

			}

			//Logger.MsgDebug("Trigger Actions Complete", DebugTypeEnum.Actions);
			//this.OnComplete?.Invoke();

		}

		public void ProcessDamageTriggerWatchers(object target, MyDamageInformation info) {

			if (!_behavior.IsAIReady())
				return;

			//Logger.MsgDebug("Damage Trigger Count: " + this.DamageTriggers.Count.ToString(), DebugTypeEnum.Trigger);
			if (info.Amount <= 0)
				return;

			_settings.LastDamageTakenTime = MyAPIGateway.Session.GameDateTime;
			_settings.TotalDamageAccumulated += info.Amount;
			_settings.LastDamagerEntity = info.AttackerId;

			for (int i = 0; i < DamageTriggers.Count; i++) {

				//Logger.AddMsg("Got Trigger Profile", true);

				var trigger = DamageTriggers[i];

				var damageType = info.Type.ToString();

				if ((trigger.DamageTypes.Contains(damageType) || trigger.DamageTypes.Contains("Any")) && !trigger.ExcludedDamageTypes.Contains(damageType)) {

					if (trigger.UseTrigger == true) {

						trigger.ActivateTrigger();

						if (trigger.Triggered == true) {

							Logger.MsgDebug("Process Damage Actions", DebugTypeEnum.Trigger);
							ProcessTrigger(trigger, info.AttackerId);

						}

					}

				}

			}

		}

		public void ProcessCommandReceiveTriggerWatcher(Command command) {

			if (!_behavior.IsAIReady()) {

				Logger.MsgDebug("Behavior AI Not Ready", DebugTypeEnum.Command);
				return;

			}

			if (command == null) {

				Logger.MsgDebug("Command Null", DebugTypeEnum.Command);
				return;

			}

			if (string.IsNullOrWhiteSpace(command.CommandCode)) {

				Logger.MsgDebug("Command Code Null or Blank", DebugTypeEnum.Command);
				return;

			}

			if (CommandTriggers == null) {

				Logger.MsgDebug("No Eligible Command Triggers", DebugTypeEnum.Command);
				return;

			}


			if (command.SenderEntity?.PositionComp == null || RemoteControl?.SlimBlock?.CubeGrid == null) {

				Logger.MsgDebug("Sender Remote CubeGrid Null or Receiver Remote CubeGrid Null", DebugTypeEnum.Command);
				return;

			}

			var dist = Vector3D.Distance(RemoteControl.GetPosition(), command.RemoteControl.GetPosition());

			if (!command.UseTriggerTargetDistance) {

				if (!command.IgnoreAntennaRequirement) {

					var antenna = _behavior.Grid.GetActiveAntenna();

					if (antenna == null) {

						Logger.MsgDebug("Receiver Has No Antenna", DebugTypeEnum.Command);
						return;

					}

				}

				if (dist > command.Radius) {

					Logger.MsgDebug("Receiver Has No Antenna", DebugTypeEnum.Command);
					return;

				}

			}

			for (int i = 0; i < CommandTriggers.Count; i++) {

				var trigger = CommandTriggers[i];

				if (trigger.CommandCodeType != command.Type)
					continue;

				if (command.UseTriggerTargetDistance && dist > trigger.TargetDistance)
					continue;

				bool commandCodePass = !trigger.AllowCommandCodePartialMatch ? (command.CommandCode.ToLower() == trigger.CommandReceiveCode.ToLower()) : (command.CommandCode.ToLower().Contains(trigger.CommandReceiveCode.ToLower()));

				if (trigger.UseTrigger == true && commandCodePass) {

					trigger.ActivateTrigger();

					if (trigger.Triggered == true) {

						ProcessTrigger(trigger, 0, command);

					}

				}

			}

		}

		public void ProcessCompromisedTriggerWatcher(bool validToProcess) {

			if (!validToProcess || RemoteControlCompromised)
				return;

			RemoteControlCompromised = true;

			for (int i = 0; i < CompromisedTriggers.Count; i++) {

				var trigger = CompromisedTriggers[i];

				if (trigger.UseTrigger == true) {

					trigger.ActivateTrigger();

					if (trigger.Triggered == true) {

						ProcessTrigger(trigger);

					}

				}

			}

		}

		public void ProcessRetreatTriggers() {

			for (int i = 0; i < Triggers.Count; i++) {

				var trigger = Triggers[i];

				if (trigger.UseTrigger == true && trigger.Type == "Retreat") {

					trigger.ActivateTrigger();

					if (trigger.Triggered == true) {

						ProcessTrigger(trigger);

					}

				}

			}

		}

		public void ProcessDespawnTriggers() {

			for (int i = 0; i < Triggers.Count; i++) {

				var trigger = Triggers[i];

				if (trigger.UseTrigger == true && trigger.Type == "Despawn") {

					trigger.ActivateTrigger();

					if (trigger.Triggered == true) {

						ProcessTrigger(trigger);

					}

				}

			}

		}

		public void ProcessManualTrigger(TriggerProfile trigger, bool forceActivation = false) {

			Logger.MsgDebug("Attempting To Manually Trigger Profile " + trigger.ProfileSubtypeId, DebugTypeEnum.Trigger);

			if (trigger.UseTrigger) {

				trigger.ActivateTrigger();

				if (!trigger.Triggered && forceActivation)
					trigger.Triggered = true;

				if (trigger.Triggered) {

					ProcessTrigger(trigger);

				}

			}

		}

		public void ProcessTrigger(TriggerProfile trigger, long attackerEntityId = 0, Command command = null) {

			if (RemoteControl?.SlimBlock?.CubeGrid == null)
				return;

			if (trigger.Triggered == false || trigger.Actions == null || trigger.Actions.Count == 0) {

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

			if (trigger.ActionExecution == ActionExecutionEnum.All) {

				foreach (var actions in trigger.Actions) {

					ProcessAction(trigger, actions, attackerEntityId, detectedEntity, command);

				}

			}

			if (trigger.ActionExecution == ActionExecutionEnum.Sequential) {

				if (trigger.NextActionIndex >= trigger.Actions.Count)
					trigger.NextActionIndex = 0;

				ProcessAction(trigger, trigger.Actions[trigger.NextActionIndex], attackerEntityId, detectedEntity, command);
				trigger.NextActionIndex++;

			}

			if (trigger.ActionExecution == ActionExecutionEnum.Random) {

				if (trigger.Actions.Count == 1) {

					ProcessAction(trigger, trigger.Actions[0], attackerEntityId, detectedEntity, command);

				} else {

					ProcessAction(trigger, trigger.Actions[MathTools.RandomBetween(0, trigger.Actions.Count)], attackerEntityId, detectedEntity, command);

				}

			}

		}

		public void SetSandboxBool(string boolName, bool mode) {

			MyAPIGateway.Utilities.SetVariable(boolName, mode);

		}

		public void SetSandboxCounter(string counterName, int amount, bool hardSet = false) {

			if (hardSet) {

				MyAPIGateway.Utilities.SetVariable(counterName, amount);
				return;

			}

			int existingCounter = 0;

			MyAPIGateway.Utilities.GetVariable(counterName, out existingCounter);

			if (amount == 0) {

				MyAPIGateway.Utilities.SetVariable(counterName, 0);
				return;

			}

			if (amount == 1) {

				existingCounter++;
				MyAPIGateway.Utilities.SetVariable(counterName, existingCounter);
				return;

			}

			if (amount == -1) {

				existingCounter--;
				MyAPIGateway.Utilities.SetVariable(counterName, existingCounter < 0 ? 0 : existingCounter);
				return;

			}


		}

		public bool IsPlayerNearby(TriggerProfile control, bool playerOutsideDistance = false) {

			IMyPlayer player = null;

			var remotePosition = Vector3D.Transform(control.PlayerNearPositionOffset, RemoteControl.WorldMatrix);

			if (control.MinPlayerReputation != -1501 || control.MaxPlayerReputation != 1501) {

				player = TargetHelper.GetClosestPlayerWithReputation(remotePosition, _owner.FactionId, control);

			} else {

				player = TargetHelper.GetClosestPlayer(remotePosition);

			}

			if (player == null) {

				//Logger.MsgDebug(control.ProfileSubtypeId + ": No Eligible Player for PlayerNear Check", DebugTypeEnum.Trigger);
				return false;

			}

			var playerDist = Vector3D.Distance(player.GetPosition(), remotePosition);

			if (playerOutsideDistance) {

				if (playerDist < control.TargetDistance) {

					return false;

				}

			} else {

				if (playerDist > control.TargetDistance) {

					return false;

				}

			}

			

			if (control.InsideAntenna == true) {

				var antenna = _behavior.Grid.GetAntennaWithHighestRange(control.InsideAntennaName);

				if (antenna != null) {

					playerDist = Vector3D.Distance(player.GetPosition(), antenna.GetPosition());
					if (playerDist > antenna.Radius) {

						return false;

					}

				} else {

					return false;

				}

			}

			return true;

		}

		public void Setup(IMyRemoteControl remoteControl) {

			if (remoteControl?.SlimBlock == null) {

				return;

			}

			RemoteControl = remoteControl;

			AntennaList = BlockHelper.GetGridAntennas(RemoteControl.SlimBlock.CubeGrid);

		}

		public void SetupReferences(AutoPilotSystem autopilot, BroadcastSystem broadcast, DespawnSystem despawn, GridSystem extras, OwnerSystem owners, StoredSettings settings, IBehavior behavior) {

			_autopilot = autopilot;
			_broadcast = broadcast;
			_despawn = despawn;
			_extras = extras;
			_owner = owners;
			_settings = settings;
			_behavior = behavior;

		}

		public void RegisterCommandListener() {

			if (CommandListenerRegistered)
				return;

			CommandListenerRegistered = true;
			CommandHelper.CommandTrigger += ProcessCommandReceiveTriggerWatcher;

		}

		public bool AddTrigger(TriggerProfile trigger) {

			if (ExistingTriggers.Contains(trigger.ProfileSubtypeId)) {

				Logger.MsgDebug("Trigger Already Added: " + trigger.ProfileSubtypeId, DebugTypeEnum.BehaviorSetup);
				return false;

			}


			ExistingTriggers.Add(trigger.ProfileSubtypeId);

			trigger.InitRandomTimes();
			if (trigger.Type == "Damage") {

				DamageTriggers.Add(trigger);
				return true;

			}

			if (trigger.Type == "CommandReceived") {

				CommandTriggers.Add(trigger);
				RegisterCommandListener();
				return true;

			}

			if (trigger.Type == "Compromised") {

				CompromisedTriggers.Add(trigger);
				return true;

			}

			Triggers.Add(trigger);
			return true;

		}

		public void InitTags() {

			//TODO: Try To Get Triggers From Block Storage At Start

			//Start With This Class
			if (string.IsNullOrWhiteSpace(RemoteControl.CustomData) == true) {

				return;

			}

			var descSplit = RemoteControl.CustomData.Split('\n');

			foreach (var tag in descSplit) {

				//Triggers
				if (tag.Contains("[Triggers:") == true) {

					bool gotTrigger = false;
					var tempValue = TagHelper.TagStringCheck(tag);

					if (string.IsNullOrWhiteSpace(tempValue) == false) {

						byte[] byteData = { };

						if (TagHelper.TriggerObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

							try {

								var profile = MyAPIGateway.Utilities.SerializeFromBinary<TriggerProfile>(byteData);

								if (profile != null) {

									gotTrigger = AddTrigger(profile);

								}

							} catch (Exception e) {

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

									gotTrigger = true;
									foreach (var trigger in profile.Triggers) {

										AddTrigger(trigger);

									}

								}

							} catch (Exception) {



							}

						}

					}

					if (!gotTrigger)
						Logger.WriteLog("Could Not Find Trigger Group Profile Associated To Tag: " + tag);

				}

			}

		}

	}

}
