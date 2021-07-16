﻿using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;

namespace RivalAI.Helpers {

    public static class BlockHelper {

        private static Dictionary<string, long> _factionData = new Dictionary<string, long>();

        public static void ChangeBlockOwnership(IMyCubeGrid cubeGrid, List<string> blockNames, List<string> factionNames) {

            if (blockNames.Count != factionNames.Count)
                return;

            Dictionary<string, long> nameToOwner = new Dictionary<string, long>();

            for (int i = 0; i < blockNames.Count; i++) {

                if (factionNames[i] == "Nobody") {

                    if (!nameToOwner.ContainsKey(blockNames[i])) {

                        nameToOwner.Add(blockNames[i], 0);

                    }

                    continue;
                
                }

                long owner = -1;

                if (_factionData.TryGetValue(factionNames[i], out owner)) {

                    if (!nameToOwner.ContainsKey(blockNames[i])) {

                        nameToOwner.Add(blockNames[i], owner);

                    }

                    continue;

                } else {

                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(factionNames[i]);

                    if (faction != null) {

                        _factionData.Add(factionNames[i], faction.FounderId);

                        if (!nameToOwner.ContainsKey(blockNames[i])) {

                            nameToOwner.Add(blockNames[i], faction.FounderId);

                        }

                    }
                
                }
            
            }

            var blockList = GetBlocksOfType<IMyTerminalBlock>(cubeGrid);

            foreach (var block in blockList) {

                if (block.CustomName == null)
                    continue;

                long owner = -1;

                if (nameToOwner.TryGetValue(block.CustomName, out owner)) {

                    var cubeBlock = block as MyCubeBlock;
                    cubeBlock.ChangeBlockOwnerRequest(owner, cubeBlock.IDModule.ShareMode);
                
                }
            
            }

        }
		
		public static List<IMySlimBlock> GetAllBlocks(IMyCubeGrid cubeGrid) {

			List<IMySlimBlock> totalList = new List<IMySlimBlock>();
			//cubeGrid.GetBlocks(totalList);
			var gridGroup = MyAPIGateway.GridGroups.GetGroup(cubeGrid, GridLinkTypeEnum.Physical);

			foreach(var grid in gridGroup) {

				List<IMySlimBlock> blockList = new List<IMySlimBlock>();
				grid.GetBlocks(blockList);
				//blockList = new List<IMySlimBlock>(blockList.Except(totalList).ToList());
				totalList = new List<IMySlimBlock>(blockList.Concat(totalList).ToList());

			}

			return totalList;

		}

        public static IMyTerminalBlock GetBlockWithName(IMyCubeGrid cubeGrid, string name) {

            if(string.IsNullOrWhiteSpace(name) == true) {

                return null;

            }

            var blockList = TargetHelper.GetAllBlocks(cubeGrid);

            foreach(var block in blockList.Where(x => x.FatBlock != null)) {

                if((block.FatBlock as IMyTerminalBlock) == null) {

                    continue;

                }

                if((block.FatBlock as IMyTerminalBlock).CustomName == name) {

                    return block as IMyTerminalBlock;

                }

            }

            return null;

        }

        public static List<IMyTerminalBlock> GetBlocksOfType<T>(IMyCubeGrid cubeGrid) where T : class {

            var blockList = TargetHelper.GetAllBlocks(cubeGrid);
            var resultList = new List<IMyTerminalBlock>();

            foreach (IMySlimBlock block in blockList.Where(x => x.FatBlock != null)) {

                IMyTerminalBlock terminalBlock = block.FatBlock as IMyTerminalBlock;

                if (terminalBlock == null || terminalBlock as T == null)
                    continue;

                resultList.Add(terminalBlock);

            }

            return resultList;

        }

        public static List<IMyTerminalBlock> GetBlocksWithNames(IMyCubeGrid cubeGrid, List<string> names) {

            var resultList = new List<IMyTerminalBlock>();
            var blockList = TargetHelper.GetAllBlocks(cubeGrid);

            foreach(var block in blockList.Where(x => x.FatBlock != null)) {

                var tBlock = block.FatBlock as IMyTerminalBlock;

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
    
        public static List<IMyShipController> GetGridControllers(IMyCubeGrid cubeGrid){
        
            var resultList = new List<IMyShipController>();

            var blockList = TargetHelper.GetAllBlocks(cubeGrid);

            foreach(var block in blockList.Where(x => x.FatBlock != null)) {

                var controller = block.FatBlock as IMyShipController;

                if(controller != null) {

                    resultList.Add(controller);

                }

            }

            return resultList;
        
        }

        

    }

}
