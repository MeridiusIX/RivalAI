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

namespace RivalAI.Behavior.Subsystems{
	
	

	public class AutoPilotSystem{

		//Configurable
		public bool AutoPilotUseSafePlanetPathing;
		public bool AutoPilotCollisionAvoidance;
		public bool AutoPilotPrecisionMode;
		public Base6Directions.Direction AutoPilotForwardDirection;
		public double SpaceTargetPaddingDistance;
		public float DesiredMaxSpeed;
		public double MinimumAltitudeAboveTarget;
		public double PlanetSafePathingCheckDistance;
		public int EvasionModeMaxTime;

		public double EngageThrustWithinAngle;

		public int BarrelRollMinTimeMs;
		public int BarrelRollMaxTimeMs;

		public IMyRemoteControl RemoteControl;

		private CollisionSystem Collision;
		private RotationSystem Rotation;
		private TargetingSystem Targeting;
		private ThrustSystem Thrust;
		private WeaponsSystem Weapons;

		public AutoPilotMode Mode;
		public AutoPilotMode PreviousMode;
		public AutoPilotMode RevertMode;
	
		public bool WaypointChanged;
		public Vector3D WaypointCoords;
		public Vector3D PlanetSafeWaypointCoords;
		public Vector3D TargetCoords;
		public Vector3D UpDirection;
		public MyPlanet Planet;
		public Vector3D PlanetCore;

		public int EvasionModeTimer;
		public int BarrelRollTimer;

		public bool AutoPilotCorrectionEnabled;
		public DateTime AutoPilotCorrectionTimer;
		public DateTime AutoPilotCorrectionCooldownTimer;

		public DateTime BarrelRollLastActivation;
		
		public bool CollisionDetected;

		public Random Rnd;
		
		public AutoPilotSystem(IMyRemoteControl remoteControl = null) {

			AutoPilotUseSafePlanetPathing = true;
			AutoPilotCollisionAvoidance = true;
			AutoPilotPrecisionMode = false;
			AutoPilotForwardDirection = Base6Directions.Direction.Forward;
			SpaceTargetPaddingDistance = 100;
			DesiredMaxSpeed = 100;
			MinimumAltitudeAboveTarget = 200;
			PlanetSafePathingCheckDistance = 1000;
			EvasionModeMaxTime = 6;

			EngageThrustWithinAngle = 90;

			BarrelRollMinTimeMs = 2000;
			BarrelRollMaxTimeMs = 3000;

			RemoteControl = null;

			/*
			Collision = new CollisionSystem(remoteControl);
			Rotation = new RotationSystem(remoteControl);
			Targeting = new TargetingSystem(remoteControl);
			Thrust = new ThrustSystem(remoteControl);
			Weapons = new WeaponsSystem(remoteControl);
			*/

			Mode = AutoPilotMode.None;
			PreviousMode = AutoPilotMode.None;
			RevertMode = AutoPilotMode.None;

			WaypointChanged = false;
			WaypointCoords = Vector3D.Zero;
			PlanetSafeWaypointCoords = Vector3D.Zero;
			TargetCoords = Vector3D.Zero;
			UpDirection = Vector3D.Zero;
			Planet = null;
			PlanetCore = Vector3D.Zero;

			EvasionModeTimer = 0;
			BarrelRollTimer = 0;

			AutoPilotCorrectionEnabled = false;
			AutoPilotCorrectionTimer = MyAPIGateway.Session.GameDateTime;
			AutoPilotCorrectionCooldownTimer = MyAPIGateway.Session.GameDateTime;

			BarrelRollLastActivation = MyAPIGateway.Session.GameDateTime;

			CollisionDetected = false;

			Rnd = new Random();

			Setup(remoteControl);


		}

