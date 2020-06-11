using RivalAI.Behavior.Subsystems.Profiles;
using RivalAI.Behavior.Subsystems.Weapons;
using RivalAI.Helpers;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using Ingame = Sandbox.ModAPI.Ingame;

namespace RivalAI.Behavior.Subsystems {

	public enum AutoPilotType {

		None,
		CargoShip,
		Legacy,
		RivalAI,

	}

	[Flags]
	public enum NewAutoPilotMode {

		None = 0,
		RotateToWaypoint = 1 << 0,
		ThrustForward = 1 << 1,
		Strafe = 1 << 2,
		HoverToWaypoint = 1 << 3

	}

	public enum PathCheckResult {

		Ok,
		TerrainHigherThanNpc,
		TerrainHigherThanWaypoint


	}
	public partial class AutoPilotSystem {

		//General Config
		public float IdealMaxSpeed;
		public float MaxSpeedTolerance;
		public bool UseVanillaCollisionAvoidance;

		public bool UseStuckMovementCorrection;
		public double MaxUpAngleWhenStuck;
		public double MaxSpeedWhenStuck;

		//Planet Config
		public double MaxPlanetPathCheckDistance;
		public double IdealPlanetAltitude;
		public double MinimumPlanetAltitude;
		public double AltitudeTolerance;
		public double WaypointTolerance;

		//Offset Space Config
		public double OffsetSpaceMinDistFromTarget;
		public double OffsetSpaceMaxDistFromTarget;

		//Offset Planet Config
		public double OffsetPlanetMinDistFromTarget;
		public double OffsetPlanetMaxDistFromTarget;
		public double OffsetPlanetMinTargetAltitude;
		public double OffsetPlanetMaxTargetAltitude;

		//Collision Config
		public bool UseVelocityCollisionEvasion;
		public double CollisionEvasionWaypointDistance; //Make Space and Planet Variant - OR... Make This Based on Detection Type!
		public double CollisionFallEvasionWaypointDistance;
		public double CollisionEvasionResumeDistance;
		public int CollisionEvasionResumeTime;
		public bool CollisionEvasionWaypointCalculatedAwayFromEntity;
		public double CollisionEvasionWaypointFromEntityMaxAngle;

		//Lead Config
		public bool UseProjectileLeadPrediction;
		public bool UseCollisionLeadPrediction;

		//Non-Configurable
		private IMyRemoteControl _remoteControl;
		private IBehavior _behavior;

		private AutoPilotType _currentAutoPilot;
		private bool _autopilotOverride;
		private bool _strafeOverride;
		private bool _rollOverride;

		//New AutoPilot
		public NewAutoPilotMode CurrentMode;
		public NewCollisionSystem Collision;
		//public RotationSystem Rotation;
		public NewTargetingSystem Targeting;
		//public ThrustSystem Thrust;
		public TriggerSystem Trigger;
		public WeaponSystem Weapons;

		private bool _firstRun;
		private bool _getInitialWaypointFromTarget;
		private bool _calculateOffset;
		private bool _avoidPotentialCollisions;
		private bool _calculateSafePlanetPath;

		public WaypointModificationEnum DirectWaypointType;
		public WaypointModificationEnum IndirectWaypointType;
		private Vector3D _myPosition; //
		private Vector3D _forwardDir;
		private Vector3D _previousWaypoint;
		private Vector3D _initialWaypoint; //A static waypoint or derived from last valid target position.
		private Vector3D _pendingWaypoint; //Gets calculated in parallel.
		private Vector3D _currentWaypoint; //Actual position being travelled to.
		private Vector3D _calculatedOffsetWaypoint;
		private Vector3D _calculatedPlanetPathWaypoint;
		private Vector3D _calculatedWeaponPredictionWaypoint;
		private Vector3D _evadeWaypoint;
		private Vector3D _evadeFromWaypoint;
		private DateTime _evadeWaypointCreateTime;
		public double MyAltitude;
		public Vector3D UpDirectionFromPlanet;
		private bool _waypointIsOffset;
		private bool _waypointIsTarget;

		private bool _staticGrid;
		public Vector3D MyVelocity;

		//Offset Stuff
		private bool _offsetRequiresCalculation;
		private WaypointOffsetType _offsetType;
		private double _offsetDistanceFromTarget;
		private double _offsetAltitudeFromTarget;
		private bool _offsetAltitudeIsMinimum;
		private double _offsetDistance;
		private Vector3D _offsetDirection;
		private MatrixD _offsetMatrix;
		private IMyEntity _offsetRelativeEntity;

		//Autopilot Correction
		private DateTime _lastAutoPilotCorrection;
		private bool _needsThrustersRetoggled;

		public double DistanceToInitialWaypoint;
		public double DistanceToCurrentWaypoint;
		public double DistanceToWaypointAtMyAltitude;
		public double AngleToInitialWaypoint;
		public double AngleToCurrentWaypoint;
		public double DistanceToOffsetAtMyAltitude;
		private bool _requiresClimbToIdealAltitude;
		private bool _requiresNavigationAroundCollision;

		//PlanetData - Self
		private bool _inGravityLastUpdate;
		private MyPlanet _currentPlanet;
		private Vector3D _upDirection;
		private double _gravityStrength;
		private double _surfaceDistance;
		private float _airDensity;

		public Action OnComplete; //After Autopilot is done everything, it starts a new task elsewhere.

		private const double PLANET_PATH_CHECK_DISTANCE = 1000;
		private const double PLANET_PATH_CHECK_INCREMENT = 50;

		//Debug
		public List<BoundingBoxD> DebugVoxelHits = new List<BoundingBoxD>();

