using System;
using System.Collections.Generic;
using System.Text;
using RivalAI.Behavior.Subsystems.Profiles;
using RivalAI.Helpers;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.ModAPI;
using VRageMath;
using static RivalAI.Helpers.WcApiDef;

namespace RivalAI.Behavior.Subsystems {
	public class NewWeaponSystem {

		//Configurable - Enabled Weapons
		public bool UseStaticGuns;
		public bool UseTurrets;

		//Configurable - Static Weapons
		public TargetObstructionEnum AllowedObstructions; //Voxel,Safezone,OtherGrids
		public double MaxStaticWeaponRange;
		public double WeaponMaxAngleFromTarget;

		//Configurable - Barrage Fire
		public bool UseBarrageFire;
		public int MaxFireRateForBarrageWeapons;

		//Configurable - Ammo Replenish
		public bool UseAmmoReplenish;
		public int AmmoReplenishClipAmount;
		public int MaxAmmoReplenishments;

		//Non-Configurable
		private IMyRemoteControl _remoteControl;

		private NewAutoPilotSystem _autoPilot;

		private List<IWeaponProfile> Weapons;

		private bool _parallelWorkInProgress;
		private bool _pendingBarrageTrigger;
		private int _barrageWeaponIndex;

		private bool _withinTargetAngle;

		private List<WaypointModificationEnum> _allowedFlags;
		private List<WaypointModificationEnum> _restrictedFlags;
		private bool _validWaypoint;

		private float _averageAmmoSpeed;

		public Action OnComplete;

		public NewWeaponSystem(IMyRemoteControl remoteControl = null) {

			UseStaticGuns = false;
			UseTurrets = true;

			AllowedObstructions = TargetObstructionEnum.DefenseShield;
			MaxStaticWeaponRange = 1500;
			WeaponMaxAngleFromTarget = 6;

			UseBarrageFire = false;
			MaxFireRateForBarrageWeapons = 200;

			UseAmmoReplenish = true;
			AmmoReplenishClipAmount = 15;
			MaxAmmoReplenishments = 10;

			Weapons = new List<IWeaponProfile>();

			_withinTargetAngle = false;

			_allowedFlags = new List<WaypointModificationEnum>();
			_allowedFlags.Add(WaypointModificationEnum.TargetIsInitialWaypoint);
			_allowedFlags.Add(WaypointModificationEnum.WeaponLeading);
			_allowedFlags.Add(WaypointModificationEnum.CollisionLeading);

			_restrictedFlags = new List<WaypointModificationEnum>();
			_restrictedFlags.Add(WaypointModificationEnum.Collision);
			_restrictedFlags.Add(WaypointModificationEnum.Offset);
			_restrictedFlags.Add(WaypointModificationEnum.PlanetPathing);

			if (remoteControl == null || !MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid))
				return;

			_remoteControl = remoteControl;

		}

		public void Setup() {

			var blocks = BlockHelper.GetBlocksOfType<IMyTerminalBlock>(_remoteControl.SlimBlock.CubeGrid);

			foreach (var block in blocks) {

				if (RAI_SessionCore.Instance.WeaponCoreLoaded && Utilities.AllWeaponCoreBlocks.Contains(block.SlimBlock.BlockDefinition.Id)) {

					SetupBlockAsCoreWeapon(block);

				} else {

					SetupBlockAsRegularWeapon(block);

				}
			
			}

			Logger.MsgDebug("Total Weapons Registered: " + Weapons.Count.ToString(), DebugTypeEnum.Weapon);

		}

