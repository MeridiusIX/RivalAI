using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Weapons;
using SpaceEngineers.Game.ModAPI;
using ProtoBuf;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Utils;
using VRageMath;
using RivalAI.Behavior;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using VRage;

namespace RivalAI.Behavior.Subsystems.Profiles {

	public class WeaponProfile {

		public bool IsValid { get { return _weaponValid; } }
		public bool IsStatic { get { return _isStatic; } internal set { _isStatic = value; } }
		public bool IsTurret { get { return _isTurret; } internal set { _isTurret = value; } }
		public bool IsWeaponCore { get { return _isWeaponCore; } internal set { _isWeaponCore = value; } }
		public int CurrentAmmoCount { get { return _currentAmmoCount; } internal set { _currentAmmoCount = value; } }
		public double MaxAmmoTrajectory { get { return _maxAmmoTrajectory; } internal set { _maxAmmoTrajectory = value; } }
		public double MaxAmmoSpeed { get { return _maxAmmoSpeed; } internal set { _maxAmmoSpeed = value; } }
		public double WeaponCurrentRange { get { return _weaponCurrentRange; } internal set { _weaponCurrentRange = value; } }
		public bool ReadyToFire { get { return _readyToFire; } }

		internal IMyRemoteControl _remoteControl;
		internal IMyTerminalBlock _weaponBlock;
		internal IMyCubeGrid _connectedCubeGrid;
		internal IMyGunObject<MyGunBase> _gunBase;

		internal bool _baseSetupComplete;
		internal bool _weaponValid;
		internal bool _functional;

		internal bool _isStatic;
		internal bool _isTurret;
		internal bool _isWeaponCore;
		internal bool _isBarrage;
		internal bool _checkBarrageWeapon;

		internal bool _readyToFire;
		internal bool _firing;

		internal NewWeaponSystem _weaponSystem;
		internal MyWeaponDefinition _weaponDefinition;
		internal MyAmmoMagazineDefinition _ammoMagazineDefinition;
		internal List<MyAmmoMagazineDefinition> _allAmmoMagazines;
		internal MyAmmoDefinition _ammoDefinition;
		internal List<string> _ammoSubtypesInInventory;

		internal DateTime _lastAmmoRefresh;
		internal int _currentAmmoCount;
		internal double _maxAmmoTrajectory;
		internal double _maxAmmoSpeed;
		internal double _weaponCurrentRange;
		internal double _weaponRateOfFire;

		internal double _currentAngle;
		internal double _currentDistance;
		internal Vector3D _currentTargetWaypoint;
		internal bool _waypointIsTarget;
		internal IMyEntity _currentTargetEntity;

		internal bool _ammoReloadPending;
		internal int _ammoMagazinesReloads;

		public WeaponProfile(IMyRemoteControl remoteControl, IMyTerminalBlock block) {

			_remoteControl = remoteControl;
			_weaponBlock = block;

			if (_weaponBlock != null) {

				_weaponBlock.IsWorkingChanged += (cubeBlock) => { _functional = cubeBlock.IsFunctional && cubeBlock.IsWorking ? true : false; };
				_weaponBlock.OnClose += (entity) => { _weaponValid = false; };
				_weaponValid = true;

				_functional = _weaponBlock.IsFunctional && _weaponBlock.IsWorking ? true : false;
				IsConnectedToControllerGrid();

			}

			if (_weaponValid)
				_baseSetupComplete = true;

			_ammoSubtypesInInventory = new List<string>();
			_allAmmoMagazines = new List<MyAmmoMagazineDefinition>();
			_lastAmmoRefresh = MyAPIGateway.Session.GameDateTime;

		}

		public void AssignWeaponSystemReference(NewWeaponSystem weaponSystem) {

			_weaponSystem = weaponSystem;
		
		}

		public bool WeaponIsFunctional() {

			return (_weaponValid && _functional && IsConnectedToControllerGrid());

		}

		private bool IsConnectedToControllerGrid() {

			if (!_weaponValid)
				return false;

			if (_remoteControl?.SlimBlock?.CubeGrid == null || _weaponBlock?.SlimBlock?.CubeGrid == null) {

				_weaponValid = false;
				return false;

			}

			if (_weaponBlock.SlimBlock.CubeGrid == _connectedCubeGrid)
				return true;

			if (_weaponBlock.IsSameConstructAs(_remoteControl)) {

				_connectedCubeGrid = _weaponBlock.SlimBlock.CubeGrid;
				return true;

			}

			_weaponValid = false;
			return false;

		}

		public void CheckFunctional(IMyCubeBlock cubeBlock) {

			if (!cubeBlock.IsWorking || !cubeBlock.IsFunctional) {

				_functional = false;

			}

			_functional = true;

		}

