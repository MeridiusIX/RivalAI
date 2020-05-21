using RivalAI.Behavior.Subsystems.Profiles;
using RivalAI.Entities;
using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Behavior.Subsystems {
	public class NewTargetingSystem {

		public IMyRemoteControl RemoteControl;
		private StoredSettings _settings;

		public ITarget Target; //The Actual Target
		public TargetProfile Data; //The Condition Data For Acquiring Target

		public bool PreviousTargetCheckResult;
		public long PreviousTargetEntityId;

		public bool CurrentTargetCheckResult;
		public long CurrentTargetEntityId;

		public bool TargetAcquired;
		public bool TargetLost;

		public DateTime LastAcquisitionTime;
		public DateTime LastRefreshTime;
		public DateTime LastEvaluationTime;

		public Vector3D TargetLastKnownCoords;

		public bool ForceRefresh;
		public long ForceTargetEntity;
		public bool TargetAlreadyEvaluated;
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
			ForceTargetEntity = 0;
			TargetAlreadyEvaluated = false;
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

			Logger.MsgDebug(string.Format("Acquiring New Target From Profile: {0}", Data.ProfileSubtypeId), DebugTypeEnum.TargetAcquisition);
			LastAcquisitionTime = MyAPIGateway.Session.GameDateTime;
			ForceRefresh = false;

			//Get Target Below
			var targetList = new List<ITarget>();

			if (ForceTargetEntity == 0) {

				if (Data.Target == TargetTypeEnum.Player) {

					Logger.MsgDebug(" - Acquiring Player Target", DebugTypeEnum.TargetAcquisition);
					targetList = AcquirePlayerTarget();

				}

				if (Data.Target == TargetTypeEnum.Block) {

					Logger.MsgDebug(" - Acquiring Block Target", DebugTypeEnum.TargetAcquisition);
					targetList = AcquireBlockTarget();

				}

				if (Data.Target == TargetTypeEnum.Grid) {

					Logger.MsgDebug(" - Acquiring Grid Target", DebugTypeEnum.TargetAcquisition);
					targetList = AcquireGridTarget();

				}

			} else {

				Logger.MsgDebug(" - Acquiring Custom Target From EntityId", DebugTypeEnum.TargetAcquisition);
				IMyEntity entity = null;

				if (MyAPIGateway.Entities.TryGetEntityById(ForceTargetEntity, out entity)) {

					var target = EntityEvaluator.GetTargetFromEntity(entity);

					if (target != null) {

						targetList.Add(target);

					}

				}
			
			}

			//Run Filters On Target List
			Logger.MsgDebug(string.Format(" - Running Evaluation On {0} Potential Targets", targetList.Count), DebugTypeEnum.TargetAcquisition);
			for (int i = targetList.Count - 1; i >= 0; i--) {

				targetList[i].RefreshSubGrids();

				if (!EvaluateTarget(targetList[i]))
					targetList.RemoveAt(i);
			
			}

			//Filter Out Factions, if Applicable
			if (Data.PrioritizeSpecifiedFactions && (Data.MatchAllFilters.Contains(TargetFilterEnum.Faction) || Data.MatchAnyFilters.Contains(TargetFilterEnum.Faction))) {

				Logger.MsgDebug(" - Filtering Potential Preferred Faction Targets", DebugTypeEnum.TargetAcquisition);
				var factionPreferred = new List<ITarget>();

				for (int i = targetList.Count - 1; i >= 0; i--) {

					if (Data.FactionTargets.Contains(targetList[i].FactionOwner()))
						factionPreferred.Add(targetList[i]);

				}

				if (factionPreferred.Count > 0) {

					targetList = factionPreferred;

				}

			}

			Logger.MsgDebug(string.Format(" - Getting Target From List Of {0} Based On {1} Sorting Rules", targetList.Count, Data.GetTargetBy), DebugTypeEnum.TargetAcquisition);
			if (targetList.Count > 0) {

				var tempTarget = GetTargetFromSorting(targetList);

				if (tempTarget != null) {

					Logger.MsgDebug(string.Format(" - Target Acquired: {0}", tempTarget.Name()), DebugTypeEnum.TargetAcquisition);
					this.Target = tempTarget;
					this.LastRefreshTime = MyAPIGateway.Session.GameDateTime;
					this.LastEvaluationTime = MyAPIGateway.Session.GameDateTime;

				} else {

					Logger.MsgDebug(string.Format(" - No Valid Target Could be Acquired."), DebugTypeEnum.TargetAcquisition);

				}
				

			}

			this.LastAcquisitionTime = MyAPIGateway.Session.GameDateTime;

		}

		public List<ITarget> AcquireBlockTarget() {

			var result = new List<ITarget>();

			foreach (var grid in GridManager.Grids) {

				if (!grid.ActiveEntity())
					continue;

				if (grid.IsSameGrid(RemoteControl.SlimBlock.CubeGrid)) {

					continue;

				}

				if (grid.Distance(RemoteControl.GetPosition()) > Data.MaxDistance)
					continue;

				grid.GetBlocks(result, Data.BlockTargets);

			}

			return result;

		}

		public List<ITarget> AcquireGridTarget() {

			var result = new List<ITarget>();

			foreach (var grid in GridManager.Grids) {

				if (!grid.ActiveEntity())
					continue;

				if (grid.IsSameGrid(RemoteControl.SlimBlock.CubeGrid)) {

					continue;

				}

				if (grid.Distance(RemoteControl.GetPosition()) > Data.MaxDistance)
					continue;

				result.Add(grid);

			}

			return result;

		}

		public List<ITarget> AcquirePlayerTarget() {

			PlayerManager.RefreshAllPlayers(true);
			var result = new List<ITarget>();

			foreach (var player in PlayerManager.Players) {

				if (!player.ActiveEntity())
					continue;

				if (player.IsSameGrid(RemoteControl.SlimBlock.CubeGrid)) {

					continue;

				}

				if (player.Distance(RemoteControl.GetPosition()) > Data.MaxDistance)
					continue;

				result.Add(player);

			}

			return result;

		}

		public void CheckForTarget() {

			PreviousTargetCheckResult = CurrentTargetCheckResult;
			PreviousTargetEntityId = CurrentTargetEntityId;

			if (HasTarget() && Data.UseTargetRefresh) {

				var refreshDuration = MyAPIGateway.Session.GameDateTime - this.LastRefreshTime;

				if (refreshDuration.TotalSeconds > Data.TimeUntilNextRefresh) {

					ForceRefresh = true;
					this.LastRefreshTime = MyAPIGateway.Session.GameDateTime;

				}

			}

			bool evaluationDone = false;

			if (!HasTarget() || ForceRefresh) {

				AcquireNewTarget();
				evaluationDone = true;

			}

			if (!evaluationDone) {
			
				var evaluationDuration = MyAPIGateway.Session.GameDateTime - this.LastEvaluationTime;

				if (evaluationDuration.TotalSeconds > Data.TimeUntilNextEvaluation) {

					EvaluateTarget(this.Target);
					this.LastEvaluationTime = MyAPIGateway.Session.GameDateTime;

				}

			}

			CurrentTargetCheckResult = HasTarget();
			CurrentTargetEntityId = Target.GetEntityId();

			if (CurrentTargetEntityId != PreviousTargetEntityId)
				TargetAcquired = true;

			if (PreviousTargetCheckResult && !CurrentTargetCheckResult)
				TargetLost = true;

		}

		public bool EvaluateTarget(ITarget target) {

			if (target == null) {

				Logger.MsgDebug("Target Is Null, Cannot Evaluate", DebugTypeEnum.TargetEvaluation);
				return false;

			}


			if (!target.ActiveEntity()) {

				Logger.MsgDebug("Target Invalid, Cannot Evaluate", DebugTypeEnum.TargetEvaluation);
				return false;

			}

			Logger.MsgDebug(string.Format(" - Evaluating Target: {0}", target.Name()), DebugTypeEnum.TargetEvaluation);

			if (!Data.BuiltUniqueFilterList) {

				foreach (var filter in Data.MatchAllFilters) {

					if (!Data.AllUniqueFilters.Contains(filter))
						Data.AllUniqueFilters.Add(filter);

				}

				foreach (var filter in Data.MatchAnyFilters) {

					if (!Data.AllUniqueFilters.Contains(filter))
						Data.AllUniqueFilters.Add(filter);

				}

				foreach (var filter in Data.MatchNoneFilters) {

					if (!Data.AllUniqueFilters.Contains(filter))
						Data.AllUniqueFilters.Add(filter);

				}

				Data.BuiltUniqueFilterList = true;

			}

			List<TargetFilterEnum> FilterHits = new List<TargetFilterEnum>();

			//Altitude
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.Altitude)) {

				var altitude = target.CurrentAltitude();

				if (altitude == -1000000 || (altitude >= Data.MinAltitude && altitude <= Data.MaxAltitude))
					FilterHits.Add(TargetFilterEnum.Altitude);

				Logger.MsgDebug(string.Format(" - Evaluated Altitude: {0}", altitude), DebugTypeEnum.TargetEvaluation);

			}

			//Broadcasting
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.Broadcasting)) {

				var range = target.BroadcastRange();
				var distance = target.Distance(RemoteControl.GetPosition());

				if (range > distance || distance < Data.NonBroadcastVisualRange)
					FilterHits.Add(TargetFilterEnum.Broadcasting);

				Logger.MsgDebug(string.Format(" - Evaluated Broadcast Range vs Distance: {0} / {1}", range, distance), DebugTypeEnum.TargetEvaluation);

			}

			//Faction
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.Faction)) {

				var faction = target.FactionOwner() ?? "";

				if (Data.PrioritizeSpecifiedFactions || Data.FactionTargets.Contains(faction))
					FilterHits.Add(TargetFilterEnum.Faction);

				Logger.MsgDebug(string.Format(" - Evaluated Faction: {0}", faction), DebugTypeEnum.TargetEvaluation);

			}

			//Gravity
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.Gravity)) {

				var gravity = target.CurrentGravity();

				if (gravity >= Data.MinGravity && gravity <= Data.MaxGravity)
					FilterHits.Add(TargetFilterEnum.Gravity);

				Logger.MsgDebug(string.Format(" - Evaluated Gravity: {0}", gravity), DebugTypeEnum.TargetEvaluation);

			}

			//OutsideOfSafezone
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.OutsideOfSafezone)) {

				bool inZone = target.InSafeZone();

				if (!inZone)
					FilterHits.Add(TargetFilterEnum.OutsideOfSafezone);

				Logger.MsgDebug(string.Format(" - Evaluated Outside Safezone: {0}", inZone), DebugTypeEnum.TargetEvaluation);

			}

			//Owner
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.Owner)) {

				var owners = target.OwnerTypes(Data.OnlyGetFromEntityOwner, Data.GetFromMinorityGridOwners);
				bool gotRelation = false;

				var values = Enum.GetValues(typeof(OwnerTypeEnum)).Cast<OwnerTypeEnum>();

				foreach (var ownerType in values) {

					if (ownerType == OwnerTypeEnum.None)
						continue;

					if (owners.HasFlag(ownerType) && Data.Owners.HasFlag(ownerType)) {

						gotRelation = true;
						break;

					}

				}

				if (gotRelation)
					FilterHits.Add(TargetFilterEnum.Owner);

				Logger.MsgDebug(string.Format(" - Evaluated Owners: Required: {0}", Data.Owners.ToString()), DebugTypeEnum.TargetEvaluation);
				Logger.MsgDebug(string.Format(" - Evaluated Owners: Found: {0}", owners.ToString()), DebugTypeEnum.TargetEvaluation);
				Logger.MsgDebug(string.Format(" - Evaluated Target Owners: {0} / Passed: {1}", owners.ToString(), gotRelation), DebugTypeEnum.TargetEvaluation);
				
			}

			//Powered
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.Powered)) {

				bool powered = target.IsPowered();

				if (powered)
					FilterHits.Add(TargetFilterEnum.Powered);

				Logger.MsgDebug(string.Format(" - Evaluated Power: {0}", powered), DebugTypeEnum.TargetEvaluation);

			}

			//Relation
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.Relation)) {

				var relations = target.RelationTypes(RemoteControl.OwnerId, Data.OnlyGetFromEntityOwner, Data.GetFromMinorityGridOwners);
				bool gotRelation = false;

				var values = Enum.GetValues(typeof(RelationTypeEnum)).Cast<RelationTypeEnum>();

				foreach (var relationType in values) {

					if (relationType == RelationTypeEnum.None)
						continue;

					if (relations.HasFlag(relationType) && Data.Relations.HasFlag(relationType)) {

						gotRelation = true;
						break;

					}

				}

				if (gotRelation)
					FilterHits.Add(TargetFilterEnum.Relation);

				Logger.MsgDebug(string.Format(" - Evaluated Relations: Required: {0}", Data.Relations.ToString()), DebugTypeEnum.TargetEvaluation);
				Logger.MsgDebug(string.Format(" - Evaluated Relations: Found: {0}", relations.ToString()), DebugTypeEnum.TargetEvaluation);
				Logger.MsgDebug(string.Format(" - Evaluated Relations: {0} / Passed: {1}", relations.ToString(), gotRelation), DebugTypeEnum.TargetEvaluation);

			}

			//Shielded
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.Shielded)) {

				bool shielded = target.ProtectedByShields();

				if (shielded)
					FilterHits.Add(TargetFilterEnum.Shielded);

				Logger.MsgDebug(string.Format(" - Evaluated Shields: {0}", shielded), DebugTypeEnum.TargetEvaluation);

			}

			//Speed
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.Speed)) {

				var speed = target.CurrentSpeed();

				if ((Data.MinSpeed < 0 || speed >= Data.MinSpeed) && (Data.MaxSpeed < 0 || speed <= Data.MaxSpeed))
					FilterHits.Add(TargetFilterEnum.Speed);

				Logger.MsgDebug(string.Format(" - Evaluated Speed: {0}", speed), DebugTypeEnum.TargetEvaluation);

			}

			//Static
			if (Data.IsStatic != CheckEnum.Ignore && Data.AllUniqueFilters.Contains(TargetFilterEnum.Static)) {

				var staticGrid = target.IsStatic();

				if ((staticGrid && Data.IsStatic == CheckEnum.Yes) || (!staticGrid && Data.IsStatic == CheckEnum.No))
					FilterHits.Add(TargetFilterEnum.Static);

				Logger.MsgDebug(string.Format(" - Evaluated Static Grid: {0}", staticGrid), DebugTypeEnum.TargetEvaluation);

			}

			//TargetValue
			if (Data.AllUniqueFilters.Contains(TargetFilterEnum.TargetValue)) {

				var targetValue = target.TargetValue();

				if (targetValue >= Data.MinTargetValue && targetValue <= Data.MaxTargetValue)
					FilterHits.Add(TargetFilterEnum.TargetValue);

				Logger.MsgDebug(string.Format(" - Evaluated Target Value: {0}", targetValue), DebugTypeEnum.TargetEvaluation);

			}

			//Any Conditions Check
			bool anyConditionPassed = false;

			if (Data.MatchAnyFilters.Count > 0) {

				foreach (var filter in Data.MatchAnyFilters) {

					if (FilterHits.Contains(filter)) {

						anyConditionPassed = true;
						break;

					}
				
				}

			} else {

				anyConditionPassed = true;

			}

			if (!anyConditionPassed) {

				Logger.MsgDebug(" - Evaluation Condition -Any- Failed", DebugTypeEnum.TargetEvaluation);
				return false;

			}
				

			//All Condition Checks
			foreach (var filter in Data.MatchAllFilters) {

				if (!FilterHits.Contains(filter)) {

					Logger.MsgDebug(" - Evaluation Condition -All- Failed", DebugTypeEnum.TargetEvaluation);
					return false;

				}

			}

			//None Condition Checks
			foreach (var filter in Data.MatchNoneFilters) {

				if (FilterHits.Contains(filter)) {

					Logger.MsgDebug(" - Evaluation Condition -None- Failed", DebugTypeEnum.TargetEvaluation);
					return false;

				}

			}

			Logger.MsgDebug(" - Evaluation Passed", DebugTypeEnum.TargetEvaluation);
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

		public ITarget GetTargetFromSorting(List<ITarget> targets) {

			//List Empty, you get null soup!
			if (targets.Count == 0)
				return null;

			//Only 1 thing in list, therefore you get the 1 thing
			if (targets.Count == 1)
				return targets[0];

			//Random - may RNGesus be generous
			if (Data.GetTargetBy == TargetSortEnum.Random) {

				return targets[Utilities.Rnd.Next(0, targets.Count)];
			
			}


			if (Data.GetTargetBy == TargetSortEnum.ClosestDistance) {

				int index = -1;
				double dist = -1;

				for (int i = 0; i < targets.Count; i++) {

					var thisDist = targets[i].Distance(RemoteControl.GetPosition());

					if (index == -1 || thisDist < dist) {

						dist = thisDist;
						index = i;

					}
				
				}

				return targets[index];

			}

			if (Data.GetTargetBy == TargetSortEnum.FurthestDistance) {

				int index = -1;
				double dist = -1;

				for (int i = 0; i < targets.Count; i++) {

					var thisDist = targets[i].Distance(RemoteControl.GetPosition());

					if (index == -1 || thisDist > dist) {

						dist = thisDist;
						index = i;

					}

				}

				return targets[index];

			}

			if (Data.GetTargetBy == TargetSortEnum.HighestTargetValue) {

				int index = -1;
				float targetValue = -1;

				for (int i = 0; i < targets.Count; i++) {

					var thisValue = targets[i].TargetValue();

					if (index == -1 || thisValue > targetValue) {

						targetValue = thisValue;
						index = i;

					}

				}

				return targets[index];

			}

			if (Data.GetTargetBy == TargetSortEnum.LowestTargetValue) {

				int index = -1;
				float targetValue = -1;

				for (int i = 0; i < targets.Count; i++) {

					var thisValue = targets[i].TargetValue();

					if (index == -1 || thisValue < targetValue) {

						targetValue = thisValue;
						index = i;

					}

				}

				return targets[index];

			}

			return null;
		
		}

		public bool HasTarget() {

			return Data.UseCustomTargeting && Target.ActiveEntity();

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
