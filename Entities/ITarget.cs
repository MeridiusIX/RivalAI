﻿using System;
using System.Collections.Generic;
using System.Text;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Entities {
	public interface ITarget {
		
		bool ActiveEntity(); 
		double BroadcastRange(bool onlyAntenna = false);
		Vector3D CurrentAcceleration();
		double CurrentAltitude();
		double CurrentGravity();
		double CurrentSpeed();
		Vector3D CurrentVelocity();
		double Distance(Vector3D coords);
		string FactionOwner();
		IMyEntity GetEntity();
		long GetEntityId();
		List<long> GetOwners(bool onlyGetCurrentEntity = false, bool includeMinorityOwners = false);
		IMyEntity GetParentEntity();
		Vector3D GetPosition();
		double MaxSpeed();
		string Name();
		bool InSafeZone();
		bool IsClosed();
		bool IsPowered();
		bool IsSameGrid(IMyEntity entity);
		bool IsStatic();
		int MovementScore();
		OwnerTypeEnum OwnerTypes(bool onlyGetCurrentEntity = false, bool includeMinorityOwners = false);
		bool PlayerControlled();
		Vector2 PowerOutput(); // Current/Max
		bool ProtectedByShields();
		void RefreshSubGrids();
		RelationTypeEnum RelationTypes(long owner, bool onlyGetCurrentEntity = false, bool includeMinorityOwners = false);
		float TargetValue();
		bool ValidEntity();
		int WeaponCount();

	}

}
