using RivalAI.Behavior.Subsystems.Profiles;
using RivalAI.Entities;
using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace RivalAI.Behavior.Subsystems {
	public class NewTargetingSystem {

		public IMyRemoteControl RemoteControl;
		private StoredSettings _settings;

		public ITarget Target; //The Actual Target
		public TargetProfile Data; //The Condition Data For Acquiring Target

		public DateTime LastAcquisitionTime;
		public DateTime LastRefreshTime;
		public DateTime LastEvaluationTime;

		public Vector3D TargetLastKnownCoords;

		public bool ForceRefresh;
		public bool UseNewTargetProfile;
		public bool ResetTimerOnProfileChange;
		public string NewTargetProfileName;

		public NewTargetingSystem(IMyRemoteControl remoteControl = null) {

			RemoteControl = remoteControl;

			Target = new PlayerEntity(null);
			Data = new TargetProfile();

			LastAcquisitionTime = MyAPIGateway.Session.GameDateTime;
			LastRefreshTime = MyAPIGateway.Session.GameDateTime;
			LastEvaluationTime = MyAPIGateway.Session.GameDateTime;

			TargetLastKnownCoords = Vector3D.Zero;

			ForceRefresh = false;
			UseNewTargetProfile = false;
			ResetTimerOnProfileChange = false;
			NewTargetProfileName = "";

		}

		public void AcquireNewTarget() {

			bool skipTimerCheck = false;

			if (UseNewTargetProfile) {

				SetTargetProfile();
				LastAcquisitionTime = MyAPIGateway.Session.GameDateTime;

				if (!ResetTimerOnProfileChange) {

					skipTimerCheck = true;

				}

			}

			if (!Data.UseCustomTargeting)
				return;

			if (!skipTimerCheck && !ForceRefresh) {

				var timespan = MyAPIGateway.Session.GameDateTime - LastAcquisitionTime;

				if (timespan.TotalSeconds < Data.TimeUntilTargetAcquisition)
					return;

			}

			LastAcquisitionTime = MyAPIGateway.Session.GameDateTime;
			ForceRefresh = false;

		}

		public void AcquireBlockTarget() {

			var result = new List<ITarget>();

		}

		public void AcquirePlayerTarget() {

			var result = new List<ITarget>();

		}

		public void CheckForTarget() {

			if (!HasTarget() || ForceRefresh) {
			
				
			
			}
		
		}

		public bool EvaluateTarget(ITarget target) {

			return true;
		
		}



		public Vector3D GetTargetCoords() {

			if (HasTarget()) {

				TargetLastKnownCoords = Target.GetPosition();
				return TargetLastKnownCoords;

			}

			if (Data.UseTargetLastKnownPosition) {

				return TargetLastKnownCoords;

			}

			return Vector3D.Zero;

		}

		public bool HasTarget() {

			return Data.UseCustomTargeting && Target.ValidEntity() && !Target.IsClosed();

		}

		public void InitTags() {

			if (!string.IsNullOrWhiteSpace(this.RemoteControl.CustomData)) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach (var tag in descSplit) {

					//TargetData
					if (tag.Contains("[TargetData:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							byte[] byteData = { };

							if (TagHelper.TargetObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

								try {

									var profile = MyAPIGateway.Utilities.SerializeFromBinary<TargetProfile>(byteData);

									if (profile != null) {

										this.Data = profile;

									}

								} catch (Exception) {



								}

							}

						}

					}

				}

			}

		}

		public void SetTargetProfile() {

			UseNewTargetProfile = false;
			byte[] targetProfileBytes;

			if (!TagHelper.TargetObjectTemplates.TryGetValue(NewTargetProfileName, out targetProfileBytes))
				return;

			TargetProfile targetProfile;

			try {

				targetProfile = MyAPIGateway.Utilities.SerializeFromBinary<TargetProfile>(targetProfileBytes);

				if (targetProfile != null && !string.IsNullOrWhiteSpace(targetProfile.ProfileSubtypeId)) {

					Data = targetProfile;
					_settings.CustomTargetProfile = NewTargetProfileName;

				}

			} catch (Exception e) {



			}

		}

		public void SetupReferences(StoredSettings settings) {

			_settings = settings;
		
		}

	}

}
