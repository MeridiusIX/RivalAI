using RivalAI.Helpers;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;

namespace RivalAI.Behavior.Subsystems {
	public class NewWeaponProfile {

		private IMyRemoteControl _remoteControl;

		private bool _weaponValid;

		private IMyTerminalBlock _weaponBlock;
		private IMyUserControllableGun _staticWeaponBlock;
		private IMyLargeTurretBase _turretWeaponBlock;
		private IMyConveyorSorter _weaponCoreBlock;

		private WeaponType _weaponType;
		private bool _isStatic;
		private bool _isTurret;
		private bool _isWeaponCore;

		private IMyGunObject<MyGunBase> _gunBase;

		private bool _readyToFire;
		private bool _functional;
		private bool _checkIfAttachedToControllingGrid;
		private bool _firing;

		public NewWeaponProfile(IMyRemoteControl remoteControl, IMyTerminalBlock block) {

			_weaponBlock = block;

			if (block as IMyLargeTurretBase == null) {

				if (block as IMyUserControllableGun != null) {

					_staticWeaponBlock = block as IMyUserControllableGun;

				}

				if (block as IMyConveyorSorter != null && Utilities.AllWeaponCoreBlocks.Contains(block.SlimBlock.BlockDefinition.Id)) {

					_weaponCoreBlock = block as IMyConveyorSorter;

				}

			} else {

				_turretWeaponBlock = block as IMyLargeTurretBase;

			}

			if (_staticWeaponBlock != null) {

				_weaponType = WeaponType.StaticNormal;
				_isStatic = true;
				_weaponValid = true;
				_gunBase = (IMyGunObject<MyGunBase>)_staticWeaponBlock;

			}

			if (_turretWeaponBlock != null) {

				if (Utilities.AllWeaponCoreGuns.Contains(block.SlimBlock.BlockDefinition.Id)) {

					_weaponType = WeaponType.WeaponCoreStatic;
					_isStatic = true;
					_isWeaponCore = true;
					_weaponValid = true;


				} else if (Utilities.AllWeaponCoreTurrets.Contains(block.SlimBlock.BlockDefinition.Id)) {

					_weaponType = WeaponType.WeaponCoreTurret;
					_isTurret = true;
					_isWeaponCore = true;
					_weaponValid = true;

				} else {

					_weaponType = WeaponType.TurretNormal;
					_isTurret = true;
					_weaponValid = true;
					_gunBase = (IMyGunObject<MyGunBase>)_turretWeaponBlock;

				}

			}

			if (_weaponCoreBlock != null) {

				if (Utilities.AllWeaponCoreGuns.Contains(block.SlimBlock.BlockDefinition.Id)) {

					_weaponType = WeaponType.WeaponCoreSorterStatic;
					_isStatic = true;
					_isWeaponCore = true;
					_weaponValid = true;


				} else if (Utilities.AllWeaponCoreTurrets.Contains(block.SlimBlock.BlockDefinition.Id)) {

					_weaponType = WeaponType.WeaponCoreSorterTurret;
					_isTurret = true;
					_isWeaponCore = true;
					_weaponValid = true;

				}

			}

		}

		public void FireStaticGun() {

			if (!_isStatic)
				return;


		
		}

		public void ToggleStaticGun(bool fireEnable) {
		
			
		
		}

	}

}
