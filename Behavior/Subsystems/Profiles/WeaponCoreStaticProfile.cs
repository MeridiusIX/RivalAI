using RivalAI.Helpers;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using static RivalAI.Helpers.WcApiDef;

namespace RivalAI.Behavior.Subsystems.Profiles {
	public class WeaponCoreStaticProfile : WeaponCoreProfile, IWeaponProfile {

		public bool IsForwardFacingWeapon { get { return _isForwardFacingWeapon; } }
		public MyDefinitionId CurrentAmmoId { get { return GetCurrentAmmoMagazineId(); } }

		public WeaponCoreStaticProfile(IMyRemoteControl remoteControl, IMyTerminalBlock block, WeaponDefinition definition, int id) : base(remoteControl, block, definition, id) {

			if (!_weaponValid)
				return;

			IsStatic = true;

			if (_remoteControl.WorldMatrix.Forward == _weaponBlock.WorldMatrix.Forward)
				_isForwardFacingWeapon = true;

		}

		public bool FireWeaponOnce() {

			if (!_weaponSystem.UseBarrageFire || !WeaponIsFunctional() || !ReadyToFire)
				return false;

			ReloadWeapon();

			if (_weaponRateOfFire > _weaponSystem.MaxFireRateForBarrageWeapons)
				return false;

			//_staticWeaponBlock.ApplyAction("ShootOnce");

			return true;

		}

		public void ToggleFire(bool fireEnable) {

			if (!WeaponIsFunctional()) {

				Logger.MsgDebug("Non Functional Weapon", DebugTypeEnum.WeaponStaticCore);
				return;

			}
				
			ReloadWeapon();
			bool ready = IsReadyToFire(fireEnable);

			if (!_firing && ready) {

				Logger.MsgDebug("Start Fire " + _weaponBlock.CustomName, DebugTypeEnum.WeaponStaticCore);
				RAI_SessionCore.Instance.WeaponCore.ToggleWeaponFire(_weaponBlock, true, false, _weaponIndex);
				_firing = true;

			}

			if (_firing && !ready) {

				Logger.MsgDebug("Cease Fire " + _weaponBlock.CustomName, DebugTypeEnum.WeaponStaticCore);
				RAI_SessionCore.Instance.WeaponCore.ToggleWeaponFire(_weaponBlock, false, false, _weaponIndex);
				_firing = false;

			}
		
		}

		public void SetCurrentTargetAndAllowedAngle(Vector3D coords, double angle, double distance, bool validTarget, IMyEntity entity = null) {

			_currentTargetWaypoint = coords;
			_waypointIsTarget = validTarget;

			if (_currentTargetEntity != entity) {

				_currentTargetEntity = entity;
				RAI_SessionCore.Instance.WeaponCore.SetWeaponTarget(_weaponBlock, entity, _weaponIndex);

			}

		}

		public override IMyEntity CurrentTargetEntity() {

			return _currentTargetEntity;

		}

		public override void DetermineWeaponReadiness() {

			if (!_weaponSystem.UseStaticGuns) {

				_readyToFire = false;
				return;

			}

			RefreshAmmoDetails();

			_readyToFire = RAI_SessionCore.Instance.WeaponCore.IsWeaponReadyToFire(_weaponBlock, _weaponIndex, false, true);

			if (!_readyToFire && _ammoReloadPending) {

				_readyToFire = true;

			} else if(!_readyToFire) {

				Logger.MsgDebug(_weaponBlock.CustomName + " Ready: " + _readyToFire.ToString(), DebugTypeEnum.WeaponStaticCore);
				return;
			
			}

			_currentDistance = Vector3D.Distance(_weaponBlock.GetPosition(), _currentTargetWaypoint);
			_currentAngle = VectorHelper.GetAngleBetweenDirections(_weaponBlock.WorldMatrix.Forward, Vector3D.Normalize(_currentTargetWaypoint - _weaponBlock.GetPosition()));
			

			_angleCheckPassed = _currentAngle < _weaponSystem.WeaponMaxAngleFromTarget && _waypointIsTarget;
			_currentAmmoIsHoming = IsCurrentAmmoHoming();
			_currentAmmoIsBeam = IsCurrentAmmoLaser();

			if(!_angleCheckPassed && !_currentAmmoIsHoming)
				_readyToFire = false;

			//Logger.MsgDebug(_weaponBlock.CustomName + " Ready: " + _readyToFire.ToString(), DebugTypeEnum.WeaponStaticCore);

		}

		public override bool IsReadyToFire(bool targetIsWaypoint, bool isBarrage = false) {

			if (_currentDistance > _weaponCurrentRange) {

				Logger.MsgDebug(_weaponBlock.CustomName + " Not Ready: Out Of Range", DebugTypeEnum.WeaponStaticCore);
				return false;

			}
				

			if (!_currentAmmoIsHoming && !_currentAmmoIsBeam) {

				if ((isBarrage && !IsBarrageWeapon()) || (!isBarrage && IsBarrageWeapon())) {

					Logger.MsgDebug(_weaponBlock.CustomName + " Not Ready: Not Barrage Ready", DebugTypeEnum.WeaponStaticCore);
					return false;

				}


				if (_angleCheckPassed) {

					Logger.MsgDebug(_weaponBlock.CustomName + " Ready: Within Angle", DebugTypeEnum.WeaponStaticCore);
					return true;

				}
					
			}

			if (_currentAmmoIsHoming && _currentTargetEntity != null) {

				Logger.MsgDebug(_weaponBlock.CustomName + " Ready: Homing Has Target", DebugTypeEnum.WeaponStaticCore);
				return true;
			
			}

			if (_currentAmmoIsBeam) {

				if (_angleCheckPassed) {

					Logger.MsgDebug(_weaponBlock.CustomName + " Ready: Beam Within Angle", DebugTypeEnum.WeaponStaticCore);
					return true;

				}
	
			}

			Logger.MsgDebug(_weaponBlock.CustomName + " Not Ready: Out of angle or Other", DebugTypeEnum.WeaponStaticCore);
			return false;

		}

		public override bool IsBarrageWeapon() {

			if (!_checkBarrageWeapon) {

				_checkBarrageWeapon = true; //

				if (_weaponSystem.UseBarrageFire) {

					RefreshAmmoDetails();
					_isBarrage = _weaponRateOfFire < _weaponSystem.MaxFireRateForBarrageWeapons;

				}

			}

			return _isBarrage;

		}

	}

}
