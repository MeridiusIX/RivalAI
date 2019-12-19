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

namespace RivalAI.Behavior {

    public class Horsefly:CoreBehavior {

        //Configurable
        public double HorseflyMinDistFromWaypoint;
        public double HorseflyMinDistFromTarget;
        public double HorseflyMaxDistFromTarget;
        public int HorseflyWaypointWaitTimeTrigger;
        public int HorseflyWaypointAbandonTimeTrigger;

        public bool ReceivedEvadeSignal;
        public bool ReceivedRetreatSignal;
        public bool ReceivedExternalTarget;

        public byte Counter;
        public int HorseflyWaypointWaitTime;
        public int HorseflyWaypointAbandonTime;

        public Horsefly() {

            HorseflyMinDistFromWaypoint = 50;
            HorseflyMinDistFromTarget = 150;
            HorseflyMaxDistFromTarget = 300;
            HorseflyWaypointWaitTimeTrigger = 5;
            HorseflyWaypointAbandonTimeTrigger = 30;

            ReceivedEvadeSignal = false;
            ReceivedRetreatSignal = false;
            ReceivedExternalTarget = false;

            Counter = 0;
			HorseflyWaypointWaitTime = 0;
            HorseflyWaypointAbandonTime = 0;

        }

        public void RunAi() {

            if(!IsAIReady())
                return;

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

                if(Collision.UseCollisionDetection == true) {

                    Mode = BehaviorMode.WaitAtWaypoint;
                    //Set Waypoint Here
                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);

                }

            }

