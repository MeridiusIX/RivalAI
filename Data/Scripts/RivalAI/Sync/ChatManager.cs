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
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;

namespace RivalAI.Sync {
    public static class ChatManager {

        public static void ChatReceived(string messageText, ref bool sendToOthers) {

            var thisPlayer = MyAPIGateway.Session.LocalHumanPlayer;

            if(messageText.StartsWith("/RAI.") == false || MyAPIGateway.Session.LocalHumanPlayer == null) {

                return;

            }

            sendToOthers = false;

            bool isAdmin = false;

            if(thisPlayer.PromoteLevel == MyPromoteLevel.Admin || thisPlayer.PromoteLevel == MyPromoteLevel.Owner) {

                isAdmin = true;
                

            }

            if(isAdmin == false) {

                MyVisualScriptLogicProvider.ShowNotification("Access Denied. RivalAI Chat Commands Only Available To Admin Players.", 5000, "Red", thisPlayer.IdentityId);
                return;

            }

            var chatData = new ChatMessage();
            chatData.Mode = ChatMsgMode.ServerProcessing;
            chatData.Message = messageText;
            chatData.PlayerId = thisPlayer.IdentityId;
            chatData.SteamId = thisPlayer.SteamUserId;
            chatData.PlayerPosition = thisPlayer.GetPosition();
            chatData.PlayerEntity = thisPlayer.Controller?.ControlledEntity?.Entity != null ? thisPlayer.Controller.ControlledEntity.Entity.EntityId : 0;
            SendChatDataOverNetwork(chatData, true);

        }

        public static void ChatReceivedNetwork(ChatMessage chatData) {

            if(chatData.Mode == ChatMsgMode.ServerProcessing) {

                ProcessServerChat(chatData);

            }

            if(chatData.Mode == ChatMsgMode.ReturnMessage) {

                ProcessReturnChat(chatData);

            }

        }

        public static void ProcessServerChat(ChatMessage chatData) {

            var newChatData = chatData;
            newChatData.Mode = ChatMsgMode.ReturnMessage;

            //Debug Draw
            if (newChatData.Message.StartsWith("/RAI.DebugDrawToggle")) {

                BehaviorManager.DebugDraw = BehaviorManager.DebugDraw ? false : true;
                newChatData.ReturnMessage = "Debug Draw Enabled: " + BehaviorManager.DebugDraw.ToString();
                SendChatDataOverNetwork(newChatData, false);
                return;

            }

            //Debug Mode
            if (newChatData.Message.StartsWith("/RAI.Debug")) {

                newChatData.ProcessDebugMode();
                SendChatDataOverNetwork(newChatData, false);
                return;

            }

            //AdminCommand
            if (newChatData.Message.StartsWith("/RAI.Admin")) {
            
                
            
            }

            //Everything Else

            IMyEntity playerEntity = null;

            if (!MyAPIGateway.Entities.TryGetEntityById(newChatData.PlayerEntity, out playerEntity))
                return;

            var command = new Command();
            command.CommandCode = newChatData.Message;
            command.Type = CommandType.PlayerChat;
            command.Character = playerEntity;
            command.UseTriggerTargetDistance = true;
            command.Position = newChatData.PlayerPosition;
            command.PlayerIdentity = newChatData.PlayerId;
            CommandHelper.CommandTrigger?.Invoke(command);

        }

        public static void ProcessReturnChat(ChatMessage chatData) {

            if(string.IsNullOrWhiteSpace(chatData.ClipboardPayload) == false) {

                VRage.Utils.MyClipboardHelper.SetClipboard(chatData.ClipboardPayload);

            }

        }

        public static void SendChatDataOverNetwork(ChatMessage chatData, bool sendToServer) {

            if(string.IsNullOrWhiteSpace(chatData.ReturnMessage) == false && sendToServer == false) {

                MyVisualScriptLogicProvider.ShowNotification(chatData.ReturnMessage, 5000, "White", chatData.PlayerId);

            }

            var byteChatData = MyAPIGateway.Utilities.SerializeToBinary<ChatMessage>(chatData);
            var syncData = new SyncContainer(SyncMode.ChatCommand, byteChatData);
            var byteSyncData = MyAPIGateway.Utilities.SerializeToBinary<SyncContainer>(syncData);

            if(sendToServer == true) {

                MyAPIGateway.Multiplayer.SendMessageToServer(SyncManager.NetworkId, byteSyncData);

            } else {

                MyAPIGateway.Multiplayer.SendMessageTo(SyncManager.NetworkId, byteSyncData, chatData.SteamId);

            }
 
        }

    }
}
