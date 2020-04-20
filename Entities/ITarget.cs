using System;
using System.Collections.Generic;
using System.Text;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Entities {
	public interface ITarget {
		
		bool ActiveEntity(); 
		double BroadcastRange(bool onlyAntenna = false);
		double CurrentAltitude();
		double CurrentGravity();
		double CurrentSpeed();
		Vector3D CurrentVelocity();
		double Distance(Vector3D coords);
		IMyEntity GetEntity();
		IMyEntity GetParentEntity();
		Vector3D GetPosition();
		bool InSafeZone();
		bool IsClosed();
		bool IsNpcOwned();
		bool IsPowered();
		bool IsUnowned();
		Vector2 PowerOutput(); // Current/Max
		bool ProtectedByShields();
		void RefreshSubGrids();
		int Reputation(long owner);
		float TargetValue();
		bool ValidEntity();
		int WeaponCount();

	}

}
