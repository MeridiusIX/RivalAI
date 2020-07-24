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
	
	public class Hunter : CoreBehavior, IBehavior{

		//Configurable
		public int TimeBetweenNewTargetChecks;
		public int LostTargetTimerTrigger;
		public double DistanceToCheckEngagableTarget;

		public bool EngageOnCameraDetection;
		public bool EngageOnWeaponActivation;
		public bool EngageOnTargetLineOfSight;

		public double EngageDistanceSpace;
		public double EngageDistancePlanet;

		public double DisengageDistanceSpace;
		public double DisengageDistancePlanet;

		public double CameraDetectionMaxRange;

		//Non-Config
		private DateTime _checkActiveTargetTimer;
		private DateTime _lostTargetTimer;

		private bool _inRange;
		
		public Hunter() : base() {

			_behaviorType = "Hunter";

			TimeBetweenNewTargetChecks = 15;
			LostTargetTimerTrigger = 3;
			DistanceToCheckEngagableTarget = 1200;

			EngageOnCameraDetection = false;
			EngageOnWeaponActivation = false;
			EngageOnTargetLineOfSight = false;

			EngageDistanceSpace = 500;
			EngageDistancePlanet = 500;

			DisengageDistanceSpace = 600;
			DisengageDistancePlanet = 600;

			CameraDetectionMaxRange = 1800;

			_checkActiveTargetTimer = MyAPIGateway.Session.GameDateTime;
			_lostTargetTimer = MyAPIGateway.Session.GameDateTime;

			_inRange = false;

		}

		//A: Found Target (Approach)
		//B: Lost Target (Still Approach)
		//C: Lost Target (Go To Despawn)
		//D: Engage Target
		//E: Engage In Range
		//F: Engage Out Range


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
						AutoPilot.SetAutoPilotDataMode(AutoPilotDataMode.Secondary);
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

					return;

				}

				_lostTargetTimer = MyAPIGateway.Session.GameDateTime;
				bool engageTarget = false;
				var targetDist = Vector3D.Distance(RemoteControl.GetPosition(), AutoPilot.Targeting.TargetLastKnownCoords);

				//Check Turret
				if (EngageOnWeaponActivation == true) {

					if (AutoPilot.Weapons.GetTurretTarget() != 0)
						engageTarget = true;


				}

				//Check Visual Range
				if (!engageTarget && EngageOnCameraDetection && targetDist < CameraDetectionMaxRange) {
				
					if(Grid.RaycastGridCheck(AutoPilot.Targeting.TargetLastKnownCoords))
						engageTarget = true;

				}

				//Check Collision Data
				if (AutoPilot.Targeting.Data.MaxLineOfSight > 0) {
				
					if(AutoPilot.Targeting.Target.GetParentEntity().EntityId == AutoPilot.Collision.TargetResult.GetCollisionEntity().EntityId)
						engageTarget = true;

				}

				if (engageTarget) {

					BehaviorTriggerD = true;
					ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);

				}

			}

			//Engage
			if (Mode == BehaviorMode.EngageTarget) {

				if (AutoPilot.Targeting.HasTarget()) {

					var targetDist = Vector3D.Distance(RemoteControl.GetPosition(), AutoPilot.Targeting.TargetLastKnownCoords);

					if (!_inRange) {

						if (targetDist < (AutoPilot.InGravity() ? EngageDistancePlanet : EngageDistanceSpace)) {

							_inRange = true;
							BehaviorTriggerE = true;
							AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.Strafe | NewAutoPilotMode.WaypointFromTarget | AutoPilot.UserCustomMode);

						}

					} else {

						if (targetDist < (AutoPilot.InGravity() ? DisengageDistancePlanet : DisengageDistanceSpace)) {

							_inRange = false;
							BehaviorTriggerF = true;
							AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget | AutoPilot.UserCustomMode);

						}

					}

				} else {

					BehaviorTriggerB = true;
					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);

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

		public void ReturnToDespawn() {

			if(Mode == BehaviorMode.ApproachTarget)
				BehaviorTriggerC = true;

			ChangeCoreBehaviorMode(BehaviorMode.ApproachWaypoint);
			AutoPilot.SetAutoPilotDataMode(AutoPilotDataMode.Primary);
			AutoPilot.ActivateAutoPilot(Settings.DespawnCoords, NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | AutoPilot.UserCustomMode);
			_checkActiveTargetTimer = MyAPIGateway.Session.GameDateTime;

		}

		public override void BehaviorInit(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Behavior Init For Hunter", DebugTypeEnum.BehaviorSetup);

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			Despawn.UseNoTargetTimer = false;
			AutoPilot.Weapons.UseStaticGuns = true;

			AutoPilot.AssignAutoPilotDataMode("RAI-Generic-Autopilot-Hunter-A", AutoPilotDataMode.Primary);
			AutoPilot.AssignAutoPilotDataMode("RAI-Generic-Autopilot-Hunter-B", AutoPilotDataMode.Secondary);

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

					//TimeBetweenNewTargetChecks
					if (tag.Contains("[TimeBetweenNewTargetChecks:")) {

						this.TimeBetweenNewTargetChecks = TagHelper.TagIntCheck(tag, this.TimeBetweenNewTargetChecks);

					}

					//LostTargetTimerTrigger
					if (tag.Contains("[LostTargetTimerTrigger:")) {

						this.LostTargetTimerTrigger = TagHelper.TagIntCheck(tag, this.LostTargetTimerTrigger);

					}

					//DistanceToCheckEngagableTarget
					if (tag.Contains("[DistanceToCheckEngagableTarget:")) {

						this.DistanceToCheckEngagableTarget = TagHelper.TagDoubleCheck(tag, this.DistanceToCheckEngagableTarget);

					}

					//EngageOnCameraDetection
					if (tag.Contains("[EngageOnCameraDetection:")) {

						this.EngageOnCameraDetection = TagHelper.TagBoolCheck(tag);

					}

					//EngageOnWeaponActivation
					if (tag.Contains("[EngageOnWeaponActivation:")) {

						this.EngageOnWeaponActivation = TagHelper.TagBoolCheck(tag);

					}

					//EngageOnTargetLineOfSight
					if (tag.Contains("[EngageOnTargetLineOfSight:")) {

						this.EngageOnTargetLineOfSight = TagHelper.TagBoolCheck(tag);

					}

					//EngageDistanceSpace
					if (tag.Contains("[EngageDistanceSpace:")) {

						this.EngageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.EngageDistanceSpace);

					}

					//EngageDistancePlanet
					if (tag.Contains("[EngageDistancePlanet:")) {

						this.EngageDistancePlanet = TagHelper.TagDoubleCheck(tag, this.EngageDistancePlanet);

					}


					//DisengageDistanceSpace
					if (tag.Contains("[DisengageDistanceSpace:")) {

						this.DisengageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.DisengageDistanceSpace);

					}


					//DisengageDistancePlanet
					if (tag.Contains("[DisengageDistancePlanet:")) {

						this.DisengageDistancePlanet = TagHelper.TagDoubleCheck(tag, this.DisengageDistancePlanet);

					}


					//CameraDetectionMaxRange
					if (tag.Contains("[CameraDetectionMaxRange:")) {

						this.CameraDetectionMaxRange = TagHelper.TagDoubleCheck(tag, this.CameraDetectionMaxRange);

					}


				}

			}

		}

	}

}
	
