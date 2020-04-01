using RivalAI.Helpers;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using static RivalAI.Helpers.WcApiDef;
using static RivalAI.Helpers.WcApiDef.WeaponDefinition;
using static RivalAI.Helpers.WcApiDef.WeaponDefinition.AmmoDef.TrajectoryDef;

namespace RivalAI.Behavior.Subsystems.Profiles {
	public class WeaponCoreProfile : WeaponProfile {

		internal WeaponDefinition _weaponCoreDefinition;

		internal string _weaponCoreAmmoMagazineId;
		internal AmmoDef _weaponCoreAmmoDefinition;
		internal bool _currentAmmoIsHoming;
		internal bool _currentAmmoIsBeam;

		internal int _weaponIndex;
		internal bool _isForwardFacingWeapon = false;

		internal bool _weaponDefinitionHasNoAmmo = false;

		internal bool _angleCheckPassed;
		internal bool _weaponCoreAngleCheckPassed;

		public WeaponCoreProfile(IMyRemoteControl remoteControl, IMyTerminalBlock block, WeaponDefinition definition, int id) : base(remoteControl, block) {

			IsWeaponCore = true;
			_weaponCoreDefinition = definition;
			_weaponIndex = id;

			if (_weaponCoreDefinition.Ammos.Length > 0) {

				int index = 0;

				for (int i = 0; i < _weaponCoreDefinition.Ammos.Length; i++) {

					var definitionId = new MyDefinitionId(typeof(MyObjectBuilder_AmmoMagazine), _weaponCoreDefinition.Ammos[i].AmmoMagazine);

					try {

						var ammoDef = MyDefinitionManager.Static.GetAmmoMagazineDefinition(definitionId);

						if (!_allAmmoMagazines.Contains(ammoDef))
							_allAmmoMagazines.Add(ammoDef);

					} catch (Exception e) {

						Logger.MsgDebug("Could Not Get AmmoMagazine Definition From ID: " + definitionId.ToString(), DebugTypeEnum.Weapon);

					}

				}

				if (_allAmmoMagazines.Count == 0) {

					_weaponDefinitionHasNoAmmo = true;
					_weaponValid = false;
					return;

				}

			} else {

				_weaponDefinitionHasNoAmmo = true;
				_weaponValid = false;
				return;

			}

			OwnershipCheck(_weaponBlock);

		}

		public override void RefreshAmmoDetails() {

			var timeSpan = MyAPIGateway.Session.GameDateTime - _lastAmmoRefresh;

			if (timeSpan.TotalMilliseconds < 500)
				return;

			_lastAmmoRefresh = MyAPIGateway.Session.GameDateTime;

			var ammo = RAI_SessionCore.Instance.WeaponCore.GetActiveAmmo(_weaponBlock, _weaponIndex);

			if (string.IsNullOrWhiteSpace(ammo)) {

				return;

			}

			if (ammo != _weaponCoreAmmoMagazineId) {

				foreach (var ammoDef in _weaponCoreDefinition.Ammos) {

					if (ammoDef.AmmoMagazine == ammo) {

						_weaponCoreAmmoMagazineId = ammo;
						_weaponCoreAmmoDefinition = ammoDef;
						break;

					}

				}

			}

			_weaponCurrentRange = _weaponCoreAmmoDefinition.Trajectory.MaxTrajectory;
			_weaponRateOfFire = _weaponCoreDefinition.HardPoint.Loading.RateOfFire;

			if (ammo == "Energy") {

				_currentAmmoCount = 1;

			} else {

				GetTypesOfAmmoInInventory();
				var ammoId = new MyDefinitionId(typeof(MyObjectBuilder_AmmoMagazine), ammo);

				if (_ammoSubtypesInInventory.Contains(ammo)) {

					_currentAmmoCount = CountItemsInInventory(ammoId);

				} else {

					_currentAmmoCount = 0;

					if (_weaponSystem.UseAmmoReplenish && _ammoMagazinesReloads < _weaponSystem.MaxAmmoReplenishments) {

						MyAmmoMagazineDefinition ammoMagDef = null;

						if (Utilities.NormalAmmoMagReferences.TryGetValue(ammoId, out ammoMagDef)) {

							_ammoMagazineDefinition = ammoMagDef;
							_ammoReloadPending = true;

						}

					}

				}

			}

		}

		internal bool IsCurrentAmmoHoming() {

			if (_weaponCoreAmmoDefinition == null)
				return false;

			if (_weaponCoreAmmoDefinition.Trajectory.Guidance == GuidanceType.None || _weaponCoreAmmoDefinition.Trajectory.Guidance == GuidanceType.DetectFixed)
				return false;

			if (!RAI_SessionCore.Instance.WeaponCore.CanShootTarget(_weaponBlock, _currentTargetEntity, _weaponIndex))
				return false;

			return true;

		}

		internal bool IsCurrentAmmoLaser() {

			if (_weaponCoreAmmoDefinition == null)
				return false;

			return _weaponCoreAmmoDefinition.Beams.Enable;

		}

		internal void OwnershipCheck(IMyTerminalBlock block) {

			if (OwnershipHelper.IsNPC(block.OwnerId)) {

				RAI_SessionCore.Instance.WeaponCore.DisableRequiredPower(block);

			}
		
		}

	}

}
