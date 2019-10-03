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
using RivalAI;
using RivalAI.Behavior;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;

namespace RivalAI.Helpers{

	public class WeaponProfile{
		
		public IMyUserControllableGun WeaponBlock;
		public IMyGunObject<MyGunBase> GunBase;
		public bool ReadyToFire;
		public bool HasValidTarget;
		public bool CurrentlyFiring;
		
		public MyDefinitionId CurrentAmmoId;
		public float AmmoRange;
		public bool IsAmmoExplosive;
		
		public bool IsEnergyWeapon;
		public double EnergyWeaponRange;
		public bool EnergyWeaponRegularDamage;
		public bool EnergyWeaponExplosiveDamage;
		public bool EnergyWeaponVoxelDamage;
		public bool EnergyWeaponTeslaDamage;
		public bool EnergyWeaponJumpDamage;
		public bool EnergyWeaponShieldDamage;
		public bool EnergyWeaponHackDamage;
		
		public WeaponProfile(IMyUserControllableGun weapon) {
			
			WeaponBlock = null;
			GunBase = null;
			ReadyToFire = false;
			HasValidTarget = false;
			CurrentlyFiring = false;
			
			CurrentAmmoId = new MyDefinitionId();
			AmmoRange = 0;
			IsAmmoExplosive = false;
			
			IsEnergyWeapon = false;
			EnergyWeaponRange = 0;
			EnergyWeaponRegularDamage = false;
			EnergyWeaponExplosiveDamage = false;
			EnergyWeaponVoxelDamage = false;
			EnergyWeaponTeslaDamage = false;
			EnergyWeaponJumpDamage = false;
			EnergyWeaponShieldDamage = false;
			EnergyWeaponHackDamage = false;
            SetupWeapon(weapon);
			
		}
		
		public void ToggleFiring(){

            var ingameWeapon = WeaponBlock as Sandbox.ModAPI.Ingame.IMyUserControllableGun;

            if(this.ReadyToFire == true && this.HasValidTarget == true && this.CurrentlyFiring == false){
				
				this.CurrentlyFiring = true;
                Sandbox.ModAPI.Ingame.TerminalBlockExtentions.ApplyAction(WeaponBlock, "Shoot_On");

            }
			
			if((this.ReadyToFire == false || this.HasValidTarget == false) && this.CurrentlyFiring == true){
				
				this.CurrentlyFiring = false;
                Sandbox.ModAPI.Ingame.TerminalBlockExtentions.ApplyAction(WeaponBlock, "Shoot_Off");

            }
			
		}
		
		public bool SingleShot(){

            var ingameWeapon = WeaponBlock as Sandbox.ModAPI.Ingame.IMyUserControllableGun;

            if(this.ReadyToFire == true && this.HasValidTarget == true){

                Sandbox.ModAPI.Ingame.TerminalBlockExtentions.ApplyAction(WeaponBlock, "ShootOnce");
                return true;
				
			}
			
			return false;
			
		}
		
		public void CheckWeaponReadiness(bool hasTarget, double targetDistance){
			
			if(this.WeaponBlock.IsFunctional == false || this.WeaponBlock.IsWorking == false){
				
				this.ReadyToFire = false;
				return;
				
			}

            this.HasValidTarget = hasTarget;

            if(hasTarget == false) {

                this.ReadyToFire = false;
                return;

            }
			
			this.CurrentAmmoId = GunBase.GunBase.CurrentAmmoMagazineId;
			this.AmmoRange = GunBase.GunBase.CurrentAmmoDefinition.MaxTrajectory;
			this.IsAmmoExplosive = GunBase.GunBase.CurrentAmmoDefinition.IsExplosive;

            if(this.AmmoRange < targetDistance) {

                this.ReadyToFire = false;
                return;

            }

			if(this.WeaponBlock.GetInventory(0).Empty() == true){
				
				if(this.GunBase.GunBase.CurrentAmmo == 0){
					
					this.ReadyToFire = false;
					return;
					
				}else{
					
					this.ReadyToFire = true;
					return;
					
				}
				
			}
			
			if(WeaponBlock.GetInventory(0).GetItemAmount((SerializableDefinitionId)this.CurrentAmmoId) == 0){
				
				if(GunBase.GunBase.SwitchAmmoMagazineToNextAvailable() == false){
					
					this.ReadyToFire = false;
					return;
					
				}
				
				this.CurrentAmmoId = GunBase.GunBase.CurrentAmmoMagazineId;
				this.AmmoRange = GunBase.GunBase.CurrentAmmoDefinition.MaxTrajectory;
				this.IsAmmoExplosive = GunBase.GunBase.CurrentAmmoDefinition.IsExplosive;
				
			}
			
			this.ReadyToFire = true;
			
		}
		
		private bool SetupWeapon(IMyUserControllableGun weapon){
			
			if(weapon as IMyLargeTurretBase != null){
				
				return false;
				
			}
			
			this.WeaponBlock = weapon;
			this.GunBase = (IMyGunObject<MyGunBase>)this.WeaponBlock;
            this.CurrentAmmoId = GunBase.GunBase.CurrentAmmoMagazineId;

            if(GunBase.GunBase.CurrentAmmoDefinition != null) {

                this.AmmoRange = GunBase.GunBase.CurrentAmmoDefinition.MaxTrajectory;

            }

            //Check If Weapon Is Custom Energy Weapon
            string energyWeaponData = "";
			
			if(MyAPIGateway.Utilities.GetVariable<string>("CEW-" + weapon.SlimBlock.BlockDefinition.Id.SubtypeName, out energyWeaponData) == true){
				
				IsEnergyWeapon = true;
				var dataSplit = energyWeaponData.Split('\n');
				
				foreach(var item in dataSplit){
					
					if(this.EnergyWeaponRange == 0){
						
						double range = 0;
					
						if(double.TryParse(item, out range) == true){
							
							this.EnergyWeaponRange = range;
							continue;
							
						}
						
					}
					
					var itemLower = item.ToLower();

					if(itemLower.StartsWith("regulardamage-true") == true){
						
						this.EnergyWeaponRegularDamage = true;
						continue;
						
					}
					
					if(itemLower.StartsWith("explosiondamage-true") == true){
						
						this.EnergyWeaponExplosiveDamage = true;
						continue;
						
					}
					
					if(itemLower.StartsWith("voxeldamage-true") == true){
						
						this.EnergyWeaponVoxelDamage = true;
						continue;
						
					}
					
					if(itemLower.StartsWith("tesladamage-true") == true){
						
						this.EnergyWeaponTeslaDamage = true;
						continue;
						
					}
					
					if(itemLower.StartsWith("jumpdamage-true") == true){
						
						this.EnergyWeaponJumpDamage = true;
						continue;
						
					}
					
					if(itemLower.StartsWith("shielddamage-true") == true){
						
						this.EnergyWeaponShieldDamage = true;
						continue;
						
					}
					
					if(itemLower.StartsWith("hackingdamage-true") == true){
						
						this.EnergyWeaponHackDamage = true;
						continue;
						
					}
					
				}
				
			}

            return true;
			
		}
		
	}	
	
}