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
using RivalAI;
using RivalAI.Behavior;
using RivalAI.Behavior.Settings;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;

namespace RivalAI.Sync {
    
    public static class EffectManager {

        public static bool SoundsPending = false;
        public static List<string> SoundsPendingList = new List<string>();

        public static IMyCharacter CurrentPlayerCharacter;
        public static MyEntity3DSoundEmitter SoundEmitter;

        public static void ClientReceiveEffect(Effects effectData) {

            if(effectData.Mode == EffectSyncMode.PlayerSound) {
                
                SoundsPendingList.Add(effectData.SoundId);
                SoundsPending = true;

            }

            if(effectData.Mode == EffectSyncMode.PositionSound) {

                ProcessPositionSoundEffect(effectData);

            }

        }

        public static void ProcessPlayerSoundEffect() {

            if(SoundsPending == false) {

                return;

            }

            if(CheckPlayerSoundEmitter() == false) {

                return;

            }

            if(SoundEmitter.IsPlaying == true) {

                return;

            }
            
            if(SoundsPendingList.Count == 0){
            
                SoundsPending = false;
                return;
            
            }
            
            var soundPair = new MySoundPair(SoundsPendingList[0]);
            SoundsPendingList.RemoveAt(0);
            SoundEmitter.PlaySound(soundPair, false, false, true, true, false);
            
            if(SoundsPendingList.Count == 0){
            
                SoundsPending = false;
            
            }

        }

        public static bool CheckPlayerSoundEmitter() {

            if(MyAPIGateway.Session.LocalHumanPlayer?.Character == null) {

                CurrentPlayerCharacter = null;
                SoundEmitter = null;
                return false;

            }

            if(MyAPIGateway.Session.LocalHumanPlayer.Character == CurrentPlayerCharacter) {

                return true;

            }

            CurrentPlayerCharacter = MyAPIGateway.Session.LocalHumanPlayer.Character;
            SoundEmitter = new MyEntity3DSoundEmitter(CurrentPlayerCharacter as MyEntity);
            return true;

        }

        public static void ProcessPositionSoundEffect(Effects effectData) {

            MyVisualScriptLogicProvider.PlaySingleSoundAtPosition(effectData.SoundId, effectData.Coords);

        }

    }
}
