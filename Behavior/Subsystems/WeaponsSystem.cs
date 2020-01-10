using System;
using System.Collections.Generic;
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
using RivalAI;
using RivalAI.Behavior;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Behavior.Subsystems {

	public enum WeaponEngageModeEnum{
		
		Angle,
		Raycast,
		RaycastAndAngle,
		PredictionAndAngle
		
	}
	
	public class WeaponsSystem{

		//Configurable
		public bool UseStaticGuns;
		public double WeaponMaxAngleFromTarget;
		public bool UseBarrageFire;
		public int MaxFireRateForBarrageWeapons;
		public bool KeepWeaponsLoaded;
		public bool WeaponsAttackVoxels;
		public bool WeaponsAttackAnyGrids;

		//Non-Configurable
		public bool InitComplete;

		public IMyRemoteControl RemoteControl;
		public List<WeaponProfile> StaticWeapons;
		public List<IMyLargeTurretBase> Turrets;
		public List<IMyUserControllableGun> AllWeapons;
		public bool AllWeaponCollectionDone;
		public double HighestRangeStaticGun;

		public IMyEntity TurretTarget;

		public int BarrageInt;
		public bool EngageTargets;
		public bool UsingProjectileLead;
		public Vector3D TargetCoords;
		private TargetingSystem _targeting;
		public TargetEvaluation TargetEval;
		public double DistanceToTarget;

		public bool CanAnyWeaponFire;
 
		public WeaponsSystem(IMyRemoteControl remoteControl = null) {

			UseStaticGuns = false;
			WeaponMaxAngleFromTarget = 2;
			UseBarrageFire = false;
			MaxFireRateForBarrageWeapons = 300;
			KeepWeaponsLoaded = false;
			WeaponsAttackVoxels = false;
			WeaponsAttackAnyGrids = false;

			InitComplete = false;
			
			RemoteControl = null;
			StaticWeapons = new List<WeaponProfile>();
			Turrets = new List<IMyLargeTurretBase>();
			AllWeapons = new List<IMyUserControllableGun>();
			AllWeaponCollectionDone = false;
			HighestRangeStaticGun = 0;

			TurretTarget = null;

			BarrageInt = 0;
			EngageTargets = false;
			UsingProjectileLead = false;
			TargetCoords = Vector3D.Zero;
			TargetEval = new TargetEvaluation(null, TargetTypeEnum.None);
			DistanceToTarget = -1;

			CanAnyWeaponFire = true;

			Setup(remoteControl);

		}

		public void AllowFire() {

			this.EngageTargets = true;

		}

		public void CeaseFire() {

			this.EngageTargets = false;

			foreach(var weapon in this.StaticWeapons.ToList()) {

				if(weapon.CurrentlyFiring == true) {

					if(weapon.WeaponBlock != null && MyAPIGateway.Entities.Exist(weapon.WeaponBlock?.SlimBlock?.CubeGrid) != null) {

						weapon.CurrentlyFiring = false;
						weapon.ToggleFiring();

					} else {

						this.StaticWeapons.Remove(weapon);

					}

				}

			}

		}

		public void BarrageFire() {

			if(this.EngageTargets == false || this.UseBarrageFire == false || this.StaticWeapons.Count == 0) {

				return;

			}

			this.BarrageInt++;

			if(this.BarrageInt >= this.StaticWeapons.Count) {

				this.BarrageInt = 0;

			}

			if(this.StaticWeapons.Count > 0) {

				var weapon = this.StaticWeapons[this.BarrageInt];

				if(weapon.WeaponBlock != null && MyAPIGateway.Entities.Exist(weapon.WeaponBlock?.SlimBlock?.CubeGrid) != false) {

					weapon.SingleShot();

				}

			}
			

		}

		public void FireEligibleWeapons(TargetEvaluation target){

			if(this.UseStaticGuns == false || this.HighestRangeStaticGun == 0 || this.EngageTargets == false) {

				return;

			}

			this.TargetEval = target;

			bool hasTarget = false;
			double targetDistance = this.TargetEval.Distance;

			if(this.TargetEval.TargetExists == true) {

				if(this._targeting.TargetData.UseProjectileLead == true || this.TargetEval.TargetType == TargetTypeEnum.Player) {

					if(this.TargetEval.TargetAngle <= this.WeaponMaxAngleFromTarget && this.TargetEval.TargetObstruction != TargetObstructionEnum.Safezone) {

						hasTarget = true;
						this.TargetEval.Distance = this.TargetEval.TargetObstructionDistance;

					}

				} else {

					if(this.TargetEval.TargetObstruction != TargetObstructionEnum.Safezone && this.TargetEval.TargetObstruction != TargetObstructionEnum.None) {

						hasTarget = true;
						targetDistance = this.TargetEval.TargetObstructionDistance;

					}

				}

			}

			foreach(var weapon in this.StaticWeapons.ToList()) {

				if(weapon.WeaponBlock != null && MyAPIGateway.Entities.Exist(weapon.WeaponBlock?.SlimBlock?.CubeGrid) != null) {

					try {

						weapon.CheckWeaponReadiness(hasTarget, targetDistance, this.KeepWeaponsLoaded);

					} catch(Exception exc) {

						Logger.MsgDebug("Exception Detected Checking Weapon Readiness", DebugTypeEnum.Weapon);
						Logger.MsgDebug(exc.ToString(), DebugTypeEnum.Weapon);

					}
					

				} else {

					this.StaticWeapons.Remove(weapon);

				}

			}

			foreach(var weapon in this.StaticWeapons.ToList()) {

				if(this.UseBarrageFire == true && weapon.RateOfFire < this.MaxFireRateForBarrageWeapons) {

					continue;

				}

				if(weapon.WeaponBlock != null && MyAPIGateway.Entities.Exist(weapon.WeaponBlock?.SlimBlock?.CubeGrid) != null) {

					if(weapon.ReadyToFire == true && weapon.CurrentlyFiring == false) {

						weapon.ToggleFiring();

					}

					if(weapon.ReadyToFire == false && weapon.CurrentlyFiring == true) {

						weapon.ToggleFiring();

					}

				} else {

					this.StaticWeapons.Remove(weapon);

				}

			}

		}

		private void Setup(IMyRemoteControl remoteControl){

			if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false) {

				return;

			}

			this.RemoteControl = remoteControl;

			var allBlocks = TargetHelper.GetAllBlocks(remoteControl.SlimBlock.CubeGrid);
			this.StaticWeapons.Clear();

			foreach(var block in allBlocks.Where(x => x.FatBlock != null)) {

				if((block.FatBlock as IMyLargeTurretBase) != null) {

					this.AllWeapons.Add(block.FatBlock as IMyUserControllableGun);
					Turrets.Add(block.FatBlock as IMyLargeTurretBase);
					continue;

				}

				if((block.FatBlock as IMyUserControllableGun) != null) {

					this.AllWeapons.Add(block.FatBlock as IMyUserControllableGun);
					var weaponProfile = new WeaponProfile(block.FatBlock as IMyUserControllableGun);
					this.StaticWeapons.Add(weaponProfile);

					if(weaponProfile.AmmoRange > this.HighestRangeStaticGun) {

						this.HighestRangeStaticGun = weaponProfile.AmmoRange;

					}

				}

			}

			AllWeaponCollectionDone = true;

		}

		public void SetupReferences(TargetingSystem targeting) {

			_targeting = targeting;
			
		}

		public void InitTags() {

			if(string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach(var tag in descSplit) {

					//UseStaticGuns
					if(tag.Contains("[UseStaticGuns:") == true) {

						this.UseStaticGuns = TagHelper.TagBoolCheck(tag);

					}

					//WeaponMaxAngleFromTarget
					if(tag.Contains("[WeaponMaxAngleFromTarget:") == true) {

						this.WeaponMaxAngleFromTarget = TagHelper.TagDoubleCheck(tag, this.WeaponMaxAngleFromTarget);

					}

					//UseBarrageFire
					if(tag.Contains("[UseBarrageFire:") == true) {

						this.UseBarrageFire = TagHelper.TagBoolCheck(tag);

					}

					//KeepWeaponsLoaded
					if(tag.Contains("[KeepWeaponsLoaded:") == true) {

						this.KeepWeaponsLoaded = TagHelper.TagBoolCheck(tag);

					}

					//WeaponsAttackVoxels
					if(tag.Contains("[WeaponsAttackVoxels:") == true) {

						this.WeaponsAttackVoxels = TagHelper.TagBoolCheck(tag);

					}

					//WeaponsAttackAnyGrids
					if(tag.Contains("[WeaponsAttackAnyGrids:") == true) {

						this.WeaponsAttackAnyGrids = TagHelper.TagBoolCheck(tag);

					}

				}

			}

		}

	}
	
}