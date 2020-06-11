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
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;

namespace RivalAI.Behavior.Subsystems.Profiles {

    public class ThrustProfile {

        public IMyThrust ThrustBlock;
        private Direction _direction;
        public Vector3I DirectionVector;

        public float CurrentOverride;

        public IBehavior Behavior;
        //private bool _gridSplitCheck;
        private bool _valid;
        private bool _working;

        public ThrustProfile(IMyThrust thrust, IMyRemoteControl remoteControl, IBehavior behavior) {

            _valid = true;
            ThrustBlock = thrust;
            CurrentOverride = 0;
            ThrustBlock.ThrustOverridePercentage = 0;
            Behavior = behavior;

            if (thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Backward) {

                DirectionVector = new Vector3I(0, 0, 1);
                _direction = Direction.Forward;

            }

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Forward) {

                DirectionVector = new Vector3I(0, 0, -1);
                _direction = Direction.Backward;

            }

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Down) {

                DirectionVector = new Vector3I(0, 1, 0);
                _direction = Direction.Up;

            }

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Up) {

                DirectionVector = new Vector3I(0, -1, 0);
                _direction = Direction.Down;

            }

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Right) {

                DirectionVector = new Vector3I(-1, 0, 0);
                _direction = Direction.Left;

            }

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Left) {

                DirectionVector = new Vector3I(1, 0, 0);
                _direction = Direction.Right;

            }

            //ThrustBlock.SlimBlock.CubeGrid.OnGridSplit += GridSplit;
            ThrustBlock.OnClosing += CloseEntity;
            ThrustBlock.IsWorkingChanged += WorkingChange;
            WorkingChange(ThrustBlock);

        }

        public void UpdateThrust(Vector3I allowedUpdates, Vector3I requiredUpdates) {

            //Checks

            if (!_valid)
                return;

            if (ThrustBlock == null || ThrustBlock.MarkedForClose) {

                Logger.MsgDebug("Removed Thrust - Block Null or Closed", DebugTypeEnum.Thrust);
                _valid = false;
                return;

            }

            if (!_working)
                return;

            /*
            if (_gridSplitCheck) {

                _gridSplitCheck = false;

                if (Behavior?.CurrentGrids != null && ThrustBlock?.SlimBlock?.CubeGrid != null) {

                    if (!Behavior.CurrentGrids.Contains(ThrustBlock.SlimBlock.CubeGrid)) {

                        Logger.MsgDebug("Removed Thrust - Grid No Longer Attached", DebugTypeEnum.Thrust);
                        _valid = false;
                        Unload();
                        return;

                    }

                } else {

                    Logger.MsgDebug("Removed Thrust - Grid Null", DebugTypeEnum.Thrust);
                    _valid = false;
                    Unload();
                    return;

                }

            }
            */

            //Left/Right
            if(DirectionVector.X != 0) {

                if(allowedUpdates.X != 0) {

                    if(requiredUpdates.X == DirectionVector.X) {

                        SetThrustOverride(1);

                    } else {

                        SetThrustOverride(0.01f);

                    }

                } else {

                    if(CurrentOverride != 0) {

                        SetThrustOverride(0);

                    }

                }

                return;

            }

            //Up/Down
            if(DirectionVector.Y != 0) {

                if(allowedUpdates.Y != 0) {

                    if(requiredUpdates.Y == DirectionVector.Y) {

                        SetThrustOverride(1);

                    } else {

                        SetThrustOverride(0.01f);

                    }

                } else {

                    if(CurrentOverride != 0) {

                        SetThrustOverride(0);

                    }

                }

                return;

            }

            //Forward/Backward
            if(DirectionVector.Z != 0) {

                if(allowedUpdates.Z != 0) {

                    if(requiredUpdates.Z == DirectionVector.Z) {

                        if(CurrentOverride != 1) {

                            SetThrustOverride(1);

                        }

                    } else {

                        if(CurrentOverride != 0.01f) {

                            SetThrustOverride(0.01f);

                        }

                    }

                } else {

                    if(CurrentOverride != 0) {

                        SetThrustOverride(0);

                    }

                }

                return;

            }

        }

        private void SetThrustOverride(float thrustOvr) {

            ThrustBlock.ThrustOverridePercentage = thrustOvr;
            CurrentOverride = thrustOvr;

        }

        private void GridSplit(IMyCubeGrid a, IMyCubeGrid b) {

            //_gridSplitCheck = true;
        
        }

        private void WorkingChange(IMyCubeBlock cubeBlock) {

            _working = ThrustBlock.IsWorking && ThrustBlock.IsFunctional;
        
        }

        private void CloseEntity(IMyEntity entity) {

            Logger.MsgDebug("Removed Thrust - Block Closed", DebugTypeEnum.Thrust);
            _valid = false;
            Unload();

        }

        private void Unload() {

            if (ThrustBlock == null)
                return;

            //ThrustBlock.SlimBlock.CubeGrid.OnGridSplit -= GridSplit;
            ThrustBlock.OnClosing -= CloseEntity;
            ThrustBlock.IsWorkingChanged -= WorkingChange;

        }

    }

}