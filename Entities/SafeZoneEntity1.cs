using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.ModAPI;
using VRageMath;
using Sandbox.Common.ObjectBuilders;

namespace RivalAI.Entities {
	public class SafeZoneEntity : EntityBase {

		public MySafeZone SafeZone;

		public SafeZoneEntity(IMyEntity entity) : base(entity) {
		
			
		
		}

		public bool InZone(Vector3D coords) {

			if (SafeZone.Shape == MySafeZoneShape.Sphere) {

				var newSphere = new BoundingSphereD(SafeZone.PositionComp.WorldAABB.Center, SafeZone.Radius);

				if (newSphere.Contains(coords) == ContainmentType.Contains)
					return true;

			} else {

				if (SafeZone.PositionComp.WorldAABB.Contains(coords) == ContainmentType.Contains)
					return true;
			
			}

			return false;

		}

	}
}
