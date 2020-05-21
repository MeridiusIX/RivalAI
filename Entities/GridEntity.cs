using RivalAI.Helpers;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Entities {

	

	public class GridEntity : EntityBase, ITarget{

		public IMyCubeGrid CubeGrid;
		public bool HasPhysics;

		public List<GridEntity> LinkedGrids;

		public List<BlockEntity> AllTerminalBlocks;
		public List<BlockEntity> Antennas;
		public List<BlockEntity> Beacons;
		public List<BlockEntity> Containers;
		public List<BlockEntity> Controllers;
		public List<BlockEntity> Guns;
		public List<BlockEntity> JumpDrives;
		public List<BlockEntity> Mechanical;
		public List<BlockEntity> NanoBots;
		public List<BlockEntity> Production;
		public List<BlockEntity> Power;
		public List<BlockEntity> Shields;
		public List<BlockEntity> Thrusters;
		public List<BlockEntity> Tools;
		public List<BlockEntity> Turrets;

		public Dictionary<BlockTypeEnum, List<BlockEntity>> BlockListReference;

		public float ThreatScore;
		public DateTime LastThreatCalculationTime;

		public Action UnloadEntities;

		public GridEntity(IMyEntity entity) : base(entity) {

			CubeGrid = entity as IMyCubeGrid;

			LinkedGrids = new List<GridEntity>();

			AllTerminalBlocks = new List<BlockEntity>();
			Antennas = new List<BlockEntity>();
			Beacons = new List<BlockEntity>();
			Containers = new List<BlockEntity>();
			Controllers = new List<BlockEntity>();
			Guns = new List<BlockEntity>();
			JumpDrives = new List<BlockEntity>();
			Mechanical = new List<BlockEntity>();
			NanoBots = new List<BlockEntity>();
			Production = new List<BlockEntity>();
			Power = new List<BlockEntity>();
			Shields = new List<BlockEntity>();
			Thrusters = new List<BlockEntity>();
			Tools = new List<BlockEntity>();
			Turrets = new List<BlockEntity>();

			BlockListReference = new Dictionary<BlockTypeEnum, List<BlockEntity>>();
			BlockListReference.Add(BlockTypeEnum.All, AllTerminalBlocks);
			BlockListReference.Add(BlockTypeEnum.Antennas, Antennas);
			BlockListReference.Add(BlockTypeEnum.Beacons, Beacons);
			BlockListReference.Add(BlockTypeEnum.Containers, Containers);
			BlockListReference.Add(BlockTypeEnum.Controllers, Controllers);
			BlockListReference.Add(BlockTypeEnum.Guns, Guns);
			BlockListReference.Add(BlockTypeEnum.JumpDrives, JumpDrives);
			BlockListReference.Add(BlockTypeEnum.Mechanical, Mechanical);
			BlockListReference.Add(BlockTypeEnum.NanoBots, NanoBots);
			BlockListReference.Add(BlockTypeEnum.Production, Production);
			BlockListReference.Add(BlockTypeEnum.Power, Power);
			BlockListReference.Add(BlockTypeEnum.Shields, Shields);
			BlockListReference.Add(BlockTypeEnum.Thrusters, Thrusters);
			BlockListReference.Add(BlockTypeEnum.Tools, Tools);
			BlockListReference.Add(BlockTypeEnum.Turrets, Turrets);

			if (CubeGrid.Physics == null) {

				CubeGrid.OnPhysicsChanged += PhysicsCheck;

			} else {

				HasPhysics = true;
			
			}

			if (string.IsNullOrWhiteSpace(MyVisualScriptLogicProvider.GetEntityName(CubeGrid.EntityId)))
				MyVisualScriptLogicProvider.SetName(CubeGrid.EntityId, CubeGrid.EntityId.ToString());

			var blockList = new List<IMySlimBlock>();
			CubeGrid.GetBlocks(blockList);

			foreach (var block in blockList) {

				NewBlockAdded(block);

			}

			CubeGrid.OnBlockAdded += NewBlockAdded;
			CubeGrid.OnGridSplit += GridSplit;

		}

		private void NewBlockAdded(IMySlimBlock block) {

			if (block.FatBlock == null || block.FatBlock as IMyTerminalBlock == null)
				return;

			var terminalBlock = block.FatBlock as IMyTerminalBlock;
			bool assignedBlock = false;

			//All Terminal Blocks
			if (terminalBlock as IMyTerminalBlock != null) {

				AddBlock(terminalBlock, AllTerminalBlocks);

			}

			//Antenna
			if (terminalBlock as IMyRadioAntenna != null) {

				assignedBlock = AddBlock(terminalBlock, Antennas);

			}

			//Beacon
			if (terminalBlock as IMyBeacon != null) {

				assignedBlock = AddBlock(terminalBlock, Beacons);

			}

			//Container
			if (terminalBlock as IMyCargoContainer != null) {

				assignedBlock = AddBlock(terminalBlock, Containers);

			}

			//Controller
			if (terminalBlock as IMyShipController != null) {

				if((terminalBlock as IMyShipController).CanControlShip)
					assignedBlock = AddBlock(terminalBlock, Controllers);

			}

			//JumpDrive
			if (terminalBlock as IMyJumpDrive != null) {

				assignedBlock = AddBlock(terminalBlock, JumpDrives);

			}

			//Mechanical
			if (terminalBlock as IMyMechanicalConnectionBlock != null) {

				assignedBlock = AddBlock(terminalBlock, Mechanical);

			}

			//Production
			if (terminalBlock as IMyProductionBlock != null) {

				assignedBlock = AddBlock(terminalBlock, Production);

			}

			//Power
			if (terminalBlock as IMyPowerProducer != null) {

				assignedBlock = AddBlock(terminalBlock, Power);

			}

			//Thrusters
			if (terminalBlock as IMyThrust != null) {

				assignedBlock = AddBlock(terminalBlock, Thrusters);

			}

			//Tools
			if (terminalBlock as IMyShipToolBase != null) {

				assignedBlock = AddBlock(terminalBlock, Tools);

			}

			//Weapon Sections
			if (Utilities.AllWeaponCoreBlocks.Contains(block.BlockDefinition.Id)) {

				if (Utilities.AllWeaponCoreGuns.Contains(block.BlockDefinition.Id)) {

					assignedBlock = AddBlock(terminalBlock, Guns);

				}

				if (Utilities.AllWeaponCoreTurrets.Contains(block.BlockDefinition.Id)) {

					assignedBlock = AddBlock(terminalBlock, Turrets);

				}

			} else {

				if (terminalBlock as IMyLargeTurretBase != null) {

					assignedBlock = AddBlock(terminalBlock, Turrets);

				}else if (terminalBlock as IMyUserControllableGun != null) {

					assignedBlock = AddBlock(terminalBlock, Guns);

				}

			}

			//Nanobots
			if (EntityWatcher.NanobotBlockIds.Contains(block.BlockDefinition.Id)) {

				assignedBlock = AddBlock(terminalBlock, NanoBots);

			}

			//Shields
			if (EntityWatcher.ShieldBlockIds.Contains(block.BlockDefinition.Id)) {

				assignedBlock = AddBlock(terminalBlock, Shields);

			}

			//Other
			if (!assignedBlock) {
			
				//TODO: Add To 'Other'
			
			}

		}

		private bool AddBlock(IMyTerminalBlock block, List<BlockEntity> collection) {

			var blockEntity = new BlockEntity(block, CubeGrid);

			//TODO: Add Some Validation To BlockEntity ctor for a fail case

			lock (collection) {

				collection.Add(blockEntity);

			}

			return true;
		
		}

		private void CleanBlockList(List<BlockEntity> collection) {

			if (collection == null)
				return;

			lock (collection) {

				for (int i = collection.Count - 1; i >= 0; i--) {

					var block = collection[i];

					if (block == null || block.IsClosed() || block.ParentEntity != this.ParentEntity)
						collection.RemoveAt(i);
				
				}
			
			}
		
		}

		public override void CloseEntity(IMyEntity entity) {

			base.CloseEntity(entity);
			Unload();

		}

		public void GetBlocks(List<ITarget> targetList, List<BlockTypeEnum> types) {

			if (types.Contains(BlockTypeEnum.All)) {

				foreach (var block in AllTerminalBlocks) {

					targetList.Add(block);

				}

				return;

			}

			foreach (var blockType in types) {

				if (blockType == BlockTypeEnum.None)
					continue;

				foreach (var block in BlockListReference[blockType]) {

					targetList.Add(block);

				}
			
			}
		
		}

		public void GridSplit(IMyCubeGrid gridA, IMyCubeGrid gridB) {

			CleanBlockLists();

		}

		public void CleanBlockLists() {

			try {

				CleanBlockList(AllTerminalBlocks);
				CleanBlockList(Antennas);
				CleanBlockList(Beacons);
				CleanBlockList(Containers);
				CleanBlockList(Controllers);
				CleanBlockList(Guns);
				CleanBlockList(JumpDrives);
				CleanBlockList(Mechanical);
				CleanBlockList(NanoBots);
				CleanBlockList(Production);
				CleanBlockList(Power);
				CleanBlockList(Shields);
				CleanBlockList(Thrusters);
				CleanBlockList(Tools);
				CleanBlockList(Turrets);

			} catch (Exception e) {

				Logger.WriteLog("Caught Error While Cleaning Grid Block Lists");

			}

		}

		

		public void PhysicsCheck(IMyEntity entity) {

			HasPhysics = entity.Physics != null ? true : false;

		}

		public void RefreshSubGrids() {

			LinkedGrids = EntityEvaluator.GetAttachedGrids(CubeGrid);

		}

		public override void Unload() {

			base.Unload();
			CubeGrid.OnGridSplit -= GridSplit;
			CubeGrid.OnBlockAdded -= NewBlockAdded;
			UnloadEntities?.Invoke();

		}

		//---------------------------------------------------
		//-----------Start Interface Methods-----------------
		//---------------------------------------------------

		public bool ActiveEntity() {

			if (Closed || !HasPhysics)
				return false;

			return true;

		}
		public double BroadcastRange(bool onlyAntenna = false) {

			if (!ActiveEntity())
				return 0;

			return EntityEvaluator.GridBroadcastRange(LinkedGrids);

		}

		public string FactionOwner() {

			//TODO: Build Method
			var result = "";

			if (CubeGrid?.BigOwners != null) {

				if (CubeGrid.BigOwners.Count > 0) {

					var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(CubeGrid.BigOwners[0]);

					if (faction != null) {

						return faction.Tag;
					
					}
				
				}
			
			}

			return result;
		
		}

		public List<long> GetOwners(bool onlyGetCurrentEntity = false, bool includeMinorityOwners = false) {

			var result = new List<long>();

			foreach (var grid in LinkedGrids) {

				if (!grid.ActiveEntity())
					continue;

				if (onlyGetCurrentEntity && grid.CubeGrid != CubeGrid)
					continue;

				if (grid.CubeGrid?.BigOwners != null) {

					foreach (var owner in grid.CubeGrid.BigOwners) {

						if (!result.Contains(owner))
							result.Add(owner);

					}
				
				}

				if (!includeMinorityOwners)
					continue;

				if (grid.CubeGrid?.SmallOwners != null) {

					foreach (var owner in grid.CubeGrid.SmallOwners) {

						if (!result.Contains(owner))
							result.Add(owner);

					}

				}

			}

			return result;
		
		}

		public bool IsPowered() {

			if (!ActiveEntity())
				return false;

			if (string.IsNullOrWhiteSpace(MyVisualScriptLogicProvider.GetEntityName(CubeGrid.EntityId)))
				MyVisualScriptLogicProvider.SetName(CubeGrid.EntityId, CubeGrid.EntityId.ToString());

			return MyVisualScriptLogicProvider.HasPower(CubeGrid.EntityId.ToString());

		}

		public bool IsSameGrid(IMyEntity entity) {

			if (!ActiveEntity())
				return false;

			foreach (var grid in LinkedGrids) {

				if (!grid.ActiveEntity())
					continue;

				if (grid.CubeGrid.EntityId == entity.EntityId)
					return true;
			
			}

			return false;

		}

		public bool IsStatic() {

			if (!ActiveEntity())
				return false;

			return CubeGrid.IsStatic;
		
		}

		public string Name() {

			if (!ActiveEntity())
				return "N/A";

			return !string.IsNullOrWhiteSpace(CubeGrid.CustomName) ? CubeGrid.CustomName : "N/A";

		}

		public OwnerTypeEnum OwnerTypes(bool onlyGetCurrentEntity = false, bool includeMinorityOwners = false) {

			var owners = GetOwners(onlyGetCurrentEntity, includeMinorityOwners);
			return EntityEvaluator.GetOwnersFromList(owners);
		
		}

		public Vector2 PowerOutput() {

			if (!ActiveEntity())
				return Vector2.Zero;

			return EntityEvaluator.GridPowerOutput(LinkedGrids);

		}

		public RelationTypeEnum RelationTypes(long ownerId, bool onlyGetCurrentEntity = false, bool includeMinorityOwners = false) {

			var owners = GetOwners(onlyGetCurrentEntity, includeMinorityOwners);
			return EntityEvaluator.GetRelationsFromList(ownerId, owners);

		}

		public float TargetValue() {

			if (!ActiveEntity())
				return 0;

			return EntityEvaluator.GridTargetValue(LinkedGrids);

		}

		public int WeaponCount() {

			if (!ActiveEntity())
				return 0;

			return EntityEvaluator.GridWeaponCount(LinkedGrids);

		}

		//---------------------------------------------------
		//------------End Interface Methods------------------
		//---------------------------------------------------

	}

}
