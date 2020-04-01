using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RivalAI.Behavior {
	public interface IBehavior {

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

	}
}
