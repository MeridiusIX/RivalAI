using RivalAI.Behavior.Subsystems.AutoPilot;
using RivalAI.Entities;
using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.ModAPI;
using VRageMath;
using static RivalAI.Helpers.WcApiDef;
using static RivalAI.Helpers.WcApiDef.WeaponDefinition;

namespace RivalAI.Behavior.Subsystems.Weapons {
	public class CoreWeapon : BaseWeapon, IWeapon{

		internal int _weaponId;

		internal Dictionary<string, MyDefinitionId> _ammoToMagazine = new Dictionary<string, MyDefinitionId>();
		internal Dictionary<string, AmmoDef> _ammoToDefinition = new Dictionary<string, AmmoDef>();
		internal bool _requiresPhysicalAmmo;
		internal bool _beamAmmo;
		internal bool _homingAmmo;
		internal bool _flareAmmo;

		internal WeaponDefinition _weaponDefinition;

		public CoreWeapon(IMyTerminalBlock block, IMyRemoteControl remoteControl, IBehavior behavior, WeaponDefinition weaponDefinition, int weaponId) : base(block, remoteControl, behavior) {

			if (Utilities.AllWeaponCoreGuns.Contains(block.SlimBlock.BlockDefinition.Id)) {

				Logger.MsgDebug(block.CustomName + " Is WeaponCore Static Weapon", DebugTypeEnum.Weapon);
				_isStatic = true;

			} else {

				Logger.MsgDebug(block.CustomName + " Is WeaponCore Turret Weapon", DebugTypeEnum.Weapon);
				_isTurret = true;

			}

			_weaponDefinition = weaponDefinition;
			_weaponId = weaponId;

			//Rate Of Fire
			_rateOfFire = _weaponDefinition.HardPoint.Loading.RateOfFire;

			//Get Ammo Stuff

			Logger.MsgDebug(_block.CustomName + " Available Ammo Check", DebugTypeEnum.WeaponSetup);
			if (_weaponDefinition.Ammos.Length > 0) {

				foreach (var ammo in _weaponDefinition.Ammos) {

					Logger.MsgDebug(string.Format(" - {0} / {1}", ammo.AmmoMagazine, ammo.AmmoRound), DebugTypeEnum.WeaponSetup);

					if (!_ammoToMagazine.ContainsKey(ammo.AmmoRound))
						_ammoToMagazine.Add(ammo.AmmoRound, new MyDefinitionId(typeof(MyObjectBuilder_AmmoMagazine), ammo.AmmoMagazine));

					if (!_ammoToDefinition.ContainsKey(ammo.AmmoRound))
						_ammoToDefinition.Add(ammo.AmmoRound, ammo);

				}

			} else {

				Logger.MsgDebug(_block.CustomName + " Has No WC Ammo Definitions", DebugTypeEnum.WeaponSetup);
				_isValid = false;
			
			}

		}

		private void StaticWeaponReadiness() {

			var trajectory = _weaponSystem.MaxStaticWeaponRange > -1 ? MathHelper.Clamp(_ammoMaxTrajectory, 0, _weaponSystem.MaxStaticWeaponRange) : _ammoMaxTrajectory;

			//Homing Weapon
			if (_homingAmmo) {

				if (_weaponSystem.AllowHomingWeaponMultiTargeting) {
				
					//TODO: Later Date
				
				} else {

					if (_behavior.AutoPilot.Targeting.HasTarget()) {

						bool threatMatch = false;

						foreach (var threatType in _weaponDefinition.Targeting.Threats) {

							if (threatType == TargetingDef.Threat.Characters && _behavior.AutoPilot.Targeting.Target.GetEntityType() == EntityType.Player) {

								threatMatch = true;
								break;

							}

							if (threatType == TargetingDef.Threat.Grids && _behavior.AutoPilot.Targeting.Target.GetEntityType() == EntityType.Grid) {

								threatMatch = true;
								break;

							}

						}

						if (!threatMatch) {

							//Logger.MsgDebug(" - No Autopilot Target To assign For Homing Ammo", DebugTypeEnum.Weapon);
							_readyToFire = false;
							return;

						}

					} else {

						_readyToFire = false;
						return;

					}

					if (trajectory < _behavior.AutoPilot.Targeting.Target.Distance(_block.GetPosition())) {

						//Logger.MsgDebug(" - Target Out Of Range For Homing Ammo", DebugTypeEnum.Weapon);
						_readyToFire = false;
						return;

					}
				
				}

				return;

			}

			//Flare Weapon
			if (_flareAmmo) {

				if (!_weaponSystem.UseAntiSmartWeapons || !_weaponSystem.IncomingHomingProjectiles) {

					//Logger.MsgDebug(" - No Incoming Homing Weapons For Firing Flares", DebugTypeEnum.Weapon);
					_readyToFire = false;
					return;

				}

				return;
			
			}

			//---------------------
			//----Other Weapons----
			//---------------------

			_readyToFire = StaticWeaponAlignedToTarget(_beamAmmo);

		}

