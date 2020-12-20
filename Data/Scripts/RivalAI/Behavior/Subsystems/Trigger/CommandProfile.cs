using System;
using System.Collections.Generic;
using System.Text;

namespace RivalAI.Behavior.Subsystems.Trigger {
	public class CommandProfile {

		public string ProfileSubtypeId;
		public string CommandCode;

		public bool SendTargetEntityId;
		public bool SendDamagerEntityId;


		public CommandProfile() {

			ProfileSubtypeId = "";
			CommandCode = "";

		}

	}

}
