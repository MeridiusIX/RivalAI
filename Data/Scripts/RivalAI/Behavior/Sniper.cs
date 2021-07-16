using Sandbox.ModAPI;
using RivalAI.Helpers;
using RivalAI.Behavior.Subsystems.AutoPilot;
using System;
using VRageMath;

namespace RivalAI.Behavior {

	public class Sniper : CoreBehavior, IBehavior{

		public DateTime SniperWaypointWaitTime;
		public DateTime SniperWaypointAbandonTime;

		public Sniper() : base() {

			_behaviorType = "Sniper";
			SniperWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
			SniperWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;

		}

		public override void MainBehavior() {

			if(RAI_SessionCore.IsServer == false) {

				return;

			}

			base.MainBehavior();

			//Logger.MsgDebug(Mode.ToString(), DebugTypeEnum.General);

			if (Mode != BehaviorMode.Retreat && Settings.DoRetreat == true){

				ChangeCoreBehaviorMode(BehaviorMode.Retreat);
				AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing, CheckEnum.Yes, CheckEnum.No);

			}
			
			if(Mode == BehaviorMode.Init) {

				if(!AutoPilot.Targeting.HasTarget()) {

					ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

				} else {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					AutoPilot.OffsetWaypointGenerator(true);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.OffsetWaypoint, CheckEnum.Yes, CheckEnum.No);

				}

			}

			if(Mode == BehaviorMode.WaitingForTarget) {

				if(AutoPilot.CurrentMode != AutoPilot.UserCustomModeIdle) {

					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.None, CheckEnum.No, CheckEnum.Yes);

				}

				if(AutoPilot.Targeting.HasTarget()) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					AutoPilot.OffsetWaypointGenerator(true);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.OffsetWaypoint, CheckEnum.Yes, CheckEnum.No);

				} else if(Despawn.NoTargetExpire == true){
					
					Despawn.Retreat();
					
				}

			}

			if(!AutoPilot.Targeting.HasTarget() && Mode != BehaviorMode.Retreat && Mode != BehaviorMode.WaitingForTarget) {


				ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);
				AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.None, CheckEnum.No, CheckEnum.Yes);

			}

			//ApproachTarget
			if (Mode == BehaviorMode.ApproachTarget) {

				var timeSpan = MyAPIGateway.Session.GameDateTime - this.SniperWaypointAbandonTime;
				//Logger.MsgDebug("Distance To Waypoint: " + NewAutoPilot.DistanceToCurrentWaypoint.ToString(), DebugTypeEnum.General);

				if (AutoPilot.ArrivedAtOffsetWaypoint()) {

					ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);
					this.SniperWaypointWaitTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.WaypointFromTarget);
					BehaviorTriggerA = true;

				} else if (timeSpan.TotalSeconds >= AutoPilot.Data.WaypointAbandonTimeTrigger) {

					Logger.MsgDebug("Sniper Timeout, Getting New Offset", DebugTypeEnum.General);
					this.SniperWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.OffsetWaypointGenerator(true);

				} else if (AutoPilot.IsWaypointThroughVelocityCollision()) {

					Logger.MsgDebug("Sniper Velocity Through Collision, Getting New Offset", DebugTypeEnum.General);
					this.SniperWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.OffsetWaypointGenerator(true);

				}

			}

			//ApproachWaypoint
			if (Mode == BehaviorMode.ApproachWaypoint) {

				var engageDistance = AutoPilot.InGravity() ? AutoPilot.Data.EngageDistancePlanet : AutoPilot.Data.EngageDistanceSpace;
				var disengageDistance = AutoPilot.InGravity() ? AutoPilot.Data.DisengageDistancePlanet : AutoPilot.Data.DisengageDistanceSpace;

				if (AutoPilot.DistanceToTargetWaypoint < engageDistance) {

					var distanceDifference = engageDistance - AutoPilot.DistanceToTargetWaypoint;
					var engageDifferenceHalved = (disengageDistance - engageDistance) / 2;
					var directionAwayFromTarget = Vector3D.Normalize(RemoteControl.GetPosition() - AutoPilot.Targeting.TargetLastKnownCoords);
					var fallbackCoords = directionAwayFromTarget * (distanceDifference + engageDifferenceHalved) + RemoteControl.GetPosition();
					AutoPilot.SetInitialWaypoint(fallbackCoords);

				} else {

					ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.WaypointFromTarget);

				}

			}

			//Engage
			if (Mode == BehaviorMode.EngageTarget) {

				var timeSpan = MyAPIGateway.Session.GameDateTime - this.SniperWaypointWaitTime;
				var engageDistance = AutoPilot.InGravity() ? AutoPilot.Data.EngageDistancePlanet : AutoPilot.Data.EngageDistanceSpace;
				var disengageDistance = AutoPilot.InGravity() ? AutoPilot.Data.DisengageDistancePlanet : AutoPilot.Data.DisengageDistanceSpace;

				if (timeSpan.TotalSeconds >= AutoPilot.Data.WaypointWaitTimeTrigger || AutoPilot.DistanceToTargetWaypoint > disengageDistance) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					this.SniperWaypointAbandonTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.OffsetWaypointGenerator(true);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.OffsetWaypoint, CheckEnum.Yes, CheckEnum.No);

				}

				if (AutoPilot.DistanceToTargetWaypoint < engageDistance) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachWaypoint);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing, CheckEnum.Yes, CheckEnum.No);

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

		public override void BehaviorInit(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Behavior Init For Sniper", DebugTypeEnum.General);

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			AutoPilot.Data = TagHelper.GetAutopilotProfile("RAI-Generic-Autopilot-Sniper");
			Despawn.UseNoTargetTimer = true;
			AutoPilot.Weapons.UseStaticGuns = true;

			//Get Settings From Custom Data
			InitCoreTags();
			InitTags();
			SetDefaultTargeting();

			SetupCompleted = true;

		}

		public void InitTags() {

			if(string.IsNullOrWhiteSpace(this.RemoteControl?.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach(var tag in descSplit) {
					
					

				}
				
			}

		}

	}

}
	
