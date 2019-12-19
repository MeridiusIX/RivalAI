using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRageMath;

//Change namespace to your mod's namespace
namespace RivalAI.Helpers {
    public static class MESApi {

        public static bool MESApiReady = false;
        private static Action<List<string>, Vector3D, Vector3D, Vector3D, Vector3> _customSpawnRequest;
        private static Func<List<string>> _getSpawnGroupBlackList;
        private static Func<List<string>> _getNpcNameBlackList;

        //Run This Method in your SessionComponent LoadData() Method
        public static void RegisterAPIListener() {

            MyAPIGateway.Utilities.RegisterMessageHandler(1521905890, APIListener);

        }

        /// <summary>
        /// Used To Spawn A Random SpawnGroup From A Provided List At A Provided Location. The Spawn Will Not Be Categorized As A CargoShip/RandomEncounter/Etc
        /// </summary>
        /// <param name="spawnGroups">List of SpawnGroups you want to attempt spawning from</param>
        /// <param name="coords">The coordinates the Spawn will use</param>
        /// <param name="forwardDir">Forward Direction vector for the spawn</param>
        /// <param name="upDir">Up Direction Vector for the spawn</param>
        /// <param name="velocity">Velocity vector</param>
        public static void CustomSpawnRequest(List<string> spawnGroups, Vector3D coords, Vector3D forwardDir, Vector3D upDir, Vector3 velocity) => _customSpawnRequest?.Invoke(spawnGroups, coords, forwardDir, upDir, velocity);

        /// <summary>
        /// Get a String List of all Current SpawnGroup SubtypeNames Currently in the MES Blacklist
        /// </summary>
        /// <returns>List of SpawnGroup SubtypeNames</returns>
        public static List<string> GetSpawnGroupBlackList() => _getSpawnGroupBlackList?.Invoke() ?? new List<string>();

        /// <summary>
        /// Get a String List of all Current SpawnGroup SubtypeNames Currently in the MES Blacklist
        /// </summary>
        /// <returns>List of NPC Grid Names</returns>
        public static List<string> GetNpcNameBlackList() => _getNpcNameBlackList?.Invoke() ?? new List<string>();

        public static void APIListener(object data) {

            var dict = data as Dictionary<string, Delegate>;

            if(dict == null) {

                return;

            }

            MESApiReady = true;
            _customSpawnRequest = (Action<List<string>, Vector3D, Vector3D, Vector3D, Vector3>)dict["CustomSpawnRequest"];
            _getSpawnGroupBlackList = (Func<List<string>>)dict["GetSpawnGroupBlackList"];
            _getNpcNameBlackList = (Func<List<string>>)dict["GetNpcNameBlackList"];

        }

    }

}
