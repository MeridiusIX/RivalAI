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
using Ingame = Sandbox.ModAPI.Ingame;
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
	
	}

	public enum PathCheckResult {
		
		Ok,
		TerrainHigherThanNpc,
		TerrainHigherThanWaypoint

	
	}
	public class NewAutoPilotSystem {

		//General Config
		public float IdealMaxSpeed;

		//Planet Config
		public double MaxPlanetPathCheckDistance;
		public double IdealPlanetAltitude;
		public double MinimumPlanetAltitude;
		public double AltitudeTolerance;
		public double WaypointTolerance;

		//Collision Config
		public double CollisionEvasionWaypointDistance;
		public double CollisionEvasionResumeDistance;
		public int CollisionEvasionResumeTime;

		//Non-Configurable
		private IMyRemoteControl _remoteControl;

		private AutoPilotType _currentAutoPilot;
		private bool _autopilotOverride;
		private bool _strafeOverride;
		private bool _rollOverride;

		//New AutoPilot
		public NewAutoPilotMode CurrentMode;
		public CollisionSystem Collision;
		private RotationSystem _rotation;
		public TargetingSystem Targeting;
		public ThrustSystem Thrust;
		public NewWeaponSystem Weapons;

		private bool _getInitialWaypointFromTarget;
		private bool _calculateOffset;
		private bool _avoidPotentialCollisions;
		private bool _calculateSafePlanetPath;

		private Vector3D _myPosition; //
		private Vector3D _previousWaypoint;
		private Vector3D _initialWaypoint; //A static waypoint or derived from last valid target position.
		private Vector3D _pendingWaypoint; //Gets calculated in parallel.
		private Vector3D _currentWaypoint; //Actual position being travelled to.
		private Vector3D _evadeWaypoint;
		private Vector3D _evadeFromWaypoint;
		private DateTime _evadeWaypointCreateTime;
		private bool _waypointIsOffset;
		private bool _waypointIsTarget;

		//Offset Stuff
		private WaypointOffsetType _offsetType;
		private double _offsetDistanceFromTarget;
		private Vector3D _offsetAmount;
		private MatrixD _offsetMaxtrix;
		private IMyEntity _offsetRelativeEntity;

		public double DistanceToInitialWaypoint;
		public double DistanceToCurrentWaypoint;

		private bool _requiresClimbToIdealAltitude;
		private bool _requiresNavigationAroundCollision;

		//PlanetData - Self
		private MyPlanet _currentPlanet;
		private Vector3D _upDirection;
		private double _gravityStrength;
		private double _surfaceDistance;
		private float _airDensity;

		public Action OnComplete; //After Autopilot is done everything, it starts a new task elsewhere.

		private const double PLANET_PATH_CHECK_DISTANCE = 1000;
		private const double PLANET_PATH_CHECK_INCREMENT = 50;

		public NewAutoPilotSystem(IMyRemoteControl remoteControl = null) {

			IdealMaxSpeed = 100;

			MaxPlanetPathCheckDistance = 1000;
			IdealPlanetAltitude = 200;
			MinimumPlanetAltitude = 110;
			AltitudeTolerance = 10;
			WaypointTolerance = 10;

			CollisionEvasionWaypointDistance = 150;
			CollisionEvasionResumeDistance = 25;
			CollisionEvasionResumeTime = 10;

			_currentAutoPilot = AutoPilotType.None;
			_autopilotOverride = false;
			_strafeOverride = false;
			_rollOverride = false;

			CurrentMode = NewAutoPilotMode.None;

			_getInitialWaypointFromTarget = false;
			_calculateOffset = false;
			_calculateSafePlanetPath = false;

			_myPosition = Vector3D.Zero;
			_previousWaypoint = Vector3D.Zero;
			_initialWaypoint = Vector3D.Zero;
			_pendingWaypoint = Vector3D.Zero;
			_currentWaypoint = Vector3D.Zero;
			_evadeWaypoint = Vector3D.Zero;
			_evadeFromWaypoint = Vector3D.Zero;
			_evadeWaypointCreateTime = DateTime.Now;
			_waypointIsTarget = false;

			_requiresClimbToIdealAltitude = false;
			_requiresNavigationAroundCollision = false;

			_currentPlanet = null;
			_upDirection = Vector3D.Zero;
			_gravityStrength = 0;
			_surfaceDistance = 0;
			_airDensity = 0;

			if (remoteControl != null && MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid)) {

				_remoteControl = remoteControl;
				Collision = new CollisionSystem(_remoteControl);
				_rotation = new RotationSystem(_remoteControl);
				Targeting = new TargetingSystem(_remoteControl);
				Thrust = new ThrustSystem(_remoteControl);
				Weapons = new NewWeaponSystem(_remoteControl);

			}

		}

		public void SetupReferences() {

			Thrust.SetupReferences(Collision);

		}

		public void InitTags() {

			//Do Stuff

			Targeting.InitTags();
			Weapons.InitTags();

		}

		public void StartCalculations() {

			if (Collision.RequestVelocityCheckCollisions()) {

				MyAPIGateway.Parallel.Start(Collision.CheckVelocityCollisionsThreaded, FinishCollisionChecking);

			} else {

				FinishCollisionChecking();

			}
				
		
		}

		private void FinishCollisionChecking() {

			MyAPIGateway.Utilities.InvokeOnGameThread(() => {

				Targeting.RequestTarget();
				MyAPIGateway.Parallel.Start(Targeting.RequestTargetParallel, FinishTargetChecking);

			});
		
		}

		private void FinishTargetChecking() {

			MyAPIGateway.Utilities.InvokeOnGameThread(() => {

				Logger.MsgDebug("Beginning Autopilot Pathing Calculations", DebugTypeEnum.Dev);
				ThreadedAutoPilotCalculations();


			});

		}

		private void ThreadedAutoPilotCalculations() {

			_myPosition = _remoteControl.GetPosition();

			if (_autopilotOverride) {

				//TODO: Check Roll and Strafe Timers

				if (!_strafeOverride && !_rollOverride) {

					_autopilotOverride = false;

				}

				if (_autopilotOverride) {

					FinishWaypointCalculations();
					return;

				}
					

			}

			if (_currentAutoPilot == AutoPilotType.None) {

				FinishWaypointCalculations();
				return;

			}
				

			_previousWaypoint = _currentWaypoint;

			if (_getInitialWaypointFromTarget) {

				if (!Targeting.InvalidTarget) {

					_waypointIsTarget = true;
					_initialWaypoint = Targeting.GetTargetPosition();

				} else {

					_waypointIsTarget = false;

				}	

			}

			MyAPIGateway.Parallel.Start(() => {

				try {

					CalculateCurrentWaypoint();

				} catch (Exception exc) {

					Logger.MsgDebug("Caught Exception While Calculating Autopilot Pathing", DebugTypeEnum.General);
					Logger.MsgDebug(exc.ToString(), DebugTypeEnum.General);

				}
			
			}, FinishWaypointCalculations);

		}

		private void FinishWaypointCalculations() {

			MyAPIGateway.Utilities.InvokeOnGameThread(() => {

				_currentWaypoint = _pendingWaypoint;
				this.DistanceToInitialWaypoint = Vector3D.Distance(_myPosition, _initialWaypoint);
				this.DistanceToCurrentWaypoint = Vector3D.Distance(_myPosition, _currentWaypoint);

				if (_currentAutoPilot == AutoPilotType.Legacy)
					UpdateLegacyAutoPilot();

				if (_currentAutoPilot == AutoPilotType.RivalAI)
					UpdateNewAutoPilot();

				if (_currentWaypoint == Vector3D.Zero)
					Logger.MsgDebug("No Current Waypoint", DebugTypeEnum.Dev);

				StartWeaponCalculations();

			});

		}

		private void StartWeaponCalculations() {

			Logger.MsgDebug("Checking Weapon Readiness", DebugTypeEnum.Dev);
			MyAPIGateway.Parallel.Start(Weapons.CheckWeaponReadiness, WeaponFinish);

		}

		private void WeaponFinish() {

			MyAPIGateway.Utilities.InvokeOnGameThread(() => {

				Logger.MsgDebug("Autopilot Done, Handing Off To Next Action", DebugTypeEnum.Dev);
				Weapons.FireWeapons();
				OnComplete?.Invoke(); //This is where Autopilot ends

			});
		
		}

		private void UpdateLegacyAutoPilot() {

			if (_remoteControl.IsAutoPilotEnabled && Vector3D.Distance(_previousWaypoint, _currentWaypoint) < this.WaypointTolerance)
				return;

			_remoteControl.SetAutoPilotEnabled(false);
			_remoteControl.ClearWaypoints();
			_remoteControl.AddWaypoint(_currentWaypoint, "Current Waypoint Target");
			_remoteControl.FlightMode = Ingame.FlightMode.OneWay;
			_remoteControl.SetAutoPilotEnabled(true);

		}

		private void UpdateNewAutoPilot() {

			if (CurrentMode.HasFlag(NewAutoPilotMode.RotateToWaypoint)) {

				_rotation.StartCalculation(_currentWaypoint, _remoteControl, _upDirection);
			
			}

			if (CurrentMode.HasFlag(NewAutoPilotMode.Strafe)) {

				Logger.MsgDebug("Process Strafe", DebugTypeEnum.Dev);
				Thrust.ProcessStrafing();

			}

			if (CurrentMode.HasFlag(NewAutoPilotMode.ThrustForward)) {

				Thrust.ProcessForwardThrust();

			}

		}

		public void ActivateAutoPilot(AutoPilotType type, NewAutoPilotMode newType, Vector3D initialWaypoint, bool getWaypointFromTarget = false, bool useWaypointOffset = false, bool useSafePlanetPathing = false, bool avoidCollisions = true) {

			DeactivateAutoPilot();
			_currentAutoPilot = type;
			CurrentMode = newType;
			_initialWaypoint = initialWaypoint;
			_getInitialWaypointFromTarget = getWaypointFromTarget;
			_calculateOffset = useWaypointOffset;
			_calculateSafePlanetPath = useSafePlanetPathing;
			_avoidPotentialCollisions = avoidCollisions;
			_remoteControl.SpeedLimit = IdealMaxSpeed;
			//ThreadedAutoPilotCalculations();

		}

		public void DeactivateAutoPilot() {

			_currentAutoPilot = AutoPilotType.None;
			CurrentMode = NewAutoPilotMode.None;
			_remoteControl.SetAutoPilotEnabled(false);
			_rotation.StopAllRotation();
			Thrust.StopAllThrust();
		
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

			if (_initialWaypoint == Vector3D.Zero)
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

			} else {

				_upDirection = Vector3D.Zero;
				_gravityStrength = 0;
				_airDensity = 0;

			}

			//Collision
			if (Collision.VelocityResult.CollisionImminent == true) {

				if ((_gravityStrength <= 0 && Collision.VelocityResult.Type == CollisionDetectType.Voxel) || Collision.VelocityResult.Type != CollisionDetectType.Voxel) {

					if (!_requiresNavigationAroundCollision) {

						CalculateEvadeCoords();
						_requiresNavigationAroundCollision = true;
						

					} else {

						var directionToOldCollision = Vector3D.Normalize(_evadeFromWaypoint - _myPosition);
						var directionToNewCollision = Vector3D.Normalize(Collision.VelocityResult.Coords - _myPosition);
						if(VectorHelper.GetAngleBetweenDirections(directionToOldCollision, directionToNewCollision) > 45)
							CalculateEvadeCoords();

					}

				}

			}

			if (_requiresNavigationAroundCollision) {

				var timeDiff = MyAPIGateway.Session.GameDateTime - _evadeWaypointCreateTime;

				if(Vector3D.Distance(_myPosition, _evadeWaypoint) > this.CollisionEvasionResumeDistance && timeDiff.TotalSeconds < this.CollisionEvasionResumeTime)
					return;

				_requiresNavigationAroundCollision = false;


			}

			//Offset

			//PlanetPathing
			if (_calculateSafePlanetPath && _gravityStrength > 0) {

				CalculateSafePlanetPathWaypoint(_currentPlanet);

				if (_initialWaypoint != _pendingWaypoint)
					_waypointIsTarget = false;

			}

			if (_initialWaypoint == _pendingWaypoint && Targeting.Target.IsMoving) {

				bool gotLead = false;

				if (Targeting.TargetData.UseCollisionLead && !gotLead) {

					gotLead = true;
					_pendingWaypoint = VectorHelper.FirstOrderIntercept(_myPosition, Targeting.Target.MyVelocity, (float)Targeting.Target.MyVelocity.Length(), _pendingWaypoint, Targeting.Target.TargetVelocity);

				}

				if (Targeting.TargetData.UseProjectileLead && !gotLead) {

					gotLead = true;
					_pendingWaypoint = VectorHelper.FirstOrderIntercept(_myPosition, Targeting.Target.MyVelocity, Weapons.MostCommonAmmoSpeed(), _pendingWaypoint, Targeting.Target.TargetVelocity);

				}
			
			}

		}

		private void CalculateEvadeCoords() {

			var dirFromCollision = Vector3D.Normalize(_myPosition - Collision.VelocityResult.Coords);

			for (int i = 0; i < 6; i++) {

				var randomPerpDir = MyUtils.GetRandomPerpendicularVector(ref dirFromCollision);
				Collision.CheckPotentialEvasionCollisionThreaded(randomPerpDir, this.CollisionEvasionWaypointDistance);

				if (FoundEvadeCoords(randomPerpDir))
					return;

				randomPerpDir *= -1;
				Collision.CheckPotentialEvasionCollisionThreaded(randomPerpDir, this.CollisionEvasionWaypointDistance);

				if (FoundEvadeCoords(randomPerpDir))
					return;

			}

			//If we got here, no evade coords could be calculated
			//GuessIllJustDie.jpg

		}

		private bool FoundEvadeCoords(Vector3D direction) {

			if (Collision.EvasionResult.Coords == Vector3D.Zero) {

				_requiresNavigationAroundCollision = true;
				_evadeWaypointCreateTime = MyAPIGateway.Session.GameDateTime;
				_evadeFromWaypoint = Collision.VelocityResult.Coords;
				_evadeWaypoint = direction * this.CollisionEvasionWaypointDistance + _myPosition;
				return true;

			}

			return false;

		}

		private void CalculateOffsetWaypoint() {

			_waypointIsOffset = false;

			if (_offsetType == WaypointOffsetType.None)
				return;

			if (_offsetType == WaypointOffsetType.DistanceFromTarget && _offsetDistanceFromTarget > 0) {

				_waypointIsOffset = true;
				var tempCoords = Vector3D.Normalize(_myPosition - _pendingWaypoint) * _offsetDistanceFromTarget + _pendingWaypoint;
				_pendingWaypoint = tempCoords;
				return;

			}

			if (_offsetAmount != Vector3D.Zero) {

				_waypointIsOffset = true;

				if (_offsetType == WaypointOffsetType.FixedMatrixRelativePosition && _offsetRelativeEntity?.PositionComp != null && MyAPIGateway.Entities.Exist(_offsetRelativeEntity)) {

					_offsetMaxtrix.Translation = _offsetRelativeEntity.PositionComp.WorldMatrix.Translation;
				
				}

				var tempCoords = Vector3D.Transform(_offsetAmount, _offsetMaxtrix);
				_pendingWaypoint = tempCoords;
				return;

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
				return;

			}

			//Check if Climb is still required
			if (_requiresClimbToIdealAltitude) {

				if (CheckAltitudeTolerance(myAltitudeDifferenceFromHighestTerrain, this.IdealPlanetAltitude, this.AltitudeTolerance)) {

					_requiresClimbToIdealAltitude = false;

				} else {

					_pendingWaypoint = GetCoordsAboveHighestTerrain(planetPosition, directionToTarget, highestTerrainCoreDistance);
					return;

				}
			
			}

			//No Obstruction Case
			if (waypointAltitudeDifferenceFromHighestTerrain >= this.MinimumPlanetAltitude) {

				Logger.MsgDebug("Planet Pathing: No Obstruction", DebugTypeEnum.AutoPilot);
				return;

			}

			//Terrain Higher Than NPC
			Vector3D waypointCoreDirection = Vector3D.Normalize(_pendingWaypoint - planetPosition);
			_pendingWaypoint = waypointCoreDirection * (highestTerrainCoreDistance + waypointAltitude) + planetPosition;
			Logger.MsgDebug("Planet Pathing: Terrain Higher Than Target " + waypointAltitudeDifferenceFromHighestTerrain.ToString(), DebugTypeEnum.AutoPilot); ;

		}

		private Vector3D GetCoordsAboveHighestTerrain(Vector3D planetPosition, Vector3D directionToTarget, double highestTerrainDistanceFromCore) {

			//Get position 50m in direction of target
			var roughForwardStep = directionToTarget * 50 + _myPosition;

			var upDirectionFromStep = Vector3D.Normalize(roughForwardStep - planetPosition);
			return upDirectionFromStep * (highestTerrainDistanceFromCore + this.IdealPlanetAltitude) + planetPosition;
		
		}

		private List<Vector3D> GetPlanetPathSteps(Vector3D startCoords, Vector3D directionToTarget, double distanceToTarget) {

			var distanceToUse = MathHelper.Clamp(distanceToTarget, 0, this.MaxPlanetPathCheckDistance);
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
			_offsetDistanceFromTarget = distance;

		}

		public void SetOffsetWithMatrix(MatrixD matrix, Vector3D offset, IMyEntity entity = null) {

			_offsetType = (entity == null) ? WaypointOffsetType.FixedMatrix : WaypointOffsetType.FixedMatrixRelativePosition;
			_offsetMaxtrix = matrix;
			_offsetAmount = offset;
			_offsetRelativeEntity = entity;

		}

		public bool InvalidTarget() {

			return Targeting.InvalidTarget;
		
		}

		public void DebugDrawingToWaypoints() {

			Vector4 colorRed = new Vector4(1, 0, 0, 1);
			Vector4 colorGreen = new Vector4(0, 1, 0, 1);
			Vector4 colorCyan = new Vector4(0, 1, 1, 1);
			Vector4 colorMajenta = new Vector4(1, 0, 1, 1);

			MySimpleObjectDraw.DrawLine(_myPosition, _initialWaypoint, MyStringId.GetOrCompute("WeaponLaser"), ref colorGreen, 2.1f);
			MySimpleObjectDraw.DrawLine(_myPosition, _currentWaypoint, MyStringId.GetOrCompute("WeaponLaser"), ref colorMajenta, 2.1f);

		}

	}

}
