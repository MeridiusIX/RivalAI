using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRageMath;

namespace RivalAI.Behavior.Subsystems.AutoPilot {

	//New Thrust System
	public partial class AutoPilotSystem {

		



		public List<ThrusterProfile> ThrustProfiles;
		public Random Rnd;

		public bool Strafing;
		public Vector3I CurrentStrafeDirections;
		public Vector3I CurrentAllowedStrafeDirections;
		public bool InvertStrafingActivated;
		public int ThisStrafeDuration;
		public int ThisStrafeCooldown;
		public DateTime LastStrafeStartTime;
		public DateTime LastStrafeEndTime;
		public Vector3D _strafeStartPosition;

		private bool _collisionStrafeAdjusted;
		private bool _minAngleDistanceStrafeAdjusted;
		private Vector3D _collisionStrafeDirection;

		private ThrustAction _thrustToApply;


		private MyBlockOrientation _orientation {

			get {

				if (_referenceOrientation != _behavior.Settings.BlockOrientation) {

					_referenceOrientation = _behavior.Settings.BlockOrientation;
					SetBaseDirections(_referenceOrientation);

				}

				return _referenceOrientation;

			}

		}
		private SerializableBlockOrientation _referenceOrientation;


		public void ProcessThrustParallel(bool hasWaypoint) {

			if (!hasWaypoint) {

				StopAllThrust();
				return;

			}

			if (CurrentMode.HasFlag(NewAutoPilotMode.ThrustForward)) {

				if (InGravity() && CurrentMode.HasFlag(NewAutoPilotMode.LevelWithGravity)) {

					CalculateHoverThrust();

				} else {

					CalculateDirectForwardThrust();

				}

			}

			if (this.Data.AllowStrafing && CurrentMode.HasFlag(NewAutoPilotMode.Strafe)) {

				CalculateStrafeThrust();

			}

		}

		public void CalculateDirectForwardThrust() {

			if (CurrentMode.HasFlag(NewAutoPilotMode.Strafe) || _remoteControl?.SlimBlock?.CubeGrid?.Physics == null) {

				_debugThrustForwardMode = "Strafing Still Active";
				return;

			}

			if (this.AngleToCurrentWaypoint > this.Data.AngleAllowedForForwardThrust) {

				_debugThrustForwardMode = "Thrust Angle Not Matched";
				_thrustToApply.SetZ(false, false, 0, _orientation);
				return;

			}

			var velocityToTargetAngle = VectorHelper.GetAngleBetweenDirections(Vector3D.Normalize(GetCurrentWaypoint() - _remoteControl.WorldMatrix.Translation), Vector3D.Normalize(_remoteControl.SlimBlock.CubeGrid.Physics.LinearVelocity));
			var velocity = _remoteControl.SlimBlock.CubeGrid.Physics.LinearVelocity;
			var velocityAmount = velocity.Length();
			var stoppingDist = CalculateStoppingDistance(velocity, _forwardDir, Base6Directions.Direction.Forward);

			if (!IndirectWaypointType.HasFlag(WaypointModificationEnum.PlanetPathingAscend) && !IndirectWaypointType.HasFlag(WaypointModificationEnum.Collision)) {

				if ((stoppingDist >= this.DistanceToCurrentWaypoint && velocityAmount >= Data.IdealMinSpeed) || this.DistanceToCurrentWaypoint <= this.Data.WaypointTolerance) {

					_debugThrustForwardMode = string.Format("Current Dist / Stop Dist  :  {0} / {1}", Math.Round(this.DistanceToCurrentWaypoint), Math.Round(stoppingDist));
					_thrustToApply.SetZ(false, false, 0, _orientation);
					return;

				}

				if (this.DistanceToCurrentWaypoint <= this.Data.WaypointTolerance) {

					_debugThrustForwardMode = "Within Waypoint Tolerance Distance";
					_thrustToApply.SetZ(false, false, 0, _orientation);
					return;

				}

			}

			if (velocityToTargetAngle > this.Data.MaxVelocityAngleForSpeedControl || velocity.Length() <= 0.1) {

				_debugThrustForwardMode = "Velocity Angle Not Within Range";
				_thrustToApply.SetZ(true, false, 1, _orientation);
				return;

			}

			//Logger.MsgDebug(string.Format("Forward Thrust Check: {0} / {1}", velocityAmount, IdealMaxSpeed - MaxSpeedTolerance), DebugTypeEnum.Thrust);
			if (velocityAmount < Data.IdealMaxSpeed - Data.MaxSpeedTolerance) {

				_debugThrustForwardMode = "Thrust To Desired Speed";
				_thrustToApply.SetZ(true, false, 1, _orientation);
				return;

			}

			//Logger.MsgDebug(string.Format("Reverse Thrust Check: {0} / {1}", velocityAmount, IdealMaxSpeed + MaxSpeedTolerance), DebugTypeEnum.Thrust);
			if (velocityAmount > Data.IdealMaxSpeed + Data.MaxSpeedTolerance) {

				_debugThrustForwardMode = "Brake To Desired Speed";
				_thrustToApply.SetZ(true, true, 1, _orientation);
				return;

			}

			_debugThrustForwardMode = "Drifting At Desired Speed";
			_thrustToApply.SetZ(true, false, 0.0001f, _orientation);

		}

