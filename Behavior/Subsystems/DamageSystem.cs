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
        public IMyRemoteControl RemoteControl;
        public IMyCubeGrid CurrentCubeGrid;
        public List<IMyCubeGrid> CurrentGrids;
        public DateTime LastDamageEvent;

        public DamageSystem(IMyRemoteControl remoteControl = null) {

            UseDamageDetection = false;
            DamageDetectionCooldown = 5;
            DamagedAction = DamageReaction.None;

            RemoteControl = null;
            CurrentGrids = new List<IMyCubeGrid>();
            LastDamageEvent = MyAPIGateway.Session.GameDateTime;

            Setup(remoteControl);

        }

        private void Setup(IMyRemoteControl remoteControl) {

            if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false) {

                return;

            }

            this.RemoteControl = remoteControl;

        }

        public void SetupDamageHandler() {

            if(this.UseDamageDetection == true) {

                return;

            }

            this.UseDamageDetection = true;
            this.CurrentCubeGrid = this.RemoteControl.SlimBlock.CubeGrid;
            this.CurrentCubeGrid.OnGridSplit += GridSplit;

        }

        public void DamageHandler(object target, MyDamageInformation info) {



        }

        public void UnregisterDamageHandler() {

            DamageHelper.MonitoredGrids.Remove(this.CurrentCubeGrid);

            if(this.CurrentCubeGrid != null && MyAPIGateway.Entities.Exist(this.CurrentCubeGrid) == true) {

                this.CurrentCubeGrid.OnGridSplit -= GridSplit;

            }

        }

        public void GridSplit(IMyCubeGrid gridA, IMyCubeGrid gridB) {

            UnregisterGridOnWatcher();

            if(this.RemoteControl?.SlimBlock?.CubeGrid == null) {

                return;

            }

            this.CurrentCubeGrid = this.RemoteControl.SlimBlock.CubeGrid;
            RegisterGridOnWatcher(this.CurrentCubeGrid);

        }

        public void RegisterGridOnWatcher(IMyCubeGrid cubeGrid) {

            if(cubeGrid == null || MyAPIGateway.Entities.Exist(cubeGrid) == false) {

                return;

            }

            this.CurrentGrids.Clear();
            this.CurrentGrids = MyAPIGateway.GridGroups.GetGroup(cubeGrid, GridLinkTypeEnum.Mechanical);

            foreach(var grid in this.CurrentGrids) {

                grid.OnGridSplit += GridSplit;
                DamageHelper.MonitoredGrids.Add(grid);

                if(DamageHelper.RegisteredDamageHandlers.ContainsKey(grid) == false) {

                    DamageHelper.RegisteredDamageHandlers.Add(grid, new Action<object, MyDamageInformation>(DamageHandler));

                }

            }

        }

        public void UnregisterGridOnWatcher() {

            foreach(var grid in this.CurrentGrids) {

                if(grid == null || MyAPIGateway.Entities.Exist(grid) == false) {

                    return;

                }

                grid.OnGridSplit -= GridSplit;
                DamageHelper.MonitoredGrids.Remove(grid);
                DamageHelper.RegisteredDamageHandlers.Remove(grid);

            }

        }

        public void InitTags() {



        }

    }


}
