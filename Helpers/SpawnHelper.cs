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
                
                var spawnCoords = Vector3D.Transform(_currentSpawn.RelativeSpawnOffset, _currentSpawn.CurrentPosition);
                _spawnMatrix = MatrixD.CreateWorld(spawnCoords, _currentSpawn.CurrentPosition.Forward, _currentSpawn.CurrentPosition.Up);

            }else{

                if(VectorHelper.GetPlanetUpDirection(_currentSpawn.CurrentPosition.Translation) == Vector3D.Zero){

                    //Space Calculations

                }else{

                    //Planet Calculations
                    var planet = MyGamePruningStructure.GetClosestPlanet(_currentSpawn.CurrentPosition.Translation);
                    


                }

            }
            
                

        }

        private static void CompleteSpawning() {
            
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>{

                var velocity = Vector3D.Transform(_currentSpawn.RelativeSpawnVelocity, _spawnMatrix) - _spawnMatrix.Translation;
                var result = MESApi.CustomSpawnRequest(_currentSpawn.SpawnGroups, _spawnMatrix, velocity, _currentSpawn.IgnoreSafetyChecks);
                
                if(result == true){

                    _currentSpawn.SpawnCount++;



                }
                
                _spawnInProgress = false;
                
                if(_pendingSpawns.Count > 0)
                    SpawnRequest();
            
            });
   
        }

    }

}
