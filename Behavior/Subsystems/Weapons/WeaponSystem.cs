using System;
using System.Collections.Generic;
using System.Text;
using RivalAI.Behavior.Subsystems.Profiles;
using RivalAI.Helpers;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.ModAPI;
using VRageMath;
using static RivalAI.Helpers.WcApiDef;

namespace RivalAI.Behavior.Subsystems.Weapons {
	public class WeaponSystem {

		//Configurable - Enabled Weapons
		public bool UseStaticGuns;
		public bool UseTurrets;

		//Configurable - Static Weapons
		public double MaxStaticWeaponRange;
		public double WeaponMaxAngleFromTarget;
		public double WeaponMaxBaseDistanceTarget;

		//Configurable - Barrage Fire
		public bool UseBarrageFire;
		public int MaxFireRateForBarrageWeapons;

		//Configurable - Ammo Replenish
		public bool UseAmmoReplenish;
		public int AmmoReplenishClipAmount;
		public int MaxAmmoReplenishments;

		//Configurable - WeaponCore
		public bool UseAntiSmartWeapons;
		public bool AllowHomingWeaponMultiTargeting;
		public int MultiTargetCheckCooldown;

		//Non-Configurable
		private IMyRemoteControl _remoteControl;

		private IBehavior _behavior;

		public List<IWeapon> StaticWeapons;
		public List<IWeapon> Turrets;

		private DateTime _collisionTimer;

		private bool _parallelWorkInProgress;
		private bool _pendingBarrageTrigger;
		private int _barrageWeaponIndex;

		private List<WaypointModificationEnum> _allowedFlags;
		private List<WaypointModificationEnum> _restrictedFlags;

		public Dictionary<Direction, double> MaxStaticRangesPerDirection;
		
		public bool WaypointIsTarget;

		public bool IncomingHomingProjectiles;
		public DateTime LastHomingTargetCheck;
		public bool GetRandomHomingTarget;

		public WeaponSystem(IMyRemoteControl remoteControl, IBehavior behavior) {

			UseStaticGuns = false;
			UseTurrets = true;

			MaxStaticWeaponRange = 1500;
			WeaponMaxAngleFromTarget = 6;
			WeaponMaxBaseDistanceTarget = 20;

			UseBarrageFire = false;
			MaxFireRateForBarrageWeapons = 200;

			UseAmmoReplenish = true;
			AmmoReplenishClipAmount = 15;
			MaxAmmoReplenishments = 10;

			UseAntiSmartWeapons = false;
			AllowHomingWeaponMultiTargeting = false;
			MultiTargetCheckCooldown = 5;

			_allowedFlags = new List<WaypointModificationEnum>();
			_allowedFlags.Add(WaypointModificationEnum.TargetIsInitialWaypoint);
			_allowedFlags.Add(WaypointModificationEnum.WeaponLeading);
			_allowedFlags.Add(WaypointModificationEnum.CollisionLeading);

			_restrictedFlags = new List<WaypointModificationEnum>();
			_restrictedFlags.Add(WaypointModificationEnum.Collision);
			_restrictedFlags.Add(WaypointModificationEnum.Offset);
			_restrictedFlags.Add(WaypointModificationEnum.PlanetPathing);

			StaticWeapons = new List<IWeapon>();
			Turrets = new List<IWeapon>();

			_collisionTimer = MyAPIGateway.Session.GameDateTime;

			MaxStaticRangesPerDirection = new Dictionary<Direction, double>();
			MaxStaticRangesPerDirection.Add(Direction.Forward, 0);
			MaxStaticRangesPerDirection.Add(Direction.Backward, 0);
			MaxStaticRangesPerDirection.Add(Direction.Up, 0);
			MaxStaticRangesPerDirection.Add(Direction.Down, 0);
			MaxStaticRangesPerDirection.Add(Direction.Left, 0);
			MaxStaticRangesPerDirection.Add(Direction.Right, 0);

			WaypointIsTarget = false;

			IncomingHomingProjectiles = false;
			LastHomingTargetCheck = DateTime.MinValue;
			GetRandomHomingTarget = false;

			if (remoteControl == null || !MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid))
				return;

