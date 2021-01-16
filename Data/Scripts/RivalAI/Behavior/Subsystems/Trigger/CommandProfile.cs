using RivalAI.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RivalAI.Behavior.Subsystems.Trigger {
	public class CommandProfile {

		public string ProfileSubtypeId;
		public string CommandCode;

		public bool SingleRecipient;
		public bool IgnoreAntennaRequirement;
		public double Radius;
		public double MaxRadius;

		public bool SendTargetEntityId;
		public bool SendDamagerEntityId;
		public bool SendWaypoint;

		public bool MatchSenderReceiverOwners;

		public string Waypoint;

		public CommandProfile() {

			ProfileSubtypeId = "";
			CommandCode = "";

			SingleRecipient = false;
			IgnoreAntennaRequirement = false;
			Radius = 10000;
			MaxRadius = -1;

			SendTargetEntityId = false;
			SendDamagerEntityId = false;
			SendWaypoint = false;

			MatchSenderReceiverOwners = true;

			Waypoint = "";

		}

		public void InitTags(string tagData) {

			if (!string.IsNullOrWhiteSpace(tagData)) {

				var descSplit = tagData.Split('\n');

				foreach (var tag in descSplit) {

					//CommandCode
					if (tag.Contains("[CommandCode:") == true) {

						this.CommandCode = TagHelper.TagStringCheck(tag);

					}

					//SingleRecipient
					if (tag.Contains("[SingleRecipient:") == true) {

						this.SingleRecipient = TagHelper.TagBoolCheck(tag);

					}

					//IgnoreAntennaRequirement
					if (tag.Contains("[IgnoreAntennaRequirement:") == true) {

						this.IgnoreAntennaRequirement = TagHelper.TagBoolCheck(tag);

					}

					//Radius
					if (tag.Contains("[Radius:") == true) {

						this.Radius = TagHelper.TagDoubleCheck(tag, this.Radius);

					}

					//MaxRadius
					if (tag.Contains("[MaxRadius:") == true) {

						this.MaxRadius = TagHelper.TagDoubleCheck(tag, this.MaxRadius);

					}

					//SendTargetEntityId
					if (tag.Contains("[SendTargetEntityId:") == true) {

						this.SendTargetEntityId = TagHelper.TagBoolCheck(tag);

					}

					//SendDamagerEntityId
					if (tag.Contains("[SendDamagerEntityId:") == true) {

						this.SendDamagerEntityId = TagHelper.TagBoolCheck(tag);

					}

					//SendWaypoint
					if (tag.Contains("[SendWaypoint:") == true) {

						this.SendWaypoint = TagHelper.TagBoolCheck(tag);

					}

					//MatchSenderReceiverOwners
					if (tag.Contains("[MatchSenderReceiverOwners:") == true) {

						this.MatchSenderReceiverOwners = TagHelper.TagBoolCheck(tag);

					}

					//Waypoint
					if (tag.Contains("[Waypoint:") == true) {

						this.Waypoint = TagHelper.TagStringCheck(tag);

					}

				}

			}

		}

	}

}