		public void ChangeAutoPilotMode(AutoPilotMode newMode) {

			//Handle Previous Mode First
			if(this.Mode == AutoPilotMode.LegacyAutoPilotTarget || this.Mode == AutoPilotMode.LegacyAutoPilotWaypoint) {

				SetRemoteControl(this.RemoteControl, false, Vector3D.Zero);

			}

			if(this.Mode == AutoPilotMode.BarrelRoll || this.Mode == AutoPilotMode.FlyToTarget || this.Mode == AutoPilotMode.FlyToWaypoint || this.Mode == AutoPilotMode.RotateToTarget || this.Mode == AutoPilotMode.RotateToTargetAndStrafe || this.Mode == AutoPilotMode.RotateToWaypoint) {

				Rotation.StopAllRotation();

			}

			if(this.Mode == AutoPilotMode.FlyToTarget || this.Mode == AutoPilotMode.FlyToWaypoint || this.Mode == AutoPilotMode.RotateToTargetAndStrafe) {

				Thrust.StopAllThrust();

			}

			if(newMode == AutoPilotMode.BarrelRoll) {

				BarrelRollTimer = Rnd.Next(this.BarrelRollMinTimeMs, this.BarrelRollMaxTimeMs);
				BarrelRollLastActivation = MyAPIGateway.Session.GameDateTime; //

			}

			Logger.AddMsg("Autopilot Mode Changed To: " + newMode.ToString(), true);
			this.RevertMode = this.Mode;
			this.Mode = newMode;
			EngageAutoPilot();

		}