			_remoteControl = remoteControl;
			_behavior = behavior;

		}

		public void Setup() {

			var blocks = BlockHelper.GetBlocksOfType<IMyFunctionalBlock>(_remoteControl.SlimBlock.CubeGrid);
			//Logger.MsgDebug("WCAPI Ready: " + RAI_SessionCore.Instance.WeaponCore.IsReady.ToString(), DebugTypeEnum.Weapon);
			//Logger.MsgDebug(string.Format("All WC: {0} /// All WCS: {1} /// All WCT: {2}", Utilities.AllWeaponCoreBlocks.Count, Utilities.AllWeaponCoreGuns.Count, Utilities.AllWeaponCoreTurrets.Count), DebugTypeEnum.Weapon);

			foreach (var block in blocks) {

				IWeapon weapon = null;

				//Logger.MsgDebug(block.CustomName + " Has Core Weapon: " + RAI_SessionCore.Instance.WeaponCore.HasCoreWeapon(block).ToString(), DebugTypeEnum.Weapon);

				if (RAI_SessionCore.Instance.WeaponCoreLoaded && Utilities.AllWeaponCoreBlocks.Contains(block.SlimBlock.BlockDefinition.Id)) {

					var weaponsInBlock = new Dictionary<string, int>();
					RAI_SessionCore.Instance.WeaponCore.GetBlockWeaponMap(block, weaponsInBlock);

					foreach (var weaponName in weaponsInBlock.Keys) {

						WeaponDefinition weaponDef = new WeaponDefinition();

						foreach (var definition in RAI_SessionCore.Instance.WeaponCore.WeaponDefinitions) {

							if (definition.HardPoint.WeaponName == weaponName) {

								weaponDef = definition;
								break;

							}

						}

						weapon = new CoreWeapon(block, _remoteControl, _behavior, weaponDef, weaponsInBlock[weaponName]);

						if (!weapon.IsValid()) {

							Logger.MsgDebug(block.CustomName + " Is Not Valid", DebugTypeEnum.BehaviorSetup);
							continue;

						}

						Logger.MsgDebug(block.CustomName + " Is WeaponCore", DebugTypeEnum.BehaviorSetup);

						if (weapon.IsStaticGun()) {

							StaticWeapons.Add(weapon);

						} else {

							Turrets.Add(weapon);

						}

						continue;

					}

					RAI_SessionCore.Instance.WeaponCore.DisableRequiredPower(block);

					continue;

				} else if (block as IMyLargeTurretBase != null || block as IMyUserControllableGun != null) {

					weapon = new RegularWeapon(block, _remoteControl, _behavior);

				} else {

					continue;

				}

				if (!weapon.IsValid()) {

					Logger.MsgDebug(block.CustomName + " Is Not Valid", DebugTypeEnum.BehaviorSetup);
					continue;

				}

				Logger.MsgDebug(block.CustomName + " Is RegularWeapon", DebugTypeEnum.BehaviorSetup);

				if (weapon.IsStaticGun()) {

					StaticWeapons.Add(weapon);

				} else {

					Turrets.Add(weapon);

				}

			}

			Logger.MsgDebug(string.Format("{0}: Weapons Registered - Static: {1} - Turret: {2}", _remoteControl.CubeGrid.CustomName, StaticWeapons.Count, Turrets.Count), DebugTypeEnum.BehaviorSetup);

		}

		public void SetupReferences(IBehavior behavior) {

			_behavior = behavior;

		}

