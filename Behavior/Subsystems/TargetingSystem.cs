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

namespace RivalAI.Behavior.Subsystems{
	
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

        public TargetProfile TargetData;

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
        public event Action<TargetEvaluation> WeaponTrigger;

        public float PrimaryAmmoVelocity;

        public Random Rnd;
		
		public TargetingSystem(IMyRemoteControl remoteControl = null) {
			
			RemoteControl = null;

            LastValidTarget = MyAPIGateway.Session.GameDateTime;

            NeedsTarget = false;
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

            Target = new TargetEvaluation(null, TargetTypeEnum.None);

            TargetData = new TargetProfile();
            TargetDistance = TargetDistanceEnum.Closest;
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

            this.RemoteControl = remoteControl;

        }

        public void InitTags() {

            if(string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == false) {

                var descSplit = this.RemoteControl.CustomData.Split('\n');

                foreach(var tag in descSplit) {

                    //TargetData
                    if(tag.Contains("[TargetData:") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            byte[] byteData = { };

                            if(TagHelper.TargetObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

                                try {

                                    var profile = MyAPIGateway.Utilities.SerializeFromBinary<TargetProfile>(byteData);

                                    if(profile != null) {

                                        this.TargetData = profile;

                                    }

                                } catch(Exception) {



                                }

                            }

                        }

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

            this.TargetIsShielded = false;
            this.TargetPlayer = null;
            this.TargetEntity = null;
            this.TargetGrid = null;
            this.TargetBlock = null;
            this.RequestedGridId = requestGridId;
			this.RequestedBlockId = requestBlockId;

            MyAPIGateway.Parallel.Start(() => {

                //Logger.AddMsg("Get Target", true);
                try {

                    if(this.NeedsTarget == true && this.InvalidTarget == true) {

                        AcquireTarget();
                        this.Target = new TargetEvaluation(this.TargetEntity, this.TargetData.Target);
                        this.Target.TargetPlayer = this.TargetPlayer;
                        this.InvalidTarget = false;

                    }

                    if(this.NeedsTarget == true && this.InvalidTarget == false && this.Target != null) {

                        //Logger.AddMsg("Eva Target", true);
                        this.Target.Evaluate(this.RemoteControl, this.TargetData);
                        //Logger.AddMsg("Target Coords: " + this.Target.TargetCoords.ToString(), true);


                        if(this.Target.TargetExists == false) {

                            //Logger.AddMsg("Inv Target", true);
                            this.InvalidTarget = true;

                        }

                    }

                } catch(Exception exc) {

                    Logger.AddMsg("Acquire Target Exception", true);
                    Logger.AddMsg(exc.ToString(), true);

                }

                

            }, () => {

                MyAPIGateway.Utilities.InvokeOnGameThread(() => {

                    WeaponTrigger?.Invoke(Target);

                });

            });
			
		}

		//Parallel
		public void AcquireTarget(){

            this.SearchingForTarget = true;
            this.InvalidTarget = false;

            //Players
            if(TargetData.Target == TargetTypeEnum.Player) {

                try {

                    this.TargetPlayer = TargetHelper.AcquirePlayerTarget(this.RemoteControl, this.TargetData);

                } catch(Exception exc) {

                    Logger.AddMsg("Exception Getting Player Target: ", true); //

                }
                

                if(this.TargetPlayer == null) {

                    //Logger.AddMsg("Cannot Find Player", true);
                    this.InvalidTarget = true;
                    this.SearchingForTarget = false;
                    return;

                }

                //Logger.AddMsg("Got Player", true);
                this.LastValidTarget = MyAPIGateway.Session.GameDateTime;
                this.TargetEntity = this.TargetPlayer.Controller.ControlledEntity.Entity;
 
            }

            //Grids
            if(TargetData.Target == TargetTypeEnum.Grid) {

                TargetHelper.AcquireGridTarget(this.RemoteControl, this.TargetData, this.RequestedGridId);

                if(this.TargetGrid == null) {

                    this.InvalidTarget = true;
                    this.SearchingForTarget = false;
                    return;

                }

                this.LastValidTarget = MyAPIGateway.Session.GameDateTime;
                this.TargetEntity = this.TargetGrid;

            }

            //Blocks
            if(TargetData.Target == TargetTypeEnum.Block) {

                TargetHelper.AcquireBlockTarget(this.RemoteControl, this.TargetData, this.RequestedBlockId);

                if(this.TargetBlock == null) {

                    //Logger.AddMsg("No Block Found", true);
                    this.InvalidTarget = true;
                    this.SearchingForTarget = false;
                    return;

                }

                this.LastValidTarget = MyAPIGateway.Session.GameDateTime;
                this.TargetEntity = this.TargetBlock;

            }

            this.SearchingForTarget = false;

        }

		public Vector3D GetTargetPosition(Vector3D originalCoords = new Vector3D()){

			//None
			if(this.TargetData.Target == TargetTypeEnum.None){
				
				this.InvalidTarget = false;
				return originalCoords;
				
			}
			
			//Coords
			if(this.TargetData.Target == TargetTypeEnum.Coords){
				
				this.InvalidTarget = false;
                return this.TargetCoords;
				
			}
			
            if(this.Target.TargetExists == true) {

                return Target.TargetCoords;

            }
			

			this.InvalidTarget = true;
			return originalCoords;
			
		}
		
	}
		
}