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
using RivalAI.Helpers;
using RivalAI.Behavior.Subsystems.AutoPilot;

namespace RivalAI.Behavior.Subsystems.Trigger {

	[ProtoContract]
	public class ActionProfile {

		[ProtoMember(1)]
		public bool UseChatBroadcast;

		[ProtoMember(2)]
		public ChatProfile ChatDataDefunct;

		[ProtoMember(3)]
		public bool BarrelRoll;

		[ProtoMember(4)]
		public bool Strafe;

		[ProtoMember(5)]
		public bool ChangeAutopilotSpeed;

		[ProtoMember(6)]
		public float NewAutopilotSpeed;

		[ProtoMember(7)]
		public bool SpawnEncounter;

		[ProtoMember(8)]
		public SpawnProfile SpawnerDefunct;

		[ProtoMember(9)]
		public bool SelfDestruct;

		[ProtoMember(10)]
		public bool Retreat;

		[ProtoMember(11)]
		public bool BroadcastCurrentTarget;

		[ProtoMember(12)]
		public bool BroadcastDamagerTarget;

		[ProtoMember(13)]
		public string BroadcastSendCode;

		[ProtoMember(14)]
		public bool SwitchToReceivedTarget;

		[ProtoMember(15)]
		public bool SwitchToBehavior;

		[ProtoMember(16)]
		public string NewBehavior;

		[ProtoMember(17)]
		public bool PreserveSettingsOnBehaviorSwitch;

		[ProtoMember(18)]
		public bool RefreshTarget;

		[ProtoMember(19)]
		public bool SwitchTargetProfile; //Obsolete

		[ProtoMember(20)]
		public string NewTargetProfile; //Obsolete

		[ProtoMember(21)]
		public bool TriggerTimerBlocks;

		[ProtoMember(22)]
		public List<string> TimerBlockNames;

		[ProtoMember(23)]
		public bool ChangeReputationWithPlayers;

		[ProtoMember(24)]
		public double ReputationChangeRadius;

		[ProtoMember(25)]
		public List<int> ReputationChangeAmount;

		[ProtoMember(26)]
		public bool ActivateAssertiveAntennas;

		[ProtoMember(27)]
		public bool ChangeAntennaOwnership;

		[ProtoMember(28)]
		public string AntennaFactionOwner;

		[ProtoMember(29)]
		public bool CreateKnownPlayerArea;

		[ProtoMember(30)]
		public double KnownPlayerAreaRadius;

		[ProtoMember(31)]
		public int KnownPlayerAreaTimer;

		[ProtoMember(32)]
		public int KnownPlayerAreaMaxSpawns;

		[ProtoMember(33)]
		public bool DamageToolAttacker;

		[ProtoMember(34)]
		public float DamageToolAttackerAmount;

		[ProtoMember(35)]
		public string DamageToolAttackerParticle;

		[ProtoMember(36)]
		public string DamageToolAttackerSound;

		[ProtoMember(37)]
		public bool PlayParticleEffectAtRemote;

		[ProtoMember(38)]
		public string ParticleEffectId;

		[ProtoMember(39)]
		public Vector3D ParticleEffectOffset;

		[ProtoMember(40)]
		public float ParticleEffectScale;

		[ProtoMember(41)]
		public float ParticleEffectMaxTime;

		[ProtoMember(42)]
		public Vector3D ParticleEffectColor;

		[ProtoMember(43)]
		public List<string> SetBooleansTrue;

		[ProtoMember(44)]
		public List<string> SetBooleansFalse;

		[ProtoMember(45)]
		public List<string> IncreaseCounters;

		[ProtoMember(46)]
		public List<string> DecreaseCounters;

		[ProtoMember(47)]
		public List<string> ResetCounters;

		[ProtoMember(48)]
		public List<string> SetSandboxBooleansTrue;

		[ProtoMember(49)]
		public List<string> SetSandboxBooleansFalse;

		[ProtoMember(50)]
		public List<string> IncreaseSandboxCounters;

		[ProtoMember(51)]
		public List<string> DecreaseSandboxCounters;

		[ProtoMember(52)]
		public List<string> ResetSandboxCounters;

		[ProtoMember(53)]
		public bool ChangeAttackerReputation;

		[ProtoMember(54)]
		public List<string> ChangeAttackerReputationFaction;

		[ProtoMember(55)]
		public List<int> ChangeAttackerReputationAmount;

		[ProtoMember(56)]
		public List<string> ReputationChangeFactions;

		[ProtoMember(57)]
		public bool ReputationChangesForAllRadiusPlayerFactionMembers;

		[ProtoMember(58)]
		public bool ReputationChangesForAllAttackPlayerFactionMembers;

		[ProtoMember(59)]
		public string ProfileSubtypeId;

		[ProtoMember(60)]
		public bool BroadcastGenericCommand;

		[ProtoMember(61)]
		public bool BehaviorSpecificEventA;

		[ProtoMember(62)]
		public bool BehaviorSpecificEventB;

		[ProtoMember(63)]
		public bool BehaviorSpecificEventC;

		[ProtoMember(64)]
		public bool BehaviorSpecificEventD;

		[ProtoMember(65)]
		public bool BehaviorSpecificEventE;

		[ProtoMember(66)]
		public bool BehaviorSpecificEventF;

		[ProtoMember(67)]
		public bool BehaviorSpecificEventG;

		[ProtoMember(68)]
		public bool BehaviorSpecificEventH;

		[ProtoMember(69)]
		public bool TerminateBehavior;

		[ProtoMember(70)]
		public bool ChangeTargetProfile;

		[ProtoMember(71)]
		public string NewTargetProfileId; //Implement

		[ProtoMember(72)]
		public bool PreserveTriggersOnBehaviorSwitch;

		[ProtoMember(73)]
		public bool PreserveTargetDataOnBehaviorSwitch;

		[ProtoMember(74)]
		public bool ChangeBlockNames;

		[ProtoMember(75)]
		public List<string> ChangeBlockNamesFrom;

		[ProtoMember(76)]
		public List<string> ChangeBlockNamesTo;

		[ProtoMember(77)]
		public bool ChangeAntennaRanges;

		[ProtoMember(78)]
		public List<string> AntennaNamesForRangeChange;

		[ProtoMember(79)]
		public string AntennaRangeChangeType;

		[ProtoMember(80)]
		public float AntennaRangeChangeAmount;

		[ProtoMember(81)]
		public bool ForceDespawn;

		[ProtoMember(82)]
		public bool ResetCooldownTimeOfTriggers;

		[ProtoMember(83)]
		public List<string> ResetTriggerCooldownNames;

		[ProtoMember(84)]
		public bool ChangeInertiaDampeners;

