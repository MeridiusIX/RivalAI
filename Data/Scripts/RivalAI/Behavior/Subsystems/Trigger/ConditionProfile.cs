using ProtoBuf;
using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRageMath;


namespace RivalAI.Behavior.Subsystems.Trigger {

	public enum CounterCompareEnum {
	
		GreaterOrEqual,
		Greater,
		Equal,
		NotEqual,
		Less,
		LessOrEqual,
	
	}

	[ProtoContract]
	public class ConditionProfile {

		[ProtoMember(1)]
		public bool UseConditions;

		[ProtoMember(2)]
		public bool MatchAnyCondition;

		[ProtoMember(3)]
		public bool CheckAllLoadedModIDs;

		[ProtoMember(4)]
		public List<long> AllModIDsToCheck;

		[ProtoMember(5)]
		public bool CheckAnyLoadedModIDs;

		[ProtoMember(6)]
		public List<long> AnyModIDsToCheck;

		[ProtoMember(7)]
		public bool CheckTrueBooleans;

		[ProtoMember(8)]
		public List<string> TrueBooleans;

		[ProtoMember(9)]
		public bool CheckCustomCounters;

		[ProtoMember(10)]
		public List<string> CustomCounters;

		[ProtoMember(11)]
		public List<int> CustomCountersTargets;

		[ProtoMember(12)]
		public bool CheckGridSpeed;

		[ProtoMember(13)]
		public float MinGridSpeed;

		[ProtoMember(14)]
		public float MaxGridSpeed;

		[ProtoMember(15)]
		public bool CheckMESBlacklistedSpawnGroups;

		[ProtoMember(16)]
		public List<string> SpawnGroupBlacklistContainsAll;

		[ProtoMember(17)]
		public List<string> SpawnGroupBlacklistContainsAny;

		[ProtoMember(18)]
		public string ProfileSubtypeId;

		[ProtoMember(19)]
		public bool UseRequiredFunctionalBlocks;

		[ProtoMember(20)]
		public List<string> RequiredAllFunctionalBlockNames;

		[ProtoMember(21)]
		public List<string> RequiredAnyFunctionalBlockNames;

		[ProtoMember(22)]
		public List<string> RequiredNoneFunctionalBlockNames;

		[ProtoMember(23)]
		public bool UseAccumulatedDamageWatcher;

		[ProtoMember(24)]
		public float MinAccumulatedDamage;

		[ProtoMember(25)]
		public float MaxAccumulatedDamage;

		[ProtoMember(26)]
		public bool CheckTrueSandboxBooleans;

		[ProtoMember(27)]
		public List<string> TrueSandboxBooleans;

		[ProtoMember(28)]
		public bool CheckCustomSandboxCounters;

		[ProtoMember(29)]
		public List<string> CustomSandboxCounters;

		[ProtoMember(30)]
		public List<int> CustomSandboxCountersTargets;

		[ProtoMember(31)]
		public bool CheckTargetAltitudeDifference;

		[ProtoMember(32)]
		public double MinTargetAltitudeDifference;

		[ProtoMember(33)]
		public double MaxTargetAltitudeDifference;

		[ProtoMember(34)]
		public bool CheckTargetDistance;

		[ProtoMember(35)]
		public double MinTargetDistance;

		[ProtoMember(36)]
		public double MaxTargetDistance;

		[ProtoMember(37)]
		public bool CheckTargetAngleFromForward;

		[ProtoMember(38)]
		public double MinTargetAngle;

		[ProtoMember(39)]
		public double MaxTargetAngle;

		[ProtoMember(40)]
		public bool CheckIfTargetIsChasing;

		[ProtoMember(41)]
		public double MinTargetChaseAngle;

		[ProtoMember(42)]
		public double MaxTargetChaseAngle;

		[ProtoMember(43)]
		public List<CounterCompareEnum> CounterCompareTypes;

		[ProtoMember(44)]
		public List<CounterCompareEnum> SandboxCounterCompareTypes;

		[ProtoMember(45)]
		public bool CheckIfGridNameMatches;

		[ProtoMember(46)]
		public bool AllowPartialGridNameMatches;

		[ProtoMember(47)]
		public List<string> GridNamesToCheck;

		[ProtoMember(48)]
		public bool UnderwaterCheck;

		[ProtoMember(49)]
		public bool IsUnderwater;

		[ProtoMember(50)]
		public bool TargetUnderwaterCheck;

		[ProtoMember(51)]
		public bool TargetIsUnderwater;

		[ProtoMember(52)]
		public bool BehaviorModeCheck;

		[ProtoMember(53)]
		public BehaviorMode CurrentBehaviorMode;

		[ProtoMember(54)]
		public double MinDistanceUnderwater;

		[ProtoMember(55)]
		public double MaxDistanceUnderwater;

		[ProtoMember(56)]
		public double MinTargetDistanceUnderwater;

		[ProtoMember(57)]
		public double MaxTargetDistanceUnderwater;

		[ProtoIgnore]
		private IMyRemoteControl _remoteControl;

		[ProtoIgnore]
		private IBehavior _behavior;

		[ProtoIgnore]
		private StoredSettings _settings;

		[ProtoIgnore]
		private bool _gotWatchedBlocks;

