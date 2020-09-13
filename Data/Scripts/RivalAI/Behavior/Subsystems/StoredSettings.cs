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
using RivalAI.Behavior.Subsystems.Trigger;
using RivalAI.Behavior.Subsystems.AutoPilot;

namespace RivalAI.Behavior.Subsystems {

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

		[ProtoMember(10)]
		public string CustomTargetProfile;

		[ProtoMember(11)]
		public List<TriggerProfile> CompromisedTriggers;

		[ProtoMember(12)]
		public Direction RotationDirection;

		[ProtoMember(13)]
		public SerializableBlockOrientation BlockOrientation;

		[ProtoMember(14)]
		public Vector3D DespawnCoords;

		[ProtoMember(15)]
		public Vector3D StoredCoords;

		[ProtoMember(16)]
		public Vector3D StartCoords;

		[ProtoMember(17)]
		public long LastDamagerEntity;

		[ProtoMember(18)]
		public NewAutoPilotMode AutoPilotFlags;

		[ProtoMember(19)]
		public AutoPilotDataMode APDataMode;

		[ProtoMember(20)]
		public bool IgnoreTriggers;

		[ProtoMember(21)]
		public string PrimaryAutopilotId;

		[ProtoMember(22)]
		public string SecondaryAutopilotId;

		[ProtoMember(23)]
		public string TertiaryAutopilotId;

		public StoredSettings(){
			
			Mode = BehaviorMode.Init;
			CurrentTargetEntityId = 0;
			StoredCustomBooleans = new Dictionary<string, bool>();
			StoredCustomCounters = new Dictionary<string, int>();

			Triggers = new List<TriggerProfile>();
			DamageTriggers = new List<TriggerProfile>();
			CommandTriggers = new List<TriggerProfile>();
			CompromisedTriggers = new List<TriggerProfile>();

			TotalDamageAccumulated = 0;
			LastDamageTakenTime = MyAPIGateway.Session.GameDateTime;

			CustomTargetProfile = "";

			RotationDirection = Direction.Forward;
			BlockOrientation = new SerializableBlockOrientation(Base6Directions.Direction.Forward, Base6Directions.Direction.Up);

			DespawnCoords = Vector3D.Zero;
			StoredCoords = Vector3D.Zero;
			StartCoords = Vector3D.Zero;

			LastDamagerEntity = 0;

			AutoPilotFlags = NewAutoPilotMode.None;
			APDataMode = AutoPilotDataMode.Primary;

			PrimaryAutopilotId = "";
			SecondaryAutopilotId = "";
			TertiaryAutopilotId = "";

			IgnoreTriggers = false;

		}

		public StoredSettings(StoredSettings oldSettings, bool preserveSettings, bool preserveTriggers, bool preserveTargetProfile) : base() {

			//Stuff From Old Settings
			if (!preserveSettings)
				return;

			//this.Mode = oldSettings.Mode;
			this.StoredCustomBooleans = oldSettings.StoredCustomBooleans;
			this.StoredCustomCounters = oldSettings.StoredCustomCounters;
			this.TotalDamageAccumulated = oldSettings.TotalDamageAccumulated;
			this.LastDamageTakenTime = oldSettings.LastDamageTakenTime;
			this.RotationDirection = oldSettings.RotationDirection;
			this.BlockOrientation = oldSettings.BlockOrientation;
			this.DespawnCoords = oldSettings.DespawnCoords;
			this.StoredCoords = oldSettings.StoredCoords;
			this.StartCoords = oldSettings.StartCoords;
			this.LastDamagerEntity = oldSettings.LastDamagerEntity;
			this.AutoPilotFlags = oldSettings.AutoPilotFlags;
			this.APDataMode = oldSettings.APDataMode;

			//Triggers
			if (preserveTriggers) {

				this.Triggers = oldSettings.Triggers;
				this.DamageTriggers = oldSettings.DamageTriggers;
				this.CommandTriggers = oldSettings.CommandTriggers;
				this.CompromisedTriggers = oldSettings.CompromisedTriggers;

			} else {

				IgnoreTriggers = true;

			}

			//TargetProfile
			if (preserveTargetProfile) {

				this.CustomTargetProfile = oldSettings.CustomTargetProfile;
				this.CurrentTargetEntityId = oldSettings.CurrentTargetEntityId;

			}

			this.SetRotation(RotationDirection);

		}

		public bool GetCustomBoolResult(string name){

			if (string.IsNullOrWhiteSpace(name))
				return false;

			bool result = false;
			this.StoredCustomBooleans.TryGetValue(name, out result);
			return result;
			
		}
		
		public bool GetCustomCounterResult(string varName, int target, CounterCompareEnum compareType){

			if (string.IsNullOrWhiteSpace(varName)) {

				//Logger.MsgDebug("Counter String Name Null", DebugTypeEnum.Dev);
				return false;

			}
				

			int result = 0;
			this.StoredCustomCounters.TryGetValue(varName, out result);
			Logger.MsgDebug(varName + ": " + result.ToString() + " / " + target.ToString(), DebugTypeEnum.Condition);

			if(compareType == CounterCompareEnum.GreaterOrEqual)
				return (result >= target);

			if (compareType == CounterCompareEnum.Greater)
				return (result > target);

			if (compareType == CounterCompareEnum.Equal)
				return (result == target);

			if (compareType == CounterCompareEnum.NotEqual)
				return (result != target);

			if (compareType == CounterCompareEnum.Less)
				return (result < target);

			if (compareType == CounterCompareEnum.LessOrEqual)
				return (result <= target);

			return false;

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
		
		public void SetCustomCounter(string name, int value, bool reset = false, bool hardSet = false){

			if (string.IsNullOrWhiteSpace(name))
				return;

			if(this.StoredCustomCounters.ContainsKey(name)){

				if (hardSet) {

					this.StoredCustomCounters[name] = value;
					return;

				}

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

		public void SetRotation(Direction newDirection) {

			RotationDirection = newDirection;

			if (RotationDirection == Direction.Forward || RotationDirection == Direction.None)
				BlockOrientation = new MyBlockOrientation(Base6Directions.Direction.Forward, Base6Directions.Direction.Up);

			if (RotationDirection == Direction.Left)
				BlockOrientation = new MyBlockOrientation(Base6Directions.Direction.Left, Base6Directions.Direction.Up);

			if (RotationDirection == Direction.Backward)
				BlockOrientation = new MyBlockOrientation(Base6Directions.Direction.Forward, Base6Directions.Direction.Up);

			if (RotationDirection == Direction.Right)
				BlockOrientation = new MyBlockOrientation(Base6Directions.Direction.Forward, Base6Directions.Direction.Up);

			if (RotationDirection == Direction.Up)
				BlockOrientation = new MyBlockOrientation(Base6Directions.Direction.Up, Base6Directions.Direction.Backward);

			if (RotationDirection == Direction.Down)
				BlockOrientation = new MyBlockOrientation(Base6Directions.Direction.Down, Base6Directions.Direction.Forward);

		}

	}
	
}
	
	