		private void SetupBlockAsRegularWeapon(IMyTerminalBlock block) {

			IWeaponProfile iWeapon = null;

			if (block as IMyLargeTurretBase != null) {

				Logger.MsgDebug(block.CustomName + " Is Regular Turret Weapon", DebugTypeEnum.Weapon);
				iWeapon = new TurretWeaponProfile(_remoteControl, block);

			} else if (block as IMyUserControllableGun != null) {

				Logger.MsgDebug(block.CustomName + " Is Regular Static Weapon", DebugTypeEnum.Weapon);
				iWeapon = new StaticWeaponProfile(_remoteControl, block);

			} else {

				return;
			
			}

			if (!iWeapon.IsValid) {

				Logger.MsgDebug(block.CustomName + " Is Not Valid", DebugTypeEnum.Weapon);
				return;

			}

			Logger.MsgDebug(block.CustomName + " Added To Weapon Roster", DebugTypeEnum.Weapon); //
			iWeapon.AssignWeaponSystemReference(this);
			Weapons.Add(iWeapon);
		
		}

		private void SetupBlockAsCoreWeapon(IMyTerminalBlock block) {

			bool isStatic = false;

			if (Utilities.AllWeaponCoreGuns.Contains(block.SlimBlock.BlockDefinition.Id)) {

				Logger.MsgDebug(block.CustomName + " Is WeaponCore Static Weapon", DebugTypeEnum.Weapon);
				isStatic = true;

			}

			var weaponsInBlock = new Dictionary<string, int>();
			RAI_SessionCore.Instance.WeaponCore.GetBlockWeaponMap(block, weaponsInBlock);

			Logger.MsgDebug(weaponsInBlock.Keys.Count.ToString() + " Results From WeaponCore.GetBlockWeaponMap", DebugTypeEnum.Weapon);

			foreach (var weaponName in weaponsInBlock.Keys) {

				WeaponDefinition weaponDef = new WeaponDefinition();

				foreach (var definition in RAI_SessionCore.Instance.WeaponCore.WeaponDefinitions) {

					if (definition.HardPoint.WeaponName == weaponName) {

						weaponDef = definition;
						break;

					}
				
				}

				IWeaponProfile iWeapon = null;

				if (isStatic) {

					Logger.MsgDebug(block.CustomName + " Registered as WeaponCore Static", DebugTypeEnum.Weapon);
					iWeapon = new WeaponCoreStaticProfile(_remoteControl, block, weaponDef, weaponsInBlock[weaponName]);
				
				} else {

					Logger.MsgDebug(block.CustomName + " Registered as WeaponCore Turret", DebugTypeEnum.Weapon);
					iWeapon = new WeaponCoreTurretProfile(_remoteControl, block, weaponDef, weaponsInBlock[weaponName]);

				}

				if (!iWeapon.IsValid || (iWeapon.IsStatic && !this.UseStaticGuns) || (iWeapon.IsTurret && !this.UseTurrets)) {

					Logger.MsgDebug(block.CustomName + " Was Not WeaponCore Valid", DebugTypeEnum.Weapon);
					return;

				}

				Logger.MsgDebug(block.CustomName + " Added To Weapon Roster", DebugTypeEnum.Weapon);
				iWeapon.AssignWeaponSystemReference(this);
				Weapons.Add(iWeapon);

			}

		}

		public void SetupReferences(NewAutoPilotSystem autopilot) {

			_autoPilot = autopilot;
		
		}

