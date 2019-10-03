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

namespace RivalAI.Helpers {
	
    public static class TagHelper {

        private static string [] ProcessTag(string tag){
			
			var thisTag = tag;
			thisTag = thisTag.Replace("[", "");
			thisTag = thisTag.Replace("]", "");
			var tagSplit = thisTag.Split(':');
            string a = "";
            string b = "";

            if(tagSplit.Length > 2) {

                a = tagSplit[0];

                for(int i = 1;i < tagSplit.Length;i++) {

                    b += tagSplit[i];

                    if(i != tagSplit.Length - 1) {

                        b += ":";

                    }

                }

                tagSplit = new string[] {a,b};

            }

			return tagSplit;
			
		}

        public static BlockTargetTypes TagBlockTargetTypesCheck(string tag) {

            BlockTargetTypes result = BlockTargetTypes.All;
            var tagSplit = ProcessTag(tag);

            if(tagSplit.Length == 2) {

                if(BlockTargetTypes.TryParse(tagSplit[1], out result) == false) {

                    return BlockTargetTypes.All;

                }

            }

            return result;

        }

        public static bool TagBoolCheck(string tag){
			
			bool result = false;
			var tagSplit = ProcessTag(tag);
					
			if(tagSplit.Length == 2){
				
				bool parseResult = bool.TryParse(tagSplit[1], out result) == false;
				
			}
			
			return result;
			
		}
		
		public static double TagDoubleCheck(string tag, double defaultValue){
			
			double result = defaultValue;
			var tagSplit = ProcessTag(tag);
					
			if(tagSplit.Length == 2){
				
				if(double.TryParse(tagSplit[1], out result) == false){
					
					return defaultValue;
					
				}
				
			}
			
			return result;
			
		}
		
		public static float TagFloatCheck(string tag, float defaultValue){
			
			float result = defaultValue;
			var tagSplit = ProcessTag(tag);
					
			if(tagSplit.Length == 2){
				
				if(float.TryParse(tagSplit[1], out result) == false){
					
					return defaultValue;
					
				}
				
			}
			
			return result;
			
		}
		
		public static int TagIntCheck(string tag, int defaultValue){
			
			int result = defaultValue;
			var tagSplit = ProcessTag(tag);
					
			if(tagSplit.Length == 2){
				
				if(int.TryParse(tagSplit[1], out result) == false){
					
					return defaultValue;
					
				}
				
			}
			
			return result;
			
		}

		public static long TagLongCheck(string tag, long defaultValue){
			
			long result = defaultValue;
			var tagSplit = ProcessTag(tag);
					
			if(tagSplit.Length == 2){
				
				if(long.TryParse(tagSplit[1], out result) == false){
					
					return defaultValue;
					
				}
				
			}
			
			return result;
			
		}

        public static string TagStringCheck(string tag) {

            string result = "";
            var tagSplit = ProcessTag(tag);

            if(tagSplit.Length == 2) {

                result = tagSplit[1];

            }

            return result;

        }

        public static TargetDistanceEnum TagTargetDistanceEnumCheck(string tag) {

            TargetDistanceEnum result = TargetDistanceEnum.Any;
            var tagSplit = ProcessTag(tag);

            if(tagSplit.Length == 2) {

                if(TargetDistanceEnum.TryParse(tagSplit[1], out result) == false) {

                    return TargetDistanceEnum.Any;

                }

            }

            return result;

        }

        public static TargetFilterEnum TagTargetFilterEnumCheck(string tag) {

            TargetFilterEnum result = TargetFilterEnum.None;
            var tagSplit = ProcessTag(tag);

            if(tagSplit.Length == 2) {

                if(TargetFilterEnum.TryParse(tagSplit[1], out result) == false) {

                    return TargetFilterEnum.None;

                }

            }

            return result;

        }

        public static TargetOwnerEnum TagTargetOwnerEnumCheck(string tag) {

            TargetOwnerEnum result = TargetOwnerEnum.None;
            var tagSplit = ProcessTag(tag);

            if(tagSplit.Length == 2) {

                if(TargetOwnerEnum.TryParse(tagSplit[1], out result) == false) {

                    return TargetOwnerEnum.None;

                }

            }

            return result;

        }

        public static TargetRelationEnum TagTargetRelationEnumCheck(string tag) {

            TargetRelationEnum result = TargetRelationEnum.None;
            var tagSplit = ProcessTag(tag);

            if(tagSplit.Length == 2) {

                if(TargetRelationEnum.TryParse(tagSplit[1], out result) == false) {

                    return TargetRelationEnum.None;

                }

            }

            return result;

        }

        public static TargetTypeEnum TagTargetTypeEnumCheck(string tag) {

            TargetTypeEnum result = TargetTypeEnum.None;
            var tagSplit = ProcessTag(tag);

            if(tagSplit.Length == 2) {

                if(TargetTypeEnum.TryParse(tagSplit[1], out result) == false) {

                    return TargetTypeEnum.None;

                }

            }

            return result;

        }

        

    }
	
}
