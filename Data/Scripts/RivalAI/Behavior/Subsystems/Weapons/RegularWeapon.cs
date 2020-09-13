using RivalAI.Helpers;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.ModAPI;
using VRage.Game;
using VRageMath;
using Sandbox.Game.Gui;
using RivalAI.Behavior.Subsystems.AutoPilot;

namespace RivalAI.Behavior.Subsystems.Weapons {
	public class RegularWeapon : BaseWeapon, IWeapon{

		internal IMyUserControllableGun _staticGun;
		internal IMyLargeTurretBase _turret;

		internal IMyGunObject<MyGunBase> _gunBase;
		internal MyWeaponDefinition _weaponDefinition;

		public RegularWeapon(IMyTerminalBlock block, IMyRemoteControl remoteControl, IBehavior behavior) : base(block, remoteControl, behavior) {

			if (!_isValid)
				return;

			GetWeaponDefinition(_block);

			if (_weaponDefinition == null) {

				Logger.MsgDebug(_block.CustomName + " Weapon Definition Not Found", DebugTypeEnum.WeaponSetup);
				_isValid = false;
				return;

			}

			foreach (var ammoData in _weaponDefinition.WeaponAmmoDatas) {

				if (ammoData == null)
					continue;

				if (ammoData.RateOfFire > 0)
					_rateOfFire = ammoData.RateOfFire;

			}

			if (_block as IMyLargeTurretBase != null) {

				_isTurret = true;
				_turret = _block as IMyLargeTurretBase;
				_gunBase = _turret as IMyGunObject<MyGunBase>;

			} else if (_block as IMyUserControllableGun != null) {

				_isStatic = true;
				_staticGun = _block as IMyUserControllableGun;
				_gunBase = _staticGun as IMyGunObject<MyGunBase>;

			} else {

				Logger.MsgDebug(_block.CustomName + " Is Neither Gun Or Turret?", DebugTypeEnum.WeaponSetup);
				_isValid = false;
				return;
			
			}

			//Get Rate of Fire
			//Determine If Barrage Capable
		
		}

		public virtual void GetWeaponDefinition(IMyTerminalBlock weapon) {

			MyWeaponBlockDefinition weaponBlockDef;

			if (!Utilities.WeaponBlockReferences.TryGetValue(weapon.SlimBlock.BlockDefinition.Id, out weaponBlockDef)) {

				Logger.MsgDebug("No Weapon Definition Found For: " + weapon.SlimBlock.BlockDefinition.Id.ToString(), DebugTypeEnum.Weapon);

			}

			MyWeaponDefinition weaponDef;

			if (!MyDefinitionManager.Static.TryGetWeaponDefinition(weaponBlockDef.WeaponDefinitionId, out weaponDef)) {

				Logger.MsgDebug("Weapon Def Null", DebugTypeEnum.Weapon);
				return;

			}

			_weaponDefinition = weaponDef;

		}

		public bool HasAmmo() {

			bool gotAmmoDetails = false;

			if (_currentAmmoMagazine == new MyDefinitionId()) {

				gotAmmoDetails = true;
				GetAmmoDetails();

			}

			if (MyAPIGateway.Session.CreativeMode)
				return true;

			if (_inventory == null || _gunBase == null)
				return false;

			if (_inventory.Empty() && _gunBase.GunBase.CurrentAmmo == 0) {

				if (_weaponSystem.UseAmmoReplenish && _ammoRefills < _weaponSystem.MaxAmmoReplenishments) {

					_pendingAmmoRefill = true;

				} else {

					return false;

				}
	
			}

			_ammoAmount = _gunBase.GunBase.CurrentAmmo;

			if(!gotAmmoDetails)
				GetAmmoDetails();

			return true;

		}

		private void GetAmmoDetails() {

			if (_inventory == null || _gunBase == null)
				return;

			//Set Ammo Data
			if (_gunBase.GunBase.CurrentAmmoMagazineId != _currentAmmoMagazine && _gunBase.GunBase.CurrentAmmoDefinition != null) {

				_currentAmmoMagazine = _gunBase.GunBase.CurrentAmmoMagazineId;
				var ammoDef = _gunBase.GunBase.CurrentAmmoDefinition;
				_ammoMaxTrajectory = ammoDef.MaxTrajectory;


				if (ammoDef as MyMissileAmmoDefinition != null) {

					var missileAmmo = ammoDef as MyMissileAmmoDefinition;
					_ammoInitialVelocity = missileAmmo.MissileSkipAcceleration ? _ammoMaxTrajectory : missileAmmo.MissileInitialSpeed;
					_ammoAcceleration = missileAmmo.MissileSkipAcceleration ? 0 : missileAmmo.MissileAcceleration;

				} else {

					_ammoInitialVelocity = _ammoMaxVelocity;
					_ammoAcceleration = 0;

				}

			}

		}

		private void StaticWeaponReadiness() {

			_readyToFire = StaticWeaponAlignedToTarget();

		}


		//-------------------------------------------------------
		//------------START INTERFACE METHODS--------------------
		//-------------------------------------------------------

		//--Inherited From Base--
		//IsActive()
		//IsValid()

		public IMyEntity CurrentTarget() {

			if (_isTurret) {

				return _turret.HasTarget ? _turret.Target : null;
			
			}

			return null;
		
		}

		public void DetermineWeaponReadiness() {

			//Logger.MsgDebug(string.Format("{0} Weapon Ready Check", _block?.CustomName ?? "N/A"), DebugTypeEnum.Weapon);
			_readyToFire = true;

			//Valid
			if (!IsValid() || !IsActive()) {

				//Logger.MsgDebug(" - Not Valid or Ready", DebugTypeEnum.Weapon);
				_readyToFire = false;
				return;

			}

			//Ammo
			if (!HasAmmo()) {

				//Logger.MsgDebug(" - Bad Ammo Result", DebugTypeEnum.Weapon);
				_readyToFire = false;
				return;
			
			}

			if (_isStatic) {

				StaticWeaponReadiness();

			}

			//Logger.MsgDebug(string.Format("{0} Weapon Ready Result: {1}", _block?.CustomName, _readyToFire), DebugTypeEnum.Weapon);

		}

		public void FireOnce() {

			if (_isTurret)
				return;

			if (_isValid && IsActive() && _readyToFire) {

				Logger.MsgDebug(_block.CustomName + " Fire Once", DebugTypeEnum.Weapon);
				_staticGun.ApplyAction("ShootOnce");

			}

		}

		public override bool IsBarrageWeapon() {

			if (!_checkBarrageWeapon) {

				_checkBarrageWeapon = true;

				if (_isStatic && _weaponSystem.UseBarrageFire && _weaponDefinition != null) {

					_isBarrageWeapon = _rateOfFire < _weaponSystem.MaxFireRateForBarrageWeapons;

				}

			}

			return _isBarrageWeapon;

		}

		

		public void SetTarget(IMyEntity entity) {

			if (!IsValid() || _isStatic)
				return;

			_turret.TrackTarget(entity);

		}

		public void ToggleFire() {

			if (_isTurret)
				return;

			if (_isValid && IsActive() && _readyToFire && !_isBarrageWeapon) {

				if (!_firing) {

					Logger.MsgDebug(_block.CustomName + " Start Fire", DebugTypeEnum.Weapon);
					_firing = true;
					_staticGun.ApplyAction("Shoot_On");

				}

			} else {

				if (_firing) {

					Logger.MsgDebug(_block.CustomName + " End Fire", DebugTypeEnum.Weapon);
					_firing = false;
					_staticGun.ApplyAction("Shoot_Off");

				}

			}

		}


	}
}
