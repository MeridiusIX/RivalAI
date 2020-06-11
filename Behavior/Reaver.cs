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
	
	public class Reaver : CoreBehavior, IBehavior{

		//Configurable
		public int ReaverTimeBetweenNewTargetChecks;
		public int ReaverTimeBetweenEngagableTargetChecks;
		public double ReaverDistanceToCheckEngagableTarget;

		//Non-Config
		private DateTime _checkActiveTargetTimer;
		private DateTime _checkTargetEngageableTimer;
		
		public Reaver() {

			ReaverTimeBetweenNewTargetChecks = 15;
			ReaverTimeBetweenEngagableTargetChecks = 3;
			ReaverDistanceToCheckEngagableTarget = 1200;

			_checkActiveTargetTimer = MyAPIGateway.Session.GameDateTime;
			_checkTargetEngageableTimer = MyAPIGateway.Session.GameDateTime;

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

				//Get Despawn Coords


				//Set Approach Waypoint

			}

			if (Mode == BehaviorMode.ApproachWaypoint) {

				//Check 15 Second Timer

				//Check Target

				//Approach Target If Valid

			}

			if (Mode == BehaviorMode.ApproachTarget) {



			}

			//Engage
			if (Mode == BehaviorMode.EngageTarget) {



			}

			//Retreat
			if (Mode == BehaviorMode.Retreat) {

				if (Despawn.NearestPlayer?.Controller?.ControlledEntity?.Entity != null) {

					//Logger.AddMsg("DespawnCoordsCreated", true);
					NewAutoPilot.SetInitialWaypoint(VectorHelper.GetDirectionAwayFromTarget(this.RemoteControl.GetPosition(), Despawn.NearestPlayer.GetPosition()) * 1000 + this.RemoteControl.GetPosition());

				}

			}


		}

		public override void BehaviorInit(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Behavior Init For Reaver", DebugTypeEnum.BehaviorSetup);

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			Despawn.UseNoTargetTimer = false;
			NewAutoPilot.Thrust.StrafeMinDurationMs = 1500;
			NewAutoPilot.Thrust.StrafeMaxDurationMs = 2000;
			NewAutoPilot.Thrust.AllowStrafing = true;
			NewAutoPilot.Weapons.UseStaticGuns = true;

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
	
