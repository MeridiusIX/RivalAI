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
using RivalAI.Behavior.Settings;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;

namespace RivalAI.Helpers{

	public static class LogicManager{
		
		public static List<IMyRemoteControl> PendingEligibilityCheck = new List<IMyRemoteControl>();
		public static List<long> ActiveAIEntityIDs = new List<long>();
		public static List<long> RemoveAIEntityIDs = new List<long>();
		public static List<ParentBehaviorContainer> ActiveAI = new List<ParentBehaviorContainer>();

        public static int PendingCheckTimer = 0;

		public static void Setup(){

            var entityList = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entityList);

            foreach(var entity in entityList) {

                var cubeGrid = entity as IMyCubeGrid;

                if(cubeGrid == null) {

                    continue;

                }

                var blockList = TargetHelper.GetAllBlocks(cubeGrid);

                foreach(var block in blockList) {

                    if(block.FatBlock == null) {

                        continue;

                    }

                    if((block.FatBlock as IMyRemoteControl) == null) {

                        continue;

                    }

                    if(PendingEligibilityCheck.Contains(block.FatBlock as IMyRemoteControl) == false) {

                        PendingEligibilityCheck.Add(block.FatBlock as IMyRemoteControl);

                    }

                }

            }

            foreach(var remoteControl in PendingEligibilityCheck.ToList()) {

                CheckBlocksEligibility(remoteControl?.SlimBlock?.CubeGrid);

            }


            MyEntities.OnEntityCreate += EntityCreatedEvent;
			MyEntities.OnEntityDelete += EntityDeletedEvent;
			
		}
		
		public static void ProcessLists(){


            //PendingEligibilityCheck
            /*
            PendingCheckTimer++;

            if(PendingCheckTimer >= 30 * 60) {



            }
            */
            for(int i = ActiveAI.Count - 1; i >= 0; i--){
				
				ActiveAI[i].RunBehavior();
				
				if(ActiveAI[i].MarkedForRemoval == true){
					
					ActiveAI.RemoveAt(i);
					
				}
				
			}
			
		}
		
		//Entity Events - Create
		public static void EntityCreatedEvent(MyEntity entity){

			if(entity == null){
				
				return;
				
			}
			
			var remoteControl = entity as IMyRemoteControl;
			
			if(remoteControl == null){
				
				return;
				
			}

            Logger.AddMsg("New Remote Control Detected. Adding To Pending List", true);
			PendingEligibilityCheck.Add(remoteControl);
            remoteControl.SlimBlock.CubeGrid.OnPhysicsChanged += CheckBlocksEligibility;
			
		}

        //Entity Events - Delete
        public static void EntityDeletedEvent(MyEntity entity){

			if(entity == null){
				
				return;
				
			}
			
			var remoteControl = entity as IMyRemoteControl;

			if(remoteControl != null){

				if(ActiveAIEntityIDs.Contains(remoteControl.EntityId) == true){
					
					RemoveAIEntityIDs.Add(remoteControl.EntityId);
					
				}
				
			}else{
				
				var cubeGrid = entity as IMyCubeGrid;
				
				if(cubeGrid == null){
					
					return;
					
				}
				

				var blockList = new List<IMySlimBlock>();
				cubeGrid.GetBlocks(blockList);
				
				foreach(var block in blockList){
					
					if(block.FatBlock == null){
						
						continue;
						
					}
					
					var remote = block.FatBlock as IMyRemoteControl;
					
					if(remote == null){
						
						continue;
						
					}
					
					if(ActiveAIEntityIDs.Contains(remote.EntityId) == true){
						
						RemoveAIEntityIDs.Add(remote.EntityId);
						continue;
						
					}
					
				}
				
			}

		}
		
		public static void CheckBlocksEligibility(IMyEntity entity){

            var newCubeGrid = entity as IMyCubeGrid;

            if(newCubeGrid == null) {

                Logger.AddMsg("Physics Entity Not Grid", true);
                return;

            }

			foreach(var remoteControl in PendingEligibilityCheck.ToList()){

                if(remoteControl == null){

                    Logger.AddMsg("Pending Remote Null Or Non-Existent", true);
                    PendingEligibilityCheck.Remove(remoteControl);
                    continue;
					
				}
				
				if(remoteControl.CubeGrid.Physics == null){
					
					Logger.AddMsg("Remote Physics Null", true);
					continue;
					
				}
				
				//TODO: Add To ParentBehavior
				var parentBehavior = new ParentBehaviorContainer();
				parentBehavior.InitializeBehavior(remoteControl);
				
				if(parentBehavior.ValidBehavior == false){
					
					Logger.AddMsg("Behavior Not Set For Remote Control", true);
                    PendingEligibilityCheck.Remove(remoteControl);
                    continue;
					
				}
				
				if(ActiveAIEntityIDs.Contains(remoteControl.EntityId) == false){
					
					ActiveAI.Add(parentBehavior);
					ActiveAIEntityIDs.Add(remoteControl.EntityId);
					Logger.AddMsg("Remote Control Behavior Now Registered!: " + remoteControl.EntityId.ToString(), true);
                    PendingEligibilityCheck.Remove(remoteControl);



                } else{
					
					Logger.AddMsg("Remote Control Already Registered", true);
					
				}
				
			}
			
			//PendingEligibilityCheck.Clear();

		}
		
	}
	
}