using Sandbox.ModAPI;
using VRageMath;
using RivalAI.Helpers;
using RivalAI.Behavior.Subsystems.AutoPilot;
using System.Collections.Generic;
using RivalAI.Behavior.Subsystems.Trigger;

namespace RivalAI.Behavior {

	public class CargoShip : CoreBehavior, IBehavior {

		public List<Vector3D> CustomWaypoints;
		private bool _waypointIsDespawn = true;

		private Vector3D _lastCoords = Vector3D.Zero;

		private EncounterWaypoint _cargoShipWaypoint { 
			
			get {

				if (AutoPilot.State.CargoShipWaypoints.Count > 0) {

					if (_waypointIsDespawn) {

						Logger.MsgDebug("CargoShip Switching To A Non-Despawn Waypoint", DebugTypeEnum.General);
						_waypointIsDespawn = false;
						BehaviorTriggerC = true;

					}

					return AutoPilot.State.CargoShipWaypoints[0];

				}

				if (!_waypointIsDespawn) {

					Logger.MsgDebug("CargoShip Switching To A Despawn Waypoint", DebugTypeEnum.General);
					_waypointIsDespawn = true;
					BehaviorTriggerD = true;


				}

				return AutoPilot.State.CargoShipDespawn;

			} 
		
		}

		public CargoShip() : base() {

			_behaviorType = "CargoShip";
			_waypointIsDespawn = false;

			CustomWaypoints = new List<Vector3D>();

		}

		public override void MainBehavior() {

			if(RAI_SessionCore.IsServer == false) {

				return;

			}

			Logger.MsgDebug(Mode.ToString(), DebugTypeEnum.General);
			
			if(Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true){

				ChangeCoreBehaviorMode(BehaviorMode.Retreat);
				AutoPilot.ActivateAutoPilot(_cargoShipWaypoint.GetCoords(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | AutoPilot.UserCustomMode);

			}

			bool _firstRun = false;

			//Init
			if (Mode == BehaviorMode.Init) {

				foreach (var waypoint in CustomWaypoints) {

					AutoPilot.State.CargoShipWaypoints.Add(new EncounterWaypoint(waypoint));
				
				}

				SelectNextWaypoint();
				ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
				_firstRun = true;

			}

			//WaitAtWaypoint
			if (Mode == BehaviorMode.WaitAtWaypoint) {

				var timeSpan = MyAPIGateway.Session.GameDateTime - AutoPilot.State.WaypointWaitTime;

				if (timeSpan.TotalSeconds >= AutoPilot.Data.WaypointWaitTimeTrigger) {

					SelectNextWaypoint();
					AutoPilot.ActivateAutoPilot(_cargoShipWaypoint.GetCoords(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | AutoPilot.UserCustomMode);
					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);

					if (!_firstRun)
						BehaviorTriggerB = true;
					else
						_firstRun = false;

				}

			}

			//Approach
			if (Mode == BehaviorMode.ApproachTarget) {

				if (_cargoShipWaypoint == null) {

					AutoPilot.ActivateAutoPilot(_cargoShipWaypoint.GetCoords(), AutoPilot.UserCustomModeIdle);
					ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
					return;

				}

				var coords = _cargoShipWaypoint.GetCoords();

				if (!_cargoShipWaypoint.Valid || _cargoShipWaypoint.ReachedWaypoint) {

					AutoPilot.ActivateAutoPilot(_cargoShipWaypoint.GetCoords(), AutoPilot.UserCustomModeIdle);
					ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
					return;

				}

				if (!_waypointIsDespawn && _lastCoords != coords) {

					AutoPilot.SetInitialWaypoint(coords);
					_lastCoords = coords;

				}

				if (Vector3D.Distance(RemoteControl.GetPosition(), AutoPilot.State.InitialWaypoint) < MathTools.Hypotenuse(AutoPilot.Data.WaypointTolerance, AutoPilot.Data.WaypointTolerance)) {

					_cargoShipWaypoint.ReachedWaypoint = true;
					_cargoShipWaypoint.ReachedWaypointTime = MyAPIGateway.Session.GameDateTime;
					AutoPilot.State.WaypointWaitTime = MyAPIGateway.Session.GameDateTime;

					if (_waypointIsDespawn) {

						if (Despawn.NearestPlayer == null || Despawn.PlayerDistance > 1200) {

							Despawn.DoDespawn = true;
						
						}

						ChangeCoreBehaviorMode(BehaviorMode.Retreat);
						Despawn.DoRetreat = true;

					} else {

						AutoPilot.ActivateAutoPilot(_cargoShipWaypoint.GetCoords(), AutoPilot.UserCustomModeIdle);
						ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
						BehaviorTriggerA = true;

					}
				
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

		private void SelectNextWaypoint() {

			if (!AutoPilot.State.CargoShipDespawn.Valid) {

				Logger.MsgDebug("Setting Initial CargoShip Despawn Waypoint", DebugTypeEnum.General);
				var despawnCoords = MESApi.GetDespawnCoords(RemoteControl.SlimBlock.CubeGrid);

				if (despawnCoords == Vector3D.Zero) {

					Logger.MsgDebug("Could Not Get From MES. Creating Manual Despawn Waypoint", DebugTypeEnum.General);
					despawnCoords = AutoPilot.CalculateDespawnCoords(this.RemoteControl.GetPosition());

				}

				AutoPilot.State.CargoShipDespawn = new EncounterWaypoint(despawnCoords);

			}

			while (true) {

				if (AutoPilot.State.CargoShipWaypoints.Count == 0)
					break;

				var waypoint = AutoPilot.State.CargoShipWaypoints[0];
				waypoint.GetCoords();

				if (waypoint == null || !waypoint.Valid || waypoint.ReachedWaypoint) {

					Logger.MsgDebug("Invalid or Reached Waypoint Has Been Removed", DebugTypeEnum.General);
					AutoPilot.State.CargoShipWaypoints.RemoveAt(0);
					continue;

				}

				break;

			}

		}

		public override void BehaviorInit(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Behavior Init For CargoShip", DebugTypeEnum.General);

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			AutoPilot.Data = TagHelper.GetAutopilotProfile("RAI-Generic-Autopilot-CargoShip");
			Despawn.UseNoTargetTimer = false;
			AutoPilot.Weapons.UseStaticGuns = false;
			AutoPilot.Data.DisableInertiaDampeners = false;
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

					//CustomWaypoints
					if (tag.Contains("[CustomWaypoints:") == true) {

						var tempValue = TagHelper.TagVector3DCheck(tag);

						if (tempValue != Vector3D.Zero)
							this.CustomWaypoints.Add(tempValue);

					}	

				}
				
			}

		}

	}

}
	
