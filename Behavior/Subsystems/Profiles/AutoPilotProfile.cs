using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace RivalAI.Behavior.Subsystems.Profiles {
	public class AutoPilotBase {

		//Configurable - General
		public double DesiredMaxSpeed;

		//Configurable - Planet Height Offseting
		public double MinimumAltitudeAboveTarget;
		public double MinimumAltitudeAboveWaypoint;
		public double IdealTravelAltitudeAboveTarget;
		public double IdealTravelAltitudeAboveWaypoint;

		//Configurable - WaypointOffset
		public double RandomOffsetDistanceFromTarget;
		public double RandomOffsetDistanceFromWaypoint;
		public int OffsetRefreshTime;

		//Configurable - KeenAutoPilot
		public bool CollisionAvoidance;
		public bool PrecisionMode;

		//Configurable - NewAutoPilot
		public double EngageThrustWithinAngle;

		//Non-Configurable - Blocks
		public IMyRemoteControl RemoteControl;

		//Non-Configurable - Object References
		private CollisionSystem _collision;
		private RotationSystem _rotation;
		private TargetingSystem _targeting;
		private ThrustSystem _thrust;
		private WeaponsSystem _weapons;

		//Non-Configurable - Offset

		//Non-Configurable - Waypoints
		public Vector3D InitialWaypoint;
		public Vector3D CurrentWaypoint;


	}
}
