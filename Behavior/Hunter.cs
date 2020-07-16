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
using RivalAI.Behavior.Subsystems.AutoPilot;

namespace RivalAI.Behavior{
	
	public class Hunter : CoreBehavior, IBehavior{

		//Configurable
		public int TimeBetweenNewTargetChecks;
		public int LostTargetTimerTrigger;
		public double DistanceToCheckEngagableTarget;

		public bool EngageOnCameraDetection;
		public bool EngageOnWeaponActivation;

		public AutoPilotProfile NonCombatAutopilot;
		public AutoPilotProfile CombatAutopilot;

		//Non-Config
		private DateTime _checkActiveTargetTimer;
		private DateTime _lostTargetTimer;
		
		public Hunter() : base() {

			_behaviorType = "Hunter";

			TimeBetweenNewTargetChecks = 15;
			LostTargetTimerTrigger = 3;
			DistanceToCheckEngagableTarget = 1200;

			EngageOnCameraDetection = false;
			EngageOnWeaponActivation = false;

			_checkActiveTargetTimer = MyAPIGateway.Session.GameDateTime;
			_lostTargetTimer = MyAPIGateway.Session.GameDateTime;

			NonCombatAutopilot = null;
			CombatAutopilot = null;

		}

		//A: Found Target (Approach)
		//B: Lost Target (Still Approach)
		//C: Lost Target (Go To Despawn)
		//D: Engage Target


		public override void MainBehavior() {

			if(RAI_SessionCore.IsServer == false) {

				return;

			}

			//Logger.MsgDebug(Mode.ToString(), DebugTypeEnum.General);
			
			if(Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true){

				ChangeCoreBehaviorMode(BehaviorMode.Retreat);
				AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | AutoPilot.UserCustomMode);

			}
			
			if(Mode == BehaviorMode.Init) {

				if (Settings.DespawnCoords == Vector3D.Zero) {

					Settings.DespawnCoords = MESApi.GetDespawnCoords(RemoteControl.SlimBlock.CubeGrid);

					if (Settings.DespawnCoords == Vector3D.Zero)
						Settings.DespawnCoords = AutoPilot.CalculateDespawnCoords();

				}

				ReturnToDespawn();

			}

			if (BehaviorActionA) {

				BehaviorActionA = false;

				if (Settings.LastDamagerEntity != 0) {

					IMyEntity tempEntity = null;

					if (MyAPIGateway.Entities.TryGetEntityById(Settings.LastDamagerEntity, out tempEntity)) {

						AutoPilot.Targeting.ForceTargetEntityId = Settings.LastDamagerEntity;
						AutoPilot.Targeting.ForceTargetEntity = tempEntity;
						AutoPilot.Targeting.ForceRefresh = true;
						ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
						return;

					}

				}


			}

			if (Mode == BehaviorMode.ApproachWaypoint) {

				var time = MyAPIGateway.Session.GameDateTime - _checkActiveTargetTimer;

				if (time.TotalSeconds > TimeBetweenNewTargetChecks) {

					_checkActiveTargetTimer = MyAPIGateway.Session.GameDateTime;

					if (AutoPilot.Targeting.HasTarget()) {

						ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
						_lostTargetTimer = MyAPIGateway.Session.GameDateTime;
						BehaviorTriggerA = true;
						AutoPilot.Data = CombatAutopilot;
						AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget | AutoPilot.UserCustomMode);

					}
					
				}

			}

			if (Mode == BehaviorMode.ApproachTarget) {

				if (!AutoPilot.Targeting.HasTarget()) {

					AutoPilot.SetInitialWaypoint(AutoPilot.Targeting.TargetLastKnownCoords);
					var time = MyAPIGateway.Session.GameDateTime - _lostTargetTimer;

					if (time.TotalSeconds > LostTargetTimerTrigger) {

						ReturnToDespawn();
						return;

					}

				}

				//Check Turret

				//Check Visual Range

				


			}

			//Engage
			if (Mode == BehaviorMode.EngageTarget) {



			}

			//Retreat
			if (Mode == BehaviorMode.Retreat) {

				if (Despawn.NearestPlayer?.Controller?.ControlledEntity?.Entity != null) {

					//Logger.AddMsg("DespawnCoordsCreated", true);
					AutoPilot.SetInitialWaypoint(VectorHelper.GetDirectionAwayFromTarget(this.RemoteControl.GetPosition(), Despawn.NearestPlayer.GetPosition()) * 1000 + this.RemoteControl.GetPosition());

				}

			}


		}

		public void ReturnToDespawn() {

			ChangeCoreBehaviorMode(BehaviorMode.ApproachWaypoint);
			AutoPilot.Data = NonCombatAutopilot;
			AutoPilot.ActivateAutoPilot(Settings.DespawnCoords, NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | AutoPilot.UserCustomMode);
			_checkActiveTargetTimer = MyAPIGateway.Session.GameDateTime;

		}

		public override void BehaviorInit(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Behavior Init For Reaver", DebugTypeEnum.BehaviorSetup);

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			Despawn.UseNoTargetTimer = false;
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
	
