﻿using RivalAI.Helpers;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;

namespace RivalAI.Entities {
	public static class GridManager {

		public static List<GridEntity> Grids = new List<GridEntity>();
		public static Action UnloadEntities;

		public static List<MyDefinitionId> AllowedBlocks = new List<MyDefinitionId>();
		public static List<MyDefinitionId> RestrictedBlocks = new List<MyDefinitionId>();

		public static void GetBlocksFromGrid<T>(IMyCubeGrid grid, List<IMySlimBlock> blocks, bool getAttachedGrids = false) where T : class {

			lock(Grids) {

				for (int i = Grids.Count - 1; i >= 0; i--) {

					var cubeGrid = Grids[i];

					if (cubeGrid == null || !cubeGrid.ActiveEntity() || cubeGrid.CubeGrid != grid)
						continue;

					if (getAttachedGrids) {

						cubeGrid.RefreshSubGrids();

						lock (cubeGrid.LinkedGrids) {

							foreach (var linkedGrid in cubeGrid.LinkedGrids)
								GetBlocksFromGrid<T>(linkedGrid, blocks);

						}

					} else {

						GetBlocksFromGrid<T>(cubeGrid, blocks);

					}

					return;
				
				}
			
			}

			Logger.MsgDebug("Could Not Get Grid For Blocks", DebugTypeEnum.BehaviorSetup);
		
		}

		public static void GetBlocksFromGrid<T>(GridEntity grid, List<IMySlimBlock> blocks) where T : class {

			if (grid == null || !grid.ActiveEntity())
				return;

			lock (grid.AllTerminalBlocks) {

				for (int j = grid.AllTerminalBlocks.Count - 1; j >= 0; j--) {

					var block = grid.AllTerminalBlocks[j];

					if (block == null || !block.ActiveEntity())
						continue;

					if (block.Block as T != null)
						blocks.Add(block.Block.SlimBlock);

				}

			}

		}

		public static bool ProcessBlock(IMySlimBlock block) {

			if (block == null) {

				return false;

			}

			if (AllowedBlocks.Contains(block.BlockDefinition.Id)) {

				return true;

			}
				

			var grid = block.CubeGrid as MyCubeGrid;

			if (grid == null) {

				return true;

			}
				

			if (RestrictedBlocks.Contains(block.BlockDefinition.Id)) {

				grid.RazeBlock(block.Min);
				return false;

			}

			if (block.BlockDefinition.Context?.ModId == null || !block.BlockDefinition.Context.ModId.Contains(".sbm")) {

				AllowedBlocks.Add(block.BlockDefinition.Id);
				return true;
			
			}

			var idString = block.BlockDefinition.Context.ModId.Replace(".sbm", "");

			bool badResult = false;

			if (block.FatBlock == null) {

				MyCube cube = null;

				if (!grid.TryGetCube(block.Min, out cube)) {

					return true;

				}

				foreach (var part in cube.Parts) {

					IMyModel model = part.Model;

					if (model == null) {

						continue;

					}


					var dummyDict = new Dictionary<string, IMyModelDummy>();
					var count = model.GetDummies(dummyDict);

					foreach (var dummy in dummyDict.Keys) {

						if (dummy.Contains("ModEncProtSys_") && !dummy.Contains(idString)) {

							badResult = true;
							break;

						}

					}

					if (badResult)
						break;

				}

			} else {

				IMyModel model = block.FatBlock.Model;

				if (model == null) {

					return true;

				}


				var dummyDict = new Dictionary<string, IMyModelDummy>();
				var count = model.GetDummies(dummyDict);

				foreach (var dummy in dummyDict.Keys) {

					if (dummy.Contains("ModEncProtSys_") && !dummy.Contains(idString)) {

						badResult = true;
						break;

					}

				}

			}

			if (badResult) {

				RestrictedBlocks.Add(block.BlockDefinition.Id);
				grid.RazeBlock(block.Min);
				return false;

			} else {

				AllowedBlocks.Add(block.BlockDefinition.Id);
				return true;

			}
		
		}

	}
}
