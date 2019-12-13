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

namespace RivalAI.Behavior.Settings{
	
	public class CoreSettings{
		
        //Core
		public string BehaviorName;

        //General

        //Ownership
        public string FactionTag;

        //Despawn
        public bool DespawnUsingPlayerDistance;
        public double DespawnPlayerDistance;
        public int DespawnTimeIfNoPlayerInRange;

        //Chat System
        public bool UseChatSystem;
        public string ChatAuthor;
        public bool UseGreetingChat;
        public int GreetingChatChance;
        public double GreetingChatDistance;
        public string GreetingChatMessage;

        //Damage System
        public bool UseDamageAlert;
        public int DamageToTriggerAlert;

        //Collision Detection
        public bool UseCollisionDetection;

        //Targeting

        //Weapons - Static
        public bool UseStaticWeapons;
        public bool UseSequentialFire;
        public int SequentialFireTickRate;
        public bool GunsIgnoreVoxels;

        public CoreSettings(){
			
			BehaviorName = "";

			FactionTag = "";

            DespawnUsingPlayerDistance = true;
            DespawnPlayerDistance = 25000;
            DespawnTimeIfNoPlayerInRange = 60;

            UseChatSystem = false;
            ChatAuthor = "Drone";
            UseGreetingChat = false;
            GreetingChatChance = 100;
            GreetingChatDistance = 4000;
            GreetingChatMessage = "";

            UseCollisionDetection = false;

        }
		
		public bool CoreTryParse(string stringData){
			
			if(string.IsNullOrWhiteSpace(stringData) == true){
				
				return false;
				
			}
			
			var descSplit = stringData.Split('\n');
				
			foreach(var tag in descSplit){

				//BehaviorName
				if(tag.Contains("[BehaviorName") == true){

					this.BehaviorName = TagHelper.TagStringCheck(tag);
					
				}
				
				//FactionTag
				if(tag.Contains("[FactionTag") == true){

					this.FactionTag = TagHelper.TagStringCheck(tag);
					
				}
				
			}
			
			return true;
			
		}
		
		
		
		
	}
	
}