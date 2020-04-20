using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Entities {
	public class PlayerEntity : EntityBase, ITarget {

		public IMyPlayer Player;
		public bool Online;
		public bool IsParentEntityGrid;

		public bool PlayerEntityChanged;

		public List<GridEntity> LinkedGrids;

		public PlayerEntity(IMyPlayer player, IMyEntity entity = null) : base(entity) {

			if (player == null) 
				return;

			IsValidEntity = true;
			Player = player;
			Online = true;

			LinkedGrids = new List<GridEntity>();

			MyVisualScriptLogicProvider.PlayerDisconnected += PlayerDisconnect;
			MyVisualScriptLogicProvider.PlayerConnected += PlayerConnect;

			MyVisualScriptLogicProvider.PlayerSpawned += PlayerSpawn;
			MyVisualScriptLogicProvider.PlayerLeftCockpit += PlayerCockpitAction;
			MyVisualScriptLogicProvider.PlayerEnteredCockpit += PlayerCockpitAction;
			MyVisualScriptLogicProvider.RemoteControlChanged += PlayerRemoteAction;

			RefreshPlayerEntity();

		}

		public void PlayerCockpitAction(string entA, long id, string entB) {

			if (id != Player.IdentityId)
				return;

			PlayerEntityChanged = true;

		}

		public void PlayerConnect(long id) {

			if (id != Player.IdentityId)
				return;

			Online = true;

		}

		public void PlayerDisconnect(long id) {

			if (id != Player.IdentityId)
				return;

			Online = false;

		}

		public void PlayerRemoteAction(bool cont, long id, string ent, long endId, string grid, long gridA) {

			if (id != Player.IdentityId)
				return;

			PlayerEntityChanged = true;

		}

		public void PlayerSpawn(long id) {

			if (id != Player.IdentityId)
				return;

			PlayerEntityChanged = true;

		}

		public void RefreshPlayerEntity() {

			this.PlayerEntityChanged = false;
			this.IsParentEntityGrid = false;

			if (Player?.Controller?.ControlledEntity?.Entity == null)
				return;

			if (Player.Controller.ControlledEntity.Entity.Closed || Player.Controller.ControlledEntity.Entity.MarkedForClose)
				return;

			if (Entity != null && !Entity.Closed && !Entity.MarkedForClose)
				Entity.OnClose -= (e) => { Closed = true; };

			var character = Player.Controller.ControlledEntity.Entity as IMyCharacter;

			if (character != null) {

				this.Closed = false;
				this.Entity = character;
				this.ParentEntity = character;
				return;

			}

			var controller = Player.Controller.ControlledEntity.Entity as IMyShipController;

			if (controller != null) {

				this.Closed = false;
				this.Entity = controller;
				this.ParentEntity = controller.SlimBlock.CubeGrid;
				this.IsParentEntityGrid = true;
				return;

			}

		}

		//---------------------------------------------------
		//-----------Start Interface Methods-----------------
		//---------------------------------------------------

		public bool ActiveEntity() {

			if (Closed || !Online)
				return false;

			return true;
		
		}

		public double BroadcastRange(bool onlyAntenna = false) {

			if(IsClosed())
				return 0;

			if (IsParentEntityGrid) {

				var grid = ParentEntity as IMyCubeGrid;

				if (grid == null)
					return 0;

				return EntityEvaluator.GridBroadcastRange(LinkedGrids);

			} else {

				var character = ParentEntity as IMyCharacter;

				if (character == null)
					return 0;

				var controlledEntity = character as Sandbox.Game.Entities.IMyControllableEntity;

				if (controlledEntity == null || !controlledEntity.EnabledBroadcasting)
					return 0;

				return 200; //200 is max range of suit antenna

			}

		}

		public override bool IsClosed() {

			//If player is not online, they're considered
			//inactive. Otherwise, whatever entity they're
			//in control of is considered instead.

			if (!Online)
				return false;

			return Closed;

		}

		public bool IsNpcOwned() {

			return Player.IsBot;
		
		}

		public bool IsPowered() {

			var character = ParentEntity as IMyCharacter;

			if (character == null)
				return false;

			if (MyVisualScriptLogicProvider.GetPlayersEnergyLevel(Player.IdentityId) < 1)
				return false;

			return true;

		}

		public bool IsUnowned() {

			//There shouldnt be a situation where a 'player'
			//is unowned, so this is always false here.

			return false;
				
		}

		public Vector2 PowerOutput() {

			if (IsClosed())
				return Vector2.Zero;

			if (IsParentEntityGrid) {

				var grid = ParentEntity as IMyCubeGrid;

				if (grid == null)
					return Vector2.Zero;

				return EntityEvaluator.GridPowerOutput(LinkedGrids);

			} else {

				var character = ParentEntity as IMyCharacter;

				if (character == null)
					return Vector2.Zero;

				if(MyVisualScriptLogicProvider.GetPlayersEnergyLevel(Player.IdentityId) < 1)
					return Vector2.Zero;

				return new Vector2(0.009f, 0.009f);

			}

		}

		public void RefreshSubGrids() {

			if (IsParentEntityGrid) {

				var grid = ParentEntity as IMyCubeGrid;

				if (grid == null)
					return;

				LinkedGrids = EntityEvaluator.GetAttachedGrids(grid);

			}

		}

		public int Reputation(long ownerId) {

			return EntityEvaluator.GetReputationBetweenIdentities(ownerId, Player.IdentityId);
		
		}

		public float TargetValue() {

			if (IsClosed())
				return 0;

			if (IsParentEntityGrid) {

				var grid = ParentEntity as IMyCubeGrid;

				if (grid == null)
					return 0;

				return EntityEvaluator.GridTargetValue(LinkedGrids);

			} else {

				var character = ParentEntity as IMyCharacter;

				if (character == null)
					return 0;

				float threat = 0;

				if (!character.HasInventory)
					return 0;

				var items = new List<VRage.Game.ModAPI.Ingame.MyInventoryItem>();
				character.GetInventory().GetItems(items);

				foreach (var item in items) {

					if (item.Type.TypeId.EndsWith("PhysicalGunObject")) {

						threat += 25;
						continue;

					}

					if (item.Type.TypeId.EndsWith("AmmoMagazine")) {

						threat += 3;
						continue;

					}

					if (item.Type.TypeId.EndsWith("ContainerObject")) {

						threat += 10;
						continue;

					}

				}

				return threat;

			}

		}

		public int WeaponCount() {

			if (IsParentEntityGrid) {

				var grid = ParentEntity as IMyCubeGrid;

				if (grid == null)
					return 0;

				return EntityEvaluator.GridWeaponCount(EntityEvaluator.GetAttachedGrids(grid));

			} else {

				var character = ParentEntity as IMyCharacter;

				if (character == null)
					return 0;

				float count = 0;

				if (!character.HasInventory)
					return 0;

				var items = new List<VRage.Game.ModAPI.Ingame.MyInventoryItem>();
				character.GetInventory().GetItems(items);

				foreach (var item in items) {

					if (item.Type.TypeId.EndsWith("PhysicalGunObject")) {

						count++;
						continue;

					}

				}

			}

			return 0;

		}

		//---------------------------------------------------
		//------------End Interface Methods------------------
		//---------------------------------------------------

		public override void Unload(){

			base.Unload();
			MyVisualScriptLogicProvider.PlayerDisconnected -= PlayerDisconnect;
			MyVisualScriptLogicProvider.PlayerConnected -= PlayerConnect;
			MyVisualScriptLogicProvider.PlayerSpawned -= PlayerSpawn;
			MyVisualScriptLogicProvider.PlayerLeftCockpit -= PlayerCockpitAction;
			MyVisualScriptLogicProvider.PlayerEnteredCockpit -= PlayerCockpitAction;
			MyVisualScriptLogicProvider.RemoteControlChanged -= PlayerRemoteAction;

		}

	}

}
