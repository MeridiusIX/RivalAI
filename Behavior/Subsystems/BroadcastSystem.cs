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


namespace RivalAI.Behavior.Subsystems{

    public class BroadcastSystem {

        //Configurable
        public bool UseChatSystem;
        public bool UseNotificationSystem;
        public bool DelayChatIfSoundPlaying;
        public string ChatAuthor;
        public string ChatAuthorColor;

        public bool UseGreetingChat;
        public double GreetingChatTriggerDistance;
        public int GreetingChatChance;
        public List<string> GreetingChatMessages;
        public List<string> GreetingChatSounds;

        public bool UseTauntChat;
        public int TauntChatMinTime;
        public int TauntChatMaxTime;
        public int TauntChatChance;
        public int MaxTauntChats;
        public List<string> TauntChatMessages;
        public List<string> TauntChatSounds;

        public bool UseRetreatChat;
        public int RetreatChatChance;
        public List<string> RetreatChatMessages;
        public List<string> RetreatChatSounds;

        public bool UseDamageChat;
        public int DamageChatChance;
        public List<string> DamageChatMessages;
        public List<string> DamageChatSounds;

        //Non-Configurable
        public IMyRemoteControl RemoteControl;
        public string LastChatMessageSent;
        public bool GreetingChatSent; //Storage
        public int TauntsSentCount; //Storage
        public DateTime LastTauntTime;
        public int SecondsUntilTaunt;

        public bool RetreatChatSent; //Storage
        public List<IMyRadioAntenna> AntennaList;
        public double HighestRadius;
        public Vector3D AntennaCoords;
        public Random Rnd;


        public BroadcastSystem(IMyRemoteControl remoteControl = null) {

            UseChatSystem = false;
            UseNotificationSystem = false;
            DelayChatIfSoundPlaying = true;
            ChatAuthor = "";
            ChatAuthorColor = "";

            UseGreetingChat = false;
            GreetingChatTriggerDistance = 4000;
            GreetingChatChance = 100;
            GreetingChatMessages = new List<string>();

            UseTauntChat = false;
            TauntChatMinTime = 60;
            TauntChatMaxTime = 120;
            TauntChatChance = 100;
            MaxTauntChats = 5;
            TauntChatMessages = new List<string>();

            UseRetreatChat = false;
            RetreatChatChance = 100;
            RetreatChatMessages = new List<string>();

            UseDamageChat = false;

            RemoteControl = null;
            LastChatMessageSent = "";
            GreetingChatSent = false;
            TauntsSentCount = 0;
            LastTauntTime = MyAPIGateway.Session.GameDateTime;
            SecondsUntilTaunt = TauntChatMinTime;
            RetreatChatSent = false;
            AntennaList = new List<IMyRadioAntenna>();
            Rnd = new Random();

            Setup(remoteControl);


        }

        private void Setup(IMyRemoteControl remoteControl) {

            if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false) {

                return;

            }

            this.RemoteControl = remoteControl;

            if(string.IsNullOrWhiteSpace(remoteControl.CustomData) == true) {

                var descSplit = remoteControl.CustomData.Split('\n');

                foreach(var tag in descSplit) {

                    //UseChatSystem
                    if(tag.Contains("[UseChatSystem") == true) {

                        this.UseChatSystem = TagHelper.TagBoolCheck(tag);

                    }

                    //UseNotificationSystem
                    if(tag.Contains("[UseNotificationSystem") == true) {

                        this.UseNotificationSystem = TagHelper.TagBoolCheck(tag);

                    }

                    //DelayChatIfSoundPlaying
                    if(tag.Contains("[DelayChatIfSoundPlaying") == true) {

                        this.DelayChatIfSoundPlaying = TagHelper.TagBoolCheck(tag);

                    }

                    //ChatAuthor
                    if(tag.Contains("[ChatAuthor") == true) {

                        this.ChatAuthor = TagHelper.TagStringCheck(tag);

                    }

                    //UseGreetingChat
                    if(tag.Contains("[UseGreetingChat") == true) {

                        this.UseGreetingChat = TagHelper.TagBoolCheck(tag);

                    }

                    //GreetingChatTriggerDistance
                    if(tag.Contains("[GreetingChatTriggerDistance") == true) {

                        this.GreetingChatTriggerDistance = TagHelper.TagDoubleCheck(tag, this.GreetingChatTriggerDistance);

                    }

                    //GreetingChatChance
                    if(tag.Contains("[GreetingChatChance") == true) {

                        this.GreetingChatChance = TagHelper.TagIntCheck(tag, this.GreetingChatChance);

                    }

                    //GreetingChatMessages
                    if(tag.Contains("[GreetingChatMessages") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);
                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            this.GreetingChatMessages.Add(tempValue);

                        }

                    }

