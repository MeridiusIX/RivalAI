using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Behavior.Subsystems.Profiles {
	interface IWeaponProfile {

		bool IsValid { get; }

		bool IsStatic { get; }

		bool IsForwardFacingWeapon { get; }

		bool IsTurret { get; }

		bool IsWeaponCore { get; }

		int CurrentAmmoCount { get; }

		double MaxAmmoTrajectory { get; }

		double MaxAmmoSpeed { get; }

		MyDefinitionId CurrentAmmoId { get; }

		double WeaponCurrentRange { get; }

		bool ReadyToFire { get; }

		IMyEntity CurrentTargetEntity();

		void ToggleEnabled(bool enabled);

		bool FireWeaponOnce();

		void ToggleFire(bool fireEnable);

		void SetCurrentTargetAndAllowedAngle(Vector3D coords, double angle, double distance, bool targetValid, IMyEntity entity = null);

		void RefreshAmmoDetails();

		bool WeaponIsFunctional();

		void DetermineWeaponReadiness();

		Vector3D GetWeaponPosition();

		void AssignWeaponSystemReference(NewWeaponSystem weaponSystem);

		bool IsReadyToFire(bool targetIsWaypoint, bool isBarrage = false);

		bool IsBarrageWeapon();

	}
}