		public AutoPilotSystem(IMyRemoteControl remoteControl, IBehavior behavior) {

			//Configurable - AutoPilot
			IdealMaxSpeed = 100;
			MaxSpeedTolerance = 15;
			UseVanillaCollisionAvoidance = false;

			UseStuckMovementCorrection = false;
			MaxUpAngleWhenStuck = 2;
			MaxSpeedWhenStuck = 5;

			MaxPlanetPathCheckDistance = 1000;
			IdealPlanetAltitude = 200;
			MinimumPlanetAltitude = 110;
			AltitudeTolerance = 10;
			WaypointTolerance = 10;

			OffsetSpaceMinDistFromTarget = 100;
			OffsetSpaceMaxDistFromTarget = 200;

			OffsetPlanetMinDistFromTarget = 100;
			OffsetPlanetMaxDistFromTarget = 200;
			OffsetPlanetMinTargetAltitude = 100;
			OffsetPlanetMaxTargetAltitude = 200;

			UseVelocityCollisionEvasion = true;
			CollisionEvasionWaypointDistance = 300;
			CollisionFallEvasionWaypointDistance = 75;
			CollisionEvasionResumeDistance = 25;
			CollisionEvasionResumeTime = 10;
			CollisionEvasionWaypointCalculatedAwayFromEntity = false;
			CollisionEvasionWaypointFromEntityMaxAngle = 15;

			UseProjectileLeadPrediction = true;
			UseCollisionLeadPrediction = false;

			//Configurable - Rotation
			RotationMultiplier = 1;

			//Configurable - Thrust
			AngleAllowedForForwardThrust = 35;
			MaxVelocityAngleForSpeedControl = 5;
			AllowStrafing = false;
			StrafeMinDurationMs = 750;
			StrafeMaxDurationMs = 1500;
			StrafeMinCooldownMs = 1000;
			StrafeMaxCooldownMs = 3000;
			StrafeSpeedCutOff = 100;
			StrafeDistanceCutOff = 100;

			StrafeMinimumTargetDistance = 250;
			StrafeMinimumSafeAngleFromTarget = 25;

			AllowedStrafingDirectionsSpace = new Vector3I(1, 1, 1);
			AllowedStrafingDirectionsPlanet = new Vector3I(1, 1, 1);


			//Internal - AutoPilot
			_currentAutoPilot = AutoPilotType.None;
			_autopilotOverride = false;
			_strafeOverride = false;
			_rollOverride = false;

			CurrentMode = NewAutoPilotMode.None;

			_firstRun = false;
			_getInitialWaypointFromTarget = false;
			_calculateOffset = false;
			_calculateSafePlanetPath = false;

			DirectWaypointType = WaypointModificationEnum.None;
			IndirectWaypointType = WaypointModificationEnum.None;
			_myPosition = Vector3D.Zero;
			_forwardDir = Vector3D.Zero;
			_previousWaypoint = Vector3D.Zero;
			_initialWaypoint = Vector3D.Zero;
			_pendingWaypoint = Vector3D.Zero;
			_currentWaypoint = Vector3D.Zero;
			_evadeWaypoint = Vector3D.Zero;
			_evadeFromWaypoint = Vector3D.Zero;
			_evadeWaypointCreateTime = DateTime.Now;
			MyAltitude = 0;
			UpDirectionFromPlanet = Vector3D.Zero;
			_waypointIsTarget = false;

			_offsetRequiresCalculation = false;
			_offsetType = WaypointOffsetType.None;
			_offsetDistanceFromTarget = 0;
			_offsetAltitudeFromTarget = 0;
			_offsetAltitudeIsMinimum = false;
			_offsetDistance = 0;
			_offsetDirection = Vector3D.Zero;
			_offsetMatrix = MatrixD.Identity;
			_offsetRelativeEntity = null;

			_lastAutoPilotCorrection = MyAPIGateway.Session.GameDateTime;
			_needsThrustersRetoggled = false;

			_requiresClimbToIdealAltitude = false;
			_requiresNavigationAroundCollision = false;

			_currentPlanet = null;
			_upDirection = Vector3D.Zero;
			_gravityStrength = 0;
			_surfaceDistance = 0;
			_airDensity = 0;

			//Internal - Rotation
			RotationEnabled = false;
			ControlGyro = null;
			ReferenceBlock = null;
			BrokenGyros = new List<IMyGyro>();

			RotationDirection = Direction.Forward;

			ControlGyroNotFound = false;
			NewGyroFound = false;

			RotationTarget = Vector3D.Zero;
			UpDirection = Vector3D.Zero;

			ControlYaw = true;
			ControlPitch = true;
			ControlRoll = true;

			ControlGyroStrength = 1;

			UpdateMassAndForceBeforeRotation = true;
			GridMass = 0;
			GridGyroForce = 0;
			GyroMaxPower = 100;

			CurrentAngleToTarget = 0;
			CurrentYawDifference = 0;
			CurrentPitchDifference = 0;
			CurrentRollDifference = 0;
			DesiredAngleToTarget = 0.5;

			BarrelRollEnabled = false;
			BarrellRollMagnitudePerEvent = 1;

			RotationToApply = Vector3.Zero;
			ControlGyroRotationTranslation = new Dictionary<string, Vector3D>();

			//Internal - Thrust
			Mode = ThrustMode.None;
			ThrustProfiles = new List<ThrustProfile>();
			Rnd = new Random();

			CurrentAllowedThrust = Vector3I.Zero;
			CurrentRequiredThrust = Vector3I.Zero;

			if (StrafeMinDurationMs >= StrafeMaxDurationMs) {

				StrafeMaxDurationMs = StrafeMinDurationMs + 1;

			}

			if (StrafeMinCooldownMs >= StrafeMaxCooldownMs) {

				StrafeMaxCooldownMs = StrafeMinCooldownMs + 1;

			}

			_referenceOrientation = new MyBlockOrientation(Base6Directions.Direction.Forward, Base6Directions.Direction.Up);

			Strafing = false;
			CurrentStrafeDirections = Vector3I.Zero;
			CurrentAllowedStrafeDirections = Vector3I.Zero;
			ThisStrafeDuration = Rnd.Next(StrafeMinDurationMs, StrafeMaxDurationMs);
			ThisStrafeCooldown = Rnd.Next(StrafeMinCooldownMs, StrafeMaxCooldownMs);
			LastStrafeStartTime = MyAPIGateway.Session.GameDateTime;
			LastStrafeEndTime = MyAPIGateway.Session.GameDateTime;

			_collisionStrafeAdjusted = false;
			_minAngleDistanceStrafeAdjusted = false;
			_collisionStrafeDirection = Vector3D.Zero;

			//Post Constructor Setup
			if (remoteControl != null && MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid)) {

				_remoteControl = remoteControl;
				_behavior = behavior;
				Collision = new NewCollisionSystem(_remoteControl, this);
				Targeting = new NewTargetingSystem(_remoteControl);
				Weapons = new WeaponSystem(_remoteControl, _behavior);

				var blockList = new List<IMySlimBlock>();
				_remoteControl.SlimBlock.CubeGrid.GetBlocks(blockList);

				foreach (var block in blockList.Where(item => item.FatBlock as IMyThrust != null)) {

					this.ThrustProfiles.Add(new ThrustProfile(block.FatBlock as IMyThrust, _remoteControl, _behavior));

				}

				this.CubeGrid = remoteControl.SlimBlock.CubeGrid;
				UpdateMassAndGyroForce();

			}

		}

		public void SetupReferences(IBehavior behavior, StoredSettings settings, TriggerSystem trigger) {

			Targeting.SetupReferences(settings);
			Trigger = trigger;

		}