		[ProtoIgnore]
		private List<IMyCubeBlock> _watchedAllBlocks;

		[ProtoIgnore]
		private List<IMyCubeBlock> _watchedAnyBlocks;

		[ProtoIgnore]
		private List<IMyCubeBlock> _watchedNoneBlocks;

		[ProtoIgnore]
		private bool _watchingAllBlocks;

		[ProtoIgnore]
		private bool _watchingAnyBlocks;

		[ProtoIgnore]
		private bool _watchingNoneBlocks;

		[ProtoIgnore]
		private bool _watchedAllBlocksResult;

		[ProtoIgnore]
		private bool _watchedAnyBlocksResult;

		[ProtoIgnore]
		private bool _watchedNoneBlocksResult;

		public ConditionProfile() {

			UseConditions = false;
			MatchAnyCondition = false;

			CheckAllLoadedModIDs = false;
			AllModIDsToCheck = new List<long>();

			CheckAnyLoadedModIDs = false;
			AnyModIDsToCheck = new List<long>();

			CheckTrueBooleans = false;
			TrueBooleans = new List<string>();

			CheckCustomCounters = false;
			CustomCounters = new List<string>();
			CustomCountersTargets = new List<int>();

			CheckTrueSandboxBooleans = false;
			TrueSandboxBooleans = new List<string>();

			CheckCustomSandboxCounters = false;
			CustomSandboxCounters = new List<string>();
			CustomSandboxCountersTargets = new List<int>();

			CheckGridSpeed = false;
			MinGridSpeed = -1;
			MaxGridSpeed = -1;

			CheckMESBlacklistedSpawnGroups = false;
			SpawnGroupBlacklistContainsAll = new List<string>();
			SpawnGroupBlacklistContainsAny = new List<string>();

			UseRequiredFunctionalBlocks = false;
			RequiredAllFunctionalBlockNames = new List<string>();
			RequiredAnyFunctionalBlockNames = new List<string>();
			RequiredNoneFunctionalBlockNames = new List<string>();

			UseAccumulatedDamageWatcher = false;
			MinAccumulatedDamage = -1;
			MaxAccumulatedDamage = -1;

			CheckTargetAltitudeDifference = false;
			MinTargetAltitudeDifference = 0;
			MaxTargetAltitudeDifference = 0;

			CheckTargetDistance = false;
			MinTargetDistance = -1;
			MaxTargetDistance = -1;

			CheckTargetAngleFromForward = false;
			MinTargetAngle = -1;
			MaxTargetAngle = -1;

			CheckIfTargetIsChasing = false;
			MinTargetChaseAngle = -1;
			MaxTargetChaseAngle = -1;

			CounterCompareTypes = new List<CounterCompareEnum>();
			SandboxCounterCompareTypes = new List<CounterCompareEnum>();

			CheckIfGridNameMatches = false;
			AllowPartialGridNameMatches = false;
			GridNamesToCheck = new List<string>();

			UnderwaterCheck = false;
			IsUnderwater = false;
			TargetUnderwaterCheck = false;
			TargetIsUnderwater = false;
			MinDistanceUnderwater = -1;
			MaxDistanceUnderwater = -1;
			MinTargetDistanceUnderwater = -1;
			MaxTargetDistanceUnderwater = -1;

			BehaviorModeCheck = false;
			CurrentBehaviorMode = BehaviorMode.Init;

			ProfileSubtypeId = "";

			_remoteControl = null;
			_settings = new StoredSettings();

			_gotWatchedBlocks = false;
			_watchedAllBlocks = new List<IMyCubeBlock>();
			_watchedAnyBlocks = new List<IMyCubeBlock>();
			_watchedNoneBlocks = new List<IMyCubeBlock>();
			_watchedAllBlocksResult = false;
			_watchedAnyBlocksResult = false;
			_watchedNoneBlocksResult = false;

		}

		public void SetReferences(IMyRemoteControl remoteControl, StoredSettings settings) {

			_remoteControl = remoteControl;
			_settings = settings;

		}