		public void InitTags() {

			if (string.IsNullOrWhiteSpace(_remoteControl.CustomData) == false) {

				var descSplit = _remoteControl.CustomData.Split('\n');

				foreach (var tag in descSplit) {

					//UseStaticGuns
					if (tag.Contains("[UseStaticGuns:") == true) {

						UseStaticGuns = TagHelper.TagBoolCheck(tag);

					}

					//UseTurrets
					if (tag.Contains("[UseTurrets:") == true) {

						UseTurrets = TagHelper.TagBoolCheck(tag);

					}

					//MaxStaticWeaponRange
					if (tag.Contains("[MaxStaticWeaponRange:") == true) {

						MaxStaticWeaponRange = TagHelper.TagDoubleCheck(tag, MaxStaticWeaponRange);

					}

					//WeaponMaxAngleFromTarget
					if (tag.Contains("[WeaponMaxAngleFromTarget:") == true) {

						WeaponMaxAngleFromTarget = TagHelper.TagDoubleCheck(tag, WeaponMaxAngleFromTarget);

					}

					//WeaponMaxBaseDistanceTarget
					if (tag.Contains("[WeaponMaxBaseDistanceTarget:") == true) {

						WeaponMaxBaseDistanceTarget = TagHelper.TagDoubleCheck(tag, WeaponMaxBaseDistanceTarget);

					}

					//UseBarrageFire
					if (tag.Contains("[UseBarrageFire:") == true) {

						UseBarrageFire = TagHelper.TagBoolCheck(tag);

					}

					//MaxFireRateForBarrageWeapons
					if (tag.Contains("[MaxFireRateForBarrageWeapons:") == true) {

						MaxFireRateForBarrageWeapons = TagHelper.TagIntCheck(tag, MaxFireRateForBarrageWeapons);

					}

					//UseAmmoReplenish
					if (tag.Contains("[UseAmmoReplenish:") == true) {

						UseAmmoReplenish = TagHelper.TagBoolCheck(tag);

					}

					//AmmoReplenishClipAmount
					if (tag.Contains("[AmmoReplenishClipAmount:") == true) {

						AmmoReplenishClipAmount = TagHelper.TagIntCheck(tag, AmmoReplenishClipAmount);

					}

					//MaxAmmoReplenishments
					if (tag.Contains("[MaxAmmoReplenishments:") == true) {

						MaxAmmoReplenishments = TagHelper.TagIntCheck(tag, MaxAmmoReplenishments);

					}

					//UseAntiSmartWeapons
					if (tag.Contains("[UseAntiSmartWeapons:") == true) {

						UseAntiSmartWeapons = TagHelper.TagBoolCheck(tag);

					}

					//AllowHomingWeaponMultiTargeting
					if (tag.Contains("[AllowHomingWeaponMultiTargeting:") == true) {

						AllowHomingWeaponMultiTargeting = TagHelper.TagBoolCheck(tag);

					}

					//MultiTargetCheckCooldown
					if (tag.Contains("[MultiTargetCheckCooldown:") == true) {

						MultiTargetCheckCooldown = TagHelper.TagIntCheck(tag, MultiTargetCheckCooldown);

					}

				}

			}

		}

		public void PrepareWeapons() {

			try {

				var timeSpan = MyAPIGateway.Session.GameDateTime - _collisionTimer;

				if (timeSpan.TotalMilliseconds >= 1000) {

					_collisionTimer = MyAPIGateway.Session.GameDateTime;
					_behavior.AutoPilot.Collision.RunSecondaryCollisionChecks();

				}

				CheckIfWaypointIsTarget();
				CheckForIncomingHomingProjectiles();

				foreach (var gun in StaticWeapons) {

					gun.DetermineWeaponReadiness();

				}

				foreach (var turret in StaticWeapons) {

					turret.DetermineWeaponReadiness();

				}

				RefreshMaxStaticRangeReferences();

			} catch (Exception e) {

				Logger.MsgDebug("Exception While Preparing Weapons", DebugTypeEnum.Weapon);
				Logger.MsgDebug(e.ToString(), DebugTypeEnum.Weapon);

			}
			

		}

		public void ProcessWeaponReloads() {

			foreach (var weapon in StaticWeapons) {

				weapon.ReplenishAmmo();

			}

			foreach (var weapon in Turrets) {

				weapon.ReplenishAmmo();

			}
		
		}

