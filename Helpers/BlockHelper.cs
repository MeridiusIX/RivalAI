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

namespace RivalAI.Helpers {

    public static class BlockHelper {

        public static IMyTerminalBlock GetBlockWithName(IMyCubeGrid cubeGrid, string name) {

            if(string.IsNullOrWhiteSpace(name) == true) {

                return null;

            }

            var blockList = TargetHelper.GetAllBlocks(cubeGrid);

            foreach(var block in blockList.Where(x => x.FatBlock != null)) {

                if((block as IMyTerminalBlock) == null) {

                    continue;

                }

                if((block as IMyTerminalBlock).CustomName == name) {

                    return block as IMyTerminalBlock;

                }

            }

            return null;

        }

        public static List<IMyTerminalBlock> GetBlocksWithNames(IMyCubeGrid cubeGrid, List<string> names) {

            var resultList = new List<IMyTerminalBlock>();
            var blockList = TargetHelper.GetAllBlocks(cubeGrid);

            foreach(var block in blockList.Where(x => x.FatBlock != null)) {

                var tBlock = block as IMyTerminalBlock;

                if(tBlock == null) {

                    continue;

                }

                if(names.Contains(tBlock.CustomName)) {

                    resultList.Add(tBlock);

                }

            }

            return resultList;

        }

        public static List<IMyRadioAntenna> GetGridAntennas(IMyCubeGrid cubeGrid) {

            var resultList = new List<IMyRadioAntenna>();

            var blockList = TargetHelper.GetAllBlocks(cubeGrid);

            foreach(var block in blockList.Where(x => x.FatBlock != null)) {

                var antenna = block.FatBlock as IMyRadioAntenna;

                if(antenna != null) {

                    resultList.Add(antenna);

                }

            }

            return resultList;

        }

        public static IMyRadioAntenna GetAntennaWithHighestRange(List<IMyRadioAntenna> antennaList) {

            IMyRadioAntenna resultAntenna = null;
            float range = 0;

            foreach(var antenna in antennaList) {

                if(antenna == null || MyAPIGateway.Entities.Exist(antenna?.SlimBlock?.CubeGrid) == false) {

                    continue;

                }

                if(antenna.IsWorking == false || antenna.IsFunctional == false || antenna.IsBroadcasting == false) {

                    continue;

                }

                if(antenna.Radius > range) {

                    resultAntenna = antenna;
                    range = antenna.Radius;

                }

            }

            return resultAntenna;

        }
        
        public static List<IMyShipController> GetGridControllers(IMyCubeGrid cubeGrid){
        
            var resultList = new List<IMyShipController>();

            var blockList = TargetHelper.GetAllBlocks(cubeGrid);

            foreach(var block in blockList.Where(x => x.FatBlock != null)) {

                var controller = block.FatBlock as IMyShipController;

                if(controller != null) {

                    resultList.Add(antenna);

                }

            }

            return resultList;
        
        }

        public static IMyEntity TurretHasTarget(List<IMyLargeTurretBase> turretList) {

            foreach(var turret in turretList) {

                if(turret?.SlimBlock == null) {

                    continue;

                }

                if(turret.IsWorking == false || turret.IsFunctional == false || turret.HasTarget == false) {

                    continue;

                }

                return turret.Target;

            }

            return null;

        }

    }

}
