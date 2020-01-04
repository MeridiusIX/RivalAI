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

        public static List<long> ModIDs = new List<long>();

        public static Dictionary<string, MyPhysicalItemDefinition> BehaviorProfiles = new Dictionary<string, MyPhysicalItemDefinition>();
		
		public static Random Rnd = new Random();
		
		public static Vector4 ConvertColor(Color color){
			
			return new Vector4(color.X / 10, color.Y / 10, color.Z / 10, 0.1f);
			
		}

        public static void GetAllBehaviorProfiles() {

            BehaviorProfiles.Clear();
            var defList = MyDefinitionManager.Static.GetPhysicalItemDefinitions();

            foreach(var def in defList) {

                if(string.IsNullOrWhiteSpace(def.DescriptionText) == true) {

                    continue;

                }

                if(def.DescriptionText.Contains("[Rival AI Behavior]") == true) {

                    MyPhysicalItemDefinition item = null;
                    if(BehaviorProfiles.TryGetValue(def.Id.SubtypeName, out item) == false) {

                        BehaviorProfiles.Add(def.Id.SubtypeName, def);

                    } else {

                        var sb = new StringBuilder();
                        sb.Append("Error Adding RivalAI Profile: ").Append(def.Id.SubtypeName).Append(" - Profile Already Exists.");
                        Logger.DebugMsg(sb.ToString());

                    }

                }

            }

        }

        public static string GetBehaviorProfile(string behaviorSubtype) {

            MyPhysicalItemDefinition item = null;
            if(BehaviorProfiles.TryGetValue(behaviorSubtype, out item) == true) {

                if(string.IsNullOrWhiteSpace(item.DescriptionText) == false) {

                    return item.DescriptionText;

                }

            }

            return "";

        }

        public static void GetAllModIDs() {

            foreach (var mod in MyAPIGateway.Session.Mods) {

                if (mod.PublishedFileId != 0) {

                    ModIDs.Add((long)mod.PublishedFileId);

                }

            }

        }
		
	}
	
}