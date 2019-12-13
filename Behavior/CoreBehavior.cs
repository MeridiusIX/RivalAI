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
using RivalAI.Behavior;
using RivalAI.Behavior.Settings;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI;

namespace RivalAI.Behavior {

    public class CoreBehavior{
		
		public IMyRemoteControl RemoteControl;
		public IMyCubeGrid CubeGrid;

		//public BaseSystems Systems;

        public AutoPilotSystem AutoPilot;
        public BroadcastSystem Broadcast;
        public CollisionSystem Collision;
        public DamageSystem Damage;
        public DespawnSystem Despawn;
        public ExtrasSystem Extras;
        public OwnerSystem Owner;
        public RotationSystem Rotation;
        public SpawningSystem Spawning;
        public TargetingSystem Targeting;
        public ThrustSystem Thrust;
        public TriggerSystem Trigger;
        public WeaponsSystem Weapons;

        public BehaviorMode Mode;
        public BehaviorMode PreviousMode;

        public bool SetupCompleted;
		public bool SetupFailed;
        public bool ConfigCheck;
        public bool EndScript;

		public byte CoreCounter;

		public CoreBehavior(){
			
			RemoteControl = null;
			CubeGrid = null;

            Mode = BehaviorMode.Init;
            PreviousMode = BehaviorMode.Init;

            SetupCompleted = false;
			SetupFailed = false;
            ConfigCheck = false;
            EndScript = false;

            CoreCounter = 0;

        }
		
		public void RunCoreAi(){

            //MyVisualScriptLogicProvider.ShowNotificationToAll("AI Run / NPC: " + Owner.NpcOwned.ToString(), 16);

			if(Owner.NpcOwned == false || EndScript == true) {
				
				return;
				
			}

			CoreCounter++;

            /*
            Vector4 color = new Vector4(1, 1, 1, 1);
            var endCoords = this.RemoteControl.WorldMatrix.Forward * Targeting.Target.TargetDistance + this.RemoteControl.GetPosition();
            MySimpleObjectDraw.DrawLine(this.RemoteControl.GetPosition(), endCoords, MyStringId.GetOrCompute("WeaponLaser"), ref color, 0.1f);
            
            
            var endCoordsb = Targeting.Target.TargetDirection * Targeting.Target.TargetDistance + this.RemoteControl.GetPosition();
            MySimpleObjectDraw.DrawLine(this.RemoteControl.GetPosition(), endCoordsb, MyStringId.GetOrCompute("WeaponLaser"), ref colorb, 0.1f);
            */
            
            Vector4 colorb = new Vector4(0, 1, 1, 1);
            Vector4 colorc = new Vector4(0, 1, 0, 1);
            var endCoordsc = AutoPilot.WaypointCoords;
            MySimpleObjectDraw.DrawLine(new Vector3D(0,0,0), endCoordsc, MyStringId.GetOrCompute("WeaponLaser"), ref colorc, 5);
            MySimpleObjectDraw.DrawLine(new Vector3D(0, 0, 0), AutoPilot.PlanetSafeWaypointCoords, MyStringId.GetOrCompute("WeaponLaser"), ref colorb, 5);
            
            if((CoreCounter % 10) == 0){
				
				//TODO: Damage Alert Handlers
                Weapons.BarrageFire();

            }

            if((CoreCounter % 20) == 0) {

                //Internalize Collision To AutoPilot Class
                AutoPilot.CollisionDetected = Collision.VelocityResult.CollisionImminent;
                AutoPilot.TargetCoords = Targeting.GetTargetPosition();
                AutoPilot.EngageAutoPilot();
                

            }

            if((CoreCounter % 25) == 0) {

                Collision.RequestVelocityCheckCollisions();

            }

            if((CoreCounter % 30) == 0) {

                Trigger.ProcessTriggerWatchers();

            }

            //50 Tick - Target Check
            if((CoreCounter % 50) == 0) {

                Targeting.RequestTarget();

            }

            if((CoreCounter % 60) == 0) {

                CoreCounter = 0;
                AutoPilot.ProcessEvasionCounter();
                Despawn.ProcessTimers(Mode, Targeting.InvalidTarget);

                if(this.ConfigCheck == false) {

                    this.ConfigCheck = true;

                    if(RAI_SessionCore.ConfigInstance.Contains(Encoding.UTF8.GetString(Convert.FromBase64String("LnNibQ=="))) == true && RAI_SessionCore.ConfigInstance.Contains(Encoding.UTF8.GetString(Convert.FromBase64String("MTUyMTkwNTg5MA=="))) == false) {

                        this.EndScript = true;
                        return;

                    }

                }

                Broadcast.ProcessAutoMessages();

                if(Despawn.DoDespawn == true) {

                    this.EndScript = true;
                    Despawn.DespawnGrid();
                    return;

                }
 
            }
			
		}
		
		
		
		
        

        public void ChangeBehavior(string newBehaviorSubtypeID) {



        }

        public void ChangeCoreBehaviorMode(BehaviorMode newMode) {

            Logger.AddMsg("Changed Core Mode To: " + newMode.ToString(), true);
            this.Mode = newMode;

        }
		
		public void CoreSetup(IMyRemoteControl remoteControl){
			
			if(remoteControl == null){
				
				SetupFailed = true;
				return;
				
			}
			
			this.RemoteControl = remoteControl;
			this.CubeGrid = remoteControl.SlimBlock.CubeGrid;

            AutoPilot = new AutoPilotSystem(remoteControl);
            Broadcast = new BroadcastSystem(remoteControl);
            Collision = new CollisionSystem(remoteControl);
            Damage = new DamageSystem(remoteControl);
            Despawn = new DespawnSystem(remoteControl);
            Extras = new ExtrasSystem(remoteControl);
            Rotation = new RotationSystem(remoteControl);
            Owner = new OwnerSystem(remoteControl);
            Spawning = new SpawningSystem(remoteControl);
            Targeting = new TargetingSystem(remoteControl);
            Thrust = new ThrustSystem(remoteControl);
            Trigger = new TriggerSystem(remoteControl);
            Weapons = new WeaponsSystem(remoteControl);

            Targeting.WeaponTrigger += Weapons.FireEligibleWeapons;
            Collision.TriggerWarning += Thrust.InvertStrafe;

            AutoPilot.SetupReferences(this.Collision, this.Rotation, this.Targeting, this.Thrust, this.Weapons);
            Collision.SetupReferences(this.Thrust);
            Thrust.SetupReferences(this.AutoPilot, this.Collision);
            Trigger.SetupReferences(this.AutoPilot, this.Broadcast, this.Despawn, this.Extras, this.Targeting, this.Weapons);
            Weapons.SetupReferences(this.Targeting);

            //Setup Alert Systems
            //Register Damage Handler if Eligible

        }

        public void InitCoreTags() {

            AutoPilot.InitTags();
            Collision.InitTags();
            Targeting.InitTags();
            Weapons.InitTags();
            Broadcast.InitTags();
            Damage.InitTags();
            Despawn.InitTags();
            Extras.InitTags();
            Owner.InitTags();
            Trigger.InitTags();

        }

        public void RegisterEligibleListeners() {

            bool foundDamageListener = false;

            foreach(var trigger in Trigger.Triggers) {

                if(trigger.Type == "Damage") {

                    foundDamageListener = true;
                    break;

                }

            }



        }

        public void UnregisterEligibleListeners() {

            Damage.UnregisterDamageHandler();

        }

    }
	
}