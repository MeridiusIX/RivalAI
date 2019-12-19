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


namespace RivalAI.Behavior.Subsystems.Profiles{
	
	[ProtoContract]
	public class ConditionProfile{
		
		[ProtoMember(1)]
		public bool UseConditions;
		
		[ProtoMember(2)]
		public bool MatchAnyCondition;
		
		[ProtoMember(3)]
		public bool CheckLoadedModIDs;
		
		[ProtoMember(4)]
		public List<long> ModIDsToCheck;
		
		[ProtoMember(5)]
		public bool CheckTrueBooleans;
		
		[ProtoMember(6)]
		public List<string> TrueBooleans;
		
		[ProtoMember(7)]
		public bool CheckCustomCounters;
		
		[ProtoMember(8)]
		public List<string> CustomCounters;
		
		[ProtoMember(9)]
		public List<string> CustomCountersTargets;
		
		[ProtoMember(10)]
		public bool CheckGridSpeed;
		
		[ProtoMember(11)]
		public float MinGridSpeed;
		
		[ProtoMember(12)]
		public float MaxGridSpeed;

        [ProtoMember(13)]
        public bool CheckMESBlacklistedSpawnGroups;

        [ProtoMember(14)]
        public List<string> SpawnGroupBlacklistContainsAll;

        [ProtoMember(15)]
        public List<string> SpawnGroupBlacklistContainsAny;

        [ProtoIgnore]
		private IMyRemoteControl _remoteControl;

        [ProtoIgnore]
        private StoredSettings _settings;



        public ConditionProfile(){
			
			UseConditions = false;
			MatchAnyCondition = false;
			
			CheckLoadedModIDs = false;
			ModIDsToCheck = new List<long>();
			
			CheckTrueBooleans = false;
			TrueBooleans = new List<string>();
			
			CheckCustomCounters = false;
			CustomCounters = new List<string>();
			
			CheckGridSpeed = false;
			MinGridSpeed = -1;
			MaxGridSpeed = -1;

            CheckMESBlacklistedSpawnGroups = false;
            SpawnGroupBlacklistContainsAll = new List<string>();
            SpawnGroupBlacklistContainsAny = new List<string>();

        }
		
		public void SetReferences(IMyRemoteControl remoteControl, StoredSettings settings){
			
			_remoteControl = remoteControl;
            _settings = settings;

        }
		
		public bool AreConditionsMets(){
			
			if(this.UseConditions == false){
				
				return true;
				
			}
			
			int usedConditions = 0;
			int satisfiedConditions = 0;
			
			if(this.CheckLoadedModIDs == true){
				
				usedConditions++;
				//Check Condition
				
			}
			
			if(this.CheckTrueBooleans == true){
				
				usedConditions++;
				
			}
			
			if(this.CheckCustomCounters == true){
				
				usedConditions++;
				
			}
			
			if(this.CheckGridSpeed == true){
				
				usedConditions++;
				float speed = (float)_remoteControl.GetShipSpeed();
				
				if((this.MinGridSpeed == -1 || speed >= this.MinGridSpeed) && (this.MaxGridSpeed == -1 || speed <= this.MaxGridSpeed)){
					
					satisfiedConditions++;
					
				}
				
			}
			
			if(this.MatchAnyCondition == false){
				
				return (satisfiedConditions >= usedConditions);
				
			}else{
				
				return (satisfiedConditions > 0);
				
			}
			
		}
		
		public void InitTags(string customData){

            if(string.IsNullOrWhiteSpace(customData) == false) {

                var descSplit = customData.Split('\n');

                foreach(var tag in descSplit) {

                    //UseConditions
                    if(tag.Contains("[UseConditions:") == true) {

                        this.UseConditions = TagHelper.TagBoolCheck(tag);

                    }

                    //MatchAnyCondition
                    if(tag.Contains("[MatchAnyCondition:") == true) {

                        this.MatchAnyCondition = TagHelper.TagBoolCheck(tag);

                    }

                    //CheckLoadedModIDs
                    if(tag.Contains("[CheckLoadedModIDs:") == true) {

                        this.CheckLoadedModIDs = TagHelper.TagBoolCheck(tag);

                    }

                    //ModIDsToCheck

                    //CheckTrueBooleans
                    if(tag.Contains("[CheckTrueBooleans:") == true) {

                        this.CheckTrueBooleans = TagHelper.TagBoolCheck(tag);

                    }

                    //TrueBooleans

                    //CheckCustomCounters
                    if(tag.Contains("[CheckCustomCounters:") == true) {

                        this.CheckCustomCounters = TagHelper.TagBoolCheck(tag);

                    }

                    //CustomCounters

                    //CustomCountersTargets

                    //CheckGridSpeed
                    if(tag.Contains("[CheckGridSpeed:") == true) {

                        this.CheckGridSpeed = TagHelper.TagBoolCheck(tag);

                    }

                    //MinGridSpeed
                    if(tag.Contains("[MinGridSpeed:") == true) {

                        this.MinGridSpeed = TagHelper.TagFloatCheck(tag, this.MinGridSpeed);

                    }

                    //MaxGridSpeed
                    if(tag.Contains("[MaxGridSpeed:") == true) {

                        this.MaxGridSpeed = TagHelper.TagFloatCheck(tag, this.MaxGridSpeed);

                    }

                    //CheckMESBlacklistedSpawnGroups


                }

            }
			
		}
		
	}
	
}
	
	