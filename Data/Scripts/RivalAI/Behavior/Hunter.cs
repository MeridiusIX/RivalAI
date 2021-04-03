using RivalAI.Behavior.Subsystems.AutoPilot;
using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Behavior {

	public class Hunter : CoreBehavior, IBehavior{

		//Configurable
		public int TimeBetweenNewTargetChecks;
		public int LostTargetTimerTrigger;
		public double DistanceToCheckEngagableTarget;

		public bool EngageOnCameraDetection;
		public bool EngageOnWeaponActivation;
		public bool EngageOnTargetLineOfSight;

		public double CameraDetectionMaxRange;

		//Non-Config
		private DateTime _checkActiveTargetTimer;
		private DateTime _lostTargetTimer;

		private bool _inRange;
		
		public Hunter() : base() {

			_behaviorType = "Hunter";

			TimeBetweenNewTargetChecks = 15;
			LostTargetTimerTrigger = 30;
			DistanceToCheckEngagableTarget = 1200;

			EngageOnCameraDetection = false;
			EngageOnWeaponActivation = false;
			EngageOnTargetLineOfSight = false;

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

			base.MainBehavior();

			//Logger.MsgDebug(Mode.ToString(), DebugTypeEnum.General);

			if (Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true){

				ChangeCoreBehaviorMode(BehaviorMode.Retreat);
				AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing, CheckEnum.Yes, CheckEnum.No);

			}
			
			if(Mode == BehaviorMode.Init) {

				if (Settings.DespawnCoords == Vector3D.Zero) {

					Settings.DespawnCoords = MESApi.GetDespawnCoords(RemoteControl.SlimBlock.CubeGrid);

					if (Settings.DespawnCoords == Vector3D.Zero)
						Settings.DespawnCoords = AutoPilot.CalculateDespawnCoords(this.RemoteControl.GetPosition());

				}

				ReturnToDespawn();

			}

			if (BehaviorActionA && Mode != BehaviorMode.EngageTarget) {

				//Logger.MsgDebug("Hunter BehaviorActionA Triggered", DebugTypeEnum.General);

				BehaviorActionA = false;

				if (Settings.LastDamagerEntity != 0) {

					//Logger.MsgDebug("Damager Entity Id Valid" + Settings.LastDamagerEntity.ToString(), DebugTypeEnum.General);

					IMyEntity tempEntity = null;

					if (MyAPIGateway.Entities.TryGetEntityById(Settings.LastDamagerEntity, out tempEntity)) {

						//Logger.MsgDebug("Damager Entity Valid", DebugTypeEnum.General);

						var parentEnt = tempEntity.GetTopMostParent();

						if (parentEnt != null) {

							//Logger.MsgDebug("Damager Parent Entity Valid", DebugTypeEnum.General);
							var gridGroup = MyAPIGateway.GridGroups.GetGroup(RemoteControl.SlimBlock.CubeGrid, GridLinkTypeEnum.Physical);
							bool isSameGridConstrust = false;

							foreach (var grid in gridGroup) {

								if (grid.EntityId == tempEntity.GetTopMostParent().EntityId) {

									//Logger.MsgDebug("Damager Parent Entity Was Same Grid", DebugTypeEnum.General);
									isSameGridConstrust = true;
									break;

								}

							}

							if (!isSameGridConstrust) {

								//Logger.MsgDebug("Damager Parent Entity Was External", DebugTypeEnum.General);
								AutoPilot.Targeting.ForceTargetEntityId = parentEnt.EntityId;
								AutoPilot.Targeting.ForceTargetEntity = parentEnt;
								AutoPilot.Targeting.ForceRefresh = true;
								AutoPilot.SetAutoPilotDataMode(AutoPilotDataMode.Secondary);
								AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget, CheckEnum.Yes, CheckEnum.No);
								ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
								Logger.MsgDebug("Hunter Approaching Potential Target From Damage", DebugTypeEnum.BehaviorSpecific);
								return;

							}

						}

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
						AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget, CheckEnum.Yes, CheckEnum.No);
						Logger.MsgDebug("Hunter Approaching Potential Target", DebugTypeEnum.BehaviorSpecific);

					}
					
				}

				if (!BehaviorTriggerA) {

					if (Vector3D.Distance(RemoteControl.GetPosition(), Settings.DespawnCoords) <= MathTools.Hypotenuse(AutoPilot.Data.WaypointTolerance, AutoPilot.Data.WaypointTolerance)) {

						Despawn.DoDespawn = true;
					
					}
				
				}

			}

			if (Mode == BehaviorMode.ApproachTarget) {

				if (!AutoPilot.Targeting.HasTarget()) {

					AutoPilot.SetInitialWaypoint(AutoPilot.Targeting.TargetLastKnownCoords);
					var time = MyAPIGateway.Session.GameDateTime - _lostTargetTimer;

					if (time.TotalSeconds > LostTargetTimerTrigger) {

						Logger.MsgDebug("Hunter Returning To Despawn", DebugTypeEnum.BehaviorSpecific);
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

					if (AutoPilot.Weapons.GetTurretTarget() != 0) {

						Logger.MsgDebug("Hunter Turrets Detected Target", DebugTypeEnum.BehaviorSpecific);
						engageTarget = true;

					}
						


				}

				//Check Visual Range
				if (!engageTarget && EngageOnCameraDetection && targetDist < CameraDetectionMaxRange) {

					if (Grid.RaycastGridCheck(AutoPilot.Targeting.TargetLastKnownCoords)) {

						Logger.MsgDebug("Hunter Raycast Target Success", DebugTypeEnum.BehaviorSpecific);

					}
						engageTarget = true;

				}

				//Check Collision Data
				if (!engageTarget && EngageOnTargetLineOfSight && AutoPilot.Targeting.Data.MaxLineOfSight > 0 && AutoPilot.Collision.TargetResult.HasTarget(AutoPilot.Targeting.Data.MaxLineOfSight)) {

					if (AutoPilot.Targeting.Target.GetParentEntity().EntityId == AutoPilot.Collision.TargetResult.GetCollisionEntity().EntityId) {

						Logger.MsgDebug("Hunter Has Line of Sight to Target", DebugTypeEnum.BehaviorSpecific);
						engageTarget = true;

					}
						

				}

				if (engageTarget) {

					Logger.MsgDebug("Hunter Engaging Target", DebugTypeEnum.BehaviorSpecific);
					BehaviorTriggerD = true;
					ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);

				}

			}

			//Engage
			if (Mode == BehaviorMode.EngageTarget) {

				if (AutoPilot.Targeting.HasTarget()) {

					var targetDist = Vector3D.Distance(RemoteControl.GetPosition(), AutoPilot.Targeting.TargetLastKnownCoords);

					if (!_inRange) {

						if (targetDist < (AutoPilot.InGravity() ? AutoPilot.Data.EngageDistancePlanet : AutoPilot.Data.EngageDistanceSpace)) {

							Logger.MsgDebug("Hunter Within Engage Range", DebugTypeEnum.BehaviorSpecific);
							_inRange = true;
							BehaviorTriggerE = true;
							AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.Strafe | NewAutoPilotMode.WaypointFromTarget, CheckEnum.Yes, CheckEnum.No);

						}

					} else {

						if (targetDist > (AutoPilot.InGravity() ? AutoPilot.Data.DisengageDistancePlanet : AutoPilot.Data.DisengageDistanceSpace)) {

							Logger.MsgDebug("Hunter Outside Engage Range", DebugTypeEnum.BehaviorSpecific);
							_inRange = false;
							BehaviorTriggerF = true;
							AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget, CheckEnum.Yes, CheckEnum.No);

						}

					}

				} else {

					Logger.MsgDebug("Hunter Lost Target While Engaging", DebugTypeEnum.BehaviorSpecific);
					BehaviorTriggerB = true;
					BehaviorTriggerF = true;
					_inRange = false;
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget, CheckEnum.Yes, CheckEnum.No);
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
			AutoPilot.ActivateAutoPilot(Settings.DespawnCoords, NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing, CheckEnum.Yes, CheckEnum.No);
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

					//CameraDetectionMaxRange
					if (tag.Contains("[CameraDetectionMaxRange:")) {

						this.CameraDetectionMaxRange = TagHelper.TagDoubleCheck(tag, this.CameraDetectionMaxRange);

					}


				}

			}

		}

	}

}
	
