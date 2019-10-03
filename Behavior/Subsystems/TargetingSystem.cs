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
	
	public enum TargetTypeEnum{
		
		None,
		Coords,
		Player,
		Entity,
		Grid,
		Block
		
	}
	
	public enum TargetDistanceEnum{
		
		Any,
		Closest,
		Furthest
		
	}

    [Flags]
    public enum TargetFilterEnum {

        None = 0,
        IgnoreSafeZone = 1,
        IsBroadcasting = 2,
        IgnoreUnderground = 4,
        IncludeGridMinorityOwners = 8,


    }

    [Flags]
	public enum TargetRelationEnum{
		
        None = 0,
		Faction = 1,
		Neutral = 2,
		Enemy = 4,
        Friend = 8,
        Unowned = 16

	}
	
	[Flags]
	public enum TargetOwnerEnum{

        None = 0,
        Unowned = 1,
		Owned = 2,
		Player = 4,
		NPC = 8,
        All = 16
		
	}
	
	[Flags]
	public enum BlockTargetTypes{
		
		All = 1,
		Containers = 2,
		Decoys = 4,
		GravityBlocks = 8,
		Guns = 16,
		JumpDrive = 32,
		Power = 64,
		Production = 128,
		Propulsion = 256,
		Shields = 512,
		ShipControllers = 1024,
		Tools = 2048,
		Turrets = 4096,
		Communications = 8192
		
	}

    public struct PendingTargetData{

        public IMyPlayer DetectedPlayer;
        public IMyEntity DetectedEntity;
        public IMyCubeGrid DetectedCubeGrid;
        public IMyTerminalBlock DetectedBlock;

    }
	
	public class TargetingSystem{

        //Configurable
        public TargetTypeEnum TargetType;
        public TargetRelationEnum TargetRelation;
        public TargetDistanceEnum TargetDistance;
        public TargetOwnerEnum TargetOwner;
        public TargetFilterEnum TargetFilter;
        public BlockTargetTypes BlockFilter;
        public double MaximumTargetScanDistance;
        public bool UseProjectileLeadTargeting;
        public bool UseCollisionLeadTargeting;

        //Non-Configurable
        public IMyRemoteControl RemoteControl;

        public DateTime LastValidTarget;
        public DateTime LastNewTargetCheck;

        public bool NeedsTarget;
        public bool SearchingForTarget;
		public bool InvalidTarget;
        public bool TargetIsShielded;
		public Vector3D TargetCoords;
		public IMyPlayer TargetPlayer;
		public IMyEntity TargetEntity;
		public IMyCubeGrid TargetGrid;
		public IMyTerminalBlock TargetBlock;
		public long RequestedGridId;
		public long RequestedBlockId;

        public TargetEvaluation Target;

		public float PrimaryAmmoVelocity;

        public Random Rnd;
		
		public TargetingSystem(IMyRemoteControl remoteControl = null) {
			
			RemoteControl = null;

            LastValidTarget = MyAPIGateway.Session.GameDateTime;

            SearchingForTarget = false;
            TargetType = TargetTypeEnum.None;
			InvalidTarget = true;
            TargetIsShielded = false;
            TargetCoords = Vector3D.Zero;
			TargetPlayer = null;
			TargetEntity = null;
			TargetGrid = null;
			TargetBlock = null;
			RequestedGridId = 0;
			RequestedBlockId = 0;

            Target = null;

            TargetDistance = TargetDistanceEnum.Any;
			MaximumTargetScanDistance = 15000;

			BlockFilter = BlockTargetTypes.All;
			
			UseProjectileLeadTargeting = false;
			PrimaryAmmoVelocity = 0;

            Rnd = new Random();

            Setup(remoteControl);


        }
		
		private void Setup(IMyRemoteControl remoteControl) {

            if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false) {

                return;

            }

            if(string.IsNullOrWhiteSpace(remoteControl.CustomData) == false) {

                var descSplit = remoteControl.CustomData.Split('\n');

                foreach(var tag in descSplit) {

                    //TargetType
                    if(tag.Contains("[TargetType") == true) {

                        this.TargetType = TagHelper.TagTargetTypeEnumCheck(tag);

                    }

                    //TargetRelation
                    if(tag.Contains("[TargetRelation") == true) {

                        var tempValue = TagHelper.TagTargetRelationEnumCheck(tag);

                        if(this.TargetRelation.HasFlag(tempValue) == false) {

                            this.TargetRelation |= tempValue;

                        }

                    }

                    //TargetDistance
                    if(tag.Contains("[TargetDistance") == true) {

                        this.TargetDistance = TagHelper.TagTargetDistanceEnumCheck(tag);

                    }

                    //TargetOwner
                    if(tag.Contains("[TargetOwner") == true) {

                        var tempValue = TagHelper.TagTargetOwnerEnumCheck(tag);

                        if(this.TargetOwner.HasFlag(tempValue) == false) {

                            this.TargetOwner |= tempValue;

                        }

                    }

                    //TargetFilter
                    if(tag.Contains("[TargetFilter") == true) {

                        var tempValue = TagHelper.TagTargetFilterEnumCheck(tag);

                        if(this.TargetFilter.HasFlag(tempValue) == false) {

                            this.TargetFilter |= tempValue;

                        }

                    }

                    //BlockFilter
                    if(tag.Contains("[BlockFilter") == true) {

                        var tempValue = TagHelper.TagBlockTargetTypesCheck(tag);

                        if(this.BlockFilter.HasFlag(tempValue) == false) {

                            this.BlockFilter |= tempValue;

                        }

                    }

                    //MaximumTargetScanDistance
                    if(tag.Contains("[MaximumTargetScanDistance") == true) {

                        this.MaximumTargetScanDistance = TagHelper.TagDoubleCheck(tag, this.MaximumTargetScanDistance);

                    }

                    //UseProjectileLeadTargeting
                    if(tag.Contains("[UseProjectileLeadTargeting") == true) {

                        this.UseProjectileLeadTargeting = TagHelper.TagBoolCheck(tag);

                    }

                    //UseCollisionLeadTargeting
                    if(tag.Contains("[UseCollisionLeadTargeting") == true) {

                        this.UseCollisionLeadTargeting = TagHelper.TagBoolCheck(tag);

                    }

                }

            }

        }

		public void RequestTarget(long requestGridId = 0, long requestBlockId = 0){

            if(RAI_SessionCore.IsServer == false) {

                return;

            }

            if(this.RemoteControl == null || MyAPIGateway.Entities.Exist(this.RemoteControl?.SlimBlock?.CubeGrid) == false) {

                return;

            }

            this.SearchingForTarget = true;
            this.InvalidTarget = false;
            this.TargetIsShielded = false;
            this.TargetPlayer = null;
            this.TargetEntity = null;
            this.TargetGrid = null;
            this.TargetBlock = null;
            this.RequestedGridId = requestGridId;
			this.RequestedBlockId = requestBlockId;

            MyAPIGateway.Parallel.Start(() => {

                if(this.NeedsTarget == true && this.InvalidTarget == true) {

                    AcquireTarget();
                    this.Target = new TargetEvaluation(this.TargetEntity, this.TargetType);

                }

                if(this.NeedsTarget == true && this.InvalidTarget == false && this.Target != null) {

                    this.Target.Evaluate(this.RemoteControl, this.UseProjectileLeadTargeting, this.PrimaryAmmoVelocity, this.UseCollisionLeadTargeting);

                    if(this.Target.TargetExists == false) {

                        this.InvalidTarget = true;

                    }

                }

            });
			
		}

		//Parallel
		public void AcquireTarget(){

            //Players
            if(TargetType == TargetTypeEnum.Player) {

                this.TargetPlayer = TargetHelper.AcquirePlayerTarget(this.RemoteControl, this.MaximumTargetScanDistance, this.TargetRelation, this.TargetDistance);

                if(this.TargetPlayer == null) {

                    this.InvalidTarget = true;

                } else if(RAI_SessionCore.Instance.ShieldApiLoaded == true) {

                    this.LastValidTarget = MyAPIGateway.Session.GameDateTime;
                    this.TargetIsShielded = RAI_SessionCore.Instance.SApi.ProtectedByShield(this.TargetPlayer.Character);

                }

                this.TargetEntity = this.TargetPlayer.Character;

            }

            //Grids
            if(TargetType == TargetTypeEnum.Grid) {

                TargetHelper.AcquireGridTarget(this.RemoteControl, this.MaximumTargetScanDistance, this.TargetRelation, this.TargetDistance, this.TargetOwner, this.TargetFilter, this.RequestedGridId);

                if(this.TargetGrid == null) {

                    this.InvalidTarget = true;
                    return;

                } else if(RAI_SessionCore.Instance.ShieldApiLoaded == true) {

                    this.LastValidTarget = MyAPIGateway.Session.GameDateTime;
                    this.TargetIsShielded = RAI_SessionCore.Instance.SApi.ProtectedByShield(this.TargetGrid);

                }

            }

            //Blocks
            if(TargetType == TargetTypeEnum.Block) {

                TargetHelper.AcquireBlockTarget(this.RemoteControl, this.MaximumTargetScanDistance, this.TargetRelation, this.TargetDistance, this.TargetOwner, this.BlockFilter, this.TargetFilter, this.RequestedBlockId);

                if(this.TargetGrid == null) {

                    this.InvalidTarget = true;

                } else if(RAI_SessionCore.Instance.ShieldApiLoaded == true) {

                    this.LastValidTarget = MyAPIGateway.Session.GameDateTime;
                    this.TargetIsShielded = RAI_SessionCore.Instance.SApi.ProtectedByShield(this.TargetBlock.SlimBlock.CubeGrid);

                }

            }

            this.SearchingForTarget = false;

        }

		public Vector3D GetTargetPosition(){
			
			//Check Remote
			if(MyAPIGateway.Entities.Exist(this.RemoteControl) == false){
				
				this.InvalidTarget = true;
				return Vector3D.Zero;
				
			}
			
			//None
			if(this.TargetType == TargetTypeEnum.None){
				
				this.InvalidTarget = false;
				return Vector3D.Zero;
				
			}
			
			//Coords
			if(this.TargetType == TargetTypeEnum.Coords){
				
				this.InvalidTarget = false;
                return this.TargetCoords;
				
			}
			
			//Player
			if(this.TargetType == TargetTypeEnum.Player){
				
				if(this.TargetPlayer?.Character != null){
					
					this.InvalidTarget = false;

                    if(this.UseProjectileLeadTargeting == true && this.TargetPlayer.Character.Physics != null && this.RemoteControl.SlimBlock.CubeGrid.Physics != null){
						
						//return VectorHelpers.GetProjectileLeadPosition(this.PrimaryAmmoVelocity, this.RemoteControl.GetPosition(), this.RemoteControl.SlimBlock.CubeGrid.Physics.LinearVelocity, this.TargetPlayer.GetPosition(), this.TargetPlayer.Character.Physics.LinearVelocity);
						return this.TargetPlayer.Character.GetPosition(); //Remove Later
						
					}
					
					return this.TargetPlayer.Character.GetPosition();
					
				}
				
				this.InvalidTarget = true;
				return Vector3D.Zero;
				
			}
			
			//Entity
			if(this.TargetType == TargetTypeEnum.Entity){
				
				if(MyAPIGateway.Entities.Exist(this.TargetEntity) == true && this.TargetEntity != null){
					
					this.InvalidTarget = false;
                    return this.TargetEntity.GetPosition();
					
				}
				
				this.InvalidTarget = true;
				return Vector3D.Zero;
				
			}
			
			//Grid
			if(this.TargetType == TargetTypeEnum.Grid){

                if(MyAPIGateway.Entities.Exist(this.TargetGrid) == true && this.TargetGrid != null) {

                    this.InvalidTarget = false;

                    if(this.UseProjectileLeadTargeting == false || this.TargetGrid.IsStatic == true || this.TargetGrid.Physics == null) {

                        return this.TargetGrid.PositionComp.WorldAABB.Center;

                    } else {

                        //return VectorHelpers.GetProjectileLeadPosition(this.PrimaryAmmoVelocity, this.RemoteControl.GetPosition(), this.RemoteControl.SlimBlock.CubeGrid.Physics.LinearVelocity, this.TargetPlayer.GetPosition(), this.TargetPlayer.Character.Physics.LinearVelocity);
						return this.TargetGrid.PositionComp.WorldAABB.Center; //Remove Later
						
                    }

                }

                this.InvalidTarget = true;
                return Vector3D.Zero;

            }
			
			//Block
			if(this.TargetType == TargetTypeEnum.Block){
				
				if(MyAPIGateway.Entities.Exist(this.TargetBlock?.SlimBlock?.CubeGrid) == true && this.TargetBlock != null) {
					
					this.InvalidTarget = false;
					
					if(this.UseProjectileLeadTargeting == false || this.TargetBlock.SlimBlock.CubeGrid.IsStatic == true || this.TargetBlock.SlimBlock.CubeGrid.Physics == null) {

                        return this.TargetBlock.GetPosition();

                    } else {

                        //return VectorHelpers.GetProjectileLeadPosition(this.PrimaryAmmoVelocity, this.RemoteControl.GetPosition(), this.RemoteControl.SlimBlock.CubeGrid.Physics.LinearVelocity, this.TargetPlayer.GetPosition(), this.TargetPlayer.Character.Physics.LinearVelocity);
						return this.TargetBlock.GetPosition(); //Remove Later
						
                    }
					
				}
				
				this.InvalidTarget = true;
				return Vector3D.Zero;
				
			}
			
			this.InvalidTarget = true;
			return Vector3D.Zero;
			
		}
		
	}
		
}