		public bool HasAmmo() {

			bool gotAmmoDetails = false;
			bool gotAmmoResult = false;

			if (_currentAmmoMagazine == new MyDefinitionId()) {

				gotAmmoDetails = true;
				gotAmmoResult = GetAmmoDetails();

			}

			if (MyAPIGateway.Session.CreativeMode || !_requiresPhysicalAmmo)
				return true;

			if (_inventory.GetItemAmount(_currentAmmoMagazine) == 0) {

				if (_weaponSystem.UseAmmoReplenish && _ammoRefills < _weaponSystem.MaxAmmoReplenishments) {

					_pendingAmmoRefill = true;

				} else {

					return false;

				}

			}

			if(!gotAmmoDetails)
				gotAmmoResult = GetAmmoDetails();


			return gotAmmoResult;
		
		}

		private bool GetAmmoDetails() {

			//Logger.MsgDebug(string.Format(" - Getting Ammo Details For Core Weapon: {0}", _block.CustomName), DebugTypeEnum.Weapon);
			var currentAmmo = RAI_SessionCore.Instance.WeaponCore.GetActiveAmmo(_block, _weaponId);
			//Logger.MsgDebug(string.Format(" - CurrentAmmo For Core Weapon {0}: {1}", _block.CustomName, currentAmmo ?? "N/A"), DebugTypeEnum.Weapon);

			if (_ammoRound != currentAmmo) {

				//Logger.MsgDebug(" - Create New Ammo Def", DebugTypeEnum.Weapon);
				var ammoDef = new AmmoDef();

				if (currentAmmo != null) {

					//Logger.MsgDebug(" - Try Get From Ammo-To-Def", DebugTypeEnum.Weapon);
					_ammoToDefinition.TryGetValue(currentAmmo, out ammoDef);
					//Logger.MsgDebug(" - Try Get From Ammo-To-Def", DebugTypeEnum.Weapon);

				} else {

					//Logger.MsgDebug(" - Try Get From Index 0", DebugTypeEnum.Weapon);
					ammoDef = _weaponDefinition.Ammos[0];

				}

				if (!string.IsNullOrWhiteSpace(ammoDef?.AmmoRound)) {

					//Logger.MsgDebug(" - Populate Ammo Data", DebugTypeEnum.Weapon);
					_ammoRound = currentAmmo;
					_currentAmmoMagazine = new MyDefinitionId(typeof(MyObjectBuilder_AmmoMagazine), ammoDef.AmmoMagazine);
					_requiresPhysicalAmmo = _currentAmmoMagazine.SubtypeName != "Energy";
					_beamAmmo = ammoDef.Beams.Enable;
					_homingAmmo = ammoDef.Trajectory.Guidance == AmmoDef.TrajectoryDef.GuidanceType.Smart || ammoDef.Trajectory.Guidance == AmmoDef.TrajectoryDef.GuidanceType.TravelTo;
					_flareAmmo = ammoDef.AreaEffect.AreaEffect == AmmoDef.AreaDamageDef.AreaEffectType.AntiSmart;
					_ammoMaxTrajectory = ammoDef.Trajectory.MaxTrajectory;
					_ammoMaxVelocity = _beamAmmo ? -1 : ammoDef.Trajectory.DesiredSpeed;
					_ammoInitialVelocity = _beamAmmo ? -1 : ammoDef.Trajectory.DesiredSpeed;
					_ammoAcceleration = _beamAmmo ? -1 : ammoDef.Trajectory.AccelPerSec;

				} else {

					//Logger.MsgDebug(" - AmmoDef Was Null", DebugTypeEnum.Weapon);
					return false;

				}

			}

			return true;

		}