		public void InitTags() {

			if (string.IsNullOrWhiteSpace(_remoteControl.CustomData) == false) {

				var descSplit = _remoteControl.CustomData.Split('\n');

				foreach (var tag in descSplit) {

					//IdealMaxSpeed
					if (tag.Contains("[IdealMaxSpeed:") == true) {

						this.IdealMaxSpeed = TagHelper.TagFloatCheck(tag, this.IdealMaxSpeed);

					}

					//MaxSpeedTolerance
					if (tag.Contains("[MaxSpeedTolerance:") == true) {

						this.MaxSpeedTolerance = TagHelper.TagFloatCheck(tag, this.MaxSpeedTolerance);

					}

					//UseVanillaCollisionAvoidance
					if (tag.Contains("[UseVanillaCollisionAvoidance:") == true) {

						this.UseVanillaCollisionAvoidance = TagHelper.TagBoolCheck(tag);

					}

					//UseStuckMovementCorrection
					if (tag.Contains("[UseStuckMovementCorrection:") == true) {

						this.UseStuckMovementCorrection = TagHelper.TagBoolCheck(tag);

					}

					//MaxUpAngleWhenStuck
					if (tag.Contains("[MaxUpAngleWhenStuck:") == true) {

						this.MaxUpAngleWhenStuck = TagHelper.TagDoubleCheck(tag, this.MaxUpAngleWhenStuck);

					}

					//MaxSpeedWhenStuck
					if (tag.Contains("[MaxSpeedWhenStuck:") == true) {

						this.MaxSpeedWhenStuck = TagHelper.TagDoubleCheck(tag, this.MaxSpeedWhenStuck);

					}

					//MaxPlanetPathCheckDistance
					if (tag.Contains("[MaxPlanetPathCheckDistance:") == true) {

						this.MaxPlanetPathCheckDistance = TagHelper.TagDoubleCheck(tag, this.MaxPlanetPathCheckDistance);

					}

					//IdealPlanetAltitude
					if (tag.Contains("[IdealPlanetAltitude:") == true) {

						this.IdealPlanetAltitude = TagHelper.TagDoubleCheck(tag, this.IdealPlanetAltitude);

					}

					//MinimumPlanetAltitude
					if (tag.Contains("[MinimumPlanetAltitude:") == true) {

						this.MinimumPlanetAltitude = TagHelper.TagDoubleCheck(tag, this.MinimumPlanetAltitude);

					}

					//AltitudeTolerance
					if (tag.Contains("[AltitudeTolerance:") == true) {

						this.AltitudeTolerance = TagHelper.TagDoubleCheck(tag, this.AltitudeTolerance);

					}

					//WaypointTolerance
					if (tag.Contains("[WaypointTolerance:") == true) {

						this.WaypointTolerance = TagHelper.TagDoubleCheck(tag, this.WaypointTolerance);

					}

					//OffsetSpaceMinDistFromTarget
					if (tag.Contains("[OffsetSpaceMinDistFromTarget:") == true) {

						this.OffsetSpaceMinDistFromTarget = TagHelper.TagDoubleCheck(tag, this.OffsetSpaceMinDistFromTarget);

					}

					//OffsetSpaceMaxDistFromTarget
					if (tag.Contains("[OffsetSpaceMaxDistFromTarget:") == true) {

						this.OffsetSpaceMaxDistFromTarget = TagHelper.TagDoubleCheck(tag, this.OffsetSpaceMaxDistFromTarget);

					}

					//OffsetPlanetMinDistFromTarget
					if (tag.Contains("[OffsetPlanetMinDistFromTarget:") == true) {

						this.OffsetPlanetMinDistFromTarget = TagHelper.TagDoubleCheck(tag, this.OffsetPlanetMinDistFromTarget);

					}

					//OffsetPlanetMaxDistFromTarget
					if (tag.Contains("[OffsetPlanetMaxDistFromTarget:") == true) {

						this.OffsetPlanetMaxDistFromTarget = TagHelper.TagDoubleCheck(tag, this.OffsetPlanetMaxDistFromTarget);

					}

					//OffsetPlanetMinTargetAltitude
					if (tag.Contains("[OffsetPlanetMinTargetAltitude:") == true) {

						this.OffsetPlanetMinTargetAltitude = TagHelper.TagDoubleCheck(tag, this.OffsetPlanetMinTargetAltitude);

					}

					//OffsetPlanetMaxTargetAltitude
					if (tag.Contains("[OffsetPlanetMaxTargetAltitude:") == true) {

						this.OffsetPlanetMaxTargetAltitude = TagHelper.TagDoubleCheck(tag, this.OffsetPlanetMaxTargetAltitude);

					}

					//UseVelocityCollisionEvasion
					if (tag.Contains("[UseVelocityCollisionEvasion:") == true) {

						this.UseVelocityCollisionEvasion = TagHelper.TagBoolCheck(tag);

					}

					//MinimumSpeedForVelocityChecks
					if (tag.Contains("[MinimumSpeedForVelocityChecks:") == true) {

						this.Collision.MinimumSpeedForVelocityChecks = TagHelper.TagDoubleCheck(tag, this.Collision.MinimumSpeedForVelocityChecks);

					}

					//CollisionAsteroidUsesBoundingBoxForVelocity
					if (tag.Contains("[CollisionAsteroidUsesBoundingBoxForVelocity:") == true) {

						this.Collision.CollisionAsteroidUsesBoundingBoxForVelocity = TagHelper.TagBoolCheck(tag);

					}

					//CollisionTimeTrigger
					if (tag.Contains("[CollisionTimeTrigger:") == true) {

						this.Collision.CollisionTimeTrigger = TagHelper.TagIntCheck(tag, this.Collision.CollisionTimeTrigger);

					}

					//CollisionEvasionWaypointDistance
					if (tag.Contains("[CollisionEvasionWaypointDistance:") == true) {

						this.CollisionEvasionWaypointDistance = TagHelper.TagDoubleCheck(tag, this.CollisionEvasionWaypointDistance);

					}

					//CollisionFallEvasionWaypointDistance
					if (tag.Contains("[CollisionFallEvasionWaypointDistance:") == true) {

						this.CollisionFallEvasionWaypointDistance = TagHelper.TagDoubleCheck(tag, this.CollisionFallEvasionWaypointDistance);

					}

					//CollisionEvasionResumeDistance
					if (tag.Contains("[CollisionEvasionResumeDistance:") == true) {

						this.CollisionEvasionResumeDistance = TagHelper.TagDoubleCheck(tag, this.CollisionEvasionResumeDistance);

					}

					//CollisionEvasionResumeTime
					if (tag.Contains("[CollisionEvasionResumeTime:") == true) {

						this.CollisionEvasionResumeTime = TagHelper.TagIntCheck(tag, this.CollisionEvasionResumeTime);

					}

					//UseProjectileLeadPrediction
					if (tag.Contains("[UseProjectileLeadPrediction:") == true) {

						this.UseProjectileLeadPrediction = TagHelper.TagBoolCheck(tag);

					}

					//UseCollisionLeadPrediction
					if (tag.Contains("[UseCollisionLeadPrediction:") == true) {

						this.UseCollisionLeadPrediction = TagHelper.TagBoolCheck(tag);

					}

					////////////////////
					//Rotation and Thrust
					////////////////////

					//RotationMultiplier
					if (tag.Contains("[RotationMultiplier:") == true) {

						this.RotationMultiplier = TagHelper.TagFloatCheck(tag, this.RotationMultiplier);

					}

					//AngleAllowedForForwardThrust
					if (tag.Contains("[AngleAllowedForForwardThrust:") == true) {

						this.AngleAllowedForForwardThrust = TagHelper.TagDoubleCheck(tag, this.AngleAllowedForForwardThrust);

					}

					//AllowStrafing
					if (tag.Contains("[AllowStrafing:") == true) {

						this.AllowStrafing = TagHelper.TagBoolCheck(tag);

					}

					//StrafeMinDurationMs
					if (tag.Contains("[StrafeMinDurationMs:") == true) {

						this.StrafeMinDurationMs = TagHelper.TagIntCheck(tag, this.StrafeMinDurationMs);

					}

					//StrafeMaxDurationMs
					if (tag.Contains("[StrafeMaxDurationMs:") == true) {

						this.StrafeMaxDurationMs = TagHelper.TagIntCheck(tag, this.StrafeMaxDurationMs);

					}

					//StrafeMinCooldownMs
					if (tag.Contains("[StrafeMinCooldownMs:") == true) {

						this.StrafeMinCooldownMs = TagHelper.TagIntCheck(tag, this.StrafeMinCooldownMs);

					}

					//StrafeMaxCooldownMs
					if (tag.Contains("[StrafeMaxCooldownMs:") == true) {

						this.StrafeMaxCooldownMs = TagHelper.TagIntCheck(tag, this.StrafeMaxCooldownMs);

					}

					//StrafeSpeedCutOff
					if (tag.Contains("[StrafeSpeedCutOff:") == true) {

						this.StrafeSpeedCutOff = TagHelper.TagDoubleCheck(tag, this.StrafeSpeedCutOff);

					}

					//StrafeDistanceCutOff
					if (tag.Contains("[StrafeDistanceCutOff:") == true) {

						this.StrafeDistanceCutOff = TagHelper.TagDoubleCheck(tag, this.StrafeDistanceCutOff);

					}

					//StrafeMinimumTargetDistance
					if (tag.Contains("[StrafeMinimumTargetDistance:") == true) {

						this.StrafeMinimumTargetDistance = TagHelper.TagDoubleCheck(tag, this.StrafeMinimumTargetDistance);

					}

					//StrafeMinimumSafeAngleFromTarget
					if (tag.Contains("[StrafeMinimumSafeAngleFromTarget:") == true) {

						this.StrafeMinimumSafeAngleFromTarget = TagHelper.TagDoubleCheck(tag, this.StrafeMinimumSafeAngleFromTarget);

					}

					//AllowedStrafingDirectionsSpace


					//AllowedStrafingDirectionsPlanet


				}

				Targeting.InitTags();
				Weapons.InitTags();

			}

		}

