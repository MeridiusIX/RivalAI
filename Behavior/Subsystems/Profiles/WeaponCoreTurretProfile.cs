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
using static RivalAI.Helpers.WcApiDef;

namespace RivalAI.Behavior.Subsystems.Profiles {
	public class WeaponCoreTurretProfile : WeaponProfile, IWeaponProfile {

		public bool IsForwardFacingWeapon { get { return false; } }
		public MyDefinitionId CurrentAmmoId { get { return GetCurrentAmmoMagazineId(); } }

		private IMyLargeTurretBase _turretWeaponBlock;

		public WeaponCoreTurretProfile(IMyRemoteControl remoteControl, IMyTerminalBlock block, WeaponDefinition definition, int id) : base(remoteControl, block) {

			

		}

		public bool FireWeaponOnce() {

			ReloadWeapon();
			return false;

		}

		public void ToggleFire(bool fireEnable) {

			ReloadWeapon();

		}

		public void SetCurrentTargetAndAllowedAngle(Vector3D coords, double angle, double distance, bool validTarget, IMyEntity entity = null) {
		
			
		
		}

		public override void RefreshAmmoDetails() {

			base.RefreshAmmoDetails();

			//if(WeaponIsFunctional())
				//_weaponCurrentRange = _turretWeaponBlock.Range;

		}

		public override IMyEntity CurrentTargetEntity() {

			if (WeaponIsFunctional()) {

				//return (_turretWeaponBlock.Target != null && _turretWeaponBlock.IsShooting) ? _turretWeaponBlock.Target : null;

			}

			return null;

		}

		public void DetermineWeaponReadiness() {

			

		}

	}

}
