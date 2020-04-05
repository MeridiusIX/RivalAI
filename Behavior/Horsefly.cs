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

namespace RivalAI.Behavior {

	public class Horsefly : CoreBehavior, IBehavior {

		//Configurable
		public int HorseflyWaypointWaitTimeTrigger;
		public int HorseflyWaypointAbandonTimeTrigger;

		public byte Counter;
		public DateTime HorseflyWaypointWaitTime;
		public DateTime HorseflyWaypointAbandonTime;

		private bool _previouslyAvoidingCollision = false;

		public Horsefly() {

			HorseflyWaypointWaitTimeTrigger = 5;
			HorseflyWaypointAbandonTimeTrigger = 30;

			Counter = 0;
			HorseflyWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
			HorseflyWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;

		}

		public override void MainBehavior() {

			if(Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true) {

				Mode = BehaviorMode.Retreat;
				NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), false, true, true);

			}

			//Init
			if(Mode == BehaviorMode.Init) {

				if(NewAutoPilot.Targeting.InvalidTarget == true) {

					Mode = BehaviorMode.WaitingForTarget;

				} else {

					Mode = BehaviorMode.WaitAtWaypoint;
					this.HorseflyWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
					NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), true, true, true);

				}

			}

			//Waiting For Target
			if(Mode == BehaviorMode.WaitingForTarget) {

				if(NewAutoPilot.Targeting.InvalidTarget == false) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					this.HorseflyWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
					NewAutoPilot.SetRandomOffset(NewAutoPilot.Targeting.Target.Target, false);
					NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), true, true, true);

				} else if(Despawn.NoTargetExpire == true) {

					Despawn.Retreat();

				}

			}

			if(NewAutoPilot.Targeting.InvalidTarget == true && Mode != BehaviorMode.Retreat) {

				ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

			}

			//Approach
			if(Mode == BehaviorMode.ApproachTarget) {

				var timeSpan = MyAPIGateway.Session.GameDateTime - this.HorseflyWaypointAbandonTime;
				//Logger.MsgDebug("Distance To Waypoint: " + NewAutoPilot.DistanceToCurrentWaypoint.ToString(), DebugTypeEnum.General);

				if (ArrivedAtWaypoint()) {

					ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
					this.HorseflyWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
					NewAutoPilot.ActivateAutoPilot(AutoPilotType.None, NewAutoPilotMode.None, Vector3D.Zero);

				} else if (timeSpan.TotalSeconds >= this.HorseflyWaypointAbandonTimeTrigger) {

					Logger.MsgDebug("Horsefly Timeout, Getting New Offset", DebugTypeEnum.General);
					this.HorseflyWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
					NewAutoPilot.SetRandomOffset(NewAutoPilot.Targeting.Target.Target, false);

				} else if (NewAutoPilot.IsWaypointThroughVelocityCollision()) {

					Logger.MsgDebug("Horsefly Velocity Through Collision, Getting New Offset", DebugTypeEnum.General);
					this.HorseflyWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
					NewAutoPilot.SetRandomOffset(NewAutoPilot.Targeting.Target.Target, false);

				}

			}

			//WaitAtWaypoint
			if (Mode == BehaviorMode.WaitAtWaypoint) {

				var timeSpan = MyAPIGateway.Session.GameDateTime - this.HorseflyWaypointWaitTime;

				if (timeSpan.TotalSeconds >= this.HorseflyWaypointWaitTimeTrigger) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					this.HorseflyWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
					NewAutoPilot.SetRandomOffset(NewAutoPilot.Targeting.Target.Target, false);
					NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), true, true, true);

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

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			Despawn.UseNoTargetTimer = true;
			NewAutoPilot.Targeting.NeedsTarget = true;
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

			//Behavior Specific Default Enums (If None is Not Acceptable)
			if (NewAutoPilot.Targeting.TargetData.UseCustomTargeting == false) {

				NewAutoPilot.Targeting.TargetData.Target = TargetTypeEnum.Player;
				NewAutoPilot.Targeting.TargetData.Relations = TargetRelationEnum.Enemy;
				NewAutoPilot.Targeting.TargetData.Owners = TargetOwnerEnum.Player;

			}

			SetupCompleted = true;

		}

		public void InitTags() {

			if (string.IsNullOrWhiteSpace(this.RemoteControl?.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach (var tag in descSplit) {

					//HorseflyWaypointWaitTimeTrigger
					if (tag.Contains("[HorseflyWaypointWaitTimeTrigger:") == true) {

						this.HorseflyWaypointWaitTimeTrigger = TagHelper.TagIntCheck(tag, this.HorseflyWaypointWaitTimeTrigger);

					}

					//HorseflyWaypointAbandonTimeTrigger
					if (tag.Contains("[HorseflyWaypointAbandonTimeTrigger:") == true) {

						this.HorseflyWaypointAbandonTimeTrigger = TagHelper.TagIntCheck(tag, this.HorseflyWaypointAbandonTimeTrigger);

					}

				}

			}


		}

	}

}

