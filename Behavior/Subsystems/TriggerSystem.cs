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

		private AutoPilotSystem _autopilot;
		private BroadcastSystem _broadcast;
		private DespawnSystem _despawn;
		private ExtrasSystem _extras;
		private OwnerSystem _owner;
		private StoredSettings _settings;
		private TargetingSystem _targeting;
		private WeaponsSystem _weapons;

		public List<TriggerProfile> Triggers;
		public List<TriggerProfile> DamageTriggers;
		public List<TriggerProfile> CommandTriggers;

		public bool CommandListenerRegistered;
		public bool DamageHandlerRegistered;
		public MyDamageInformation DamageInfo;
		public bool PendingDamage;


		public TriggerSystem(IMyRemoteControl remoteControl){

			RemoteControl = null;
			AntennaList = new List<IMyRadioAntenna>();
			TurretList = new List<IMyLargeTurretBase>();

			Triggers = new List<TriggerProfile>();
			DamageTriggers = new List<TriggerProfile>();
			CommandTriggers = new List<TriggerProfile>();

			CommandListenerRegistered = false;

			Setup(remoteControl);

		}

		public void ProcessTriggerWatchers() {

			MyAPIGateway.Parallel.Start(() => {

				for(int i = 0;i < this.Triggers.Count;i++) {

					var trigger = this.Triggers[i];

					//Timer
					if (trigger.Type == "Timer") {

						if (trigger.UseTrigger == true) {

							trigger.ActivateTrigger();

						}

						continue;

					}

					//PlayerNear
					if (trigger.Type == "PlayerNear") {

						if(trigger.UseTrigger == true) {

							if(IsPlayerNearby(trigger)) {

								trigger.ActivateTrigger();

							}

						}

						continue;

					}

					//TurretTarget
					if(trigger.Type == "TurretTarget") {

						if(trigger.UseTrigger == true) {

							_weapons.TurretTarget = null;

							foreach(var turret in _weapons.Turrets) {

								if(turret == null) {

									continue;

								}

								if(turret.Target != null && turret.IsShooting) {

									_weapons.TurretTarget = turret.Target;
									break;

								}

							}

							if(_weapons.TurretTarget != null) {

								trigger.ActivateTrigger();

								if(trigger.Triggered == true) {

									trigger.DetectedEntityId = _weapons.TurretTarget.EntityId;

								}

							}

						}

						continue;

					}

					//NoWeapon
					if(trigger.Type == "NoWeapon") {

						if(trigger.UseTrigger == true && _weapons.AllWeaponCollectionDone == true) {

							bool validWeapon = false;

							if (this.RemoteControl?.SlimBlock?.CubeGrid == null)
								continue;

							foreach(var weapon in _weapons.AllWeapons) {

								if(weapon == null) {

									continue;

								}

								if(weapon.IsFunctional == false || weapon.IsWorking == false) {

									continue;

								}

								if (!this.RemoteControl.SlimBlock.CubeGrid.IsSameConstructAs(weapon.SlimBlock.CubeGrid))
									continue;

								if(!_weapons.KeepWeaponsLoaded && weapon.GetInventory().Empty() == true) {

									continue;

								}

								validWeapon = true;
								break;

							}

							if(validWeapon == false) {

								trigger.ActivateTrigger();

							}

						}

						continue;

					}

					//TargetInSafezone
					if(trigger.Type == "TargetInSafezone") {

						if(trigger.UseTrigger == true) {

							if(_targeting.Target.TargetExists == true && _targeting.Target.InSafeZone == true) {

								trigger.ActivateTrigger();

							}

						}

						continue;

					}

					//Grounded
					if(trigger.Type == "Grounded") {

						if(trigger.UseTrigger == true) {

							//Check if Grounded
							trigger.ActivateTrigger();

						}

						continue;

					}
				
				}

			}, () => {

				MyAPIGateway.Utilities.InvokeOnGameThread(() => {

					for(int i = 0;i < this.Triggers.Count;i++) {

						ProcessTrigger(this.Triggers[i]);

					}

				});

			});


		}

		public void ProcessDamageTriggerWatchers(object target, MyDamageInformation info) {

			//Logger.AddMsg("Damage Trigger Count: " + this.DamageTriggers.Count.ToString(), true);

			for (int i = 0;i < this.DamageTriggers.Count;i++) {

				//Logger.AddMsg("Got Trigger Profile", true);

				var trigger = this.DamageTriggers[i];

				if(trigger.DamageTypes.Contains(info.Type.ToString()) || trigger.DamageTypes.Contains("Any")) {

					if(trigger.UseTrigger == true) {

						trigger.ActivateTrigger();

						if(trigger.Triggered == true) {

							//Logger.AddMsg("Process Damage Actions", true);
							ProcessTrigger(trigger, info.AttackerId);

						}

					}

				}

			}

		}

		public void ProcessCommandReceiveTriggerWatcher(string commandCode, IMyRemoteControl senderRemote, double radius, long entityId) {

			if(senderRemote?.SlimBlock?.CubeGrid == null || this.RemoteControl?.SlimBlock?.CubeGrid == null)
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

						ProcessTrigger(trigger, entityId);

					}

				}
 
			}

		}

		public void ProcessTrigger(TriggerProfile trigger, long attackerEntityId = 0) {

			if (this.RemoteControl?.SlimBlock?.CubeGrid == null)
				return;

			if(trigger.Triggered == false || trigger.Actions == null) {

				return;

			}

			long detectedEntity = attackerEntityId;

			if (trigger.DetectedEntityId != 0 && detectedEntity != 0) {

				detectedEntity = trigger.DetectedEntityId;

			}

			trigger.DetectedEntityId = 0;
			trigger.Triggered = false;
			trigger.CooldownTime = trigger.Rnd.Next((int)trigger.MinCooldownMs, (int)trigger.MaxCooldownMs);
			trigger.LastTriggerTime = MyAPIGateway.Session.GameDateTime;
			trigger.TriggerCount++;

			//ChatBroadcast
			if(trigger.Actions.UseChatBroadcast == true) {

				//Logger.AddMsg("Chat Broadcast", true);
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

				_autopilot.DesiredMaxSpeed = trigger.Actions.NewAutopilotSpeed;
				var blockList = TargetHelper.GetAllBlocks(RemoteControl.SlimBlock.CubeGrid);

				foreach (var block in blockList.Where(x => x.FatBlock != null)) {

					var tBlock = block.FatBlock as IMyRemoteControl;

					if (tBlock != null) {

						tBlock.SpeedLimit = trigger.Actions.NewAutopilotSpeed;

					}

				}

			}

			//SpawnReinforcements
			if(trigger.Actions.SpawnEncounter == true && trigger.Actions.Spawner.UseSpawn) {

				if (trigger.Actions.Spawner.IsReadyToSpawn()) {

					//Logger.AddMsg("Do Spawn", true);
					trigger.Actions.Spawner.CurrentPositionMatrix = this.RemoteControl.WorldMatrix;
					SpawnHelper.SpawnRequest(trigger.Actions.Spawner);

				}

			}

			//SelfDestruct
			if(trigger.Actions.SelfDestruct == true) {

				var blockList = TargetHelper.GetAllBlocks(RemoteControl.SlimBlock.CubeGrid);
				int totalWarheads = 0;
				
				foreach(var block in blockList.Where(x => x.FatBlock != null)) {

					var tBlock = block.FatBlock as IMyWarhead;

					if(tBlock != null) {

						tBlock.IsArmed = true;
						tBlock.DetonationTime = 0;
						tBlock.Detonate();
						totalWarheads++;

					}

				}

				//Logger.AddMsg("TotalBlocks:  " + blockList.Count.ToString(), true);
				//Logger.AddMsg("TotalWarheads: " + totalWarheads.ToString(), true);

				//TODO: Shield EMP

			}

			//Retreat
			if(trigger.Actions.Retreat == true) {

				_despawn.Retreat();

			}

			//BroadcastCurrentTarget
			if (trigger.Actions.BroadcastCurrentTarget == true && detectedEntity != 0) {

				var antenna = BlockHelper.GetAntennaWithHighestRange(this.AntennaList);
				
				if(antenna != null)
					CommandHelper.CommandTrigger?.Invoke(trigger.Actions.BroadcastSendCode, this.RemoteControl, (double)antenna.Radius, detectedEntity);

			}

			//SwitchToReceivedTarget
			if (trigger.Actions.SwitchToReceivedTarget == true && detectedEntity != 0) {

				_targeting.UpdateSpecificTarget = detectedEntity;

			}

			//SwitchToBehavior
			if(trigger.Actions.SwitchToBehavior == true) {

				//TODO:

			}

			//RefreshTarget
			if(trigger.Actions.RefreshTarget == true) {

				_targeting.UpdateTargetRequested = true;

			}

			//ChangeReputationWithPlayers
			if(trigger.Actions.ChangeReputationWithPlayers == true) {

				OwnershipHelper.ChangeReputationWithPlayersInRadius(this.RemoteControl, trigger.Actions.ReputationChangeRadius, trigger.Actions.ReputationChangeAmount, trigger.Actions.ReputationChangeFactions, trigger.Actions.ReputationChangesForAllRadiusPlayerFactionMembers);

			}

			//ChangeAttackerReputation
			if (trigger.Actions.ChangeAttackerReputation == true && detectedEntity != 0) {

				OwnershipHelper.ChangeDamageOwnerReputation(trigger.Actions.ChangeAttackerReputationFaction, detectedEntity, trigger.Actions.ChangeAttackerReputationAmount, trigger.Actions.ReputationChangesForAllAttackPlayerFactionMembers);

			}


			//TriggerTimerBlock
			if (trigger.Actions.TriggerTimerBlocks == true) {

				var blockList = BlockHelper.GetBlocksWithNames(RemoteControl.SlimBlock.CubeGrid, trigger.Actions.TimerBlockNames);

				foreach(var block in blockList) {

					var tBlock = block as IMyTimerBlock;

					if(tBlock != null) {

						tBlock.Trigger();

					}

				}

			}

			//ActivateAssertiveAntennas
			if(trigger.Actions.ActivateAssertiveAntennas == true) {

				/*TODO:
				_extras.SetAssertiveAntennas(true);
				_extras.AssertiveEngage = true;
				*/
			}

			//ChangeAntennaOwnership
			if (trigger.Actions.ChangeAntennaOwnership == true) {

				OwnershipHelper.ChangeAntennaBlockOwnership(AntennaList, trigger.Actions.AntennaFactionOwner);

			}

			//CreateKnownPlayerArea
			if (trigger.Actions.CreateKnownPlayerArea == true) {

				MESApi.AddKnownPlayerLocation(this.RemoteControl.GetPosition(), _owner.Faction?.Tag, trigger.Actions.KnownPlayerAreaRadius, trigger.Actions.KnownPlayerAreaTimer, trigger.Actions.KnownPlayerAreaMaxSpawns);

			}

			//DamageAttacker
			if (trigger.Actions.DamageToolAttacker == true && detectedEntity != 0) {

				DamageHelper.ApplyDamageToTarget(attackerEntityId, trigger.Actions.DamageToolAttackerAmount, trigger.Actions.DamageToolAttackerParticle, trigger.Actions.DamageToolAttackerSound);

			}

			//PlayParticleEffectAtRemote
			if (trigger.Actions.PlayParticleEffectAtRemote == true) {

				EffectManager.SendParticleEffectRequest(trigger.Actions.ParticleEffectId, this.RemoteControl.WorldMatrix, trigger.Actions.ParticleEffectOffset, trigger.Actions.ParticleEffectScale, trigger.Actions.ParticleEffectMaxTime, trigger.Actions.ParticleEffectColor);
					
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

		}

		public bool IsPlayerNearby(TriggerProfile control) {

			IMyPlayer player = null;

			if(control.MinPlayerReputation != -1501 || control.MaxPlayerReputation != 1501) {

				player = TargetHelper.GetClosestPlayerWithReputation(this.RemoteControl.GetPosition(), _owner.FactionId, control.MinPlayerReputation, control.MaxPlayerReputation);

			} else {

				player = TargetHelper.GetClosestPlayer(this.RemoteControl.GetPosition());

			}

			if(player == null) {

				return false;

			}

			var playerDist = Vector3D.Distance(player.GetPosition(), this.RemoteControl.GetPosition());

			if(playerDist > control.TargetDistance) {

				return false;

			}

			if(control.InsideAntenna == true) {

				var antenna = BlockHelper.GetAntennaWithHighestRange(this.AntennaList);

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

		public void SetupReferences(AutoPilotSystem autopilot, BroadcastSystem broadcast, DespawnSystem despawn, ExtrasSystem extras, OwnerSystem owners, StoredSettings settings, TargetingSystem targeting, WeaponsSystem weapons) {

			this._autopilot = autopilot;
			this._broadcast = broadcast;
			this._despawn = despawn;
			this._extras = extras;
			this._owner = owners;
			this._settings = settings;
			this._targeting = targeting;
			this._weapons = weapons;

		}
		
		public void RegisterCommandListener(){
		
			if(this.CommandListenerRegistered)
				return;
			
			this.CommandListenerRegistered = true;
			CommandHelper.CommandTrigger += ProcessCommandReceiveTriggerWatcher;

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

					var tempValue = TagHelper.TagStringCheck(tag);

					if(string.IsNullOrWhiteSpace(tempValue) == false) {

						byte[] byteData = { };

						if(TagHelper.TriggerObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

							try {

								var profile = MyAPIGateway.Utilities.SerializeFromBinary<TriggerProfile>(byteData);

								if(profile != null) {

									if(profile.Type == "Damage") {

										this.DamageTriggers.Add(profile);
										continue;

									}
									
									if(profile.Type == "CommandReceived"){

										this.CommandTriggers.Add(profile);
										RegisterCommandListener();
										continue;

									}

									this.Triggers.Add(profile);
									
								}

							} catch(Exception) {



							}

						}

					}

				}

			}

		}

	}

}
