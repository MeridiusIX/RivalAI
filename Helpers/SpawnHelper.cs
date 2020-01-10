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
using RivalAI.Behavior;
using RivalAI.Behavior.Settings;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI;
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Helpers {

	public static class SpawnHelper {

		private static bool _spawnInProgress = false;
		private static List<SpawnProfile> _pendingSpawns = new List<SpawnProfile>();
		
		private static SpawnProfile _currentSpawn;
		private static MatrixD _spawnMatrix;
		

		public static void SpawnRequest(SpawnProfile spawn = null) {

			if(spawn != null) {

				_pendingSpawns.Add(spawn);

			}

			if(_spawnInProgress == true || _pendingSpawns.Count == 0)
				return;
			
			_currentSpawn = _pendingSpawns[0];
			_pendingSpawns.RemoveAt(0);
			_spawnInProgress = true;
			MyAPIGateway.Parallel.Start(SpawningParallelChecks, CompleteSpawning);

		}

		private static void SpawningParallelChecks() {
			
			if(_currentSpawn.UseRelativeSpawnPosition){
				
				var spawnCoords = Vector3D.Transform(_currentSpawn.RelativeSpawnOffset, _currentSpawn.CurrentPositionMatrix);
				_spawnMatrix = MatrixD.CreateWorld(spawnCoords, _currentSpawn.CurrentPositionMatrix.Forward, _currentSpawn.CurrentPositionMatrix.Up);

			}else{
				
				var upDir = VectorHelper.GetPlanetUpDirection(_currentSpawn.CurrentPositionMatrix.Translation);
				var playerList = new List<IMyPlayer>();
				MyAPIGateway.Players.GetPlayers(playerList);

				for (int i = 0; i < 15; i++) {

					if (upDir == Vector3D.Zero) {

						var spawnCoords = VectorHelper.RandomDirection() * VectorHelper.RandomDistance(_currentSpawn.MinDistance, _currentSpawn.MaxDistance) + _currentSpawn.CurrentPositionMatrix.Translation;
						var forwardDir = Vector3D.Normalize(spawnCoords - _currentSpawn.CurrentPositionMatrix.Translation);
						var upPerpDir = Vector3D.CalculatePerpendicularVector(forwardDir);
						_spawnMatrix = MatrixD.CreateWorld(spawnCoords, forwardDir, upPerpDir);

					} else {

						_spawnMatrix = VectorHelper.GetPlanetRandomSpawnMatrix(_currentSpawn.CurrentPositionMatrix.Translation, _currentSpawn.MinDistance, _currentSpawn.MaxDistance, _currentSpawn.MinAltitude, _currentSpawn.MaxAltitude, _currentSpawn.InheritNpcAltitude);

					}

					foreach (var player in playerList) {

						if (player.IsBot || player.Controller?.ControlledEntity?.Entity == null) {

							continue;

						}

						if (Vector3D.Distance(_spawnMatrix.Translation, player.GetPosition()) < 100) {

							Logger.MsgDebug(_currentSpawn.ProfileSubtypeId + ": Player Too Close To Possible Spawn Coords. Attempt " + (i + 1).ToString(), DebugTypeEnum.Spawn);
							_spawnMatrix = MatrixD.Identity;
							break;

						}

					}

					if (_spawnMatrix != MatrixD.Identity) {

						break;

					}

				}
				
			}
			
				

		}

		private static void CompleteSpawning() {
			
			MyAPIGateway.Utilities.InvokeOnGameThread(() =>{

				if (_spawnMatrix == MatrixD.Identity) {

					Logger.MsgDebug(_currentSpawn.ProfileSubtypeId + ": Spawn Coords Could Not Be Calculated. Aborting Process", DebugTypeEnum.Spawn);
					PerformNextSpawn();
					return;

				}

				Logger.MsgDebug(_currentSpawn.ProfileSubtypeId + ": Sending SpawnData to MES", DebugTypeEnum.Spawn);
				var velocity = Vector3D.Transform(_currentSpawn.RelativeSpawnVelocity, _spawnMatrix) - _spawnMatrix.Translation;
				var result = MESApi.CustomSpawnRequest(_currentSpawn.SpawnGroups, _spawnMatrix, velocity, _currentSpawn.IgnoreSafetyChecks);

				if (result == true) {

					Logger.MsgDebug(_currentSpawn.ProfileSubtypeId + ": Spawn Successful", DebugTypeEnum.Spawn);
					_currentSpawn.SpawnCount++;



				} else {

					Logger.MsgDebug(_currentSpawn.ProfileSubtypeId + ": Spawn Failed", DebugTypeEnum.Spawn);

				}

				PerformNextSpawn();

			});
   
		}

		private static void PerformNextSpawn() {

			_spawnInProgress = false;

			if (_pendingSpawns.Count > 0)
				SpawnRequest();

		}

	}

}
