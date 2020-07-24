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
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI.Entities;
using RivalAI.Behavior.Subsystems.Profiles;
using RivalAI.Behavior.Subsystems.AutoPilot;

namespace RivalAI.Behavior {

	public class Horsefly : CoreBehavior, IBehavior {

		//Configurable
		public int HorseflyWaypointWaitTimeTrigger;
		public int HorseflyWaypointAbandonTimeTrigger;

		public byte Counter;
		public DateTime HorseflyWaypointWaitTime;
		public DateTime HorseflyWaypointAbandonTime;

		public Horsefly() : base() {

			_behaviorType = "Horsefly";

			HorseflyWaypointWaitTimeTrigger = 5;
			HorseflyWaypointAbandonTimeTrigger = 30;

			Counter = 0;
			HorseflyWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
			HorseflyWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;

		}

		public override void MainBehavior() {

			if(Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true) {

				ChangeCoreBehaviorMode(BehaviorMode.Retreat);
				AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | AutoPilot.UserCustomMode);

			}

			//Init
			if(Mode == BehaviorMode.Init) {

				if(!AutoPilot.Targeting.HasTarget()) {

					ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

				} else {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					this.HorseflyWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | AutoPilot.UserCustomMode | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.OffsetWaypoint);

				}

			}

			//Waiting For Target
			if(Mode == BehaviorMode.WaitingForTarget) {

				if(AutoPilot.Targeting.HasTarget()) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					this.HorseflyWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.OffsetWaypointGenerator(true);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | AutoPilot.UserCustomMode | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.OffsetWaypoint);

				} else if(Despawn.NoTargetExpire == true) {

					Despawn.Retreat();

				}

			}

			if(!AutoPilot.Targeting.HasTarget() && Mode != BehaviorMode.Retreat) {

				ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

			}

			//Approach
			if(Mode == BehaviorMode.ApproachTarget) {

				var timeSpan = MyAPIGateway.Session.GameDateTime - this.HorseflyWaypointAbandonTime;
				//Logger.MsgDebug("Distance To Waypoint: " + NewAutoPilot.DistanceToCurrentWaypoint.ToString(), DebugTypeEnum.General);

				if (ArrivedAtWaypoint()) {

					ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
					this.HorseflyWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), AutoPilot.UserCustomModeIdle);
					BehaviorTriggerA = true;

				} else if (timeSpan.TotalSeconds >= this.HorseflyWaypointAbandonTimeTrigger) {

					Logger.MsgDebug("Horsefly Timeout, Getting New Offset", DebugTypeEnum.General);
					this.HorseflyWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.OffsetWaypointGenerator(true);

				} else if (AutoPilot.IsWaypointThroughVelocityCollision()) {

					Logger.MsgDebug("Horsefly Velocity Through Collision, Getting New Offset", DebugTypeEnum.General);
					this.HorseflyWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.OffsetWaypointGenerator(true);

				}

			}

			//WaitAtWaypoint
			if (Mode == BehaviorMode.WaitAtWaypoint) {

				var timeSpan = MyAPIGateway.Session.GameDateTime - this.HorseflyWaypointWaitTime;

				if (timeSpan.TotalSeconds >= this.HorseflyWaypointWaitTimeTrigger) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					this.HorseflyWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.OffsetWaypointGenerator(true);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | AutoPilot.UserCustomMode | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.OffsetWaypoint);
					BehaviorTriggerB = true;

				}

			}

			//Retreat
			if (Mode == BehaviorMode.Retreat) {

				if (Despawn.NearestPlayer?.Controller?.ControlledEntity?.Entity != null) {

					//Logger.AddMsg("DespawnCoordsCreated", true);
					AutoPilot.SetInitialWaypoint(VectorHelper.GetDirectionAwayFromTarget(this.RemoteControl.GetPosition(), Despawn.NearestPlayer.GetPosition()) * 1000 + this.RemoteControl.GetPosition());

				}

			}

		}

		public bool ArrivedAtWaypoint() {

			if (AutoPilot.InGravity() && AutoPilot.MyAltitude < AutoPilot.Data.IdealPlanetAltitude) {

				if (AutoPilot.DistanceToWaypointAtMyAltitude == -1 || AutoPilot.DistanceToOffsetAtMyAltitude == -1)
					return false;

				if (AutoPilot.DistanceToWaypointAtMyAltitude < AutoPilot.Data.WaypointTolerance && AutoPilot.DistanceToOffsetAtMyAltitude < AutoPilot.Data.WaypointTolerance) {

					Logger.MsgDebug("Offset Compensation", DebugTypeEnum.General);
					return true;

				}

				return false;

			}

			if (AutoPilot.DistanceToCurrentWaypoint < AutoPilot.Data.WaypointTolerance)
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
			AutoPilot.Data = TagHelper.GetAutopilotProfile("RAI-Generic-Autopilot-Horsefly");
			Despawn.UseNoTargetTimer = true;

			//Get Settings From Custom Data
			InitCoreTags();
			InitTags();
			SetDefaultTargeting();

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

