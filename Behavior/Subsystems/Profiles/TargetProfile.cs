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
        
        public void InitTags(string customData) {

            if(string.IsNullOrWhiteSpace(customData) == false) {

                var descSplit = customData.Split('\n');

                foreach(var tag in descSplit) {

                    //UseCustomTargeting
                    if(tag.Contains("[UseCustomTargeting:") == true) {

                        this.UseCustomTargeting = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //BlockTargets
                    if(tag.Contains("[BlockTargets:") == true) {

                        var tempValue = TagHelper.TagBlockTargetTypesCheck(tag);

                        if(this.BlockTargets.HasFlag(tempValue) == false) {

                            this.BlockTargets |= tempValue;

                        }

                    }
                    
                    //Distance
                    if(tag.Contains("[Distance:") == true) {

                        this.Distance = TagHelper.TagTargetDistanceEnumCheck(tag);

                    }
                    
                    //Filters
                    if(tag.Contains("[Filters:") == true) {

                        var tempValue = TagHelper.TagTargetFilterEnumCheck(tag);

                        if(this.Filters.HasFlag(tempValue) == false) {

                            this.Filters |= tempValue;

                        }

                    }
                    
                    //Owners
                    if(tag.Contains("[Owners:") == true) {

                        var tempValue = TagHelper.TagTargetOwnerEnumCheck(tag);

                        if(this.Owners.HasFlag(tempValue) == false) {

                            this.Owners |= tempValue;

                        }

                    }
                    
                    //Relations
                    if(tag.Contains("[Relations:") == true) {

                        var tempValue = TagHelper.TagTargetRelationEnumCheck(tag);

                        if(this.Relations.HasFlag(tempValue) == false) {

                            this.Relations |= tempValue;

                        }

                    }
                    
                    //Target
                    if(tag.Contains("[Target:") == true) {

                        this.Target = TagHelper.TagTargetTypeEnumCheck(tag);

                    }
                    
                    //UseTimeout
                    if(tag.Contains("[UseTimeout:") == true) {

                        this.UseTimeout = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //MinTimeout
                    if(tag.Contains("[MinTimeout:") == true) {

                        this.MinTimeout = TagHelper.TagIntCheck(tag, this.MinTimeout);

                    }
                    
                    //MaxTimeout
                    if(tag.Contains("[MaxTimeout:") == true) {

                        this.MaxTimeout = TagHelper.TagIntCheck(tag, this.MaxTimeout);

                    }
                    
                    //NonBroadcastingMaxDistance
                    if(tag.Contains("[NonBroadcastingMaxDistance:") == true) {

                        this.NonBroadcastingMaxDistance = TagHelper.TagDoubleCheck(tag, this.NonBroadcastingMaxDistance);

                    }
                    
                    //MaxDistance
                    if(tag.Contains("[MaxDistance:") == true) {

                        this.MaxDistance = TagHelper.TagDoubleCheck(tag, this.MaxDistance);

                    }
                    
                    //UseProjectileLead
                    if(tag.Contains("[UseProjectileLead:") == true) {

                        this.UseProjectileLead = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //UseCollisionLead
                    if(tag.Contains("[UseCollisionLead:") == true) {

                        this.UseCollisionLead = TagHelper.TagBoolCheck(tag);

                    }
                    
                }

            }

            if(MinTimeout > MaxTimeout) {

                MinTimeout = MaxTimeout + 1;

            }

        }

    }
}