		public void ThreadedAutoPilotCalculations() {

			_myPosition = _remoteControl.GetPosition();
			_forwardDir = _remoteControl.WorldMatrix.Forward;
			DirectWaypointType = WaypointModificationEnum.None;
			IndirectWaypointType = WaypointModificationEnum.None;

			if (_remoteControl.Physics != null) {

				MyVelocity = _remoteControl.Physics.LinearVelocity;

			} else {

				MyVelocity = Vector3D.Zero;

			}

			if (_autopilotOverride) {

				//TODO: Check Roll and Strafe Timers

				if (!_strafeOverride && !_rollOverride) {

					_autopilotOverride = false;

				}

				if (_autopilotOverride) {

					//FinishWaypointCalculations();
					return;

				}


			}

			if (_currentAutoPilot == AutoPilotType.None && _firstRun) {

				//FinishWaypointCalculations();
				return;

			}


			_previousWaypoint = _currentWaypoint;

			if (_getInitialWaypointFromTarget) {

				if (Targeting.HasTarget()) {

					DirectWaypointType = WaypointModificationEnum.TargetIsInitialWaypoint;
					_initialWaypoint = Targeting.GetTargetCoords();

				} else {

					//Logger.MsgDebug(" - Autopilot Has No Target", DebugTypeEnum.TargetEvaluation);

				}

			}

			try {

				CalculateCurrentWaypoint();
				_currentWaypoint = _pendingWaypoint;
				this.DistanceToInitialWaypoint = Vector3D.Distance(_myPosition, _initialWaypoint);
				this.DistanceToCurrentWaypoint = Vector3D.Distance(_myPosition, _currentWaypoint);
				this.AngleToInitialWaypoint = VectorHelper.GetAngleBetweenDirections(_forwardDir, Vector3D.Normalize(_initialWaypoint - _myPosition));
				this.AngleToCurrentWaypoint = VectorHelper.GetAngleBetweenDirections(_forwardDir, Vector3D.Normalize(_currentWaypoint - _myPosition));
				this.DistanceToWaypointAtMyAltitude = VectorHelper.GetDistanceToTargetAtMyAltitude(_myPosition, _currentWaypoint, _currentPlanet);
				this.DistanceToOffsetAtMyAltitude = VectorHelper.GetDistanceToTargetAtMyAltitude(_myPosition, _calculatedOffsetWaypoint, _currentPlanet);
				this.MyAltitude = _surfaceDistance;
				this.UpDirectionFromPlanet = _upDirection;

				_firstRun = true;

			} catch (Exception exc) {

				Logger.MsgDebug("Caught Exception While Calculating Autopilot Pathing", DebugTypeEnum.General);
				Logger.MsgDebug(exc.ToString(), DebugTypeEnum.General);

			}

		}

		public void EngageAutoPilot() {

			if (_currentAutoPilot == AutoPilotType.Legacy)
				UpdateLegacyAutoPilot();

			if (_currentAutoPilot == AutoPilotType.RivalAI)
				UpdateNewAutoPilot();

			/*
			if (_currentWaypoint == Vector3D.Zero)
				Logger.MsgDebug("No Current Waypoint", DebugTypeEnum.Dev);
				*/

			//StartWeaponCalculations();

		}

		private void UpdateLegacyAutoPilot() {

			if (_remoteControl.IsAutoPilotEnabled && Vector3D.Distance(_previousWaypoint, _currentWaypoint) < this.WaypointTolerance) {

				return;

			}
				

			_remoteControl.SetAutoPilotEnabled(false); //
			_remoteControl.ClearWaypoints();

			if (IdealMaxSpeed == 0)
				return;

			if (UseStuckMovementCorrection && _upDirection != Vector3D.Zero) {

				var timeSpan = MyAPIGateway.Session.GameDateTime - _lastAutoPilotCorrection;

				if (timeSpan.TotalSeconds > 5) {

					if (Collision.Velocity.Length() < this.MaxSpeedWhenStuck) {

						bool yawing = false;

						if (_remoteControl?.SlimBlock?.CubeGrid?.Physics != null) {

							var rotationAxis = Vector3D.Normalize(_remoteControl.SlimBlock.CubeGrid.Physics.AngularVelocity);
							var myUp = _remoteControl.WorldMatrix.Up;
							var myDown = _remoteControl.WorldMatrix.Down;
							yawing = VectorHelper.GetAngleBetweenDirections(myUp, rotationAxis) <= 1 || VectorHelper.GetAngleBetweenDirections(myDown, rotationAxis) <= 1;

						}

						if (VectorHelper.GetAngleBetweenDirections(_upDirection, Vector3D.Normalize(Collision.Velocity)) <= this.MaxUpAngleWhenStuck || yawing) {

							Logger.MsgDebug("AutoPilot Stuck, Attempting Fix", DebugTypeEnum.General);
							_lastAutoPilotCorrection = MyAPIGateway.Session.GameDateTime;
							_needsThrustersRetoggled = true;
							_remoteControl.ControlThrusters = false;
							return;

						}

					}

				}

			}

			if (_needsThrustersRetoggled) {

				_needsThrustersRetoggled = false;
				_remoteControl.ControlThrusters = true;

			}

			_remoteControl.AddWaypoint(_currentWaypoint, "Current Waypoint Target");
			_remoteControl.FlightMode = Ingame.FlightMode.OneWay;
			_remoteControl.SetCollisionAvoidance(this.UseVanillaCollisionAvoidance);
			_remoteControl.SetAutoPilotEnabled(true);

		}