		public void CheckIfWaypointIsTarget() {

			foreach (var waypointType in _restrictedFlags) {

				if (_behavior.AutoPilot.IndirectWaypointType.HasFlag(waypointType)) {

					WaypointIsTarget = false;
					return;
				
				}
			
			}

			foreach (var waypointType in _allowedFlags) {

				if (_behavior.AutoPilot.DirectWaypointType.HasFlag(waypointType)) {

					WaypointIsTarget = true;
					return;

				}

			}

			WaypointIsTarget = false;

		}

		public void CheckForIncomingHomingProjectiles() {

			if (!UseAntiSmartWeapons || !RAI_SessionCore.Instance.WeaponCoreLoaded)
				return;

			IncomingHomingProjectiles = RAI_SessionCore.Instance.WeaponCore.GetProjectilesLockedOn(_remoteControl).Item1;

		}

		public void CheckForPotentialHomingTargets() {

			if (!AllowHomingWeaponMultiTargeting || !RAI_SessionCore.Instance.WeaponCoreLoaded)
				return;

			var timeSpan = MyAPIGateway.Session.GameDateTime - LastHomingTargetCheck;

			if (timeSpan.TotalSeconds < MultiTargetCheckCooldown) {

				GetRandomHomingTarget = false;
				return;

			}

			LastHomingTargetCheck = MyAPIGateway.Session.GameDateTime;
			GetRandomHomingTarget = true;

		}

		public void RefreshMaxStaticRangeReferences() {

			double forward = 0;
			double backward = 0;
			double up = 0;
			double down = 0;
			double left = 0;
			double right = 0;

			foreach (var weapon in StaticWeapons) {

				if (!weapon.IsValid() && !weapon.IsActive())
					continue;

				var remMatrix = _remoteControl.WorldMatrix;
				var wepMatrix = weapon.Block().WorldMatrix;

				if (wepMatrix.Forward == remMatrix.Forward || VectorHelper.GetAngleBetweenDirections(remMatrix.Forward, wepMatrix.Forward) < 1) {

					weapon.SetDirection(Direction.Forward);

					if (weapon.MaxAmmoTrajectory() > forward)
						forward = weapon.MaxAmmoTrajectory();

				}

				if (wepMatrix.Backward == remMatrix.Backward || VectorHelper.GetAngleBetweenDirections(remMatrix.Backward, wepMatrix.Backward) < 1) {

					weapon.SetDirection(Direction.Backward);

					if (weapon.MaxAmmoTrajectory() > backward)
						backward = weapon.MaxAmmoTrajectory();

				}

				if (wepMatrix.Up == remMatrix.Up || VectorHelper.GetAngleBetweenDirections(remMatrix.Up, wepMatrix.Up) < 1) {

					weapon.SetDirection(Direction.Up);

					if (weapon.MaxAmmoTrajectory() > up)
						up = weapon.MaxAmmoTrajectory();

				}

				if (wepMatrix.Down == remMatrix.Down || VectorHelper.GetAngleBetweenDirections(remMatrix.Down, wepMatrix.Down) < 1) {

					weapon.SetDirection(Direction.Down);

					if (weapon.MaxAmmoTrajectory() > down)
						down = weapon.MaxAmmoTrajectory();

				}

				if (wepMatrix.Left == remMatrix.Left || VectorHelper.GetAngleBetweenDirections(remMatrix.Left, wepMatrix.Left) < 1) {

					weapon.SetDirection(Direction.Left);

					if (weapon.MaxAmmoTrajectory() > left)
						left = weapon.MaxAmmoTrajectory();

				}

				if (wepMatrix.Right == remMatrix.Right || VectorHelper.GetAngleBetweenDirections(remMatrix.Right, wepMatrix.Right) < 1) {

					weapon.SetDirection(Direction.Right);

					if (weapon.MaxAmmoTrajectory() > right)
						right = weapon.MaxAmmoTrajectory();

				}

			}

			MaxStaticRangesPerDirection[Direction.Forward] = forward;
			MaxStaticRangesPerDirection[Direction.Backward] = backward;
			MaxStaticRangesPerDirection[Direction.Up] = up;
			MaxStaticRangesPerDirection[Direction.Down] = down;
			MaxStaticRangesPerDirection[Direction.Left] = left;
			MaxStaticRangesPerDirection[Direction.Right] = right;

		}

