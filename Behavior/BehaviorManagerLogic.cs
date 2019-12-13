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
        private IMyRemoteControl RemoteControl;
        public string BehaviorName = "";
        public string GridName = "";

        //Emissive
        private Color PreviousEmissiveColor;

		//Behavior
		public event Action BehaviorRun;
		public event Action BehaviorRemove;
        private CoreBehavior CoreBehaviorInstance;
        private Fighter FighterBehaviorInstance;
        private Horsefly HorseflyBehaviorInstance;
        private Strike StrikeBehaviorInstance;

        //Behavior Change
        public bool BehaviorChangeRequest = false;
        public int BehaviorChangeTimeBuffer = 0;
        public string BehaviorProfileChangeTo = "";

        //Setup
        private bool ValidBehavior = false;
        private bool SetupComplete = false;
        private bool SetupFailed = false;
		
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

            if(BehaviorChangeRequest == true) {

                BehaviorChangeTimeBuffer++;

                if(BehaviorChangeTimeBuffer < 10) {

                    return;

                }

                BehaviorChangeTimeBuffer = 0;
                BehaviorChangeRequest = false;
                SetupComplete = false;
                ValidBehavior = false;
                //TODO: Replace Custom Data
                BehaviorProfileChangeTo = "";

            }

			if(SetupComplete == false){

                if(MyAPIGateway.Multiplayer.IsServer == false) {

                    NeedsUpdate = MyEntityUpdateEnum.NONE;
                    return;

                }

                BehaviorRun = null;
                BehaviorRemove = null;
                SetupComplete = true;
				RemoteControl = Entity as IMyRemoteControl;

                if(string.IsNullOrEmpty(RemoteControl?.CustomData) == true){
				
					Logger.AddMsg("Remote Control Null Or Has No Behavior Data In CustomData.", true);
                    NeedsUpdate = MyEntityUpdateEnum.NONE;
                    return;
					
				}

                if(RemoteControl.CustomData.Contains("[RivalAI Behavior]") == false && RemoteControl.CustomData.Contains("[Rival AI Behavior]") == false) {

                    Logger.AddMsg("Remote Control CustomData Does Not Contain Initializer.", true);
                    NeedsUpdate = MyEntityUpdateEnum.NONE;
                    return;

                }

                MyAPIGateway.Parallel.Start(() => {

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
                        BehaviorName = "Fighter";
                        GridName = RemoteControl.SlimBlock.CubeGrid.CustomName;
                        FighterBehaviorInstance.BehaviorInit(RemoteControl);
                        BehaviorRun += FighterBehaviorInstance.RunAi;

                    }

                    //Horsefly
                    if(RemoteControl.CustomData.Contains("[BehaviorName:Horsefly]")) {

                        ValidBehavior = true;
                        HorseflyBehaviorInstance = new Horsefly();
                        BehaviorName = "Horsefly";
                        GridName = RemoteControl.SlimBlock.CubeGrid.CustomName;
                        HorseflyBehaviorInstance.BehaviorInit(RemoteControl);
                        BehaviorRun += HorseflyBehaviorInstance.RunAi;

                    }

                    //Strike
                    if(RemoteControl.CustomData.Contains("[BehaviorName:Strike]")) {

                        ValidBehavior = true;
                        StrikeBehaviorInstance = new Strike();
                        BehaviorName = "Strike";
                        GridName = RemoteControl.SlimBlock.CubeGrid.CustomName;
                        StrikeBehaviorInstance.BehaviorInit(RemoteControl);
                        BehaviorRun += StrikeBehaviorInstance.RunAi;

                    }

                });

			}

            if(ValidBehavior == true) {

                try {

                    BehaviorRun?.Invoke();

                } catch(Exception exc) {

                    Logger.AddMsg("Exception Found In Behavior: " + BehaviorName + " / " + GridName, true);

                }


            }

        }

        public void ResetBehavior() {

            SetupComplete = false;

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