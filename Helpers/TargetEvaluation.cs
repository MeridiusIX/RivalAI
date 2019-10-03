using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Sandbox.Game.Weapons;
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

namespace RivalAI.Helpers {

    public class TargetEvaluation {

        public Vector3D MyPosition;
        public Vector3D MyVelocity;
        public bool UseLeadPrediction;
        public float ProjectileVelocity;

        public IMyEntity Target;
        public IMyTerminalBlock TargetBlock;
        public IMyPlayer TargetPlayer;
        public TargetTypeEnum TargetType;
        public Vector3D TargetCoords;
        public Vector3D TargetVelocity;
        public double TargetDistance;

        public DateTime TargetEvaluationTime;
        public bool TargetExists;
        public bool InSafeZone;
        public bool IsMoving;
        public bool IsArmed;
        public bool IsBroadcasting;
        public double BroadcastingRange;
        public bool IsPowered;
        public bool IsHumanControlled;
        public bool IsShielded;
        public bool HasThrust;

        public TargetEvaluation(IMyEntity entity, TargetTypeEnum entityType) {

            MyPosition = Vector3D.Zero;
            MyVelocity = Vector3D.Zero;
            UseLeadPrediction = false;
            ProjectileVelocity = 100;

            Target = entity;
            TargetBlock = null;
            TargetPlayer = null;
            TargetType = entityType;
            TargetCoords = Vector3D.Zero;
            TargetDistance = 0;
            TargetVelocity = Vector3D.Zero;

            TargetEvaluationTime = MyAPIGateway.Session.GameDateTime;
            TargetExists = false;
            InSafeZone = false;
            IsMoving = false;
            IsArmed = false;
            IsBroadcasting = false;
            BroadcastingRange = 0;
            IsPowered = false;
            IsHumanControlled = false;
            IsShielded = false;
            HasThrust = false;


        }

        public void Evaluate(IMyRemoteControl remoteControl, bool leadPrediction = false, float projectileVelocity = 100, bool useGridSpeedForLead = false) {

            this.MyPosition = remoteControl.GetPosition();
            this.UseLeadPrediction = leadPrediction;
            this.ProjectileVelocity = projectileVelocity;
            this.MyVelocity = remoteControl.GetShipVelocities().LinearVelocity;

            if(useGridSpeedForLead == true) {

                this.UseLeadPrediction = true;
                this.ProjectileVelocity = (float)this.MyVelocity.Length();

            }

            MyAPIGateway.Parallel.Start(EvaluateParallel);

        }

        public void EvaluateParallel() {

            //Check If Entity Exists
            if(this.TargetType == TargetTypeEnum.Block) {

                if(this.TargetBlock == null || MyAPIGateway.Entities.Exist(this.TargetBlock?.SlimBlock?.CubeGrid) == false) {

                    SetTargetsNull();
                    return;

                }

            } else {

                if(this.Target == null || MyAPIGateway.Entities.Exist(this.Target) == false) {

                    SetTargetsNull();
                    return;

                }

            }

            if(this.TargetType == TargetTypeEnum.Player) {

                var character = this.Target as IMyCharacter;

                if(character != null) {

                    if(character.IsDead == true) {

                        SetTargetsNull();
                        return;

                    }

                } else {

                    SetTargetsNull();
                    return;

                }

            }

            this.TargetExists = true;
            this.TargetEvaluationTime = MyAPIGateway.Session.GameDateTime;

            //TargetCoords
            if(this.TargetType == TargetTypeEnum.Grid) {

                this.TargetCoords = this.Target.PositionComp.WorldAABB.Center;

            } else {

                this.TargetCoords = this.Target.GetPosition();

            }

            this.TargetDistance = Vector3D.Distance(this.MyPosition, this.TargetCoords);
            this.InSafeZone = TargetHelper.IsPositionInSafeZone(this.TargetCoords);

            //IsMoving
            this.TargetVelocity = Vector3D.Zero;

            if(TargetType == TargetTypeEnum.Block) {

                if(TargetBlock?.SlimBlock?.CubeGrid?.Physics != null) {

                    this.TargetVelocity = Target.Physics.LinearVelocity;

                    if(TargetBlock.SlimBlock.CubeGrid.Physics.LinearVelocity.Length() > 0.1) {

                        this.IsMoving = true;

                    } else {

                        this.IsMoving = false;

                    }

                }

            } else {

                if(Target?.Physics != null) {

                    this.TargetVelocity = Target.Physics.LinearVelocity;

                    if(Target.Physics.LinearVelocity.Length() > 0.1) {

                        this.IsMoving = true;

                    } else {

                        this.IsMoving = false;

                    }

                }

            }

            //Target Lead Stuff
            if(this.UseLeadPrediction == true) {

                this.TargetCoords = VectorHelper.FirstOrderIntercept(this.MyPosition, this.MyVelocity, this.ProjectileVelocity, this.TargetCoords, this.TargetVelocity);

            }

            //IsHumanControlled
            if(TargetType == TargetTypeEnum.Block) {

                this.IsHumanControlled = TargetHelper.IsHumanControllingTarget(this.TargetBlock.SlimBlock.CubeGrid);

            } else if(TargetType == TargetTypeEnum.Grid) {

                this.IsHumanControlled = TargetHelper.IsHumanControllingTarget(this.Target as IMyCubeGrid);

            } else if(TargetType == TargetTypeEnum.Player) {

                this.IsHumanControlled = true;

            } else {

                this.IsHumanControlled = false;

            }

            //IsPowered
            if(TargetType == TargetTypeEnum.Block) {

                this.IsPowered = TargetHelper.IsGridPowered(this.TargetBlock.SlimBlock.CubeGrid);

            } else if(TargetType == TargetTypeEnum.Grid) {

                this.IsPowered = TargetHelper.IsGridPowered(this.Target as IMyCubeGrid);

            } else if(TargetType == TargetTypeEnum.Player && this.TargetPlayer != null) {

                if(MyVisualScriptLogicProvider.GetPlayersEnergyLevel(this.TargetPlayer.IdentityId) > 0) {

                    this.IsPowered = true;

                } else {

                    this.IsPowered = false;

                }

            } else {

                this.IsHumanControlled = false;

            }

            //IsShielded
            if(RAI_SessionCore.Instance.ShieldApiLoaded == true) {

                var api = RAI_SessionCore.Instance.SApi;
                this.IsShielded = api.ProtectedByShield(this.Target);

                if(this.IsShielded == true) {

                    var line = new LineD(this.MyPosition, this.TargetCoords);
                    var intersect = api.ClosestShieldInLine(line, true);

                    if(intersect.Item1.HasValue == true) {

                        this.TargetCoords = Vector3D.Normalize(this.TargetCoords - this.MyPosition) * intersect.Item1.Value + this.MyPosition;

                    }

                }
                
            } else {

                this.IsShielded = false;

            }

        }

        public void SetTargetsNull() {

            this.Target = null;
            this.TargetBlock = null;
            this.TargetPlayer = null;
            this.TargetExists = false;

        }

    }

}