		[ProtoMember(85)]
		public bool InertiaDampenersEnable;

		[ProtoMember(86)]
		public bool CreateWeatherAtPosition;

		[ProtoMember(87)]
		public string WeatherSubtypeId;

		[ProtoMember(88)]
		public double WeatherRadius;

		[ProtoMember(89)]
		public bool StaggerWarheadDetonation;

		[ProtoMember(90)]
		public bool EnableTriggers;

		[ProtoMember(91)]
		public List<string> EnableTriggerNames;

		[ProtoMember(92)]
		public bool DisableTriggers;

		[ProtoMember(93)]
		public List<string> DisableTriggerNames;

		[ProtoMember(94)]
		public bool ChangeRotationDirection;

		[ProtoMember(95)]
		public Direction RotationDirection;

		[ProtoMember(96)]
		public int WeatherDuration;

		[ProtoMember(97)]
		public bool GenerateExplosion;

		[ProtoMember(98)]
		public Vector3D ExplosionOffsetFromRemote;

		[ProtoMember(99)]
		public int ExplosionDamage;

		[ProtoMember(100)]
		public int ExplosionRange;

		[ProtoMember(101)]
		public CheckEnum GridEditable;

		[ProtoMember(102)]
		public CheckEnum SubGridsEditable;

		[ProtoMember(103)]
		public CheckEnum GridDestructible;

		[ProtoMember(104)]
		public CheckEnum SubGridsDestructible;

		[ProtoMember(105)]
		public bool RecolorGrid;

		[ProtoMember(106)]
		public bool RecolorSubGrids;

		[ProtoMember(107)]
		public List<Vector3D> OldBlockColors;

		[ProtoMember(108)]
		public List<Vector3D> NewBlockColors;

		[ProtoMember(109)]
		public List<string> NewBlockSkins;

		[ProtoMember(110)]
		public bool ExplosionIgnoresVoxels;

		[ProtoMember(111)]
		public bool ChangeBlockOwnership;

		[ProtoMember(112)]
		public List<string> OwnershipBlockNames;

		[ProtoMember(113)]
		public List<string> OwnershipBlockFactions;

		[ProtoMember(114)]
		public bool ChangeBlockDamageMultipliers;

		[ProtoMember(115)]
		public List<string> DamageMultiplierBlockNames;

		[ProtoMember(116)]
		public List<int> DamageMultiplierValues;

		[ProtoMember(117)]
		public int KnownPlayerAreaMinThreatForAvoidingAbandonment;

		[ProtoMember(118)]
		public bool RazeBlocksWithNames;

		[ProtoMember(119)]
		public List<string> RazeBlocksNames;

		[ProtoMember(120)]
		public bool ManuallyActivateTrigger;

		[ProtoMember(121)]
		public List<string> ManuallyActivatedTriggerNames;

		[ProtoMember(122)]
		public bool SendCommandWithoutAntenna;

		[ProtoMember(123)]
		public double SendCommandWithoutAntennaRadius;

		[ProtoMember(124)]
		public bool SwitchToDamagerTarget;

		[ProtoMember(125)]
		public List<ChatProfile> ChatData;

		[ProtoMember(126)]
		public List<SpawnProfile> Spawner;

		[ProtoMember(127)]
		public bool RemoveKnownPlayerArea;

		[ProtoMember(128)]
		public bool RemoveAllKnownPlayerAreas;

		[ProtoMember(129)]
		public int Chance;

		[ProtoMember(130)]
		public bool EnableBlocks;

		[ProtoMember(131)]
		public List<string> EnableBlockNames;

		[ProtoMember(132)]
		public List<SwitchEnum> EnableBlockStates;

		[ProtoMember(133)]
		public bool ChangeAutopilotProfile;

		[ProtoMember(134)]
		public AutoPilotDataMode AutopilotProfile;

		[ProtoMember(135)]
		public bool Ramming;

		[ProtoMember(136)]
		public bool CreateRandomLightning;

		[ProtoMember(137)]
		public bool CreateLightningAtAttacker;

		[ProtoMember(138)]
		public int LightningDamage;

		[ProtoMember(139)]
		public int LightningExplosionRadius;

		[ProtoMember(140)]
		public Vector3D LightningColor;

		[ProtoMember(141)]
		public double LightningMinDistance;

		[ProtoMember(142)]
		public double LightningMaxDistance;

		[ProtoMember(143)]
		public bool CreateLightningAtTarget;

		[ProtoMember(144)]
		public int SelfDestructTimerPadding;

		[ProtoMember(145)]
		public int SelfDestructTimeBetweenBlasts;

		[ProtoMember(146)]
		public List<string> SetCounters;

		[ProtoMember(147)]
		public List<string> SetSandboxCounters;

		[ProtoMember(148)]
		public List<int> SetCountersValues;

		[ProtoMember(149)]
		public List<int> SetSandboxCountersValues;

		[ProtoMember(150)]
		public bool InheritLastAttackerFromCommand;

		[ProtoMember(151)]
		public bool ChangePlayerCredits;

		[ProtoMember(152)]
		public long ChangePlayerCreditsAmount;

		[ProtoMember(153)]
		public bool ChangeNpcFactionCredits;

		[ProtoMember(154)]
		public long ChangeNpcFactionCreditsAmount;

		[ProtoMember(155)]
		public string ChangeNpcFactionCreditsTag;

		[ProtoMember(156)]
		public bool BuildProjectedBlocks;

		[ProtoMember(157)]
		public int MaxProjectedBlocksToBuild;

		[ProtoMember(158)]
		public bool ForceManualTriggerActivation;

		[ProtoMember(159)]
		public bool OverwriteAutopilotProfile;

		[ProtoMember(160)]
		public AutoPilotDataMode OverwriteAutopilotMode;

		[ProtoMember(161)]
		public string OverwriteAutopilotId;

		[ProtoMember(162)]
		public bool SwitchTargetToDamager;

		[ProtoMember(163)]
		public bool BroadcastCommandProfiles;

		[ProtoMember(164)]
		public List<string> CommandProfileIds;

		[ProtoMember(165)]
		public bool AddWaypointFromCommand;

		[ProtoMember(166)]
		public bool RecalculateDespawnCoords;

		[ProtoMember(168)]
		public bool AddDatapadsToSeats;

		[ProtoMember(169)]
		public List<string> DatapadNamesToAdd;

		[ProtoMember(170)]
		public int DatapadCountToAdd;

		[ProtoMember(171)]
		public bool ToggleBlocksOfType;

		[ProtoMember(172)]
		public List<SerializableDefinitionId> BlockTypesToToggle;

		[ProtoMember(173)]
		public List<SwitchEnum> BlockTypeToggles;

		[ProtoMember(174)]
		public bool CancelWaitingAtWaypoint;

