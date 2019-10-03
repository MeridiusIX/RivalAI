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

namespace RivalAI.Behavior{
	
	public class Hacker : CoreBehavior{

        public bool HackerAllowDecoyDefense;
        public int HackerNumberOfInterferenceDecoys;
        public bool HackerChangeLightingColors;
        public bool HackerDamageReputation;
        public bool HackerDetechMechanicalBlocks;
        public bool HackerDisableAutomation;
        public bool HackerDisableConveyance;
        public bool HackerDisableModdedBlocks;
        public bool HackerDisableProduction;
        public bool HackerDisableWeapons;
        public bool HackerFireAllWeapons;
        public bool HackerInstallProgramBlockVirus;
        public bool HackerLockDoors;
        public bool HackerOverrideGyro;
        public bool HackerOverrideThrust;
        public bool HackerPlaySoundBlocks;
        public bool HackerRandomLcdImages;
        public bool HackerScrambleBlockNames;
        public bool HackerShowAllBlocksOnHUD;
        public bool HackerStealCredits;
        public bool HackerTakeBlockOwnership;
        public bool HackerThrowInventory;

        public Hacker(){
			
			
			
		}
		
	}
	
}