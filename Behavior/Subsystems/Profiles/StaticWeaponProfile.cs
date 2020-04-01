using RivalAI.Helpers;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Behavior.Subsystems.Profiles {
	public class StaticWeaponProfile : WeaponProfile, IWeaponProfile {

		public bool IsForwardFacingWeapon { get { return _isForwardFacingWeapon; } }
		public MyDefinitionId CurrentAmmoId { get { return GetCurrentAmmoMagazineId(); } }

		private IMyUserControllableGun _staticWeaponBlock;
		private bool _isForwardFacingWeapon;

		public StaticWeaponProfile(IMyRemoteControl remoteControl, IMyTerminalBlock block) : base(remoteControl, block) {

			_staticWeaponBlock = block as IMyUserControllableGun;
			IsStatic = true;

			if (_staticWeaponBlock != null) {

				_gunBase = (IMyGunObject<MyGunBase>)_staticWeaponBlock;

			} else {

				_weaponValid = false;

			}

			if (!_weaponValid)
				return;

			if (_remoteControl.WorldMatrix.Forward == _staticWeaponBlock.WorldMatrix.Forward)
				_isForwardFacingWeapon = true;

			GetWeaponDefinition(_staticWeaponBlock);

		}

		public bool FireWeaponOnce() {

			if (!_weaponSystem.UseBarrageFire || !WeaponIsFunctional() || !ReadyToFire)
				return false;

			ReloadWeapon();

			if (_weaponRateOfFire > _weaponSystem.MaxFireRateForBarrageWeapons)
				return false;

			_staticWeaponBlock.ApplyAction("ShootOnce");

			return true;

		}

		public void ToggleFire(bool fireEnable) {

			if (!WeaponIsFunctional())
				return;

			ReloadWeapon();
			bool ready = IsReadyToFire(fireEnable);

			if (!_firing && ready) {

				_staticWeaponBlock.ApplyAction("Shoot_On");
				_firing = true;

			}

			if (_firing && !ready) {

				_staticWeaponBlock.ApplyAction("Shoot_Off");
				_firing = false;

			}
		
		}

		public void SetCurrentTargetAndAllowedAngle(Vector3D coords, double angle, double distance, bool validTarget, IMyEntity entity = null) {

			_currentTargetWaypoint = coords;
			_waypointIsTarget = validTarget;
			_currentTargetEntity = entity;

		}

		public override void RefreshAmmoDetails() {

			base.RefreshAmmoDetails();
			_weaponCurrentRange = _maxAmmoTrajectory;

		}

		public override IMyEntity CurrentTargetEntity() {

			return base.CurrentTargetEntity();

		}

		public override void DetermineWeaponReadiness() {

			base.DetermineWeaponReadiness();

			if (!_readyToFire)
				return;

			if (!_weaponSystem.UseStaticGuns) {

				_readyToFire = false;
				return;

			}

			_currentAngle = VectorHelper.GetAngleBetweenDirections(_staticWeaponBlock.WorldMatrix.Forward, Vector3D.Normalize(_currentTargetWaypoint - _staticWeaponBlock.GetPosition()));

			if (_currentAngle > _weaponSystem.WeaponMaxAngleFromTarget || !_waypointIsTarget) {

				//Logger.MsgDebug("Angle: " + (_currentAngle > _weaponSystem.WeaponMaxAngleFromTarget).ToString() + " - Waypoint Target: " + _waypointIsTarget.ToString(), DebugTypeEnum.Weapon);
				_readyToFire = false;
				return;

			}

			if (_currentDistance > _weaponCurrentRange) {

				_readyToFire = false;
				return;

			}
			
		}

		public override bool IsReadyToFire(bool targetIsWaypoint, bool isBarrage = false) {

			if (_currentDistance > _weaponCurrentRange || !targetIsWaypoint)
				return false;

			//XNOR
			//Logger.MsgDebug(isBarrage.ToString() + " - " + IsBarrageWeapon().ToString(), DebugTypeEnum.Weapon);
			if ((isBarrage && !IsBarrageWeapon()) || (!isBarrage && IsBarrageWeapon()))
				return false;

			if (!ReadyToFire)
				return false;

			return true;
		
		}

		public override bool IsBarrageWeapon() {

			if (!_checkBarrageWeapon) {

				_checkBarrageWeapon = true; //

				if (_weaponSystem.UseBarrageFire && _weaponDefinition != null) {

					foreach (var ammoData in _weaponDefinition.WeaponAmmoDatas) {

						if (ammoData == null)
							continue;

						if (ammoData.RateOfFire > 0)
							_weaponRateOfFire = ammoData.RateOfFire;

					}

					_isBarrage = _weaponRateOfFire < _weaponSystem.MaxFireRateForBarrageWeapons;

				}
			
			}

			return _isBarrage;

		}

	}

}
