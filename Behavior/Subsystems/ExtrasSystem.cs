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

    public class ExtrasSystem {

        public bool UseAssertiveAntennas;
        public List<string> AssertiveAntennaNames;
        public float AssertiveAntennaIdleDistance;
        public int AssertiveAntennaIdleMinTime;
        public int AssertiveAntennaIdleMaxTime;
        public float AssertiveAntennaEngageDistance;
        public int AssertiveAntennaEngageMinTime;
        public int AssertiveAntennaEngageMaxTime;

        public IMyRemoteControl RemoteControl;

        public List<IMyRadioAntenna> AssertiveAntennaList;
        public bool GotAntennas;
        public bool AssertiveEngage;
        public DateTime LastAssertiveActivation;
        public int TimeUntilAssertiveActivation;

        public Random Rnd;

        public ExtrasSystem(IMyRemoteControl remoteControl = null) {

            UseAssertiveAntennas = false;
            AssertiveAntennaNames = new List<string>();
            AssertiveAntennaIdleDistance = 1000;
            AssertiveAntennaIdleMinTime = 600;
            AssertiveAntennaIdleMaxTime = 900;
            AssertiveAntennaEngageDistance = 15000;
            AssertiveAntennaEngageMinTime = 120;
            AssertiveAntennaEngageMaxTime = 300;

            RemoteControl = null;

            AssertiveAntennaList = new List<IMyRadioAntenna>();
            GotAntennas = false;
            AssertiveEngage = false;
            LastAssertiveActivation = MyAPIGateway.Session.GameDateTime;
            TimeUntilAssertiveActivation = 0;

            Rnd = new Random();

            Setup(remoteControl);


        }

        private void Setup(IMyRemoteControl remoteControl) {

            if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false) {

                return;

            }

            this.RemoteControl = remoteControl;

        }

        public void InitTags() {

            if(string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == false) {

                var descSplit = this.RemoteControl.CustomData.Split('\n');

                foreach(var tag in descSplit) {

                    //UseAssertiveAntennas
                    if(tag.Contains("[UseAssertiveAntennas:") == true) {

                        this.UseAssertiveAntennas = TagHelper.TagBoolCheck(tag);

                    }

                    //AssertiveAntennaNames
                    if(tag.Contains("[AssertiveAntennaNames:") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);
                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            this.AssertiveAntennaNames.Add(tempValue);

                        }

                    }

                    //AssertiveAntennaIdleDistance
                    if(tag.Contains("[AssertiveAntennaIdleDistance:") == true) {

                        this.AssertiveAntennaIdleDistance = TagHelper.TagFloatCheck(tag, this.AssertiveAntennaIdleDistance);

                    }

                    //AssertiveAntennaIdleMinTime
                    if(tag.Contains("[AssertiveAntennaIdleMinTime:") == true) {

                        this.AssertiveAntennaIdleMinTime = TagHelper.TagIntCheck(tag, this.AssertiveAntennaIdleMinTime);

                    }

                    //AssertiveAntennaIdleMaxTime
                    if(tag.Contains("[AssertiveAntennaIdleMaxTime:") == true) {

                        this.AssertiveAntennaIdleMaxTime = TagHelper.TagIntCheck(tag, this.AssertiveAntennaIdleMaxTime);

                    }

                    //AssertiveAntennaEngageDistance
                    if(tag.Contains("[AssertiveAntennaEngageDistance:") == true) {

                        this.AssertiveAntennaEngageDistance = TagHelper.TagFloatCheck(tag, this.AssertiveAntennaEngageDistance);

                    }

                    //AssertiveAntennaEngageMinTime
                    if(tag.Contains("[AssertiveAntennaEngageMinTime:") == true) {

                        this.AssertiveAntennaEngageMinTime = TagHelper.TagIntCheck(tag, this.AssertiveAntennaEngageMinTime);

                    }

                    //AssertiveAntennaEngageMaxTime
                    if(tag.Contains("[AssertiveAntennaEngageMaxTime:") == true) {

                        this.AssertiveAntennaEngageMaxTime = TagHelper.TagIntCheck(tag, this.AssertiveAntennaEngageMaxTime);

                    }

                }

            }

            PostSetup();

        }

        public void PostSetup() {

            TimeUntilAssertiveActivation = Rnd.Next(this.AssertiveAntennaIdleMinTime, this.AssertiveAntennaIdleMaxTime);

        }

        public void RunAssertiveAntennaTimer() {

            if(this.UseAssertiveAntennas == false) {

                return;

            }

            if(this.GotAntennas == false) {

                GetAssertiveAntennas();

            }

            TimeSpan duration = MyAPIGateway.Session.GameDateTime - this.LastAssertiveActivation;

            if(duration.TotalSeconds >= TimeUntilAssertiveActivation) {

                if(this.AssertiveEngage == false) {

                    TimeUntilAssertiveActivation = Rnd.Next(this.AssertiveAntennaEngageMinTime, this.AssertiveAntennaEngageMaxTime);
                    SetAssertiveAntennas(true);

                } else {

                    TimeUntilAssertiveActivation = Rnd.Next(this.AssertiveAntennaIdleMinTime, this.AssertiveAntennaIdleMaxTime);
                    SetAssertiveAntennas(false);

                }

                this.LastAssertiveActivation = MyAPIGateway.Session.GameDateTime;

            }

        }

        public void SetAssertiveAntennas(bool engage) {

            if(this.UseAssertiveAntennas == false) {

                return;

            }

            if(this.GotAntennas == false) {

                GetAssertiveAntennas();

            }

            foreach(var antenna in this.AssertiveAntennaList) {

                if(antenna?.SlimBlock != null) {

                    if(engage == true) {

                        antenna.Radius = this.AssertiveAntennaEngageDistance;

                    } else {

                        antenna.Radius = this.AssertiveAntennaIdleDistance;

                    }

                }

            }

            this.AssertiveEngage = engage;

        }

        public void GetAssertiveAntennas() {

            if(this.AssertiveAntennaList.Count == 0) {

                this.AssertiveAntennaList = BlockHelper.GetGridAntennas(this.RemoteControl.SlimBlock.CubeGrid);

                if(this.AssertiveAntennaList.Count > 0) {

                    for(int i = this.AssertiveAntennaList.Count - 1;i >= 0;i--) {

                        var ant = this.AssertiveAntennaList[i];

                        if(ant?.SlimBlock == null) {

                            this.AssertiveAntennaList.RemoveAt(i);
                            continue;

                        }

                        if(ant.IsFunctional == false || ant.IsWorking == false || ant.IsBroadcasting == false) {

                            this.AssertiveAntennaList.RemoveAt(i);
                            continue;

                        }

                        if(this.AssertiveAntennaNames.Count > 0 && this.AssertiveAntennaNames.Contains(ant.CustomName) == false) {

                            this.AssertiveAntennaList.RemoveAt(i);
                            continue;

                        }

                    }

                    if(this.AssertiveAntennaList.Count == 0) {

                        this.UseAssertiveAntennas = false;
                        return;

                    }

                }

            }

        }

    }


}
