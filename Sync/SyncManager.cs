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
    public static class SyncManager {

        public static ushort NetworkId = 42007;

        public static void Setup() {

            MyAPIGateway.Multiplayer.RegisterMessageHandler(NetworkId, NetworkMessageReceiver);
            MyAPIGateway.Utilities.MessageEntered += ChatManager.ChatReceived;

        }
        
        public static void SendSyncMesage(SyncContainer syncContainer, ulong userId = 0, bool sendServer = false, bool sendOthers = false){
        
            var byteData = MyAPIGateway.Utilities.SerializeToBinary<SyncContainer>(syncContainer);
            
            if(userId != 0){
            
                MyAPIGateway.Multiplayer.SendMessageTo(NetworkId, byteData, userId);
            
            }
            
            if(sendServer == true){
            
                MyAPIGateway.Multiplayer.SendMessageToServer(NetworkId, byteData);
            
            }
            
            if(sendOthers == true){
            
                MyAPIGateway.Multiplayer.SendMessageToOthers(NetworkId, byteData);
            
            }
        
        }

        public static void NetworkMessageReceiver(byte[] initialData) {

            try {

                var container = MyAPIGateway.Utilities.SerializeFromBinary<SyncContainer>(initialData);

                if(container == null) {

                    return;

                }

                if(container.Mode == SyncMode.ChatCommand) {

                    var chatData = MyAPIGateway.Utilities.SerializeFromBinary<ChatMessage>(container.Data);

                    if(chatData != null) {

                        ChatManager.ChatReceivedNetwork(chatData);

                    }

                }

                if(container.Mode == SyncMode.Effect) {

                    var effectData = MyAPIGateway.Utilities.SerializeFromBinary<Effects>(container.Data);

                    if(effectData != null) {

                        EffectManager.ClientReceiveEffect(effectData);

                    }

                }

                if (container.Mode == SyncMode.ReputationAlert) {

                    var alertData = MyAPIGateway.Utilities.SerializeFromBinary<ReputationMessage>(container.Data);

                    if (alertData != null) {

                        ReputationAnnounceManager.ProcessMessage(alertData);

                    }

                }

            } catch(Exception exc) {

                Logger.MsgDebug("Exception in NetworkMessageReceiver", DebugTypeEnum.General);
                Logger.MsgDebug(exc.ToString(), DebugTypeEnum.General);

            }

        }

        public static void Close() {

            MyAPIGateway.Utilities.MessageEntered -= ChatManager.ChatReceived;
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(NetworkId, NetworkMessageReceiver);

        }

    }
}
