using System;
using System.Collections.Generic;
using System.Text;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Behavior.Subsystems.Trigger {

	public enum WaypointType {
	
		None,
		Static,
		RelativeOffset,
		RelativeRandom,
	
	}

	public enum RelativeEntityType {
	
		None,
		Self,
		Target,
		Damager
	
	}

	public class WaypointProfile {

		public string ProfileSubtypeId;

		public WaypointType Waypoint;
		public RelativeEntityType RelativeEntity;

		public bool RelativeWaypointUpdatesWithEntity;
		public Vector3D Coordinates;

		public double MinDistance;
		public double MaxDistance;

		public double MinAltitude;
		public double MaxAltitude;
		public bool InheritRelativeAltitude;

		public WaypointProfile() {

			ProfileSubtypeId = "";

			Waypoint = WaypointType.None;
			RelativeEntity = RelativeEntityType.None;

			RelativeWaypointUpdatesWithEntity = false;
			Coordinates = Vector3D.Zero;

			MinDistance = 100;
			MaxDistance = 101;

			MinAltitude = 100;
			MaxAltitude = 101;
			InheritRelativeAltitude = false;

		}

		public EncounterWaypoint GenerateEncounterWaypoint(Vector3D coords, IMyEntity entity) {

			return null;
		
		}

	}

}