		private void UpdateNewAutoPilot() {

			if (CurrentMode.HasFlag(NewAutoPilotMode.RotateToWaypoint)) {

				StartRotationCalculation(_currentWaypoint, _remoteControl, _upDirection);

			}

			if (CurrentMode.HasFlag(NewAutoPilotMode.Strafe)) {

				//Logger.MsgDebug("Process Strafe", DebugTypeEnum.Dev);
				ProcessThrustStrafing();

			}

			if (CurrentMode.HasFlag(NewAutoPilotMode.ThrustForward)) {

				ProcessForwardThrust(AngleToCurrentWaypoint);

			}

			if (CurrentMode.HasFlag(NewAutoPilotMode.HoverToWaypoint)) {

				ProcessForwardThrust(AngleToCurrentWaypoint);
				//Thrust.ProcessUpwardThrust();

			}

		}

		public void ActivateAutoPilot(AutoPilotType type, NewAutoPilotMode newType, Vector3D initialWaypoint, bool getWaypointFromTarget = false, bool useWaypointOffset = false, bool useSafePlanetPathing = false) {

			DeactivateAutoPilot();
			_currentAutoPilot = type;
			CurrentMode = newType;
			_initialWaypoint = initialWaypoint;
			_getInitialWaypointFromTarget = getWaypointFromTarget;
			_calculateOffset = useWaypointOffset;
			_calculateSafePlanetPath = useSafePlanetPathing;
			_remoteControl.SpeedLimit = IdealMaxSpeed;

		}

		public void DeactivateAutoPilot() {

			_currentAutoPilot = AutoPilotType.None;
			CurrentMode = NewAutoPilotMode.None;
			_remoteControl.SetAutoPilotEnabled(false);
			_requiresNavigationAroundCollision = false;
			_requiresClimbToIdealAltitude = false;
			StopAllRotation();
			StopAllThrust();

		}

		public Vector3D GetInitialCoords() {

			return _initialWaypoint;

		}

		public bool InGravity() {

			return (_gravityStrength > 0);

		}

		public void SetInitialWaypoint(Vector3D coords) {

			_initialWaypoint = coords;

		}

		private void CalculateCurrentWaypoint() {

			if (_initialWaypoint == Vector3D.Zero && _firstRun)
				return;

			if (_currentPlanet == null || !MyAPIGateway.Entities.Exist(_currentPlanet))
				_currentPlanet = MyGamePruningStructure.GetClosestPlanet(_myPosition);

			_pendingWaypoint = _initialWaypoint;

			var planetEntity = _currentPlanet as IMyEntity;
			var gravityProvider = planetEntity?.Components?.Get<MyGravityProviderComponent>();

			if (gravityProvider != null && gravityProvider.IsPositionInRange(_myPosition)) {

				_upDirection = Vector3D.Normalize(_myPosition - planetEntity.GetPosition());
				_gravityStrength = gravityProvider.GetWorldGravity(_myPosition).Length();
				_surfaceDistance = Vector3D.Distance(VectorHelper.GetPlanetSurfaceCoordsAtPosition(_myPosition, _currentPlanet), _myPosition);
				_airDensity = _currentPlanet.GetAirDensity(_myPosition);

				if (!_inGravityLastUpdate && (_offsetType == WaypointOffsetType.RandomOffsetFixed || _offsetType == WaypointOffsetType.RandomOffsetRelativeEntity)) {

					_offsetRequiresCalculation = true;
					_inGravityLastUpdate = true;

				}

			} else {

				_upDirection = Vector3D.Zero;
				_gravityStrength = 0;
				_airDensity = 0;

				if (_inGravityLastUpdate && (_offsetType == WaypointOffsetType.RandomOffsetFixed || _offsetType == WaypointOffsetType.RandomOffsetRelativeEntity)) {

					_offsetRequiresCalculation = true;
					_inGravityLastUpdate = false;

				}

			}

			if (CurrentMode.HasFlag(NewAutoPilotMode.Strafe)) {

				return;

			}

			//Collision
			if (this.UseVelocityCollisionEvasion && Collision.VelocityResult.CollisionImminent()) {

				if ((_gravityStrength <= 0 && Collision.VelocityResult.Type == CollisionType.Voxel) || Collision.VelocityResult.Type != CollisionType.Voxel) {

					if (!_requiresNavigationAroundCollision) {

						CalculateEvadeCoords();


					} else {

						var directionToOldCollision = Vector3D.Normalize(_evadeFromWaypoint - _myPosition);
						var directionToNewCollision = Vector3D.Normalize(Collision.VelocityResult.GetCollisionCoords() - _myPosition);
						if (VectorHelper.GetAngleBetweenDirections(directionToOldCollision, directionToNewCollision) > 45)
							CalculateEvadeCoords();

					}

				} else if (Collision.VelocityResult.Type == CollisionType.Voxel && _gravityStrength > 0 && VectorHelper.GetAngleBetweenDirections(Vector3D.Normalize(Collision.Velocity), _upDirection) < 15) {

					CalculateFallEvadeCoords();

				}

			} 

			if (_requiresNavigationAroundCollision) {

				//Logger.MsgDebug("Collision Evasion Required", DebugTypeEnum.General);
				IndirectWaypointType |= WaypointModificationEnum.Collision;
				var timeDiff = MyAPIGateway.Session.GameDateTime - _evadeWaypointCreateTime;
				var evadeCoordsDistanceFromTarget = Vector3D.Distance(_evadeWaypoint, _evadeFromWaypoint);
				var myDistEvadeFromCoords = Vector3D.Distance(_myPosition, _evadeFromWaypoint);

				if (myDistEvadeFromCoords < evadeCoordsDistanceFromTarget && Vector3D.Distance(_myPosition, _evadeWaypoint) > this.WaypointTolerance && timeDiff.TotalSeconds < this.CollisionEvasionResumeTime) {

					_pendingWaypoint = _evadeWaypoint;
					return;
				
				}
				
				_requiresNavigationAroundCollision = false;


			}

			//Offset
			
			if(_calculateOffset)
				CalculateOffsetWaypoint();

			//PlanetPathing
			if (_calculateSafePlanetPath && _gravityStrength > 0) {

				CalculateSafePlanetPathWaypoint(_currentPlanet);

				if (_initialWaypoint != _pendingWaypoint) {

					IndirectWaypointType |= WaypointModificationEnum.Offset;

				}	

			}

			if (Targeting.Target != null && _initialWaypoint == _pendingWaypoint && Targeting.Target.CurrentSpeed() > 0.1) {

				bool gotLead = false;

				if (UseCollisionLeadPrediction && !gotLead) {

					gotLead = true;
					DirectWaypointType |= WaypointModificationEnum.CollisionLeading;
					_pendingWaypoint = VectorHelper.FirstOrderIntercept(_myPosition, MyVelocity, (float)MyVelocity.Length(), _pendingWaypoint, Targeting.Target.CurrentVelocity());
					_calculatedWeaponPredictionWaypoint = _pendingWaypoint;

				}

				if (UseProjectileLeadPrediction && !gotLead) {

					gotLead = true;
					DirectWaypointType |= WaypointModificationEnum.WeaponLeading;
					//Logger.MsgDebug("Weapon Lead, Target Velocity: " + Targeting.Target.TargetVelocity.ToString(), DebugTypeEnum.Weapon);
					//_pendingWaypoint = VectorHelper.FirstOrderIntercept(_myPosition, _myVelocity, Subsystems.Weapons.MostCommonAmmoSpeed(true), _pendingWaypoint, Targeting.Target.CurrentVelocity());
					double ammoAccel;
					double ammoInitVel;
					double ammoVel;

					Weapons.GetAmmoSpeedDetails(RotationDirection, out ammoVel, out ammoInitVel, out ammoAccel);

					if (ammoVel > 0) {

						_pendingWaypoint = VectorHelper.TrajectoryEstimation(
						Targeting.TargetLastKnownCoords,
						Targeting.Target.CurrentVelocity(),
						Targeting.Target.CurrentAcceleration(),
						Targeting.Target.MaxSpeed(),
						_myPosition,
						MyVelocity,
						ammoVel,
						ammoInitVel,
						ammoAccel
						);

						_calculatedWeaponPredictionWaypoint = _pendingWaypoint;

					}

				}

				if (!gotLead)
					_calculatedWeaponPredictionWaypoint = Vector3D.Zero;

			}

		}

		