		public bool AreConditionsMets() {

			if (!_gotWatchedBlocks)
				SetupWatchedBlocks();

			if (UseConditions == false) {

				return true;

			}

			int usedConditions = 0;
			int satisfiedConditions = 0;

			if (_behavior == null) {

				_behavior = BehaviorManager.GetBehavior(_remoteControl);

				if (_behavior == null)
					return false;

			}

			if (CheckAllLoadedModIDs == true) {

				usedConditions++;
				bool missingMod = false;

				foreach (var mod in AllModIDsToCheck) {

					if (Utilities.ModIDs.Contains(mod) == false) {

						Logger.MsgDebug(ProfileSubtypeId + ": Mod ID Not Present", DebugTypeEnum.Condition);
						missingMod = true;
						break;

					}

				}

				if (!missingMod)
					satisfiedConditions++;

			}

			if (CheckAnyLoadedModIDs == true) {

				usedConditions++;

				foreach (var mod in AllModIDsToCheck) {

					if (Utilities.ModIDs.Contains(mod)) {

						Logger.MsgDebug(ProfileSubtypeId + ": A Mod ID was Found: " + mod.ToString(), DebugTypeEnum.Condition);
						satisfiedConditions++;
						break;

					}

				}

			}

			if (CheckTrueBooleans == true) {

				usedConditions++;
				bool failedCheck = false;

				foreach (var boolName in TrueBooleans) {

					if (!_settings.GetCustomBoolResult(boolName)) {

						Logger.MsgDebug(ProfileSubtypeId + ": Boolean Not True: " + boolName, DebugTypeEnum.Condition);
						failedCheck = true;
						break;

					}

				}

				if (!failedCheck)
					satisfiedConditions++;

			}

			if (CheckCustomCounters == true) {

				usedConditions++;
				bool failedCheck = false;

				if (CustomCounters.Count == CustomCountersTargets.Count) {

					for (int i = 0; i < CustomCounters.Count; i++) {

						try {

							var compareType = CounterCompareEnum.GreaterOrEqual;

							if (i <= CounterCompareTypes.Count - 1)
								compareType = CounterCompareTypes[i];

							if (_settings.GetCustomCounterResult(CustomCounters[i], CustomCountersTargets[i], compareType) == false) {

								Logger.MsgDebug(ProfileSubtypeId + ": Counter Amount Condition Not Satisfied: " + CustomCounters[i], DebugTypeEnum.Condition);
								failedCheck = true;
								break;

							}

						} catch (Exception e) {

							Logger.MsgDebug("Exception: ", DebugTypeEnum.Condition);
							Logger.MsgDebug(e.ToString(), DebugTypeEnum.Condition);

						}

					}

				} else {

					Logger.MsgDebug(ProfileSubtypeId + ": Counter Names and Targets List Counts Don't Match. Check Your Condition Profile", DebugTypeEnum.Condition);
					failedCheck = true;

				}

				if (!failedCheck)
					satisfiedConditions++;

			}

			if (CheckTrueSandboxBooleans == true) {

				usedConditions++;
				bool failedCheck = false;

				for (int i = 0; i < TrueSandboxBooleans.Count; i++) {

					try {

						bool output = false;
						var result = MyAPIGateway.Utilities.GetVariable(TrueSandboxBooleans[i], out output);

						if (!result || !output) {

							Logger.MsgDebug(ProfileSubtypeId + ": Sandbox Boolean False: " + TrueSandboxBooleans[i], DebugTypeEnum.Condition);
							failedCheck = true;
							break;

						}

					} catch (Exception e) {

						Logger.MsgDebug("Exception: ", DebugTypeEnum.Condition);
						Logger.MsgDebug(e.ToString(), DebugTypeEnum.Condition);

					}

				}

				if (!failedCheck)
					satisfiedConditions++;

			}

			if (CheckCustomSandboxCounters == true) {

				usedConditions++;
				bool failedCheck = false;

				if (CustomSandboxCounters.Count == CustomSandboxCountersTargets.Count) {

					for (int i = 0; i < CustomSandboxCounters.Count; i++) {

						try {

							int counter = 0;
							var result = MyAPIGateway.Utilities.GetVariable(CustomSandboxCounters[i], out counter);

							var compareType = CounterCompareEnum.GreaterOrEqual;

							if (i <= SandboxCounterCompareTypes.Count - 1)
								compareType = SandboxCounterCompareTypes[i];

							bool counterResult = false;

							if (compareType == CounterCompareEnum.GreaterOrEqual)
								counterResult = (counter >= CustomSandboxCountersTargets[i]);

							if (compareType == CounterCompareEnum.Greater)
								counterResult = (counter > CustomSandboxCountersTargets[i]);

							if (compareType == CounterCompareEnum.Equal)
								counterResult = (counter == CustomSandboxCountersTargets[i]);

							if (compareType == CounterCompareEnum.NotEqual)
								counterResult = (counter != CustomSandboxCountersTargets[i]);

							if (compareType == CounterCompareEnum.Less)
								counterResult = (counter < CustomSandboxCountersTargets[i]);

							if (compareType == CounterCompareEnum.LessOrEqual)
								counterResult = (counter <= CustomSandboxCountersTargets[i]);

							if (!result || !counterResult) {

								Logger.MsgDebug(ProfileSubtypeId + ": Sandbox Counter Amount Condition Not Satisfied: " + CustomSandboxCounters[i], DebugTypeEnum.Condition);
								failedCheck = true;
								break;

							}

						} catch (Exception e) {

							Logger.MsgDebug("Exception: ", DebugTypeEnum.Condition);
							Logger.MsgDebug(e.ToString(), DebugTypeEnum.Condition);

						}

					}

				} else {

					Logger.MsgDebug(ProfileSubtypeId + ": Sandbox Counter Names and Targets List Counts Don't Match. Check Your Condition Profile", DebugTypeEnum.Condition);
					failedCheck = true;

				}

				if (!failedCheck)
					satisfiedConditions++;

			}

			if (CheckGridSpeed == true) {

				usedConditions++;
				float speed = (float)_remoteControl.GetShipSpeed();

				if ((MinGridSpeed == -1 || speed >= MinGridSpeed) && (MaxGridSpeed == -1 || speed <= MaxGridSpeed)) {

					Logger.MsgDebug(ProfileSubtypeId + ": Grid Speed High Enough", DebugTypeEnum.Condition);
					satisfiedConditions++;

				} else {

					Logger.MsgDebug(ProfileSubtypeId + ": Grid Speed Not High Enough", DebugTypeEnum.Condition);

				}

			}

			if (MESApi.MESApiReady && CheckMESBlacklistedSpawnGroups) {

				var blackList = MESApi.GetSpawnGroupBlackList();

				if (SpawnGroupBlacklistContainsAll.Count > 0) {

					usedConditions++;
					bool failedCheck = false;

					foreach (var group in SpawnGroupBlacklistContainsAll) {

						if (blackList.Contains(group) == false) {

							Logger.MsgDebug(ProfileSubtypeId + ": A Spawngroup was not on MES BlackList: " + group, DebugTypeEnum.Condition);
							failedCheck = true;
							break;

						}

					}

					if (!failedCheck)
						satisfiedConditions++;

				}

				if (SpawnGroupBlacklistContainsAny.Count > 0) {

					usedConditions++;
					foreach (var group in SpawnGroupBlacklistContainsAll) {

						if (blackList.Contains(group)) {

							Logger.MsgDebug(ProfileSubtypeId + ": A Spawngroup was on MES BlackList: " + group, DebugTypeEnum.Condition);
							satisfiedConditions++;
							break;

						}

					}

				}

			}

			if (UseAccumulatedDamageWatcher) {

				usedConditions++;
				bool failedCheck = false;
				Logger.MsgDebug("Damage Accumulated: " + _settings.TotalDamageAccumulated, DebugTypeEnum.Condition);

				if (MinAccumulatedDamage >= 0 && _settings.TotalDamageAccumulated < MinAccumulatedDamage)
					failedCheck = true;

				if (MaxAccumulatedDamage >= 0 && _settings.TotalDamageAccumulated > MaxAccumulatedDamage)
					failedCheck = true;

				if (!failedCheck)
					satisfiedConditions++;

			}

			if (UseRequiredFunctionalBlocks) {

				if (_watchingAllBlocks) {

					usedConditions++;

					if (_watchedAllBlocksResult)
						satisfiedConditions++;

				}

				if (_watchingAnyBlocks) {

					usedConditions++;

					if (_watchedAnyBlocksResult)
						satisfiedConditions++;

				}

				if (_watchingNoneBlocks) {

					usedConditions++;

					if (_watchedNoneBlocksResult)
						satisfiedConditions++;

				}

			}

			if (CheckTargetAltitudeDifference) {

				usedConditions++;

				if (_behavior.AutoPilot.Targeting.HasTarget() && _behavior.AutoPilot.InGravity()) {

					var planetPos = _behavior.AutoPilot.CurrentPlanet.PositionComp.WorldAABB.Center;
					var targetCoreDist = _behavior.AutoPilot.Targeting.Target.Distance(planetPos);
					var myCoreDist = Vector3D.Distance(planetPos, _remoteControl.GetPosition());
					var difference = targetCoreDist - myCoreDist;

					if (difference >= this.MinTargetAltitudeDifference && difference <= this.MaxTargetAltitudeDifference)
						satisfiedConditions++;

				}
			
			}

			if (CheckTargetDistance) {

				usedConditions++;

				if (_behavior.AutoPilot.Targeting.HasTarget()) {

					var dist = _behavior.AutoPilot.Targeting.Target.Distance(_remoteControl.GetPosition());

					if ((this.MinTargetDistance == -1 || dist >= this.MinTargetDistance) && (this.MaxTargetDistance == -1 || dist <= this.MaxTargetDistance))
						satisfiedConditions++;

				}

			}

			if (CheckTargetAngleFromForward) {

				usedConditions++;

				if (_behavior.AutoPilot.Targeting.HasTarget()) {

					var dirToTarget = Vector3D.Normalize(_behavior.AutoPilot.Targeting.GetTargetCoords() - _remoteControl.GetPosition());
					var myForward = _behavior.AutoPilot.RefBlockMatrixRotation.Forward;
					var angle = VectorHelper.GetAngleBetweenDirections(dirToTarget, myForward);

					if ((this.MinTargetAngle == -1 || angle >= this.MinTargetAngle) && (this.MaxTargetAngle == -1 || angle <= this.MaxTargetAngle))
						satisfiedConditions++;

				}

			}

			if (CheckIfTargetIsChasing) {

				usedConditions++;

				if (_behavior.AutoPilot.Targeting.HasTarget()) {

					var dirFromTarget = Vector3D.Normalize(_remoteControl.GetPosition() - _behavior.AutoPilot.Targeting.GetTargetCoords());
					var targetVelocity = Vector3D.Normalize(_behavior.AutoPilot.Targeting.Target.CurrentVelocity());

					if (targetVelocity.IsValid() && targetVelocity.Length() > 0) {

						var angle = VectorHelper.GetAngleBetweenDirections(dirFromTarget, targetVelocity);

						if ((this.MinTargetChaseAngle == -1 || angle >= this.MinTargetChaseAngle) && (this.MaxTargetChaseAngle == -1 || angle <= this.MaxTargetChaseAngle))
							satisfiedConditions++;
					
					}

				}

			}

			if (CheckIfGridNameMatches) {

				usedConditions++;

				if(!string.IsNullOrWhiteSpace(_remoteControl.SlimBlock.CubeGrid.CustomName)){

					bool pass = false;

					foreach (var name in GridNamesToCheck) {

						if (AllowPartialGridNameMatches) {

							if (_remoteControl.SlimBlock.CubeGrid.CustomName.Contains(name))
								pass = true;

						} else {

							if (_remoteControl.SlimBlock.CubeGrid.CustomName == name)
								pass = true;

						}

					}

					if(pass)
						satisfiedConditions++;

				}

			}

			if (UnderwaterCheck) {

				usedConditions++;

				if (WaterHelper.UnderwaterAndDepthCheck(_remoteControl.GetPosition(), _behavior.AutoPilot.CurrentWater, IsUnderwater, MinDistanceUnderwater, MaxDistanceUnderwater))
					satisfiedConditions++;

			}

			if (TargetUnderwaterCheck) {

				usedConditions++;

				if (WaterHelper.UnderwaterAndDepthCheck(_remoteControl.GetPosition(), _behavior.AutoPilot.CurrentWater, TargetIsUnderwater, MinTargetDistanceUnderwater, MaxTargetDistanceUnderwater))
					satisfiedConditions++;

			}

			if (BehaviorModeCheck) {

				usedConditions++;

				if(_behavior.Mode == CurrentBehaviorMode)
					satisfiedConditions++;

			}

			if (MatchAnyCondition == false) {

				bool result = satisfiedConditions >= usedConditions;
				Logger.MsgDebug(ProfileSubtypeId + ": All Condition Satisfied: " + result.ToString(), DebugTypeEnum.Condition);
				Logger.MsgDebug(string.Format("Used Conditions: {0} // Satisfied Conditions: {1}", usedConditions, satisfiedConditions), DebugTypeEnum.Condition);
				return result;

			} else {

				bool result = satisfiedConditions > 0;
				Logger.MsgDebug(ProfileSubtypeId + ": Any Condition(s) Satisfied: " + result.ToString(), DebugTypeEnum.Condition);
				Logger.MsgDebug(string.Format("Used Conditions: {0} // Satisfied Conditions: {1}", usedConditions, satisfiedConditions), DebugTypeEnum.Condition);
				return result;

			}

		}

		