                    //GreetingChatSounds
                    if(tag.Contains("[GreetingChatSounds") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);
                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            this.GreetingChatSounds.Add(tempValue);

                        }

                    }

                    //UseTauntChat
                    if(tag.Contains("[UseTauntChat") == true) {

                        this.UseTauntChat = TagHelper.TagBoolCheck(tag);

                    }

                    //TauntChatMinTime
                    if(tag.Contains("[TauntChatMinTime") == true) {

                        this.TauntChatMinTime = TagHelper.TagIntCheck(tag, this.TauntChatMinTime);

                    }

                    //TauntChatMaxTime
                    if(tag.Contains("[TauntChatMaxTime") == true) {

                        this.TauntChatMaxTime = TagHelper.TagIntCheck(tag, this.TauntChatMaxTime);

                    }

                    //TauntChatChance
                    if(tag.Contains("[TauntChatChance") == true) {

                        this.TauntChatChance = TagHelper.TagIntCheck(tag, this.TauntChatChance);

                    }

                    //MaxTauntChats
                    if(tag.Contains("[MaxTauntChats") == true) {

                        this.MaxTauntChats = TagHelper.TagIntCheck(tag, this.MaxTauntChats);

                    }

                    //TauntChatMessages
                    if(tag.Contains("[TauntChatMessages") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);
                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            this.TauntChatMessages.Add(tempValue);

                        }

                    }

                    //TauntChatSounds
                    if(tag.Contains("[TauntChatSounds") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);
                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            this.TauntChatSounds.Add(tempValue);

                        }

                    }

                    //UseRetreatChat
                    if(tag.Contains("[UseRetreatChat") == true) {

                        this.UseRetreatChat = TagHelper.TagBoolCheck(tag);

                    }

                    //RetreatChatChance
                    if(tag.Contains("[RetreatChatChance") == true) {

                        this.RetreatChatChance = TagHelper.TagIntCheck(tag, this.RetreatChatChance);

                    }

                    //RetreatChatMessages
                    if(tag.Contains("[RetreatChatMessages") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);
                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            this.RetreatChatMessages.Add(tempValue);

                        }

                    }

                    //RetreatChatSounds
                    if(tag.Contains("[RetreatChatMessages") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);
                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            this.RetreatChatMessages.Add(tempValue);

                        }

                    }

                    //UseDamageChat
                    if(tag.Contains("[UseDamageChat") == true) {

                        this.UseDamageChat = TagHelper.TagBoolCheck(tag);

                    }

                    //DamageChatChance
                    if(tag.Contains("[DamageChatChance") == true) {

                        this.DamageChatChance = TagHelper.TagIntCheck(tag, this.DamageChatChance);

                    }

                    //DamageChatMessages
                    if(tag.Contains("[DamageChatMessages") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);
                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            this.DamageChatMessages.Add(tempValue);

                        }

                    }

                    //DamageChatSounds
                    if(tag.Contains("[DamageChatSounds") == true) {

                        var tempValue = TagHelper.TagStringCheck(tag);
                        if(string.IsNullOrWhiteSpace(tempValue) == false) {

                            this.DamageChatSounds.Add(tempValue);

                        }

                    }



                }

            }

            if(this.UseTauntChat == true) {

                if(this.TauntChatMinTime > TauntChatMaxTime) {

                    this.TauntChatMinTime = this.TauntChatMaxTime;

                }

                this.SecondsUntilTaunt = Rnd.Next(this.TauntChatMinTime - this.TauntChatMaxTime);

            }

            RefreshAntennaList();

        }

        public void ProcessAutoMessages() {

            if(RAI_SessionCore.IsServer == false || this.UseChatSystem == false) {

                return;

            }

            if(this.UseGreetingChat == true && this.GreetingChatSent == false) {

                var player = TargetHelper.GetClosestPlayer(this.RemoteControl.GetPosition());

                if(player != null) {

                    if(Vector3D.Distance(player.GetPosition(), this.RemoteControl.GetPosition()) <= this.GreetingChatTriggerDistance) {

                        this.GreetingChatSent = true;

                        if(Rnd.Next(1, 101) <= this.GreetingChatChance) {

                            string msg = "";
                            string sound = "";
                            BroadcastChatToPlayers(msg, this.ChatAuthor, this.ChatAuthorColor, sound);

                        }

                    }

                }

            }

            if(this.UseTauntChat == true && this.TauntsSentCount < this.MaxTauntChats) {

                TimeSpan timePassed = MyAPIGateway.Session.GameDateTime - this.LastTauntTime;

                if(timePassed.TotalSeconds >= this.SecondsUntilTaunt) {

                    this.LastTauntTime = MyAPIGateway.Session.GameDateTime;
                    var player = TargetHelper.GetClosestPlayer(this.RemoteControl.GetPosition());

                    if(player != null) {

                        if(Vector3D.Distance(player.GetPosition(), this.RemoteControl.GetPosition()) <= this.GreetingChatTriggerDistance) {

                            if(Rnd.Next(1, 101) <= this.TauntChatChance) {

                                this.TauntsSentCount++;
                                string msg = "";
                                string sound = "";
                                BroadcastChatToPlayers(msg, this.ChatAuthor, this.ChatAuthorColor, sound);

                            }

                        }

                    }

                }

            }

        }

        private void GetHighestAntennaRange() {

            this.HighestRadius = 0;
            this.AntennaCoords = Vector3D.Zero;

            foreach(var antenna in this.AntennaList) {

                if(antenna.IsWorking == false || antenna.IsFunctional == false || antenna.IsBroadcasting == false) {

                    continue;

                }

                if(antenna.Radius > this.HighestRadius) {

                    this.HighestRadius = antenna.Radius;
                    this.AntennaCoords = antenna.GetPosition();

                }

            }

            

        }

        private void GetRandomChatAndSoundFromLists(List<string> messages, List<string> sounds, ref string message, ref string sound){

            if(messages.Count == 0) {

                return;

            }

            var index = Rnd.Next(0, messages.Count);
            message = messages[index];

            if(sounds.Count >= messages.Count) {

                sound = sounds[index];

            }

        }

		public string BroadcastChatToPlayers(string msg, string author, string color, string audio){
			
			if(this.LastChatMessageSent == msg || string.IsNullOrWhiteSpace(msg) == true){
				
				return "";
				
			}

            GetHighestAntennaRange();

            if(this.HighestRadius == 0) {

                return "";

            }



            var playerList = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(playerList);
            
			foreach(var player in playerList){
				
				if(player.IsBot == true || player.Character == null){
					
					continue;
					
				}

                if(Vector3D.Distance(player.GetPosition(), this.AntennaCoords) > this.HighestRadius) {

                    continue;

                }
				
				var modifiedMsg = msg;
				
				if(modifiedMsg.Contains("{PlayerName}") == true){
					
					modifiedMsg = modifiedMsg.Replace("{PlayerName}", player.DisplayName);
					
				}
				
				if(modifiedMsg.Contains("{PlayerFactionName}") == true){
					
					var playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
					
					if(playerFaction != null){
						
						modifiedMsg = modifiedMsg.Replace("{PlayerFactionName}", playerFaction.Name);
					
					}else{
						
						modifiedMsg = modifiedMsg.Replace("{PlayerFactionName}", "Unaffiliated");
						
					}
					
				}
				
				var authorColor = color;
				
				if(authorColor != "White" && authorColor != "Red" && authorColor != "Green" && authorColor != "Blue"){
					
					authorColor = "White";
					
				}
				
				MyVisualScriptLogicProvider.SendChatMessage(modifiedMsg, author, player.IdentityId, authorColor);
				
				if(audio != ""){
					
					if(string.IsNullOrEmpty(player.Character.Name) == true){
						
						MyVisualScriptLogicProvider.SetName(player.Character.EntityId, player.Character.EntityId.ToString());
						
					}
					
					MyVisualScriptLogicProvider.PlaySingleSoundAtEntity(audio, player.Character.EntityId.ToString());
					
				}

			}
			
			return msg;
			
		}
		
		public void BroadcastNotificationToPlayers(string msg, string author, string color, string audio, double broadcastDistance, int disappearTimeMs, Vector3D coords){
			
			var playerList = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(playerList);
			
			foreach(var player in playerList){
				
				if(player.IsBot == true || player.Character == null){
					
					continue;
					
				}
				
				var modifiedMsg = msg;
				
				if(modifiedMsg.Contains("{PlayerName}") == true){
					
					modifiedMsg = modifiedMsg.Replace("{PlayerName}", player.DisplayName);
					
				}
				
				if(modifiedMsg.Contains("{PlayerFactionName}") == true){
					
					var playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
					
					if(playerFaction != null){
						
						modifiedMsg = modifiedMsg.Replace("{PlayerFactionName}", playerFaction.Name);
					
					}else{
						
						modifiedMsg = modifiedMsg.Replace("{PlayerFactionName}", "Unaffiliated");
						
					}
					
				}
				
				var authorColor = color;
				
				if(authorColor != "White" && authorColor != "Red" && authorColor != "Green" && authorColor != "Blue"){
					
					authorColor = "White";
					
				}
				
				MyVisualScriptLogicProvider.ShowNotification(modifiedMsg, disappearTimeMs, authorColor, player.IdentityId);
				
				if(audio != ""){
					
					if(string.IsNullOrEmpty(player.Character.Name) == true){
						
						MyVisualScriptLogicProvider.SetName(player.Character.EntityId, player.Character.EntityId.ToString());
						
					}
					
					MyVisualScriptLogicProvider.PlaySingleSoundAtEntity(audio, player.Character.EntityId.ToString());
					
				}

			}
			
		}

        public void DamageChatTriggered() {



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