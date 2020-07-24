using RivalAI.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RivalAI.Sync {
	public static class ReputationAnnounceManager {

		public static List<ReputationMessage> MessageList = new List<ReputationMessage>();
		private static Dictionary<string, ReputationMessage> _messages = new Dictionary<string, ReputationMessage>();

		public static void ProcessMessage(ReputationMessage receivedMessage) {

			//Logger.MsgDebug("Process Rep Sync Msg", DebugTypeEnum.Owner);
			ReputationMessage existingMsg = null;

			if (_messages.TryGetValue(receivedMessage.ReputationFactionTarget, out existingMsg)) {

				//Logger.MsgDebug("Process Existing Rep Sync Msg", DebugTypeEnum.Owner);
				existingMsg.DisplayMessage(receivedMessage.ReputationChangeAmount);
				return;

			}

			//Logger.MsgDebug("Process New Rep Sync Msg", DebugTypeEnum.Owner);
			receivedMessage.DisplayMessage(receivedMessage.ReputationChangeAmount);
			_messages.Add(receivedMessage.ReputationFactionTarget, receivedMessage);

		}

	}

}
