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
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI.Entities;
using RivalAI.Behavior.Subsystems.Profiles;
using RivalAI.Behavior.Subsystems.AutoPilot;

namespace RivalAI.Behavior {

    public class Strike : CoreBehavior, IBehavior {

        //Configurable

        public double StrikeBeginSpaceAttackRunDistance;
        public double StrikeBeginPlanetAttackRunDistance;
        public double StrikeBreakawayDistance;
        public int StrikeOffsetRecalculationTime;
        public bool StrikeEngageUseSafePlanetPathing;
        public bool StrikeEngageUseCollisionEvasionSpace;
        public bool StrikeEngageUseCollisionEvasionPlanet;

        public bool EngageOverrideWithDistanceAndTimer;
        public int EngageOverrideTimerTrigger;
        public double EngageOverrideDistance;

        private bool _defaultCollisionSettings = false;

        public DateTime LastOffsetCalculation;
        public DateTime EngageOverrideTimer;
        public bool TargetIsHigh;

        public byte Counter;

        public Strike() : base() {

            _behaviorType = "Strike";

            StrikeBeginSpaceAttackRunDistance = 75;
            StrikeBeginPlanetAttackRunDistance = 100;
            StrikeBreakawayDistance = 450;
            StrikeOffsetRecalculationTime = 30;
            StrikeEngageUseSafePlanetPathing = true;
            StrikeEngageUseCollisionEvasionSpace = true;
            StrikeEngageUseCollisionEvasionPlanet = false;

            EngageOverrideWithDistanceAndTimer = true;
            EngageOverrideTimerTrigger = 20;
            EngageOverrideDistance = 1200;

            LastOffsetCalculation = MyAPIGateway.Session.GameDateTime;
            EngageOverrideTimer = MyAPIGateway.Session.GameDateTime;

            Counter = 0;

        }

        public override void MainBehavior() {

            if (RAI_SessionCore.IsServer == false) {

                return;

            }

            base.MainBehavior();

            bool skipEngageCheck = false;

            if (Mode != BehaviorMode.Retreat && Despawn.DoRetreat == true) {

                Mode = BehaviorMode.Retreat;
                AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing, CheckEnum.Yes, CheckEnum.No);

            }

            //Init
            if (Mode == BehaviorMode.Init) {

                if (!AutoPilot.Targeting.HasTarget()) {

                    ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

                } else {

                    EngageOverrideTimer = MyAPIGateway.Session.GameDateTime;
                    ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
                    CreateAndMoveToOffset();
                    skipEngageCheck = true;

                }

            }

            //Waiting For Target
            if (Mode == BehaviorMode.WaitingForTarget) {

                if (AutoPilot.CurrentMode != AutoPilot.UserCustomMode) {

                    AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.None, CheckEnum.No, CheckEnum.Yes);

                }

                if (AutoPilot.Targeting.HasTarget()) {

                    EngageOverrideTimer = MyAPIGateway.Session.GameDateTime;
                    ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
                    CreateAndMoveToOffset();
                    skipEngageCheck = true;
                    BehaviorTriggerA = true;

                } else if (Despawn.NoTargetExpire == true) {

                    Despawn.Retreat();

                }

            }

            if (!AutoPilot.Targeting.HasTarget() && Mode != BehaviorMode.Retreat) {

                ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

            }