		[ProtoMember(175)]
		public bool SwitchToNextWaypoint;

		public ActionProfile() {

			UseChatBroadcast = false;
			ChatData = new List<ChatProfile>();
			ChatDataDefunct = new ChatProfile();

			BarrelRoll = false;
			Strafe = false;

			ChangeAutopilotSpeed = false;
			NewAutopilotSpeed = 0;

			SpawnEncounter = false;
			Spawner = new List<SpawnProfile>();
			SpawnerDefunct = new SpawnProfile();

			SelfDestruct = false;
			StaggerWarheadDetonation = false;

			Retreat = false;

			BroadcastCurrentTarget = false;
			BroadcastDamagerTarget = false;
			BroadcastSendCode = "";
			SwitchToReceivedTarget = false;
			SwitchTargetToDamager = false;

			SwitchToBehavior = false;
			NewBehavior = "";
			PreserveSettingsOnBehaviorSwitch = false;
			PreserveTriggersOnBehaviorSwitch = false;
			PreserveTargetDataOnBehaviorSwitch = false;

			RefreshTarget = false;

			TriggerTimerBlocks = false;
			TimerBlockNames = new List<string>();

			ChangeReputationWithPlayers = false;
			ReputationChangeRadius = 0;
			ReputationChangeFactions = new List<string>();
			ReputationChangeAmount = new List<int>();
			ReputationChangesForAllRadiusPlayerFactionMembers = false;

			ChangeAttackerReputation = false;
			ChangeAttackerReputationFaction = new List<string>();
			ChangeAttackerReputationAmount = new List<int>();
			ReputationChangesForAllAttackPlayerFactionMembers = false;

			ActivateAssertiveAntennas = false;

			ChangeAntennaOwnership = false;
			AntennaFactionOwner = "Nobody";

			CreateKnownPlayerArea = false;
			KnownPlayerAreaRadius = 10000;
			KnownPlayerAreaTimer = 30;
			KnownPlayerAreaMaxSpawns = -1;

			DamageToolAttacker = false;
			DamageToolAttackerAmount = 90;
			DamageToolAttackerParticle = "";
			DamageToolAttackerSound = "";

			PlayParticleEffectAtRemote = false;
			ParticleEffectId = "";
			ParticleEffectOffset = Vector3D.Zero;
			ParticleEffectScale = 1;
			ParticleEffectMaxTime = -1;
			ParticleEffectColor = Vector3D.Zero;

			SetBooleansTrue = new List<string>();
			SetBooleansFalse = new List<string>();
			IncreaseCounters = new List<string>();
			DecreaseCounters = new List<string>();
			ResetCounters = new List<string>();
			SetCounters = new List<string>();
			SetCountersValues = new List<int>();

			SetSandboxBooleansTrue = new List<string>();
			SetSandboxBooleansFalse = new List<string>();
			IncreaseSandboxCounters = new List<string>();
			DecreaseSandboxCounters = new List<string>();
			ResetSandboxCounters = new List<string>();
			SetSandboxCounters = new List<string>();
			SetSandboxCountersValues = new List<int>();

			BroadcastGenericCommand = false;

			BehaviorSpecificEventA = false;
			BehaviorSpecificEventB = false;
			BehaviorSpecificEventC = false;
			BehaviorSpecificEventD = false;
			BehaviorSpecificEventE = false;
			BehaviorSpecificEventF = false;
			BehaviorSpecificEventG = false;
			BehaviorSpecificEventH = false;

			TerminateBehavior = false;

			ChangeTargetProfile = false;
			NewTargetProfileId = "";

			ChangeBlockNames = false;
			ChangeBlockNamesFrom = new List<string>();
			ChangeBlockNamesTo = new List<string>();

			ChangeAntennaRanges = false;
			AntennaNamesForRangeChange = new List<string>();
			AntennaRangeChangeType = "Set";
			AntennaRangeChangeAmount = 0;

			ForceDespawn = false;

			ResetCooldownTimeOfTriggers = false;
			ResetTriggerCooldownNames = new List<string>();

			ChangeInertiaDampeners = false;
			InertiaDampenersEnable = false;

			EnableTriggers = false;
			EnableTriggerNames = new List<string>();

			DisableTriggers = false;
			DisableTriggerNames = new List<string>();

			ChangeRotationDirection = false;
			RotationDirection = Direction.None;

			GenerateExplosion = false;
			ExplosionOffsetFromRemote = Vector3D.Zero;
			ExplosionDamage = 1;
			ExplosionRange = 1;
			ExplosionIgnoresVoxels = false;

			GridEditable = CheckEnum.Ignore;
			SubGridsEditable = CheckEnum.Ignore;
			GridDestructible = CheckEnum.Ignore;
			SubGridsDestructible = CheckEnum.Ignore;

			RecolorGrid = false;
			RecolorSubGrids = false;
			OldBlockColors = new List<Vector3D>();
			NewBlockColors = new List<Vector3D>();
			NewBlockSkins = new List<string>();

			ChangeBlockOwnership = false;
			OwnershipBlockNames = new List<string>();
			OwnershipBlockFactions = new List<string>();

			ChangeBlockDamageMultipliers = false;
			DamageMultiplierBlockNames = new List<string>();
			DamageMultiplierValues = new List<int>();

			KnownPlayerAreaMinThreatForAvoidingAbandonment = -1;

			RazeBlocksWithNames = false;
			RazeBlocksNames = new List<string>();

			ManuallyActivateTrigger = false;
			ManuallyActivatedTriggerNames = new List<string>();

			SendCommandWithoutAntenna = false;
			SendCommandWithoutAntennaRadius = -1;

			SwitchToDamagerTarget = false;

			RemoveKnownPlayerArea = false;
			RemoveAllKnownPlayerAreas = false;

			Chance = 100;

			EnableBlocks = false;
			EnableBlockNames = new List<string>();
			EnableBlockStates = new List<SwitchEnum>();

			ChangeAutopilotProfile = false;
			AutopilotProfile = AutoPilotDataMode.Primary;

			OverwriteAutopilotProfile = false;
			OverwriteAutopilotMode = AutoPilotDataMode.Primary;
			OverwriteAutopilotId = "";

			Ramming = false;

			CreateRandomLightning = false;
			CreateLightningAtAttacker = false;
			LightningDamage = 0;
			LightningExplosionRadius = 1;
			LightningColor = new Vector3D(100, 100, 100);
			LightningMinDistance = 100;
			LightningMaxDistance = 200;
			CreateLightningAtTarget = false;

			SelfDestructTimerPadding = 0;
			SelfDestructTimeBetweenBlasts = 1;

			InheritLastAttackerFromCommand = false;

			ChangePlayerCredits = false;
			ChangePlayerCreditsAmount = 0;

			ChangeNpcFactionCredits = false;
			ChangeNpcFactionCreditsAmount = 0;
			ChangeNpcFactionCreditsTag = "";

			BuildProjectedBlocks = false;
			MaxProjectedBlocksToBuild = -1;

			ForceManualTriggerActivation = false;

			BroadcastCommandProfiles = false;
			CommandProfileIds = new List<string>();

			AddWaypointFromCommand = false;

			RecalculateDespawnCoords = false;

			AddDatapadsToSeats = false;
			DatapadNamesToAdd = new List<string>();
			DatapadCountToAdd = 1;

			ToggleBlocksOfType = false;
			BlockTypesToToggle = new List<SerializableDefinitionId>();
			BlockTypeToggles = new List<SwitchEnum>();

			CancelWaitingAtWaypoint = false;
			SwitchToNextWaypoint = false;

			ProfileSubtypeId = "";

		}

