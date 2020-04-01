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
	public class TurretWeaponProfile : WeaponProfile, IWeaponProfile {

		public bool IsForwardFacingWeapon { get { return false; } }
		public MyDefinitionId CurrentAmmoId { get { return GetCurrentAmmoMagazineId(); } }

		private IMyLargeTurretBase _turretWeaponBlock;

		public TurretWeaponProfile(IMyRemoteControl remoteControl, IMyTerminalBlock block) : base(remoteControl, block) {

			_turretWeaponBlock = block as IMyLargeTurretBase;

			if (_turretWeaponBlock != null) {

				_gunBase = (IMyGunObject<MyGunBase>)_turretWeaponBlock;

			} else {

				_weaponValid = false;

			}

			if (!_weaponValid)
				return;

			GetWeaponDefinition(_turretWeaponBlock);

		}

		public bool FireWeaponOnce() {

			ReloadWeapon();
			return false;

		}

		public void ToggleFire(bool fireEnable) {

			ReloadWeapon();

		}

		public void SetCurrentTargetAndAllowedAngle(Vector3D coords, double angle, double distance, bool validTarget, IMyEntity entity = null) {
		
			//Turrets Do Their Own Thing... Mostly?
		
		}

		public override void RefreshAmmoDetails() {

			base.RefreshAmmoDetails();

			if(WeaponIsFunctional())
				_weaponCurrentRange = _turretWeaponBlock.Range;

		}

		public override IMyEntity CurrentTargetEntity() {

			if (WeaponIsFunctional()) {

				return (_turretWeaponBlock.Target != null && _turretWeaponBlock.IsShooting) ? _turretWeaponBlock.Target : null;

			}

			return null;

		}

		public void DetermineWeaponReadiness() {

			base.DetermineWeaponReadiness();

		}

		

	}

}
