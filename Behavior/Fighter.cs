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

namespace RivalAI.Behavior{
	
	public class Fighter : CoreBehavior{

        //Configurable
        public double FighterEngageDistanceSpace;
        public double FighterEngageDistancePlanet;
		
		public bool ReceivedEvadeSignal;
		public bool ReceivedRetreatSignal;
		public bool ReceivedExternalTarget;
		
        public byte Counter;

        public Fighter() {

            FighterEngageDistanceSpace = 300;
            FighterEngageDistancePlanet = 600;
			
			ReceivedEvadeSignal = false;
			ReceivedRetreatSignal = false;
			ReceivedExternalTarget = false;
			
            Counter = 0;

        }

        public void RunAi() {

            if(Owner.NpcOwned == false) {

                return;

            }

            RunCoreAi();

            if(EndScript == true) {

                return;

            }

            Counter++;

            if(Counter >= 60) {

                MainBehavior();
                Counter = 0;

            }


        }

        public void MainBehavior() {

            if(RAI_SessionCore.IsServer == false) {

                return;

            }
			
			if(ReceivedEvadeSignal == true && Mode != BehaviorMode.Retreat) {
				
				ReceivedEvadeSignal = false;
				
				if(Collision.UseCollisionDetection == true){
					
					Mode = BehaviorMode.EvadeCollision;
					//Set Waypoint Here
					AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotTarget);
					
				}
				
			}
			
			if(Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true){

				Mode = BehaviorMode.Retreat;
				AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotWaypoint);
			
			}
			
			if(ReceivedExternalTarget == true){
				
				ReceivedExternalTarget = false;
				//Set New Target
				
			}

            if(Mode == BehaviorMode.Init) {

                if(Targeting.InvalidTarget == true) {

                    Mode = BehaviorMode.WaitingForTarget;

                } else {

                    Mode = BehaviorMode.ApproachTarget;
                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotTarget);

                }

            }

            if(Mode == BehaviorMode.WaitingForTarget) {

                if(AutoPilot.Mode != AutoPilotMode.None) {

                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);

                }

                if(Targeting.InvalidTarget == false) {

                    Mode = BehaviorMode.ApproachTarget;
                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotTarget);

                }else if(Despawn.NoTargetExpire == true){
					
					Despawn.Retreat();
					
				}

            }

            if(Targeting.InvalidTarget == true && Mode != BehaviorMode.Retreat) {

                Mode = BehaviorMode.WaitingForTarget;

            }

            //Evade
            if(Mode == BehaviorMode.EvadeCollision) {

				if(AutoPilot.EvasionModeTimer >= AutoPilot.EvasionModeTimer || Vector3D.Distance(this.RemoteControl.GetPosition(), this.AutoPilot.WaypointCoords) < 50){
					
					AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotTarget);
					Mode = BehaviorMode.ApproachTarget;
					
				}

            }

            //Space Approach
            if(Mode == BehaviorMode.ApproachTarget && AutoPilot.UpDirection == Vector3D.Zero) {

                Weapons.AllowFire();
                var newCoords = VectorHelper.CreateDirectionAndTarget(AutoPilot.TargetCoords, RemoteControl.GetPosition(), AutoPilot.TargetCoords, this.FighterEngageDistanceSpace);
                AutoPilot.UpdateWaypoint(newCoords);
                CheckTarget();

                if(Targeting.Target.Distance < this.FighterEngageDistanceSpace) {

                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.RotateToTargetAndStrafe);
                    Mode = BehaviorMode.EngageTarget;

                }

            }

            //Space Engage
            if(Mode == BehaviorMode.EngageTarget && AutoPilot.UpDirection == Vector3D.Zero) {

                Weapons.AllowFire();
                CheckTarget();

                if(Targeting.Target.Distance > this.FighterEngageDistanceSpace) {

                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotTarget);
                    Mode = BehaviorMode.ApproachTarget;

                }

            }
			
			//Space Retreat
			if(Mode == BehaviorMode.Retreat && AutoPilot.UpDirection == Vector3D.Zero) {

				if(Despawn.NearestPlayer?.Controller?.ControlledEntity?.Entity != null){
					
					var despawnCoords = Vector3D.Normalize(this.RemoteControl.GetPosition() - Despawn.NearestPlayer.GetPosition()) * 1000 + this.RemoteControl.GetPosition();
					AutoPilot.UpdateWaypoint(despawnCoords);
					
				}

            }

            //Planet Approach
            if(Mode == BehaviorMode.ApproachTarget && AutoPilot.UpDirection != Vector3D.Zero) {

                CheckTarget();
                Weapons.AllowFire();
                if(Targeting.Target.Distance < this.FighterEngageDistancePlanet) {

                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.RotateToTargetAndStrafe);
                    Mode = BehaviorMode.EngageTarget;

                }

            }

            //Planet Engage
            if(Mode == BehaviorMode.EngageTarget && AutoPilot.UpDirection != Vector3D.Zero) {

                //Logger.AddMsg(AutoPilot.UpDirection.ToString(), true);
                CheckTarget();
                Weapons.AllowFire();
                if(Targeting.Target.Distance > this.FighterEngageDistancePlanet) {

                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotTarget);
                    Mode = BehaviorMode.ApproachTarget;

                }

            }
			
			//Planet Retreat
			if(Mode == BehaviorMode.Retreat && AutoPilot.UpDirection != Vector3D.Zero) {

                if(Despawn.NearestPlayer?.Controller?.ControlledEntity?.Entity != null){

                    //Logger.AddMsg("DespawnCoordsCreated", true);
					var roughDespawnCoords = VectorHelper.GetDirectionAwayFromTarget(this.RemoteControl.GetPosition(), Despawn.NearestPlayer.GetPosition()) * 1000 + this.RemoteControl.GetPosition();
					var despawnCoords = VectorHelper.GetPlanetWaypointPathing(this.RemoteControl.GetPosition(), roughDespawnCoords);
					AutoPilot.UpdateWaypoint(despawnCoords);
					
				}

            }


        }

        public void CheckTarget() {

            if(Targeting.InvalidTarget == true) {

                Mode = BehaviorMode.WaitingForTarget;
                AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);

            }

        }

        public void CollisionWarningTrigger(Vector3D collisionCoords) {

            if(Mode == BehaviorMode.ApproachTarget == true) {

                Mode = BehaviorMode.EvadeCollision;

                if(AutoPilot.UpDirection == Vector3D.Zero) {

                    AutoPilot.UpdateWaypoint(Collision.SpaceEvadeCoords);

                } else {

                    AutoPilot.UpdateWaypoint(Collision.PlanetEvadeCoords);

                }

                AutoPilot.ProcessEvasionCounter(true);
                AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotWaypoint);

            }

        }

        public void BehaviorInit(IMyRemoteControl remoteControl) {

            //Core Setup
            CoreSetup(remoteControl);

            //Behavior Specific Defaults
            Despawn.UseNoTargetTimer = true;
            Targeting.NeedsTarget = true;
            Weapons.UseStaticGuns = true;

            //Get Settings From Custom Data
            InitCoreTags();

            if(Targeting.TargetData.UseCustomTargeting == false) {

                Targeting.TargetData.Target = TargetTypeEnum.Player;
                Targeting.TargetData.Relations = TargetRelationEnum.Enemy;
                Targeting.TargetData.Owners = TargetOwnerEnum.Player;

            }

            Collision.TriggerWarning += CollisionWarningTrigger;

        }

        public void InitTags() {

            //Core Tags
            

            //Behavior Tags
            

        }

    }

}
	
