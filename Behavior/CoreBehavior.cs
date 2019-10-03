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
using RivalAI.Behavior;
using RivalAI.Behavior.Settings;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI;

namespace RivalAI.Behavior {

    public enum BehaviorMode {

        Init,
        Idle,
        Retreat,
        ApproachWaypoint,
        ApproachTarget,
        EvadeCollision,
        KamikazeCollision,
        EngageTarget,
        WaitingForTarget

    }

    public class CoreBehavior{
		
		public IMyRemoteControl RemoteControl;
		public IMyCubeGrid CubeGrid;

		public BaseSystems Systems;
        public BehaviorMode Mode;
        public CoreBehaviorStatus Status;

        public bool SetupCompleted;
		public bool SetupFailed;
        public bool EndScript;

		public byte CoreCounter;

		public CoreBehavior(){
			
			RemoteControl = null;
			CubeGrid = null;

            Mode = BehaviorMode.Init;
            Status = new CoreBehaviorStatus();

            SetupCompleted = false;
			SetupFailed = false;
            EndScript = false;

            CoreCounter = 0;

        }
		
		public void RunCoreAi(){

            //MyVisualScriptLogicProvider.ShowNotificationToAll("AI Run / NPC: " + Systems.Owner.NpcOwned.ToString(), 16);

			if(Systems.Owner.NpcOwned == false || EndScript == true) {
				
				return;
				
			}

            RunCoreBehavior();
			
			CoreCounter++;


            if((CoreCounter % 10) == 0){
				
				
				
			}

            //15 Tick - Autopilot
            if((CoreCounter % 15) == 0) {

                Systems.AutoPilot.TargetCoords = Systems.Targeting.TargetCoords;
                Systems.AutoPilot.EngageAutoPilot();
                Systems.Weapons.BarrageFire();

            }

            if((CoreCounter % 30) == 0) {

                

            }

            //50 Tick - Collision Check
            if((CoreCounter % 50) == 0) {

                Systems.Collision.RequestCheckCollisions();

            }

            //55 Tick - Target Check
            if((CoreCounter % 55) == 0) {

                Systems.Targeting.RequestTarget();

            }

            if((CoreCounter % 60) == 0) {

                CoreCounter = 0;

                Systems.Despawn.ProcessTimers(Mode);

                if(Mode != BehaviorMode.Retreat) {

                    if(Systems.Despawn.DoRetreat == true) {

                        Mode = BehaviorMode.Retreat;

                    }

                }

                if(Systems.Despawn.DoDespawn == true) {

                    EndScript = true;
                    Systems.Despawn.DespawnGrid();
                    return;

                }
 
            }
			
		}
		
		//Runs Every Tick
		public void RunCoreBehavior(){
			
			
			
			//Above Here Runs On All Clients
			if(RAI_SessionCore.IsServer == false){
				
				return;
				
			}
			//Below Here Runs On Server Only
			
			
			
		}
		
		//Runs Every 10 Ticks
		public void RunCoreBehavior10(){

			if(RAI_SessionCore.IsServer == false){
				
				return;
				
			}

            //Weapon Systems

            
        }

        //Runs Every 30 Ticks (Approx 0.5 Second)
        public void RunCoreBehavior30() {


            if(RAI_SessionCore.IsServer == false) {

                return;

            }

            //Collision
            Systems.Collision.RequestCheckCollisions();

            //Target

            //Autopilot



        }

        //Runs Every 60 Ticks (Approx 1 Second)
        public void RunCoreBehavior60(){
			
			
			if(RAI_SessionCore.IsServer == false){
				
				return;
				
			}
			
            //Chat Stuff

			//Update Status and Save To Entity
			
		}
		
		public void CoreSetup(IMyRemoteControl remoteControl){
			
			if(remoteControl == null){
				
				SetupFailed = true;
				return;
				
			}
			
			this.RemoteControl = remoteControl;
			this.CubeGrid = remoteControl.SlimBlock.CubeGrid;
            Systems = new BaseSystems(remoteControl);
            Systems.SetupBaseSystems(remoteControl);
            Status = new CoreBehaviorStatus();
            //TODO: Try Get Existing CoreBehaviorStatus

        }
		
		
		
	}
	
}