		public void InitTags(string customData) {

			if (string.IsNullOrWhiteSpace(customData) == false) {

				var descSplit = customData.Split('\n');

				foreach (var tag in descSplit) {

					//UseChatBroadcast
					if (tag.Contains("[UseChatBroadcast:") == true) {

						UseChatBroadcast = TagHelper.TagBoolCheck(tag);

					}

					//ChatData
					if (tag.Contains("[ChatData:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);
						bool gotChat = false;

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							byte[] byteData = { };

							if (TagHelper.ChatObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

								try {

									var profile = MyAPIGateway.Utilities.SerializeFromBinary<ChatProfile>(byteData);

									if (profile != null) {

										ChatData.Add(profile);
										gotChat = true;

									} else {

										Logger.WriteLog("Deserialized Chat Profile was Null");

									}

								} catch (Exception e) {

									Logger.WriteLog("Caught Exception While Attaching to Action Profile:");
									Logger.WriteLog(e.ToString());

								}

							} else {

								Logger.WriteLog("Chat Profile Not in Dictionary");

							}

						}

						if (!gotChat)
							Logger.WriteLog("Could Not Find Chat Profile Associated To Tag: " + tag);

					}

					//BarrelRoll
					if (tag.Contains("[BarrelRoll:") == true) {

						BarrelRoll = TagHelper.TagBoolCheck(tag);

					}

					//Strafe
					if (tag.Contains("[Strafe:") == true) {

						Strafe = TagHelper.TagBoolCheck(tag);

					}

					//ChangeAutopilotSpeed
					if (tag.Contains("[ChangeAutopilotSpeed:") == true) {

						ChangeAutopilotSpeed = TagHelper.TagBoolCheck(tag);

					}

					//NewAutopilotSpeed
					if (tag.Contains("[NewAutopilotSpeed:") == true) {

						NewAutopilotSpeed = TagHelper.TagFloatCheck(tag, NewAutopilotSpeed);

					}

					//SpawnEncounter
					if (tag.Contains("[SpawnEncounter:") == true) {

						SpawnEncounter = TagHelper.TagBoolCheck(tag);

					}

					//Spawner
					if (tag.Contains("[Spawner:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);
						bool gotSpawn = false;

						if (string.IsNullOrWhiteSpace(tempValue) == false) {

							byte[] byteData = { };

							if (TagHelper.SpawnerObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

								try {

									var profile = MyAPIGateway.Utilities.SerializeFromBinary<SpawnProfile>(byteData);

									if (profile != null) {

										Spawner.Add(profile);
										gotSpawn = true;

									}

								} catch (Exception) {



								}

							}

						}

						if (!gotSpawn)
							Logger.WriteLog("Could Not Find Spawn Profile Associated To Tag: " + tag);


					}

					//SelfDestruct
					if (tag.Contains("[SelfDestruct:") == true) {

						SelfDestruct = TagHelper.TagBoolCheck(tag);

					}

					//Retreat
					if (tag.Contains("[Retreat:") == true) {

						Retreat = TagHelper.TagBoolCheck(tag);

					}

					//TerminateBehavior
					if (tag.Contains("[TerminateBehavior:") == true) {

						TerminateBehavior = TagHelper.TagBoolCheck(tag);

					}

					//BroadcastCurrentTarget
					if (tag.Contains("[BroadcastCurrentTarget:") == true) {

						BroadcastCurrentTarget = TagHelper.TagBoolCheck(tag);

					}

					//SwitchToReceivedTarget
					if (tag.Contains("[SwitchToReceivedTarget:") == true) {

						SwitchToReceivedTarget = TagHelper.TagBoolCheck(tag);

					}

					//SwitchTargetToDamager
					if (tag.Contains("[SwitchTargetToDamager:") == true) {

						SwitchTargetToDamager = TagHelper.TagBoolCheck(tag);

					}

					//BroadcastDamagerTarget
					if (tag.Contains("[BroadcastDamagerTarget:") == true) {

						BroadcastDamagerTarget = TagHelper.TagBoolCheck(tag);

					}

					//BroadcastSendCode
					if (tag.Contains("[BroadcastSendCode:") == true) {

						BroadcastSendCode = TagHelper.TagStringCheck(tag);

					}

					//SwitchToBehavior
					if (tag.Contains("[SwitchToBehavior:") == true) {

						SwitchToBehavior = TagHelper.TagBoolCheck(tag);

					}

					//NewBehavior
					if (tag.Contains("[NewBehavior:") == true) {

						NewBehavior = TagHelper.TagStringCheck(tag);

					}

					//PreserveSettingsOnBehaviorSwitch
					if (tag.Contains("[PreserveSettingsOnBehaviorSwitch:") == true) {

						PreserveSettingsOnBehaviorSwitch = TagHelper.TagBoolCheck(tag);

					}

					//PreserveTriggersOnBehaviorSwitch
					if (tag.Contains("[PreserveTriggersOnBehaviorSwitch:") == true) {

						PreserveTriggersOnBehaviorSwitch = TagHelper.TagBoolCheck(tag);

					}

					//PreserveTargetDataOnBehaviorSwitch
					if (tag.Contains("[PreserveTargetDataOnBehaviorSwitch:") == true) {

						PreserveTargetDataOnBehaviorSwitch = TagHelper.TagBoolCheck(tag);

					}

					//RefreshTarget
					if (tag.Contains("[RefreshTarget:") == true) {

						RefreshTarget = TagHelper.TagBoolCheck(tag);

					}

					//SwitchTargetProfile
					if (tag.Contains("[SwitchTargetProfile:") == true) {

						SwitchTargetProfile = TagHelper.TagBoolCheck(tag);

					}

					//NewTargetProfile
					if (tag.Contains("[NewTargetProfile:") == true) {

						NewTargetProfile = TagHelper.TagStringCheck(tag);

					}

					//TriggerTimerBlocks
					if (tag.Contains("[TriggerTimerBlocks:") == true) {

						TriggerTimerBlocks = TagHelper.TagBoolCheck(tag);

					}

					//TimerBlockNames
					if (tag.Contains("[TimerBlockNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							TimerBlockNames.Add(tempvalue);

						}

					}

					//ChangeReputationWithPlayers
					if (tag.Contains("[ChangeReputationWithPlayers:") == true) {

						ChangeReputationWithPlayers = TagHelper.TagBoolCheck(tag);

					}

					//ReputationChangeRadius
					if (tag.Contains("[ReputationChangeRadius:") == true) {

						ReputationChangeRadius = TagHelper.TagDoubleCheck(tag, ReputationChangeRadius);

					}

					//ReputationChangeFactions
					if (tag.Contains("[ReputationChangeFactions:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							ReputationChangeFactions.Add(tempvalue);

						}

					}

					//ReputationChangeAmount
					if (tag.Contains("[ReputationChangeAmount:") == true) {

						int tempValue = TagHelper.TagIntCheck(tag, 0);
						ReputationChangeAmount.Add(tempValue);

					}

					//ActivateAssertiveAntennas
					if (tag.Contains("[ActivateAssertiveAntennas:") == true) {

						ActivateAssertiveAntennas = TagHelper.TagBoolCheck(tag);

					}

					//ChangeAntennaOwnership
					if (tag.Contains("[ChangeAntennaOwnership:") == true) {

						ChangeAntennaOwnership = TagHelper.TagBoolCheck(tag);

					}

					//AntennaFactionOwner
					if (tag.Contains("[AntennaFactionOwner:") == true) {

						AntennaFactionOwner = TagHelper.TagStringCheck(tag);

					}

					//CreateKnownPlayerArea
					if (tag.Contains("[CreateKnownPlayerArea:") == true) {

						CreateKnownPlayerArea = TagHelper.TagBoolCheck(tag);

					}

					//KnownPlayerAreaRadius
					if (tag.Contains("[KnownPlayerAreaRadius:") == true) {

						KnownPlayerAreaRadius = TagHelper.TagDoubleCheck(tag, KnownPlayerAreaRadius);

					}

					//KnownPlayerAreaTimer
					if (tag.Contains("[KnownPlayerAreaTimer:") == true) {

						KnownPlayerAreaTimer = TagHelper.TagIntCheck(tag, KnownPlayerAreaTimer);

					}

					//KnownPlayerAreaMaxSpawns
					if (tag.Contains("[KnownPlayerAreaMaxSpawns:") == true) {

						KnownPlayerAreaMaxSpawns = TagHelper.TagIntCheck(tag, KnownPlayerAreaMaxSpawns);

					}

					//KnownPlayerAreaMinThreatForAvoidingAbandonment
					if (tag.Contains("[KnownPlayerAreaMinThreatForAvoidingAbandonment:") == true) {

						KnownPlayerAreaMinThreatForAvoidingAbandonment = TagHelper.TagIntCheck(tag, KnownPlayerAreaMinThreatForAvoidingAbandonment);

					}

					//DamageToolAttacker
					if (tag.Contains("[DamageToolAttacker:") == true) {

						DamageToolAttacker = TagHelper.TagBoolCheck(tag);

					}

					//DamageToolAttackerAmount
					if (tag.Contains("[DamageToolAttackerAmount:") == true) {

						DamageToolAttackerAmount = TagHelper.TagFloatCheck(tag, DamageToolAttackerAmount);

					}

					//DamageToolAttackerParticle
					if (tag.Contains("[DamageToolAttackerParticle:") == true) {

						DamageToolAttackerParticle = TagHelper.TagStringCheck(tag);

					}

					//DamageToolAttackerSound
					if (tag.Contains("[DamageToolAttackerSound:") == true) {

						DamageToolAttackerSound = TagHelper.TagStringCheck(tag);

					}

					//PlayParticleEffectAtRemote
					if (tag.Contains("[PlayParticleEffectAtRemote:") == true) {

						PlayParticleEffectAtRemote = TagHelper.TagBoolCheck(tag);

					}

					//ParticleEffectId
					if (tag.Contains("[ParticleEffectId:") == true) {

						ParticleEffectId = TagHelper.TagStringCheck(tag);

					}

					//ParticleEffectOffset
					if (tag.Contains("[ParticleEffectOffset:") == true) {

						ParticleEffectOffset = TagHelper.TagVector3DCheck(tag);

					}

					//ParticleEffectScale
					if (tag.Contains("[ParticleEffectScale:") == true) {

						ParticleEffectScale = TagHelper.TagFloatCheck(tag, ParticleEffectScale);

					}

					//ParticleEffectMaxTime
					if (tag.Contains("[ParticleEffectMaxTime:") == true) {

						ParticleEffectMaxTime = TagHelper.TagFloatCheck(tag, ParticleEffectMaxTime);

					}

					//ParticleEffectColor
					if (tag.Contains("[ParticleEffectColor:") == true) {

						ParticleEffectColor = TagHelper.TagVector3DCheck(tag);

					}

					//SetBooleansTrue
					if (tag.Contains("[SetBooleansTrue:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							SetBooleansTrue.Add(tempvalue);

						}

					}

					//SetBooleansFalse
					if (tag.Contains("[SetBooleansFalse:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							SetBooleansFalse.Add(tempvalue);

						}

					}

					//IncreaseCounters
					if (tag.Contains("[IncreaseCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							IncreaseCounters.Add(tempvalue);

						}

					}

					//DecreaseCounters
					if (tag.Contains("[DecreaseCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							DecreaseCounters.Add(tempvalue);

						}

					}

					//ResetCounters
					if (tag.Contains("[ResetCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							ResetCounters.Add(tempvalue);

						}

					}

					//SetSandboxBooleansTrue
					if (tag.Contains("[SetSandboxBooleansTrue:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							SetSandboxBooleansTrue.Add(tempvalue);

						}

					}

					//SetSandboxBooleansFalse
					if (tag.Contains("[SetSandboxBooleansFalse:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							SetSandboxBooleansFalse.Add(tempvalue);

						}

					}

					//IncreaseSandboxCounters
					if (tag.Contains("[IncreaseSandboxCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							IncreaseSandboxCounters.Add(tempvalue);

						}

					}

					//DecreaseSandboxCounters
					if (tag.Contains("[DecreaseSandboxCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							DecreaseSandboxCounters.Add(tempvalue);

						}

					}

					//ResetSandboxCounters
					if (tag.Contains("[ResetSandboxCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							ResetSandboxCounters.Add(tempvalue);

						}

					}

					//ChangeAttackerReputation
					if (tag.Contains("[ChangeAttackerReputation:") == true) {

						ChangeAttackerReputation = TagHelper.TagBoolCheck(tag);

					}

					//ChangeAttackerReputationFaction
					if (tag.Contains("[ChangeAttackerReputationFaction:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							ChangeAttackerReputationFaction.Add(tempvalue);

						}

					}

					//ChangeAttackerReputationAmount
					if (tag.Contains("[ChangeAttackerReputationAmount:") == true) {

						var tempvalue = TagHelper.TagIntCheck(tag, 0);
						ChangeAttackerReputationAmount.Add(tempvalue);

					}

					//ReputationChangesForAllAttackPlayerFactionMembers
					if (tag.Contains("[ReputationChangesForAllAttackPlayerFactionMembers:") == true) {

						ReputationChangesForAllAttackPlayerFactionMembers = TagHelper.TagBoolCheck(tag);

					}

					//ChangeTargetProfile
					if (tag.Contains("[ChangeTargetProfile:") == true) {

						ChangeTargetProfile = TagHelper.TagBoolCheck(tag);

					}

					//NewTargetProfileId
					if (tag.Contains("[NewTargetProfileId:") == true) {

						NewTargetProfileId = TagHelper.TagStringCheck(tag);

					}

					//ChangeBlockNames
					if (tag.Contains("[ChangeBlockNames:") == true) {

						ChangeBlockNames = TagHelper.TagBoolCheck(tag);

					}

					//ChangeBlockNamesFrom
					if (tag.Contains("[ChangeBlockNamesFrom:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							ChangeBlockNamesFrom.Add(tempvalue);

						}

					}

					//ChangeBlockNamesTo
					if (tag.Contains("[ChangeBlockNamesTo:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							ChangeBlockNamesTo.Add(tempvalue);

						}

					}

					//ChangeAntennaRanges
					if (tag.Contains("[ChangeAntennaRanges:") == true) {

						ChangeAntennaRanges = TagHelper.TagBoolCheck(tag);

					}

					//AntennaNamesForRangeChange
					if (tag.Contains("[AntennaNamesForRangeChange:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							AntennaNamesForRangeChange.Add(tempvalue);

						}

					}

					//AntennaRangeChangeType
					if (tag.Contains("[AntennaRangeChangeType:") == true) {

						AntennaRangeChangeType = TagHelper.TagStringCheck(tag);

					}

					//AntennaRangeChangeAmount
					if (tag.Contains("[AntennaRangeChangeAmount:") == true) {

						AntennaRangeChangeAmount = TagHelper.TagFloatCheck(tag, AntennaRangeChangeAmount);

					}

					//ForceDespawn
					if (tag.Contains("[ForceDespawn:") == true) {

						ForceDespawn = TagHelper.TagBoolCheck(tag);

					}

					//ResetCooldownTimeOfTriggers
					if (tag.Contains("[ResetCooldownTimeOfTriggers:") == true) {

						ResetCooldownTimeOfTriggers = TagHelper.TagBoolCheck(tag);

					}

					//ResetTriggerCooldownNames
					if (tag.Contains("[ResetTriggerCooldownNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							ResetTriggerCooldownNames.Add(tempvalue);

						}

					}

					//BroadcastGenericCommand
					if (tag.Contains("[BroadcastGenericCommand:") == true) {

						BroadcastGenericCommand = TagHelper.TagBoolCheck(tag);

					}

					//BehaviorSpecificEventA
					if (tag.Contains("[BehaviorSpecificEventA:") == true) {

						BehaviorSpecificEventA = TagHelper.TagBoolCheck(tag);

					}

					//ChangeInertiaDampeners
					if (tag.Contains("[ChangeInertiaDampeners:") == true) {

						ChangeInertiaDampeners = TagHelper.TagBoolCheck(tag);

					}

					//InertiaDampenersEnable
					if (tag.Contains("[InertiaDampenersEnable:") == true) {

						InertiaDampenersEnable = TagHelper.TagBoolCheck(tag);

					}

					//EnableTriggers
					if (tag.Contains("[EnableTriggers:") == true) {

						EnableTriggers = TagHelper.TagBoolCheck(tag);

					}

					//EnableTriggerNames
					if (tag.Contains("[EnableTriggerNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							EnableTriggerNames.Add(tempvalue);

						}

					}

					//DisableTriggers
					if (tag.Contains("[DisableTriggers:") == true) {

						DisableTriggers = TagHelper.TagBoolCheck(tag);

					}

					//DisableTriggerNames
					if (tag.Contains("[DisableTriggerNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							DisableTriggerNames.Add(tempvalue);

						}

					}

					//StaggerWarheadDetonation
					if (tag.Contains("[StaggerWarheadDetonation:") == true) {

						StaggerWarheadDetonation = TagHelper.TagBoolCheck(tag);

					}

					//ChangeRotationDirection
					if (tag.Contains("[ChangeRotationDirection:") == true) {

						ChangeRotationDirection = TagHelper.TagBoolCheck(tag);

					}

					//RotationDirection
					if (tag.Contains("[RotationDirection:") == true) {

						RotationDirection = TagHelper.TagDirectionEnumCheck(tag);

					}

					//GenerateExplosion
					if (tag.Contains("[GenerateExplosion:") == true) {

						GenerateExplosion = TagHelper.TagBoolCheck(tag);

					}

					//ExplosionOffsetFromRemote
					if (tag.Contains("[ExplosionOffsetFromRemote:") == true) {

						ExplosionOffsetFromRemote = TagHelper.TagVector3DCheck(tag);

					}

					//ExplosionRange
					if (tag.Contains("[ExplosionRange:") == true) {

						ExplosionRange = TagHelper.TagIntCheck(tag, ExplosionRange);

					}

					//ExplosionDamage
					if (tag.Contains("[ExplosionDamage:") == true) {

						ExplosionDamage = TagHelper.TagIntCheck(tag, ExplosionDamage);

					}

					//ExplosionIgnoresVoxels
					if (tag.Contains("[ExplosionIgnoresVoxels:") == true) {

						ExplosionIgnoresVoxels = TagHelper.TagBoolCheck(tag);

					}

					//GridEditable
					if (tag.Contains("[GridEditable:") == true) {

						GridEditable = TagHelper.TagCheckEnumCheck(tag);

					}

					//SubGridsEditable
					if (tag.Contains("[SubGridsEditable:") == true) {

						SubGridsEditable = TagHelper.TagCheckEnumCheck(tag);

					}

					//GridDestructible
					if (tag.Contains("[GridDestructible:") == true) {

						GridDestructible = TagHelper.TagCheckEnumCheck(tag);

					}

					//SubGridsDestructible
					if (tag.Contains("[SubGridsDestructible:") == true) {

						SubGridsDestructible = TagHelper.TagCheckEnumCheck(tag);

					}

					//RecolorGrid
					if (tag.Contains("[RecolorGrid:") == true) {

						RecolorGrid = TagHelper.TagBoolCheck(tag);

					}

					//RecolorSubGrids
					if (tag.Contains("[RecolorSubGrids:") == true) {

						RecolorSubGrids = TagHelper.TagBoolCheck(tag);

					}

					//OldBlockColors
					if (tag.Contains("[OldBlockColors:") == true) {

						var tempvalue = TagHelper.TagVector3DCheck(tag);
						tempvalue = tempvalue == Vector3D.Zero ? new Vector3D(-10, -10, -10) : tempvalue;
						OldBlockColors.Add(tempvalue);

					}

					//NewBlockColors
					if (tag.Contains("[NewBlockColors:") == true) {

						var tempvalue = TagHelper.TagVector3DCheck(tag);
						tempvalue = tempvalue == Vector3D.Zero ? new Vector3D(-10, -10, -10) : tempvalue;
						NewBlockColors.Add(tempvalue);

					}

					//NewBlockSkins
					if (tag.Contains("[NewBlockSkins:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);
						tempvalue = string.IsNullOrWhiteSpace(tempvalue) ? "" : tempvalue;
						NewBlockSkins.Add(tempvalue);

					}

					//ChangeBlockOwnership
					if (tag.Contains("[ChangeBlockOwnership:") == true) {

						ChangeBlockOwnership = TagHelper.TagBoolCheck(tag);

					}

					//OwnershipBlockNames
					if (tag.Contains("[OwnershipBlockNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							OwnershipBlockNames.Add(tempvalue);

						}

					}

					//OwnershipBlockFactions
					if (tag.Contains("[OwnershipBlockFactions:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							OwnershipBlockFactions.Add(tempvalue);

						}

					}

					//ChangeBlockDamageMultipliers
					if (tag.Contains("[ChangeBlockDamageMultipliers:") == true) {

						ChangeBlockDamageMultipliers = TagHelper.TagBoolCheck(tag);

					}

					//DamageMultiplierBlockNames
					if (tag.Contains("[DamageMultiplierBlockNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							DamageMultiplierBlockNames.Add(tempvalue);

						}

					}

					//DamageMultiplierValues
					if (tag.Contains("[DamageMultiplierValues:") == true) {

						var tempvalue = TagHelper.TagIntCheck(tag, 1);
						DamageMultiplierValues.Add(tempvalue);

					}

					//RazeBlocksWithNames
					if (tag.Contains("[RazeBlocksWithNames:") == true) {

						RazeBlocksWithNames = TagHelper.TagBoolCheck(tag);

					}

					//RazeBlocksNames
					if (tag.Contains("[RazeBlocksNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							RazeBlocksNames.Add(tempvalue);

						}

					}

					//ManuallyActivateTrigger
					if (tag.Contains("[ManuallyActivateTrigger:") == true) {

						ManuallyActivateTrigger = TagHelper.TagBoolCheck(tag);

					}

					//ManuallyActivatedTriggerNames
					if (tag.Contains("[ManuallyActivatedTriggerNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							ManuallyActivatedTriggerNames.Add(tempvalue);

						}

					}

					//SendCommandWithoutAntenna
					if (tag.Contains("[SendCommandWithoutAntenna:") == true) {

						SendCommandWithoutAntenna = TagHelper.TagBoolCheck(tag);

					}

					//SendCommandWithoutAntennaRadius
					if (tag.Contains("[SendCommandWithoutAntennaRadius:") == true) {

						SendCommandWithoutAntennaRadius = TagHelper.TagDoubleCheck(tag, SendCommandWithoutAntennaRadius);

					}

					//RemoveKnownPlayerArea
					if (tag.Contains("[RemoveKnownPlayerArea:") == true) {

						RemoveKnownPlayerArea = TagHelper.TagBoolCheck(tag);

					}

					//RemoveAllKnownPlayerAreas
					if (tag.Contains("[RemoveAllKnownPlayerAreas:") == true) {

						RemoveAllKnownPlayerAreas = TagHelper.TagBoolCheck(tag);

					}

					//Chance
					if (tag.Contains("[Chance:") == true) {

						Chance = TagHelper.TagIntCheck(tag, Chance);

					}

					//EnableBlocks
					if (tag.Contains("[EnableBlocks:") == true) {

						EnableBlocks = TagHelper.TagBoolCheck(tag);

					}

					//EnableBlockNames
					if (tag.Contains("[EnableBlockNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							EnableBlockNames.Add(tempvalue);

						}

					}

					//EnableBlockStates
					if (tag.Contains("[EnableBlockStates:") == true) {

						var tempvalue = TagHelper.TagSwitchEnumCheck(tag);
						EnableBlockStates.Add(tempvalue);

					}

					//ChangeAutopilotProfile
					if (tag.Contains("[ChangeAutopilotProfile:") == true) {

						ChangeAutopilotProfile = TagHelper.TagBoolCheck(tag);

					}

					//AutopilotProfile
					if (tag.Contains("[AutopilotProfile:") == true) {

						AutopilotProfile = TagHelper.TagAutoPilotProfileModeCheck(tag);

					}

					//Ramming
					if (tag.Contains("[Ramming:") == true) {

						Ramming = TagHelper.TagBoolCheck(tag);

					}

					//CreateRandomLightning
					if (tag.Contains("[CreateRandomLightning:") == true) {

						CreateRandomLightning = TagHelper.TagBoolCheck(tag);

					}

					//CreateLightningAtAttacker
					if (tag.Contains("[CreateLightningAtAttacker:") == true) {

						CreateLightningAtAttacker = TagHelper.TagBoolCheck(tag);

					}

					//LightningDamage
					if (tag.Contains("[LightningDamage:") == true) {

						LightningDamage = TagHelper.TagIntCheck(tag, LightningDamage);

					}

					//LightningExplosionRadius
					if (tag.Contains("[LightningExplosionRadius:") == true) {

						LightningExplosionRadius = TagHelper.TagIntCheck(tag, LightningExplosionRadius);

					}

					//LightningColor
					if (tag.Contains("[LightningColor:") == true) {

						LightningColor = TagHelper.TagVector3DCheck(tag);

					}

					//LightningMinDistance
					if (tag.Contains("[LightningMinDistance:") == true) {

						LightningMinDistance = TagHelper.TagDoubleCheck(tag, LightningMinDistance);

					}

					//LightningMaxDistance
					if (tag.Contains("[LightningMaxDistance:") == true) {

						LightningMaxDistance = TagHelper.TagDoubleCheck(tag, LightningMaxDistance);

					}

					//CreateLightningAtTarget
					if (tag.Contains("[CreateLightningAtTarget:") == true) {

						CreateLightningAtTarget = TagHelper.TagBoolCheck(tag);

					}

					//SelfDestructTimerPadding
					if (tag.Contains("[SelfDestructTimerPadding:") == true) {

						SelfDestructTimerPadding = TagHelper.TagIntCheck(tag, SelfDestructTimerPadding);

					}

					//SelfDestructTimeBetweenBlasts
					if (tag.Contains("[SelfDestructTimeBetweenBlasts:") == true) {

						SelfDestructTimeBetweenBlasts = TagHelper.TagIntCheck(tag, SelfDestructTimeBetweenBlasts);

					}

					//SetCounters
					if (tag.Contains("[SetCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							SetCounters.Add(tempvalue);

						}

					}

					//SetSandboxCounters
					if (tag.Contains("[SetSandboxCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							SetSandboxCounters.Add(tempvalue);

						}

					}

					//SetCountersValues
					if (tag.Contains("[SetCountersValues:") == true) {

						var tempvalue = TagHelper.TagIntCheck(tag, 0);
						SetCountersValues.Add(tempvalue);

					}

					//SetSandboxCountersValues
					if (tag.Contains("[SetSandboxCountersValues:") == true) {

						var tempvalue = TagHelper.TagIntCheck(tag, 0);
						SetSandboxCountersValues.Add(tempvalue);

					}

					//InheritLastAttackerFromCommand
					if (tag.Contains("[InheritLastAttackerFromCommand:") == true) {

						InheritLastAttackerFromCommand = TagHelper.TagBoolCheck(tag);

					}

					//ChangePlayerCredits
					if (tag.Contains("[ChangePlayerCredits:") == true) {

						ChangePlayerCredits = TagHelper.TagBoolCheck(tag);

					}

					//ChangePlayerCreditsAmount
					if (tag.Contains("[ChangePlayerCreditsAmount:") == true) {

						ChangePlayerCreditsAmount = TagHelper.TagLongCheck(tag, ChangePlayerCreditsAmount);

					}

					//ChangeNpcFactionCredits
					if (tag.Contains("[ChangeNpcFactionCredits:") == true) {

						ChangeNpcFactionCredits = TagHelper.TagBoolCheck(tag);

					}

					//ChangeNpcFactionCreditsAmount
					if (tag.Contains("[ChangeNpcFactionCreditsAmount:") == true) {

						ChangeNpcFactionCreditsAmount = TagHelper.TagLongCheck(tag, ChangeNpcFactionCreditsAmount);

					}

					//ChangeNpcFactionCreditsTag
					if (tag.Contains("[ChangeNpcFactionCreditsTag:") == true) {

						ChangeNpcFactionCreditsTag = TagHelper.TagStringCheck(tag);

					}

					//BuildProjectedBlocks
					if (tag.Contains("[BuildProjectedBlocks:") == true) {

						BuildProjectedBlocks = TagHelper.TagBoolCheck(tag);

					}

					//MaxProjectedBlocksToBuild
					if (tag.Contains("[MaxProjectedBlocksToBuild:") == true) {

						MaxProjectedBlocksToBuild = TagHelper.TagIntCheck(tag, MaxProjectedBlocksToBuild);

					}

					//ForceManualTriggerActivation
					if (tag.Contains("[ForceManualTriggerActivation:") == true) {

						ForceManualTriggerActivation = TagHelper.TagBoolCheck(tag);

					}

					//OverwriteAutopilotProfile
					if (tag.Contains("[OverwriteAutopilotProfile:") == true) {

						OverwriteAutopilotProfile = TagHelper.TagBoolCheck(tag);

					}

					//OverwriteAutopilotMode
					if (tag.Contains("[OverwriteAutopilotMode:") == true) {

						OverwriteAutopilotMode = TagHelper.TagAutoPilotProfileModeCheck(tag);

					}

					//OverwriteAutopilotId
					if (tag.Contains("[OverwriteAutopilotId:") == true) {

						OverwriteAutopilotId = TagHelper.TagStringCheck(tag);

					}

					//BroadcastCommandProfiles
					if (tag.Contains("[BroadcastCommandProfiles:") == true) {

						BroadcastCommandProfiles = TagHelper.TagBoolCheck(tag);

					}

					//CommandProfileIds
					if (tag.Contains("[CommandProfileIds:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							CommandProfileIds.Add(tempvalue);

						}

					}

					//AddWaypointFromCommand
					if (tag.Contains("[AddWaypointFromCommand:") == true) {

						AddWaypointFromCommand = TagHelper.TagBoolCheck(tag);

					}

					//RecalculateDespawnCoords
					if (tag.Contains("[RecalculateDespawnCoords:") == true) {

						RecalculateDespawnCoords = TagHelper.TagBoolCheck(tag);

					}

					//AddDatapadsToSeats
					if (tag.Contains("[AddDatapadsToSeats:") == true) {

						AddDatapadsToSeats = TagHelper.TagBoolCheck(tag);

					}

					//DatapadNamesToAdd
					if (tag.Contains("[DatapadNamesToAdd:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							DatapadNamesToAdd.Add(tempvalue);

						}

					}

					//DatapadCountToAdd
					if (tag.Contains("[DatapadCountToAdd:") == true) {

						DatapadCountToAdd = TagHelper.TagIntCheck(tag, DatapadCountToAdd);

					}

					//ToggleBlocksOfType
					if (tag.Contains("[ToggleBlocksOfType:") == true) {

						ToggleBlocksOfType = TagHelper.TagBoolCheck(tag);

					}

					//BlockTypesToToggle
					if (tag.Contains("[BlockTypesToToggle:") == true) {

						var tempvalue = TagHelper.TagDefinitionIdCheck(tag);

						if (tempvalue != new MyDefinitionId()) {

							BlockTypesToToggle.Add(tempvalue);

						}

					}

					//BlockTypeToggles
					if (tag.Contains("[BlockTypeToggles:") == true) {

						var tempvalue = TagHelper.TagSwitchEnumCheck(tag);
						BlockTypeToggles.Add(tempvalue);

					}

					//CancelWaitingAtWaypoint
					if (tag.Contains("[CancelWaitingAtWaypoint:") == true) {

						CancelWaitingAtWaypoint = TagHelper.TagBoolCheck(tag);

					}

					//SwitchToNextWaypoint
					if (tag.Contains("[SwitchToNextWaypoint:") == true) {

						SwitchToNextWaypoint = TagHelper.TagBoolCheck(tag);

					}

				}

			}

		}

	}

}