		public void EngageAutoPilot() {

			this.UpDirection = VectorHelper.GetPlanetUpDirection(this.RemoteControl.GetPosition());

			if(this.UpDirection != Vector3D.Zero && this.Planet == null) {

				this.Planet = MyGamePruningStructure.GetClosestPlanet(this.RemoteControl.GetPosition());
				this.PlanetCore = this.Planet.PositionComp.GetPosition();

			} else if(this.UpDirection == Vector3D.Zero && this.Planet != null) {

				this.Planet = null;
				this.PlanetCore = Vector3D.Zero;

			}

			if(Thrust.Mode == ThrustMode.Strafe) {

				if(this.UpDirection == Vector3D.Zero) {

					Thrust.CurrentAllowedStrafeDirections = Thrust.AllowedStrafingDirectionsSpace;

				} else {

					Thrust.CurrentAllowedStrafeDirections = Thrust.AllowedStrafingDirectionsPlanet;

				}

			}

			if(this.Mode != this.PreviousMode) {

				if(this.PreviousMode == AutoPilotMode.LegacyAutoPilotTarget || this.PreviousMode == AutoPilotMode.LegacyAutoPilotWaypoint) {

					SetRemoteControl(this.RemoteControl, false, Vector3D.Zero);

				}

				if(this.Mode == AutoPilotMode.BarrelRoll || this.Mode == AutoPilotMode.FlyToTarget || this.Mode == AutoPilotMode.FlyToWaypoint || this.Mode == AutoPilotMode.RotateToTarget || this.Mode == AutoPilotMode.RotateToTargetAndStrafe || this.Mode == AutoPilotMode.RotateToWaypoint) {

					Rotation.StopAllRotation();

				}

				if(this.Mode == AutoPilotMode.FlyToTarget || this.Mode == AutoPilotMode.FlyToWaypoint || this.Mode == AutoPilotMode.RotateToTargetAndStrafe) {

					Thrust.StopAllThrust();

				}

				this.PreviousMode = this.Mode;

			}

			if(this.Mode == AutoPilotMode.None) {

				return;

			}

			if(this.Mode == AutoPilotMode.LegacyAutoPilotTarget) {

				if(AutoPilotCorrection() == false) {

					return;

				}

				if(this.WaypointCoords != this.TargetCoords) {

					this.WaypointCoords = this.TargetCoords;
					this.WaypointChanged = true;

					if(this.UpDirection != Vector3D.Zero && this.AutoPilotUseSafePlanetPathing == true) {

						this.WaypointCoords = VectorHelper.GetPlanetWaypointPathing(this.RemoteControl.GetPosition(), this.WaypointCoords, this.MinimumAltitudeAboveTarget);

					} else {

						this.WaypointCoords = VectorHelper.CreateDirectionAndTarget(this.WaypointCoords, this.RemoteControl.GetPosition(), this.WaypointCoords, this.SpaceTargetPaddingDistance);

					}

				}

				if(this.WaypointChanged == true) {

					this.WaypointChanged = false;
					SetRemoteControl(this.RemoteControl, true, this.WaypointCoords);

				}

				return;

			}

			if(this.Mode == AutoPilotMode.LegacyAutoPilotWaypoint) {

				if(AutoPilotCorrection() == false) {

					return;

				}

				bool planetPathUsed = false;

				if(this.UpDirection != Vector3D.Zero && this.AutoPilotUseSafePlanetPathing == true) {

					planetPathUsed = true;
					this.WaypointChanged = true;
					this.PlanetSafeWaypointCoords = VectorHelper.GetPlanetWaypointPathing(this.RemoteControl.GetPosition(), this.WaypointCoords, this.MinimumAltitudeAboveTarget);

				}

				if(this.WaypointChanged == true) {

					this.WaypointChanged = false;

					if(planetPathUsed == false) {

						SetRemoteControl(this.RemoteControl, true, this.WaypointCoords);

					} else {

						SetRemoteControl(this.RemoteControl, true, this.PlanetSafeWaypointCoords);

					}
					

				}

				return;

			}

			if(this.Mode == AutoPilotMode.BarrelRoll) {

				Rotation.BarrelRollEnabled = true;
				Rotation.StartCalculation(this.TargetCoords, this.RemoteControl, this.UpDirection);
				TimeSpan duration = MyAPIGateway.Session.GameDateTime - this.BarrelRollLastActivation;

				if(duration.TotalMilliseconds >= this.BarrelRollTimer) {

					ChangeAutoPilotMode(this.RevertMode);

				}

				return;

			}

			if(this.Mode == AutoPilotMode.RotateToTarget) {

				Rotation.StartCalculation(this.TargetCoords, this.RemoteControl, this.UpDirection);
				return;

			}

			if(this.Mode == AutoPilotMode.RotateToTargetAndStrafe) {

				//RemoteControl.SlimBlock.CubeGrid.Physics.AngularVelocity = RemoteControl.WorldMatrix.Forward;
				Rotation.StartCalculation(this.TargetCoords, this.RemoteControl, this.UpDirection);
				Thrust.ChangeMode(ThrustMode.Strafe);

				if(this.Collision.VelocityResult.CollisionImminent == true) {

					Thrust.InvertStrafe(this.Collision.VelocityResult.Coords);

				}

				Thrust.ProcessThrust(this.UpDirection, this.TargetCoords, this.MinimumAltitudeAboveTarget, this.SpaceTargetPaddingDistance);
				return;

			}

			if(this.Mode == AutoPilotMode.RotateToWaypoint) {

				Rotation.StartCalculation(this.WaypointCoords, this.RemoteControl, this.UpDirection);
				return;

			}

			if(this.Mode == AutoPilotMode.FlyToTarget) {

				Rotation.StartCalculation(this.TargetCoords, this.RemoteControl, this.UpDirection);

				if(VectorHelper.GetAngleBetweenDirections(this.RemoteControl.WorldMatrix.Forward, Vector3D.Normalize(this.TargetCoords - this.RemoteControl.GetPosition())) <= this.EngageThrustWithinAngle) {

					this.Thrust.CurrentAllowedThrust = new Vector3I(0,0,1);
					this.Thrust.CurrentRequiredThrust = new Vector3I(0, 0, 1);

				} else {

					this.Thrust.CurrentAllowedThrust = Vector3I.Zero;
					this.Thrust.CurrentRequiredThrust = Vector3I.Zero;

				}

				Thrust.ChangeMode(ThrustMode.ConstantForward);
				Thrust.ProcessThrust(this.UpDirection);

				return;

			}

			if(this.Mode == AutoPilotMode.FlyToWaypoint) {

				Rotation.StartCalculation(this.WaypointCoords, this.RemoteControl, this.UpDirection);

				if(VectorHelper.GetAngleBetweenDirections(this.RemoteControl.WorldMatrix.Forward, Vector3D.Normalize(this.WaypointCoords - this.RemoteControl.GetPosition())) <= this.EngageThrustWithinAngle) {

					this.Thrust.CurrentAllowedThrust = new Vector3I(0, 0, 1);
					this.Thrust.CurrentRequiredThrust = new Vector3I(0, 0, 1);

				} else {

					this.Thrust.CurrentAllowedThrust = Vector3I.Zero;
					this.Thrust.CurrentRequiredThrust = Vector3I.Zero;

				}

				Thrust.ChangeMode(ThrustMode.ConstantForward);
				Thrust.ProcessThrust(this.UpDirection);

				return;

			}

		}

