using RivalAI.Behavior.Subsystems;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RivalAI.Behavior {
	public interface IBehavior {

		NewAutoPilotSystem NewAutoPilot { get; }
		BroadcastSystem Broadcast { get; }
		DamageSystem Damage { get; }
		DespawnSystem Despawn { get; }
		ExtrasSystem Extras { get; }
		OwnerSystem Owner { get; }
		SpawningSystem Spawning { get; }
		StoredSettings Settings { get; }
		TriggerSystem Trigger { get; }

		bool BehaviorTerminated { get; set; }

		void BehaviorInit(IMyRemoteControl remoteControl);
		bool IsAIReady();
		void ProcessCollisionChecks();
		void ProcessTargetingChecks();
		void ProcessAutoPilotChecks();
		void ProcessWeaponChecks();
		void ProcessTriggerChecks();
		void EngageAutoPilot();
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
