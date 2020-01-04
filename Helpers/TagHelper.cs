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
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Helpers {

	public static class TagHelper {

		public static Dictionary<string, string> BehaviorTemplates = new Dictionary<string, string>();

		public static Dictionary<string, byte[]> ActionObjectTemplates = new Dictionary<string, byte[]>();
		public static Dictionary<string, byte[]> ChatObjectTemplates = new Dictionary<string, byte[]>();
		public static Dictionary<string, byte[]> ConditionObjectTemplates = new Dictionary<string, byte[]>();
		public static Dictionary<string, byte[]> SpawnerObjectTemplates = new Dictionary<string, byte[]>();
		public static Dictionary<string, byte[]> TargetObjectTemplates = new Dictionary<string, byte[]>();
		public static Dictionary<string, byte[]> TriggerObjectTemplates = new Dictionary<string, byte[]>();

		public static void Setup() {

			var definitionList = MyDefinitionManager.Static.GetEntityComponentDefinitions();

			//Get All Chat, Spawner
			foreach (var def in definitionList) {

				try {

					if (string.IsNullOrWhiteSpace(def.DescriptionText) == true) {

						continue;

					}

					if (def.DescriptionText.Contains("[RivalAI Chat]") == true && ChatObjectTemplates.ContainsKey(def.Id.SubtypeName) == false) {

						var chatObject = new ChatProfile();
						chatObject.InitTags(def.DescriptionText);
						chatObject.ProfileSubtypeId = def.Id.SubtypeName;
						var chatBytes = MyAPIGateway.Utilities.SerializeToBinary<ChatProfile>(chatObject);
						Logger.WriteLog("Chat Profile Added: " + def.Id.SubtypeName);
						ChatObjectTemplates.Add(def.Id.SubtypeName, chatBytes);
						continue;

					}

					if (def.DescriptionText.Contains("[RivalAI Spawn]") == true && SpawnerObjectTemplates.ContainsKey(def.Id.SubtypeName) == false) {

						var spawnerObject = new SpawnProfile();
						spawnerObject.InitTags(def.DescriptionText);
						spawnerObject.ProfileSubtypeId = def.Id.SubtypeName;
						var spawnerBytes = MyAPIGateway.Utilities.SerializeToBinary<SpawnProfile>(spawnerObject);
						Logger.WriteLog("Spawner Profile Added: " + def.Id.SubtypeName);
						SpawnerObjectTemplates.Add(def.Id.SubtypeName, spawnerBytes);
						continue;

					}

				} catch (Exception) {

					Logger.DebugMsg(string.Format("Caught Error While Processing Definition {0}", def.Id));

				}

			}

			foreach (var def in definitionList) {

				try {

					if(string.IsNullOrWhiteSpace(def.DescriptionText) == true) {

						continue;

					}

					if(def.DescriptionText.Contains("[RivalAI Action]") == true && ActionObjectTemplates.ContainsKey(def.Id.SubtypeName) == false) {

						var actionObject = new ActionProfile();
						actionObject.InitTags(def.DescriptionText);
						actionObject.ProfileSubtypeId = def.Id.SubtypeName;
						var targetBytes = MyAPIGateway.Utilities.SerializeToBinary<ActionProfile>(actionObject);
						Logger.WriteLog("Action Profile Added: " + def.Id.SubtypeName);
						ActionObjectTemplates.Add(def.Id.SubtypeName, targetBytes);
						continue;

					}
			
					if(def.DescriptionText.Contains("[RivalAI Condition]") == true && ChatObjectTemplates.ContainsKey(def.Id.SubtypeName) == false) {

						var conditionObject = new ConditionProfile();
						conditionObject.InitTags(def.DescriptionText);
						conditionObject.ProfileSubtypeId = def.Id.SubtypeName;
						var conditionBytes = MyAPIGateway.Utilities.SerializeToBinary<ConditionProfile>(conditionObject);
						Logger.WriteLog("Condition Profile Added: " + def.Id.SubtypeName);
						ConditionObjectTemplates.Add(def.Id.SubtypeName, conditionBytes);
						continue;

					}

					if(def.DescriptionText.Contains("[RivalAI Target]") == true && TargetObjectTemplates.ContainsKey(def.Id.SubtypeName) == false) {

						var targetObject = new TargetProfile();
						targetObject.InitTags(def.DescriptionText);
						targetObject.ProfileSubtypeId = def.Id.SubtypeName;
						var targetBytes = MyAPIGateway.Utilities.SerializeToBinary<TargetProfile>(targetObject);
						Logger.WriteLog("Target Profile Added: " + def.Id.SubtypeName);
						TargetObjectTemplates.Add(def.Id.SubtypeName, targetBytes);
						continue;

					}

				} catch(Exception) {

					Logger.DebugMsg(string.Format("Caught Error While Processing Definition {0}", def.Id));

				}

			}

			//Get All Triggers - Build With Action, Chat and Spawner
			foreach(var def in definitionList) {

				if(string.IsNullOrWhiteSpace(def.DescriptionText) == true) {

					continue;

				}

				if(def.DescriptionText.Contains("[RivalAI Trigger]") == true && TriggerObjectTemplates.ContainsKey(def.Id.SubtypeName) == false) {

					var triggerObject = new TriggerProfile();
					triggerObject.InitTags(def.DescriptionText);
					triggerObject.ProfileSubtypeId = def.Id.SubtypeName;
					var triggerBytes = MyAPIGateway.Utilities.SerializeToBinary<TriggerProfile>(triggerObject);
					Logger.WriteLog("Trigger Profile Added: " + def.Id.SubtypeName);
					TriggerObjectTemplates.Add(def.Id.SubtypeName, triggerBytes);
					continue;

				}


			}

			//Get All Behavior

		}

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

		public static Base6Directions.Direction TagBase6DirectionCheck(string tag) {

			Base6Directions.Direction result = Base6Directions.Direction.Forward;
			var tagSplit = ProcessTag(tag);

			if(tagSplit.Length == 2) {

				bool parseResult = Base6Directions.Direction.TryParse(tagSplit[1], out result) == false;

			}

			return result;

		}

		public static bool TagBoolCheck(string tag){
			
			bool result = false;
			var tagSplit = ProcessTag(tag);

			if(tagSplit.Length == 2){

				bool parseResult = bool.TryParse(tagSplit[1], out result);
				
			}
			
			return result;
			
		}

		public static BroadcastType TagBroadcastTypeEnumCheck(string tag) {

			BroadcastType result = BroadcastType.None;
			var tagSplit = ProcessTag(tag);

			if(tagSplit.Length == 2) {

				if(SpawnPositioningEnum.TryParse(tagSplit[1], out result) == false) {

					return BroadcastType.None;

				}

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

		public static SpawnPositioningEnum TagSpawnPositioningEnumCheck(string tag) {

			SpawnPositioningEnum result = SpawnPositioningEnum.RandomDirection;
			var tagSplit = ProcessTag(tag);

			if(tagSplit.Length == 2) {

				if(SpawnPositioningEnum.TryParse(tagSplit[1], out result) == false) {

					return SpawnPositioningEnum.RandomDirection;

				}

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

		public static TriggerAction TagTriggerActionCheck(string tag) {

			TriggerAction result = TriggerAction.None;
			var tagSplit = ProcessTag(tag);

			if(tagSplit.Length == 2) {

				if(TriggerAction.TryParse(tagSplit[1], out result) == false) {

					return TriggerAction.None;

				}

			}

			return result;

		}

		public static Vector3D TagVector3DCheck(string tag) {

			Vector3D result = Vector3D.Zero;
			var tagSplit = ProcessTag(tag);

			if(tagSplit.Length == 2) {

				if(Vector3D.TryParse(tagSplit[1], out result) == false) {

					return Vector3D.Zero;

				}

			}

			return result;

		}

	}
	
}
