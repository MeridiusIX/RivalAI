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

        public IMyUserControllableGun WeaponBlock;
        public MyWeaponDefinition WeaponDefinition;
        public IMyGunObject<MyGunBase> GunBase;
        public int RateOfFire;
        public bool ReadyToFire;
        public bool HasValidTarget;
        public bool CurrentlyFiring;

        public MyDefinitionId CurrentAmmoId;
        public MyAmmoMagazineDefinition CurrentAmmoDefinition;
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
            RateOfFire = 0;
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

        public void ToggleFiring() {

            if(ReadyToFire == true && HasValidTarget == true && CurrentlyFiring == false) {

                CurrentlyFiring = true;
                Sandbox.ModAPI.Ingame.TerminalBlockExtentions.ApplyAction(WeaponBlock, "Shoot_On");

            }

            if((ReadyToFire == false || HasValidTarget == false) && CurrentlyFiring == true) {

                CurrentlyFiring = false;
                Sandbox.ModAPI.Ingame.TerminalBlockExtentions.ApplyAction(WeaponBlock, "Shoot_Off");

            }

        }

        public bool SingleShot() {

            if(ReadyToFire == true && HasValidTarget == true) {

                Sandbox.ModAPI.Ingame.TerminalBlockExtentions.ApplyAction(WeaponBlock, "ShootOnce");
                return true;

            }

            return false;

        }

        public void CheckWeaponReadiness(bool hasTarget, double targetDistance, bool keepLoaded) {

            if(WeaponBlock.IsFunctional == false || WeaponBlock.IsWorking == false) {

                ReadyToFire = false;
                return;

            }

            HasValidTarget = hasTarget;

            if(hasTarget == false) {

                ReadyToFire = false;
                return;

            }

            CurrentAmmoId = GunBase.GunBase.CurrentAmmoMagazineId;
            AmmoRange = GunBase.GunBase.CurrentAmmoDefinition.MaxTrajectory;
            IsAmmoExplosive = GunBase.GunBase.CurrentAmmoDefinition.IsExplosive;

            if(AmmoRange < targetDistance) {

                ReadyToFire = false;
                return;

            }

            if(WeaponBlock.GetInventory(0).Empty() == true) {

                if(keepLoaded == true) {

                    CheckAmmoMagazineDefinition(CurrentAmmoId);
                    ReloadWeapon();

                }

                if(GunBase.GunBase.CurrentAmmo == 0) {

                    ReadyToFire = false;
                    return;

                } else {

                    ReadyToFire = true;
                    return;

                }

            }

            if(WeaponBlock.GetInventory(0).GetItemAmount((SerializableDefinitionId)CurrentAmmoId) == 0) {

                if(GunBase.GunBase.SwitchAmmoMagazineToNextAvailable() == false) {

                    ReadyToFire = false;
                    return;

                }

                CurrentAmmoId = GunBase.GunBase.CurrentAmmoMagazineId;
                AmmoRange = GunBase.GunBase.CurrentAmmoDefinition.MaxTrajectory;
                IsAmmoExplosive = GunBase.GunBase.CurrentAmmoDefinition.IsExplosive;

            }

            ReadyToFire = true;

        }

        private bool SetupWeapon(IMyUserControllableGun weapon) {

            if(weapon as IMyLargeTurretBase != null) {

                return false;

            }

            WeaponBlock = weapon;
            Sandbox.ModAPI.Ingame.TerminalBlockExtentions.ApplyAction(weapon, "Shoot_Off");
            GunBase = (IMyGunObject<MyGunBase>)WeaponBlock;
            CurrentAmmoId = GunBase.GunBase.CurrentAmmoMagazineId;

            if(GunBase.GunBase.CurrentAmmoDefinition != null) {

                AmmoRange = GunBase.GunBase.CurrentAmmoDefinition.MaxTrajectory;

            }

            //Get Weapon Defintion
            try {

                var blockDef = weapon.SlimBlock.BlockDefinition as MyWeaponBlockDefinition;

                if(blockDef != null) {

                    MyWeaponDefinition tempWeapDef = null;

                    if(MyDefinitionManager.Static.TryGetWeaponDefinition(blockDef.WeaponDefinitionId, out tempWeapDef) == true) {

                        WeaponDefinition = tempWeapDef;

                        foreach(var ammoData in tempWeapDef.WeaponAmmoDatas) {

                            if(ammoData == null) {

                                continue;

                            }

                            Logger.AddMsg("Rate Of Fire: " + ammoData.RateOfFire.ToString(), true);
                            RateOfFire = ammoData.RateOfFire;
                            break;

                        }


                    }

                }

            } catch(Exception e) {

                Logger.AddMsg("Failed to get Weapon Definition for " + weapon.CustomName, true);
                Logger.AddMsg(e.ToString(), true);

            }

            //Check If Weapon Is Custom Energy Weapon
            string energyWeaponData = "";

            if(MyAPIGateway.Utilities.GetVariable("CEW-" + weapon.SlimBlock.BlockDefinition.Id.SubtypeName, out energyWeaponData) == true) {

                IsEnergyWeapon = true;
                var dataSplit = energyWeaponData.Split('\n');

                foreach(var item in dataSplit) {

                    if(EnergyWeaponRange == 0) {

                        double range = 0;

                        if(double.TryParse(item, out range) == true) {

                            EnergyWeaponRange = range;
                            continue;

                        }

                    }

                    var itemLower = item.ToLower();

                    if(itemLower.StartsWith("regulardamage-true") == true) {

                        EnergyWeaponRegularDamage = true;
                        continue;

                    }

                    if(itemLower.StartsWith("explosiondamage-true") == true) {

                        EnergyWeaponExplosiveDamage = true;
                        continue;

                    }

                    if(itemLower.StartsWith("voxeldamage-true") == true) {

                        EnergyWeaponVoxelDamage = true;
                        continue;

                    }

                    if(itemLower.StartsWith("tesladamage-true") == true) {

                        EnergyWeaponTeslaDamage = true;
                        continue;

                    }

                    if(itemLower.StartsWith("jumpdamage-true") == true) {

                        EnergyWeaponJumpDamage = true;
                        continue;

                    }

                    if(itemLower.StartsWith("shielddamage-true") == true) {

                        EnergyWeaponShieldDamage = true;
                        continue;

                    }

                    if(itemLower.StartsWith("hackingdamage-true") == true) {

                        EnergyWeaponHackDamage = true;
                        continue;

                    }

                }

            }

            return true;

        }

        private void CheckAmmoMagazineDefinition(MyDefinitionId defId) {

            if(defId == null) {

                return;

            }

            if(CurrentAmmoDefinition != null && CurrentAmmoDefinition.Id == defId) {

                return;

            }

            var ammoMag = MyDefinitionManager.Static.GetAmmoMagazineDefinition(defId);

            if(ammoMag == null) {

                return;

            }

            CurrentAmmoDefinition = ammoMag;

        }

        private void ReloadWeapon() {

            if(CurrentAmmoDefinition == null) {

                return;

            }

            //CreateAmmo
            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(CurrentAmmoDefinition.Id);
            var inventoryItem = new MyObjectBuilder_InventoryItem { Amount = 1, Content = content };

            //Get Inventory Volume
            float amtToAdd = (float)Math.Floor((float)(WeaponBlock.GetInventory(0).MaxVolume - WeaponBlock.GetInventory(0).CurrentVolume) / CurrentAmmoDefinition.Volume);

            //Add
            if(WeaponBlock.GetInventory(0).CanItemsBeAdded((MyFixedPoint)amtToAdd, CurrentAmmoDefinition.Id) == true) {

                Logger.AddMsg("Weapon Reloaded! " + amtToAdd.ToString(), true);
                WeaponBlock.GetInventory().AddItems((MyFixedPoint)amtToAdd, inventoryItem.Content);

            }


        }

    }

}