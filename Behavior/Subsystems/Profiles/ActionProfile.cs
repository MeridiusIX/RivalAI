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
        public List<string> SpawnGroups;
        
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
        public List<string> TimeBlockNames;
        
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
            SpawnGroups = new List<string>();
            
            SelfDestruct = false;
            
            Retreat = false;
            
            SwitchToReceivedTarget = false;
            
            SwitchToDamagerTarget = false;
            
            RefreshTarget = false;
            
            TriggerTimerBlocks = false;
            TimeBlockNames = new List<string>();
            
            ChangeReputationWithPlayers = false;
            ReputationChangeRadius = 0;
            ReputationChangeAmount = 0;
            
            ActivateAssertiveAntennas = false;
            
            ChangeAntennaOwnership = false;
            AntennaFactionOwner = "Nobody";
        
        }

        public void InitTags(string customData) {

            

        }

    }

}