		private void SetupWatchedBlocks() {

			Logger.MsgDebug("Setting Up Required Block Watcher", DebugTypeEnum.Condition);
			_gotWatchedBlocks = true;
			_watchedAnyBlocks.Clear();
			_watchedAllBlocks.Clear();
			_watchedNoneBlocks.Clear();

			if (!UseRequiredFunctionalBlocks)
				return;

			_remoteControl.SlimBlock.CubeGrid.OnGridSplit += GridSplitHandler;
			var allBlocks = TargetHelper.GetAllBlocks(_remoteControl?.SlimBlock?.CubeGrid).Where(x => x.FatBlock != null);

			foreach (var block in allBlocks) {

				var terminalBlock = block.FatBlock as IMyTerminalBlock;

				if (terminalBlock == null)
					continue;

				Logger.MsgDebug(" - " + terminalBlock.CustomName.Trim(), DebugTypeEnum.Condition);

				if (RequiredAllFunctionalBlockNames.Contains(terminalBlock.CustomName.Trim())) {

					Logger.MsgDebug("Monitoring Required-All Block: " + terminalBlock.CustomName, DebugTypeEnum.Condition);
					_watchedAllBlocks.Add(block.FatBlock);
					block.FatBlock.IsWorkingChanged += CheckAllBlocks;
					_watchingAllBlocks = true;

				}

				if (RequiredAnyFunctionalBlockNames.Contains(terminalBlock.CustomName.Trim())) {

					Logger.MsgDebug("Monitoring Required-Any Block: " + terminalBlock.CustomName, DebugTypeEnum.Condition);
					_watchedAnyBlocks.Add(block.FatBlock);
					block.FatBlock.IsWorkingChanged += CheckAnyBlocks;
					_watchingAnyBlocks = true;

				}

				if (RequiredNoneFunctionalBlockNames.Contains(terminalBlock.CustomName.Trim())) {

					Logger.MsgDebug("Monitoring Required-None Block: " + terminalBlock.CustomName, DebugTypeEnum.Condition);
					_watchedNoneBlocks.Add(block.FatBlock);
					block.FatBlock.IsWorkingChanged += CheckNoneBlocks;
					_watchingNoneBlocks = true;

				}

			}

			CheckAllBlocks();
			CheckAnyBlocks();
			CheckNoneBlocks();

		}