		public void CalculateHoverThrust() {

			var upAngle = VectorHelper.GetAngleBetweenDirections(_upDirection, RefBlockMatrixRotation.Up);
			var planetPos = CurrentPlanet.PositionComp.WorldAABB.Center;
			var velocity = _remoteControl.SlimBlock.CubeGrid.Physics.LinearVelocity;
			var velocityAmount = velocity.Length();
			var velocityToTargetAngle = VectorHelper.GetAngleBetweenDirections(Vector3D.Normalize(GetCurrentWaypoint() - _remoteControl.WorldMatrix.Translation), Vector3D.Normalize(velocity));

			//UP Axis
			//TODO: change magic 10 to configurable field
			if (upAngle <= 10) {

				var myDistToCore = Vector3D.Distance(_myPosition, planetPos);
				var targetDistCore = Vector3D.Distance(_currentWaypoint, planetPos);
				var altitudeDist = myDistToCore - targetDistCore;

				if (Math.Abs(altitudeDist) > this.Data.WaypointTolerance) {

					bool skipCheck = false;
					bool invertedDir = false;
					double stoppingDist = 0;
					double upDistance = 0;
					double upVelocityAmt = 0;

					if (altitudeDist < 0) {

						//UP
						stoppingDist = CalculateStoppingDistance(velocity, RefBlockMatrixRotation.Up, Base6Directions.Direction.Up);
						upDistance = Vector3D.Dot(_currentWaypoint - _myPosition, _upDirection);
						upVelocityAmt = Vector3D.Dot(velocity, RefBlockMatrixRotation.Up);
						invertedDir = false;
						

					} else {

						//DOWN
						stoppingDist = CalculateStoppingDistance(velocity, RefBlockMatrixRotation.Down, Base6Directions.Direction.Down);
						upDistance = -Vector3D.Dot(_currentWaypoint - _myPosition, _upDirection);
						upVelocityAmt = Vector3D.Dot(velocity, RefBlockMatrixRotation.Down);
						invertedDir = true;

					}

					/*
					Logger.MsgDebug("-------------------", DebugTypeEnum.Thrust);
					Logger.MsgDebug("Altitude Distance: " + altitudeDist.ToString(), DebugTypeEnum.Thrust);
					Logger.MsgDebug("Up Distance:       " + upDistance.ToString(), DebugTypeEnum.Thrust);
					Logger.MsgDebug("Stop Distance:     " + stoppingDist.ToString(), DebugTypeEnum.Thrust);
					Logger.MsgDebug("-------------------", DebugTypeEnum.Thrust);
					*/

					if (!IndirectWaypointType.HasFlag(WaypointModificationEnum.PlanetPathingAscend) && !IndirectWaypointType.HasFlag(WaypointModificationEnum.Collision)) {

						if ((stoppingDist >= upDistance && upVelocityAmt >= Data.IdealMinSpeed)) {

							_thrustToApply.SetY(false, false, 0, _orientation);
							skipCheck = true;
							_debugThrustUpMode = string.Format("Current Dist / Stop Dist  :  {0} / {1}", Math.Round(upDistance), Math.Round(stoppingDist));

						}

						if (upDistance <= this.Data.WaypointTolerance) {

							_thrustToApply.SetY(false, false, 0, _orientation);
							skipCheck = true;
							_debugThrustUpMode = "Within Waypoint Tolerance Distance";

						}

					}

					if (!skipCheck && upVelocityAmt < Data.IdealMaxSpeed - Data.MaxSpeedTolerance) {

						_thrustToApply.SetY(true, invertedDir, 1, _orientation);
						skipCheck = true;
						_debugThrustUpMode = "Thrust To Desired Speed";

					}

					if (!skipCheck && upVelocityAmt > Data.IdealMaxSpeed + Data.MaxSpeedTolerance) {

						_thrustToApply.SetY(true, invertedDir ? false : true, 1, _orientation);
						skipCheck = true;
						_debugThrustUpMode = "Brake To Desired Speed";

					}

					if (!skipCheck) {

						_thrustToApply.SetY(true, invertedDir, 0.0001f, _orientation);
						_debugThrustUpMode = "Drifting At Desired Speed";

					}
	
				} else {

					_debugThrustUpMode = "At Desired Altitude Range";
					_thrustToApply.SetY(false, false, 0, _orientation);

				}

				

			} else {

				_debugThrustForwardMode = "Not Level With Gravity Direction";
				_debugThrustUpMode = "Not Level With Gravity Direction";
				_thrustToApply.SetY(false, false, 0, _orientation);
				_thrustToApply.SetZ(false, false, 0, _orientation);
				return;

			}

			//Forward Axis
			var forwardToDestAngle = VectorHelper.GetAngleBetweenDirections(this.RefBlockMatrixRotation.Forward, Vector3D.Normalize(_currentWaypoint - _myPosition));

			if (forwardToDestAngle <= 90) {

				if (this.YawAngleDifference <= this.Data.AngleAllowedForForwardThrust) {

					
					var stoppingDist = CalculateStoppingDistance(velocity, _forwardDir, Base6Directions.Direction.Backward);
					var forwardDistance = Vector3D.Dot(_currentWaypoint - _myPosition, _forwardDir);
					bool skipCheck = false;
					var forwardVelocityAmt = Vector3D.Dot(velocity, _forwardDir);

					if (!IndirectWaypointType.HasFlag(WaypointModificationEnum.PlanetPathingAscend) && !IndirectWaypointType.HasFlag(WaypointModificationEnum.Collision)) {

						if ((stoppingDist >= forwardDistance && forwardVelocityAmt >= Data.IdealMinSpeed)) {

							_debugThrustForwardMode = string.Format("Current Dist / Stop Dist  :  {0} / {1}", Math.Round(forwardDistance), Math.Round(stoppingDist));
							_thrustToApply.SetZ(false, false, 0, _orientation);
							skipCheck = true;

						}

						if (forwardDistance <= this.Data.WaypointTolerance) {

							_debugThrustForwardMode = "Within Waypoint Tolerance Distance";
							_thrustToApply.SetZ(false, false, 0, _orientation);
							skipCheck = true;

						}

					}

					if (!skipCheck && forwardVelocityAmt < Data.IdealMaxSpeed - Data.MaxSpeedTolerance) {

						_debugThrustForwardMode = "Thrust To Desired Speed";
						_thrustToApply.SetZ(true, false, 1, _orientation);
						skipCheck = true;

					}

					if (!skipCheck && forwardVelocityAmt > Data.IdealMaxSpeed + Data.MaxSpeedTolerance) {

						_debugThrustForwardMode = "Brake To Desired Speed";
						_thrustToApply.SetZ(true, true, 1, _orientation);
						skipCheck = true;

					}

					if (!skipCheck) {

						_debugThrustForwardMode = "Drifting At Desired Speed";
						_thrustToApply.SetZ(true, false, 0.0001f, _orientation);

					}
						
				} else {

					_thrustToApply.SetZ(false, false, 0, _orientation);

				}

			} else {

				_debugThrustForwardMode = "Forward Angle To Waypoint Greater Than 90";
				_thrustToApply.SetZ(false, false, 0, _orientation);

			}

		}

