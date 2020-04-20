using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Entities {
	public class BlockEntity : EntityBase, ITarget {

		public IMyTerminalBlock Block;
		public IMyFunctionalBlock FunctionalBlock;

		public bool Enabled;		//Block Is Turned On (If It Can Be)
		public bool Functional;		//Built & Undamaged
		public bool Modded;			//Is From a Mod
		public bool Working;        //Powered

		public List<GridEntity> LinkedGrids;

		public BlockEntity(IMyEntity entity, IMyEntity parentEntity) : base(entity){

			if (entity == null || parentEntity == null)
				return;

			Block = entity as IMyTerminalBlock;
			IsValidEntity = true;
			RefreshSubGrids();

			Block.IsWorkingChanged += WorkingChanged;
			WorkingChanged(Block);

			FunctionalBlock = Block as IMyFunctionalBlock;

			if (FunctionalBlock != null) {

				FunctionalBlock.EnabledChanged += EnabledChanged;
				EnabledChanged(FunctionalBlock);

			}


		}

		public void EnabledChanged(IMyTerminalBlock block) {

			Enabled = FunctionalBlock.Enabled;

		}

		public void WorkingChanged(IMyCubeBlock block) {

			Functional = block.IsFunctional;
			Working = block.IsWorking;

		}

		//---------------------------------------------------
		//-----------Start Interface Methods-----------------
		//---------------------------------------------------

		public bool ActiveEntity() {

			if (Closed || !Functional || !Working)
				return false;

			return true;

		}
		public double BroadcastRange(bool onlyAntenna = false) {

			return EntityEvaluator.GridBroadcastRange(LinkedGrids, onlyAntenna);

		}

		public bool IsNpcOwned() {

			if (IsClosed() || Block?.SlimBlock?.CubeGrid?.BigOwners == null)
				return false;

			if (Block.SlimBlock.CubeGrid.BigOwners.Count > 0)
				return EntityEvaluator.IsIdentityNPC(Block.SlimBlock.CubeGrid.BigOwners[0]);

			return false;

		}

		public bool IsPowered() {

			if (IsClosed() || !Functional)
				return false;

			if (FunctionalBlock != null) {

				if (Enabled) {

					return Working && Functional;

				} else {

					return EntityEvaluator.GridPowered(LinkedGrids);
				
				}
			
			}

			return Working && Functional;

		}

		public bool IsUnowned() {

			if (IsClosed() || Block?.SlimBlock?.CubeGrid?.BigOwners == null)
				return false;

			if (Block.SlimBlock.CubeGrid.BigOwners.Count == 0) {

				return true;

			} else {

				if (Block.SlimBlock.CubeGrid.BigOwners[0] == 0)
					return true;
			
			}

			return false;

		}

		public Vector2 PowerOutput() {

			return EntityEvaluator.GridPowerOutput(LinkedGrids);

		}

		public void RefreshSubGrids() {

			LinkedGrids = EntityEvaluator.GetAttachedGrids(Block.SlimBlock.CubeGrid);

		}

		public int Reputation(long ownerId) {

			if (IsClosed() || Block?.SlimBlock?.CubeGrid?.BigOwners == null)
				return -1000;

			if (Block.SlimBlock.CubeGrid.BigOwners.Count > 0)
				return EntityEvaluator.GetReputationBetweenIdentities(ownerId, Block.SlimBlock.CubeGrid.BigOwners[0]);

			return -1000;

		}

		public float TargetValue() {

			return EntityEvaluator.GridTargetValue(LinkedGrids);

		}

		public int WeaponCount() {

			return EntityEvaluator.GridWeaponCount(LinkedGrids);

		}

		//---------------------------------------------------
		//------------End Interface Methods------------------
		//---------------------------------------------------

		public override void Unload() {

			base.Unload();
			Block.IsWorkingChanged -= WorkingChanged;

			if (FunctionalBlock != null)
				FunctionalBlock.EnabledChanged -= EnabledChanged;

		}

	}

}
