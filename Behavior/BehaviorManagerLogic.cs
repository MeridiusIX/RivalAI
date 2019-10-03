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
using Sandbox.Game.Lights;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
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

namespace RivalAI.Behavior{
	
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_RemoteControl), false, "RivalAIRemoteControlSmall", "RivalAIRemoteControlLarge")]
	 
	public class BehaviorManagerLogic : MyGameLogicComponent{

        //Block and Entity
        public IMyRemoteControl RemoteControl;

        //Emissive
        public Color PreviousEmissiveColor;

		//Behavior
		public event Action BehaviorRun;
		public event Action BehaviorRemove;
		public CoreBehavior CoreBehaviorInstance;
		public Fighter FighterBehaviorInstance;

        //Setup

        
        bool ValidBehavior = false;
		bool SetupComplete = false;
		bool SetupFailed = false;
		
		public override void Init(MyObjectBuilder_EntityBase objectBuilder){
			
			base.Init(objectBuilder);
			
			try{
				
				RemoteControl = Entity as IMyRemoteControl;
				NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;

            } catch(Exception exc){
				
				//Behavior Logic Init Failed
				
			}
			
		}
		
		public override void UpdateBeforeSimulation(){
			
			if(SetupComplete == false){
				
				SetupComplete = true;
				RemoteControl = Entity as IMyRemoteControl;
				
				if(string.IsNullOrEmpty(RemoteControl?.CustomData) == true){
				
					Logger.AddMsg("Remote Control Null Or Has No Behavior Data In CustomData.", true);
					return;
					
				}

                //Emissive
                PreviousEmissiveColor = Color.Blue;

                //CoreBehavior
                if(RemoteControl.CustomData.Contains("[BehaviorName:CoreBehavior]")) {
					
					ValidBehavior = true;
					CoreBehaviorInstance = new CoreBehavior();
					CoreBehaviorInstance.CoreSetup(RemoteControl);
					BehaviorRun += CoreBehaviorInstance.RunCoreAi;

				}
				
				//Fighter
				if(RemoteControl.CustomData.Contains("[BehaviorName:Fighter]")) {
					
					ValidBehavior = true;
					FighterBehaviorInstance = new Fighter();
					FighterBehaviorInstance.CoreSetup(RemoteControl);
					BehaviorRun += FighterBehaviorInstance.RunAi;

				}
				
			}

            if(ValidBehavior == true) {

                BehaviorRun?.Invoke();

            }

        }

        public override void UpdateBeforeSimulation100() {

            

        }

        public override void OnRemovedFromScene(){
			
			base.OnRemovedFromScene();
			
			var Block = Entity as IMyRemoteControl;
			
			if(Block == null){
				
				return;
				
			}
			
			//Unregister any handlers here
			
		}
		
		public override void OnBeforeRemovedFromContainer(){
			
			base.OnBeforeRemovedFromContainer();
			
			if(Entity.InScene == true){
				
				OnRemovedFromScene();
				
			}
			
		}
		
	}
	
}