		private void CheckAllBlocks(IMyCubeBlock cubeBlock = null) {

			for (int i = _watchedAllBlocks.Count - 1; i >= 0; i--) {

				var block = _watchedAllBlocks[i];

				if (block == null || block?.SlimBlock?.CubeGrid == null || block.SlimBlock.CubeGrid.MarkedForClose) {

					_watchedAllBlocks.RemoveAt(i);
					continue;

				}

				if (!block.IsWorking || !block.IsFunctional) {

					_watchedAllBlocksResult = false;
					return;

				}

			}

			_watchedAllBlocksResult = true;

		}

		private void CheckAnyBlocks(IMyCubeBlock cubeBlock = null) {

			for (int i = _watchedAnyBlocks.Count - 1; i >= 0; i--) {

				var block = _watchedAnyBlocks[i];

				if (block == null || block?.SlimBlock?.CubeGrid == null || block.SlimBlock.CubeGrid.MarkedForClose) {

					_watchedAnyBlocks.RemoveAt(i);
					continue;

				}

				if (block.IsWorking && block.IsFunctional) {

					_watchedAnyBlocksResult = true;
					return;

				}

			}

			_watchedAnyBlocksResult = false;

		}

		private void CheckNoneBlocks(IMyCubeBlock cubeBlock = null) {

			for (int i = _watchedNoneBlocks.Count - 1; i >= 0; i--) {

				var block = _watchedNoneBlocks[i];

				if (block == null || block?.SlimBlock?.CubeGrid == null || block.SlimBlock.CubeGrid.MarkedForClose) {

					_watchedNoneBlocks.RemoveAt(i);
					continue;

				}

				if (block.IsWorking && block.IsFunctional) {

					_watchedNoneBlocksResult = false;
					return;

				}

			}

			_watchedNoneBlocksResult = true;

		}

