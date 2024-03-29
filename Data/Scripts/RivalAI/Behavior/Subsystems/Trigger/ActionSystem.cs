﻿using System;
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
using RivalAI.Helpers;
using RivalAI.Sync;
using RivalAI.Behavior.Subsystems.AutoPilot;
using RivalAI.Behavior.Subsystems.Trigger;

namespace RivalAI.Behavior.Subsystems.Trigger {
	public partial class TriggerSystem {

		public void ProcessAction(TriggerProfile trigger, ActionProfile actions, long attackerEntityId = 0, long detectedEntity = 0, Command command = null) {

			Logger.MsgDebug(trigger.ProfileSubtypeId + " Attempting To Execute Action Profile " + actions.ProfileSubtypeId, DebugTypeEnum.Action);

			if (!string.IsNullOrWhiteSpace(actions.ParentGridNameRequirement) && !string.IsNullOrWhiteSpace(_behavior?.RemoteControl?.SlimBlock?.CubeGrid?.CustomName)) {

				if (_behavior.RemoteControl.SlimBlock.CubeGrid.CustomName != actions.ParentGridNameRequirement)
					return;

			}

			if (actions.Chance < 100) {

				var roll = Utilities.Rnd.Next(0, 101);

				if (roll > actions.Chance) {

					Logger.MsgDebug(actions.ProfileSubtypeId + ": Did Not Pass Chance Check", DebugTypeEnum.Action);
					return;

				}


			}

			Logger.MsgDebug(actions.ProfileSubtypeId + ": Performing Eligible Actions", DebugTypeEnum.Action);

			//ChatBroadcast
			if (actions.UseChatBroadcast == true) {

				foreach (var chatData in actions.ChatData) {

					Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Chat Broadcast", DebugTypeEnum.Action);
					_broadcast.BroadcastRequest(chatData);

				}

			}

			//BarrellRoll
			if (actions.BarrelRoll == true) {

				_behavior.AutoPilot.ActivateBarrelRoll();

			}

			//HeavyYaw
			if (actions.HeavyYaw == true) {

				_behavior.AutoPilot.ActivateHeavyYaw();

			}

			//Ramming
			if (actions.Ramming == true) {

				_behavior.AutoPilot.ActivateRamming();

			}

			//Strafe - Implement Post Release
			if (actions.Strafe == true) {

				//_autopilot.ChangeAutoPilotMode(AutoPilotMode.Strafe);

			}

			//ChangeAutopilotSpeed
			if (actions.ChangeAutopilotSpeed == true) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Changing AutoPilot Speed To: " + actions.NewAutopilotSpeed.ToString(), DebugTypeEnum.Action);
				_autopilot.State.MaxSpeedOverride = actions.NewAutopilotSpeed;
				var blockList = TargetHelper.GetAllBlocks(RemoteControl.SlimBlock.CubeGrid);

				foreach (var block in blockList.Where(x => x.FatBlock != null)) {

					var tBlock = block.FatBlock as IMyRemoteControl;

					if (tBlock != null) {

						tBlock.SpeedLimit = actions.NewAutopilotSpeed;

					}

				}

			}

			//SpawnReinforcements
			if (actions.SpawnEncounter == true) {

				foreach (var spawner in actions.Spawner) {

					if (spawner.UseSpawn) {

						if (!string.IsNullOrWhiteSpace(spawner.ParentGridNameRequirement) && !string.IsNullOrWhiteSpace(_behavior?.RemoteControl?.SlimBlock?.CubeGrid?.CustomName)) {

							if (_behavior.RemoteControl.SlimBlock.CubeGrid.CustomName != spawner.ParentGridNameRequirement)
								continue;
						
						}

						Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Spawn", DebugTypeEnum.Spawn);
						if (spawner.IsReadyToSpawn()) {

							//Logger.AddMsg("Do Spawn", true);
							spawner.AssignInitialMatrix(RemoteControl.WorldMatrix);
							spawner.CurrentFactionTag = spawner.ForceSameFactionOwnership && !string.IsNullOrWhiteSpace(_owner.Faction?.Tag) ? _owner.Faction.Tag : "";
							SpawnHelper.SpawnRequest(spawner);

						}

					}

				}

			} else {



			}

			//SelfDestruct
			if (actions.SelfDestruct == true) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting SelfDestruct", DebugTypeEnum.Action);
				var blockList = TargetHelper.GetAllBlocks(RemoteControl.SlimBlock.CubeGrid);
				int totalWarheads = 0;