		private void CalculateEvadeCoords() {

			if (!this.CollisionEvasionWaypointCalculatedAwayFromEntity) {

				Collision.RunSecondaryCollisionChecks();
				var dirFromCollision = Vector3D.Normalize(_myPosition - Collision.VelocityResult.GetCollisionCoords());
				var evadeCoordList = new List<Vector3D>();

				if (FoundEvadeCoords(Collision.ForwardResult))
					evadeCoordList.Add(_evadeWaypoint);

				if (FoundEvadeCoords(Collision.UpResult))
					evadeCoordList.Add(_evadeWaypoint);

				if (FoundEvadeCoords(Collision.DownResult))
					evadeCoordList.Add(_evadeWaypoint);

				if (FoundEvadeCoords(Collision.LeftResult))
					evadeCoordList.Add(_evadeWaypoint);

				if (FoundEvadeCoords(Collision.RightResult))
					evadeCoordList.Add(_evadeWaypoint);

				if (FoundEvadeCoords(Collision.BackwardResult))
					evadeCoordList.Add(_evadeWaypoint);

				Vector3D closestToTarget = Vector3D.Zero;
				double closestDistanceToTarget = -1;

				foreach (var coords in evadeCoordList) {

					var distToTarget = Vector3D.Distance(coords, _pendingWaypoint);

					if (distToTarget < closestDistanceToTarget || closestDistanceToTarget == -1) {

						closestToTarget = coords;
						closestDistanceToTarget = distToTarget;

					}

				}

				if (closestDistanceToTarget != -1) {

					_evadeWaypoint = closestToTarget;
					return;

				}

				//If we got here, no evade coords could be calculated
				//GuessIllJustDie.jpg
				Logger.MsgDebug("No Collision Coords Found: ", DebugTypeEnum.Collision);


			} else {

				if (Collision.VelocityResult.GetCollisionEntity()?.PositionComp != null) {

					GetEvadeCoordsAwayFromEntity(Collision.VelocityResult.GetCollisionEntity().PositionComp.WorldAABB.Center);

				}

			}
			
		}

		private void CalculateFallEvadeCoords() {

			_requiresNavigationAroundCollision = true;
			_evadeWaypointCreateTime = MyAPIGateway.Session.GameDateTime;
			_evadeFromWaypoint = Collision.VelocityResult.GetCollisionCoords();
			_evadeWaypoint = Vector3D.Normalize(Collision.Velocity * -1) * this.CollisionFallEvasionWaypointDistance + _myPosition;

		}

		private bool FoundEvadeCoords(NewCollisionResult result) {

			if (result.Type == CollisionType.None || result.GetCollisionDistance() > this.CollisionEvasionWaypointDistance) {

				_requiresNavigationAroundCollision = true;
				_evadeWaypointCreateTime = MyAPIGateway.Session.GameDateTime;
				_evadeFromWaypoint = Collision.VelocityResult.GetCollisionCoords();
				_evadeWaypoint = result.DirectionVector * this.CollisionEvasionWaypointDistance + _myPosition;
				_evadeWaypoint = _evadeWaypoint + (VectorHelper.RandomPerpendicular(result.DirectionVector) * 10);
				return true;

			}

			return false;

		}

		private void GetEvadeCoordsAwayFromEntity(Vector3D entityCoords) {

			Logger.MsgDebug("Get Collision Evasion Waypoint Away From Entity", DebugTypeEnum.Collision);
			_requiresNavigationAroundCollision = true;
			_evadeWaypointCreateTime = MyAPIGateway.Session.GameDateTime;
			_evadeFromWaypoint = entityCoords;
			_evadeWaypoint = (Vector3D.Normalize(_myPosition - entityCoords)) * this.CollisionEvasionWaypointDistance + _myPosition;

		}

		private void CalculateOffsetWaypoint() {

			//Logger.MsgDebug(_offsetType.ToString(), DebugTypeEnum.General);

			if (_offsetRequiresCalculation)
				CreateRandomOffset(_offsetDistanceFromTarget, _offsetAltitudeFromTarget, _offsetAltitudeIsMinimum);

			_waypointIsOffset = false;

			if (_offsetType == WaypointOffsetType.None)
				return;

			if (_offsetType == WaypointOffsetType.DistanceFromTarget && _offsetDistance > 0) {

				IndirectWaypointType |= WaypointModificationEnum.Offset;
				_waypointIsOffset = true;
				var tempCoords = Vector3D.Normalize(_myPosition - _pendingWaypoint) * _offsetDistance + _pendingWaypoint;
				_pendingWaypoint = tempCoords;
				_calculatedOffsetWaypoint = _pendingWaypoint;
				return;

			}

			if (_offsetDirection != Vector3D.Zero) {

				IndirectWaypointType |= WaypointModificationEnum.Offset;
				_waypointIsOffset = true;

				if (_offsetType == WaypointOffsetType.RandomOffsetRelativeEntity && _offsetRelativeEntity?.PositionComp != null && MyAPIGateway.Entities.Exist(_offsetRelativeEntity)) {

					_offsetMatrix.Translation = _offsetRelativeEntity.PositionComp.WorldMatrix.Translation;

				} else {

					if (!Targeting.Target.IsClosed()) {

						_offsetRelativeEntity = Targeting.Target.GetEntity();
						_offsetMatrix.Translation = _offsetRelativeEntity.PositionComp.WorldMatrix.Translation;

					}

				}

				var tempCoords = _offsetDirection * _offsetDistance + _offsetMatrix.Translation;
				_pendingWaypoint = tempCoords;
				_calculatedOffsetWaypoint = _pendingWaypoint;
				return;

			}

			_calculatedOffsetWaypoint = Vector3D.Zero;

		}

