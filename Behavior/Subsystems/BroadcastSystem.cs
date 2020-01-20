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
using RivalAI.Sync;

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
		public string HighestAntennaRangeName;
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
			HighestAntennaRangeName = "";
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

		public void BroadcastRequest(ChatProfile chat) {

			string message = "";
			string sound = "";
			string avatar = "";
			var broadcastType = BroadcastType.None;

			if(chat.ProcessChat(ref message, ref sound, ref broadcastType, ref avatar) == false) {

				Logger.MsgDebug(chat.ProfileSubtypeId + ": Process Chat Fail", DebugTypeEnum.Chat);
				return;

			}

			if(this.LastChatMessageSent == message || string.IsNullOrWhiteSpace(message) == true) {

				Logger.MsgDebug(chat.ProfileSubtypeId + ": Last Message Same", DebugTypeEnum.Chat);
				return;

			}

			if (chat.IgnoreAntennaRequirement) {

				this.HighestRadius = chat.IgnoredAntennaRangeOverride;
				this.HighestAntennaRangeName = "";

			} else {

				GetHighestAntennaRange();

				if (this.HighestRadius == 0) {

					Logger.MsgDebug(chat.ProfileSubtypeId + ": No Valid Antenna", DebugTypeEnum.Chat);
					return;

				}

			}

			

			var playerList = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(playerList);
			Logger.MsgDebug(chat.ProfileSubtypeId + ": Sending Chat to all Players within distance: " + this.HighestRadius.ToString(), DebugTypeEnum.Chat);

			if (message.Contains("{AntennaName}"))
				message = message.Replace("{AntennaName}", this.HighestAntennaRangeName);

			if(this.RemoteControl?.SlimBlock?.CubeGrid?.CustomName != null && message.Contains("{GridName}"))
				message = message.Replace("{GridName}", this.RemoteControl.SlimBlock.CubeGrid.CustomName);
			
			if(chat.UseRandomNameGeneratorFromMES && MESApi.MESApiReady)
				message = MESApi.ConvertRandomNamePatterns(message);
			
			var authorName = chat.Author;

			if (authorName.Contains("{AntennaName}"))
				authorName = authorName.Replace("{AntennaName}", this.HighestAntennaRangeName);

			if (this.RemoteControl?.SlimBlock?.CubeGrid?.CustomName != null && authorName.Contains("{GridName}"))
				authorName = authorName.Replace("{GridName}", this.RemoteControl.SlimBlock.CubeGrid.CustomName);


			foreach (var player in playerList) {

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

				
				var authorColor = chat.Color;

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

					var effect = new Effects();
					effect.Mode = EffectSyncMode.PlayerSound;
					effect.SoundId = sound;
					effect.AvatarId = avatar;
					var sync = new SyncContainer(effect);
					SyncManager.SendSyncMesage(sync, player.SteamUserId);

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
			this.HighestAntennaRangeName = "";

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
					this.HighestAntennaRangeName = antenna.CustomName;

				}

			}

			

		}
		/*
		private void GetRandomChatAndSoundFromLists(List<string> messages, List<string> sounds, List<BroadcastType> broadcastTypes, List<string> avatars, ref string message, ref string sound, ref BroadcastType broadcastType, ref string avatar){

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
			
			if(avatars.Count >= messages.Count) {

				avatar = avatars[index];

			}

		}
		*/
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
