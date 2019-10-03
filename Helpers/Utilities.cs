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

namespace RivalAI.Helpers{

	public static class Utilities{
		
		public static ushort SessionNetworkId = 46061;
		public static ushort BlockNetworkId = 46062;
		public static long SessionModMessageId = 0;
		public static long BlockModMessageId = 0;
		public static string ModName = "";
		
		public static Random Rnd = new Random();
		
		public static Vector4 ConvertColor(Color color){
			
			return new Vector4(color.X / 10, color.Y / 10, color.Z / 10, 0.1f);
			
		}
		
	}
	
}