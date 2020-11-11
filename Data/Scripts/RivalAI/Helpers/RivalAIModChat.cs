using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RivalAI.Helpers {

	//Create an object of RivalAIModChat, then Register it (LoadData, BeforeStart, First Tick, Etc)
	//Register a Method to the `MessageReceived` Action in the object you create.
	//Remember to Unregister at UnloadData

	public class RivalAIModChat {

		public Action<RivalAIModChatMessage> MessageReceived; //Register Your Method To This.

		public long ModId = 20435439250001; //Mod ID + 0001 since this is a fairly unique API addition.

		public void Register() {

			MyAPIGateway.Utilities.RegisterMessageHandler(20435439250001, ModMessageReceive);
		
		}

		public void ModMessageReceive(object payload) {

			try {

				var data = payload as byte[];

				if (data == null)
					return;

				var messageData = MyAPIGateway.Utilities.SerializeFromBinary<RivalAIModChatMessage>(data);

				if (messageData == null)
					return;

				MessageReceived?.Invoke(messageData);

			} catch (Exception e) {

				//Add Logging Here If You Want If Message Receiver Crashes;
			
			}

		}

		public void Unregister() {

			MyAPIGateway.Utilities.UnregisterMessageHandler(20435439250001, ModMessageReceive);

		}

	}

	[ProtoContract]
	public class RivalAIModChatMessage {

		[ProtoMember(1)]
		public string Author;

		[ProtoMember(2)]
		public string Message;

		[ProtoMember(3)]
		public long RecipientPlayerId;

		[ProtoMember(4)]
		public RivalAIModChatType MessageType;

		public RivalAIModChatMessage() {

			Author = "";
			Message = "";
			RecipientPlayerId = 0;
			MessageType = RivalAIModChatType.None;

		}

		public RivalAIModChatMessage(string author, string msg, long playerId, RivalAIModChatType type) {

			Author = author;
			Message = msg;
			RecipientPlayerId = playerId;
			MessageType = type;

		}

	}

	public enum RivalAIModChatType {
	
		None,
		Chat,
		Notification
	
	}

}
