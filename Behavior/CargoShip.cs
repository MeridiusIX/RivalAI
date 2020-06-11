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

	public enum CargoShipMode {
	
		SpawnerEndCoords,
		RandomEndCoords,
		UseCustomPath,
	
	}

	public class CargoShip : CoreBehavior, IBehavior{

		//Configurable
		
		public bool CargoShipUseCustomPath;
		public bool CargoShipUseLastWaypointAsDespawn;
		public Vector3D CargoShipCustomWaypoints;

		public bool CargoShipCalculateRandomDespawn;

		public CargoShip() {

			

		}

		public override void MainBehavior() {

			if(RAI_SessionCore.IsServer == false) {

				return;

			}

			Logger.MsgDebug(Mode.ToString(), DebugTypeEnum.General);
			
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

				} else if(Despawn.NoTargetExpire == true){
					
					Despawn.Retreat();
					
				}

			}

			if(!NewAutoPilot.Targeting.HasTarget() && Mode != BehaviorMode.Retreat && Mode != BehaviorMode.WaitingForTarget) {


				ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);
				NewAutoPilot.ActivateAutoPilot(AutoPilotType.None, NewAutoPilotMode.None, Vector3D.Zero);

			}

			//Approach
			if (Mode == BehaviorMode.ApproachTarget) {



			}

			//WaitWaitAtWaypoint
			if (Mode == BehaviorMode.WaitAtWaypoint) {



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

			Logger.MsgDebug("Beginning Behavior Init For CargoShip", DebugTypeEnum.General);

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			Despawn.UseNoTargetTimer = false;
			NewAutoPilot.Thrust.AllowStrafing = false;
			NewAutoPilot.Weapons.UseStaticGuns = false;

			//Get Settings From Custom Data
			InitCoreTags();
			InitTags();
			//SetDefaultTargeting();

			SetupCompleted = true;

		}

		public void InitTags() {

			if(string.IsNullOrWhiteSpace(this.RemoteControl?.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach(var tag in descSplit) {
					
					//FighterEngageDistanceSpace
					if(tag.Contains("[FighterEngageDistanceSpace:") == true) {

						//this.FighterEngageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.FighterEngageDistanceSpace);

					}	

				}
				
			}

		}

	}

}
	
