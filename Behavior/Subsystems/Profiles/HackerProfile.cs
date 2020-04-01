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

	public enum HackingType {

		None,
		ChangeLcdTextures,
		ConnectorThrowing,
		DetachMechanicalBlocks,
		DisableAutomation,
		DisableProduction,
		DisableShields,
		DisableWeapons,
		FireAllWeapons,
		GyroOverride,
		HudSpam,
		ScrambleLightColors,
		TakeOwnership,
		TerminalScrambler,
		ThrustOverride,

	}
	public class HackerProfile {

		public bool EnableHacking;

		public int PreHackingTime;
		public int HackingCooldownTime;
		public int MaxHackingAttacks;
		public int MaxFailedHackingAttacks;
		public List<HackingType> HackingTypes;

		public bool UseBlockInterference;
		public List<MyDefinitionId> InterferenceBlockIDs;
		public int InterferenceBlockCountRequired;
		public bool InterferenceBlocksReduceSuccess;

		public ChatProfile HackingChat; //Change Chat Profile To Use Modifiers

		public HackerProfile() {

			EnableHacking = false;
			PreHackingTime = 10;
			HackingCooldownTime = 30;
			MaxHackingAttacks = 3;
			MaxFailedHackingAttacks = 3;
			HackingTypes = new List<HackingType>();

			UseBlockInterference = false;
			InterferenceBlockIDs = new List<MyDefinitionId>();
			InterferenceBlockCountRequired = 3;
			InterferenceBlocksReduceSuccess = true;

			HackingChat = new ChatProfile();

		}

	}
}
