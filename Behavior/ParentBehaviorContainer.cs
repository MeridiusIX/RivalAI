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

namespace RivalAI.Behavior{
	
	public class ParentBehaviorContainer{

        public IMyEntity RemoteControlEntity;

		public long RemoteControlId = 0;
        
		public bool ValidBehavior = false;
		public bool MarkedForRemoval = false;

        public bool PhysicsDetected = false;
		
		public event Action BehaviorRun;
		public CoreBehavior CoreBehaviorInstance;
		
		public void InitializeBehavior(IMyRemoteControl remoteControl){
			
			var baseSettings = new CoreSettings();
			this.RemoteControlId = remoteControl.EntityId;
			
			if(baseSettings.CoreTryParse(remoteControl.CustomData) == false || string.IsNullOrEmpty(remoteControl.CustomData) == true){
				
				Logger.AddMsg("Remote Control Has No Behavior Data In CustomData. EntityID: " + remoteControl.EntityId.ToString(), true);
				return;
				
			}
			
			//TemplateBehavior
			if(baseSettings.BehaviorName == "CoreBehavior") {
				
				ValidBehavior = true;
				CoreBehaviorInstance = new CoreBehavior();
                CoreBehaviorInstance.CoreSetup(remoteControl);
                BehaviorRun += CoreBehaviorInstance.RunCoreAi;


            }
			
		}
		
		public void RunBehavior(){
			
			if(MarkedForRemoval == true){
				
				return;
				
			}

           

            if(LogicManager.RemoveAIEntityIDs.Contains(RemoteControlId) == true){
				
				LogicManager.RemoveAIEntityIDs.Remove(RemoteControlId);
				LogicManager.ActiveAIEntityIDs.Remove(RemoteControlId);
				MarkedForRemoval = true;
				Logger.AddMsg("Behavior Marked For Removal: " + RemoteControlId.ToString());
				return;
				
			}
			
		}
		
	}
	
}