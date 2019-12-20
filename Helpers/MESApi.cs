﻿using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRageMath;

//Change namespace to your mod's namespace
namespace RivalAI.Helpers {
    public static class MESApi {

        public static bool MESApiReady = false;

        private static long _mesModId = 1521905890;
        private static Action<Vector3D, string, double, int, int> _addKnownPlayerLocation;
        private static Action<List<string>, Vector3D, Vector3D, Vector3D, Vector3> _customSpawnRequest;
        private static Func<List<string>> _getSpawnGroupBlackList;
        private static Func<List<string>> _getNpcNameBlackList;
        private static Func<Vector3D, bool, string, bool> _isPositionInKnownPlayerLocation;

        //Run This Method in your SessionComponent LoadData() Method
        public static void RegisterAPIListener() {

            MyAPIGateway.Utilities.RegisterMessageHandler(_mesModId, APIListener);

        }

        /// <summary>
        /// Used to Create a Known Player Location that SpawnGroups can use as a Spawn Condition
        /// If a KPL already exists within the radius of the newly created location, its timer will be reset.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="faction"></param>
        /// <param name="radius"></param>
        /// <param name="expirationMinutes"></param>
        /// <param name="maxSpawns"></param>
        public static void AddKnownPlayerLocation(Vector3D coords, string faction, double radius, int expirationMinutes, int maxSpawns) => _addKnownPlayerLocation?.Invoke(coords, faction, radius, expirationMinutes, maxSpawns);

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

        /// <summary>
        /// Indicates whether a set of coordinates is within a Known Player Location.
        /// Accepts additional parameters to narrow search to faction specific Locations.
        /// </summary>
        /// <param name="coords">The coordinates you want to check</param>
        /// <param name="mustMatchFaction">Indicates if the faction match checks should be used</param>
        /// <param name="faction">Faction Tag to check against if mustMatchFaction is true</param>
        /// <returns>true if position is in a valid Known Player Location</returns>
        public static bool IsPositionInKnownPlayerLocation(Vector3D coords, bool mustMatchFaction = false, string faction = "") => _isPositionInKnownPlayerLocation?.Invoke(coords, mustMatchFaction, faction) ?? false;

        public static void APIListener(object data) {

            var dict = data as Dictionary<string, Delegate>;

            if(dict == null) {

                return;

            }

            MESApiReady = true;
            _addKnownPlayerLocation = (Action<Vector3D, string, double, int, int>)dict["AddKnownPlayerLocation"];
            _customSpawnRequest = (Action<List<string>, Vector3D, Vector3D, Vector3D, Vector3>)dict["CustomSpawnRequest"];
            _getSpawnGroupBlackList = (Func<List<string>>)dict["GetSpawnGroupBlackList"];
            _getNpcNameBlackList = (Func<List<string>>)dict["GetNpcNameBlackList"];
            _isPositionInKnownPlayerLocation = (Func<Vector3D, bool, string, bool>)dict["IsPositionInKnownPlayerLocation"];

        }

    }

}
