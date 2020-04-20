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

	public enum BlockTypeEnum {
	
		None,
		All,
		Antennas,
		Beacons,
		Containers,
		Controllers,
		Guns,
		JumpDrives,
		Mechanical,
		Production,
		Power,
		Shields,
		Thrusters,
		Tools,
		Turrets
	
	}

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

			collection.Add(blockEntity);
			return true;
		
		}

		public override void CloseEntity(IMyEntity entity) {

			base.CloseEntity(entity);
			Unload();

		}

		public void PhysicsCheck(IMyEntity entity) {

			HasPhysics = entity.Physics != null ? true : false;

		}

		public void RefreshSubGrids() {

			LinkedGrids = EntityEvaluator.GetAttachedGrids(CubeGrid);

		}

		public override void Unload() {

			base.Unload();
			CubeGrid.OnBlockAdded -= NewBlockAdded;
			UnloadEntities?.Invoke();

		}

		//---------------------------------------------------
		//-----------Start Interface Methods-----------------
		//---------------------------------------------------

		public bool ActiveEntity() {

			if (Closed)
				return false;

			return true;

		}
		public double BroadcastRange(bool onlyAntenna = false) {

			if (Closed)
				return 0;



			return 0;

		}

		public bool IsNpcOwned() {

			if (IsClosed() || CubeGrid?.BigOwners == null)
				return false;

			if (CubeGrid.BigOwners.Count > 0)
				return EntityEvaluator.IsIdentityNPC(CubeGrid.BigOwners[0]);

			return false;

		}

		public bool IsPowered() {

			if (IsClosed())
				return false;

			if (string.IsNullOrWhiteSpace(MyVisualScriptLogicProvider.GetEntityName(CubeGrid.EntityId)))
				MyVisualScriptLogicProvider.SetName(CubeGrid.EntityId, CubeGrid.EntityId.ToString());

			return MyVisualScriptLogicProvider.HasPower(CubeGrid.EntityId.ToString());

		}

		public bool IsUnowned() {

			if (IsClosed() || CubeGrid?.BigOwners == null)
				return false;

			if (CubeGrid.BigOwners.Count == 0) {

				return true;

			} else {

				if (CubeGrid.BigOwners[0] == 0)
					return true;

			}

			return false;

		}

		public Vector2 PowerOutput() {

			return Vector2.Zero;

		}

		public int Reputation(long ownerId) {

			if (IsClosed() || CubeGrid?.BigOwners == null)
				return -1000;

			if (CubeGrid.BigOwners.Count > 0)
				return EntityEvaluator.GetReputationBetweenIdentities(ownerId, CubeGrid.BigOwners[0]);

			return -1000;

		}

		public float TargetValue() {

			return 0;

		}

		public int WeaponCount() {

			return 0;

		}

		//---------------------------------------------------
		//------------End Interface Methods------------------
		//---------------------------------------------------

	}

}
