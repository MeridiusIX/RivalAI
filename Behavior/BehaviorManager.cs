using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RivalAI.Behavior {

	public enum BehaviorManagerMode {
	
		None,
		Parallel,
		ParallelWorking,
		MainThread
	
	}

	public enum BehaviorManageSubmode {

		None,
		Collision,
		Targeting,
		AutoPilot,
		Weapons,
		Triggers,
		Behavior

	}

	public static class BehaviorManager {

		public static List<IBehavior> Behaviors = new List<IBehavior>();
		public static List<IMyRemoteControl> DormantAiBlocks = new List<IMyRemoteControl>();

		public static BehaviorManagerMode Mode = BehaviorManagerMode.None;
		public static BehaviorManageSubmode Submode = BehaviorManageSubmode.None;
		public static int CurrentBehaviorIndex = 0;

		private static bool _debugDraw = false;

		private static byte _barrageCounter = 0;
		private static byte _behaviorCounter = 0;

		public static void ProcessBehaviors() {

			if (_debugDraw) {

				for (int i = Behaviors.Count - 1; i >= 0; i--) {

					if (!Behaviors[i].IsClosed() && Behaviors[i].IsAIReady()) {

						Behaviors[i].DebugDrawWaypoints();

					}

				}

			}

			_barrageCounter++;

			if (Mode != BehaviorManagerMode.None) {

				try {

					ProcessParallelMethods();
					ProcessMainThreadMethods();

				} catch (Exception e) {

					Logger.MsgDebug("Exception in Main Behavior Processing", DebugTypeEnum.General);
					Logger.MsgDebug(e.ToString(), DebugTypeEnum.General);

				}

			} else {

				_behaviorCounter++;

			}

			if ((_barrageCounter % 10) == 0) {

				ProcessWeaponsBarrage();
				_barrageCounter = 0;

			}

			if (_behaviorCounter == 15) {

				for (int i = Behaviors.Count - 1; i >= 0; i--) {

					if (Behaviors[i].IsClosed() || Behaviors[i].BehaviorTerminated) {

						Behaviors.RemoveAt(i);
						continue;
					
					}
				
				}

				//Logger.MsgDebug("Start Parallel For All Behaviors", DebugTypeEnum.General);
				Mode = BehaviorManagerMode.Parallel;
				_behaviorCounter = 0;

			}

		}

		public static void RegisterBehaviorFromRemoteControl(IMyRemoteControl remoteControl) {

			try {

				Logger.MsgDebug("Determining Behavior Type of RemoteControl", DebugTypeEnum.BehaviorSetup);
				//CoreBehavior
				if (remoteControl.CustomData.Contains("[BehaviorName:CoreBehavior]")) {

					var CoreBehaviorInstance = new CoreBehavior();
					CoreBehaviorInstance.CoreSetup(remoteControl);
					return;

				}

				//Fighter
				if (remoteControl.CustomData.Contains("[BehaviorName:Fighter]")) {

					Logger.MsgDebug("Behavior: Fighter", DebugTypeEnum.BehaviorSetup);
					var MainBehavior = new Fighter();
					MainBehavior.BehaviorInit(remoteControl);

					lock(Behaviors)
						Behaviors.Add(MainBehavior);

					return;

				}

				//HorseFighter
				if (remoteControl.CustomData.Contains("[BehaviorName:HorseFighter]")) {

					Logger.MsgDebug("Behavior: HorseFighter", DebugTypeEnum.BehaviorSetup);
					var MainBehavior = new HorseFighter();
					MainBehavior.BehaviorInit(remoteControl);

					lock (Behaviors)
						Behaviors.Add(MainBehavior);

					return;

				}

				//Horsefly
				if (remoteControl.CustomData.Contains("[BehaviorName:Horsefly]")) {

					Logger.MsgDebug("Behavior: Horsefly", DebugTypeEnum.BehaviorSetup);
					var MainBehavior = new Horsefly();
					MainBehavior.BehaviorInit(remoteControl);

					lock (Behaviors)
						Behaviors.Add(MainBehavior);

					return;

				}

				//Hunter
				if (remoteControl.CustomData.Contains("[BehaviorName:Hunter]")) {

					Logger.MsgDebug("Behavior: Hunter", DebugTypeEnum.BehaviorSetup);
					var MainBehavior = new Hunter();
					MainBehavior.BehaviorInit(remoteControl);

					lock (Behaviors)
						Behaviors.Add(MainBehavior);

					return;

				}

				//Passive
				if (remoteControl.CustomData.Contains("[BehaviorName:Passive]")) {

					Logger.MsgDebug("Behavior: Passive", DebugTypeEnum.BehaviorSetup);
					var MainBehavior = new Passive();
					MainBehavior.BehaviorInit(remoteControl);

					lock (Behaviors)
						Behaviors.Add(MainBehavior);

					return;

				}

				//Strike
				if (remoteControl.CustomData.Contains("[BehaviorName:Strike]")) {

					Logger.MsgDebug("Behavior: Strike", DebugTypeEnum.BehaviorSetup);
					var MainBehavior = new Strike();
					MainBehavior.BehaviorInit(remoteControl);

					lock (Behaviors)
						Behaviors.Add(MainBehavior);

					return;

				}

			} catch (Exception exc) {

				Logger.WriteLog("Exception Found During Behavior Setup:");
				Logger.WriteLog(exc.ToString());

			}

		}

		private static void ProcessParallelMethods() {

			if (Mode != BehaviorManagerMode.Parallel)
				return;

			MyAPIGateway.Parallel.Start(() => {

				try {

					Mode = BehaviorManagerMode.ParallelWorking;
					//Logger.MsgDebug("Start Parallel Methods", DebugTypeEnum.General);
					ProcessCollisionChecksParallel();
					ProcessTargetingParallel();
					ProcessAutoPilotParallel();
					ProcessWeaponsParallel();
					ProcessTriggersParallel();
					Mode = BehaviorManagerMode.MainThread;
					//Logger.MsgDebug("End Parallel Methods", DebugTypeEnum.General);

				} catch (Exception e) {

					Mode = BehaviorManagerMode.Parallel;
					Logger.MsgDebug("Exception in Parallel Calculations", DebugTypeEnum.General);
					Logger.MsgDebug(e.ToString(), DebugTypeEnum.General);

				}
				

			});
	
		}

		private static void ProcessMainThreadMethods() {

			if (Mode != BehaviorManagerMode.MainThread)
				return;

			//Logger.MsgDebug("Start Main Methods", DebugTypeEnum.General);
			ProcessAutoPilotMain();
			ProcessWeaponsMain();
			ProcessTriggersMain();
			ProcessDespawnConditions();
			ProcessMainBehavior();
			Mode = BehaviorManagerMode.None;
			//Logger.MsgDebug("End Main Methods", DebugTypeEnum.General);

		}

		private static void ProcessCollisionChecksParallel() {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].ProcessCollisionChecks();

			}
		
		}

		private static void ProcessTargetingParallel() {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].ProcessTargetingChecks();

			}

		}

		private static void ProcessAutoPilotParallel() {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].ProcessAutoPilotChecks();

			}

		}

		private static void ProcessAutoPilotMain() {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].EngageAutoPilot();

			}

		}

		private static void ProcessWeaponsParallel() {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].ProcessWeaponChecks();

			}

		}

		private static void ProcessWeaponsMain() {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].SetInitialWeaponReadiness();
				Behaviors[i].FireWeapons();

			}

		}

		private static void ProcessWeaponsBarrage() {

			//TODO
			//Mexpex reported single unreproducible crash here
			//Do some testing here just to be sure.

			if (Behaviors == null) {

				Logger.WriteLog("ERROR: Behaviors List in BehaviorManager is NULL");
				return;
			
			}

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (Behaviors[i] == null) {

					Logger.WriteLog("ERROR: Behavior in Active Behaviors is NULL");
					continue;

				}

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].FireBarrageWeapons();

			}

		}

		private static void ProcessTriggersParallel() {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].ProcessTriggerChecks();

			}

		}

		private static void ProcessTriggersMain() {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].ProcessActivatedTriggers();

			}

		}

		private static void ProcessDespawnConditions() {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].CheckDespawnConditions();

			}

		}

		private static void ProcessMainBehavior() {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (!Behaviors[i].IsAIReady())
					continue;

				Behaviors[i].RunMainBehavior();

			}

		}

		public static IBehavior GetBehavior(IMyRemoteControl remoteControl) {

			for (int i = Behaviors.Count - 1; i >= 0; i--) {

				if (Behaviors[i].IsAIReady() && Behaviors[i].RemoteControl == remoteControl)
					return Behaviors[i];

			}

			return null;

		}

	}

}
