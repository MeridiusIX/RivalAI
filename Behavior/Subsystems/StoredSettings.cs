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
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Behavior.Subsystems{
	
	[ProtoContract]
	public class StoredSettings{
		
		[ProtoMember(1)]
		public BehaviorMode Mode;

		[ProtoMember(2)]
		public long CurrentTargetEntityId;

		[ProtoMember(3)]
		public Dictionary<string, bool> StoredCustomBooleans;

		[ProtoMember(4)]
		public Dictionary<string, int> StoredCustomCounters;

		[ProtoMember(5)]
		public List<TriggerProfile> Triggers;

		[ProtoMember(6)]
		public List<TriggerProfile> DamageTriggers;

		[ProtoMember(7)]
		public List<TriggerProfile> CommandTriggers;

		[ProtoMember(8)]
		public float TotalDamageAccumulated;

		[ProtoMember(9)]
		public DateTime LastDamageTakenTime;

		public StoredSettings(){
			
			Mode = BehaviorMode.Init;
			CurrentTargetEntityId = 0;
			StoredCustomBooleans = new Dictionary<string, bool>();
			StoredCustomCounters = new Dictionary<string, int>();

			Triggers = new List<TriggerProfile>();
			DamageTriggers = new List<TriggerProfile>();
			CommandTriggers = new List<TriggerProfile>();

			TotalDamageAccumulated = 0;
			LastDamageTakenTime = MyAPIGateway.Session.GameDateTime;

		}
		
		public bool GetCustomBoolResult(string name){

			if (string.IsNullOrWhiteSpace(name))
				return false;

			bool result = false;
			this.StoredCustomBooleans.TryGetValue(name, out result);
			return result;
			
		}
		
		public bool GetCustomCounterResult(string varName, int target){

			if (string.IsNullOrWhiteSpace(varName)) {

				Logger.MsgDebug("Counter String Name Null", DebugTypeEnum.Dev);
				return false;

			}
				

			int result = 0;
			this.StoredCustomCounters.TryGetValue(varName, out result);
			Logger.MsgDebug(varName + ": " + result.ToString() + " / " + target.ToString(), DebugTypeEnum.Condition);
			return (result >= target);
			
		}
		
		public void SetCustomBool(string name, bool value){

			if (string.IsNullOrWhiteSpace(name))
				return;

			if (this.StoredCustomBooleans.ContainsKey(name)){
				
				this.StoredCustomBooleans[name] = value;
				
			}else{
				
				this.StoredCustomBooleans.Add(name, value);
				
			}
			
		}
		
		public void SetCustomCounter(string name, int value, bool reset = false){

			if (string.IsNullOrWhiteSpace(name))
				return;

			if(this.StoredCustomCounters.ContainsKey(name)){

				if (reset) {

					this.StoredCustomCounters[name] = 0;

				} else {

					//Logger.AddMsg("Increased Counter", true);
					this.StoredCustomCounters[name] += value;

				}

			}else{

				//Logger.AddMsg("Increased Counter", true);
				this.StoredCustomCounters.Add(name, value);
				
			}
			
		}
		
	}
	
}
	
	