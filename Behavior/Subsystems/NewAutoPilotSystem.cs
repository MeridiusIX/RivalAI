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

		//Non-Configurable
		private IMyRemoteControl _remoteControl;

		private AutoPilotType _currentAutoPilot;
		private bool _autopilotOverride;
		private bool _strafeOverride;
		private bool _rollOverride;

		//New AutoPilot
		private NewAutoPilotMode _newAutoPilotMode;
		private ThrustSystem _thrust;
		private RotationSystem _rotation;
		private TargetingSystem _targeting;

		private bool _getInitialWaypointFromTarget;
		private bool _calculateOffset;
		private bool _calculateSafePlanetPath;

		private Vector3D _myPosition; //
		private Vector3D _previousWaypoint;
		private Vector3D _initialWaypoint; //A static waypoint or derived from last valid target position.
		private Vector3D _pendingWaypoint; //Gets calculated in parallel.
		private Vector3D _currentWaypoint; //Actual position being travelled to.

		private bool _requiresClimbToIdealAltitude;

		//PlanetData - Self
		private MyPlanet _currentPlanet;
		private Vector3D _upDirection;
		private double _gravityStrength;
		private double _surfaceDistance;
		private float _airDensity;

		private const double PLANET_PATH_CHECK_DISTANCE = 1000;
		private const double PLANET_PATH_CHECK_INCREMENT = 50;

		public NewAutoPilotSystem(IMyRemoteControl remoteControl = null) {

			IdealMaxSpeed = 100;

			MaxPlanetPathCheckDistance = 1000;
			IdealPlanetAltitude = 200;
			MinimumPlanetAltitude = 110;
			AltitudeTolerance = 10;
			WaypointTolerance = 10;

			_currentAutoPilot = AutoPilotType.None;
			_autopilotOverride = false;
			_strafeOverride = false;
			_rollOverride = false;

			_newAutoPilotMode = NewAutoPilotMode.None;

			_getInitialWaypointFromTarget = false;
			_calculateOffset = false;
			_calculateSafePlanetPath = false;

			_myPosition = Vector3D.Zero;
			_previousWaypoint = Vector3D.Zero;
			_initialWaypoint = Vector3D.Zero;
			_pendingWaypoint = Vector3D.Zero;
			_currentWaypoint = Vector3D.Zero;

			_requiresClimbToIdealAltitude = false;

			_currentPlanet = null;
			_upDirection = Vector3D.Zero;
			_gravityStrength = 0;
			_surfaceDistance = 0;
			_airDensity = 0;

			if (remoteControl != null && MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid)) {

				_remoteControl = remoteControl;
				_rotation = new RotationSystem(_remoteControl);
				_thrust = new ThrustSystem(_remoteControl);

			}

		}

		public void SetupReferences(CollisionSystem collision, TargetingSystem targeting, WeaponsSystem weapons) {

			_thrust.SetupReferences(collision);

		}

		public void UpdateAutoPilot() {

			_myPosition = _remoteControl.GetPosition();

			if (_autopilotOverride) {

				//TODO: Check Roll and Strafe Timers

				if (!_strafeOverride && !_rollOverride) {

					_autopilotOverride = false;

				}
					

				if (_autopilotOverride)
					return;

			}

			if (_currentAutoPilot == AutoPilotType.None)
				return;

			_previousWaypoint = _currentWaypoint;

			if (_getInitialWaypointFromTarget) {


				//TODO: Get Last or Current Waypoint From TargetEvaluation

			}

			MyAPIGateway.Parallel.Start(CalculateCurrentWaypoint, () => {

				MyAPIGateway.Utilities.InvokeOnGameThread(() => {

					_currentWaypoint = _pendingWaypoint;

					if (_currentWaypoint == Vector3D.Zero)
						return;

					if (_currentAutoPilot == AutoPilotType.Legacy)
						UpdateLegacyAutoPilot();

					if (_currentAutoPilot == AutoPilotType.RivalAI)
						UpdateNewAutoPilot();

				});
			
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

			if (_newAutoPilotMode.HasFlag(NewAutoPilotMode.RotateToWaypoint)) {

				_rotation.StartCalculation(_currentWaypoint, _remoteControl, _upDirection);
			
			}

			if (_newAutoPilotMode.HasFlag(NewAutoPilotMode.Strafe)) {

				_thrust.ProcessStrafing();

			}

			if (_newAutoPilotMode.HasFlag(NewAutoPilotMode.ThrustForward)) {

				_thrust.ProcessForwardThrust();

			}

		}

		public void ActivateAutoPilot(AutoPilotType type, NewAutoPilotMode newType, Vector3D initialWaypoint, bool getWaypointFromTarget = false, bool useWaypointOffset = false, bool useSafePlanetPathing = false) {

			DeactivateAutoPilot();
			_currentAutoPilot = type;
			_newAutoPilotMode = newType;
			_initialWaypoint = initialWaypoint;
			_getInitialWaypointFromTarget = getWaypointFromTarget;
			_calculateOffset = useWaypointOffset;
			_calculateSafePlanetPath = useSafePlanetPathing;
			_remoteControl.SpeedLimit = IdealMaxSpeed;
			UpdateAutoPilot();

		}

		public void DeactivateAutoPilot() {

			_currentAutoPilot = AutoPilotType.None;
			_newAutoPilotMode = NewAutoPilotMode.None;
			_remoteControl.SetAutoPilotEnabled(false);
			_rotation.StopAllRotation();
			_thrust.StopAllThrust();
		
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

			//Offset

			//PlanetPathing
			if (_calculateSafePlanetPath && _gravityStrength > 0) {

				CalculateSafePlanetPathWaypoint(_currentPlanet);

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
			double waypointCoreDistance = Vector3D.Distance(waypointSurfaceCoords, planetPosition);

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

				return;

			}

			//Terrain Higher Than NPC
			Vector3D waypointCoreDirection = Vector3D.Normalize(_pendingWaypoint - planetPosition);
			_pendingWaypoint = waypointCoreDirection * (highestTerrainCoreDistance + this.IdealPlanetAltitude) + planetPosition;
			

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

	}

}
