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

namespace RivalAI.Sync {

    public enum SyncMode {

        None,
        BehaviorChange,
        Effect,
        ChatCommand,

    }

    [ProtoContract]
    public class SyncContainer {

        [ProtoMember(1)]
        public SyncMode Mode;

        [ProtoMember(2)]
        public byte[] Data;

        public SyncContainer() {

            Mode = SyncMode.None;
            Data = new byte[0];

        }

        public SyncContainer(SyncMode mode, byte[] data) {

            Mode = mode;
            Data = data;

        }

    }

}
