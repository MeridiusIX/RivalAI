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

namespace RivalAI.Behavior.Subsystems{
	
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
        public bool WeaponsAttackVoxels;
        public bool WeaponsAttackAnyGrids;

        //Non-Configurable
        public bool InitComplete;

        public IMyRemoteControl RemoteControl;
        public List<WeaponProfile> StaticWeapons;
        public List<IMyLargeTurretBase> Turrets;
        public double HighestRangeStaticGun;

        public int BarrageInt;
        public bool EngageTargets;
        public bool UsingProjectileLead;
        public Vector3D TargetCoords;
        public double DistanceToTarget;
 
		public WeaponsSystem(IMyRemoteControl remoteControl = null) {

            UseStaticGuns = false;
            WeaponMaxAngleFromTarget = 2;
            UseBarrageFire = false;
            WeaponsAttackVoxels = false;
            WeaponsAttackAnyGrids = false;

            InitComplete = false;
			
			RemoteControl = null;
			StaticWeapons = new List<WeaponProfile>();
			Turrets = new List<IMyLargeTurretBase>();
            HighestRangeStaticGun = 0;

            BarrageInt = 0;
            EngageTargets = false;
            UsingProjectileLead = false;
            TargetCoords = Vector3D.Zero;
            DistanceToTarget = -1;

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

            if(RAI_SessionCore.IsServer == false || this.EngageTargets == false || this.UseBarrageFire == false || this.StaticWeapons.Count == 0) {

                return;

            }

            int originalBarrageInt = this.BarrageInt;
            this.BarrageInt++;

            while(originalBarrageInt != this.BarrageInt) {

                if(this.BarrageInt >= this.StaticWeapons.Count) {

                    this.BarrageInt = 0;

                }

                var weapon = this.StaticWeapons[this.BarrageInt];

                if(weapon.WeaponBlock != null && MyAPIGateway.Entities.Exist(weapon.WeaponBlock?.SlimBlock?.CubeGrid) != null) {

                    weapon.SingleShot();

                }

            }

        }

        public void FireEligibleWeapons(Vector3D targetCoords, bool usingProjectileLead){

            if(this.UseStaticGuns == false || this.HighestRangeStaticGun == 0 || this.EngageTargets == false) {

                return;

            }

            this.TargetCoords = targetCoords;
            this.UsingProjectileLead = usingProjectileLead;

            MyAPIGateway.Parallel.Start(() => {

                bool hasTarget = false;
                double targetDistance = Vector3D.Distance(this.TargetCoords, this.RemoteControl.GetPosition());
                CollisionDetectType closestThing = CollisionDetectType.None;
                double closestDistance = targetDistance;

                var safezoneCoords = TargetHelper.SafeZoneIntersectionCheck(this.RemoteControl.GetPosition(), this.RemoteControl.WorldMatrix.Forward, targetDistance);
                double safeZoneDistance = targetDistance + 100;

                if(safezoneCoords != Vector3D.Zero) {

                    safeZoneDistance = Vector3D.Distance(safezoneCoords, this.RemoteControl.GetPosition());

                    if(safeZoneDistance < closestDistance) {

                        closestDistance = safeZoneDistance;
                        closestThing = CollisionDetectType.SafeZone;

                    }

                }

                var voxelCoords = Vector3D.Zero;
                double voxelDistance = targetDistance + 100;

                if(this.WeaponsAttackVoxels == false) {

                    voxelCoords = TargetHelper.VoxelIntersectionCheck(this.RemoteControl.GetPosition(), this.RemoteControl.WorldMatrix.Forward, targetDistance);

                    if(voxelCoords != Vector3D.Zero) {

                        voxelDistance = Vector3D.Distance(voxelCoords, this.RemoteControl.GetPosition());

                        if(voxelDistance < closestDistance) {

                            closestDistance = voxelDistance;
                            closestThing = CollisionDetectType.Voxel;

                        }

                    }

                }

                var shieldCoords = TargetHelper.ShieldIntersectionCheck(this.RemoteControl.GetPosition(), this.RemoteControl.WorldMatrix.Forward, targetDistance, this.RemoteControl.OwnerId);
                double shieldDistance = targetDistance + 100;

                if(shieldCoords != Vector3D.Zero) {

                    shieldDistance = Vector3D.Distance(shieldCoords, this.RemoteControl.GetPosition());

                    if(shieldDistance < closestDistance) {

                        closestDistance = shieldDistance;
                        closestThing = CollisionDetectType.DefenseShield;

                    }

                }

                if(this.UsingProjectileLead == true) {

                    var angle = VectorHelper.GetAngleBetweenDirections(this.RemoteControl.WorldMatrix.Forward, Vector3D.Normalize(this.TargetCoords - this.RemoteControl.GetPosition()));

                    if(angle <= this.WeaponMaxAngleFromTarget && closestThing != CollisionDetectType.SafeZone) {

                        hasTarget = true;
                        targetDistance = closestDistance;

                    }

                } else {

                    IMyCubeGrid grid = null;
                    var gridCoords = TargetHelper.TargetIntersectionCheck(this.RemoteControl, this.RemoteControl.WorldMatrix.Forward, targetDistance, out grid);
                    double gridDistance = targetDistance + 100;
                    var relation = OwnershipHelper.GetTargetReputation(this.RemoteControl.OwnerId, grid);
                    bool allowFire = (relation.HasFlag(TargetRelationEnum.Enemy) == true || this.WeaponsAttackAnyGrids == true);

                    if(gridCoords != Vector3D.Zero && allowFire == true) {

                        gridDistance = Vector3D.Distance(gridCoords, this.RemoteControl.GetPosition());

                        if(gridDistance < closestDistance) {

                            closestDistance = gridDistance;
                            closestThing = CollisionDetectType.Grid;

                        }

                    }

                    if(closestThing != CollisionDetectType.SafeZone && closestThing != CollisionDetectType.None) {

                        hasTarget = true;
                        targetDistance = closestDistance;

                    }

                }

                foreach(var weapon in this.StaticWeapons.ToList()) {

                    if(weapon.WeaponBlock != null && MyAPIGateway.Entities.Exist(weapon.WeaponBlock?.SlimBlock?.CubeGrid) != null) {

                        weapon.CheckWeaponReadiness(hasTarget, targetDistance);

                    } else {

                        this.StaticWeapons.Remove(weapon);

                    }

                }

            }, () => {

                if(this.UseBarrageFire == true) {

                    return;

                }

                foreach(var weapon in this.StaticWeapons.ToList()) {

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

            });

        }

        private void Setup(IMyRemoteControl remoteControl){

            if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false) {

                return;

            }

            var allBlocks = TargetHelper.GetAllBlocks(remoteControl.SlimBlock.CubeGrid);
            this.StaticWeapons.Clear();

            foreach(var block in allBlocks.Where(x => x.FatBlock != null)) {

                if((block.FatBlock as IMyLargeTurretBase) != null) {

                    Turrets.Add(block.FatBlock as IMyLargeTurretBase);
                    continue;

                }

                if((block.FatBlock as IMyUserControllableGun) != null) {

                    var weaponProfile = new WeaponProfile(block.FatBlock as IMyUserControllableGun);
                    this.StaticWeapons.Add(weaponProfile);

                    if(weaponProfile.AmmoRange > this.HighestRangeStaticGun) {

                        this.HighestRangeStaticGun = weaponProfile.AmmoRange;

                    }

                }

            }

        }

    }
	
}