		public bool AutoPilotCorrection() {

			if(this.UpDirection == Vector3D.Zero || this.RemoteControl?.SlimBlock?.CubeGrid?.Physics == null) {

				return true;

			}

			if(this.AutoPilotCorrectionEnabled == false) {

				TimeSpan duration = MyAPIGateway.Session.GameDateTime - this.AutoPilotCorrectionCooldownTimer;

				if(duration.TotalSeconds >= 5) {

					Vector3D velocity = this.RemoteControl.SlimBlock.CubeGrid.Physics.LinearVelocity;
					//Logger.AddMsg(string.Format("Velocity {0}", velocity.Length()), true);
					//Logger.AddMsg(string.Format("Angle {0}", VectorHelper.GetAngleBetweenDirections(this.UpDirection, Vector3D.Normalize(velocity))), true);

					if(VectorHelper.GetAngleBetweenDirections(this.UpDirection, Vector3D.Normalize(velocity)) <= 1 && velocity.Length() < 7) {

						this.AutoPilotCorrectionTimer = MyAPIGateway.Session.GameDateTime;
						this.AutoPilotCorrectionEnabled = true;
						SetRemoteControl(this.RemoteControl, false, Vector3D.Zero);
						return false;

					}

				}

			} else {

				TimeSpan duration = MyAPIGateway.Session.GameDateTime - this.AutoPilotCorrectionTimer;

				if(duration.TotalSeconds > 2) {

					this.AutoPilotCorrectionCooldownTimer = MyAPIGateway.Session.GameDateTime;
					this.AutoPilotCorrectionEnabled = false;
					SetRemoteControl(this.RemoteControl, true, this.WaypointCoords);

				}

				return false;

			}

			return true;

		}

		public void InitTags() {

			if(string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach(var tag in descSplit) {

					//AutoPilotUseSafePlanetPathing
					if(tag.Contains("[AutoPilotUseSafePlanetPathing:") == true) {

						this.AutoPilotUseSafePlanetPathing = TagHelper.TagBoolCheck(tag);

					}

					//AutoPilotCollisionAvoidance
					if(tag.Contains("[AutoPilotCollisionAvoidance:") == true) {

						this.AutoPilotCollisionAvoidance = TagHelper.TagBoolCheck(tag);

					}

					//AutoPilotPrecisionMode
					if(tag.Contains("[AutoPilotPrecisionMode:") == true) {

						this.AutoPilotPrecisionMode = TagHelper.TagBoolCheck(tag);

					}

					//AutoPilotForwardDirection
					if(tag.Contains("[AutoPilotForwardDirection:") == true) {

						this.AutoPilotForwardDirection = TagHelper.TagBase6DirectionCheck(tag);

					}

					//DesiredMaxSpeed
					if(tag.Contains("[DesiredMaxSpeed:") == true) {

						this.DesiredMaxSpeed = TagHelper.TagFloatCheck(tag, this.DesiredMaxSpeed);

					}

					//MinimumAltitudeAboveTarget
					if(tag.Contains("[MinimumAltitudeAboveTarget:") == true) {

						this.MinimumAltitudeAboveTarget = TagHelper.TagDoubleCheck(tag, this.MinimumAltitudeAboveTarget);

					}

					//PlanetSafePathingCheckDistance
					if(tag.Contains("[PlanetSafePathingCheckDistance:") == true) {

						this.PlanetSafePathingCheckDistance = TagHelper.TagDoubleCheck(tag, this.PlanetSafePathingCheckDistance);

					}

					//SpaceTargetPaddingDistance
					if(tag.Contains("[SpaceTargetPaddingDistance:") == true) {

						this.SpaceTargetPaddingDistance = TagHelper.TagDoubleCheck(tag, this.SpaceTargetPaddingDistance);

					}

					//EvasionModeMaxTime
					if(tag.Contains("[EvasionModeMaxTime:") == true) {

						this.EvasionModeMaxTime = TagHelper.TagIntCheck(tag, this.EvasionModeMaxTime);

					}
			
			//Rotation Settings
			
					//RotationMultiplier
					if(tag.Contains("[RotationMultiplier:") == true) {

						this.Rotation.RotationMultiplier = TagHelper.TagFloatCheck(tag, this.Rotation.RotationMultiplier);

					}

					//Thrust Settings

					//EngageThrustWithinAngle
					if (tag.Contains("[EngageThrustWithinAngle:") == true) {

						this.EngageThrustWithinAngle = TagHelper.TagDoubleCheck(tag, this.EngageThrustWithinAngle);

					}

					//AllowStrafing
					if (tag.Contains("[AllowStrafing:") == true) {

						this.Thrust.AllowStrafing = TagHelper.TagBoolCheck(tag);

					}

					//StrafeMinDurationMs
					if(tag.Contains("[StrafeMinDurationMs:") == true) {

						this.Thrust.StrafeMinDurationMs = TagHelper.TagIntCheck(tag, this.Thrust.StrafeMinDurationMs);

					}

					//StrafeMaxDurationMs
					if(tag.Contains("[StrafeMaxDurationMs:") == true) {

						this.Thrust.StrafeMaxDurationMs = TagHelper.TagIntCheck(tag, this.Thrust.StrafeMaxDurationMs);

					}

					//StrafeMinCooldownMs
					if(tag.Contains("[StrafeMinCooldownMs:") == true) {

						this.Thrust.StrafeMinCooldownMs = TagHelper.TagIntCheck(tag, this.Thrust.StrafeMinCooldownMs);

					}

					//StrafeMaxCooldownMs
					if(tag.Contains("[StrafeMaxCooldownMs:") == true) {

						this.Thrust.StrafeMaxCooldownMs = TagHelper.TagIntCheck(tag, this.Thrust.StrafeMaxCooldownMs);

					}

					//StrafeSpeedCutOff
					if(tag.Contains("[StrafeSpeedCutOff:") == true) {

						this.Thrust.StrafeSpeedCutOff = TagHelper.TagDoubleCheck(tag, this.Thrust.StrafeSpeedCutOff);

					}

					//StrafeDistanceCutOff
					if(tag.Contains("[StrafeDistanceCutOff:") == true) {

						this.Thrust.StrafeDistanceCutOff = TagHelper.TagDoubleCheck(tag, this.Thrust.StrafeDistanceCutOff);

					}

				}

			}

		}

