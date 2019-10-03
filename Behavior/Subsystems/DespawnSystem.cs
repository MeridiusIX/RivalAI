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
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;

namespace RivalAI.Behavior.Subsystems{
	
	public class DespawnSystem{
		
		public IMyRemoteControl RemoteControl;
		
		public bool UsePlayerDistanceTimer;
		public int PlayerDistanceTimer;
		public int PlayerDistanceTimerTrigger;
		public double PlayerDistance;
		public double PlayerDistanceTrigger; //Storage
		
		public bool UseRetreatTimer;
        public int RetreatTimer;
        public int RetreatTimerTrigger; //Storage

        public double DespawnDistance;

        public bool DoDespawn;
        public bool DoRetreat;

        public DespawnSystem(IMyRemoteControl remoteControl = null) {
			
			RemoteControl = null;
			
			UsePlayerDistanceTimer = false;
			PlayerDistanceTimer = 0;
			PlayerDistanceTimerTrigger = 150;
			PlayerDistance = 0;
			PlayerDistanceTrigger = 25000;
			
			UseRetreatTimer = false;
			RetreatTimer = 0;
			RetreatTimerTrigger = 600;

            DespawnDistance = 3000;

            Setup(remoteControl);


        }
		
		private void Setup(IMyRemoteControl remoteControl){
			
			
			
		}
		
		public void ProcessTimers(BehaviorMode mode){
			
			if(RAI_SessionCore.IsServer == false || this.RemoteControl == null){
				
				return;
				
			}

            if(mode == BehaviorMode.Retreat) {

                var player = TargetHelper.GetClosestPlayer(this.RemoteControl.GetPosition());

                if(player?.Character != null) {



                } else {

                    DoDespawn = true;

                }

            }
			
			if(this.UsePlayerDistanceTimer == true){
				
				var player = TargetHelper.GetClosestPlayer(this.RemoteControl.GetPosition());
				
				if(player == null){
					
					PlayerDistanceTimer++;
					
				}else if(player.Character != null){

					if(Vector3D.Distance(player.GetPosition(), this.RemoteControl.GetPosition()) > PlayerDistance){
						
						PlayerDistanceTimer++;

                        if(PlayerDistanceTimer >= PlayerDistanceTimerTrigger) {

                            DoDespawn = true;

                        }
						
					}else{
						
						PlayerDistanceTimer = 0;
						
					}
					
				}
				
			}
			
			if(this.UseRetreatTimer == true){
				
				RetreatTimer++;

                if(RetreatTimer >= RetreatTimerTrigger) {

                    DoRetreat = true;

                }
				
			}
            			
		}

        public void DespawnGrid() {



        }
		
	}
	
}