		public double GetMaxRange(Direction direction) {

			double range = 0;
			MaxStaticRangesPerDirection.TryGetValue(direction, out range);

			return (range > MaxStaticWeaponRange) ? MaxStaticWeaponRange : range;

		}

		public void FireWeapons() {

			if (_pendingBarrageTrigger) {

				Logger.MsgDebug("Pending Parallel For Barrage", DebugTypeEnum.WeaponBarrage);
				_pendingBarrageTrigger = false;
				FireBarrageWeapons();

			}

			foreach (var weapon in StaticWeapons) {

				weapon.ToggleFire();

			}

		}

		public void FireBarrageWeapons() {

			if (_parallelWorkInProgress) {

				Logger.MsgDebug("Pending Parallel For Barrage", DebugTypeEnum.WeaponBarrage);
				_pendingBarrageTrigger = true;
				return;

			}

			int weaponCount = this.StaticWeapons.Count;
			int iteratedWeapons = 0;

			if (weaponCount > 1) {

				while (true) {

					_barrageWeaponIndex++;
					iteratedWeapons++;

					if (_barrageWeaponIndex >= weaponCount) {

						_barrageWeaponIndex = 0;

					}

					var weapon = StaticWeapons[_barrageWeaponIndex];

					if (weapon.IsBarrageWeapon() && weapon.IsActive() && weapon.IsReadyToFire()) {

						weapon.FireOnce();
						break;

					}

					if (iteratedWeapons >= weaponCount)
						break;

				}

			} else if (weaponCount == 1) {

				var weapon = StaticWeapons[0];

				if (weapon.IsActive() && weapon.IsReadyToFire()) {

					weapon.FireOnce();

				}

			}

		}

		public bool HasWorkingWeapons() {

			foreach (var weapon in this.StaticWeapons) {

				if (weapon.IsValid() && weapon.IsActive())
					return true;

			}

			foreach (var weapon in this.Turrets) {

				if (weapon.IsValid() && weapon.IsActive())
					return true;

			}

			return false;

		}

		public long GetTurretTarget() {

			var resultList = new List<IMyEntity>();
			IMyEntity closestEntity = null;
			double closestEntityDistance = -1;

			foreach (var weapon in Turrets) {

				if (!weapon.IsValid() || !weapon.IsActive())
					continue;

				var entity = weapon.CurrentTarget();

				if (entity == null)
					continue;

				//TODO: Add some additional filters?

				double distance = Vector3D.Distance(weapon.Block().GetPosition(), entity.GetPosition());

				if (closestEntityDistance == -1 || distance < closestEntityDistance) {

					closestEntity = entity;
					closestEntityDistance = distance;

				}

			}

			return closestEntity != null ? closestEntity.EntityId : 0;

		}

		public void GetAmmoSpeedDetails(Direction direction, out double velocity, out double initialVelocity, out double acceleration) {

			velocity = 0;
			initialVelocity = 0;
			acceleration = 0;

			Dictionary<MyTuple<double, double, double>, int> commonVelocity = new Dictionary<MyTuple<double, double, double>, int>();

			foreach (var weapon in StaticWeapons) {

				if (weapon.GetDirection() != direction)
					continue;

				var ammoData = new MyTuple<double, double, double>(weapon.AmmoAcceleration(), weapon.AmmoInitialVelocity(), weapon.AmmoVelocity());

				if (commonVelocity.ContainsKey(ammoData)) {

					commonVelocity[ammoData]++;


				} else {

					commonVelocity.Add(ammoData, 1);

				}

			}

			int highestValue = 0;

			foreach (var data in commonVelocity.Keys) {

				int amount = 0;

				if (commonVelocity.TryGetValue(data, out amount)) {

					if (amount > highestValue) {

						highestValue = amount;
						acceleration = data.Item1;
						initialVelocity = data.Item2;
						velocity = data.Item3;

					}
				
				}
			
			}

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
