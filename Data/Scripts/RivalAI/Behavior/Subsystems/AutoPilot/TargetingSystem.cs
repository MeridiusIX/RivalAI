﻿using RivalAI.Behavior.Subsystems.AutoPilot;
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
	public class TargetingSystem {

		public IMyRemoteControl RemoteControl;
		private IBehavior _behavior;

		public ITarget Target { 
			
			get { 

				if (OverrideTarget != null && OverrideTarget.ActiveEntity()) {

					return OverrideTarget;
				
				}

				return NormalTarget;
			}

			set {
			
				if (OverrideTarget != null && OverrideTarget.ActiveEntity()) {

					OverrideTarget = value;
					_behavior.Settings.CurrentTargetEntityId = OverrideTarget.GetEntityId();
					return;
				
				}

				NormalTarget = value;
				_behavior.Settings.CurrentTargetEntityId = NormalTarget?.GetEntityId() ?? 0;

			}
		}

		public TargetProfile Data { get { 

				if (OverrideTarget != null && OverrideTarget.ActiveEntity()) {

					//Logger.MsgDebug("Data - Use Override", DebugTypeEnum.Target);
					return OverrideData;
				
				}

				//Logger.MsgDebug("Data - Use Normal", DebugTypeEnum.Target);
				return NormalData;
			} 

		}

		public ITarget NormalTarget;
		public TargetProfile NormalData;

		public ITarget OverrideTarget;
		public TargetProfile OverrideData;

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
		public long ForceTargetEntityId;
		public IMyEntity ForceTargetEntity;
		public bool TargetAlreadyEvaluated;
		public bool UseNewTargetProfile;
		public bool ResetTimerOnProfileChange;
		public string NewTargetProfileName;

		public TargetingSystem(IMyRemoteControl remoteControl = null) {

			RemoteControl = remoteControl;

			NormalTarget = null;
			NormalData = new TargetProfile();

			OverrideTarget = null;
			OverrideData = new TargetProfile();

			LastAcquisitionTime = MyAPIGateway.Session.GameDateTime;
			LastRefreshTime = MyAPIGateway.Session.GameDateTime;
			LastEvaluationTime = MyAPIGateway.Session.GameDateTime;

			TargetLastKnownCoords = Vector3D.Zero;

			ForceRefresh = false;
			ForceTargetEntityId = 0;
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

			OverrideTarget = null;
			LastAcquisitionTime = MyAPIGateway.Session.GameDateTime;
			ForceRefresh = false;

			var data = ForceTargetEntityId == 0 ? NormalData : OverrideData;

			Logger.MsgDebug(string.Format("Acquiring New Target From Profile: {0}", data.ProfileSubtypeId), DebugTypeEnum.TargetAcquisition);

			//Get Target Below
			var targetList = new List<ITarget>();

			bool targetIsOverride = false;

			if (ForceTargetEntityId == 0) {

				if (Data.Target == TargetTypeEnum.Player || Data.Target == TargetTypeEnum.PlayerAndBlock || Data.Target == TargetTypeEnum.PlayerAndGrid) {

					Logger.MsgDebug(" - Acquiring Player Target", DebugTypeEnum.TargetAcquisition);
					AcquirePlayerTarget(targetList);

				}

				if (Data.Target == TargetTypeEnum.Block || Data.Target == TargetTypeEnum.PlayerAndBlock) {

					Logger.MsgDebug(" - Acquiring Block Target", DebugTypeEnum.TargetAcquisition);
					AcquireBlockTarget(targetList);

				}

				if (Data.Target == TargetTypeEnum.Grid || Data.Target == TargetTypeEnum.PlayerAndGrid) {

					Logger.MsgDebug(" - Acquiring Grid Target", DebugTypeEnum.TargetAcquisition);
					AcquireGridTarget(targetList);

				}

			} else {

				Logger.MsgDebug(" - Acquiring Custom Target From EntityId: " + ForceTargetEntityId, DebugTypeEnum.TargetAcquisition);

				if (ForceTargetEntity != null) {

					var target = EntityEvaluator.GetTargetFromEntity(ForceTargetEntity);

					if (target != null) {

						targetIsOverride = true;
						targetList.Add(target);

					} else {

						Logger.MsgDebug(" - Failed To Get Valid Target Entity From: " + ForceTargetEntityId, DebugTypeEnum.TargetAcquisition);

					}

				} else {

					Logger.MsgDebug(" - Failed To Parse Entity From EntityId: " + ForceTargetEntityId, DebugTypeEnum.TargetAcquisition);

				}

				ForceTargetEntityId = 0;
				ForceTargetEntity = null;

			}

			//Run Filters On Target List
			Logger.MsgDebug(string.Format(" - Running Evaluation On {0} Potential Targets", targetList.Count), DebugTypeEnum.TargetAcquisition);
			for (int i = targetList.Count - 1; i >= 0; i--) {

				targetList[i].RefreshSubGrids();

				if (!EvaluateTarget(targetList[i], data, true))
					targetList.RemoveAt(i);
			
			}

			//Filter Out Factions, if Applicable
			if (data.PrioritizeSpecifiedFactions && (data.MatchAllFilters.Contains(TargetFilterEnum.Faction) || data.MatchAnyFilters.Contains(TargetFilterEnum.Faction))) {

				Logger.MsgDebug(" - Filtering Potential Preferred Faction Targets", DebugTypeEnum.TargetAcquisition);
				var factionPreferred = new List<ITarget>();

				for (int i = targetList.Count - 1; i >= 0; i--) {

					if (data.FactionTargets.Contains(targetList[i].FactionOwner()))
						factionPreferred.Add(targetList[i]);

				}

				if (factionPreferred.Count > 0) {

					targetList = factionPreferred;

				}

			}

			//Filter Out PlayerControlled Grids, if Applicable
			if (data.PrioritizePlayerControlled && (data.MatchAllFilters.Contains(TargetFilterEnum.PlayerControlled) || data.MatchAnyFilters.Contains(TargetFilterEnum.PlayerControlled))) {

				Logger.MsgDebug(" - Filtering Potential Player Controlled Targets", DebugTypeEnum.TargetAcquisition);
				var playerControlled = new List<ITarget>();

				for (int i = targetList.Count - 1; i >= 0; i--) {

					if (targetList[i].PlayerControlled())
						playerControlled.Add(targetList[i]);

				}

				if (playerControlled.Count > 0) {

					targetList = playerControlled;

				}

			}

			Logger.MsgDebug(string.Format(" - Getting Target From List Of {0} Based On {1} Sorting Rules", targetList.Count, data.GetTargetBy), DebugTypeEnum.TargetAcquisition);
			if (targetList.Count > 0) {

				var tempTarget = GetTargetFromSorting(targetList, data);

				if (tempTarget != null) {

					Logger.MsgDebug(string.Format(" - Target Acquired: {0}", tempTarget.Name()), DebugTypeEnum.TargetAcquisition);

					if (targetIsOverride) {

						this.OverrideTarget = tempTarget;

					} else {

						this.NormalTarget = tempTarget;

					}

					
					this.LastRefreshTime = MyAPIGateway.Session.GameDateTime;
					this.LastEvaluationTime = MyAPIGateway.Session.GameDateTime;

				} else {

					Logger.MsgDebug(string.Format(" - No Valid Target Could be Acquired."), DebugTypeEnum.TargetAcquisition);

				}
				

			}

			this.LastAcquisitionTime = MyAPIGateway.Session.GameDateTime;

		}

		public void AcquireBlockTarget(List<ITarget> result) {

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

		}

		public void AcquireGridTarget(List<ITarget> result) {

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

		}

		public void AcquirePlayerTarget(List<ITarget> result) {

			PlayerManager.RefreshAllPlayers(true);
			
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

		}

		public void CheckForTarget() {

			//Logger.MsgDebug("CheckForTarget - Start", DebugTypeEnum.Target);

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

			//Logger.MsgDebug("CheckForTarget - Check For New Acquire", DebugTypeEnum.Target);

			if (!HasTarget() || ForceRefresh) {

				AcquireNewTarget();
				evaluationDone = true;

			}

			//Logger.MsgDebug("CheckForTarget - Check For Evaluation", DebugTypeEnum.Target);

			if (!evaluationDone) {
			
				var evaluationDuration = MyAPIGateway.Session.GameDateTime - this.LastEvaluationTime;

				if (evaluationDuration.TotalSeconds > Data.TimeUntilNextEvaluation) {

					if (OverrideTarget != null) {

						var evalResult = EvaluateTarget(this.Target, this.Data);
						this.LastEvaluationTime = MyAPIGateway.Session.GameDateTime;

						if (!evalResult)
							this.Target = null;

					}

					if (OverrideTarget == null) {
					
						var evalResult = EvaluateTarget(this.Target, this.Data);
						this.LastEvaluationTime = MyAPIGateway.Session.GameDateTime;

						if (!evalResult)
							this.Target = null;

					}

				}

			}

			CurrentTargetCheckResult = HasTarget();
			CurrentTargetEntityId = Target != null ? Target.GetEntityId() : 0;

			if (CurrentTargetEntityId != PreviousTargetEntityId)
				TargetAcquired = true;

			if (PreviousTargetCheckResult && !CurrentTargetCheckResult)
				TargetLost = true;

			//Logger.MsgDebug("CheckForTarget - Complete: " + RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.Target);

		}

		public bool EvaluateTarget(ITarget target, TargetProfile data, bool skipExpensiveChecks = false) {

			if (target == null) {

				Logger.MsgDebug("Target Is Null, Cannot Evaluate", DebugTypeEnum.TargetEvaluation);
				return false;

			}

			if (!target.ActiveEntity()) {

				Logger.MsgDebug("Target Invalid, Cannot Evaluate", DebugTypeEnum.TargetEvaluation);
				return false;

			}

			Logger.MsgDebug(string.Format(" - Evaluating Target: {0} using profile {1}", target.Name(), data.ProfileSubtypeId), DebugTypeEnum.TargetEvaluation);

			if (!data.BuiltUniqueFilterList) {

				foreach (var filter in data.MatchAllFilters) {

					if (!data.AllUniqueFilters.Contains(filter))
						data.AllUniqueFilters.Add(filter);

				}

				foreach (var filter in data.MatchAnyFilters) {

					if (!data.AllUniqueFilters.Contains(filter))
						data.AllUniqueFilters.Add(filter);

				}

				foreach (var filter in data.MatchNoneFilters) {

					if (!data.AllUniqueFilters.Contains(filter))
						data.AllUniqueFilters.Add(filter);

				}

				data.BuiltUniqueFilterList = true;

			}

			List<TargetFilterEnum> FilterHits = new List<TargetFilterEnum>();

			//Distance
			var distance = target.Distance(RemoteControl.GetPosition());

			if (distance > data.MaxDistance) {

				return false;
			
			}

			//Altitude
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Altitude)) {

				var altitude = target.CurrentAltitude();

				if (altitude == -1000000 || (altitude >= data.MinAltitude && altitude <= data.MaxAltitude))
					FilterHits.Add(TargetFilterEnum.Altitude);

				Logger.MsgDebug(string.Format(" - Evaluated Altitude: {0}", altitude), DebugTypeEnum.TargetEvaluation);

			}

			//Broadcasting
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Broadcasting)) {

				var range = target.BroadcastRange(data.BroadcastOnlyAntenna);
				
				if (range > distance || distance < data.NonBroadcastVisualRange)
					FilterHits.Add(TargetFilterEnum.Broadcasting);

				Logger.MsgDebug(string.Format(" - Evaluated Broadcast Range vs Distance: {0} / {1}", range, distance), DebugTypeEnum.TargetEvaluation);

			}

			//Faction
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Faction)) {

				var faction = target.FactionOwner() ?? "";

				if (data.PrioritizeSpecifiedFactions || data.FactionTargets.Contains(faction))
					FilterHits.Add(TargetFilterEnum.Faction);

				Logger.MsgDebug(string.Format(" - Evaluated Faction: {0}", faction), DebugTypeEnum.TargetEvaluation);

			}

			//Gravity
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Gravity)) {

				var gravity = target.CurrentGravity();

				if (gravity >= data.MinGravity && gravity <= data.MaxGravity)
					FilterHits.Add(TargetFilterEnum.Gravity);

				Logger.MsgDebug(string.Format(" - Evaluated Gravity: {0}", gravity), DebugTypeEnum.TargetEvaluation);

			}

			//LineOfSight
			if (!skipExpensiveChecks && data.AllUniqueFilters.Contains(TargetFilterEnum.LineOfSight) && _behavior.AutoPilot.Collision.TargetResult.HasTarget()) {
				
				bool targetMatch = (target.GetParentEntity().EntityId == _behavior.AutoPilot.Collision.TargetResult.GetCollisionEntity().EntityId);

				if (targetMatch)
					FilterHits.Add(TargetFilterEnum.MovementScore);

			}

			//MovementScore
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.MovementScore)) {

				if (distance < data.MaxMovementDetectableDistance || data.MaxMovementDetectableDistance < 0) {

					var score = target.MovementScore();

					if ((data.MinMovementScore == -1 || score >= data.MinMovementScore) && (data.MaxMovementScore == -1 || score <= data.MaxMovementScore))
						FilterHits.Add(TargetFilterEnum.MovementScore);

				}

			}

			//Name
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Name)) {

				var name = target.Name();
				string successName = "N/A";

				foreach (var allowedName in data.Names) {

					if (string.IsNullOrWhiteSpace(allowedName))
						continue;

					if (data.UsePartialNameMatching) {

						if (name.Contains(allowedName)) {

							successName = allowedName;
							break;

						}

					} else {

						if (name == allowedName) {

							successName = allowedName;
							break;

						}
					
					}
				
				}

				if(successName != "N/A")
					FilterHits.Add(TargetFilterEnum.Name);

				Logger.MsgDebug(string.Format(" - Evaluated Name: {0} // {1}", name, successName), DebugTypeEnum.TargetEvaluation);

			}

			//OutsideOfSafezone
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.OutsideOfSafezone)) {

				bool inZone = target.InSafeZone();

				if (!inZone)
					FilterHits.Add(TargetFilterEnum.OutsideOfSafezone);

				Logger.MsgDebug(string.Format(" - Evaluated Outside Safezone: {0}", !inZone), DebugTypeEnum.TargetEvaluation);

			}

			//Owner
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Owner)) {

				var owners = target.OwnerTypes(data.OnlyGetFromEntityOwner, data.GetFromMinorityGridOwners);
				bool gotRelation = false;

				var values = Enum.GetValues(typeof(OwnerTypeEnum)).Cast<OwnerTypeEnum>();

				foreach (var ownerType in values) {

					if (ownerType == OwnerTypeEnum.None)
						continue;

					if (owners.HasFlag(ownerType) && data.Owners.HasFlag(ownerType)) {

						gotRelation = true;
						break;

					}

				}

				if (gotRelation)
					FilterHits.Add(TargetFilterEnum.Owner);

				Logger.MsgDebug(string.Format(" - Evaluated Owners: Required: {0}", data.Owners.ToString()), DebugTypeEnum.TargetEvaluation);
				Logger.MsgDebug(string.Format(" - Evaluated Owners: Found: {0}", owners.ToString()), DebugTypeEnum.TargetEvaluation);
				Logger.MsgDebug(string.Format(" - Evaluated Target Owners: {0} / Passed: {1}", owners.ToString(), gotRelation), DebugTypeEnum.TargetEvaluation);
				
			}

			//PlayerControlled
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.PlayerControlled)) {

				var controlled = target.PlayerControlled();

				if (data.PrioritizePlayerControlled || controlled)
					FilterHits.Add(TargetFilterEnum.PlayerControlled);

				Logger.MsgDebug(string.Format(" - Evaluated Player Controlled: {0}", controlled), DebugTypeEnum.TargetEvaluation);

			}

			//PlayerKnownLocation
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.PlayerKnownLocation)) {

				bool inKnownLocation = false;

				if (MESApi.MESApiReady) {

					if (MESApi.IsPositionInKnownPlayerLocation(target.GetPosition(), true, string.IsNullOrWhiteSpace(data.PlayerKnownLocationFactionOverride) ? _behavior.Owner.Faction?.Tag : data.PlayerKnownLocationFactionOverride)) {

						FilterHits.Add(TargetFilterEnum.PlayerKnownLocation);
						inKnownLocation = true;

					}
						

				}

				Logger.MsgDebug(string.Format(" - Evaluated Player Known Location: {0}", inKnownLocation), DebugTypeEnum.TargetEvaluation);

			}

			//Powered
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Powered)) {

				bool powered = target.IsPowered();

				if (powered)
					FilterHits.Add(TargetFilterEnum.Powered);

				Logger.MsgDebug(string.Format(" - Evaluated Power: {0}", powered), DebugTypeEnum.TargetEvaluation);

			}

			//Relation
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Relation)) {

				var relations = target.RelationTypes(RemoteControl.OwnerId, data.OnlyGetFromEntityOwner, data.GetFromMinorityGridOwners);
				bool gotRelation = false;

				var values = Enum.GetValues(typeof(RelationTypeEnum)).Cast<RelationTypeEnum>();

				foreach (var relationType in values) {

					if (relationType == RelationTypeEnum.None)
						continue;

					if (relations.HasFlag(relationType) && data.Relations.HasFlag(relationType)) {

						gotRelation = true;
						break;

					}

				}

				if (gotRelation)
					FilterHits.Add(TargetFilterEnum.Relation);

				Logger.MsgDebug(string.Format(" - Evaluated Relations: Required: {0}", data.Relations.ToString()), DebugTypeEnum.TargetEvaluation);
				Logger.MsgDebug(string.Format(" - Evaluated Relations: Found: {0}", relations.ToString()), DebugTypeEnum.TargetEvaluation);
				Logger.MsgDebug(string.Format(" - Evaluated Relations: {0} / Passed: {1}", relations.ToString(), gotRelation), DebugTypeEnum.TargetEvaluation);

			}

			//Shielded
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Shielded)) {

				bool shielded = target.ProtectedByShields();

				if (shielded)
					FilterHits.Add(TargetFilterEnum.Shielded);

				Logger.MsgDebug(string.Format(" - Evaluated Shields: {0}", shielded), DebugTypeEnum.TargetEvaluation);

			}

			//Speed
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Speed)) {

				var speed = target.CurrentSpeed();

				if ((data.MinSpeed < 0 || speed >= data.MinSpeed) && (data.MaxSpeed < 0 || speed <= data.MaxSpeed))
					FilterHits.Add(TargetFilterEnum.Speed);

				Logger.MsgDebug(string.Format(" - Evaluated Speed: {0}", speed), DebugTypeEnum.TargetEvaluation);

			}

			//Static
			if (data.IsStatic != CheckEnum.Ignore && data.AllUniqueFilters.Contains(TargetFilterEnum.Static)) {

				var staticGrid = target.IsStatic();

				if ((staticGrid && data.IsStatic == CheckEnum.Yes) || (!staticGrid && data.IsStatic == CheckEnum.No))
					FilterHits.Add(TargetFilterEnum.Static);

				Logger.MsgDebug(string.Format(" - Evaluated Static Grid: {0}", staticGrid), DebugTypeEnum.TargetEvaluation);

			}

			//TargetValue
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.TargetValue)) {

				var targetValue = target.TargetValue();

				if ((data.MinTargetValue == -1 || targetValue >= data.MinTargetValue) && (data.MaxTargetValue == -1 || targetValue <= data.MaxTargetValue))
					FilterHits.Add(TargetFilterEnum.TargetValue);

				Logger.MsgDebug(string.Format(" - Evaluated Target Value: {0}", targetValue), DebugTypeEnum.TargetEvaluation);

			}

			//Underwater
			if (data.AllUniqueFilters.Contains(TargetFilterEnum.Underwater)) {

				bool result = false;

				if (WaterHelper.Enabled)
					result = WaterHelper.UnderwaterAndDepthCheck(target.GetPosition(), _behavior.AutoPilot.CurrentWater, true, Data.MinUnderWaterDepth, Data.MaxUnderWaterDepth);

				if (result)
					FilterHits.Add(TargetFilterEnum.Underwater);

				Logger.MsgDebug(string.Format(" - Evaluated Underwater: {0}", result), DebugTypeEnum.TargetEvaluation);

			}

			//Any Conditions Check
			bool anyConditionPassed = false;

			if (data.MatchAnyFilters.Count > 0) {

				foreach (var filter in data.MatchAnyFilters) {

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
			foreach (var filter in data.MatchAllFilters) {

				if (!FilterHits.Contains(filter)) {

					Logger.MsgDebug(" - Evaluation Condition -All- Failed", DebugTypeEnum.TargetEvaluation);
					return false;

				}

			}

			//None Condition Checks
			foreach (var filter in data.MatchNoneFilters) {

				if (FilterHits.Contains(filter)) {

					Logger.MsgDebug(" - Evaluation Condition -None- Failed", DebugTypeEnum.TargetEvaluation);
					return false;

				}

			}

			Logger.MsgDebug(" - Evaluation Passed", DebugTypeEnum.TargetEvaluation);
			TargetLastKnownCoords = target.GetPosition();
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

		public ITarget GetTargetFromSorting(List<ITarget> targets, TargetProfile data) {

			//List Empty, you get null soup!
			if (targets.Count == 0)
				return null;

			//Only 1 thing in list, therefore you get the 1 thing
			if (targets.Count == 1)
				return targets[0];

			//Random - may RNGesus be generous
			if (data.GetTargetBy == TargetSortEnum.Random) {

				return targets[Utilities.Rnd.Next(0, targets.Count)];
			
			}


			if (data.GetTargetBy == TargetSortEnum.ClosestDistance) {

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

			if (data.GetTargetBy == TargetSortEnum.FurthestDistance) {

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

			if (data.GetTargetBy == TargetSortEnum.HighestTargetValue) {

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

			if (data.GetTargetBy == TargetSortEnum.LowestTargetValue) {

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

			if (Target == null)
				return false;

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

										this.NormalData = profile;
										Logger.MsgDebug(profile.ProfileSubtypeId + " Target Profile Loaded", DebugTypeEnum.BehaviorSetup);

									}

								} catch (Exception) {



								}

							}

						}

					}

					//OverrideTargetData
					if (tag.Contains("[OverrideTargetData:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							byte[] byteData = { };

							if (TagHelper.TargetObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

								try {

									var profile = MyAPIGateway.Utilities.SerializeFromBinary<TargetProfile>(byteData);

									if (profile != null) {

										this.OverrideData = profile;
										Logger.MsgDebug(profile.ProfileSubtypeId + " Override Target Profile Loaded", DebugTypeEnum.BehaviorSetup);

									}

								} catch (Exception) {



								}

							}

						}

					}

				}

			}

		}

		public void SetTargetProfile(bool setOverride = false) {

			UseNewTargetProfile = false;
			byte[] targetProfileBytes;

			if (!TagHelper.TargetObjectTemplates.TryGetValue(NewTargetProfileName, out targetProfileBytes)) {

				Logger.MsgDebug("No Target Profile For: " + NewTargetProfileName, DebugTypeEnum.Target);
				return;

			}

			TargetProfile targetProfile;

			try {

				targetProfile = MyAPIGateway.Utilities.SerializeFromBinary<TargetProfile>(targetProfileBytes);

				if (targetProfile != null && !string.IsNullOrWhiteSpace(targetProfile.ProfileSubtypeId)) {

					if (!setOverride) {

						NormalData = targetProfile;
						_behavior.Settings.CustomTargetProfile = NewTargetProfileName;
						Logger.MsgDebug("Target Profile Switched To: " + NewTargetProfileName, DebugTypeEnum.Target);


					} else {

						OverrideData = targetProfile;
						_behavior.Settings.CustomTargetProfile = NewTargetProfileName;
						Logger.MsgDebug("Target Profile Switched To: " + NewTargetProfileName, DebugTypeEnum.Target);

					}

				}

			} catch (Exception e) {

				Logger.MsgDebug("Target Profile Exception: " + NewTargetProfileName, DebugTypeEnum.Target);
				Logger.MsgDebug(e.ToString(), DebugTypeEnum.Target);

			}

		}

		public void SetupReferences(IBehavior behavior) {

			_behavior = behavior;
		
		}

	}

}
