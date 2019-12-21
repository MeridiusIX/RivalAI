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

        public static void SpawnRequest(SpawnProfile spawn = null) {

            if(spawn != null) {

                _pendingSpawns.Add(spawn);

            }

            if(_spawnInProgress == true) {

                return;

            }



        }

        private static void SpawningParallelChecks() {



        }

        private static void CompleteSpawning() {



        }

    }

}
