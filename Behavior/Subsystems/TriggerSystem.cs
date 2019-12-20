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
using RivalAI.Helpers;
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Behavior.Subsystems {

    public class TriggerSystem{

        public IMyRemoteControl RemoteControl;
        public List<IMyRadioAntenna> AntennaList = new List<IMyRadioAntenna>();
        public List<IMyLargeTurretBase> TurretList = new List<IMyLargeTurretBase>();

        private AutoPilotSystem _autopilot;
        private BroadcastSystem _broadcast;
        private DespawnSystem _despawn;
        private ExtrasSystem _extras;
        private OwnerSystem _owner;
        private TargetingSystem _targeting;
        private WeaponsSystem _weapons;

        public List<TriggerProfile> Triggers;
        public List<TriggerProfile> DamageTriggers;
        public List<TriggerProfile> CommandTriggers;
        public string CommandReceiveCode;

        public bool DamageHandlerRegistered;
        public MyDamageInformation DamageInfo;
        public bool PendingDamage;


        public TriggerSystem(IMyRemoteControl remoteControl){

            RemoteControl = null;
            AntennaList = new List<IMyRadioAntenna>();
            TurretList = new List<IMyLargeTurretBase>();

            Triggers = new List<TriggerProfile>();
            DamageTriggers = new List<TriggerProfile>();
            CommandTriggers = new List<TriggerProfile>();
            CommandReceiveCode = "";

            DamageHandlerRegistered = false;
            DamageInfo = new MyDamageInformation();
            PendingDamage = false;

            Setup(remoteControl);

        }

        public void ProcessTriggerWatchers() {

            MyAPIGateway.Parallel.Start(() => {

                for(int i = 0;i < this.Triggers.Count;i++) {

                    var trigger = this.Triggers[i];

                    //Damage
                    if(trigger.Type == "Damage") {

                        if(trigger.UseTrigger == true && this.PendingDamage == true) {

                            //TODO: Get Damage Data and Do Final If Check If Damage Type Matches
                            trigger.ActivateTrigger();

                        }

                        continue;

                    }

                    //PlayerNear
                    if(trigger.Type == "PlayerNear") {

                        if(trigger.UseTrigger == true) {

                            if(IsPlayerNearby(trigger)) {

                                trigger.ActivateTrigger();

                            }

                        }

                        continue;

                    }

                    //TurretTarget
                    if(trigger.Type == "TurretTarget") {

                        if(trigger.UseTrigger == true) {

                            _weapons.TurretTarget = null;

                            foreach(var turret in _weapons.Turrets) {

                                if(turret == null) {

                                    continue;

                                }

                                if(turret.Target != null) {

                                    _weapons.TurretTarget = turret.Target;
                                    break;

                                }

                            }

                            if(_weapons.TurretTarget != null) {

                                trigger.ActivateTrigger();

                                if(trigger.Triggered == true) {

                                    trigger.DetectedEntityId = _weapons.TurretTarget.EntityId;

                                }

                            }

                        }

                        continue;

                    }

                    //NoWeapon
                    if(trigger.Type == "NoWeapon") {

                        if(trigger.UseTrigger == true) {

                            bool validWeapon = false;

                            foreach(var weapon in _weapons.AllWeapons) {

                                if(weapon == null) {

                                    continue;

                                }

                                if(weapon.IsFunctional == false || weapon.IsWorking == false) {

                                    continue;

                                }

                                if(weapon.GetInventory().Empty() == true) {

                                    continue;

                                }

                                validWeapon = true;
                                break;

                            }

                            if(validWeapon == false) {

                                trigger.ActivateTrigger();

                            }

                        }

                        continue;

                    }

                    //TargetInSafezone
                    if(trigger.Type == "TargetInSafezone") {

                        if(trigger.UseTrigger == true) {

                            if(_targeting.Target.TargetExists == true && _targeting.Target.InSafeZone == true) {

                                trigger.ActivateTrigger();

                            }

                        }

                        continue;

                    }

                    //Grounded
                    if(trigger.Type == "Grounded") {

                        if(trigger.UseTrigger == true) {

                            //Check if Grounded
                            trigger.ActivateTrigger();

                        }

                        continue;

                    }

                    //CommandReceive
                    if(trigger.Type == "CommandReceive") {

                        if(trigger.UseTrigger == true) {

                            //Check if Command Received
                            //Set Received Target if Eligible
                            trigger.ActivateTrigger();

                        }

                        continue;

                    }

                }

            }, () => {

                MyAPIGateway.Utilities.InvokeOnGameThread(() => {

                    for(int i = 0;i < this.Triggers.Count;i++) {

                        ProcessTrigger(this.Triggers[i]);

                    }

                    ResetDamage();

                });

            });


        }

        public void ProcessDamageTriggerWatchers(object target, MyDamageInformation info) {

            for(int i = 0;i < this.DamageTriggers.Count;i++) {

                var trigger = this.DamageTriggers[i];

                if(trigger.DamageTypes.Contains(info.Type.ToString())) {

                    if(trigger.UseTrigger == true) {

                        trigger.ActivateTrigger();

                        if(trigger.Triggered == true) {

                            ProcessTrigger(trigger, info.AttackerId);

                        }

                    }

                }

            }

        }

        public void ProcessTrigger(TriggerProfile trigger, long attackerEntityId = 0) {

            if(trigger.Triggered == false || trigger.Actions == null) {

                return;

            }

            trigger.Triggered = false;
            trigger.CooldownTime = trigger.Rnd.Next((int)trigger.MinCooldownMs, (int)trigger.MaxCooldownMs);
            trigger.LastTriggerTime = MyAPIGateway.Session.GameDateTime;
            trigger.TriggerCount++;

            //ChatBroadcast
            if(trigger.Actions.UseChatBroadcast == true) {

                _broadcast.BroadcastRequest(trigger.ChatMessage);

            }

            //BarrellRoll
            if(trigger.Actions.BarrelRoll == true) {

                _autopilot.ChangeAutoPilotMode(AutoPilotMode.BarrelRoll);

            }

            //DamageAttacker
            if(trigger.Actions.DamageToolAttacker == true && attackerEntityId == 0) {

                //

            }

            //Retreat
            if(trigger.Actions.Retreat == true) {

                _despawn.Retreat();

            }

            //SelfDestruct
            if(trigger.Actions.SelfDestruct == true) {

                var blockList = TargetHelper.GetAllBlocks(RemoteControl.SlimBlock.CubeGrid);

                foreach(var block in blockList.Where(x => x.FatBlock != null)) {

                    var tBlock = block as IMyWarhead;

                    if(tBlock != null) {

                        tBlock.IsArmed = true;
                        tBlock.Detonate();

                    }

                }

            }

            //SpawnReinforcements
            if(trigger.Actions.SpawnEncounter == true) {

                

            }

            //Strafe
            if(trigger.Actions.Strafe == true) {



            }

            //SwitchToReceivedTarget
            if(trigger.Actions.SwitchToReceivedTarget == true) {



            }

            //SwitchToDamagerTarget
            if(trigger.Actions.SwitchToDamagerTarget == true) {



            }

            //SwitchToBehavior
            if(trigger.Actions.SwitchToBehavior == true) {



            }

            //TriggerTimerBlock
            if(trigger.Actions.TriggerTimerBlocks == true) {

                var blockList = BlockHelper.GetBlocksWithNames(RemoteControl.SlimBlock.CubeGrid, trigger.TimerNames);

                foreach(var block in blockList) {

                    var tBlock = block as IMyTimerBlock;

                    if(tBlock != null) {

                        tBlock.Trigger();

                    }

                }

            }

            //RefreshTarget
            if(trigger.Actions.RefreshTarget == true) {

                _targeting.InvalidTarget = true;

            }

            //ActivateAssertiveAntennas
            if(trigger.Actions.ActivateAssertiveAntennas == true) {

                _extras.SetAssertiveAntennas(true);
                _extras.AssertiveEngage = true;

            }

        }

        public bool IsPlayerNearby(TriggerProfile control) {

            IMyPlayer player = null;

            if(control.MinPlayerReputation != -1501 || control.MaxPlayerReputation != 1501) {

                player = TargetHelper.GetClosestPlayerWithReputation(this.RemoteControl.GetPosition(), _owner.FactionId, control.MinPlayerReputation, control.MaxPlayerReputation);

            } else {

                player = TargetHelper.GetClosestPlayer(this.RemoteControl.GetPosition());

            }

            if(player == null) {

                return false;

            }

            var playerDist = Vector3D.Distance(player.GetPosition(), this.RemoteControl.GetPosition());

            if(playerDist > control.TargetDistance) {

                return false;

            }

            if(control.InsideAntenna == true) {

                var antenna = BlockHelper.GetAntennaWithHighestRange(this.AntennaList);

                if(antenna != null) {

                    playerDist = Vector3D.Distance(player.GetPosition(), antenna.GetPosition());
                    if(playerDist > antenna.Radius) {

                        return false;

                    }

                } else {

                    return false;

                }

            }

            return true;

        }

        public void Setup(IMyRemoteControl remoteControl) {

            if(remoteControl?.SlimBlock == null) {

                return;

            }

            this.RemoteControl = remoteControl;

            this.AntennaList = BlockHelper.GetGridAntennas(this.RemoteControl.SlimBlock.CubeGrid);

        }

        public void SetupReferences(AutoPilotSystem autopilot, BroadcastSystem broadcast, DespawnSystem despawn, ExtrasSystem extras, TargetingSystem targeting, WeaponsSystem weapons) {

            this._autopilot = autopilot;
            this._broadcast = broadcast;
            this._despawn = despawn;
            this._extras = extras;
            this._targeting = targeting;
            this._weapons = weapons;

        }
        
        public void RegisterCommandListener(){
        
        
        
        }

        public void InitTags() {

            //TODO: Try To Get Triggers From Block Storage At Start

            //Start With This Class
            if(string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == true) {

                return;

            }

            var descSplit = this.RemoteControl.CustomData.Split('\n');

            foreach(var tag in descSplit) {

                //Triggers
                if(tag.Contains("[Triggers:") == true) {

                    var tempValue = TagHelper.TagStringCheck(tag);

                    if(string.IsNullOrWhiteSpace(tempValue) == false) {

                        byte[] byteData = { };

                        if(TagHelper.TriggerObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

                            try {

                                var profile = MyAPIGateway.Utilities.SerializeFromBinary<TriggerProfile>(byteData);

                                if(profile != null) {

                                    if(profile.Type == "Damage") {

                                        this.DamageTriggers.Add(profile);
                                        continue;

                                    }
                                    
                                    if(profile.Type == "CommandReceived"){

                                        this.CommandTriggers.Add(profile);
                                        RegisterCommandListener();
                                        continue;

                                    }
                                    
                                    this.Triggers.Add(profile);
                                    
                                }

                            } catch(Exception) {



                            }

                        }

                    }

                }

            }

        }

    }

}
