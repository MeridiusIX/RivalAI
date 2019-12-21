﻿using System;
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
    public class ActionProfile {
        
        [ProtoMember(1)]
        public bool UseChatBroadcast;

        [ProtoMember(2)]
        public ChatProfile ChatData;

        [ProtoMember(3)]
        public bool BarrelRoll;

        [ProtoMember(4)]
        public bool Strafe;

        [ProtoMember(5)]
        public bool ChangeAutopilotSpeed;
        
        [ProtoMember(6)]
        public float NewAutopilotSpeed;
        
        //[ProtoMember()]
        public bool SpawnEncounter;
        
        //[ProtoMember()]
        public SpawnProfile Spawner;
        
        //[ProtoMember()]
        public bool SelfDestruct;
        
        //[ProtoMember()]
        public bool Retreat;
        
        //[ProtoMember()]
        public bool BroadcastCurrentTarget;
        
        //[ProtoMember()]
        public bool SwitchToReceivedTarget;
        
        //[ProtoMember()]
        public bool BroadcastDamagerTarget;
        
        //[ProtoMember()]
        public bool SwitchToDamagerTarget;

        //[ProtoMember()]
        public bool SwitchToBehavior;
        
        //[ProtoMember()]
        public string NewBehavior;
        
        //[ProtoMember()]
        public bool ClearSettingsOnBehaviorSwitch;
        
        //[ProtoMember()]
        public bool RefreshTarget;

        //[ProtoMember()]
        public bool SwitchTargetProfile;

        //[ProtoMember()]
        public string NewTargetProfile;

        //[ProtoMember()]
        public bool TriggerTimerBlocks;
        
        //[ProtoMember()]
        public List<string> TimerBlockNames;
        
        //[ProtoMember()]
        public bool ChangeReputationWithPlayers;
        
        //[ProtoMember()]
        public double ReputationChangeRadius;
        
        //[ProtoMember()]
        public int ReputationChangeAmount;
        
        //[ProtoMember()]
        public bool ActivateAssertiveAntennas;
        
        //[ProtoMember()]
        public bool ChangeAntennaOwnership;
        
        //[ProtoMember()]
        public string AntennaFactionOwner;

        //[ProtoMember()]
        public bool CreateKnownPlayerArea;

        //[ProtoMember()]
        public double KnownPlayerAreaRadius;

        //[ProtoMember()]
        public int KnownPlayerAreaTimer;

        //[ProtoMember()]
        public int KnownPlayerAreaMaxSpawns;

        //[ProtoMember()]
        public bool DamageToolAttacker;

        //[ProtoMember()]
        public float DamageToolAttackerAmount;

        //[ProtoMember()]
        public string DamageToolAttackerParticle;

        //[ProtoMember()]
        public string DamageToolAttackerSound;
        
        //[ProtoMember()]
        public bool PlayParticleEffectAtRemote;
        
        //[ProtoMember()]
        public string ParticleEffectId;
        
        //[ProtoMember()]
        public Vector3D ParticleEffectOffset;
        
        //[ProtoMember()]
        public float ParticleEffectScale;
        
        //[ProtoMember()]
        public float ParticleEffectMaxTime;
        
        //[ProtoMember()]
        public Vector4 ParticleEffectColor;

        //[ProtoMember()]
        public List<string> SetBooleansTrue;
        
        //[ProtoMember()]
        public List<string> SetBooleansFalse;
        
        //[ProtoMember()]
        public List<string> IncreaseCounters;
        
        //[ProtoMember()]
        public List<string> DecreaseCounters;
        
        //[ProtoMember()]
        public List<string> ResetCounters;
        
        //[ProtoMember()]
        public List<string> SetSandboxBooleansTrue;
        
        //[ProtoMember()]
        public List<string> SetSandboxBooleansFalse;
        
        //[ProtoMember()]
        public List<string> IncreaseSandboxCounters;
        
        //[ProtoMember()]
        public List<string> DecreaseSandboxCounters;
        
        //[ProtoMember()]
        public List<string> ResetSandboxCounters;

        public ActionProfile(){

            UseChatBroadcast = false;
            ChatData = new ChatProfile();

            BarrelRoll = false;
            Strafe = false;
            
            ChangeAutopilotSpeed = false;
            NewAutopilotSpeed = 0;
            
            SpawnEncounter = false;
            Spawner = new SpawnProfile();

            SelfDestruct = false;
            
            Retreat = false;
            
            SwitchToReceivedTarget = false;
            
            SwitchToDamagerTarget = false;
            
            SwitchToBehavior = false;
            NewBehavior = "";
            ClearSettingsOnBehaviorSwitch = false;
            
            RefreshTarget = false;
            
            TriggerTimerBlocks = false;
            TimerBlockNames = new List<string>();
            
            ChangeReputationWithPlayers = false;
            ReputationChangeRadius = 0;
            ReputationChangeAmount = 0;
            
            ActivateAssertiveAntennas = false;
            
            ChangeAntennaOwnership = false;
            AntennaFactionOwner = "Nobody";

            CreateKnownPlayerArea = false;
            KnownPlayerAreaRadius = 10000;
            KnownPlayerAreaTimer = 30;
            KnownPlayerAreaMaxSpawns = -1;
            
            DamageToolAttacker = false;
            DamageToolAttackerAmount = 90;
            DamageToolAttackerParticle = "";
            DamageToolAttackerSound = "";

            SetBooleansTrue = new List<string>();
            SetBooleansFalse = new List<string>();
            IncreaseCounters = new List<string>();
            DecreaseCounters = new List<string>();
            ResetCounters = new List<string>();
            
            SetSandboxBooleansTrue = new List<string>();
            SetSandboxBooleansFalse = new List<string>();
            IncreaseSandboxCounters = new List<string>();
            DecreaseSandboxCounters = new List<string>();
            ResetSandboxCounters = new List<string>();

        }

        public void InitTags(string customData) {

            if(string.IsNullOrWhiteSpace(customData) == false) {

                var descSplit = customData.Split('\n');

                foreach(var tag in descSplit) {

                    //UseChatBroadcast
                    if(tag.Contains("[UseChatBroadcast:") == true) {

                        this.UseChatBroadcast = TagHelper.TagBoolCheck(tag);

                    }

                    //ChatData
                    if(tag.Contains("[ChatData:") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            byte[] byteData = { };

                            if(TagHelper.ChatObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

                                try {

                                    var profile = MyAPIGateway.Utilities.SerializeFromBinary<ChatProfile>(byteData);

                                    if(profile != null) {

                                        ChatData = profile;

                                    }

                                } catch(Exception) {



                                }

                            }

                        }

                    }

                    //BarrelRoll
                    if(tag.Contains("[BarrelRoll:") == true) {

                        this.BarrelRoll = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //Strafe
                    if(tag.Contains("[Strafe:") == true) {

                        this.Strafe = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //ChangeAutopilotSpeed
                    if(tag.Contains("[ChangeAutopilotSpeed:") == true) {

                        this.ChangeAutopilotSpeed = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //NewAutopilotSpeed
                    if(tag.Contains("[NewAutopilotSpeed:") == true) {

                        this.NewAutopilotSpeed = TagHelper.TagFloatCheck(tag, this.NewAutopilotSpeed);

                    }
                    
                    //SpawnEncounter
                    if(tag.Contains("[SpawnEncounter:") == true) {

                        this.SpawnEncounter = TagHelper.TagBoolCheck(tag);

                    }

                    //Spawner
                    if(tag.Contains("[Spawner:") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            byte[] byteData = { };

                            if(TagHelper.SpawnerObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

                                try {

                                    var profile = MyAPIGateway.Utilities.SerializeFromBinary<SpawnProfile>(byteData);

                                    if(profile != null) {

                                        Spawner = profile;

                                    }

                                } catch(Exception) {



                                }

                            }

                        }

                    }

                    //SelfDestruct
                    if(tag.Contains("[SelfDestruct:") == true) {

                        this.SelfDestruct = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //Retreat
                    if(tag.Contains("[Retreat:") == true) {

                        this.Retreat = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //BroadcastCurrentTarget
                    if(tag.Contains("[BroadcastCurrentTarget:") == true) {

                        this.BroadcastCurrentTarget = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //SwitchToReceivedTarget
                    if(tag.Contains("[SwitchToReceivedTarget:") == true) {

                        this.SwitchToReceivedTarget = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //BroadcastDamagerTarget
                    if(tag.Contains("[BroadcastDamagerTarget:") == true) {

                        this.BroadcastDamagerTarget = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //SwitchToDamagerTarget
                    if(tag.Contains("[SwitchToDamagerTarget:") == true) {

                        this.SwitchToDamagerTarget = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //SwitchToBehavior
                    if(tag.Contains("[SwitchToBehavior:") == true) {

                        this.SwitchToBehavior = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //NewBehavior
                    if(tag.Contains("[NewBehavior:") == true) {

                        this.NewBehavior = TagHelper.TagStringCheck(tag);

                    }
                    
                    //ClearSettingsOnBehaviorSwitch
                    if(tag.Contains("[ClearSettingsOnBehaviorSwitch:") == true) {

                        this.ClearSettingsOnBehaviorSwitch = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //RefreshTarget
                    if(tag.Contains("[RefreshTarget:") == true) {

                        this.RefreshTarget = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //SwitchTargetProfile
                    if(tag.Contains("[SwitchTargetProfile:") == true) {

                        this.SwitchTargetProfile = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //NewTargetProfile
                    if(tag.Contains("[NewTargetProfile:") == true) {

                        this.NewTargetProfile = TagHelper.TagStringCheck(tag);

                    }
                    
                    //TriggerTimerBlocks
                    if(tag.Contains("[TriggerTimerBlocks:") == true) {

                        this.TriggerTimerBlocks = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //TimerBlockNames
                    if(tag.Contains("[TimerBlockNames:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.TimerBlockNames.Add(tempvalue);

                        }

                    }
                    
                    //ChangeReputationWithPlayers
                    if(tag.Contains("[ChangeReputationWithPlayers:") == true) {

                        this.ChangeReputationWithPlayers = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //ReputationChangeRadius
                    if(tag.Contains("[ReputationChangeRadius:") == true) {

                        this.ReputationChangeRadius = TagHelper.TagDoubleCheck(tag, ReputationChangeRadius);

                    }
                    
                    //ReputationChangeAmount
                    if(tag.Contains("[ReputationChangeAmount:") == true) {

                        this.ReputationChangeAmount = TagHelper.TagIntCheck(tag, ReputationChangeAmount);

                    }
                    
                    //ActivateAssertiveAntennas
                    if(tag.Contains("[ActivateAssertiveAntennas:") == true) {

                        this.ActivateAssertiveAntennas = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //ChangeAntennaOwnership
                    if(tag.Contains("[ChangeAntennaOwnership:") == true) {

                        this.ChangeAntennaOwnership = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //AntennaFactionOwner
                    if(tag.Contains("[AntennaFactionOwner:") == true) {

                        this.AntennaFactionOwner = TagHelper.TagStringCheck(tag);

                    }

                    //CreateKnownPlayerArea
                    if(tag.Contains("[CreateKnownPlayerArea:") == true) {

                        this.CreateKnownPlayerArea = TagHelper.TagBoolCheck(tag);

                    }

                    //KnownPlayerAreaRadius
                    if(tag.Contains("[KnownPlayerAreaRadius:") == true) {

                        this.KnownPlayerAreaRadius = TagHelper.TagDoubleCheck(tag, KnownPlayerAreaRadius);

                    }

                    //KnownPlayerAreaTimer
                    if(tag.Contains("[KnownPlayerAreaTimer:") == true) {

                        this.KnownPlayerAreaTimer = TagHelper.TagIntCheck(tag, KnownPlayerAreaTimer);

                    }

                    //KnownPlayerAreaMaxSpawns
                    if(tag.Contains("[KnownPlayerAreaMaxSpawns:") == true) {

                        this.KnownPlayerAreaMaxSpawns = TagHelper.TagIntCheck(tag, KnownPlayerAreaMaxSpawns);

                    }
                    
                    //DamageToolAttacker
                    if(tag.Contains("[DamageToolAttacker:") == true) {

                        this.DamageToolAttacker = TagHelper.TagBoolCheck(tag);

                    }
                    
                    //DamageToolAttackerAmount
                    if(tag.Contains("[DamageToolAttackerAmount:") == true) {

                        this.DamageToolAttackerAmount = TagHelper.TagFloatCheck(tag, this.DamageToolAttackerAmount);

                    }
                    
                    //DamageToolAttackerParticle
                    if(tag.Contains("[DamageToolAttackerParticle:") == true) {

                        this.DamageToolAttackerParticle = TagHelper.TagStringCheck(tag);

                    }
                    
                    //DamageToolAttackerSound
                    if(tag.Contains("[DamageToolAttackerSound:") == true) {

                        this.DamageToolAttackerSound = TagHelper.TagStringCheck(tag);

                    }
                    
                    //SetBooleansTrue
                    if(tag.Contains("[SetBooleansTrue:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.SetBooleansTrue.Add(tempvalue);

                        }

                    }
                    
                    //SetBooleansFalse
                    if(tag.Contains("[SetBooleansFalse:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.SetBooleansFalse.Add(tempvalue);

                        }

                    }
                    
                    //IncreaseCounters
                    if(tag.Contains("[IncreaseCounters:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.IncreaseCounters.Add(tempvalue);

                        }

                    }
                    
                    //DecreaseCounters
                    if(tag.Contains("[DecreaseCounters:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.DecreaseCounters.Add(tempvalue);

                        }

                    }
                    
                    //ResetCounters
                    if(tag.Contains("[ResetCounters:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.ResetCounters.Add(tempvalue);

                        }

                    }
                    
                    //SetSandboxBooleansTrue
                    if(tag.Contains("[SetSandboxBooleansTrue:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.SetSandboxBooleansTrue.Add(tempvalue);

                        }

                    }
                    
                    //SetSandboxBooleansFalse
                    if(tag.Contains("[SetSandboxBooleansFalse:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.SetSandboxBooleansFalse.Add(tempvalue);

                        }

                    }
                    
                    //IncreaseSandboxCounters
                    if(tag.Contains("[IncreaseSandboxCounters:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.IncreaseSandboxCounters.Add(tempvalue);

                        }

                    }
                    
                    //DecreaseSandboxCounters
                    if(tag.Contains("[DecreaseSandboxCounters:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.DecreaseSandboxCounters.Add(tempvalue);

                        }

                    }
                    
                    //ResetSandboxCounters
                    if(tag.Contains("[ResetSandboxCounters:") == true) {

                        var tempvalue = TagHelper.TagStringCheck(tag);

                        if(string.IsNullOrWhiteSpace(tempvalue) == false) {

                            this.ResetSandboxCounters.Add(tempvalue);

                        }

                    }

                }

            }

        }

    }

}
