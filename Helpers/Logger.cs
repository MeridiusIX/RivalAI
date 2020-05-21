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
		ActionChat,
		AutoPilot,
		AutoPilotStuck,
		AutoPilotOffset,
		AutoPilotPlanetPath,
		BehaviorMode,
		BehaviorSetup,
		Chat,
		Condition,
		Despawn,
		Dev,
		Collision,
		General,
		Owner,
		Spawn,
		Target,
		TargetAcquisition,
		TargetEvaluation,
		Thrust,
		Trigger,
		TriggerPlayerNear,
		Weapon,
		WeaponBarrage,
		WeaponCore,
		WeaponSetup,
		WeaponStaticCore,
		WeaponStaticRegular

	}

	public static class Logger{
		
		public static bool LoggerDebugMode = false;

		public static bool SkipNextMessage = false;
		public static string LogDefaultIdentifier = "RivalAI: ";

		public static bool DebugWriteToLog = false;
		public static bool DebugNotification = false;

		public static List<DebugTypeEnum> CurrentDebugTypeList = new List<DebugTypeEnum>();

		public static DateTime StartTimer = DateTime.Now;
		public static DateTime StepTimer = DateTime.Now;

		public static void MsgDebug(string message, DebugTypeEnum type = DebugTypeEnum.None){

			if (!LoggerDebugMode)
				return;

			if (string.IsNullOrWhiteSpace(message))
				return;

			if (!IsMessageValid(type))
				return;
			
			string typeModifier = type.ToString() + ": ";
			
			if(DebugWriteToLog)
				MyLog.Default.WriteLineAndConsole(LogDefaultIdentifier + typeModifier + message);

		}

		public static void WriteLog(string message) {

			if (string.IsNullOrWhiteSpace(message))
				return;

			MyLog.Default.WriteLineAndConsole(LogDefaultIdentifier + message);

		}

		public static void UseStopwatch(string msg, bool reset = false, bool endTimer = false) {

			if (reset) {

				StepTimer = DateTime.Now;
				StartTimer = DateTime.Now;
				WriteLog(msg);
				return;
			
			}

			if (endTimer) {

				var timespanE = DateTime.Now - StartTimer;
				WriteLog(msg + timespanE.TotalMilliseconds.ToString());
				return;
			
			}

			var timespan = DateTime.Now - StepTimer;
			WriteLog(msg + timespan.TotalMilliseconds.ToString());
			StepTimer = DateTime.Now;

		}

		public static void LoadDebugFromSandbox() {

			bool result = false;
			string listStringResult = "";

			if(MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugMode", out result))
				Logger.LoggerDebugMode = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugWriteToLog", out result))
				Logger.DebugWriteToLog = result;

			if (MyAPIGateway.Utilities.GetVariable<bool>("RAI-Setting-DebugNotification", out result))
				Logger.DebugNotification = result;

			if (MyAPIGateway.Utilities.GetVariable<string>("RAI-Setting-DebugTypes", out listStringResult)) {

				try {

					var resultBytes = Convert.FromBase64String(listStringResult);
					var resultList = MyAPIGateway.Utilities.SerializeFromBinary<List<DebugTypeEnum>>(resultBytes);

					if (resultList != null && resultList.Count > 0) {

						CurrentDebugTypeList = resultList;

					}

				} catch (Exception e) {
				
				
				
				}
				
			
			}

		}

		public static bool EnableDebugOption(string type, bool mode) {

			DebugTypeEnum debugEnum = DebugTypeEnum.None;

			if (!DebugTypeEnum.TryParse(type, out debugEnum))
				return false;

			if (mode) {

				if (!CurrentDebugTypeList.Contains(debugEnum))
					CurrentDebugTypeList.Add(debugEnum);

			} else {

				if (CurrentDebugTypeList.Contains(debugEnum))
					CurrentDebugTypeList.Remove(debugEnum);

			}

			return true;
		
		}

		public static void EnableAllOptions() {

			LoggerDebugMode = true;
			DebugWriteToLog = true;

			CurrentDebugTypeList.Clear();

			var values = Enum.GetValues(typeof(DebugTypeEnum)).Cast<DebugTypeEnum>();

			foreach (var debugType in values) {

				CurrentDebugTypeList.Add(debugType);

			}

		}

		public static void DisableAllOptions() {

			LoggerDebugMode = false;
			DebugWriteToLog = false;

			CurrentDebugTypeList.Clear();

		}

		public static void SaveDebugToSandbox() {

			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugMode", Logger.LoggerDebugMode);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugWriteToLog", Logger.DebugWriteToLog);
			MyAPIGateway.Utilities.SetVariable<bool>("RAI-Setting-DebugNotification", Logger.DebugNotification);

			try {

				var resultBytes = MyAPIGateway.Utilities.SerializeToBinary<List<DebugTypeEnum>>(CurrentDebugTypeList);
				var resultList = Convert.ToBase64String(resultBytes);
				MyAPIGateway.Utilities.SetVariable<string>("RAI-Setting-DebugTypes", resultList);

			} catch (Exception e) {

				//WriteLog("Could Not Save DebugTypes As Base64");
				//WriteLog(e.ToString());

			}

		}

		public static bool IsMessageValid(DebugTypeEnum type) {

			return CurrentDebugTypeList.Contains(type);

		}
		
	}
	
}