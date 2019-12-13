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
using RivalAI.Behavior.Settings;
using RivalAI.Helpers;

namespace RivalAI.Behavior.Subsystems.Profiles {

    [ProtoContract]
    public class TargetProfile {

        [ProtoMember(1)]
        public bool UseCustomTargeting;

        [ProtoMember(2)]
        public BlockTargetTypes BlockTargets;

        [ProtoMember(3)]
        public TargetDistanceEnum Distance;

        [ProtoMember(4)]
        public TargetFilterEnum Filters;

        [ProtoMember(5)]
        public TargetOwnerEnum Owners;

        [ProtoMember(6)]
        public TargetRelationEnum Relations;

        [ProtoMember(7)]
        public TargetTypeEnum Target;

        [ProtoMember(8)]
        public bool UseTimeout;

        [ProtoMember(9)]
        public int MinTimeout;

        [ProtoMember(10)]
        public int MaxTimeout;

        [ProtoMember(11)]
        public double NonBroadcastingMaxDistance;

        [ProtoMember(12)]
        public double MaxDistance;

        [ProtoMember(13)]
        public bool UseProjectileLead;

        [ProtoMember(14)]
        public bool UseCollisionLead;

        public TargetProfile() {

            UseCustomTargeting = false;
            BlockTargets = BlockTargetTypes.None;
            Distance = TargetDistanceEnum.Closest;
            Filters = TargetFilterEnum.None;
            Owners = TargetOwnerEnum.None;
            Relations = TargetRelationEnum.None;
            Target = TargetTypeEnum.None;
            UseTimeout = false;
            MinTimeout = 0;
            MaxTimeout = 1;
            NonBroadcastingMaxDistance = 3000;
            MaxDistance = 12000;
            UseProjectileLead = false;
            UseCollisionLead = false;

        }

    }
}