		public void InitTags() {

			if (string.IsNullOrWhiteSpace(_remoteControl.CustomData) == false) {

				var descSplit = _remoteControl.CustomData.Split('\n');

				foreach (var tag in descSplit) {

					//UseStaticGuns
					if (tag.Contains("[UseStaticGuns:") == true) {

						this.UseStaticGuns = TagHelper.TagBoolCheck(tag);

					}

					//UseTurrets
					if (tag.Contains("[UseTurrets:") == true) {

						this.UseTurrets = TagHelper.TagBoolCheck(tag);

					}

					//MaxStaticWeaponRange
					if (tag.Contains("[MaxStaticWeaponRange:") == true) {

						this.MaxStaticWeaponRange = TagHelper.TagDoubleCheck(tag, this.MaxStaticWeaponRange);

					}

					//WeaponMaxAngleFromTarget
					if (tag.Contains("[WeaponMaxAngleFromTarget:") == true) {

						this.WeaponMaxAngleFromTarget = TagHelper.TagDoubleCheck(tag, this.WeaponMaxAngleFromTarget);

					}

					//UseBarrageFire
					if (tag.Contains("[UseBarrageFire:") == true) {

						this.UseBarrageFire = TagHelper.TagBoolCheck(tag);

					}

					//MaxFireRateForBarrageWeapons
					if (tag.Contains("[MaxFireRateForBarrageWeapons:") == true) {

						this.MaxFireRateForBarrageWeapons = TagHelper.TagIntCheck(tag, this.MaxFireRateForBarrageWeapons);

					}

					//UseAmmoReplenish
					if (tag.Contains("[UseAmmoReplenish:") == true) {

						this.UseAmmoReplenish = TagHelper.TagBoolCheck(tag);

					}

					//AmmoReplenishClipAmount
					if (tag.Contains("[AmmoReplenishClipAmount:") == true) {

						this.AmmoReplenishClipAmount = TagHelper.TagIntCheck(tag, this.AmmoReplenishClipAmount);

					}

					//MaxAmmoReplenishments
					if (tag.Contains("[MaxAmmoReplenishments:") == true) {

						this.MaxAmmoReplenishments = TagHelper.TagIntCheck(tag, this.MaxAmmoReplenishments);

					}

				}

			}

		}

		public void SetInitialWeaponReadiness() {

			foreach (var weapon in Weapons) {

				weapon.SetCurrentTargetAndAllowedAngle(_autoPilot.GetCurrentWaypoint(), this.WeaponMaxAngleFromTarget, this.MaxStaticWeaponRange, _validWaypoint, _autoPilot.Targeting.Target.GetEntity());

			}

		}

		public void CheckWeaponReadiness() {

			_parallelWorkInProgress = true;

			bool hasAllowedFlag = false;
			bool hasRestrictedFlag = false;
			bool hasInvalidCollision = false;

			foreach (var flag in _allowedFlags) {

				if (_autoPilot.DirectWaypointType.HasFlag(flag)) {

					hasAllowedFlag = true;
					break;

				}
			
			}

			foreach (var flag in _restrictedFlags) {

				if (_autoPilot.IndirectWaypointType.HasFlag(flag)) {

					hasRestrictedFlag = true;
					break;

				}

			}

			if (_autoPilot.Collision.ForwardResult.Type != CollisionType.None) {

				if (_autoPilot.Collision.ForwardResult.GetCollisionDistance() < _autoPilot.DistanceToCurrentWaypoint) {

					if (_autoPilot.Collision.ForwardResult.Type == CollisionType.Shield || _autoPilot.Collision.ForwardResult.Type == CollisionType.Grid) {

						if (_autoPilot.Collision.ForwardResult.GridRelation != TargetRelationEnum.Enemy && _autoPilot.Collision.ForwardResult.ShieldRelation != TargetRelationEnum.Enemy) {

							hasInvalidCollision = true;

						}

					} else {

						hasInvalidCollision = true;

					}
				
				}

			}

			_validWaypoint = (hasAllowedFlag && !hasRestrictedFlag && !hasInvalidCollision);
			//Logger.MsgDebug("Waypoint Valid: " + _validWaypoint.ToString(), DebugTypeEnum.Weapon);

			foreach (var weapon in Weapons) {

				weapon.SetCurrentTargetAndAllowedAngle(_autoPilot.GetCurrentWaypoint(), this.WeaponMaxAngleFromTarget, this.MaxStaticWeaponRange, _validWaypoint, _autoPilot.Targeting.Target.GetEntity());
				weapon.DetermineWeaponReadiness();

			}

			//Logger.MsgDebug("Weapon Ready Types: " + _autoPilot.DirectWaypointType.ToString(), DebugTypeEnum.Weapon);
			//Logger.MsgDebug("Weapon Not Ready Types: " + _autoPilot.IndirectWaypointType.ToString(), DebugTypeEnum.Weapon);

			_parallelWorkInProgress = false;

		}



