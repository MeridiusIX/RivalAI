using System;
using System.Collections.Generic;
using System.Text;
using RivalAI.Helpers;
using Sandbox.ModAPI;

namespace RivalAI.Behavior.Subsystems {
	public class NewWeaponSystem {

		//Configurable - Enabled Weapons
		public bool UseStaticGuns;
		public bool UseTurrets;
		public bool UseWeaponCore;

		//Configurable - Static Weapons
		public TargetObstructionEnum AllowedObstructions; //Voxel,Safezone,OtherGrids
		public double WeaponMaxAngleFromTarget;

		//Configurable - Barrage Fire
		public bool UseBarrageFire;
		public int MaxFireRateForBarrageWeapons;

		//Non-Configurable
		private IMyRemoteControl _remoteControl;

		private NewAutoPilotSystem _autoPilot;

		private List<IMyUserControllableGun> AllRegularWeapons;
		private List<IMyTerminalBlock> AllWeaponCoreBlocks;

		private List<NewWeaponProfile> Weapons;

		private bool _parallelWorkInProgress;
		private bool _pendingBarrageTrigger;

		private float _averageAmmoSpeed;

		public Action OnComplete;

		public NewWeaponSystem(IMyRemoteControl remoteControl = null) {

			UseStaticGuns = false;
			UseTurrets = false;
			UseWeaponCore = false;

			AllRegularWeapons = new List<IMyUserControllableGun>();
			AllWeaponCoreBlocks = new List<IMyTerminalBlock>();

			Weapons = new List<NewWeaponProfile>();

			if (remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid))
				return;

		}

		public void InitTags() {
		
			
		
		}

		public void CheckWeaponReadiness() {

			_parallelWorkInProgress = true;



			_parallelWorkInProgress = false;

		}

		public void FireWeapons() {

			if (_pendingBarrageTrigger) {

				_pendingBarrageTrigger = false;
				FireBarrageWeapons();

			}

			OnComplete?.Invoke();

		}

		public void FireBarrageWeapons() {

			if (_parallelWorkInProgress) {

				_pendingBarrageTrigger = true;
				return;

			}


					
		}

		public bool HasWorkingWeapons() {

			//Check if weapons are functional

			//Check if loaded (account for infinite)

			return true;
		
		}

		public long GetTurretTarget() {

			return 0;
		
		}

		public float MostCommonAmmoSpeed(bool calculationRefresh = false) {

			if(!calculationRefresh)
				return _averageAmmoSpeed;



			return 0;

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
