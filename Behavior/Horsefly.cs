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

	public class Horsefly:CoreBehavior {

		//Configurable
		public double HorseflyMinDistFromWaypoint;
		public double HorseflyMinDistFromTarget;
		public double HorseflyMaxDistFromTarget;
		public int HorseflyWaypointWaitTimeTrigger;
		public int HorseflyWaypointAbandonTimeTrigger;

		public bool ReceivedEvadeSignal;
		public bool ReceivedRetreatSignal;
		public bool ReceivedExternalTarget;

		public byte Counter;
		public int HorseflyWaypointWaitTime;
		public int HorseflyWaypointAbandonTime;

		public Horsefly() {

			HorseflyMinDistFromWaypoint = 50;
			HorseflyMinDistFromTarget = 150;
			HorseflyMaxDistFromTarget = 300;
			HorseflyWaypointWaitTimeTrigger = 5;
			HorseflyWaypointAbandonTimeTrigger = 30;

			ReceivedEvadeSignal = false;
			ReceivedRetreatSignal = false;
			ReceivedExternalTarget = false;

			Counter = 0;
			HorseflyWaypointWaitTime = 0;
			HorseflyWaypointAbandonTime = 0;

		}

		public void RunAi() {

			if(!IsAIReady())
				return;

			RunCoreAi();

			if(EndScript == true) {

				return;

			}

			Counter++;

			if(Counter >= 60) {

				MainBehavior();
				Counter = 0;

			}


		}

		public void MainBehavior() {

			if(RAI_SessionCore.IsServer == false) {

				return;

			}
			
			if(Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true) {

				Mode = BehaviorMode.Retreat;
				NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), false, true, true);

			}

			if(Mode == BehaviorMode.Init) {

				if(NewAutoPilot.Targeting.InvalidTarget == true) {

					Mode = BehaviorMode.WaitingForTarget;

				} else {

					Mode = BehaviorMode.WaitAtWaypoint;
					this.HorseflyWaypointWaitTime = this.HorseflyWaypointWaitTimeTrigger;
					NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), true, true, true);

				}

			}

			if(Mode == BehaviorMode.WaitingForTarget) {

				if(NewAutoPilot.Targeting.InvalidTarget == false) {

					ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
					this.HorseflyWaypointWaitTime = this.HorseflyWaypointWaitTimeTrigger;
					NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), true, true, true);

				} else if(Despawn.NoTargetExpire == true) {

					Despawn.Retreat();

				}

			}

			if(NewAutoPilot.Targeting.InvalidTarget == true && Mode != BehaviorMode.Retreat) {

				ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

			}

			//WaitAtWaypoint
			if(Mode == BehaviorMode.WaitAtWaypoint == true) {



			}

			//Approach
			if(Mode == BehaviorMode.ApproachTarget) {
				


			}

			//Retreat
			if(Mode == BehaviorMode.Retreat) {

				

			}

		}

		public void BehaviorInit(IMyRemoteControl remoteControl) {

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			Despawn.UseNoTargetTimer = true;
			NewAutoPilot.Targeting.NeedsTarget = true;

			//Get Settings From Custom Data
			InitCoreTags();

			//Behavior Specific Default Enums (If None is Not Acceptable)
			if(NewAutoPilot.Targeting.TargetType == TargetTypeEnum.None) {

				NewAutoPilot.Targeting.TargetType = TargetTypeEnum.Player;

			}

			if(NewAutoPilot.Targeting.TargetRelation == TargetRelationEnum.None) {

				NewAutoPilot.Targeting.TargetRelation = TargetRelationEnum.Enemy;

			}

			if(NewAutoPilot.Targeting.TargetOwner == TargetOwnerEnum.None) {

				NewAutoPilot.Targeting.TargetOwner = TargetOwnerEnum.Player;

			}

		}

		public void InitTags() {

			//Core Tags


			//Behavior Tags


		}

	}

}