		public void FireWeapons() {

			if (_pendingBarrageTrigger) {

				Logger.MsgDebug("Pending Parallel For Barrage", DebugTypeEnum.WeaponBarrage);
				_pendingBarrageTrigger = false;
				FireBarrageWeapons();

			}

			foreach (var weapon in Weapons) {

				weapon.ToggleFire(_validWaypoint);
			
			}

			OnComplete?.Invoke();

		}

		public void FireBarrageWeapons() {

			if (_parallelWorkInProgress) {

				Logger.MsgDebug("Pending Parallel For Barrage", DebugTypeEnum.WeaponBarrage);
				_pendingBarrageTrigger = true;
				return;

			}

			int weaponCount = this.Weapons.Count;
			bool loopedOnce = false;

			if (weaponCount > 1) {

				while (true) {

					_barrageWeaponIndex++;

					if (_barrageWeaponIndex >= weaponCount && !loopedOnce) {

						_barrageWeaponIndex = 0;
						loopedOnce = true;

					} else if (_barrageWeaponIndex >= weaponCount && loopedOnce) {

						break;
					
					}

					var weapon = Weapons[_barrageWeaponIndex];

					if (weapon.WeaponIsFunctional() && weapon.IsReadyToFire(_validWaypoint, true)) {

						Logger.MsgDebug("Fire Barrage Weapon", DebugTypeEnum.WeaponBarrage);
						weapon.FireWeaponOnce();
						break;

					}

				}

			} else if (weaponCount == 1) {

				var weapon = Weapons[0];

				if (weapon.WeaponIsFunctional() && weapon.ReadyToFire) {

					weapon.FireWeaponOnce();

				}

			}

		}

		public bool HasWorkingWeapons() {

			foreach (var weapon in this.Weapons) {

				if (weapon.WeaponIsFunctional())
					return true;
			
			}

			return false;
		
		}

		public long GetTurretTarget() {

			var resultList = new List<IMyEntity>();
			IMyEntity closestEntity = null;
			double closestEntityDistance = -1;

			foreach (var weapon in Weapons) {

				if (!weapon.WeaponIsFunctional())
					continue;

				var entity = weapon.CurrentTargetEntity();

				if (entity == null)
					continue;

				//TODO: Add some additional filters?

				double distance = Vector3D.Distance(weapon.GetWeaponPosition(), entity.GetPosition());

				if (closestEntityDistance == -1 || distance < closestEntityDistance) {

					closestEntity = entity;
					closestEntityDistance = distance;

				}

			}

			return closestEntity != null ? closestEntity.EntityId : 0;
		
		}

		public float MostCommonAmmoSpeed(bool calculationRefresh = false) {

			if(!calculationRefresh)
				return _averageAmmoSpeed;

			var ammoRegDict = new Dictionary<double, int>();

			foreach (var weapon in this.Weapons) {

				if (!weapon.IsStatic || !weapon.IsForwardFacingWeapon)
					continue;

				weapon.RefreshAmmoDetails();

				if (weapon.MaxAmmoSpeed > 0) {

					if (ammoRegDict.ContainsKey(weapon.MaxAmmoSpeed)) {

						ammoRegDict[weapon.MaxAmmoSpeed]++;

					} else {

						ammoRegDict.Add(weapon.MaxAmmoSpeed, 1);

					}
				
				}
			
			}

			double mostCommonAmmoSpeed = 0;
			int highestAmmoCount = 0;

			foreach (var ammoSpeed in ammoRegDict.Keys) {
			
				if(ammoRegDict[ammoSpeed] > highestAmmoCount) {

					mostCommonAmmoSpeed = ammoSpeed;
					highestAmmoCount = ammoRegDict[ammoSpeed];

				}
			
			}

			return (float)mostCommonAmmoSpeed;

		}

	}

	public enum WeaponType {
	
		StaticNormal,
		TurretNormal,
		WeaponCoreStatic,
		WeaponCoreTurret,
		WeaponCoreSorterStatic,
		WeaponCoreSorterTurret,

	}

}
