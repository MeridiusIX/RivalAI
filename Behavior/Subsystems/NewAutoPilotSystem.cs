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

namespace RivalAI.Behavior.Subsystems {

	public enum AutoPilotType {

		None,
		CargoShip,
		Legacy,
		RivalAI,

	}
	public class NewAutoPilotSystem {

		//Planet Config
		public double MaxPlanetPathCheckDistance;
		public double IdealPlanetAltitude;
		public double MinimumPlanetAltitude;

		//Non-Configurable
		private IMyRemoteControl _remoteControl;

		private AutoPilotType _currentAutoPilot;
		private OldAutoPilot _oldAutoPilot;
		private NewAutoPilot _newAutoPilot;

		private bool _autopilotOverride;
		private bool _strafeOverride;
		private bool _rollOverride;

		private bool _getInitialWaypointFromTarget;
		private bool _calculateOffset;
		private bool _calculateSafePlanetPath;


		private Vector3D _myPosition; //
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
		private double _currentAltitude;

		//PlanetData - Waypoint
		private double _highestTerrainToWaypoint;

		private const double PLANET_PATH_CHECK_DISTANCE = 1000;
		private const double PLANET_PATH_CHECK_INCREMENT = 50;

		public NewAutoPilotSystem() {

			MaxPlanetPathCheckDistance = 1000;

			_currentAutoPilot = AutoPilotType.None;
			_oldAutoPilot = null; //Fix Later
			_newAutoPilot = null; //Fix Later

			_autopilotOverride = false;
			_strafeOverride = false;
			_rollOverride = false;

			_getInitialWaypointFromTarget = false;
			_calculateOffset = false;
			_calculateSafePlanetPath = false;

			_myPosition = Vector3D.Zero;
			_initialWaypoint = Vector3D.Zero;
			_pendingWaypoint = Vector3D.Zero;
			_currentWaypoint = Vector3D.Zero;

			_requiresClimbToIdealAltitude = false;

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

		}

		private void CalculateCurrentWaypoint() {

			if (_getInitialWaypointFromTarget)
				//TODO: Move This Out Of Parallel
				//TODO: Get Last or Current Waypoint From TargetEvaluation

			//TODO: Verify if we are on a planet, and get data from it
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

				CalculateSafePlanetPathWaypoint();

			}

		}

		private void CalculateSafePlanetPathWaypoint() {

			var directionToTarget = Vector3D.Normalize(_pendingWaypoint - _myPosition);
			double requiredAltitude = _requiresClimbToIdealAltitude ? this.IdealPlanetAltitude : this.MinimumPlanetAltitude;


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

		public void SetAutoPilotMode(AutoPilotType type, bool useTargetPosition = false) {

			DisableAutoPilot();
			_getInitialWaypointFromTarget = useTargetPosition;

		}

		public void DisableAutoPilot() {



		}

	}
}
