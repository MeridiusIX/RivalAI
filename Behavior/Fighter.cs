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

namespace RivalAI.Behavior{
	
	public class Fighter : CoreBehavior, IBehavior{

		//Configurable
		public double FighterEngageDistanceSpace;
		public double FighterEngageDistancePlanet;

		public double FighterDisengageDistanceSpace;
		public double FighterDisengageDistancePlanet;
		
		public byte Counter;

		public Fighter() {

			FighterEngageDistanceSpace = 400;
			FighterEngageDistancePlanet = 600;

			FighterDisengageDistanceSpace = 600;
			FighterDisengageDistancePlanet = 600;
			
			Counter = 0;

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

				bool inRange = false;

				if (!NewAutoPilot.InGravity() && NewAutoPilot.DistanceToInitialWaypoint < this.FighterEngageDistanceSpace)
					inRange = true;

				if(NewAutoPilot.InGravity() && NewAutoPilot.DistanceToInitialWaypoint < this.FighterEngageDistancePlanet)
					inRange = true;

				if (inRange) {

					NewAutoPilot.ActivateAutoPilot(AutoPilotType.RivalAI, NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.Strafe, Vector3D.Zero, true, false, false);
					ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);

				}

			}

			//Engage
			if (Mode == BehaviorMode.EngageTarget) {

				bool outRange = false;

				if (!NewAutoPilot.InGravity() && NewAutoPilot.DistanceToInitialWaypoint > this.FighterDisengageDistanceSpace)
					outRange = true;

				if (NewAutoPilot.InGravity() && NewAutoPilot.DistanceToInitialWaypoint > this.FighterDisengageDistancePlanet)
					outRange = true;

				if (outRange) {

					NewAutoPilot.ActivateAutoPilot(AutoPilotType.Legacy, NewAutoPilotMode.None, this.RemoteControl.GetPosition(), true, true, true);
					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);

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

		public override void BehaviorInit(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Behavior Init For Fighter", DebugTypeEnum.General);

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			Despawn.UseNoTargetTimer = true;
			NewAutoPilot.Thrust.StrafeMinDurationMs = 1500;
			NewAutoPilot.Thrust.StrafeMaxDurationMs = 2000;
			NewAutoPilot.Thrust.AllowStrafing = true;
			NewAutoPilot.Weapons.UseStaticGuns = true;

			//Get Settings From Custom Data
			InitCoreTags();
			InitTags();

			if (NewAutoPilot.Targeting.Data.UseCustomTargeting == false) {

				NewAutoPilot.Targeting.Data.UseCustomTargeting = true;
				NewAutoPilot.Targeting.Data.Target = TargetTypeEnum.Player;
				NewAutoPilot.Targeting.Data.Relations = TargetRelationEnum.Enemy;
				NewAutoPilot.Targeting.Data.Owners = TargetOwnerEnum.Player;

			}

			SetupCompleted = true;

		}

		public void InitTags() {

			if(string.IsNullOrWhiteSpace(this.RemoteControl?.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach(var tag in descSplit) {
					
					//FighterEngageDistanceSpace
					if(tag.Contains("[FighterEngageDistanceSpace:") == true) {

						this.FighterEngageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.FighterEngageDistanceSpace);

					}	
			
					//FighterEngageDistancePlanet
					if(tag.Contains("[FighterEngageDistancePlanet:") == true) {

						this.FighterEngageDistancePlanet = TagHelper.TagDoubleCheck(tag, this.FighterEngageDistancePlanet);

					}

					//FighterDisengageDistanceSpace
					if (tag.Contains("[FighterDisengageDistanceSpace:") == true) {

						this.FighterDisengageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.FighterDisengageDistanceSpace);

					}

					//FighterDisengageDistancePlanet
					if (tag.Contains("[FighterDisengageDistancePlanet:") == true) {

						this.FighterDisengageDistancePlanet = TagHelper.TagDoubleCheck(tag, this.FighterDisengageDistancePlanet);

					}

				}
				
			}

		}

	}

}
	