		private void CreateRandomOffset(double distance, double altitude = 0, bool altitudeIsMinimum = false) {

			_offsetRequiresCalculation = false;

			if (!InGravity()) {

				_offsetDistance = distance;
				var directionRand = VectorHelper.RandomDirection();
				var directionRandInv = directionRand * -1;
				var dirDist = Vector3D.Distance(_pendingWaypoint + directionRand, _myPosition);
				var dirDistInv = Vector3D.Distance(_pendingWaypoint + directionRandInv, _myPosition);
				_offsetDirection = dirDist < dirDistInv ? directionRand : directionRandInv;

			} else {

				var upAtTarget = Vector3D.Normalize(_pendingWaypoint - _currentPlanet.PositionComp.WorldAABB.Center);
				var perpendicularDir = VectorHelper.RandomPerpendicular(upAtTarget);

				if (Vector3D.Distance(perpendicularDir * -1, _myPosition) < Vector3D.Distance(perpendicularDir, _myPosition))
					perpendicularDir *= -1;

				_offsetMatrix = MatrixD.CreateWorld(_pendingWaypoint, perpendicularDir, upAtTarget);
				var roughCoords = perpendicularDir * distance + _pendingWaypoint;
				var roughCoordsSurface = VectorHelper.GetPlanetSurfaceCoordsAtPosition(roughCoords, _currentPlanet);
				var roughCoordsDistance = Vector3D.Distance(roughCoordsSurface, roughCoords);

				if (_offsetAltitudeIsMinimum) {

					if (roughCoordsDistance >= altitude) {

						_offsetDirection = perpendicularDir;
						_offsetDistance = distance;

					} else {

						var altitudeDiff = altitude - roughCoordsDistance;
						var newRoughCoords = Vector3D.Normalize(roughCoords - roughCoordsSurface) * altitudeDiff + roughCoords;
						_offsetDistance = Vector3D.Distance(newRoughCoords, _pendingWaypoint);
						_offsetDirection = Vector3D.Normalize(newRoughCoords - _pendingWaypoint);

					}

				} else {

					Vector3D newRoughCoords = Vector3D.Zero;

					if (roughCoordsDistance < MinimumPlanetAltitude || Vector3D.Distance(_currentPlanet.PositionComp.WorldAABB.Center, roughCoords) < Vector3D.Distance(_currentPlanet.PositionComp.WorldAABB.Center, roughCoordsSurface)) {

						newRoughCoords = Vector3D.Normalize(roughCoordsSurface - _currentPlanet.PositionComp.WorldAABB.Center) * MinimumPlanetAltitude + roughCoordsSurface;

					} else {

						newRoughCoords = Vector3D.Normalize(roughCoordsSurface - _currentPlanet.PositionComp.WorldAABB.Center) * altitude + roughCoords;
						

					}

					_offsetDistance = Vector3D.Distance(newRoughCoords, _pendingWaypoint);
					_offsetDirection = Vector3D.Normalize(newRoughCoords - _pendingWaypoint);

				}

			}

		}

		private void CalculateSafePlanetPathWaypoint(MyPlanet planet) {

			Vector3D directionToTarget = Vector3D.Normalize(_pendingWaypoint - _myPosition);
			double distanceToTarget = Vector3D.Distance(_pendingWaypoint, _myPosition);

			double requiredAltitude = _requiresClimbToIdealAltitude ? this.IdealPlanetAltitude : this.MinimumPlanetAltitude;
			Vector3D planetPosition = planet.PositionComp.WorldAABB.Center;

			Vector3D mySurfaceCoords = VectorHelper.GetPlanetSurfaceCoordsAtPosition(_myPosition, _currentPlanet);
			Vector3D waypointSurfaceCoords = VectorHelper.GetPlanetSurfaceCoordsAtPosition(_pendingWaypoint, _currentPlanet);

			double myAltitude = Vector3D.Distance(_myPosition, mySurfaceCoords);
			double waypointAltitude = Vector3D.Distance(_pendingWaypoint, waypointSurfaceCoords);

			double myCoreDistance = Vector3D.Distance(_myPosition, planetPosition);
			double waypointCoreDistance = Vector3D.Distance(_pendingWaypoint, planetPosition);

			List<Vector3D> stepsList = GetPlanetPathSteps(_myPosition, directionToTarget, distanceToTarget);

			Vector3D highestTerrainPoint = Vector3D.Zero;
			double highestTerrainCoreDistance = 0;

			foreach (Vector3D pathPoint in stepsList) {

				Vector3D surfacePathPoint = VectorHelper.GetPlanetSurfaceCoordsAtPosition(pathPoint, _currentPlanet);
				double surfaceCoreDistance = Vector3D.Distance(surfacePathPoint, planetPosition);

				if (surfaceCoreDistance >= highestTerrainCoreDistance) {

					highestTerrainPoint = surfacePathPoint;
					highestTerrainCoreDistance = surfaceCoreDistance;

				}

			}

			double myAltitudeDifferenceFromHighestTerrain = myCoreDistance - highestTerrainCoreDistance;
			double waypointAltitudeDifferenceFromHighestTerrain = waypointCoreDistance - highestTerrainCoreDistance;

			//Terrain Higher Than Me
			if (myAltitudeDifferenceFromHighestTerrain < this.MinimumPlanetAltitude) {

				//Logger.MsgDebug("Planet Pathing: Terrain Higher Than NPC", DebugTypeEnum.Dev);
				_requiresClimbToIdealAltitude = true;
				_pendingWaypoint = GetCoordsAboveHighestTerrain(planetPosition, directionToTarget, highestTerrainCoreDistance);
				_calculatedPlanetPathWaypoint = _pendingWaypoint;
				return;

			}

			//Check if Climb is still required
			if (_requiresClimbToIdealAltitude) {

				if (CheckAltitudeTolerance(myAltitudeDifferenceFromHighestTerrain, this.IdealPlanetAltitude, this.AltitudeTolerance)) {

					_requiresClimbToIdealAltitude = false;

				} else {

					_pendingWaypoint = GetCoordsAboveHighestTerrain(planetPosition, directionToTarget, highestTerrainCoreDistance);
					_calculatedPlanetPathWaypoint = _pendingWaypoint;
					return;

				}

			}

			//No Obstruction Case
			if (waypointAltitudeDifferenceFromHighestTerrain >= this.MinimumPlanetAltitude) {

				Logger.MsgDebug("Planet Pathing: No Obstruction", DebugTypeEnum.AutoPilot);
				_calculatedPlanetPathWaypoint = _pendingWaypoint;
				return;

			}

			//Terrain Higher Than NPC
			Vector3D waypointCoreDirection = Vector3D.Normalize(_pendingWaypoint - planetPosition);
			_pendingWaypoint = waypointCoreDirection * (highestTerrainCoreDistance + waypointAltitude) + planetPosition;
			_calculatedPlanetPathWaypoint = _pendingWaypoint;
			Logger.MsgDebug("Planet Pathing: Terrain Higher Than Target " + waypointAltitudeDifferenceFromHighestTerrain.ToString(), DebugTypeEnum.AutoPilot); ;

		}

		private Vector3D GetCoordsAboveHighestTerrain(Vector3D planetPosition, Vector3D directionToTarget, double highestTerrainDistanceFromCore) {

			//Get position 50m in direction of target
			var roughForwardStep = directionToTarget * 50 + _myPosition;

			var upDirectionFromStep = Vector3D.Normalize(roughForwardStep - planetPosition);
			return upDirectionFromStep * (highestTerrainDistanceFromCore + this.IdealPlanetAltitude) + planetPosition;

		}

		public List<Vector3D> GetPlanetPathSteps(Vector3D startCoords, Vector3D directionToTarget, double distanceToTarget, bool overrideMaxDistance = false) {

			var distanceToUse = MathHelper.Clamp(distanceToTarget, 0, overrideMaxDistance ? distanceToTarget : this.MaxPlanetPathCheckDistance);
			var result = new List<Vector3D>();
			double currentPathDistance = 0;

			while (currentPathDistance < distanceToUse) {

				if ((distanceToUse - currentPathDistance) < 50) {

					currentPathDistance = distanceToUse;

				} else {

					currentPathDistance += 50;

				}

				result.Add(directionToTarget * currentPathDistance + startCoords);

			}

			return result;

		}

