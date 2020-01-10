using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	
	public enum DebugTypeEnum {

		None,
		Action,
		Chat,
		Condition,
		Despawn,
		Dev,
		Collision,
		General,
		Owner,
		Spawn,
		Target,
		Trigger,
		Weapon

	}

	public static class Logger{
		
		public static bool LoggerDebugMode = false;

		public static bool SkipNextMessage = false;
		public static string LogDefaultIdentifier = "RivalAI: ";

		public static bool DebugWriteToLog = false;
		public static bool DebugNotification = false;

		public static bool DebugAction = false;
		public static bool DebugChat = false;
		public static bool DebugCollision = false;
		public static bool DebugCondition = false;
		public static bool DebugDespawn = false;
		public static bool DebugDev = false;
		public static bool DebugGeneral = false;
		public static bool DebugOwner = false;
		public static bool DebugSpawn = false;
		public static bool DebugTarget = false;
		public static bool DebugTrigger = true;
		public static bool DebugWeapon = false;

		public static void MsgDebug(string message, DebugTypeEnum type = DebugTypeEnum.None){
			
			if(LoggerDebugMode == false){
				
				return;
				
			}

			if (!IsMessageValid(type))
				return;
			
			string typeModifier = type.ToString() + ": ";
			
			if(DebugWriteToLog)
				MyLog.Default.WriteLineAndConsole(LogDefaultIdentifier + typeModifier + message);

			if(DebugNotification)
				MyVisualScriptLogicProvider.ShowNotificationToAll(typeModifier + message, 4000);
			
		}

		public static void WriteLog(string message) {

			MyLog.Default.WriteLineAndConsole(LogDefaultIdentifier + message);

		}

		public static void LoadDebugFromSandbox() {

			bool result = false;

			if(MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugMode", out result))
				Logger.LoggerDebugMode = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugWriteToLog", out result))
				Logger.DebugWriteToLog = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugNotification", out result))
				Logger.DebugNotification = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugAction", out result))
				Logger.DebugAction = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugChat", out result))
				Logger.DebugChat = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugCollision", out result))
				Logger.DebugCollision = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugCondition", out result))
				Logger.DebugCondition = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugDespawn", out result))
				Logger.DebugDespawn = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugDev", out result))
				Logger.DebugDev = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugGeneral", out result))
				Logger.DebugGeneral = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugOwner", out result))
				Logger.DebugOwner = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugSpawn", out result))
				Logger.DebugSpawn = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugTarget", out result))
				Logger.DebugTarget = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugTrigger", out result))
				Logger.DebugTrigger = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugWeapon", out result))
				Logger.DebugWeapon = result;

		}

		public static void EnableAllOptions() {

			LoggerDebugMode = true;
			DebugWriteToLog = true;
			DebugNotification = true;
			DebugAction = true;
			DebugChat = true;
			DebugCollision = true;
			DebugCondition = true;
			DebugDespawn = true;
			DebugDev = true;
			DebugGeneral = true;
			DebugOwner = true;
			DebugSpawn = true;
			DebugTarget = true;
			DebugTrigger = true;
			DebugWeapon = true;

		}

		public static void DisableAllOptions() {

			LoggerDebugMode = false;
			DebugWriteToLog = false;
			DebugNotification = false;
			DebugAction = false;
			DebugChat = false;
			DebugCollision = false;
			DebugCondition = false;
			DebugDespawn = false;
			DebugDev = false;
			DebugGeneral = false;
			DebugOwner = false;
			DebugSpawn = false;
			DebugTarget = false;
			DebugTrigger = false;
			DebugWeapon = false;

		}

		public static void SaveDebugToSandbox() {

			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugMode", Logger.LoggerDebugMode);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugWriteToLog", Logger.DebugWriteToLog);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugNotification", Logger.DebugNotification);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugAction", Logger.DebugAction);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugChat", Logger.DebugChat);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugCollision", Logger.DebugCollision);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugCondition", Logger.DebugCondition);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugDespawn", Logger.DebugDespawn);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugDev", Logger.DebugDev);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugGeneral", Logger.DebugGeneral);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugOwner", Logger.DebugOwner);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugSpawn", Logger.DebugSpawn);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugTarget", Logger.DebugTarget);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugTrigger", Logger.DebugTrigger);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugWeapon", Logger.DebugWeapon);

		}

		public static bool IsMessageValid(DebugTypeEnum type) {

			if (type == DebugTypeEnum.Action && DebugAction)
				return true;

			if (type == DebugTypeEnum.Chat && DebugChat)
				return true;

			if (type == DebugTypeEnum.Condition && DebugCondition)
				return true;

			if (type == DebugTypeEnum.Collision && DebugCollision)
				return true;

			if (type == DebugTypeEnum.Despawn && DebugDespawn)
				return true;

			if (type == DebugTypeEnum.Dev && DebugDev)
				return true;

			if (type == DebugTypeEnum.General && DebugGeneral)
				return true;

			if (type == DebugTypeEnum.Owner && DebugOwner)
				return true;

			if (type == DebugTypeEnum.Spawn && DebugSpawn)
				return true;

			if (type == DebugTypeEnum.Target && DebugTarget)
				return true;

			if (type == DebugTypeEnum.Trigger && DebugTrigger)
				return true;

			if (type == DebugTypeEnum.Weapon && DebugWeapon)
				return true;

			return false;

		}
		
	}
	
}