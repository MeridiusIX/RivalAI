using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Entities {
	public class PlanetEntity : EntityBase {

		public MyPlanet Planet;

		public bool HasAtmosphere;
		public bool HasGravity;
		public bool HasOxygen;

		public MyGravityProviderComponent Gravity;

		
		public PlanetEntity(IMyEntity entity) : base(entity) {

			Planet = entity as MyPlanet;
			HasGravity = entity.Components.TryGet<MyGravityProviderComponent>(out Gravity);
			HasAtmosphere = Planet.HasAtmosphere;
			HasOxygen = Planet.GetOxygenForPosition(GetPosition()) > 0 ? true : false;

		}

		public bool IsPositionInRange(Vector3D coords) {

			if (Planet.PositionComp.WorldAABB.Contains(coords) == ContainmentType.Contains)
				return true;

			return false;
		
		}

	}
}
