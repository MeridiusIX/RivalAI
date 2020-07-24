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
using RivalAI.Helpers;

namespace RivalAI.Behavior.Subsystems.AutoPilot {

    public class ThrusterProfile {

        public IMyThrust Block;
        public IMyCubeGrid Grid;
        private Base6Directions.Direction _realDirection;
        private Base6Directions.Direction _activeDirection;

        public bool AxisEnabled;
        public bool DirectionEnabled;
        public float CurrentOverride;

        public IBehavior Behavior;
        //private bool _gridSplitCheck;
        private bool _valid;
        private bool _working;

        public ThrusterProfile(IMyThrust thrust, IMyRemoteControl remoteControl, IBehavior behavior) {

            _valid = true;
            Block = thrust;

            AxisEnabled = false;

            CurrentOverride = 0;
            DirectionEnabled = false;
            Behavior = behavior;

            if (thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Backward) {

                _realDirection = Base6Directions.Direction.Forward;

            }

            if (thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Forward) {

                _realDirection = Base6Directions.Direction.Backward;

            }

            if (thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Down) {

                _realDirection = Base6Directions.Direction.Up;

            }

            if (thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Up) {

                _realDirection = Base6Directions.Direction.Down;

            }

            if (thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Right) {

                _realDirection = Base6Directions.Direction.Left;

            }

            if (thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Left) {

                _realDirection = Base6Directions.Direction.Right;

            }

            _activeDirection = _realDirection;

            Grid = remoteControl.SlimBlock.CubeGrid;
            Block.OnClosing += CloseEntity;
            Block.IsWorkingChanged += WorkingChange;
            WorkingChange(Block);

        }

        //New
        public void SetBaseDirection(MyBlockOrientation orientation) {

            _activeDirection = orientation.TransformDirection(_realDirection);

        }

        public double GetEffectiveThrust(Base6Directions.Direction direction) {

            if (direction != _activeDirection || !_working || !ValidCheck())
                return 0;

            return Block.MaxEffectiveThrust;
        
        }

        //New
        public void ApplyThrust(ThrustAction action) {

            if (!_working)
                return;

            if (_activeDirection == Base6Directions.Direction.Left || _activeDirection == Base6Directions.Direction.Right) {

                bool direction = (_activeDirection == Base6Directions.Direction.Left && action.InvertX) || (_activeDirection == Base6Directions.Direction.Right && !action.InvertX);
                UpdateThrusterBlock(action.ControlX, direction, action.StrengthX);
                return;
            
            }

            if (_activeDirection == Base6Directions.Direction.Up || _activeDirection == Base6Directions.Direction.Down) {

                bool direction = (_activeDirection == Base6Directions.Direction.Down && action.InvertY) || (_activeDirection == Base6Directions.Direction.Up && !action.InvertY);
                UpdateThrusterBlock(action.ControlY, direction, action.StrengthY);
                return;

            }

            if (_activeDirection == Base6Directions.Direction.Forward || _activeDirection == Base6Directions.Direction.Backward) {

                bool direction = (_activeDirection == Base6Directions.Direction.Backward && action.InvertZ) || (_activeDirection == Base6Directions.Direction.Forward && !action.InvertZ);
                UpdateThrusterBlock(action.ControlZ, direction, action.StrengthZ);
                return;

            }

        }
        
        //New
        public void UpdateThrusterBlock(bool axisEnabled, bool directionEnabled, float overrideAmount) {

            if (!ValidCheck())
                return;

            if (AxisEnabled == axisEnabled && DirectionEnabled == directionEnabled && CurrentOverride == overrideAmount)
                return;

            if (!_working) {

                return;
            
            }

            AxisEnabled = axisEnabled;
            DirectionEnabled = directionEnabled;
            CurrentOverride = overrideAmount;

            if (AxisEnabled) {

                if (DirectionEnabled) {

                    Block.ThrustOverridePercentage = CurrentOverride;

                } else {

                    Block.ThrustOverridePercentage = 0.0001f;

                }

            } else {

                Block.ThrustOverridePercentage = 0;

            }

        }

        public bool ValidCheck() {

            if (!_valid)
                return false;

            if (Block == null || Block.MarkedForClose) {

                Logger.MsgDebug("Removed Thrust - Block Null or Closed", DebugTypeEnum.Thrust);
                _valid = false;
                return false;

            }

            if (Grid == null || Grid.MarkedForClose || Grid != Block.SlimBlock.CubeGrid) {

                _valid = false;
                return false;

            }

            return true;
        
        }

        private void WorkingChange(IMyCubeBlock cubeBlock) {

            _working = Block.IsWorking && Block.IsFunctional;

        }

        private void CloseEntity(IMyEntity entity) {

            Logger.MsgDebug("Removed Thrust - Block Closed", DebugTypeEnum.Thrust);
            _valid = false;
            Unload();

        }

        private void Unload() {

            if (Block == null)
                return;

            Block.OnClosing -= CloseEntity;
            Block.IsWorkingChanged -= WorkingChange;

        }

    }

}