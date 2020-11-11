using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RivalAI.Behavior.Subsystems {
	public class DiagnosticSystem {

		private bool _diagnosticsReady = false;

		private IMyRemoteControl _remoteControl;
		private IBehavior _behavior;

		private List<string> _events;

		public DiagnosticSystem(IBehavior behavior, IMyRemoteControl remoteControl) {

			_remoteControl = remoteControl;
			_behavior = behavior;

			if (remoteControl == null || behavior == null)
				return;

			_diagnosticsReady = true;
			_events = new List<string>();

			//TODO: Add Log Beginning For Behavior


		}

		public void LogEvent(string text, DebugTypeEnum eventType) {

			if (!Logger.LoggerDebugMode)
				return;

			var sb = new StringBuilder();
			sb.Append("[").Append(DateTime.Now.ToString("yyyyMMddHHmmssfff")).Append("] ");
			sb.Append("[").Append(eventType.ToString()).Append("] ");
			sb.Append(text).AppendLine();

		}

		private void AddEventToLogger(string text) {

			lock (_events)
				_events.Add(text);

		}

	}

}
