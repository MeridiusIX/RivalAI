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
	
	public static class Logger{
		
		public static bool LoggerDebugMode = false;
		public static bool SkipNextMessage = false;
		public static string LogDefaultIdentifier = "RivalAI: ";
		public static Stopwatch PerformanceTimer = new Stopwatch();
		
		public static void AddMsg(string message, bool debugOnly = false, string identifier = ""){
			
			if(LoggerDebugMode == false && debugOnly == true){
				
				return;
				
			}
			
			if(LoggerDebugMode == false && SkipNextMessage == true){
				
				SkipNextMessage = false;
				return;
				
			}
			
			SkipNextMessage = false;
			
			string thisIdentifier = "";
			
			if(identifier == ""){
				
				thisIdentifier = LogDefaultIdentifier;
				
			}
			
			MyLog.Default.WriteLineAndConsole(thisIdentifier + message);
			
			if(LoggerDebugMode == true){
				
				MyVisualScriptLogicProvider.ShowNotificationToAll(message, 5000);
				
			}
			
		}
		
		public static void StartTimer(){
			
			if(LoggerDebugMode == false){
				
				return;
				
			}
			
			PerformanceTimer = Stopwatch.StartNew();
			
		}
		
		public static string StopTimer(){
			
			if(LoggerDebugMode == false){
				
				return "";
				
			}
			
			PerformanceTimer.Stop();
			return PerformanceTimer.Elapsed.ToString();
			
		}
		
	}
	
}