		private bool CheckAltitudeTolerance(double currentCoreDistance, double targetCoreDistance, double tolerance) {

			if (currentCoreDistance < targetCoreDistance - tolerance || currentCoreDistance > targetCoreDistance + tolerance)
				return false;

			return true;

		}

		public void SetOffsetNone() {

			_offsetType = WaypointOffsetType.None;

		}

		public void SetOffsetDirectionFromTarget(double distance) {

			_offsetType = WaypointOffsetType.DistanceFromTarget;
			_offsetDistance = distance;

		}

		public void SetRandomOffset(IMyEntity entity = null, bool altitudeIsMinimum = false) {

			double distance = 0;
			double altitude = 0;

			if (_gravityStrength > 0) {

				distance = VectorHelper.RandomDistance(this.OffsetPlanetMinDistFromTarget, this.OffsetPlanetMaxDistFromTarget);
				altitude = VectorHelper.RandomDistance(this.OffsetPlanetMinTargetAltitude, this.OffsetPlanetMaxTargetAltitude);

			} else {

				distance = VectorHelper.RandomDistance(this.OffsetSpaceMinDistFromTarget, this.OffsetSpaceMaxDistFromTarget);

			}

			SetRandomOffset(distance, altitude, entity, altitudeIsMinimum);

		}

		public void SetRandomOffset(double distance, double altitude, IMyEntity entity, bool altitudeIsMinimum = false) {

			_offsetType = (entity == null) ? WaypointOffsetType.RandomOffsetFixed : WaypointOffsetType.RandomOffsetRelativeEntity;
			_offsetAltitudeIsMinimum = altitudeIsMinimum;
			_offsetRelativeEntity = entity;
			_offsetRequiresCalculation = true;
			_offsetDistanceFromTarget = distance;
			_offsetAltitudeFromTarget = altitude;

		}

		public void ReverseOffsetDirection(double minSafeAngle = 80) {

			if (InGravity()) {

				if (VectorHelper.GetAngleBetweenDirections(-_offsetDirection, _upDirection) < minSafeAngle)
					return;
			
			}

			_offsetDirection *= -1;


		}

		public bool InvalidTarget() {

			return !Targeting.HasTarget();

		}

		public Vector3D GetCurrentWaypoint() {

			return _currentWaypoint;
		
		}

		public MyPlanet GetCurrentPlanet() {

			return _currentPlanet;
		
		}

		public bool IsAvoidingCollision() {

			return _requiresNavigationAroundCollision;
		
		}

		public bool IsWaypointThroughVelocityCollision(int timeToCollision = -1, CollisionType type = CollisionType.None) {

			if (!Collision.VelocityResult.CollisionImminent(timeToCollision) || Collision.VelocityResult.Type == CollisionType.None)
				return false;

			if (Collision.VelocityResult.Type != type && type != CollisionType.None)
				return false;

			if (DistanceToCurrentWaypoint > Collision.VelocityResult.GetCollisionDistance() && VectorHelper.GetAngleBetweenDirections(Collision.VelocityResult.DirectionVector, Vector3D.Normalize(_currentWaypoint - _myPosition)) < 15)
				return true;

			return false;
		
		}

		public void DebugDrawingToWaypoints() {

			Vector4 colorRed = new Vector4(1, 0, 0, 1);
			Vector4 colorOrange = new Vector4(1, 0.5f, 0, 1);
			Vector4 colorYellow = new Vector4(1, 1, 0, 1);
			Vector4 colorGreen = new Vector4(0, 1, 0, 1);
			Vector4 colorCyan = new Vector4(0, 1, 1, 1);
			Vector4 colorMajenta = new Vector4(1, 0, 1, 1);

			//MySimpleObjectDraw.DrawLine(_initialWaypoint, _offsetDirection * 5 + _initialWaypoint, MyStringId.GetOrCompute("WeaponLaser"), ref colorRed, 1.1f);

			if (_evadeWaypoint != Vector3D.Zero) {

				MySimpleObjectDraw.DrawLine(_myPosition, _evadeWaypoint, MyStringId.GetOrCompute("WeaponLaser"), ref colorRed, 1.1f);
				MySimpleObjectDraw.DrawLine(_evadeFromWaypoint, _evadeWaypoint, MyStringId.GetOrCompute("WeaponLaser"), ref colorOrange, 1.1f);

			}

			if (_currentWaypoint != Vector3D.Zero) {

				MySimpleObjectDraw.DrawLine(_myPosition, _currentWaypoint, MyStringId.GetOrCompute("WeaponLaser"), ref colorGreen, 1.1f);

			}

			if (_calculatedOffsetWaypoint != Vector3D.Zero) {

				MySimpleObjectDraw.DrawLine(_myPosition, _calculatedOffsetWaypoint, MyStringId.GetOrCompute("WeaponLaser"), ref colorCyan, 1.1f);

			}

			if (_calculatedPlanetPathWaypoint != Vector3D.Zero) {

				MySimpleObjectDraw.DrawLine(_myPosition, _calculatedPlanetPathWaypoint, MyStringId.GetOrCompute("WeaponLaser"), ref colorMajenta, 1.1f);

			}

			if (_calculatedWeaponPredictionWaypoint != Vector3D.Zero) {

				MySimpleObjectDraw.DrawLine(_myPosition, _calculatedWeaponPredictionWaypoint, MyStringId.GetOrCompute("WeaponLaser"), ref colorYellow, 1.1f);

			}

			//Collisions
			/*
			if (Collision.ForwardResult.Type != CollisionType.None) {

				MySimpleObjectDraw.DrawLine(_myPosition, Collision.ForwardResult.GetCollisionCoords(), MyStringId.GetOrCompute("WeaponLaser"), ref colorRed, 1.1f);

			}

			if (Collision.BackwardResult.Type != CollisionType.None) {

				MySimpleObjectDraw.DrawLine(_myPosition, Collision.BackwardResult.GetCollisionCoords(), MyStringId.GetOrCompute("WeaponLaser"), ref colorRed, 1.1f);

			}

			if (Collision.LeftResult.Type != CollisionType.None) {

				MySimpleObjectDraw.DrawLine(_myPosition, Collision.LeftResult.GetCollisionCoords(), MyStringId.GetOrCompute("WeaponLaser"), ref colorRed, 1.1f);

			}

			if (Collision.RightResult.Type != CollisionType.None) {

				MySimpleObjectDraw.DrawLine(_myPosition, Collision.RightResult.GetCollisionCoords(), MyStringId.GetOrCompute("WeaponLaser"), ref colorRed, 1.1f);

			}

			if (Collision.UpResult.Type != CollisionType.None) {

				MySimpleObjectDraw.DrawLine(_myPosition, Collision.UpResult.GetCollisionCoords(), MyStringId.GetOrCompute("WeaponLaser"), ref colorRed, 1.1f);

			}

			if (Collision.DownResult.Type != CollisionType.None) {

				MySimpleObjectDraw.DrawLine(_myPosition, Collision.DownResult.GetCollisionCoords(), MyStringId.GetOrCompute("WeaponLaser"), ref colorRed, 1.1f);

			}
			*/
		}

		public void DebugDisplayCoords() {

			Logger.MsgDebug("Initial Waypoint: ");
		
		}

	}

}
