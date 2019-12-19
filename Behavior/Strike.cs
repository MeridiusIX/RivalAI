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

    public class Strike:CoreBehavior {

        //Configurable
        public double StrikePlanetTargetInitialAltitude;
        public double StrikePlanetEngageFromInitialDistance;
        public double StrikeBreakawayDistnace;
        public int StrikeMaximumAttackRuns;

        public Vector3D PlanetInitalTargetCoords;
        public Vector3D PlanetApproachCoords;
        public int CurrentAttackRuns;

        public bool ReceivedEvadeSignal;
        public bool ReceivedRetreatSignal;
        public bool ReceivedExternalTarget;

        public byte Counter;

        public Strike() {

            StrikePlanetTargetInitialAltitude = 1200;
            StrikePlanetEngageFromInitialDistance = 100;
            StrikeBreakawayDistnace = 450;
            StrikeMaximumAttackRuns = 10;

            PlanetInitalTargetCoords = Vector3D.Zero;
            PlanetApproachCoords = Vector3D.Zero;
            CurrentAttackRuns = 0;

            ReceivedEvadeSignal = false;
            ReceivedRetreatSignal = false;
            ReceivedExternalTarget = false;

            Counter = 0;

        }

        public void RunAi() {

            if(!IsAIReady())
                return;

            RunCoreAi();

            if(EndScript == true) {

                return;

            }

            Counter++;

            if(Counter % 20 == 0) {

                if(Mode == BehaviorMode.ApproachTarget && Targeting.InvalidTarget == false && AutoPilot.UpDirection != Vector3D.Zero)
                    PlanetApproachPathing();

            }

            if(Counter >= 60) {

                MainBehavior();
                Counter = 0;

            }


        }

        public void PlanetApproachPathing() {

            MyAPIGateway.Parallel.Start(() => {

                var dirPlanetCoreToTarget = Vector3D.Normalize(Targeting.GetTargetPosition() - AutoPilot.PlanetCore);
                var initialApproachCoords = dirPlanetCoreToTarget * this.StrikePlanetTargetInitialAltitude + Targeting.GetTargetPosition();
                this.PlanetApproachCoords = VectorHelper.GetPlanetWaypointPathing(this.RemoteControl.GetPosition(), initialApproachCoords, 200, 1000, true);

            }, () => {

                MyAPIGateway.Utilities.InvokeOnGameThread(() => {

                    AutoPilot.UpdateWaypoint(this.PlanetApproachCoords);

                });

            });

        }

        public void CreateApproachOffset() {



        }

        public void MainBehavior() {

            if(RAI_SessionCore.IsServer == false) {

                return;

            }

            if(ReceivedEvadeSignal == true && Mode != BehaviorMode.Retreat) {

                ReceivedEvadeSignal = false;

                if(Collision.UseCollisionDetection == true) {



                }

            }

            if(Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true) {

                Mode = BehaviorMode.Retreat;
                

            }

            if(ReceivedExternalTarget == true) {

                ReceivedExternalTarget = false;
                //Set New Target

            }

            if(Mode == BehaviorMode.Init) {

                AutoPilot.SetRemoteControl(this.RemoteControl, false, Vector3D.Zero);
                Mode = BehaviorMode.WaitingForTarget;

            }

            if(Mode == BehaviorMode.WaitingForTarget) {

                if(AutoPilot.Mode != AutoPilotMode.None) {

                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);

                }

                if(Targeting.InvalidTarget == false) {

                    Mode = BehaviorMode.ApproachTarget;
                    CreateApproachOffset();
                    SetWaypointTargetAndAutoPilot();

                } else if(Despawn.NoTargetExpire == true) {

                    Despawn.Retreat();

                }

            }

            if(Targeting.InvalidTarget == true && Mode != BehaviorMode.Retreat) {

                Mode = BehaviorMode.WaitingForTarget;

            }

            //Evade
            if(Mode == BehaviorMode.EvadeCollision) {

                if(AutoPilot.EvasionModeTimer >= AutoPilot.EvasionModeTimer || Vector3D.Distance(this.RemoteControl.GetPosition(), this.AutoPilot.WaypointCoords) < 50) {

                    //AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotTarget);
                    Mode = BehaviorMode.ApproachTarget;

                }

            }

            //Space Approach
            if(Mode == BehaviorMode.ApproachTarget && AutoPilot.UpDirection == Vector3D.Zero) {

            

            }

            //Space Engage
            if(Mode == BehaviorMode.EngageTarget && AutoPilot.UpDirection == Vector3D.Zero) {

                

            }

            //Space Retreat
            if(Mode == BehaviorMode.Retreat && AutoPilot.UpDirection == Vector3D.Zero) {

                if(Despawn.NearestPlayer?.Controller?.ControlledEntity?.Entity != null) {

                    var despawnCoords = Vector3D.Normalize(this.RemoteControl.GetPosition() - Despawn.NearestPlayer.GetPosition()) * 1000 + this.RemoteControl.GetPosition();
                    AutoPilot.UpdateWaypoint(despawnCoords);

                }

            }

            //Planet Approach
            if(Mode == BehaviorMode.ApproachTarget && AutoPilot.UpDirection != Vector3D.Zero) {

                Weapons.CeaseFire();

                if(Vector3D.Distance(this.PlanetApproachCoords, this.RemoteControl.GetPosition()) <= this.StrikePlanetEngageFromInitialDistance) {

                    ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);
                    AutoPilot.ChangeAutoPilotMode(AutoPilotMode.FlyToTarget);

                }

            }

            //Planet Engage
            if(Mode == BehaviorMode.EngageTarget && AutoPilot.UpDirection != Vector3D.Zero) {

                Weapons.AllowFire();

                if(Targeting.Target.Distance < this.StrikeBreakawayDistnace) {

                    ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
                    SetWaypointTargetAndAutoPilot();

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

        public void SetWaypointTargetAndAutoPilot() {

            if(AutoPilot.UpDirection == Vector3D.Zero) {

                AutoPilot.ChangeAutoPilotMode(AutoPilotMode.FlyToTarget);

            } else {

                this.PlanetInitalTargetCoords = VectorHelper.CreateDirectionAndTarget(AutoPilot.PlanetCore, Targeting.GetTargetPosition(), Targeting.GetTargetPosition(), this.StrikePlanetTargetInitialAltitude);
                AutoPilot.UpdateWaypoint(this.PlanetInitalTargetCoords);
                AutoPilot.ChangeAutoPilotMode(AutoPilotMode.FlyToWaypoint);

            }

        }

        public void CheckTarget() {

            if(Targeting.InvalidTarget == true) {

                Mode = BehaviorMode.WaitingForTarget;
                AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);

            }

        }

        public void CollisionWarningTrigger(Vector3D collisionCoords) {

            if(Mode == BehaviorMode.ApproachTarget) {

                Mode = BehaviorMode.EvadeCollision;

                if(AutoPilot.UpDirection == Vector3D.Zero) {

                    AutoPilot.UpdateWaypoint(Collision.SpaceEvadeCoords);

                } else {

                    //Create Evade Coords Based on Current Mode

                }

                AutoPilot.ProcessEvasionCounter(true);
                //AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilotWaypoint);

            }

            if(Mode == BehaviorMode.EngageTarget) {

                SetWaypointTargetAndAutoPilot();
                CreateApproachOffset();
                AutoPilot.ChangeAutoPilotMode(AutoPilotMode.FlyToWaypoint);

            }

        }

        public void BehaviorInit(IMyRemoteControl remoteControl) {

            //Core Setup
            CoreSetup(remoteControl);

            //Behavior Specific Defaults
            Despawn.UseNoTargetTimer = true;
            Targeting.NeedsTarget = true;
            Weapons.UseStaticGuns = true;
            Collision.CollisionTimeTrigger = 5;

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

