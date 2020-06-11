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
using RivalAI.Behavior.Settings;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;


namespace RivalAI.Behavior.Subsystems.Profiles{
	
	[ProtoContract]
	public class ConditionProfile{
		
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

		[ProtoIgnore]
		private IMyRemoteControl _remoteControl;

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

		public ConditionProfile(){
			
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
		
		public void SetReferences(IMyRemoteControl remoteControl, StoredSettings settings){
			
			_remoteControl = remoteControl;
			_settings = settings;

		}
		
		public bool AreConditionsMets(){

			if (!_gotWatchedBlocks)
				SetupWatchedBlocks();

			if(this.UseConditions == false){
				
				return true;
				
			}

			int usedConditions = 0;
			int satisfiedConditions = 0;
			
			if(this.CheckAllLoadedModIDs == true){
				
				usedConditions++;
				bool missingMod = false;

				foreach (var mod in this.AllModIDsToCheck) {

					if (Utilities.ModIDs.Contains(mod) == false) {

						Logger.MsgDebug(this.ProfileSubtypeId + ": Mod ID Not Present", DebugTypeEnum.Condition);
						missingMod = true;
						break;

					}

				}

				if (!missingMod)
					satisfiedConditions++;
				
			}

			if (this.CheckAnyLoadedModIDs == true) {

				usedConditions++;

				foreach (var mod in this.AllModIDsToCheck) {

					if (Utilities.ModIDs.Contains(mod)) {

						Logger.MsgDebug(this.ProfileSubtypeId + ": A Mod ID was Found: " + mod.ToString(), DebugTypeEnum.Condition);
						satisfiedConditions++;
						break;

					}

				}

			}

			if (this.CheckTrueBooleans == true){
				
				usedConditions++;
				bool failedCheck = false;

				foreach (var boolName in this.TrueBooleans) {

					if (!_settings.GetCustomBoolResult(boolName)) {

						Logger.MsgDebug(this.ProfileSubtypeId + ": Boolean Not True: " + boolName, DebugTypeEnum.Condition);
						failedCheck = true;
						break;

					}

				}

				if(!failedCheck)
					satisfiedConditions++;

			}
			
			if(this.CheckCustomCounters == true){

				usedConditions++;
				bool failedCheck = false;

				if (this.CustomCounters.Count == this.CustomCountersTargets.Count) {

					for (int i = 0; i < this.CustomCounters.Count; i++) {

						try {

							if (_settings.GetCustomCounterResult(this.CustomCounters[i], this.CustomCountersTargets[i]) == false) {

								Logger.MsgDebug(this.ProfileSubtypeId + ": Counter Amount Not High Enough: " + this.CustomCounters[i], DebugTypeEnum.Condition);
								failedCheck = true;
								break;

							}

						} catch (Exception e) {

							Logger.MsgDebug("Exception: ", DebugTypeEnum.Condition);
							Logger.MsgDebug(e.ToString(), DebugTypeEnum.Condition);

						}

					}

				} else {

					Logger.MsgDebug(this.ProfileSubtypeId + ": Counter Names and Targets List Counts Don't Match. Check Your Condition Profile", DebugTypeEnum.Condition);
					failedCheck = true;

				}

				if (!failedCheck)
					satisfiedConditions++;

			}

			if (this.CheckTrueSandboxBooleans == true) {

				usedConditions++;
				bool failedCheck = false;

				for (int i = 0; i < this.TrueSandboxBooleans.Count; i++) {

					try {

						bool output = false;
						var result = MyAPIGateway.Utilities.GetVariable<bool>(this.TrueSandboxBooleans[i], out output);

						if (!result || !output) {

							Logger.MsgDebug(this.ProfileSubtypeId + ": Sandbox Boolean False: " + this.TrueSandboxBooleans[i], DebugTypeEnum.Condition);
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

			if (this.CheckCustomSandboxCounters == true) {

				usedConditions++;
				bool failedCheck = false;

				if (this.CustomCounters.Count == this.CustomCountersTargets.Count) {

					for (int i = 0; i < this.CustomCounters.Count; i++) {

						try {

							int counter = 0;
							var result = MyAPIGateway.Utilities.GetVariable<int>(this.CustomCounters[i], out counter);

							if (!result || counter < this.CustomCountersTargets[i]) {

								Logger.MsgDebug(this.ProfileSubtypeId + ": Sandbox Counter Amount Not High Enough: " + this.CustomSandboxCounters[i], DebugTypeEnum.Condition);
								failedCheck = true;
								break;

							}

						} catch (Exception e) {

							Logger.MsgDebug("Exception: ", DebugTypeEnum.Condition);
							Logger.MsgDebug(e.ToString(), DebugTypeEnum.Condition);

						}

					}

				} else {

					Logger.MsgDebug(this.ProfileSubtypeId + ": Sandbox Counter Names and Targets List Counts Don't Match. Check Your Condition Profile", DebugTypeEnum.Condition);
					failedCheck = true;

				}

				if (!failedCheck)
					satisfiedConditions++;

			}

			if (this.CheckGridSpeed == true){
				
				usedConditions++;
				float speed = (float)_remoteControl.GetShipSpeed();

				if ((this.MinGridSpeed == -1 || speed >= this.MinGridSpeed) && (this.MaxGridSpeed == -1 || speed <= this.MaxGridSpeed)) {

					Logger.MsgDebug(this.ProfileSubtypeId + ": Grid Speed High Enough", DebugTypeEnum.Condition);
					satisfiedConditions++;

				} else {

					Logger.MsgDebug(this.ProfileSubtypeId + ": Grid Speed Not High Enough", DebugTypeEnum.Condition);

				}
				
			}

			if (MESApi.MESApiReady && this.CheckMESBlacklistedSpawnGroups) {

				var blackList = MESApi.GetSpawnGroupBlackList();

				if (this.SpawnGroupBlacklistContainsAll.Count > 0) {

					usedConditions++;
					bool failedCheck = false;

					foreach (var group in this.SpawnGroupBlacklistContainsAll) {

						if (blackList.Contains(group) == false) {

							Logger.MsgDebug(this.ProfileSubtypeId + ": A Spawngroup was not on MES BlackList: " + group, DebugTypeEnum.Condition);
							failedCheck = true;
							break;

						}

					}

					if (!failedCheck)
						satisfiedConditions++;

				}

				if (this.SpawnGroupBlacklistContainsAny.Count > 0) {

					usedConditions++;
					foreach (var group in this.SpawnGroupBlacklistContainsAll) {

						if (blackList.Contains(group)) {

							Logger.MsgDebug(this.ProfileSubtypeId + ": A Spawngroup was on MES BlackList: " + group, DebugTypeEnum.Condition);
							satisfiedConditions++;
							break;

						}

					}

				}

			}

			if (UseAccumulatedDamageWatcher) {

				usedConditions++;
				bool failedCheck = false;

				if (this.MinAccumulatedDamage >= 0 && this.MinAccumulatedDamage < _settings.TotalDamageAccumulated)
					failedCheck = true;

				if (this.MaxAccumulatedDamage >= 0 && this.MaxAccumulatedDamage > _settings.TotalDamageAccumulated)
					failedCheck = true;

				if(!failedCheck)
					satisfiedConditions++;

			}

			if (UseRequiredFunctionalBlocks) {

				if (_watchingAllBlocks) {

					usedConditions++;

					if(_watchedAllBlocksResult)
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
			
			if(this.MatchAnyCondition == false){

				bool result = (satisfiedConditions >= usedConditions);
				Logger.MsgDebug(this.ProfileSubtypeId + ": All Condition Satisfied: " + result.ToString(), DebugTypeEnum.Condition);
				Logger.MsgDebug(string.Format("Used Conditions: {0} // Satisfied Conditions: {1}", usedConditions, satisfiedConditions), DebugTypeEnum.Condition);
				return result;
				
			}else{

				bool result = (satisfiedConditions > 0);
				Logger.MsgDebug(this.ProfileSubtypeId + ": Any Condition(s) Satisfied: " + result.ToString(), DebugTypeEnum.Condition);
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

				if (this.RequiredAllFunctionalBlockNames.Contains(terminalBlock.CustomName.Trim())) {

					Logger.MsgDebug("Monitoring Required-All Block: " + terminalBlock.CustomName, DebugTypeEnum.Condition);
					_watchedAllBlocks.Add(block.FatBlock);
					block.FatBlock.IsWorkingChanged += CheckAllBlocks;
					_watchingAllBlocks = true;

				}

				if (this.RequiredAnyFunctionalBlockNames.Contains(terminalBlock.CustomName.Trim())) {

					Logger.MsgDebug("Monitoring Required-Any Block: " + terminalBlock.CustomName, DebugTypeEnum.Condition);
					_watchedAnyBlocks.Add(block.FatBlock);
					block.FatBlock.IsWorkingChanged += CheckAnyBlocks;
					_watchingAnyBlocks = true;

				}

				if (this.RequiredNoneFunctionalBlockNames.Contains(terminalBlock.CustomName.Trim())) {

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

				if (block == null || !MyAPIGateway.Entities.Exist(block?.SlimBlock?.CubeGrid)) {

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

				if (block == null || !MyAPIGateway.Entities.Exist(block?.SlimBlock?.CubeGrid)) {

					_watchedAnyBlocks.RemoveAt(i);
					continue;

				}

				if (block.IsWorking || block.IsFunctional) {

					_watchedAnyBlocksResult = true;
					return;

				}

			}

			_watchedAnyBlocksResult = false;

		}

		private void CheckNoneBlocks(IMyCubeBlock cubeBlock = null) {

			for (int i = _watchedNoneBlocks.Count - 1; i >= 0; i--) {

				var block = _watchedNoneBlocks[i];

				if (block == null || !MyAPIGateway.Entities.Exist(block?.SlimBlock?.CubeGrid)) {

					_watchedNoneBlocks.RemoveAt(i);
					continue;

				}

				if (block.IsWorking || block.IsFunctional) {

					_watchedNoneBlocksResult = false;
					return;

				}

			}

			_watchedNoneBlocksResult = true;

		}

		private void GridSplitHandler(IMyCubeGrid gridA, IMyCubeGrid gridB) {

			gridA.OnGridSplit -= GridSplitHandler;
			gridB.OnGridSplit -= GridSplitHandler;

			if (_remoteControl == null || !MyAPIGateway.Entities.Exist(_remoteControl?.SlimBlock?.CubeGrid))
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

		public void InitTags(string customData){

			if(string.IsNullOrWhiteSpace(customData) == false) {

				var descSplit = customData.Split('\n');

				foreach(var tag in descSplit) {

					//UseConditions
					if(tag.Contains("[UseConditions:") == true) {

						this.UseConditions = TagHelper.TagBoolCheck(tag);

					}

					//MatchAnyCondition
					if(tag.Contains("[MatchAnyCondition:") == true) {

						this.MatchAnyCondition = TagHelper.TagBoolCheck(tag);

					}

					//CheckAllLoadedModIDs
					if(tag.Contains("[CheckAllLoadedModIDs:") == true) {

						this.CheckAllLoadedModIDs = TagHelper.TagBoolCheck(tag);

					}

					//AllModIDsToCheck
					if (tag.Contains("[AllModIDsToCheck:") == true) {

						var tempValue = TagHelper.TagLongCheck(tag, 0);

						if (tempValue != 0) {

							this.AllModIDsToCheck.Add(tempValue);

						}

					}

					//CheckAnyLoadedModIDs
					if (tag.Contains("[CheckAnyLoadedModIDs:") == true) {

						this.CheckAnyLoadedModIDs = TagHelper.TagBoolCheck(tag);

					}

					//AnyModIDsToCheck
					if (tag.Contains("[AnyModIDsToCheck:") == true) {

						var tempValue = TagHelper.TagLongCheck(tag, 0);

						if (tempValue != 0) {

							this.AnyModIDsToCheck.Add(tempValue);

						}

					}

					//CheckTrueBooleans
					if (tag.Contains("[CheckTrueBooleans:") == true) {

						this.CheckTrueBooleans = TagHelper.TagBoolCheck(tag);

					}

					//TrueBooleans
					if (tag.Contains("[TrueBooleans:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							this.TrueBooleans.Add(tempValue);

						}

					}

					//CheckCustomCounters
					if (tag.Contains("[CheckCustomCounters:") == true) {

						this.CheckCustomCounters = TagHelper.TagBoolCheck(tag);

					}

					//CustomCounters
					if (tag.Contains("[CustomCounters:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							this.CustomCounters.Add(tempValue);

						}

					}

					//CustomCountersTargets
					if (tag.Contains("[CustomCountersTargets:") == true) {

						var tempValue = TagHelper.TagIntCheck(tag, 0);

						if (tempValue != 0) {

							this.CustomCountersTargets.Add(tempValue);

						}

					}

					//CheckTrueSandboxBooleans
					if (tag.Contains("[CheckTrueSandboxBooleans:") == true) {

						this.CheckTrueSandboxBooleans = TagHelper.TagBoolCheck(tag);

					}

					//TrueSandboxBooleans
					if (tag.Contains("[TrueSandboxBooleans:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							this.TrueSandboxBooleans.Add(tempValue);

						}

					}

					//CheckCustomSandboxCounters
					if (tag.Contains("[CheckCustomSandboxCounters:") == true) {

						this.CheckCustomSandboxCounters = TagHelper.TagBoolCheck(tag);

					}

					//CustomSandboxCounters
					if (tag.Contains("[CustomSandboxCounters:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							this.CustomSandboxCounters.Add(tempValue);

						}

					}

					//CustomSandboxCountersTargets
					if (tag.Contains("[CustomSandboxCountersTargets:") == true) {

						var tempValue = TagHelper.TagIntCheck(tag, 0);

						if (tempValue != 0) {

							this.CustomSandboxCountersTargets.Add(tempValue);

						}

					}

					//CheckGridSpeed
					if (tag.Contains("[CheckGridSpeed:") == true) {

						this.CheckGridSpeed = TagHelper.TagBoolCheck(tag);

					}

					//MinGridSpeed
					if(tag.Contains("[MinGridSpeed:") == true) {

						this.MinGridSpeed = TagHelper.TagFloatCheck(tag, this.MinGridSpeed);

					}

					//MaxGridSpeed
					if(tag.Contains("[MaxGridSpeed:") == true) {

						this.MaxGridSpeed = TagHelper.TagFloatCheck(tag, this.MaxGridSpeed);

					}

					//CheckMESBlacklistedSpawnGroups
					if (tag.Contains("[CheckMESBlacklistedSpawnGroups:") == true) {

						this.CheckMESBlacklistedSpawnGroups = TagHelper.TagBoolCheck(tag);

					}

					//SpawnGroupBlacklistContainsAll
					if (tag.Contains("[SpawnGroupBlacklistContainsAll:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							this.SpawnGroupBlacklistContainsAll.Add(tempValue);

						}

					}

					//SpawnGroupBlacklistContainsAny
					if (tag.Contains("[SpawnGroupBlacklistContainsAny:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							this.SpawnGroupBlacklistContainsAny.Add(tempValue);

						}

					}

					//UseRequiredFunctionalBlocks
					if (tag.Contains("[UseRequiredFunctionalBlocks:") == true) {

						this.UseRequiredFunctionalBlocks = TagHelper.TagBoolCheck(tag);

					}

					//RequiredAllFunctionalBlockNames
					if (tag.Contains("[RequiredAllFunctionalBlockNames:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							this.RequiredAllFunctionalBlockNames.Add(tempValue);

						}

					}

					//RequiredAnyFunctionalBlockNames
					if (tag.Contains("[RequiredAnyFunctionalBlockNames:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							this.RequiredAnyFunctionalBlockNames.Add(tempValue);

						}

					}

					//RequiredNoneFunctionalBlockNames
					if (tag.Contains("[RequiredNoneFunctionalBlockNames:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							this.RequiredNoneFunctionalBlockNames.Add(tempValue);

						}

					}

				}

			}
			
		}
		
	}
	
}
	
	
