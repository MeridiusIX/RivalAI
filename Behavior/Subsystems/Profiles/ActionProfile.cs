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
using RivalAI.Behavior.Settings;
using RivalAI.Helpers;

namespace RivalAI.Behavior.Subsystems.Profiles {
    
    [ProtoContract]
    public class ActionProfile {
        
        [ProtoMember(1)]
        public bool BroadcastMessage;
        
        [ProtoMember(2)]
        public bool BarrelRoll;
        
        [ProtoMember(3)]
        public bool ChangeAutopilotSpeed;
        
        [ProtoMember(4)]
        public float NewAutopilotSpeed;
        
        [ProtoMember(5)]
        public bool SpawnEncounter;
        
        [ProtoMember(6)]
        public List<string> SpawnGroups; //Unused
        
        [ProtoMember(7)]
        public bool SelfDestruct;
        
        [ProtoMember(8)]
        public bool Retreat;
        
        [ProtoMember(9)]
        public bool SwitchToReceivedTarget;
        
        [ProtoMember(10)]
        public bool SwitchToDamagerTarget;
        
        [ProtoMember(11)]
        public bool RefreshTarget;
        
        [ProtoMember(12)]
        public bool TriggerTimerBlocks;
        
        [ProtoMember(13)]
        public List<string> TimerBlockNames;
        
        [ProtoMember(14)]
        public bool ChangeReputationWithPlayers;
        
        [ProtoMember(15)]
        public double ReputationChangeRadius;
        
        [ProtoMember(16)]
        public int ReputationChangeAmount;
        
        [ProtoMember(17)]
        public bool ActivateAssertiveAntennas;
        
        [ProtoMember(18)]
        public bool ChangeAntennaOwnership;
        
        [ProtoMember(19)]
        public string AntennaFactionOwner;
        
        public ActionProfile(){
        
            BroadcastMessage = false;
            
            BarrelRoll = false;
            
            ChangeAutopilotSpeed = false;
            NewAutopilotSpeed = 0;
            
            SpawnEncounter = false;

            SelfDestruct = false;
            
            Retreat = false;
            
            SwitchToReceivedTarget = false;
            
            SwitchToDamagerTarget = false;
            
            RefreshTarget = false;
            
            TriggerTimerBlocks = false;
            TimerBlockNames = new List<string>();
            
            ChangeReputationWithPlayers = false;
            ReputationChangeRadius = 0;
            ReputationChangeAmount = 0;
            
            ActivateAssertiveAntennas = false;
            
            ChangeAntennaOwnership = false;
            AntennaFactionOwner = "Nobody";
        
        }

        public void InitTags(string customData) {

            if(string.IsNullOrWhiteSpace(customData) == false) {

                var descSplit = customData.Split('\n');

                foreach(var tag in descSplit) {
                    
                    //BroadcastMessage
                    if(tag.Contains("[BroadcastMessage:") == true) {

                        this.BroadcastMessage = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //BarrelRoll
                    if(tag.Contains("[BarrelRoll:") == true) {

                        this.BroadcastMessage = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //ChangeAutopilotSpeed
                    if(tag.Contains("[ChangeAutopilotSpeed:") == true) {

                        this.ChangeAutopilotSpeed = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //NewAutopilotSpeed
                    if(tag.Contains("[NewAutopilotSpeed:") == true) {

                        this.NewAutopilotSpeed = TagHelper.TagFloatCheck(tag, this.NewAutopilotSpeed);

                    }
                    
                    //SpawnEncounter
                    if(tag.Contains("[SpawnEncounter:") == true) {

                        this.SpawnEncounter = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //SelfDestruct
                    if(tag.Contains("[SelfDestruct:") == true) {

                        this.SelfDestruct = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //Retreat
                    if(tag.Contains("[Retreat:") == true) {

                        this.Retreat = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //SwitchToReceivedTarget
                    if(tag.Contains("[SwitchToReceivedTarget:") == true) {

                        this.SwitchToReceivedTarget = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //SwitchToDamagerTarget
                    if(tag.Contains("[SwitchToDamagerTarget:") == true) {

                        this.SwitchToDamagerTarget = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //RefreshTarget
                    if(tag.Contains("[RefreshTarget:") == true) {

                        this.RefreshTarget = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //TriggerTimerBlocks
                    if(tag.Contains("[TriggerTimerBlocks:") == true) {

                        this.TriggerTimerBlocks = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //TimerBlockNames
                    if(tag.Contains("[TimerBlockNames:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            TimerBlockNames.Add(tempvalue);

                        }

                    }
                    
                    //ChangeReputationWithPlayers
                    if(tag.Contains("[ChangeReputationWithPlayers:") == true) {

                        this.ChangeReputationWithPlayers = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //ReputationChangeRadius
                    if(tag.Contains("[ReputationChangeRadius:") == true) {

                        ReputationChangeRadius = TagHelper.TagDoubleCheck(tag, ReputationChangeRadius);

                    }
                    
                    //ReputationChangeAmount
                    if(tag.Contains("[ReputationChangeAmount:") == true) {

                        ReputationChangeAmount = TagHelper.TagIntCheck(tag, ReputationChangeAmount);

                    }
                    
                    //ActivateAssertiveAntennas
                    if(tag.Contains("[ActivateAssertiveAntennas:") == true) {

                        this.ActivateAssertiveAntennas = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //ChangeAntennaOwnership
                    if(tag.Contains("[ChangeAntennaOwnership:") == true) {

                        this.ChangeAntennaOwnership = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //AntennaFactionOwner
                    if(tag.Contains("[AntennaFactionOwner:") == true) {

                        this.AntennaFactionOwner = TagHelper.TagStringCheck(tag);

                    }
                    
                }

            }

        }

    }

}
