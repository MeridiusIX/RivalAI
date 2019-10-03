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

namespace RivalAI.Behavior.Subsystems {

    [Flags]
    public enum DamageReaction {

        None = 0,
        Evasion = 1,
        ChangeTarget = 2,
        BarrelRoll = 4,
        Alert = 8,
        CallReinforcements = 16

    }

    public class DamageSystem {

        //Configurable
        public bool UseDamageDetection;
        public bool BarrelRollOnGrinderDamage;
        public int DamageDetectionCooldown;
        public DamageReaction DamagedAction;

        //Non-Configurable
        public DateTime LastDamageEvent;
        public event Action DamageChatEvent;

        public DamageSystem(IMyRemoteControl remoteControl = null) {

            UseDamageDetection = false;
            DamageDetectionCooldown = 5;
            DamagedAction = DamageReaction.None;

            LastDamageEvent = MyAPIGateway.Session.GameDateTime;

            Setup(remoteControl);

        }

        private void Setup(IMyRemoteControl remoteControl) {



        }

    }


}
