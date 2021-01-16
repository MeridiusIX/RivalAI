using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace RivalAI.Entities {
	public static class PlayerManager {

		public static List<IMyPlayer> ActivePlayers = new List<IMyPlayer>();
		public static List<PlayerEntity> Players = new List<PlayerEntity>();

		private static bool _refreshFromActivePlayers = false;

		public static Action UnloadEntities;

		public static void PlayerConnectEvent() {

			_refreshFromActivePlayers = true;

		}

		public static void RefreshAllPlayers(bool forceCheck = false) {

			if (!_refreshFromActivePlayers && !forceCheck)
				return;

			_refreshFromActivePlayers = false;
			ActivePlayers.Clear();
			MyAPIGateway.Players.GetPlayers(ActivePlayers);

			foreach (var player in ActivePlayers) {

				bool foundExisting = false;

				foreach (var playerEnt in Players) {

					if (playerEnt.Player == null)
						continue;

					if (playerEnt.Player.SteamUserId == player.SteamUserId) {

						foundExisting = true;

						if (playerEnt.Player.IdentityId != player.IdentityId) {

							playerEnt.Player = player;

						}
					
					}
				
				}

				if (foundExisting)
					continue;

				if (!player.IsBot && player.SteamUserId > 0) {

					var playerEntity = new PlayerEntity(player);
					UnloadEntities += playerEntity.Unload;
					Players.Add(playerEntity);

				}
			
			}

		}

	}

}
