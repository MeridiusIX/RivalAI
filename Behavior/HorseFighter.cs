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
using RivalAI.Entities;
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Behavior{
	
	public class HorseFighter : CoreBehavior, IBehavior{

		//Configurable
		public double HorseFighterEngageDistanceSpace;
		public double HorseFighterEngageDistancePlanet;

		public double HorseFighterDisengageDistanceSpace;
		public double HorseFighterDisengageDistancePlanet;

		public int HorseFighterWaypointWaitTimeTrigger;
		public int HorseFighterWaypointAbandonTimeTrigger;

		public int HorseFighterTimeApproaching;
		public int HorseFighterTimeEngaging;

		public DateTime HorseFighterWaypointWaitTime;
		public DateTime HorseFighterWaypointAbandonTime;
		public DateTime HorseFighterModeSwitchTime;

		public bool FighterMode;

		public byte Counter;

		public HorseFighter() {

			HorseFighterEngageDistanceSpace = 400;
			HorseFighterEngageDistancePlanet = 600;

			HorseFighterDisengageDistanceSpace = 600;
			HorseFighterDisengageDistancePlanet = 600;

			HorseFighterWaypointWaitTimeTrigger = 5;
			HorseFighterWaypointAbandonTimeTrigger = 30;

			HorseFighterTimeApproaching = 30;
			HorseFighterTimeEngaging = 10;

			HorseFighterWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
			HorseFighterWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
			HorseFighterModeSwitchTime = MyAPIGateway.Session.GameDateTime;

			FighterMode = false;

			Counter = 0;

		}

		public override void MainBehavior() {

			if(RAI_SessionCore.IsServer == false) {

				return;

			}

			//Logger.MsgDebug(Mode.ToString(), DebugTypeEnum.General);
			
			if(Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true){

				ChangeCoreBehaviorMode(BehaviorMode.Retreat);
				NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), false, false, true);

			}
			
			if(Mode == BehaviorMode.Init) {

				if(!NewAutoPilot.Targeting.HasTarget()) {

					ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

				} else {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), true, true, true);

				}

			}

			if(Mode == BehaviorMode.WaitingForTarget) {

				if(NewAutoPilot.CurrentMode != NewAutoPilotMode.None) {

					NewAutoPilot.ActivateAutoPilot(AutoPilotType.None, NewAutoPilotMode.None, Vector3D.Zero);

				}

				if(NewAutoPilot.Targeting.HasTarget()) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), true, true, true);
					BehaviorTriggerA = true;

				} else if(Despawn.NoTargetExpire == true){
					
					Despawn.Retreat();
					
				}

			}

			if(!NewAutoPilot.Targeting.HasTarget() && Mode != BehaviorMode.Retreat && Mode != BehaviorMode.WaitingForTarget) {


				ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);
				NewAutoPilot.ActivateAutoPilot(AutoPilotType.None, NewAutoPilotMode.None, Vector3D.Zero);

			}

			//TimerThing
			var modeTime = MyAPIGateway.Session.GameDateTime - HorseFighterModeSwitchTime;

			if (modeTime.TotalSeconds > (FighterMode ? HorseFighterTimeEngaging : HorseFighterTimeApproaching)) {

				HorseFighterModeSwitchTime = MyAPIGateway.Session.GameDateTime;
				FighterMode = FighterMode ? false : true;
				Logger.MsgDebug("HorseFighter Using Fighter Mode: " + FighterMode.ToString(), DebugTypeEnum.General);

			}

			//Approach
			if (Mode == BehaviorMode.ApproachTarget) {

				if (FighterMode && NewAutoPilot.DistanceToInitialWaypoint < (NewAutoPilot.InGravity() ? HorseFighterEngageDistancePlanet : HorseFighterEngageDistanceSpace)) {

					NewAutoPilot.ActivateAutoPilot(AutoPilotType.RivalAI, NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.Strafe, Vector3D.Zero, true, false, false);
					ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);
					BehaviorTriggerC = true;

				} else {

					var timeSpan = MyAPIGateway.Session.GameDateTime - this.HorseFighterWaypointAbandonTime;

					if (ArrivedAtWaypoint()) {

						ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
						this.HorseFighterWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
						NewAutoPilot.ActivateAutoPilot(AutoPilotType.None, NewAutoPilotMode.None, Vector3D.Zero);
						BehaviorTriggerB = true;

					} else if (timeSpan.TotalSeconds >= this.HorseFighterWaypointAbandonTimeTrigger) {

						this.HorseFighterWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
						NewAutoPilot.SetRandomOffset(NewAutoPilot.Targeting.Target.GetEntity(), false);

					} else if (NewAutoPilot.IsWaypointThroughVelocityCollision()) {

						this.HorseFighterWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
						NewAutoPilot.SetRandomOffset(NewAutoPilot.Targeting.Target.GetEntity(), false);

					}

				}

			}

			//Engage
			if (Mode == BehaviorMode.EngageTarget) {

				bool outRange = false;

				if (FighterMode) {

					outRange = NewAutoPilot.DistanceToInitialWaypoint > (NewAutoPilot.InGravity() ? HorseFighterDisengageDistancePlanet : HorseFighterDisengageDistanceSpace);

				} else {

					outRange = true;

				}

				if (outRange) {

					NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), true, true, true);
					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					BehaviorTriggerA = true;


				}

			}

			//WaitAtWaypoint
			if (Mode == BehaviorMode.WaitAtWaypoint) {

				var timeSpan = MyAPIGateway.Session.GameDateTime - this.HorseFighterWaypointWaitTime;

				if (timeSpan.TotalSeconds >= this.HorseFighterWaypointWaitTimeTrigger) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					this.HorseFighterWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
					NewAutoPilot.SetRandomOffset(NewAutoPilot.Targeting.Target.GetEntity(), false);
					NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), true, true, true);
					BehaviorTriggerA = true;

				}

			}

			//Retreat
			if (Mode == BehaviorMode.Retreat) {

				if (Despawn.NearestPlayer?.Controller?.ControlledEntity?.Entity != null) {

					//Logger.AddMsg("DespawnCoordsCreated", true);
					NewAutoPilot.SetInitialWaypoint(VectorHelper.GetDirectionAwayFromTarget(this.RemoteControl.GetPosition(), Despawn.NearestPlayer.GetPosition()) * 1000 + this.RemoteControl.GetPosition());

				}

			}


		}

		public bool ArrivedAtWaypoint() {

			if (NewAutoPilot.InGravity() && NewAutoPilot.MyAltitude < NewAutoPilot.IdealPlanetAltitude) {

				if (NewAutoPilot.DistanceToWaypointAtMyAltitude == -1 || NewAutoPilot.DistanceToOffsetAtMyAltitude == -1)
					return false;

				if (NewAutoPilot.DistanceToWaypointAtMyAltitude < NewAutoPilot.WaypointTolerance && NewAutoPilot.DistanceToOffsetAtMyAltitude < NewAutoPilot.WaypointTolerance) {

					Logger.MsgDebug("Offset Compensation", DebugTypeEnum.General);
					return true;

				}

				return false;

			}

			if (NewAutoPilot.DistanceToCurrentWaypoint < NewAutoPilot.WaypointTolerance)
				return true;

			/*
			if (NewAutoPilot.IsAvoidingCollision() && !_previouslyAvoidingCollision) {

				_previouslyAvoidingCollision = true;
				return false;

			}

			if (_previouslyAvoidingCollision && !NewAutoPilot.IsAvoidingCollision()) {

				_previouslyAvoidingCollision = false;
				return true;


			}
			*/

			return false;

		}

		public override void BehaviorInit(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Behavior Init For HorseFighter", DebugTypeEnum.BehaviorSetup);

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			Despawn.UseNoTargetTimer = true;
			NewAutoPilot.Thrust.StrafeMinDurationMs = 1500;
			NewAutoPilot.Thrust.StrafeMaxDurationMs = 2000;
			NewAutoPilot.Thrust.AllowStrafing = true;
			NewAutoPilot.Weapons.UseStaticGuns = true;
			NewAutoPilot.MinimumPlanetAltitude = 200;
			NewAutoPilot.IdealPlanetAltitude = 300;
			NewAutoPilot.WaypointTolerance = 30;
			NewAutoPilot.OffsetSpaceMinDistFromTarget = 150;
			NewAutoPilot.OffsetSpaceMaxDistFromTarget = 300;
			NewAutoPilot.OffsetPlanetMinDistFromTarget = 150;
			NewAutoPilot.OffsetPlanetMaxDistFromTarget = 300;
			NewAutoPilot.OffsetPlanetMinTargetAltitude = -200;
			NewAutoPilot.OffsetPlanetMaxTargetAltitude = 200;

			//Get Settings From Custom Data
			InitCoreTags();
			InitTags();

			if (string.IsNullOrWhiteSpace(NewAutoPilot.Targeting.Data.ProfileSubtypeId)) {

				byte[] byteData = { };

				if (TagHelper.TargetObjectTemplates.TryGetValue("RivalAI-GenericTargetProfile-EnemyPlayer", out byteData) == true) {

					try {

						var profile = MyAPIGateway.Utilities.SerializeFromBinary<TargetProfile>(byteData);

						if (profile != null) {

							NewAutoPilot.Targeting.NormalData = profile;

						}

					} catch (Exception) {



					}

				}

			}

			SetupCompleted = true;

		}

		public void InitTags() {

			if(string.IsNullOrWhiteSpace(this.RemoteControl?.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach(var tag in descSplit) {

					//HorseFighterEngageDistanceSpace
					if (tag.Contains("[HorseFighterEngageDistanceSpace:") == true) {

						this.HorseFighterEngageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.HorseFighterEngageDistanceSpace);

					}

					//HorseFighterEngageDistancePlanet
					if (tag.Contains("[HorseFighterEngageDistancePlanet:") == true) {

						this.HorseFighterEngageDistancePlanet = TagHelper.TagDoubleCheck(tag, this.HorseFighterEngageDistancePlanet);

					}

					//HorseFighterDisengageDistanceSpace
					if (tag.Contains("[HorseFighterDisengageDistanceSpace:") == true) {

						this.HorseFighterDisengageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.HorseFighterDisengageDistanceSpace);

					}

					//HorseFighterDisengageDistancePlanet
					if (tag.Contains("[HorseFighterDisengageDistancePlanet:") == true) {

						this.HorseFighterDisengageDistancePlanet = TagHelper.TagDoubleCheck(tag, this.HorseFighterDisengageDistancePlanet);

					}

					//HorseFighterWaypointWaitTimeTrigger
					if (tag.Contains("[HorseFighterWaypointWaitTimeTrigger:") == true) {

						this.HorseFighterWaypointWaitTimeTrigger = TagHelper.TagIntCheck(tag, this.HorseFighterWaypointWaitTimeTrigger);

					}

					//HorseFighterWaypointAbandonTimeTrigger
					if (tag.Contains("[HorseFighterWaypointAbandonTimeTrigger:") == true) {

						this.HorseFighterWaypointAbandonTimeTrigger = TagHelper.TagIntCheck(tag, this.HorseFighterWaypointAbandonTimeTrigger);

					}

					//HorseFighterTimeApproaching
					if (tag.Contains("[HorseFighterTimeApproaching:") == true) {

						this.HorseFighterTimeApproaching = TagHelper.TagIntCheck(tag, this.HorseFighterTimeApproaching);

					}

					//HorseFighterTimeEngaging
					if (tag.Contains("[HorseFighterTimeEngaging:") == true) {

						this.HorseFighterTimeEngaging = TagHelper.TagIntCheck(tag, this.HorseFighterTimeEngaging);

					}

				}
				
			}

		}

	}

}
	
