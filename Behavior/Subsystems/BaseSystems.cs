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
using RivalAI.Helpers;

namespace RivalAI.Behavior.Subsystems{
	
	public class BaseSystems{

        //Subsystems
        public TriggerSystem Trigger;
		public AutoPilotSystem AutoPilot;
		public BroadcastSystem Broadcast;
        public DamageSystem Damage;
        public DespawnSystem Despawn;
        public ExtrasSystem Extras;
        public OwnerSystem Owner;

        //Base Status
        bool IsWorking = false;
        bool IsFunctional = false;
        bool IsPhysicsEnabled = false;

        public BaseSystems(IMyRemoteControl remoteControl){

            Trigger = new TriggerSystem(remoteControl);
            AutoPilot = new AutoPilotSystem(remoteControl);
			Broadcast = new BroadcastSystem(remoteControl);
            Damage = new DamageSystem(remoteControl);
            Despawn = new DespawnSystem(remoteControl);
            Extras = new ExtrasSystem(remoteControl);
            Owner = new OwnerSystem(remoteControl);

        }
		
		public void SetupBaseSystems(IMyRemoteControl remoteControl){
			
            //Cross-Class Event Setup

            
            remoteControl.IsWorkingChanged += OnWorkingChanged;
            remoteControl.OnPhysicsChanged += PhysicsExist;

        }

        public void InitTags() {



        }

        public void OnWorkingChanged(IMyCubeBlock Block) {

            this.IsWorking = Block.IsWorking;
            this.IsFunctional = Block.IsFunctional;

        }

        public void PhysicsExist(IMyEntity entity) {

            if(entity.Physics == null) {

                this.IsPhysicsEnabled = false;

            } else {

                this.IsPhysicsEnabled = true;

            }

        }
		
	}
	
}