            //Approach Target
            if (Mode == BehaviorMode.ApproachTarget && !skipEngageCheck) {

                double distance = AutoPilot.InGravity() ? this.StrikeBeginPlanetAttackRunDistance : this.StrikeBeginSpaceAttackRunDistance;
                bool engageOverride = false;

                if (EngageOverrideWithDistanceAndTimer) {

                    if (AutoPilot.DistanceToCurrentWaypoint < EngageOverrideDistance) {
                    
                        var time = MyAPIGateway.Session.GameDateTime - EngageOverrideTimer;

                        if (time.TotalSeconds > EngageOverrideTimerTrigger) {

                            engageOverride = true;

                        }

                    }
                
                }

                if ((engageOverride || AutoPilot.DistanceToCurrentWaypoint <= distance) && AutoPilot.Targeting.Target.Distance(RemoteControl.GetPosition()) > this.StrikeBreakawayDistance && !AutoPilot.IsAvoidingCollision()) {

                    ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);
                    AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | (StrikeEngageUseSafePlanetPathing ? NewAutoPilotMode.PlanetaryPathing : NewAutoPilotMode.None) | NewAutoPilotMode.WaypointFromTarget);
                    skipEngageCheck = true;
                    BehaviorTriggerB = true;

                }

                if (skipEngageCheck == false) {

                    var timeSpan = MyAPIGateway.Session.GameDateTime - LastOffsetCalculation;

                    if (timeSpan.TotalSeconds >= StrikeOffsetRecalculationTime) {

                        skipEngageCheck = true;
                        AutoPilot.DebugDataA = "Offset Expire, Recalc";
                        CreateAndMoveToOffset();

                    }


                    if (AutoPilot.Data.ReverseOffsetDistAltAboveHeight) {

                        if (TargetIsHigh && AutoPilot.Targeting.Target.CurrentAltitude() < AutoPilot.Data.ReverseOffsetHeight) {

                            TargetIsHigh = false;
                            AutoPilot.DebugDataA = "Target is Low";
                            CreateAndMoveToOffset();

                        } else if (!TargetIsHigh && AutoPilot.Targeting.Target.CurrentAltitude() > AutoPilot.Data.ReverseOffsetHeight) {

                            TargetIsHigh = true;
                            AutoPilot.DebugDataA = "Target is High";
                            CreateAndMoveToOffset();

                        }

                    }
                    

                }

            }

            //Engage Target
            if (Mode == BehaviorMode.EngageTarget && !skipEngageCheck) {

                Logger.MsgDebug("Strike: " + StrikeBreakawayDistance.ToString() + " - " + AutoPilot.DistanceToInitialWaypoint, DebugTypeEnum.General);
                if (AutoPilot.DistanceToInitialWaypoint <= StrikeBreakawayDistance || (AutoPilot.Data.Unused && AutoPilot.Collision.VelocityResult.CollisionImminent())) {

                    EngageOverrideTimer = MyAPIGateway.Session.GameDateTime;
                    ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
                    CreateAndMoveToOffset();
                    BehaviorTriggerA = true;

                }
            
            }

        }

        public override void ChangeCoreBehaviorMode(BehaviorMode newMode) {

            base.ChangeCoreBehaviorMode(newMode);

            if (_defaultCollisionSettings == true) {

                if (this.Mode == BehaviorMode.EngageTarget) {

                    this.AutoPilot.Data.Unused = UseEngageCollisionEvasion();

                } else {

                    this.AutoPilot.Data.Unused = true;

                }

            }

        }

        private bool UseEngageCollisionEvasion() {

            return AutoPilot.InGravity() ? this.StrikeEngageUseCollisionEvasionPlanet : this.StrikeEngageUseCollisionEvasionSpace;
        
        }


        private void ChangeOffsetAction() {

            return;
            if(Mode == BehaviorMode.ApproachTarget)
                AutoPilot.ReverseOffsetDirection(70);

        }

        private void CreateAndMoveToOffset() {

            AutoPilot.OffsetWaypointGenerator(true);
            LastOffsetCalculation = MyAPIGateway.Session.GameDateTime;
            AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget | NewAutoPilotMode.OffsetWaypoint, CheckEnum.Yes, CheckEnum.No);

        }

        public override void BehaviorInit(IMyRemoteControl remoteControl) {

            //Core Setup
            CoreSetup(remoteControl);

            //Behavior Specific Defaults
            AutoPilot.Data = TagHelper.GetAutopilotProfile("RAI-Generic-Autopilot-Strike");
            Despawn.UseNoTargetTimer = true;
            AutoPilot.Weapons.UseStaticGuns = true;
            AutoPilot.Collision.CollisionTimeTrigger = 5;

            //Get Settings From Custom Data
            InitCoreTags();
            InitTags();
            SetDefaultTargeting();

            _defaultCollisionSettings = AutoPilot.Data.Unused;

            SetupCompleted = true;

        }

        public void InitTags() {

            if (string.IsNullOrWhiteSpace(this.RemoteControl?.CustomData) == false) {

                var descSplit = this.RemoteControl.CustomData.Split('\n');

                foreach (var tag in descSplit) {

                    //StrikeBeginSpaceAttackRunDistance
                    if (tag.Contains("[StrikeBeginSpaceAttackRunDistance:") == true) {

                        this.StrikeBeginSpaceAttackRunDistance = TagHelper.TagDoubleCheck(tag, this.StrikeBeginSpaceAttackRunDistance);

                    }

                    //StrikeBeginPlanetAttackRunDistance
                    if (tag.Contains("[StrikeBeginPlanetAttackRunDistance:") == true) {

                        this.StrikeBeginPlanetAttackRunDistance = TagHelper.TagDoubleCheck(tag, this.StrikeBeginPlanetAttackRunDistance);

                    }

                    //StrikeBreakawayDistance
                    if (tag.Contains("[StrikeBreakawayDistance:") == true) {

                        this.StrikeBreakawayDistance = TagHelper.TagDoubleCheck(tag, this.StrikeBreakawayDistance);

                    }

                    //StrikeOffsetRecalculationTime
                    if (tag.Contains("[StrikeOffsetRecalculationTime:") == true) {

                        this.StrikeOffsetRecalculationTime = TagHelper.TagIntCheck(tag, this.StrikeOffsetRecalculationTime);

                    }

                    //StrikeEngageUseSafePlanetPathing
                    if (tag.Contains("[StrikeEngageUseSafePlanetPathing:") == true) {

                        this.StrikeEngageUseSafePlanetPathing = TagHelper.TagBoolCheck(tag);

                    }

                }

            }

        }

    }

}