            if(Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true) {

                Mode = BehaviorMode.Retreat;
                AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotWaypoint);

            }

            if(ReceivedExternalTarget == true) {

                ReceivedExternalTarget = false;
                //Set New Target

            }

            if(Mode == BehaviorMode.Init) {

                if(Targeting.InvalidTarget == true) {

                    Mode = BehaviorMode.WaitingForTarget;

                } else {

                    Mode = BehaviorMode.WaitAtWaypoint;
					this.HorseflyWaypointWaitTime = this.HorseflyWaypointWaitTimeTrigger;
					AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);

                }

            }

            if(Mode == BehaviorMode.WaitingForTarget) {

                if(AutoPilot.Mode != AutoPilotMode.None) {

                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);

                }

                if(Targeting.InvalidTarget == false) {

                    ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
                    this.HorseflyWaypointWaitTime = this.HorseflyWaypointWaitTimeTrigger;
                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);

                } else if(Despawn.NoTargetExpire == true) {

                    Despawn.Retreat();

                }

            }

            if(Targeting.InvalidTarget == true && Mode != BehaviorMode.Retreat) {

                ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

                

            }

            //WaitAtWaypoint
            if(Mode == BehaviorMode.WaitAtWaypoint == true) {

                this.HorseflyWaypointWaitTime++;

                if(this.HorseflyWaypointWaitTime >= this.HorseflyWaypointWaitTimeTrigger) {

                    if(AutoPilot.UpDirection == Vector3D.Zero) {

						var dist = VectorHelper.RandomDistance(this.HorseflyMinDistFromTarget, this.HorseflyMaxDistFromTarget);
						var direction = VectorHelper.RandomDirection();
						var coordsA = direction * dist + this.Targeting.Target.TargetCoords;
						var coordsB = direction * -dist + this.Targeting.Target.TargetCoords;
						
						var distA = Vector3D.Distance(coordsA, this.RemoteControl.GetPosition());
						var distB = Vector3D.Distance(coordsB, this.RemoteControl.GetPosition());
						
						if(distA < distB){
							
							AutoPilot.UpdateWaypoint(coordsA);
							
						}else{
							
							AutoPilot.UpdateWaypoint(coordsB);
							
						}

                    } else {

                        var randomPerp = VectorHelper.RandomPerpendicular(AutoPilot.UpDirection);
                        var roughCoords = randomPerp * VectorHelper.RandomDistance(this.HorseflyMinDistFromTarget, this.HorseflyMaxDistFromTarget) + Targeting.GetTargetPosition();
                        var surfaceCoords = VectorHelper.GetPlanetSurfaceCoordsAtPosition(roughCoords);
                        var targetAltitude = Vector3D.Distance(AutoPilot.PlanetCore, Targeting.GetTargetPosition());
                        var finalCoords = Vector3D.Normalize(surfaceCoords - AutoPilot.PlanetCore) * targetAltitude + AutoPilot.PlanetCore;
                        AutoPilot.UpdateWaypoint(finalCoords);

                    }

                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotWaypoint);
                    ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
                    this.HorseflyWaypointWaitTime = 0;

                }

            }

            //Approach
            if(Mode == BehaviorMode.ApproachTarget) {

                this.HorseflyWaypointAbandonTime++;
                var toCoords = AutoPilot.WaypointCoords;
                bool atPosition = false;
                bool timeUp = this.HorseflyWaypointAbandonTime >= this.HorseflyWaypointAbandonTimeTrigger;

                if(AutoPilot.UpDirection == Vector3D.Zero) {

                    atPosition = Vector3D.Distance(this.RemoteControl.GetPosition(), toCoords) < this.HorseflyMinDistFromWaypoint;

                } else {

                    var safePointDist = Vector3D.Distance(this.RemoteControl.GetPosition(), AutoPilot.PlanetSafeWaypointCoords);
                    var sealevelA = VectorHelper.GetPlanetSealevelAtPosition(AutoPilot.WaypointCoords, AutoPilot.Planet);
                    var sealevelB = VectorHelper.GetPlanetSealevelAtPosition(AutoPilot.PlanetSafeWaypointCoords, AutoPilot.Planet);
                    var waypointDist = Vector3D.Distance(sealevelA, sealevelB);

                    if(safePointDist < this.HorseflyMinDistFromWaypoint && waypointDist < this.HorseflyMinDistFromWaypoint) {

                        atPosition = true;

                    }

                }

				if(timeUp || atPosition == true) {

                    if(timeUp == true) {

                        this.HorseflyWaypointWaitTime = this.HorseflyWaypointWaitTimeTrigger;

                    }
                        
                    this.HorseflyWaypointAbandonTime = 0;
                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);
                    ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);

                }

            }

            //Space Retreat
            if(Mode == BehaviorMode.Retreat && AutoPilot.UpDirection == Vector3D.Zero) {

                if(Despawn.NearestPlayer?.Controller?.ControlledEntity?.Entity != null) {

                    var despawnCoords = Vector3D.Normalize(this.RemoteControl.GetPosition() - Despawn.NearestPlayer.GetPosition()) * 1000 + this.RemoteControl.GetPosition();
                    AutoPilot.UpdateWaypoint(despawnCoords);

                }

            }

            //Planet Retreat
            if(Mode == BehaviorMode.Retreat && AutoPilot.UpDirection != Vector3D.Zero) {

                if(Despawn.NearestPlayer?.Controller?.ControlledEntity?.Entity != null) {

                    //Logger.AddMsg("DespawnCoordsCreated", true);
                    var roughDespawnCoords = VectorHelper.GetDirectionAwayFromTarget(this.RemoteControl.GetPosition(), Despawn.NearestPlayer.GetPosition()) * 1000 + this.RemoteControl.GetPosition();
                    var despawnCoords = VectorHelper.GetPlanetWaypointPathing(this.RemoteControl.GetPosition(), roughDespawnCoords);
                    AutoPilot.UpdateWaypoint(despawnCoords);

                }

            }


        }

        public void CollisionWarningTrigger(Vector3D collisionCoords) {

            if(Mode == BehaviorMode.ApproachTarget == true) {

                ChangeCoreBehaviorMode(BehaviorMode.WaitAtWaypoint);
                AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);
                this.HorseflyWaypointWaitTime = this.HorseflyWaypointWaitTimeTrigger;

            }

        }

        public void BehaviorInit(IMyRemoteControl remoteControl) {

            //Core Setup
            CoreSetup(remoteControl);

            //Behavior Specific Defaults
            Despawn.UseNoTargetTimer = true;
            Targeting.NeedsTarget = true;

            //Get Settings From Custom Data
            InitCoreTags();

            //Behavior Specific Default Enums (If None is Not Acceptable)
            if(Targeting.TargetType == TargetTypeEnum.None) {

                Targeting.TargetType = TargetTypeEnum.Player;

            }

            if(Targeting.TargetRelation == TargetRelationEnum.None) {

                Targeting.TargetRelation = TargetRelationEnum.Enemy;

            }

            if(Targeting.TargetOwner == TargetOwnerEnum.None) {

                Targeting.TargetOwner = TargetOwnerEnum.Player;

            }

            Collision.TriggerWarning += CollisionWarningTrigger;

        }

        public void InitTags() {

            //Core Tags


            //Behavior Tags


        }

    }

}