		public IMyEntity CurrentTarget() {

			return !IsValid() ? null : RAI_SessionCore.Instance.WeaponCore.GetWeaponTarget(_block, _weaponId).Item4;

		}

		public void DetermineWeaponReadiness() {

			//Logger.MsgDebug(_block.CustomName + " WC Check Readiness", DebugTypeEnum.Weapon);

			_readyToFire = true;

			//Valid
			if (!IsValid() || !IsActive()) {

				//Logger.MsgDebug(string.Format(" - Core Valid/Active Check Failed: {0} / {1}", IsValid(), IsActive()), DebugTypeEnum.Weapon);
				_readyToFire = false;
				return;

			}

			//Ammo
			if (!HasAmmo()) {

				//Logger.MsgDebug(string.Format(" - AmmoRound: {0} /// AmmoMag: {1}", _ammoRound ?? "null", _currentAmmoMagazine.SubtypeName ?? "null"), DebugTypeEnum.Weapon);
				_readyToFire = false;
				return;

			}

			//WeaponCoreReadyFireCheck
			if (_isStatic)
				StaticWeaponReadiness();

		}

		public void FireOnce() {

			if (_isTurret)
				return;

			if (_isValid && IsActive() && _readyToFire) {

				Logger.MsgDebug(_block.CustomName + " Fire Once", DebugTypeEnum.Weapon);
				RAI_SessionCore.Instance.WeaponCore.FireWeaponOnce(_block, false, _weaponId);

			}
	
		}

		public override bool IsBarrageWeapon() {

			if (!_checkBarrageWeapon) {

				_checkBarrageWeapon = true;

				if (_isStatic && _weaponSystem.UseBarrageFire) {

					_isBarrageWeapon = _rateOfFire < _weaponSystem.MaxFireRateForBarrageWeapons;

				}

			}

			return _isBarrageWeapon;

		}

		public void SetTarget(IMyEntity entity) {

			if (!IsValid())
				return;

			RAI_SessionCore.Instance.WeaponCore.SetWeaponTarget(_block, entity, _weaponId);

		}

		public void ToggleFire() {

			if (_isTurret)
				return;

			//Logger.MsgDebug(_block.CustomName + " Valid:  " + _isValid, DebugTypeEnum.Weapon);
			//Logger.MsgDebug(_block.CustomName + " Active: " + IsActive(), DebugTypeEnum.Weapon);
			//Logger.MsgDebug(_block.CustomName + " Ready:  " + _readyToFire, DebugTypeEnum.Weapon);
			//Logger.MsgDebug(_block.CustomName + " Barra:  " + _isBarrageWeapon, DebugTypeEnum.Weapon);


			if (_isValid && IsActive() && _readyToFire && !_isBarrageWeapon) {

				if (!_firing) {

					Logger.MsgDebug(_block.CustomName + " Start Fire", DebugTypeEnum.Weapon);
					_firing = true;
					RAI_SessionCore.Instance.WeaponCore.ToggleWeaponFire(_block, true, false, _weaponId);

				}

			} else {

				if (_firing) {

					Logger.MsgDebug(_block.CustomName + " End Fire", DebugTypeEnum.Weapon);
					_firing = false;
					RAI_SessionCore.Instance.WeaponCore.ToggleWeaponFire(_block, false, false, _weaponId);

				}

			}

		}

	}
}
