using RivalAI.Behavior.Subsystems;
using RivalAI.Behavior.Subsystems.AutoPilot;
using RivalAI.Behavior.Subsystems.Trigger;
using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace RivalAI.Behavior {
	public interface IBehavior {

		AutoPilotSystem AutoPilot { get; }
		BroadcastSystem Broadcast { get; }
		DamageSystem Damage { get; }
		DespawnSystem Despawn { get; }
		GridSystem Grid { get; }
		OwnerSystem Owner { get; }
		StoredSettings Settings { get; }
		TriggerSystem Trigger { get; }
		BehaviorMode Mode { get; }
		bool BehaviorTerminated { get; set; }
		bool BehaviorTriggerA { get; set; }
		bool BehaviorTriggerB { get; set; }
		bool BehaviorTriggerC { get; set; }
		bool BehaviorTriggerD { get; set; }
		bool BehaviorTriggerE { get; set; }
		bool BehaviorTriggerF { get; set; }
		bool BehaviorTriggerG { get; set; }
		bool BehaviorTriggerH { get; set; }
		bool BehaviorActionA { get; set; }
		bool BehaviorActionB { get; set; }
		bool BehaviorActionC { get; set; }
		bool BehaviorActionD { get; set; }
		bool BehaviorActionE { get; set; }
		bool BehaviorActionF { get; set; }
		bool BehaviorActionG { get; set; }
		bool BehaviorActionH { get; set; }

		void BehaviorInit(IMyRemoteControl remoteControl);
		string BehaviorType { get; }
		List<IMyCubeGrid> CurrentGrids { get; }
		List<IMyCockpit> DebugCockpits { get; }
		long GridId { get; }
		string GridName { get; }
		IMyRemoteControl RemoteControl { get; set; }
		bool IsAIReady();
		void ProcessCollisionChecks();
		void ProcessTargetingChecks();
		void ProcessAutoPilotChecks();
		void ProcessWeaponChecks();
		void ProcessTriggerChecks();
		void EngageAutoPilot();
		void SetDebugCockpit(IMyCockpit block, bool addMode);
		void SetInitialWeaponReadiness();
		void FireWeapons();
		void FireBarrageWeapons();
		void ProcessActivatedTriggers();
		void CheckDespawnConditions();
		void RunMainBehavior();
		bool IsClosed();
		void DebugDrawWaypoints();
		void ChangeTargetProfile(string newTargetProfile);
		void ChangeBehavior(string newBehavior, bool preserveSettings, bool preserveTriggers, bool preserveTargetData);

	}
}
