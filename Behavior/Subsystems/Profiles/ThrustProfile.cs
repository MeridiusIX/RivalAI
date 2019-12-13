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

namespace RivalAI.Behavior.Subsystems.Profiles {

    public class ThrustProfile {

        public IMyThrust ThrustBlock;
        public Vector3I Direction;

        public float CurrentOverride;

        public ThrustProfile(IMyThrust thrust, IMyRemoteControl remoteControl) {

            ThrustBlock = thrust;
            CurrentOverride = 0;
            ThrustBlock.ThrustOverridePercentage = 0;

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Backward) {

                Direction = new Vector3I(0, 0, 1);

            }

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Forward) {

                Direction = new Vector3I(0, 0, -1);

            }

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Down) {

                Direction = new Vector3I(0, 1, 0);

            }

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Up) {

                Direction = new Vector3I(0, -1, 0);

            }

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Right) {

                Direction = new Vector3I(-1, 0, 0);

            }

            if(thrust.WorldMatrix.Forward == remoteControl.WorldMatrix.Left) {

                Direction = new Vector3I(1, 0, 0);

            }

        }

        public void UpdateThrust(Vector3I allowedUpdates, Vector3I requiredUpdates) {

            //Left/Right
            if(Direction.X != 0) {

                if(allowedUpdates.X != 0) {

                    if(requiredUpdates.X == Direction.X) {

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
            if(Direction.Y != 0) {

                if(allowedUpdates.Y != 0) {

                    if(requiredUpdates.Y == Direction.Y) {

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
            if(Direction.Z != 0) {

                if(allowedUpdates.Z != 0) {

                    if(requiredUpdates.Z == Direction.Z) {

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


    }

}