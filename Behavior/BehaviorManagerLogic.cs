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
	
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_RemoteControl), false, "RivalAIRemoteControlSmall", "RivalAIRemoteControlLarge", "K_Imperial_Dropship_Guild_RC", "K_TIE_Fighter_RC", "K_Imperial_SpeederBike_FakePilot", "K_Imperial_ProbeDroid_Top_II", "K_Imperial_DroidCarrier_DroidBrain", "K_Imperial_DroidCarrier_DroidBrain_Aggressor", "K_NewRepublic_EWing_RC", "K_Imperial_RC_Largegrid", "K_TIE_Drone_Core")]
	 
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
		private IBehavior MainBehavior;
		private CoreBehavior CoreBehaviorInstance;
		private Fighter FighterBehaviorInstance;
		private Horsefly HorseflyBehaviorInstance;
		private Passive PassiveBehaviorInstance;
		private Strike StrikeBehaviorInstance;

		//Behavior Change
		public bool BehaviorChangeRequest = false;
		public int BehaviorChangeTimeBuffer = 0;
		public string BehaviorProfileChangeTo = "";

		//Setup
		private bool ValidBehavior = false;
		private bool SetupComplete = false;
		private bool SetupFailed = false;

		//Counter
		private int _counter = 0;
		
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

			/*
			_counter++;

			if (_counter < 10)
				return;
				*/
			NeedsUpdate = MyEntityUpdateEnum.NONE;

			if(SetupComplete == false){

				if(MyAPIGateway.Multiplayer.IsServer == false) {

					return;

				}

				SetupComplete = true;
				RemoteControl = Entity as IMyRemoteControl;

				if(string.IsNullOrEmpty(RemoteControl?.CustomData) == true){
				
					Logger.MsgDebug("Remote Control Null Or Has No Behavior Data In CustomData.", DebugTypeEnum.General);
					return;
					
				}

				if(RemoteControl.CustomData.Contains("[RivalAI Behavior]") == false && RemoteControl.CustomData.Contains("[Rival AI Behavior]") == false) {

					Logger.MsgDebug("Remote Control CustomData Does Not Contain Initializer.", DebugTypeEnum.General);
					return;

				}

				MyAPIGateway.Parallel.Start(() => {

					BehaviorManager.RegisterBehaviorFromRemoteControl(RemoteControl);

				});

			}

			if(ValidBehavior == true) {

				try {

					BehaviorRun?.Invoke();

				} catch(Exception exc) {

					Logger.MsgDebug("Exception Found In Behavior: " + BehaviorName + " / " + GridName, DebugTypeEnum.General);
					Logger.MsgDebug(exc.ToString(), DebugTypeEnum.General);

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