		private void GridSplitHandler(IMyCubeGrid gridA, IMyCubeGrid gridB) {

			gridA.OnGridSplit -= GridSplitHandler;
			gridB.OnGridSplit -= GridSplitHandler;

			if (_remoteControl == null || _remoteControl?.SlimBlock?.CubeGrid == null || _remoteControl.SlimBlock.CubeGrid.MarkedForClose)
				return;

			_remoteControl.SlimBlock.CubeGrid.OnGridSplit += GridSplitHandler;

			for (int i = _watchedAllBlocks.Count - 1; i >= 0; i--) {

				var block = _watchedAllBlocks[i];

				if (block == null || !MyAPIGateway.Entities.Exist(block?.SlimBlock?.CubeGrid)) {

					_watchedAllBlocks.RemoveAt(i);
					continue;

				}

				if (!_remoteControl.SlimBlock.CubeGrid.IsSameConstructAs(block.SlimBlock.CubeGrid)) {

					_watchedAllBlocks.RemoveAt(i);
					continue;

				}

			}

			for (int i = _watchedAnyBlocks.Count - 1; i >= 0; i--) {

				var block = _watchedAnyBlocks[i];

				if (block == null || !MyAPIGateway.Entities.Exist(block?.SlimBlock?.CubeGrid)) {

					_watchedAnyBlocks.RemoveAt(i);
					continue;

				}

				if (!_remoteControl.SlimBlock.CubeGrid.IsSameConstructAs(block.SlimBlock.CubeGrid)) {

					_watchedAnyBlocks.RemoveAt(i);
					continue;

				}

			}

			for (int i = _watchedNoneBlocks.Count - 1; i >= 0; i--) {

				var block = _watchedNoneBlocks[i];

				if (block == null || !MyAPIGateway.Entities.Exist(block?.SlimBlock?.CubeGrid)) {

					_watchedNoneBlocks.RemoveAt(i);
					continue;

				}

				if (!_remoteControl.SlimBlock.CubeGrid.IsSameConstructAs(block.SlimBlock.CubeGrid)) {

					_watchedNoneBlocks.RemoveAt(i);
					continue;

				}

			}

			CheckAllBlocks();
			CheckAnyBlocks();
			CheckNoneBlocks();

		}