		public double CalculateStoppingDistance(Vector3D velocity, Vector3D travelDir, Base6Directions.Direction baseBrakeDir) {

			if (!Data.SlowDownOnWaypointApproach)
				return -1;

			var force = GetEffectiveThrustInDirection(baseBrakeDir);
			var mass = _remoteControl.CalculateShipMass().TotalMass;
			var accel = MathTools.CalculateAcceleration(force, mass);
			var gravity = _remoteControl.GetNaturalGravity();

			if (gravity != Vector3D.Zero) {

				var gravLen = gravity.Length();
				accel += Vector3D.Dot(Vector3D.Normalize(gravity), travelDir) * gravLen;
			
			}

			if (accel <= 0)
				return 0;

			return MathTools.StoppingDistance(accel, velocity.Length(), Data.IdealMinSpeed) + Data.ExtraSlowDownDistance;

		}

		public void CalculateStrafeThrust() {

			if (this.Strafing == false) {

				TimeSpan duration = MyAPIGateway.Session.GameDateTime - this.LastStrafeEndTime;

				if (duration.TotalMilliseconds >= this.ThisStrafeCooldown) {

					if (MyVelocity.Length() > Data.StrafeSpeedCutOff || (_upDirection != Vector3D.Zero && VectorHelper.GetAngleBetweenDirections(_upDirection, RefBlockMatrixRotation.Up) > 90)) {

						this.LastStrafeStartTime = MyAPIGateway.Session.GameDateTime;
						_debugThrustForwardMode = "None";
						_debugThrustUpMode = "None";
						_debugThrustSideMode = "None";
						return;

					}

					//Logger.MsgDebug("Begin Strafe", DebugTypeEnum.Strafe);
					this.LastStrafeStartTime = MyAPIGateway.Session.GameDateTime;
					this.ThisStrafeDuration = Rnd.Next(Data.StrafeMinDurationMs, Data.StrafeMaxDurationMs);
					_strafeStartPosition = _myPosition;
					this.Strafing = true;

					Collision.RunSecondaryCollisionChecks();

					var strafeRandomization = new Vector3I(Rnd.Next(-1, 2), Rnd.Next(-1, 2), Rnd.Next(-1, 2));

					if (strafeRandomization.X != 0) {

						this.CurrentAllowedStrafeDirections.X = 1;
						var rAngle = VectorHelper.GetAngleBetweenDirections(Collision.Matrix.Right, Vector3D.Normalize(GetCurrentWaypoint() - Collision.Matrix.Translation));
						var lAngle = VectorHelper.GetAngleBetweenDirections(Collision.Matrix.Left, Vector3D.Normalize(GetCurrentWaypoint() - Collision.Matrix.Translation));
						bool rTargetIntersect = (rAngle < this.Data.StrafeMinimumSafeAngleFromTarget && DistanceToCurrentWaypoint < this.Data.StrafeMinimumTargetDistance);
						bool lTargetIntersect = (lAngle < this.Data.StrafeMinimumSafeAngleFromTarget && DistanceToCurrentWaypoint < this.Data.StrafeMinimumTargetDistance);

						if (strafeRandomization.X == 1) {

							if (rTargetIntersect || (Collision.RightResult.HasTarget(Data.StrafeMinimumTargetDistance) && !Collision.LeftResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: X Reverse", DebugTypeEnum.Strafe);
								strafeRandomization.X *= -1;

							} else if (lTargetIntersect || (Collision.RightResult.HasTarget(Data.StrafeMinimumTargetDistance) && Collision.LeftResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: X Negate", DebugTypeEnum.Strafe);
								strafeRandomization.X = 0;

							}

						} else {

							if (lTargetIntersect || (Collision.LeftResult.HasTarget(Data.StrafeMinimumTargetDistance) && !Collision.RightResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: X Reverse", DebugTypeEnum.Strafe);
								strafeRandomization.X *= -1;

							} else if (rTargetIntersect || (Collision.LeftResult.HasTarget(Data.StrafeMinimumTargetDistance) && Collision.RightResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: X Negate", DebugTypeEnum.Strafe);
								strafeRandomization.X = 0;

							}

						}

					}

					if (strafeRandomization.Y != 0) {

						strafeRandomization.Y = 1;
						var uAngle = VectorHelper.GetAngleBetweenDirections(Collision.Matrix.Up, Vector3D.Normalize(GetCurrentWaypoint() - Collision.Matrix.Translation));
						var dAngle = VectorHelper.GetAngleBetweenDirections(Collision.Matrix.Down, Vector3D.Normalize(GetCurrentWaypoint() - Collision.Matrix.Translation));
						bool uTargetIntersect = (uAngle < this.Data.StrafeMinimumSafeAngleFromTarget && DistanceToCurrentWaypoint < this.Data.StrafeMinimumTargetDistance);
						bool dTargetIntersect = (dAngle < this.Data.StrafeMinimumSafeAngleFromTarget && DistanceToCurrentWaypoint < this.Data.StrafeMinimumTargetDistance);

						if (strafeRandomization.Y == 1) {

							if (uTargetIntersect || (Collision.UpResult.HasTarget(Data.StrafeMinimumTargetDistance) && !Collision.DownResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: Y Reverse", DebugTypeEnum.Strafe);
								strafeRandomization.Y *= -1;

							} else if (dTargetIntersect || (Collision.UpResult.HasTarget(Data.StrafeMinimumTargetDistance) && Collision.DownResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: Y Negate", DebugTypeEnum.Strafe);
								strafeRandomization.Y = 0;

							}

						} else {

							if (dTargetIntersect || (Collision.DownResult.HasTarget(Data.StrafeMinimumTargetDistance) && !Collision.UpResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: Y Reverse", DebugTypeEnum.Strafe);
								strafeRandomization.Y *= -1;

							} else if (uTargetIntersect || (Collision.DownResult.HasTarget(Data.StrafeMinimumTargetDistance) && Collision.UpResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: Y Negate", DebugTypeEnum.Strafe);
								strafeRandomization.Y = 0;

							}

						}

					}

					if (strafeRandomization.Z != 0) {

						strafeRandomization.Z = 1;
						var fAngle = VectorHelper.GetAngleBetweenDirections(Collision.Matrix.Forward, Vector3D.Normalize(GetCurrentWaypoint() - Collision.Matrix.Translation));
						var bAngle = VectorHelper.GetAngleBetweenDirections(Collision.Matrix.Backward, Vector3D.Normalize(GetCurrentWaypoint() - Collision.Matrix.Translation));
						bool fTargetIntersect = (fAngle < this.Data.StrafeMinimumSafeAngleFromTarget && DistanceToCurrentWaypoint < this.Data.StrafeMinimumTargetDistance);
						bool bTargetIntersect = (bAngle < this.Data.StrafeMinimumSafeAngleFromTarget && DistanceToCurrentWaypoint < this.Data.StrafeMinimumTargetDistance);

						if (strafeRandomization.Z == 1) {

							if (fTargetIntersect || (Collision.ForwardResult.HasTarget(Data.StrafeMinimumTargetDistance) && !Collision.BackwardResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: Z Reverse", DebugTypeEnum.AutoPilot);
								strafeRandomization.Z *= -1;

							} else if (bTargetIntersect || (Collision.ForwardResult.HasTarget(Data.StrafeMinimumTargetDistance) && Collision.BackwardResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: Z Negate", DebugTypeEnum.AutoPilot);
								strafeRandomization.Z = 0;

							}

						} else {

							if (bTargetIntersect || (Collision.BackwardResult.HasTarget(Data.StrafeMinimumTargetDistance) && !Collision.ForwardResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: Z Reverse", DebugTypeEnum.AutoPilot);
								strafeRandomization.Z *= -1;

							} else if (fTargetIntersect || (Collision.BackwardResult.HasTarget(Data.StrafeMinimumTargetDistance) && Collision.ForwardResult.HasTarget(Data.StrafeMinimumTargetDistance))) {

								Logger.MsgDebug("Strafe: Z Negate", DebugTypeEnum.AutoPilot);
								strafeRandomization.Z = 0;

							}

						}

					}

					if (UpDirectionFromPlanet != Vector3D.Zero && MyAltitude < Data.MinimumPlanetAltitude) {

						var thrustDir = VectorHelper.GetThrustDirectionsAwayFromDirection(Collision.Matrix, -UpDirectionFromPlanet);

						if (thrustDir.X != 0) {

							strafeRandomization.X = thrustDir.X;

						}

						if (thrustDir.Y != 0) {

							strafeRandomization.Y = thrustDir.Y;

						}

						if (thrustDir.Z != 0) {

							strafeRandomization.Z = thrustDir.Z;

						}

					}

					//Set Thrust To Apply Here!

					if (strafeRandomization.X != 0) {

						_thrustToApply.SetX(true, strafeRandomization.X == -1, 1, _orientation);

					} else {

						_thrustToApply.SetX(false, false, 0, _orientation);

					}

					if (strafeRandomization.Y != 0) {

						_thrustToApply.SetY(true, strafeRandomization.Y == -1, 1, _orientation);

					} else {

						_thrustToApply.SetY(false, false, 0, _orientation);

					}

					if (strafeRandomization.Z != 0) {

						_thrustToApply.SetZ(true, strafeRandomization.Z == -1, 1, _orientation);

					} else {

						_thrustToApply.SetZ(false, false, 0, _orientation);

					}

				} else {

					_debugThrustForwardMode = "None";
					_debugThrustUpMode = "None";
					_debugThrustSideMode = "None";

				}

			} else {

				TimeSpan duration = MyAPIGateway.Session.GameDateTime - this.LastStrafeStartTime;

				if (duration.TotalMilliseconds >= this.ThisStrafeDuration || MyVelocity.Length() > Data.StrafeSpeedCutOff || Vector3D.Distance(_strafeStartPosition, _myPosition) > Data.StrafeDistanceCutOff) {

					//Logger.MsgDebug("End Strafe", DebugTypeEnum.General);
					this.InvertStrafingActivated = false;
					this.LastStrafeEndTime = MyAPIGateway.Session.GameDateTime;
					this.ThisStrafeCooldown = Rnd.Next(Data.StrafeMinCooldownMs, Data.StrafeMaxCooldownMs);
					this.Strafing = false;
					_collisionStrafeAdjusted = false;
					_minAngleDistanceStrafeAdjusted = false;
					_collisionStrafeDirection = Vector3D.Zero;

					_thrustToApply.SetX(false, false, 0, _orientation);
					_thrustToApply.SetY(false, false, 0, _orientation);
					_thrustToApply.SetZ(false, false, 0, _orientation);

					//Logger.AddMsg("Cooldown: " + this.ThisStrafeCooldown.ToString(), true);

				} else {

					//Logger.MsgDebug("Strafe Collision: " + Collision.VelocityResult.CollisionImminent.ToString() + " - " + Collision.VelocityResult.Time.ToString(), DebugTypeEnum.Collision);

					if (!_collisionStrafeAdjusted && Collision.VelocityResult.CollisionImminent()) {

						Logger.MsgDebug("Strafe Velocity Collision Detect: " + Collision.VelocityResult.Type.ToString() + ", " + Collision.VelocityResult.GetCollisionDistance(), DebugTypeEnum.Collision);
						_collisionStrafeAdjusted = true;
						StopStrafeDirectionNearestPosition(Collision.VelocityResult.GetCollisionCoords());
						_collisionStrafeDirection = Vector3D.Normalize(Collision.VelocityResult.GetCollisionCoords() - _remoteControl.WorldMatrix.Translation);

					} else if (_collisionStrafeAdjusted && VectorHelper.GetAngleBetweenDirections(_collisionStrafeDirection, Vector3D.Normalize(Collision.Velocity - _remoteControl.WorldMatrix.Translation)) > 15) {

						Logger.MsgDebug("Strafe Collision Detect", DebugTypeEnum.General);
						StopStrafeDirectionNearestPosition(Collision.VelocityResult.GetCollisionCoords());
						_collisionStrafeDirection = Vector3D.Normalize(Collision.VelocityResult.GetCollisionCoords() - _remoteControl.WorldMatrix.Translation);

					}

					if (_minAngleDistanceStrafeAdjusted && AngleToCurrentWaypoint < this.Data.StrafeMinimumSafeAngleFromTarget && DistanceToCurrentWaypoint < this.Data.StrafeMinimumTargetDistance) {

						Logger.MsgDebug("Strafe Min Dist/Angle Detect", DebugTypeEnum.General);
						_minAngleDistanceStrafeAdjusted = false;
						StopStrafeDirectionNearestPosition(Collision.VelocityResult.GetCollisionCoords());
						_collisionStrafeDirection = Vector3D.Normalize(Collision.VelocityResult.GetCollisionCoords() - _remoteControl.WorldMatrix.Translation);

					}

					var forwardData = _thrustToApply.GetThrustDataFromDirection(Direction.Forward, _behavior.Settings.BlockOrientation);
					var upData = _thrustToApply.GetThrustDataFromDirection(Direction.Up, _behavior.Settings.BlockOrientation);
					var sideData = _thrustToApply.GetThrustDataFromDirection(Direction.Right, _behavior.Settings.BlockOrientation);

					_debugThrustForwardMode = (forwardData.X == 0 || forwardData.Z <= 0.01) ? "None" : forwardData.Y == 1 ? "Strafe Forward" : "Strafe Backward";
					_debugThrustUpMode = (upData.X == 0 || upData.Z <= 0.01) ? "Strafe None" : upData.Y == 1 ? "Strafe Up" : "Strafe Down";
					_debugThrustSideMode = (sideData.X == 0 || sideData.Z <= 0.01) ? "Strafe None" : sideData.Y == 1 ? "Strafe Right" : "Strafe Left";

				}

			}

		}

		//Parallel
		public void StopAllThrust() {

			_thrustToApply.SetX(false, false, 0, _orientation);
			_thrustToApply.SetY(false, false, 0, _orientation);
			_thrustToApply.SetZ(false, false, 0, _orientation);

		}

		//Main Thread
		public void ApplyThrust() {

			foreach (var thrust in ThrustProfiles)
				thrust.ApplyThrust(_thrustToApply);

		}

		public double GetEffectiveThrustInDirection(Base6Directions.Direction direction) {

			var transformedDirection = _orientation.TransformDirection(direction);
			double result = 0;

			foreach (var thrust in ThrustProfiles)
				result += thrust.GetEffectiveThrust(transformedDirection);

			return result;
		
		}

		//Parallel
		public void SetBaseDirections(MyBlockOrientation orientation) {

			foreach (var thrust in ThrustProfiles)
				thrust.SetBaseDirection(orientation);

		}

		public void StopStrafeDirectionNearestPosition(Vector3D coords) {

			double minAngle = 50; //Move this to fields later
			var targetDir = Vector3D.Normalize(coords - _remoteControl.WorldMatrix.Translation);
			var leftAngle = VectorHelper.GetAngleBetweenDirections(_remoteControl.WorldMatrix.Left, targetDir);
			var rightAngle = VectorHelper.GetAngleBetweenDirections(_remoteControl.WorldMatrix.Right, targetDir);
			var upAngle = VectorHelper.GetAngleBetweenDirections(_remoteControl.WorldMatrix.Up, targetDir);
			var downAngle = VectorHelper.GetAngleBetweenDirections(_remoteControl.WorldMatrix.Down, targetDir);
			var forwardAngle = VectorHelper.GetAngleBetweenDirections(_remoteControl.WorldMatrix.Forward, targetDir);
			var backAngle = VectorHelper.GetAngleBetweenDirections(_remoteControl.WorldMatrix.Backward, targetDir);

			if ((this.CurrentStrafeDirections.X == 1 && rightAngle < minAngle) || (this.CurrentStrafeDirections.X == -1 && leftAngle < minAngle))
				_thrustToApply.SetX(false, false, 0, _orientation);

			if ((this.CurrentStrafeDirections.Y == 1 && upAngle < minAngle) || (this.CurrentStrafeDirections.Y == -1 && downAngle < minAngle))
				_thrustToApply.SetY(false, false, 0, _orientation);

			if ((this.CurrentStrafeDirections.Z == 1 && forwardAngle < minAngle) || (this.CurrentStrafeDirections.Z == -1 && backAngle < minAngle))
				_thrustToApply.SetZ(false, false, 0, _orientation);

		}

	}
}
