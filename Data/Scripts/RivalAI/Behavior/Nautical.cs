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

namespace RivalAI.Behavior{
	
	public class Nautical : CoreBehavior, IBehavior{

		DateTime WaitTime;

		public Nautical() : base() {

			_behaviorType = "Nautical";
			WaitTime = MyAPIGateway.Session.GameDateTime;

		}

		public override void MainBehavior() {

			if(RAI_SessionCore.IsServer == false) {

				return;

			}

			//Logger.MsgDebug(Mode.ToString(), DebugTypeEnum.General);
			
			if(Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true){

				ChangeCoreBehaviorMode(BehaviorMode.Retreat);
				AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.LevelWithGravity | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.WaterNavigation);

			}
			
			if(Mode == BehaviorMode.Init) {

				if(!AutoPilot.Targeting.HasTarget()) {

					ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);
					BehaviorTriggerD = true;

				} else {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.LevelWithGravity | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.WaterNavigation);

				}

			}

			if(Mode == BehaviorMode.WaitingForTarget) {

				if(AutoPilot.CurrentMode != AutoPilot.UserCustomModeIdle) {

					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.LevelWithGravity);

				}

				if(AutoPilot.Targeting.HasTarget()) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.LevelWithGravity | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.WaterNavigation);

				} else if(Despawn.NoTargetExpire == true){
					
					Despawn.Retreat();
					BehaviorTriggerD = true;

				}

			}

			if(!AutoPilot.Targeting.HasTarget() && Mode != BehaviorMode.Retreat && Mode != BehaviorMode.WaitingForTarget) {


				ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);
				AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.LevelWithGravity);
				BehaviorTriggerD = true;

			}

			//A - Stop All Movement
			if (BehaviorActionA) {

				BehaviorActionA = false;
				ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
				AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.LevelWithGravity);
				WaitTime = MyAPIGateway.Session.GameDateTime;
				BehaviorTriggerC = true;

			}

			//WaitAtWaypoint
			if (Mode == BehaviorMode.WaitAtWaypoint) {

				var timespan = MyAPIGateway.Session.GameDateTime - WaitTime;

				if (timespan.TotalSeconds >= AutoPilot.Data.WaypointWaitTimeTrigger) {

					ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);
					BehaviorTriggerD = true;

				}

			}

			//Approach
			if (Mode == BehaviorMode.ApproachTarget) {

				bool inRange = false;

				if (!AutoPilot.InGravity() && AutoPilot.DistanceToTargetWaypoint < AutoPilot.Data.EngageDistanceSpace)
					inRange = true;

				if(AutoPilot.InGravity() && AutoPilot.DistanceToTargetWaypoint < AutoPilot.Data.EngageDistancePlanet)
					inRange = true;

				if (inRange) {

					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToTarget | NewAutoPilotMode.LevelWithGravity | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.WaterNavigation);
					ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);
					BehaviorTriggerA = true;

				}

			}

			//Engage
			if (Mode == BehaviorMode.EngageTarget) {

				bool outRange = false;

				if (!AutoPilot.InGravity() && AutoPilot.DistanceToTargetWaypoint > AutoPilot.Data.DisengageDistanceSpace)
					outRange = true;

				if (AutoPilot.InGravity() && AutoPilot.DistanceToTargetWaypoint > AutoPilot.Data.DisengageDistancePlanet)
					outRange = true;

				if (outRange) {

					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.LevelWithGravity | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.WaterNavigation);
					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
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

		public override void BehaviorInit(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Behavior Init For Nautical", DebugTypeEnum.General);

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			AutoPilot.Data = TagHelper.GetAutopilotProfile("RAI-Generic-Autopilot-Nautical");
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
	
