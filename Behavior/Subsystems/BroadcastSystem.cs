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
using RivalAI.Helpers;
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Behavior.Subsystems {

    public struct ChatDetails {

        public bool UseChat;
        public double ChatTriggerDistance;
        public int ChatMinTime;
        public int ChatMaxTime;
        public int ChatChance;
        public int MaxChats;
        public List<string> ChatMessages;
        public List<string> ChatAudio;
        public List<BroadcastType> BroadcastChatType;

    }

    public class BroadcastSystem {

        //Configurable
        public bool UseChatSystem;
        public bool UseNotificationSystem;
        public bool DelayChatIfSoundPlaying;
        public bool SingleChatPerTrigger;
        public string ChatAuthor;
        public string ChatAuthorColor;

        //New Classes
        public List<ChatProfile> ChatControlReference;

        //Non-Configurable
        public IMyRemoteControl RemoteControl;
        public string LastChatMessageSent;
        public List<IMyRadioAntenna> AntennaList;
        public double HighestRadius;
        public Vector3D AntennaCoords;
        public Random Rnd;


        public BroadcastSystem(IMyRemoteControl remoteControl = null) {

            UseChatSystem = false;
            UseNotificationSystem = false;
            DelayChatIfSoundPlaying = true;
            SingleChatPerTrigger = true;
            ChatAuthor = "";
            ChatAuthorColor = "";

            ChatControlReference = new List<ChatProfile>();

            RemoteControl = null;
            LastChatMessageSent = "";
            AntennaList = new List<IMyRadioAntenna>();
            HighestRadius = 0;
            AntennaCoords = Vector3D.Zero;
            Rnd = new Random();

            Setup(remoteControl);


        }

        private void Setup(IMyRemoteControl remoteControl) {

            if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false) {

                return;

            }

            this.RemoteControl = remoteControl;
            RefreshAntennaList();

        }

        public void InitTags() {

            if(string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == false) {

                var descSplit = this.RemoteControl.CustomData.Split('\n');

                foreach(var tag in descSplit) {

                    //UseChatSystem
                    if(tag.Contains("[UseChatSystem:") == true) {

                        this.UseChatSystem = TagHelper.TagBoolCheck(tag);

                    }

                    //UseNotificationSystem
                    if(tag.Contains("[UseNotificationSystem:") == true) {

                        this.UseNotificationSystem = TagHelper.TagBoolCheck(tag);

                    }

                    //DelayChatIfSoundPlaying
                    if(tag.Contains("[DelayChatIfSoundPlaying:") == true) {

                        this.DelayChatIfSoundPlaying = TagHelper.TagBoolCheck(tag);

                    }

                    //DelayChatIfSoundPlaying
                    if(tag.Contains("[DelayChatIfSoundPlaying:") == true) {

                        this.DelayChatIfSoundPlaying = TagHelper.TagBoolCheck(tag);

                    }

                    //SingleChatPerTrigger
                    if(tag.Contains("[SingleChatPerTrigger:") == true) {

                        this.SingleChatPerTrigger = TagHelper.TagBoolCheck(tag);

                    }

                    //ChatAuthor
                    if(tag.Contains("[ChatAuthor:") == true) {

                        this.ChatAuthor = TagHelper.TagStringCheck(tag);

                    }

                    //ChatAuthorColor
                    if(tag.Contains("[ChatAuthorColor:") == true) {

                        this.ChatAuthorColor = TagHelper.TagStringCheck(tag);

                    }

                }

                foreach(var chatControl in this.ChatControlReference) {

                    chatControl.InitTags(this.RemoteControl.CustomData);

                }

            }


        }

        public void BroadcastRequest(ChatProfile chat) {

            if(chat.UseChat == false) {

                return;

            }

            string message = "";
            string sound = "";
            var broadcastType = BroadcastType.None;

            if(chat.ProcessChat(ref message, ref sound, ref broadcastType) == false) {

                return;

            }

            if(this.LastChatMessageSent == message || string.IsNullOrWhiteSpace(message) == true) {

                Logger.AddMsg("Last Message Same", true);
                return;

            }

            GetHighestAntennaRange();

            if(this.HighestRadius == 0) {

                Logger.AddMsg("No Valid Antenna", true);
                return;

            }

            var playerList = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(playerList);

            foreach(var player in playerList) {

                if(player.IsBot == true || player.Character == null) {

                    continue;

                }

                if(Vector3D.Distance(player.GetPosition(), this.AntennaCoords) > this.HighestRadius) {

                    continue;

                }

                var modifiedMsg = message;

                if(modifiedMsg.Contains("{PlayerName}") == true) {

                    modifiedMsg = modifiedMsg.Replace("{PlayerName}", player.DisplayName);

                }

                if(modifiedMsg.Contains("{PlayerFactionName}") == true) {

                    var playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);

                    if(playerFaction != null) {

                        modifiedMsg = modifiedMsg.Replace("{PlayerFactionName}", playerFaction.Name);

                    } else {

                        modifiedMsg = modifiedMsg.Replace("{PlayerFactionName}", "Unaffiliated");

                    }

                }

                var authorName = this.ChatAuthor;
                var authorColor = this.ChatAuthorColor;

                if(string.IsNullOrWhiteSpace(chat.AuthorOverride) == false) {

                    authorName = chat.AuthorOverride;

                }

                if(string.IsNullOrWhiteSpace(chat.ColorOverride) == false) {

                    authorColor = chat.ColorOverride;

                }

                if(authorColor != "White" && authorColor != "Red" && authorColor != "Green" && authorColor != "Blue") {

                    authorColor = "White";

                }

                if(broadcastType == BroadcastType.Chat || broadcastType == BroadcastType.Both) {

                    MyVisualScriptLogicProvider.SendChatMessage(modifiedMsg, authorName, player.IdentityId, authorColor);

                }

                if(broadcastType == BroadcastType.Notify || broadcastType == BroadcastType.Both) {

                    MyVisualScriptLogicProvider.ShowNotification(modifiedMsg, 6000, authorColor, player.IdentityId);

                }

                if(string.IsNullOrWhiteSpace(sound) == false && sound != "None") {

                    if(string.IsNullOrEmpty(player.Character.Name) == true) {

                        MyVisualScriptLogicProvider.SetName(player.Character.EntityId, player.Character.EntityId.ToString());

                    }

                    var effect = new Effects();
                    effect.Mode = EffectSyncMode.PlayerSound;
                    effect.SoundId = sound;
                    

                }

            }

        }

        public void ProcessAutoMessages() {

            if(RAI_SessionCore.IsServer == false || (this.UseChatSystem == false && this.UseNotificationSystem == false)) {

                //Logger.AddMsg("Chat System Inactive", true);
                return;

            }

        }

        private void GetHighestAntennaRange() {

            this.HighestRadius = 0;
            this.AntennaCoords = Vector3D.Zero;

            foreach(var antenna in this.AntennaList) {

                if(antenna?.SlimBlock == null) {

                    continue;

                }

                if(antenna.IsWorking == false || antenna.IsFunctional == false || antenna.IsBroadcasting == false) {

                    continue;

                }

                if(antenna.Radius > this.HighestRadius) {

                    this.HighestRadius = antenna.Radius;
                    this.AntennaCoords = antenna.GetPosition();

                }

            }

            

        }

        private void GetRandomChatAndSoundFromLists(List<string> messages, List<string> sounds, List<BroadcastType> broadcastTypes, ref string message, ref string sound, ref BroadcastType broadcastType){

            if(messages.Count == 0) {

                return;

            }

            var index = Rnd.Next(0, messages.Count);
            message = messages[index];

            if(sounds.Count >= messages.Count) {

                sound = sounds[index];

            }

            if(broadcastTypes.Count >= messages.Count) {

                broadcastType = broadcastTypes[index];

            }

        }

        public void RefreshAntennaList() {

            if(this.RemoteControl == null || MyAPIGateway.Entities.Exist(this.RemoteControl?.SlimBlock?.CubeGrid) == false) {

                return;

            }

            this.AntennaList.Clear();
            var blockList = TargetHelper.GetAllBlocks(this.RemoteControl.SlimBlock.CubeGrid);

            foreach(var block in blockList.Where(x => x.FatBlock != null)) {

                var antenna = block.FatBlock as IMyRadioAntenna;

                if(antenna != null) {

                    this.AntennaList.Add(antenna);

                }

            }

        }

    }

}