		public virtual void RefreshAmmoDetails() {

			var timeSpan = MyAPIGateway.Session.GameDateTime - _lastAmmoRefresh;

			if (timeSpan.TotalMilliseconds < 500)
				return;

			_lastAmmoRefresh = MyAPIGateway.Session.GameDateTime;

			if (_gunBase?.GunBase == null) {

				_maxAmmoTrajectory = 0;
				_maxAmmoSpeed = 0;
				return;

			}

			_ammoMagazineDefinition = _gunBase.GunBase.CurrentAmmoMagazineDefinition;
			_ammoDefinition = _gunBase.GunBase.CurrentAmmoDefinition;

			if (_ammoDefinition != null) {

				_maxAmmoTrajectory = _ammoDefinition.MaxTrajectory;
				_maxAmmoSpeed = _ammoDefinition.DesiredSpeed;

			}

			if (_ammoMagazineDefinition != null) {

				_currentAmmoCount = _gunBase.GunBase.CurrentAmmo + (_ammoMagazineDefinition.Capacity * CountItemsInInventory(_ammoMagazineDefinition.Id));

			}

			if (_currentAmmoCount == 0 && _weaponSystem.UseAmmoReplenish) {

				if (_ammoMagazinesReloads < _weaponSystem.MaxAmmoReplenishments || _weaponSystem.MaxAmmoReplenishments == -1) {

					_ammoMagazinesReloads++;
					_ammoReloadPending = true;

				}
			
			}


		}

		internal int CountItemsInInventory(MyDefinitionId id) {

			if (_weaponBlock == null || !_weaponBlock.HasInventory)
				return 0;

			int result = 0;

			foreach (var item in _weaponBlock.GetInventory().GetItems()) {

				if (item.Content.GetId() == id)
					result += (int)item.Amount;

			}

			return result;
		
		}

		internal void GetTypesOfAmmoInInventory() {

			if (_weaponBlock == null || !_weaponBlock.HasInventory)
				return;

			_ammoSubtypesInInventory.Clear();

			foreach (var item in _weaponBlock.GetInventory().GetItems()) {

				if (!_ammoSubtypesInInventory.Contains(item.Content.GetId().SubtypeName))
					_ammoSubtypesInInventory.Add(item.Content.GetId().SubtypeName);

			}

			return;

		}

		internal virtual void ReloadWeapon() {

			if (!_ammoReloadPending)
				return;

			var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(_ammoMagazineDefinition.Id);
			MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = 1, Content = content };
			float freeSpace = (float)(_weaponBlock.GetInventory().MaxVolume - _weaponBlock.GetInventory().CurrentVolume);
			var amountToAdd = Math.Floor(freeSpace / _ammoMagazineDefinition.Volume);

			if (amountToAdd > _weaponSystem.AmmoReplenishClipAmount && _weaponSystem.AmmoReplenishClipAmount > -1) {

				var adjustedAmt = amountToAdd - _weaponSystem.AmmoReplenishClipAmount;
				amountToAdd = adjustedAmt;

			}

			if (amountToAdd > 0 && _weaponBlock.GetInventory().CanItemsBeAdded((MyFixedPoint)amountToAdd, _ammoMagazineDefinition.Id) == true) {

				_weaponBlock.GetInventory().AddItems((MyFixedPoint)amountToAdd, inventoryItem.Content);

			}

			_ammoReloadPending = false;
		
		}

		public virtual MyDefinitionId GetCurrentAmmoMagazineId() {

			if (_ammoMagazineDefinition != null)
				return _ammoMagazineDefinition.Id;

			return new MyDefinitionId();

		}

		internal void GetAmmoMagazineDefinition(MyDefinitionId id) {
		
			
		
		}

		public virtual IMyEntity CurrentTargetEntity() {

			return null;
		
		}

		public Vector3D GetWeaponPosition() {

			return _weaponBlock?.PositionComp != null ? _weaponBlock.GetPosition() : Vector3D.Zero;
		
		}

		public virtual void DetermineWeaponReadiness() {

			if (!WeaponIsFunctional()) {

				//Logger.MsgDebug("Weapon Not Functional", DebugTypeEnum.Weapon);
				_readyToFire = false;
				return;

			}

			_currentDistance = Vector3D.Distance(_weaponBlock.GetPosition(), _currentTargetWaypoint);
			RefreshAmmoDetails();

			if (CurrentAmmoCount == 0 && !_ammoReloadPending) {

				//Logger.MsgDebug("Weapon Lacks Ammo", DebugTypeEnum.Weapon);
				_readyToFire = false;
				return;

			}

			_readyToFire = true;

		}

		public virtual void GetWeaponDefinition(IMyTerminalBlock weapon) {

			MyWeaponBlockDefinition weaponBlockDef;

			if(!Utilities.WeaponBlockReferences.TryGetValue(weapon.SlimBlock.BlockDefinition.Id, out weaponBlockDef)){

				Logger.MsgDebug("No Weapon Definition Found For: " + weapon.SlimBlock.BlockDefinition.Id.ToString(), DebugTypeEnum.Weapon);

			}

			MyWeaponDefinition weaponDef;

			if (!MyDefinitionManager.Static.TryGetWeaponDefinition(weaponBlockDef.WeaponDefinitionId, out weaponDef)) {

				Logger.MsgDebug("Weapon Def Null", DebugTypeEnum.Weapon);
				return;

			}
	
			_weaponDefinition = weaponDef;
		
		}

		public void ToggleEnabled(bool enabled) {

			var funcBlock = _weaponBlock as IMyFunctionalBlock;

			if (funcBlock != null)
				funcBlock.Enabled = enabled;
		
		}

		public virtual bool IsReadyToFire(bool targetIsWaypoint, bool isBarrage = false) {

			return true;

		}

		public virtual bool IsBarrageWeapon() {

			return _isBarrage;
		
		}

	}

}