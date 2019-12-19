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


namespace RivalAI.Behavior.Subsystems{
	
	[ProtoContract]
	public class StoredSettings{
		
		public BehaviorMode Mode;

		public long CurrentTargetEntityId;
		
		public Dictionary<string, bool> StoredCustomBooleans;
		public Dictionary<string, int> StoredCustomCounters;
		
		public StoredSettings(){
			
			Mode = BehaviorMode.Init;
			CurrentTargetEntityId = 0;
			StoredCustomBooleans = new Dictionary<string, bool>();
			StoredCustomCounters = new Dictionary<string, int>();
			
		}
		
		public bool GetCustomBoolResult(string name){
			
			bool result = false;
			this.StoredCustomBooleans.TryGetValue(name, out result);
			return result;
			
		}
		
		public bool GetCustomCounterResult(string name, int target){
			
			int result = 0;
			this.StoredCustomCounters.TryGetValue(name, out result);
			return (result >= target);
			
		}
		
		public void SetCustomBool(string name, bool value){
			
			if(this.StoredCustomBooleans.ContainsKey(name)){
				
				this.StoredCustomBooleans[name] = value;
				
			}else{
				
				this.StoredCustomBooleans.Add(name, value);
				
			}
			
		}
		
		public void SetCustomCounter(string name, int value){
			
			if(this.StoredCustomCounters.ContainsKey(name)){
				
				this.StoredCustomCounters[name] += value;
				
			}else{
				
				this.StoredCustomCounters.Add(name, value);
				
			}
			
		}
		
	}
	
}
	
	