				foreach (var block in blockList.Where(x => x.FatBlock != null)) {

					var tBlock = block.FatBlock as IMyWarhead;

					if (tBlock != null) {

						if (!actions.StaggerWarheadDetonation) {

							tBlock.IsArmed = true;
							tBlock.DetonationTime = 0 + actions.SelfDestructTimerPadding;
							tBlock.Detonate();
							totalWarheads++;

						} else {

							totalWarheads++;
							tBlock.IsArmed = true;
							tBlock.DetonationTime = (totalWarheads * actions.SelfDestructTimeBetweenBlasts) + actions.SelfDestructTimerPadding;
							tBlock.StartCountdown();

						}

					}

				}

				//Logger.AddMsg("TotalBlocks:  " + blockList.Count.ToString(), true);
				//Logger.AddMsg("TotalWarheads: " + totalWarheads.ToString(), true);

				//TODO: Shield EMP

			}

			//Retreat
			if (actions.Retreat) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Retreat", DebugTypeEnum.Action);
				_despawn.Retreat();

			}

			//RecalculateDespawnCoords
			if (actions.RecalculateDespawnCoords) {

				_behavior.AutoPilot.State.CargoShipDespawn = new EncounterWaypoint(_behavior.AutoPilot.CalculateDespawnCoords(this.RemoteControl.GetPosition()));

			}

			//ForceDespawn
			if (actions.ForceDespawn) {

				_despawn.DespawnGrid();

			}

			//TerminateBehavior
			if (actions.TerminateBehavior) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Termination Of Behavior", DebugTypeEnum.Action);
				_autopilot.ActivateAutoPilot(Vector3D.Zero, NewAutoPilotMode.None);
				_behavior.BehaviorTerminated = true;

			}

			//BroadcastCommandProfiles
			if (actions.BroadcastCommandProfiles) {

				foreach (var commandId in actions.CommandProfileIds) {

					CommandProfile commandProfile = null;

					if (!TagHelper.CommandProfiles.TryGetValue(commandId, out commandProfile)) {

						Logger.MsgDebug(commandId + ": Command Profile Not Found", DebugTypeEnum.Action);
						continue;

					}

					var newCommand = new Command(commandProfile);
					newCommand.RemoteControl = RemoteControl;
					newCommand.CommandOwnerId = RemoteControl.OwnerId;

					double sendRadius = 0;

					if (commandProfile.IgnoreAntennaRequirement) {

						sendRadius = commandProfile.Radius;
						newCommand.IgnoreAntennaRequirement = true;

					} else {

						var antenna = _behavior.Grid.GetAntennaWithHighestRange();

						if (antenna != null)
							sendRadius = antenna.Radius;

					}

					if (commandProfile.MaxRadius > -1 && sendRadius > commandProfile.MaxRadius)
						sendRadius = commandProfile.MaxRadius;

					newCommand.Radius = sendRadius;

					if (commandProfile.SendTargetEntityId)

						if(_behavior.AutoPilot.Targeting.HasTarget())
							newCommand.TargetEntityId = _behavior.AutoPilot.Targeting.Target.GetEntityId();
						else
							Logger.MsgDebug("No Current Target To Send With Command", DebugTypeEnum.Command);

					if (commandProfile.SendDamagerEntityId)

						if(_behavior.Settings.LastDamagerEntity == 0)
							newCommand.DamagerEntityId = _behavior.Settings.LastDamagerEntity;
						else
							Logger.MsgDebug("No Damager ID To Send With Command", DebugTypeEnum.Command);

					if (commandProfile.SendWaypoint) {

						WaypointProfile waypointProfile = null;

						if (TagHelper.WaypointProfiles.TryGetValue(commandProfile.Waypoint, out waypointProfile)) {

							if ((int)waypointProfile.Waypoint > 2) {

								Logger.MsgDebug(actions.ProfileSubtypeId + ": Creating an Entity/Relative Waypoint", DebugTypeEnum.Command);

								if (waypointProfile.RelativeEntity == RelativeEntityType.Self)
									newCommand.Waypoint = waypointProfile.GenerateEncounterWaypoint(RemoteControl);

								if (waypointProfile.RelativeEntity == RelativeEntityType.Target && _behavior.AutoPilot.Targeting.HasTarget())
									newCommand.Waypoint = waypointProfile.GenerateEncounterWaypoint(_behavior.AutoPilot.Targeting.Target.GetEntity());
								else
									Logger.MsgDebug("No Current Target To Send As Target Relative Waypoint", DebugTypeEnum.Command);

								if (waypointProfile.RelativeEntity == RelativeEntityType.Damager) {

									IMyEntity entity = null;

									if (MyAPIGateway.Entities.TryGetEntityById(_behavior.Settings.LastDamagerEntity, out entity)) {

										var parentEnt = entity.GetTopMostParent();

										if (parentEnt != null) {

											//Logger.MsgDebug("Damager Parent Entity Valid", DebugTypeEnum.General);
											var gridGroup = MyAPIGateway.GridGroups.GetGroup(RemoteControl.SlimBlock.CubeGrid, GridLinkTypeEnum.Physical);
											bool isSameGridConstrust = false;

											foreach (var grid in gridGroup) {

												if (grid.EntityId == parentEnt.EntityId) {

													//Logger.MsgDebug("Damager Parent Entity Was Same Grid", DebugTypeEnum.General);
													isSameGridConstrust = true;
													break;

												}

											}

											if (!isSameGridConstrust) {

												newCommand.Waypoint = waypointProfile.GenerateEncounterWaypoint(parentEnt);

											}

										}

									}
								
								}
			
							} else {

								newCommand.Waypoint = waypointProfile.GenerateEncounterWaypoint(RemoteControl);

							}
								
							
						}
					
					}

					Logger.MsgDebug(actions.ProfileSubtypeId + ": Sending Command: " + newCommand.CommandCode, DebugTypeEnum.Action);
					CommandHelper.CommandTrigger?.Invoke(newCommand);

				}
			
			}

			//BroadcastGenericCommand
			if (actions.BroadcastGenericCommand == true) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Broadcast of Generic Command", DebugTypeEnum.Action);

				double sendRadius = 0;

				if (actions.SendCommandWithoutAntenna) {

					sendRadius = actions.SendCommandWithoutAntennaRadius;

				} else {

					var antenna = _behavior.Grid.GetAntennaWithHighestRange();

					if (antenna != null)
						sendRadius = antenna.Radius;

				}

				if (sendRadius != 0) {

					var newCommand = new Command();
					newCommand.CommandCode = actions.BroadcastSendCode;
					newCommand.RemoteControl = RemoteControl;
					newCommand.Radius = sendRadius;
					CommandHelper.CommandTrigger?.Invoke(newCommand);

				}

			}

			//BroadcastDamagerTarget
			if (actions.BroadcastDamagerTarget == true && detectedEntity != 0) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Broadcast of Damager", DebugTypeEnum.Action);

				double sendRadius = 0;

				if (actions.SendCommandWithoutAntenna) {

					sendRadius = actions.SendCommandWithoutAntennaRadius;

				} else {

					var antenna = _behavior.Grid.GetAntennaWithHighestRange();

					if (antenna != null)
						sendRadius = antenna.Radius;

				}

				if (sendRadius != 0) {

					var newCommand = new Command();
					newCommand.CommandCode = actions.BroadcastSendCode;
					newCommand.RemoteControl = RemoteControl;
					newCommand.Radius = sendRadius;
					newCommand.TargetEntityId = detectedEntity;
					CommandHelper.CommandTrigger?.Invoke(newCommand);

				}

			}

			//InheritLastAttackerFromCommand
			if (actions.InheritLastAttackerFromCommand) {

				_behavior.Settings.LastDamagerEntity = command != null ? command.TargetEntityId : 0;

			}

			//SwitchToReceivedTarget
			if (actions.SwitchToReceivedTarget == true && (command != null || detectedEntity != 0)) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Switch to Received Target Data", DebugTypeEnum.Action);
				long switchToId = 0;

				if (command != null && command.TargetEntityId != 0) {

					switchToId = command.TargetEntityId;


				} else if (detectedEntity != 0) {

					switchToId = detectedEntity;

				}

				IMyEntity tempEntity = null;

				if (MyAPIGateway.Entities.TryGetEntityById(switchToId, out tempEntity)) {

					//Logger.MsgDebug("Damager Entity Valid", DebugTypeEnum.General);

					var parentEnt = tempEntity.GetTopMostParent();

					if (parentEnt != null) {

						//Logger.MsgDebug("Damager Parent Entity Valid", DebugTypeEnum.General);
						var gridGroup = MyAPIGateway.GridGroups.GetGroup(RemoteControl.SlimBlock.CubeGrid, GridLinkTypeEnum.Physical);
						bool isSameGridConstrust = false;

						foreach (var grid in gridGroup) {

							if (grid.EntityId == tempEntity.GetTopMostParent().EntityId) {

								//Logger.MsgDebug("Damager Parent Entity Was Same Grid", DebugTypeEnum.General);
								isSameGridConstrust = true;
								break;

							}

						}

						if (!isSameGridConstrust) {

							//Logger.MsgDebug("Damager Parent Entity Was External", DebugTypeEnum.General);
							_behavior.AutoPilot.Targeting.ForceTargetEntityId = parentEnt.EntityId;
							_behavior.AutoPilot.Targeting.ForceTargetEntity = parentEnt;
							_behavior.AutoPilot.Targeting.ForceRefresh = true;

						}

					}

				}

			}

			//SwitchTargetToDamager
			if (actions.SwitchTargetToDamager == true && _behavior.Settings.LastDamagerEntity != 0) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Switch Target to Damager", DebugTypeEnum.Action);

				IMyEntity tempEntity = null;

				if (MyAPIGateway.Entities.TryGetEntityById(_behavior.Settings.LastDamagerEntity, out tempEntity)) {

					//Logger.MsgDebug("Damager Entity Valid", DebugTypeEnum.General);

					var parentEnt = tempEntity.GetTopMostParent();

					if (parentEnt != null) {

						//Logger.MsgDebug("Damager Parent Entity Valid", DebugTypeEnum.General);
						var gridGroup = MyAPIGateway.GridGroups.GetGroup(RemoteControl.SlimBlock.CubeGrid, GridLinkTypeEnum.Physical);
						bool isSameGridConstrust = false;

						foreach (var grid in gridGroup) {

							if (grid.EntityId == tempEntity.GetTopMostParent().EntityId) {

								//Logger.MsgDebug("Damager Parent Entity Was Same Grid", DebugTypeEnum.General);
								isSameGridConstrust = true;
								break;

							}

						}

						if (!isSameGridConstrust) {

							//Logger.MsgDebug("Damager Parent Entity Was External", DebugTypeEnum.General);
							_behavior.AutoPilot.Targeting.ForceTargetEntityId = parentEnt.EntityId;
							_behavior.AutoPilot.Targeting.ForceTargetEntity = parentEnt;
							_behavior.AutoPilot.Targeting.ForceRefresh = true;

						}

					}

				}

			}

			//AddWaypointFromCommand
			if (actions.AddWaypointFromCommand && command?.Waypoint != null) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Adding Received Waypoint From Command", DebugTypeEnum.Action);
				_behavior.AutoPilot.State.CargoShipWaypoints.Add(command.Waypoint);
			
			}

			//CancelWaitingAtWaypoint
			if (actions.CancelWaitingAtWaypoint) {

				_behavior.AutoPilot.State.WaypointWaitTime = DateTime.MinValue;

			}

			//SwitchToNextWaypoint
			if (actions.SwitchToNextWaypoint && _behavior.AutoPilot.State.CargoShipWaypoints.Count > 0) {

				for (int i = 0; i < _behavior.AutoPilot.State.CargoShipWaypoints.Count; i++) {

					_behavior.AutoPilot.State.CargoShipWaypoints[0].Valid = false;

				}

			}

			//SwitchToBehavior
			if (actions.SwitchToBehavior == true) {

				_behavior.ChangeBehavior(actions.NewBehavior, actions.PreserveSettingsOnBehaviorSwitch, actions.PreserveTriggersOnBehaviorSwitch, actions.PreserveTargetDataOnBehaviorSwitch);

			}

			//ChangePlayerCredits
			if (actions.ChangePlayerCredits && command != null && command.Type == CommandType.PlayerChat) {

				if (command.PlayerIdentity != 0) {

					var playerList = new List<IMyPlayer>();
					MyAPIGateway.Players.GetPlayers(playerList, p => p.IdentityId == command.PlayerIdentity);

					foreach (var player in playerList) {

						long credits = 0;
						player.TryGetBalanceInfo(out credits);

						if (actions.ChangePlayerCreditsAmount > 0) {

							player.RequestChangeBalance(actions.ChangePlayerCreditsAmount);
							PaymentSuccessTriggered = true;
						
						} else {

							if (actions.ChangePlayerCreditsAmount > credits) {

								PaymentFailureTriggered = true;
							
							} else {

								player.RequestChangeBalance(actions.ChangePlayerCreditsAmount);
								PaymentSuccessTriggered = true;

							}
						
						}
					
					}

				}
			
			}

			//ChangeNpcFactionCredits
			if (actions.ChangeNpcFactionCredits) {

				IMyFaction faction = null;

				if (string.IsNullOrWhiteSpace(actions.ChangeNpcFactionCreditsTag)) {

					faction = _behavior.Owner.Faction;
				
				} else {

					faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(actions.ChangeNpcFactionCreditsTag);

				}

				if (faction != null) {

					long credits = 0;
					faction.TryGetBalanceInfo(out credits);

					if (actions.ChangePlayerCreditsAmount > 0) {

						faction.RequestChangeBalance(actions.ChangePlayerCreditsAmount);
						PaymentSuccessTriggered = true;

					} else {

						if (actions.ChangePlayerCreditsAmount > credits) {

							PaymentFailureTriggered = true;

						} else {

							faction.RequestChangeBalance(actions.ChangePlayerCreditsAmount);
							PaymentSuccessTriggered = true;

						}

					}

				}

			}

			//RefreshTarget
			if (actions.RefreshTarget == true) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Target Refresh", DebugTypeEnum.Action);
				_autopilot.Targeting.ForceRefresh = true;

			}

			//ChangeTargetProfile
			if (actions.ChangeTargetProfile == true) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Target Profile Change", DebugTypeEnum.Action);
				_autopilot.Targeting.UseNewTargetProfile = true;
				_autopilot.Targeting.NewTargetProfileName = actions.NewTargetProfileId;

			}

			//ChangeReputationWithPlayers
			if (actions.ChangeReputationWithPlayers == true) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Reputation Change With Players In Radius", DebugTypeEnum.Action);
				OwnershipHelper.ChangeReputationWithPlayersInRadius(RemoteControl, actions.ReputationChangeRadius, actions.ReputationChangeAmount, actions.ReputationChangeFactions, actions.ReputationChangesForAllRadiusPlayerFactionMembers);

			}

			//ChangeAttackerReputation
			if (actions.ChangeAttackerReputation == true && detectedEntity != 0) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Reputation Change for Attacker", DebugTypeEnum.Action);
				OwnershipHelper.ChangeDamageOwnerReputation(actions.ChangeAttackerReputationFaction, detectedEntity, actions.ChangeAttackerReputationAmount, actions.ReputationChangesForAllAttackPlayerFactionMembers);

			}


			//TriggerTimerBlock
			if (actions.TriggerTimerBlocks == true) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Trigger of Timer Blocks", DebugTypeEnum.Action);
				var blockList = BlockHelper.GetBlocksWithNames(RemoteControl.SlimBlock.CubeGrid, actions.TimerBlockNames);

				foreach (var block in blockList) {

					var tBlock = block as IMyTimerBlock;

					if (tBlock != null) {

						tBlock.Trigger();

					}

				}

			}

			//ChangeBlockNames
			if (actions.ChangeBlockNames) {

				_behavior.Grid.RenameBlocks(actions.ChangeBlockNamesFrom, actions.ChangeBlockNamesTo, actions.ProfileSubtypeId);

			}

			//ChangeBlockNames
			if (actions.ToggleBlocksOfType) {

				_behavior.Grid.ToggleBlocksOfType(actions.BlockTypesToToggle, actions.BlockTypeToggles);

			}

			//ChangeAntennaRanges
			if (actions.ChangeAntennaRanges) {

				_behavior.Grid.SetGridAntennaRanges(actions.AntennaNamesForRangeChange, actions.AntennaRangeChangeType, actions.AntennaRangeChangeAmount);

			}

			//ChangeAntennaOwnership
			if (actions.ChangeAntennaOwnership == true) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Antenna Ownership Change", DebugTypeEnum.Action);
				OwnershipHelper.ChangeAntennaBlockOwnership(AntennaList, actions.AntennaFactionOwner);

			}

			//CreateKnownPlayerArea
			if (actions.CreateKnownPlayerArea == true) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Creation of Known Player Area in MES", DebugTypeEnum.Action);
				MESApi.AddKnownPlayerLocation(RemoteControl.GetPosition(), _owner.Faction?.Tag, actions.KnownPlayerAreaRadius, actions.KnownPlayerAreaTimer, actions.KnownPlayerAreaMaxSpawns, actions.KnownPlayerAreaMinThreatForAvoidingAbandonment);

			}

			//RemoveKnownPlayerLocation
			if (actions.RemoveKnownPlayerArea == true) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Removal of Known Player Area in MES", DebugTypeEnum.Action);
				MESApi.RemoveKnownPlayerLocation(RemoteControl.GetPosition(), _owner.Faction?.Tag, actions.RemoveAllKnownPlayerAreas);

			}

			//DamageAttacker
			if (actions.DamageToolAttacker == true && detectedEntity != 0) {

				Logger.MsgDebug(actions.ProfileSubtypeId + ": Attempting Damage to Tool User", DebugTypeEnum.Action);
				DamageHelper.ApplyDamageToTarget(attackerEntityId, actions.DamageToolAttackerAmount, actions.DamageToolAttackerParticle, actions.DamageToolAttackerSound);

			}

			//PlayParticleEffectAtRemote
			if (actions.PlayParticleEffectAtRemote == true) {

				EffectManager.SendParticleEffectRequest(actions.ParticleEffectId, RemoteControl.WorldMatrix, actions.ParticleEffectOffset, actions.ParticleEffectScale, actions.ParticleEffectMaxTime, actions.ParticleEffectColor);

			}

			//ResetCooldownTimeOfTriggers
			if (actions.ResetCooldownTimeOfTriggers) {

				foreach (var resetTrigger in Triggers) {

					if (actions.ResetTriggerCooldownNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.LastTriggerTime = MyAPIGateway.Session.GameDateTime;

				}

				foreach (var resetTrigger in DamageTriggers) {

					if (actions.ResetTriggerCooldownNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.LastTriggerTime = MyAPIGateway.Session.GameDateTime;

				}

				foreach (var resetTrigger in CommandTriggers) {

					if (actions.ResetTriggerCooldownNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.LastTriggerTime = MyAPIGateway.Session.GameDateTime;

				}

			}

			//EnableTriggers
			if (actions.EnableTriggers) {

				Logger.MsgDebug(actions.ProfileSubtypeId + " Attempting To Enable Triggers.", DebugTypeEnum.Action);

				foreach (var resetTrigger in Triggers) {

					if (actions.EnableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = true;

				}

				foreach (var resetTrigger in DamageTriggers) {

					if (actions.EnableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = true;

				}

				foreach (var resetTrigger in CommandTriggers) {

					if (actions.EnableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = true;

				}

			}

			//DisableTriggers
			if (actions.DisableTriggers) {

				Logger.MsgDebug(actions.ProfileSubtypeId + " Attempting To Disable Triggers.", DebugTypeEnum.Action);

				foreach (var resetTrigger in Triggers) {

					if (actions.DisableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = false;

				}

				foreach (var resetTrigger in DamageTriggers) {

					if (actions.DisableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = false;

				}

				foreach (var resetTrigger in CommandTriggers) {

					if (actions.DisableTriggerNames.Contains(resetTrigger.ProfileSubtypeId))
						resetTrigger.UseTrigger = false;

				}

			}

			//ManuallyActivateTrigger
			if (actions.ManuallyActivateTrigger) {

				Logger.MsgDebug(actions.ProfileSubtypeId + " Attempting To Manually Activate Triggers.", DebugTypeEnum.Action);

				foreach (var manualTrigger in Triggers) {

					if (actions.ManuallyActivatedTriggerNames.Contains(manualTrigger.ProfileSubtypeId))
						ProcessManualTrigger(manualTrigger, actions.ForceManualTriggerActivation);

				}

			}

			//ChangeInertiaDampeners
			if (actions.ChangeInertiaDampeners) {

				RemoteControl.DampenersOverride = actions.InertiaDampenersEnable;

			}

			//ChangeRotationDirection
			if (actions.ChangeRotationDirection) {

				_behavior.Settings.SetRotation(actions.RotationDirection);
				_behavior.AutoPilot.StopAllThrust();

			}

			//GenerateExplosion
			if (actions.GenerateExplosion) {

				var coords = Vector3D.Transform(actions.ExplosionOffsetFromRemote, RemoteControl.WorldMatrix);
				DamageHelper.CreateExplosion(coords, actions.ExplosionRange, actions.ExplosionDamage, RemoteControl, actions.ExplosionIgnoresVoxels);

			}

			//GridEditable
			if (actions.GridEditable != CheckEnum.Ignore) {

				_behavior.Grid.SetGridEditable(RemoteControl.SlimBlock.CubeGrid, actions.GridEditable == CheckEnum.Yes);

				if (actions.SubGridsEditable != CheckEnum.Ignore) {

					foreach (var cubeGrid in MyAPIGateway.GridGroups.GetGroup(RemoteControl.SlimBlock.CubeGrid, GridLinkTypeEnum.Physical)) {

						_behavior.Grid.SetGridEditable(cubeGrid, actions.SubGridsEditable == CheckEnum.Yes);

					}

				}

			}

			//GridDestructible
			if (actions.GridDestructible != CheckEnum.Ignore) {

				_behavior.Grid.SetGridDestructible(RemoteControl.SlimBlock.CubeGrid, actions.GridDestructible == CheckEnum.Yes);

				if (actions.SubGridsDestructible != CheckEnum.Ignore) {

					foreach (var cubeGrid in MyAPIGateway.GridGroups.GetGroup(RemoteControl.SlimBlock.CubeGrid, GridLinkTypeEnum.Physical)) {

						_behavior.Grid.SetGridDestructible(cubeGrid, actions.SubGridsDestructible == CheckEnum.Yes);

					}

				}

			}

			//RecolorGrid
			if (actions.RecolorGrid) {

				_behavior.Grid.RecolorBlocks(RemoteControl.SlimBlock.CubeGrid, actions.OldBlockColors, actions.NewBlockColors, actions.NewBlockSkins);

				if (actions.RecolorSubGrids) {

					foreach (var cubeGrid in MyAPIGateway.GridGroups.GetGroup(RemoteControl.SlimBlock.CubeGrid, GridLinkTypeEnum.Physical)) {

						_behavior.Grid.RecolorBlocks(cubeGrid, actions.OldBlockColors, actions.NewBlockColors, actions.NewBlockSkins);

					}

				}

			}

			//Enable Blocks
			if (actions.EnableBlocks) {

				_behavior.Grid.EnableBlocks(actions.EnableBlockNames, actions.EnableBlockStates);

			}

			//BuildProjectedBlocks
			if (actions.BuildProjectedBlocks) {

				_behavior.Grid.BuildProjectedBlocks(actions.MaxProjectedBlocksToBuild);

			}

			//ChangeBlockOwnership
			if (actions.ChangeBlockOwnership) {

				BlockHelper.ChangeBlockOwnership(RemoteControl.SlimBlock.CubeGrid, actions.OwnershipBlockNames, actions.OwnershipBlockFactions);

			}

			//RazeBlocks
			if (actions.RazeBlocksWithNames) {

				_behavior.Grid.RazeBlocksWithNames(actions.RazeBlocksNames);

			}

			//OverwriteAutopilotProfile
			if (actions.OverwriteAutopilotProfile) {

				_behavior.AutoPilot.AssignAutoPilotDataMode(actions.OverwriteAutopilotId, actions.OverwriteAutopilotMode);

			}

			//ChangeAutoPilotProfile
			if (actions.ChangeAutopilotProfile) {

				_behavior.AutoPilot.SetAutoPilotDataMode(actions.AutopilotProfile);

			}

			//CreateRandomLightning
			if (actions.CreateRandomLightning) {

				if (_behavior.AutoPilot.InGravity() && _behavior.AutoPilot.CurrentPlanet.HasAtmosphere) {

					var up = Vector3D.Normalize(RemoteControl.GetPosition() - _behavior.AutoPilot.CurrentPlanet.PositionComp.WorldAABB.Center);
					var randomPerpendicular = MyUtils.GetRandomPerpendicularVector(ref up);
					var strikeCoords = _behavior.AutoPilot.CurrentPlanet.GetClosestSurfacePointGlobal(randomPerpendicular * MathTools.RandomBetween(actions.LightningMinDistance, actions.LightningMaxDistance) + RemoteControl.GetPosition());
					DamageHelper.CreateLightning(strikeCoords, actions.LightningDamage, actions.LightningExplosionRadius, actions.LightningColor);

				}

			}

			//CreateLightningAtAttacker
			if (actions.CreateLightningAtAttacker && detectedEntity != 0) {

				if (_behavior.AutoPilot.InGravity() && _behavior.AutoPilot.CurrentPlanet.HasAtmosphere) {

					IMyEntity entity = null;

					if (MyAPIGateway.Entities.TryGetEntityById(detectedEntity, out entity)) {

						DamageHelper.CreateLightning(entity.PositionComp.WorldAABB.Center, actions.LightningDamage, actions.LightningExplosionRadius, actions.LightningColor);

					}

				}

			}

			//CreateLightningAtTarget
			if (actions.CreateLightningAtTarget && _behavior.AutoPilot.Targeting.HasTarget()) {

				if (_behavior.AutoPilot.InGravity() && _behavior.AutoPilot.CurrentPlanet.HasAtmosphere) {

					DamageHelper.CreateLightning(_behavior.AutoPilot.Targeting.TargetLastKnownCoords, actions.LightningDamage, actions.LightningExplosionRadius, actions.LightningColor);

				}

			}

			//AddDatapadsToSeats
			if (actions.AddDatapadsToSeats) {

				_behavior.Grid.InsertDatapadsIntoSeats(actions.DatapadNamesToAdd, actions.DatapadCountToAdd);

			}

			//StopAllRotation
			if (actions.StopAllRotation) {

				_behavior.AutoPilot.StopAllRotation();
				_behavior.AutoPilot.ApplyGyroRotation();

			}

			//StopAllThrust
			if (actions.StopAllThrust) {

				_behavior.AutoPilot.StopAllThrust();
				_behavior.AutoPilot.ApplyThrust();

			}

			//RandomGyroRotation
			if (actions.RandomGyroRotation) {

				if (MathTools.RandomBool())
					_behavior.AutoPilot.RotationToApply.X = MathTools.RandomBetween(-314, 315, 100);

				if (MathTools.RandomBool())
					_behavior.AutoPilot.RotationToApply.Y = MathTools.RandomBetween(-314, 315, 100);

				if (MathTools.RandomBool())
					_behavior.AutoPilot.RotationToApply.Z = MathTools.RandomBetween(-314, 315, 100);

				_behavior.AutoPilot.PrepareGyroForRotation();
				_behavior.AutoPilot.ApplyGyroRotation();

			}

			//RandomThrustDirection
			if (actions.RandomThrustDirection) {

				_behavior.AutoPilot.SetRandomThrust();
				_behavior.AutoPilot.ApplyThrust();

			}

			//SetBooleansTrue
			foreach (var variable in actions.SetBooleansTrue)
				_settings.SetCustomBool(variable, true);

			//SetBooleansFalse
			foreach (var variable in actions.SetBooleansFalse)
				_settings.SetCustomBool(variable, false);

			//IncreaseCounters
			foreach (var variable in actions.IncreaseCounters)
				_settings.SetCustomCounter(variable, 1);

			//DecreaseCounters
			foreach (var variable in actions.DecreaseCounters)
				_settings.SetCustomCounter(variable, -1);

			//ResetCounters
			foreach (var variable in actions.ResetCounters)
				_settings.SetCustomCounter(variable, 0, true);

			//SetCounters
			if (actions.SetCounters.Count == actions.SetCountersValues.Count) {

				for (int i = 0; i < actions.SetCounters.Count; i++)
					_settings.SetCustomCounter(actions.SetCounters[i], actions.SetCountersValues[i], false, true);

			}

			//SetSandboxBooleansTrue
			foreach (var variable in actions.SetSandboxBooleansTrue)
				SetSandboxBool(variable, true);

			//SetSandboxBooleansFalse
			foreach (var variable in actions.SetSandboxBooleansFalse)
				SetSandboxBool(variable, false);

			//IncreaseSandboxCounters
			foreach (var variable in actions.IncreaseSandboxCounters)
				SetSandboxCounter(variable, 1);

			//DecreaseSandboxCounters
			foreach (var variable in actions.DecreaseSandboxCounters)
				SetSandboxCounter(variable, -1);

			//ResetSandboxCounters
			foreach (var variable in actions.ResetSandboxCounters)
				SetSandboxCounter(variable, 0);

			//SetSandboxCounters
			if (actions.SetSandboxCounters.Count != 0 && actions.SetSandboxCounters.Count == actions.SetSandboxCountersValues.Count) {

				for (int i = 0; i < actions.SetCounters.Count; i++)
					SetSandboxCounter(actions.SetSandboxCounters[i], actions.SetSandboxCountersValues[i], true);

			}

			//BehaviorSpecificEventA
			if (actions.BehaviorSpecificEventA)
				_behavior.BehaviorActionA = true;

			//BehaviorSpecificEventB
			if (actions.BehaviorSpecificEventB)
				_behavior.BehaviorActionB = true;

			//BehaviorSpecificEventC
			if (actions.BehaviorSpecificEventC)
				_behavior.BehaviorActionC = true;

			//BehaviorSpecificEventD
			if (actions.BehaviorSpecificEventD)
				_behavior.BehaviorActionD = true;

			//BehaviorSpecificEventE
			if (actions.BehaviorSpecificEventE)
				_behavior.BehaviorActionE = true;

			//BehaviorSpecificEventF
			if (actions.BehaviorSpecificEventF)
				_behavior.BehaviorActionF = true;

			//BehaviorSpecificEventG
			if (actions.BehaviorSpecificEventG)
				_behavior.BehaviorActionG = true;

			//BehaviorSpecificEventH
			if (actions.BehaviorSpecificEventH)
				_behavior.BehaviorActionH = true;

		}

	}
}