		public void InitTags(string customData) {

			if (string.IsNullOrWhiteSpace(customData) == false) {

				var descSplit = customData.Split('\n');

				foreach (var tag in descSplit) {

					//UseConditions
					if (tag.Contains("[UseConditions:") == true) {

						UseConditions = TagHelper.TagBoolCheck(tag);

					}

					//MatchAnyCondition
					if (tag.Contains("[MatchAnyCondition:") == true) {

						MatchAnyCondition = TagHelper.TagBoolCheck(tag);

					}

					//CheckAllLoadedModIDs
					if (tag.Contains("[CheckAllLoadedModIDs:") == true) {

						CheckAllLoadedModIDs = TagHelper.TagBoolCheck(tag);

					}

					//AllModIDsToCheck
					if (tag.Contains("[AllModIDsToCheck:") == true) {

						var tempValue = TagHelper.TagLongCheck(tag, 0);

						if (tempValue != 0) {

							AllModIDsToCheck.Add(tempValue);

						}

					}

					//CheckAnyLoadedModIDs
					if (tag.Contains("[CheckAnyLoadedModIDs:") == true) {

						CheckAnyLoadedModIDs = TagHelper.TagBoolCheck(tag);

					}

					//AnyModIDsToCheck
					if (tag.Contains("[AnyModIDsToCheck:") == true) {

						var tempValue = TagHelper.TagLongCheck(tag, 0);

						if (tempValue != 0) {

							AnyModIDsToCheck.Add(tempValue);

						}

					}

					//CheckTrueBooleans
					if (tag.Contains("[CheckTrueBooleans:") == true) {

						CheckTrueBooleans = TagHelper.TagBoolCheck(tag);

					}

					//TrueBooleans
					if (tag.Contains("[TrueBooleans:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							TrueBooleans.Add(tempValue);

						}

					}

					//CheckCustomCounters
					if (tag.Contains("[CheckCustomCounters:") == true) {

						CheckCustomCounters = TagHelper.TagBoolCheck(tag);

					}

					//CustomCounters
					if (tag.Contains("[CustomCounters:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							CustomCounters.Add(tempValue);

						}

					}

					//CustomCountersTargets
					if (tag.Contains("[CustomCountersTargets:") == true) {

						var tempValue = TagHelper.TagIntCheck(tag, 0);

						if (tempValue != 0) {

							CustomCountersTargets.Add(tempValue);

						}

					}

					//CheckTrueSandboxBooleans
					if (tag.Contains("[CheckTrueSandboxBooleans:") == true) {

						CheckTrueSandboxBooleans = TagHelper.TagBoolCheck(tag);

					}

					//TrueSandboxBooleans
					if (tag.Contains("[TrueSandboxBooleans:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							TrueSandboxBooleans.Add(tempValue);

						}

					}

					//CheckCustomSandboxCounters
					if (tag.Contains("[CheckCustomSandboxCounters:") == true) {

						CheckCustomSandboxCounters = TagHelper.TagBoolCheck(tag);

					}

					//CustomSandboxCounters
					if (tag.Contains("[CustomSandboxCounters:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							CustomSandboxCounters.Add(tempValue);

						}

					}

					//CustomSandboxCountersTargets
					if (tag.Contains("[CustomSandboxCountersTargets:") == true) {

						var tempValue = TagHelper.TagIntCheck(tag, 0);

						if (tempValue != 0) {

							CustomSandboxCountersTargets.Add(tempValue);

						}

					}

					//CheckGridSpeed
					if (tag.Contains("[CheckGridSpeed:") == true) {

						CheckGridSpeed = TagHelper.TagBoolCheck(tag);

					}

					//MinGridSpeed
					if (tag.Contains("[MinGridSpeed:") == true) {

						MinGridSpeed = TagHelper.TagFloatCheck(tag, MinGridSpeed);

					}

					//MaxGridSpeed
					if (tag.Contains("[MaxGridSpeed:") == true) {

						MaxGridSpeed = TagHelper.TagFloatCheck(tag, MaxGridSpeed);

					}

					//CheckMESBlacklistedSpawnGroups
					if (tag.Contains("[CheckMESBlacklistedSpawnGroups:") == true) {

						CheckMESBlacklistedSpawnGroups = TagHelper.TagBoolCheck(tag);

					}

					//SpawnGroupBlacklistContainsAll
					if (tag.Contains("[SpawnGroupBlacklistContainsAll:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							SpawnGroupBlacklistContainsAll.Add(tempValue);

						}

					}

					//SpawnGroupBlacklistContainsAny
					if (tag.Contains("[SpawnGroupBlacklistContainsAny:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							SpawnGroupBlacklistContainsAny.Add(tempValue);

						}

					}

					//UseRequiredFunctionalBlocks
					if (tag.Contains("[UseRequiredFunctionalBlocks:") == true) {

						UseRequiredFunctionalBlocks = TagHelper.TagBoolCheck(tag);

					}

					//RequiredAllFunctionalBlockNames
					if (tag.Contains("[RequiredAllFunctionalBlockNames:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							RequiredAllFunctionalBlockNames.Add(tempValue);

						}

					}

					//RequiredAnyFunctionalBlockNames
					if (tag.Contains("[RequiredAnyFunctionalBlockNames:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							RequiredAnyFunctionalBlockNames.Add(tempValue);

						}

					}

					//RequiredNoneFunctionalBlockNames
					if (tag.Contains("[RequiredNoneFunctionalBlockNames:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							RequiredNoneFunctionalBlockNames.Add(tempValue);

						}

					}

					//UseAccumulatedDamageWatcher
					if (tag.Contains("[UseAccumulatedDamageWatcher:") == true) {

						UseAccumulatedDamageWatcher = TagHelper.TagBoolCheck(tag);

					}

					//MinAccumulatedDamage
					if (tag.Contains("[MinAccumulatedDamage:") == true) {

						MinAccumulatedDamage = TagHelper.TagFloatCheck(tag, MinAccumulatedDamage);

					}

					//MaxAccumulatedDamage
					if (tag.Contains("[MaxAccumulatedDamage:") == true) {

						MaxAccumulatedDamage = TagHelper.TagFloatCheck(tag, MaxAccumulatedDamage);

					}

					//CheckTargetAltitudeDifference
					if (tag.Contains("[CheckTargetAltitudeDifference:") == true) {

						CheckTargetAltitudeDifference = TagHelper.TagBoolCheck(tag);

					}

					//MinTargetAltitudeDifference
					if (tag.Contains("[MinTargetAltitudeDifference:") == true) {

						MinTargetAltitudeDifference = TagHelper.TagDoubleCheck(tag, MinTargetAltitudeDifference);

					}

					//MaxTargetAltitudeDifference
					if (tag.Contains("[MaxTargetAltitudeDifference:") == true) {

						MaxTargetAltitudeDifference = TagHelper.TagDoubleCheck(tag, MaxTargetAltitudeDifference);

					}

					//CheckTargetDistance
					if (tag.Contains("[CheckTargetDistance:") == true) {

						CheckTargetDistance = TagHelper.TagBoolCheck(tag);

					}

					//MinTargetDistance
					if (tag.Contains("[MinTargetDistance:") == true) {

						MinTargetDistance = TagHelper.TagDoubleCheck(tag, MinTargetDistance);

					}

					//MaxTargetDistance
					if (tag.Contains("[MaxTargetDistance:") == true) {

						MaxTargetDistance = TagHelper.TagDoubleCheck(tag, MaxTargetDistance);

					}

					//CheckTargetAngleFromForward
					if (tag.Contains("[CheckTargetAngleFromForward:") == true) {

						CheckTargetAngleFromForward = TagHelper.TagBoolCheck(tag);

					}

					//MinTargetAngle
					if (tag.Contains("[MinTargetAngle:") == true) {

						MinTargetAngle = TagHelper.TagDoubleCheck(tag, MinTargetAngle);

					}

					//MaxTargetAngle
					if (tag.Contains("[MaxTargetAngle:") == true) {

						MaxTargetAngle = TagHelper.TagDoubleCheck(tag, MaxTargetAngle);

					}

					//CheckIfTargetIsChasing
					if (tag.Contains("[CheckIfTargetIsChasing:") == true) {

						CheckIfTargetIsChasing = TagHelper.TagBoolCheck(tag);

					}

					//MinTargetChaseAngle
					if (tag.Contains("[MinTargetChaseAngle:") == true) {

						MinTargetChaseAngle = TagHelper.TagDoubleCheck(tag, MinTargetChaseAngle);

					}

					//MaxTargetChaseAngle
					if (tag.Contains("[MaxTargetChaseAngle:") == true) {

						MaxTargetChaseAngle = TagHelper.TagDoubleCheck(tag, MaxTargetChaseAngle);

					}

					//CounterCompareTypes
					if (tag.Contains("[CounterCompareTypes:") == true) {

						var tempValue = TagHelper.TagCounterCompareEnumCheck(tag);
						CounterCompareTypes.Add(tempValue);

					}

					//SandboxCounterCompareTypes
					if (tag.Contains("[SandboxCounterCompareTypes:") == true) {

						var tempValue = TagHelper.TagCounterCompareEnumCheck(tag);
						SandboxCounterCompareTypes.Add(tempValue);

					}

					//CheckIfGridNameMatches
					if (tag.Contains("[CheckIfGridNameMatches:") == true) {

						CheckIfGridNameMatches = TagHelper.TagBoolCheck(tag);

					}

					//AllowPartialGridNameMatches
					if (tag.Contains("[AllowPartialGridNameMatches:") == true) {

						AllowPartialGridNameMatches = TagHelper.TagBoolCheck(tag);

					}

					//GridNamesToCheck
					if (tag.Contains("[GridNamesToCheck:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);
						GridNamesToCheck.Add(tempValue);

					}

					//UnderwaterCheck
					if (tag.Contains("[UnderwaterCheck:") == true) {

						UnderwaterCheck = TagHelper.TagBoolCheck(tag);

					}

					//IsUnderwater
					if (tag.Contains("[IsUnderwater:") == true) {

						IsUnderwater = TagHelper.TagBoolCheck(tag);

					}

					//TargetUnderwaterCheck
					if (tag.Contains("[TargetUnderwaterCheck:") == true) {

						TargetUnderwaterCheck = TagHelper.TagBoolCheck(tag);

					}

					//TargetIsUnderwater
					if (tag.Contains("[TargetIsUnderwater:") == true) {

						TargetIsUnderwater = TagHelper.TagBoolCheck(tag);

					}

					//MinDistanceUnderwater
					if (tag.Contains("[MinDistanceUnderwater:") == true) {

						MinDistanceUnderwater = TagHelper.TagDoubleCheck(tag, MinDistanceUnderwater);

					}

					//MaxDistanceUnderwater
					if (tag.Contains("[MaxDistanceUnderwater:") == true) {

						MaxDistanceUnderwater = TagHelper.TagDoubleCheck(tag, MaxDistanceUnderwater);

					}

					//MinTargetDistanceUnderwater
					if (tag.Contains("[MinTargetDistanceUnderwater:") == true) {

						MinTargetDistanceUnderwater = TagHelper.TagDoubleCheck(tag, MinTargetDistanceUnderwater);

					}

					//MaxTargetDistanceUnderwater
					if (tag.Contains("[MaxTargetDistanceUnderwater:") == true) {

						MaxTargetDistanceUnderwater = TagHelper.TagDoubleCheck(tag, MaxTargetDistanceUnderwater);

					}

					//BehaviorModeCheck
					if (tag.Contains("[BehaviorModeCheck:") == true) {

						BehaviorModeCheck = TagHelper.TagBoolCheck(tag);

					}

					//CurrentBehaviorMode
					if (tag.Contains("[CurrentBehaviorMode:") == true) {

						CurrentBehaviorMode = TagHelper.TagBehaviorModeEnumCheck(tag);

					}

				}

			}

		}

	}

}