		public void UpdateWaypoint(Vector3D coords) {

			this.WaypointCoords = coords;
			this.WaypointChanged = true;

		}
		
		private void Setup(IMyRemoteControl remoteControl){
			
			if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false){

				return;
				
			}
			
			this.RemoteControl = remoteControl;

		}

		public void SetupReferences(CollisionSystem collision, RotationSystem rotation, TargetingSystem targeting, ThrustSystem thrust, WeaponsSystem weapons) {

			this.Collision = collision;
			this.Rotation = rotation;
			this.Targeting = targeting;
			this.Thrust = thrust;
			this.Weapons = weapons;

		}
		
		//SetRemoteControl
		public void SetRemoteControl(IMyRemoteControl remoteControl, bool enabled, Vector3D targetCoords, float speedLimit = 100, bool collisionAvoidance = false, bool precisionMode = false, Sandbox.ModAPI.Ingame.FlightMode flightMode = Sandbox.ModAPI.Ingame.FlightMode.OneWay, Base6Directions.Direction direction = Base6Directions.Direction.Forward){
			
			if(remoteControl == null){
				
				return;
				
			}
			
			if(enabled == false){
				
				remoteControl.SetAutoPilotEnabled(enabled);
				return;
				
			}
			
			remoteControl.ClearWaypoints();
			remoteControl.AddWaypoint(targetCoords, "TargetCoords");
			remoteControl.SpeedLimit = speedLimit;
			remoteControl.SetCollisionAvoidance(collisionAvoidance);
			remoteControl.SetDockingMode(precisionMode);
			remoteControl.FlightMode = flightMode;
			remoteControl.Direction = direction;
			remoteControl.SetAutoPilotEnabled(enabled);
			
		}

		public void ProcessEvasionCounter(bool reset = false) {

			if(reset == true) {

				this.EvasionModeTimer = 0;
				return;

			}

			if(this.EvasionModeTimer < this.EvasionModeMaxTime) {

				this.EvasionModeTimer++;

			}

		}
		
	}
	
}
