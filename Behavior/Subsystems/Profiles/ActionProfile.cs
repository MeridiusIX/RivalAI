﻿using System;
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
    
    public class ActionProfile {
        
        public bool BroadcastMessage;
        
        public bool BarrelRoll;
        
        public bool ChangeAutopilotSpeed;
        public float NewAutopilotSpeed;
        
        public bool SpawnEncounter;
        public List<string> SpawnGroups;
        
        public ActionProfile(){
        
            BroadcastMessage = false;
            
            BarrelRoll = false;
            
            ChangeAutopilotSpeed = false;
            NewAutopilotSpeed = 0;
            
            SpawnEncounter = false;
            SpawnGroups = new List<string>();
        
        }

        public void InitTags(string customData) {



        }

    }

}
