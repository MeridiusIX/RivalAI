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

    public class Strike : CoreBehavior, IBehavior {

        //Configurable

        public double StrikeBeginSpaceAttackRunDistance;
        public double StrikeBeginPlanetAttackRunDistance;
        public double StrikeBreakawayDistance;
        public int StrikeOffsetRecalculationTime;
        public bool StrikeEngageUseSafePlanetPathing;
        public bool StrikeEngageUseCollisionEvasionSpace;
        public bool StrikeEngageUseCollisionEvasionPlanet;

        private bool _defaultCollisionSettings = false;

        public DateTime LastOffsetCalculation;
        public bool TargetIsHigh;

        public byte Counter;

        public Strike() {

            StrikeBeginSpaceAttackRunDistance = 75;
            StrikeBeginPlanetAttackRunDistance = 100;
            StrikeBreakawayDistance = 450;
            StrikeOffsetRecalculationTime = 30;
            StrikeEngageUseSafePlanetPathing = true;
            StrikeEngageUseCollisionEvasionSpace = true;
            StrikeEngageUseCollisionEvasionPlanet = false;

            Counter = 0;

        }

        public override void MainBehavior() {

            bool skipEngageCheck = false;


            //Init
            if (Mode == BehaviorMode.Init) {

                if (NewAutoPilot.Targeting.InvalidTarget == true) {

                    ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

                } else {

                    ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
                    CreateAndMoveToOffset();
                    skipEngageCheck = true;

                }

            }

            //Waiting For Target
            if (Mode == BehaviorMode.WaitingForTarget) {

                if (NewAutoPilot.Targeting.InvalidTarget == false) {

                    ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
                    CreateAndMoveToOffset();
                    skipEngageCheck = true;

                } else if (Despawn.NoTargetExpire == true) {

                    Despawn.Retreat();

                }

            }

            if (NewAutoPilot.Targeting.InvalidTarget == true && Mode != BehaviorMode.Retreat) {

                ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

            }

            //Approach Target
            if (Mode == BehaviorMode.ApproachTarget && !skipEngageCheck) {

                double distance = NewAutoPilot.InGravity() ? this.StrikeBeginPlanetAttackRunDistance : this.StrikeBeginSpaceAttackRunDistance;

                if (NewAutoPilot.DistanceToCurrentWaypoint <= distance && !NewAutoPilot.IsAvoidingCollision()) {

                    ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);
                    NewAutoPilot.ActivateAutoPilot(AutoPilotType.RivalAI, NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward, RemoteControl.GetPosition(), true, false, StrikeEngageUseSafePlanetPathing);
                    skipEngageCheck = true;

                }

                if (skipEngageCheck == false) {

                    var timeSpan = MyAPIGateway.Session.GameDateTime - LastOffsetCalculation;

                    if (timeSpan.TotalSeconds >= StrikeOffsetRecalculationTime) {

                        skipEngageCheck = true;
                        CreateAndMoveToOffset();

                    }

                    if (TargetIsHigh && NewAutoPilot.Targeting.Target.Altitude < NewAutoPilot.OffsetPlanetMinTargetAltitude) {

                        TargetIsHigh = false;
                        CreateAndMoveToOffset();

                    } else if (!TargetIsHigh && NewAutoPilot.Targeting.Target.Altitude > NewAutoPilot.OffsetPlanetMinTargetAltitude) {

                        TargetIsHigh = true;
                        CreateAndMoveToOffset();

                    }

                }

            }

            //Engage Target
            if (Mode == BehaviorMode.EngageTarget && !skipEngageCheck) {

                Logger.MsgDebug("Strike: " + StrikeBreakawayDistance.ToString() + " - " + NewAutoPilot.DistanceToInitialWaypoint, DebugTypeEnum.General);
                if (NewAutoPilot.DistanceToInitialWaypoint <= StrikeBreakawayDistance || (NewAutoPilot.UseVelocityCollisionEvasion && NewAutoPilot.Collision.VelocityResult.CollisionImminent())) {

                    ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
                    CreateAndMoveToOffset();

                }
            
            }



        }

        public override void ChangeCoreBehaviorMode(BehaviorMode newMode) {

            base.ChangeCoreBehaviorMode(newMode);

            if (_defaultCollisionSettings == true) {

                if (this.Mode == BehaviorMode.EngageTarget) {

                    this.NewAutoPilot.UseVelocityCollisionEvasion = UseEngageCollisionEvasion();

                } else {

                    this.NewAutoPilot.UseVelocityCollisionEvasion = true;

                }

            }

        }

        private bool UseEngageCollisionEvasion() {

            return NewAutoPilot.InGravity() ? this.StrikeEngageUseCollisionEvasionPlanet : this.StrikeEngageUseCollisionEvasionSpace;
        
        }


        private void ChangeOffsetAction() {

            return;
            if(Mode == BehaviorMode.ApproachTarget)
                NewAutoPilot.ReverseOffsetDirection(70);

        }

        private void CreateAndMoveToOffset() {

            if (NewAutoPilot.InGravity()) {

                if (NewAutoPilot.Targeting.Target.Altitude > NewAutoPilot.OffsetPlanetMinTargetAltitude) {

                    //Logger.MsgDebug("Target Is High", DebugTypeEnum.General);
                    NewAutoPilot.SetRandomOffset(VectorHelper.RandomDistance(NewAutoPilot.OffsetPlanetMinTargetAltitude, NewAutoPilot.OffsetPlanetMaxTargetAltitude), 0, NewAutoPilot.Targeting.Target.Target);

                } else {

                    //Logger.MsgDebug("Target Is Low", DebugTypeEnum.General);
                    NewAutoPilot.SetRandomOffset(NewAutoPilot.Targeting.Target.Target);

                }
            
            } else {

                //Logger.MsgDebug("Target Is Space", DebugTypeEnum.General);
                NewAutoPilot.SetRandomOffset(NewAutoPilot.Targeting.Target.Target);
            
            }

            LastOffsetCalculation = MyAPIGateway.Session.GameDateTime;
            NewAutoPilot.ActivateAutoPilot(AutoPilotType.RivalAI, NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward, RemoteControl.GetPosition(), true, true, true);

        }

        public void BehaviorInit(IMyRemoteControl remoteControl) {

            //Core Setup
            CoreSetup(remoteControl);

            //Behavior Specific Defaults
            Despawn.UseNoTargetTimer = true;
            NewAutoPilot.Targeting.NeedsTarget = true;
            NewAutoPilot.Weapons.UseStaticGuns = true;
            NewAutoPilot.Collision.CollisionTimeTrigger = 5;
            NewAutoPilot.CollisionEvasionWaypointCalculatedAwayFromEntity = true;
            NewAutoPilot.OffsetSpaceMinDistFromTarget = 900;
            NewAutoPilot.OffsetSpaceMaxDistFromTarget = 1000;
            NewAutoPilot.OffsetPlanetMinDistFromTarget = 100;
            NewAutoPilot.OffsetPlanetMaxDistFromTarget = 150;
            NewAutoPilot.OffsetPlanetMinTargetAltitude = 900;
            NewAutoPilot.OffsetPlanetMaxTargetAltitude = 1100;
            NewAutoPilot.WaypointTolerance = 50;

            //Get Settings From Custom Data
            InitCoreTags();
            InitTags();

            //Behavior Specific Default Enums (If None is Not Acceptable)
            if (NewAutoPilot.Targeting.TargetType == TargetTypeEnum.None) {

                NewAutoPilot.Targeting.TargetType = TargetTypeEnum.Player;

            }

            if(NewAutoPilot.Targeting.TargetRelation == TargetRelationEnum.None) {

                NewAutoPilot.Targeting.TargetRelation = TargetRelationEnum.Enemy;

            }

            if(NewAutoPilot.Targeting.TargetOwner == TargetOwnerEnum.None) {

                NewAutoPilot.Targeting.TargetOwner = TargetOwnerEnum.Player;

            }

            Trigger.BehaviorEventA += ChangeOffsetAction;

            _defaultCollisionSettings = NewAutoPilot.UseVelocityCollisionEvasion;

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

