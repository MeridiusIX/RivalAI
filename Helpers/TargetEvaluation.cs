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
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Helpers {

    public class TargetEvaluation {

        public Vector3D MyPosition;
        public Vector3D MyVelocity;
        public Vector3D MyDirection;
        public IMyRemoteControl RemoteControl;

        public long MyIdentityId;
        public float ProjectileVelocity;
        private TargetProfile _targetData;

        public IMyEntity Target;
        public IMyTerminalBlock TargetBlock;
        public IMyPlayer TargetPlayer;
        public TargetTypeEnum TargetType;
        public Vector3D TargetCoords;
        public Vector3D TargetVelocity;
        public Vector3D TargetDirection;
        public double Distance;
        public double Altitude;
        public double TargetAngle;

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

        public TargetObstructionEnum TargetObstruction;
        public double TargetObstructionDistance;
        public IMyEntity TargetObstructionEntity;

        public TargetEvaluation(IMyEntity entity, TargetTypeEnum entityType) {

            MyPosition = Vector3D.Zero;
            MyVelocity = Vector3D.Zero;
            MyDirection = Vector3D.Zero;
            RemoteControl = null;
            ProjectileVelocity = 100;

            Target = entity;
            TargetBlock = null;
            TargetPlayer = null;
            TargetType = entityType;
            TargetCoords = Vector3D.Zero;
            Distance = 0;
            TargetDirection = Vector3D.Zero;
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

            TargetObstruction = TargetObstructionEnum.None;
            TargetObstructionDistance = 0;
            TargetObstructionEntity = null;

        }

        public void Evaluate(IMyRemoteControl remoteControl, TargetProfile targetData, float projectileVelocity = 100) {

            this.RemoteControl = remoteControl;
            this.MyPosition = remoteControl.GetPosition();
            this.MyDirection = remoteControl.WorldMatrix.Forward;
            this.MyIdentityId = remoteControl.OwnerId;
            this.MyVelocity = remoteControl.GetShipVelocities().LinearVelocity;
            this._targetData = targetData;

            if(targetData.UseCollisionLead == true) {

                this.ProjectileVelocity = (float)this.MyVelocity.Length();

            }

            try {
            
            
            
            } catch (Exception e) {

                Logger.WriteLog("Exception in target eval");
                Logger.WriteLog(e.ToString());

            }

            EvaluateParallel();

        }

        public void EvaluateParallel() {

            //Logger.MsgDebug("Target Evaluation Start: ", DebugTypeEnum.Target);
            var start = DateTime.Now;
            var stepTime = DateTime.Now;
            //Check If Entity Exists
            if(this.TargetType == TargetTypeEnum.Block) {

                if(this.TargetBlock == null || MyAPIGateway.Entities.Exist(this.TargetBlock?.SlimBlock?.CubeGrid) == false) {

                    SetTargetsNull("Target Block or Block Grid Null");
                    return;

                }

                if(this.TargetBlock.IsFunctional == false) {

                    SetTargetsNull("Target Block Not Functional");
                    return;

                }

                this.Target = this.TargetBlock.SlimBlock.CubeGrid;

            }

            if(this.TargetType == TargetTypeEnum.Grid) {

                if(this.Target == null || MyAPIGateway.Entities.Exist(this.Target) == false) {

                    //Logger.AddMsg("Target Grid Null", true);
                    SetTargetsNull("Target Grid Null");
                    return;

                }

            }

            //Player
            if(this.TargetType == TargetTypeEnum.Player) {

                var character = this.TargetPlayer?.Controller?.ControlledEntity?.Entity;

                if((character as IMyCharacter) != null) {

                    if((character as IMyCharacter).IsDead == true) {

                        SetTargetsNull("Target Character Dead");
                        return;

                    }

                } else if(character == null) {

                    SetTargetsNull("Target Character Null");
                    return;

                }

                if(this.Target != this.TargetPlayer.Controller.ControlledEntity.Entity) {

                    this.Target = this.TargetPlayer.Controller.ControlledEntity.Entity;

                }

            }

            //Logger.MsgDebug("Target Exists: ", DebugTypeEnum.Target);
            this.TargetExists = true;
            this.TargetEvaluationTime = MyAPIGateway.Session.GameDateTime;

            //TargetCoords
            this.TargetCoords = this.Target.PositionComp.WorldAABB.Center;

            this.Distance = Vector3D.Distance(this.MyPosition, this.TargetCoords);
            this.Altitude = VectorHelper.GetAltitudeAtPosition(this.TargetCoords);
            this.TargetDirection = Vector3D.Normalize(this.TargetCoords - this.MyPosition);
            //this.TargetAngle = VectorHelper.GetAngleBetweenDirections(this.MyDirection, this.TargetDirection);
            //this.InSafeZone = TargetHelper.IsPositionInSafeZone(this.TargetCoords);

            //Logger.MsgDebug("Target moving: ", DebugTypeEnum.Target);
            //IsMoving
            this.TargetVelocity = Vector3D.Zero;
            this.IsMoving = false;

            if (TargetType == TargetTypeEnum.Block) {

                if(TargetBlock?.SlimBlock?.CubeGrid?.Physics != null) {

                    this.TargetVelocity = Target.Physics.LinearVelocity;

                    if(TargetBlock.SlimBlock.CubeGrid.Physics.LinearVelocity.Length() > 0.1) {

                        this.IsMoving = true;

                    }

                }

            } else {

                if (Target != null) {

                    if (Target?.Physics != null) {

                        this.TargetVelocity = Target.Physics.LinearVelocity;

                        if (Target.Physics.LinearVelocity.Length() > 0.1) {

                            this.IsMoving = true;

                        }

                    } else {

                        var controller = Target as IMyTerminalBlock;

                        if (controller != null && controller.SlimBlock.CubeGrid.Physics != null) {

                            this.TargetVelocity = controller.SlimBlock.CubeGrid.Physics.LinearVelocity;

                            if (controller.SlimBlock.CubeGrid.Physics.LinearVelocity.Length() > 0.1) {

                                this.IsMoving = true;

                            }

                        }

                    }

                }

                

            }

            //Obstruction To Target (For Weapons)
            //IsTargetObstructed(); //Broken?

            //Logger.MsgDebug("Target human control: ", DebugTypeEnum.Target);
            //IsHumanControlled
            if (TargetType == TargetTypeEnum.Block) {

                this.IsHumanControlled = TargetHelper.IsHumanControllingTarget(this.TargetBlock.SlimBlock.CubeGrid);

            } else if(TargetType == TargetTypeEnum.Grid) {

                this.IsHumanControlled = TargetHelper.IsHumanControllingTarget(this.Target as IMyCubeGrid);

            } else if(TargetType == TargetTypeEnum.Player) {

                this.IsHumanControlled = true;

            } else {

                this.IsHumanControlled = false;

            }

            //Logger.MsgDebug("Target powered: ", DebugTypeEnum.Target);
            //IsPowered
            if (TargetType == TargetTypeEnum.Block) {

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

            //Logger.MsgDebug("Target shielded: ", DebugTypeEnum.Target);
            //IsShielded
            if (RAI_SessionCore.Instance.ShieldApiLoaded == true) {

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

            //Logger.MsgDebug("Target eval end: ", DebugTypeEnum.Target);

        }

        public void IsTargetObstructed() {


            
        }

        public bool IsTargetReachable() {

            return false;

        }

        public void SetTargetsNull(string reason = "") {

            this.Target = null;
            this.TargetBlock = null;
            this.TargetPlayer = null;
            this.TargetExists = false;
            Logger.MsgDebug("Target Evaluation Fail: " + reason, DebugTypeEnum.Target);

        }

    }

}
