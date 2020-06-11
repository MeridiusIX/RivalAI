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
using RivalAI.Behavior.Settings;
using RivalAI.Helpers;

namespace RivalAI.Behavior.Subsystems.Profiles {
	
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

		public ActionProfile(){

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
			
			SetSandboxBooleansTrue = new List<string>();
			SetSandboxBooleansFalse = new List<string>();
			IncreaseSandboxCounters = new List<string>();
			DecreaseSandboxCounters = new List<string>();
			ResetSandboxCounters = new List<string>();

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

			ProfileSubtypeId = "";

		}

		public void InitTags(string customData) {

			if(string.IsNullOrWhiteSpace(customData) == false) {

				var descSplit = customData.Split('\n');

				foreach(var tag in descSplit) {

					//UseChatBroadcast
					if(tag.Contains("[UseChatBroadcast:") == true) {

						this.UseChatBroadcast = TagHelper.TagBoolCheck(tag);

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
					if(tag.Contains("[BarrelRoll:") == true) {

						this.BarrelRoll = TagHelper.TagBoolCheck(tag);

					}
					
					//Strafe
					if(tag.Contains("[Strafe:") == true) {

						this.Strafe = TagHelper.TagBoolCheck(tag);

					}
					
					//ChangeAutopilotSpeed
					if(tag.Contains("[ChangeAutopilotSpeed:") == true) {

						this.ChangeAutopilotSpeed = TagHelper.TagBoolCheck(tag);

					}
					
					//NewAutopilotSpeed
					if(tag.Contains("[NewAutopilotSpeed:") == true) {

						this.NewAutopilotSpeed = TagHelper.TagFloatCheck(tag, this.NewAutopilotSpeed);

					}
					
					//SpawnEncounter
					if(tag.Contains("[SpawnEncounter:") == true) {

						this.SpawnEncounter = TagHelper.TagBoolCheck(tag);

					}

					//Spawner
					if(tag.Contains("[Spawner:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);
						bool gotSpawn = false;

						if(string.IsNullOrWhiteSpace(tempValue) == false) {

							byte[] byteData = { };

							if(TagHelper.SpawnerObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

								try {

									var profile = MyAPIGateway.Utilities.SerializeFromBinary<SpawnProfile>(byteData);

									if(profile != null) {

										Spawner.Add(profile);
										gotSpawn = true;

									}

								} catch(Exception) {



								}

							}

						}

						if (!gotSpawn)
							Logger.WriteLog("Could Not Find Spawn Profile Associated To Tag: " + tag);


					}

					//SelfDestruct
					if (tag.Contains("[SelfDestruct:") == true) {

						this.SelfDestruct = TagHelper.TagBoolCheck(tag);

					}
					
					//Retreat
					if(tag.Contains("[Retreat:") == true) {

						this.Retreat = TagHelper.TagBoolCheck(tag);

					}

					//TerminateBehavior
					if (tag.Contains("[TerminateBehavior:") == true) {

						this.TerminateBehavior = TagHelper.TagBoolCheck(tag);

					}

					//BroadcastCurrentTarget
					if (tag.Contains("[BroadcastCurrentTarget:") == true) {

						this.BroadcastCurrentTarget = TagHelper.TagBoolCheck(tag);

					}
					
					//SwitchToReceivedTarget
					if(tag.Contains("[SwitchToReceivedTarget:") == true) {

						this.SwitchToReceivedTarget = TagHelper.TagBoolCheck(tag);

					}
					
					//BroadcastDamagerTarget
					if(tag.Contains("[BroadcastDamagerTarget:") == true) {

						this.BroadcastDamagerTarget = TagHelper.TagBoolCheck(tag);

					}

					//BroadcastSendCode
					if (tag.Contains("[BroadcastSendCode:") == true) {

						this.BroadcastSendCode = TagHelper.TagStringCheck(tag);

					}

					//SwitchToBehavior
					if (tag.Contains("[SwitchToBehavior:") == true) {

						this.SwitchToBehavior = TagHelper.TagBoolCheck(tag);

					}
					
					//NewBehavior
					if(tag.Contains("[NewBehavior:") == true) {

						this.NewBehavior = TagHelper.TagStringCheck(tag);

					}
					
					//PreserveSettingsOnBehaviorSwitch
					if(tag.Contains("[PreserveSettingsOnBehaviorSwitch:") == true) {

						this.PreserveSettingsOnBehaviorSwitch = TagHelper.TagBoolCheck(tag);

					}

					//PreserveTriggersOnBehaviorSwitch
					if (tag.Contains("[PreserveTriggersOnBehaviorSwitch:") == true) {

						this.PreserveTriggersOnBehaviorSwitch = TagHelper.TagBoolCheck(tag);

					}

					//PreserveTargetDataOnBehaviorSwitch
					if (tag.Contains("[PreserveTargetDataOnBehaviorSwitch:") == true) {

						this.PreserveTargetDataOnBehaviorSwitch = TagHelper.TagBoolCheck(tag);

					}

					//RefreshTarget
					if (tag.Contains("[RefreshTarget:") == true) {

						this.RefreshTarget = TagHelper.TagBoolCheck(tag);

					}
					
					//SwitchTargetProfile
					if(tag.Contains("[SwitchTargetProfile:") == true) {

						this.SwitchTargetProfile = TagHelper.TagBoolCheck(tag);

					}
					
					//NewTargetProfile
					if(tag.Contains("[NewTargetProfile:") == true) {

						this.NewTargetProfile = TagHelper.TagStringCheck(tag);

					}
					
					//TriggerTimerBlocks
					if(tag.Contains("[TriggerTimerBlocks:") == true) {

						this.TriggerTimerBlocks = TagHelper.TagBoolCheck(tag);

					}
					
					//TimerBlockNames
					if(tag.Contains("[TimerBlockNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.TimerBlockNames.Add(tempvalue);

						}

					}
					
					//ChangeReputationWithPlayers
					if(tag.Contains("[ChangeReputationWithPlayers:") == true) {

						this.ChangeReputationWithPlayers = TagHelper.TagBoolCheck(tag);

					}
					
					//ReputationChangeRadius
					if(tag.Contains("[ReputationChangeRadius:") == true) {

						this.ReputationChangeRadius = TagHelper.TagDoubleCheck(tag, ReputationChangeRadius);

					}

					//ReputationChangeFactions
					if (tag.Contains("[ReputationChangeFactions:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.ReputationChangeFactions.Add(tempvalue);

						}

					}

					//ReputationChangeAmount
					if (tag.Contains("[ReputationChangeAmount:") == true) {

						int tempValue = TagHelper.TagIntCheck(tag, 0);
						this.ReputationChangeAmount.Add(tempValue);

					}
					
					//ActivateAssertiveAntennas
					if(tag.Contains("[ActivateAssertiveAntennas:") == true) {

						this.ActivateAssertiveAntennas = TagHelper.TagBoolCheck(tag);

					}
					
					//ChangeAntennaOwnership
					if(tag.Contains("[ChangeAntennaOwnership:") == true) {

						this.ChangeAntennaOwnership = TagHelper.TagBoolCheck(tag);

					}
					
					//AntennaFactionOwner
					if(tag.Contains("[AntennaFactionOwner:") == true) {

						this.AntennaFactionOwner = TagHelper.TagStringCheck(tag);

					}

					//CreateKnownPlayerArea
					if(tag.Contains("[CreateKnownPlayerArea:") == true) {

						this.CreateKnownPlayerArea = TagHelper.TagBoolCheck(tag);

					}

					//KnownPlayerAreaRadius
					if(tag.Contains("[KnownPlayerAreaRadius:") == true) {

						this.KnownPlayerAreaRadius = TagHelper.TagDoubleCheck(tag, KnownPlayerAreaRadius);

					}

					//KnownPlayerAreaTimer
					if(tag.Contains("[KnownPlayerAreaTimer:") == true) {

						this.KnownPlayerAreaTimer = TagHelper.TagIntCheck(tag, KnownPlayerAreaTimer);

					}

					//KnownPlayerAreaMaxSpawns
					if(tag.Contains("[KnownPlayerAreaMaxSpawns:") == true) {

						this.KnownPlayerAreaMaxSpawns = TagHelper.TagIntCheck(tag, KnownPlayerAreaMaxSpawns);

					}

					//KnownPlayerAreaMinThreatForAvoidingAbandonment
					if (tag.Contains("[KnownPlayerAreaMinThreatForAvoidingAbandonment:") == true) {

						this.KnownPlayerAreaMinThreatForAvoidingAbandonment = TagHelper.TagIntCheck(tag, KnownPlayerAreaMinThreatForAvoidingAbandonment);

					}

					//DamageToolAttacker
					if (tag.Contains("[DamageToolAttacker:") == true) {

						this.DamageToolAttacker = TagHelper.TagBoolCheck(tag);

					}
					
					//DamageToolAttackerAmount
					if(tag.Contains("[DamageToolAttackerAmount:") == true) {

						this.DamageToolAttackerAmount = TagHelper.TagFloatCheck(tag, this.DamageToolAttackerAmount);

					}
					
					//DamageToolAttackerParticle
					if(tag.Contains("[DamageToolAttackerParticle:") == true) {

						this.DamageToolAttackerParticle = TagHelper.TagStringCheck(tag);

					}
					
					//DamageToolAttackerSound
					if(tag.Contains("[DamageToolAttackerSound:") == true) {

						this.DamageToolAttackerSound = TagHelper.TagStringCheck(tag);

					}
					
					//PlayParticleEffectAtRemote
					if(tag.Contains("[PlayParticleEffectAtRemote:") == true) {

						this.PlayParticleEffectAtRemote = TagHelper.TagBoolCheck(tag);

					}
					
					//ParticleEffectId
					if(tag.Contains("[ParticleEffectId:") == true) {

						this.ParticleEffectId = TagHelper.TagStringCheck(tag);

					}
					
					//ParticleEffectOffset
					if(tag.Contains("[ParticleEffectOffset:") == true) {

						this.ParticleEffectOffset = TagHelper.TagVector3DCheck(tag);

					}
					
					//ParticleEffectScale
					if(tag.Contains("[ParticleEffectScale:") == true) {

						this.ParticleEffectScale = TagHelper.TagFloatCheck(tag, this.ParticleEffectScale);

					}
					
					//ParticleEffectMaxTime
					if(tag.Contains("[ParticleEffectMaxTime:") == true) {

						this.ParticleEffectMaxTime = TagHelper.TagFloatCheck(tag, this.ParticleEffectMaxTime);

					}
					
					//ParticleEffectColor
					if(tag.Contains("[ParticleEffectColor:") == true) {

						this.ParticleEffectColor = TagHelper.TagVector3DCheck(tag);

					}
					
					//SetBooleansTrue
					if(tag.Contains("[SetBooleansTrue:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.SetBooleansTrue.Add(tempvalue);

						}

					}
					
					//SetBooleansFalse
					if(tag.Contains("[SetBooleansFalse:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.SetBooleansFalse.Add(tempvalue);

						}

					}
					
					//IncreaseCounters
					if(tag.Contains("[IncreaseCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.IncreaseCounters.Add(tempvalue);

						}

					}
					
					//DecreaseCounters
					if(tag.Contains("[DecreaseCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.DecreaseCounters.Add(tempvalue);

						}

					}
					
					//ResetCounters
					if(tag.Contains("[ResetCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.ResetCounters.Add(tempvalue);

						}

					}
					
					//SetSandboxBooleansTrue
					if(tag.Contains("[SetSandboxBooleansTrue:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.SetSandboxBooleansTrue.Add(tempvalue);

						}

					}
					
					//SetSandboxBooleansFalse
					if(tag.Contains("[SetSandboxBooleansFalse:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.SetSandboxBooleansFalse.Add(tempvalue);

						}

					}
					
					//IncreaseSandboxCounters
					if(tag.Contains("[IncreaseSandboxCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.IncreaseSandboxCounters.Add(tempvalue);

						}

					}
					
					//DecreaseSandboxCounters
					if(tag.Contains("[DecreaseSandboxCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.DecreaseSandboxCounters.Add(tempvalue);

						}

					}
					
					//ResetSandboxCounters
					if(tag.Contains("[ResetSandboxCounters:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.ResetSandboxCounters.Add(tempvalue);

						}

					}

					//ChangeAttackerReputation
					if (tag.Contains("[ChangeAttackerReputation:") == true) {

						this.ChangeAttackerReputation = TagHelper.TagBoolCheck(tag);

					}

					//ChangeAttackerReputationFaction
					if (tag.Contains("[ChangeAttackerReputationFaction:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.ChangeAttackerReputationFaction.Add(tempvalue);

						}

					}

					//ChangeAttackerReputationAmount
					if (tag.Contains("[ChangeAttackerReputationAmount:") == true) {

						var tempvalue = TagHelper.TagIntCheck(tag, 0);
						this.ChangeAttackerReputationAmount.Add(tempvalue);

					}

					//ReputationChangesForAllAttackPlayerFactionMembers
					if (tag.Contains("[ReputationChangesForAllAttackPlayerFactionMembers:") == true) {

						this.ReputationChangesForAllAttackPlayerFactionMembers = TagHelper.TagBoolCheck(tag);

					}

					//ChangeTargetProfile
					if (tag.Contains("[ChangeTargetProfile:") == true) {

						this.ChangeTargetProfile = TagHelper.TagBoolCheck(tag);

					}

					//NewTargetProfileId
					if (tag.Contains("[NewTargetProfileId:") == true) {

						this.NewTargetProfileId = TagHelper.TagStringCheck(tag);

					}

					//ChangeBlockNames
					if (tag.Contains("[ChangeBlockNames:") == true) {

						this.ChangeBlockNames = TagHelper.TagBoolCheck(tag);

					}

					//ChangeBlockNamesFrom
					if (tag.Contains("[ChangeBlockNamesFrom:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.ChangeBlockNamesFrom.Add(tempvalue);

						}

					}

					//ChangeBlockNamesTo
					if (tag.Contains("[ChangeBlockNamesTo:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.ChangeBlockNamesTo.Add(tempvalue);

						}

					}

					//ChangeAntennaRanges
					if (tag.Contains("[ChangeAntennaRanges:") == true) {

						this.ChangeAntennaRanges = TagHelper.TagBoolCheck(tag);

					}

					//AntennaNamesForRangeChange
					if (tag.Contains("[AntennaNamesForRangeChange:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.AntennaNamesForRangeChange.Add(tempvalue);

						}

					}

					//AntennaRangeChangeType
					if (tag.Contains("[AntennaRangeChangeType:") == true) {

						this.AntennaRangeChangeType = TagHelper.TagStringCheck(tag);

					}

					//AntennaRangeChangeAmount
					if (tag.Contains("[AntennaRangeChangeAmount:") == true) {

						this.AntennaRangeChangeAmount = TagHelper.TagFloatCheck(tag, this.AntennaRangeChangeAmount);

					}

					//ForceDespawn
					if (tag.Contains("[ForceDespawn:") == true) {

						this.ForceDespawn = TagHelper.TagBoolCheck(tag);

					}

					//ResetCooldownTimeOfTriggers
					if (tag.Contains("[ResetCooldownTimeOfTriggers:") == true) {

						this.ResetCooldownTimeOfTriggers = TagHelper.TagBoolCheck(tag);

					}

					//ResetTriggerCooldownNames
					if (tag.Contains("[ResetTriggerCooldownNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.ResetTriggerCooldownNames.Add(tempvalue);

						}

					}

					//BroadcastGenericCommand
					if (tag.Contains("[BroadcastGenericCommand:") == true) {

						this.BroadcastGenericCommand = TagHelper.TagBoolCheck(tag);

					}

					//BehaviorSpecificEventA
					if (tag.Contains("[BehaviorSpecificEventA:") == true) {

						this.BehaviorSpecificEventA = TagHelper.TagBoolCheck(tag);

					}

					//ChangeInertiaDampeners
					if (tag.Contains("[ChangeInertiaDampeners:") == true) {

						this.ChangeInertiaDampeners = TagHelper.TagBoolCheck(tag);

					}

					//InertiaDampenersEnable
					if (tag.Contains("[InertiaDampenersEnable:") == true) {

						this.InertiaDampenersEnable = TagHelper.TagBoolCheck(tag);

					}

					//EnableTriggers
					if (tag.Contains("[EnableTriggers:") == true) {

						this.EnableTriggers = TagHelper.TagBoolCheck(tag);

					}

					//EnableTriggerNames
					if (tag.Contains("[EnableTriggerNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.EnableTriggerNames.Add(tempvalue);

						}

					}

					//DisableTriggers
					if (tag.Contains("[DisableTriggers:") == true) {

						this.DisableTriggers = TagHelper.TagBoolCheck(tag);

					}

					//DisableTriggerNames
					if (tag.Contains("[DisableTriggerNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.DisableTriggerNames.Add(tempvalue);

						}

					}

					//StaggerWarheadDetonation
					if (tag.Contains("[StaggerWarheadDetonation:") == true) {

						this.StaggerWarheadDetonation = TagHelper.TagBoolCheck(tag);

					}

					//ChangeRotationDirection
					if (tag.Contains("[ChangeRotationDirection:") == true) {

						this.ChangeRotationDirection = TagHelper.TagBoolCheck(tag);

					}

					//RotationDirection
					if (tag.Contains("[RotationDirection:") == true) {

						this.RotationDirection = TagHelper.TagDirectionEnumCheck(tag);

					}

					//GenerateExplosion
					if (tag.Contains("[GenerateExplosion:") == true) {

						this.GenerateExplosion = TagHelper.TagBoolCheck(tag);

					}

					//ExplosionOffsetFromRemote
					if (tag.Contains("[ExplosionOffsetFromRemote:") == true) {

						this.ExplosionOffsetFromRemote = TagHelper.TagVector3DCheck(tag);

					}

					//ExplosionRange
					if (tag.Contains("[ExplosionRange:") == true) {

						this.ExplosionRange = TagHelper.TagIntCheck(tag, ExplosionRange);

					}

					//ExplosionDamage
					if (tag.Contains("[ExplosionDamage:") == true) {

						this.ExplosionDamage = TagHelper.TagIntCheck(tag, ExplosionDamage);

					}

					//ExplosionIgnoresVoxels
					if (tag.Contains("[ExplosionIgnoresVoxels:") == true) {

						this.ExplosionIgnoresVoxels = TagHelper.TagBoolCheck(tag);

					}

					//GridEditable
					if (tag.Contains("[GridEditable:") == true) {

						this.GridEditable = TagHelper.TagCheckEnumCheck(tag);

					}

					//SubGridsEditable
					if (tag.Contains("[SubGridsEditable:") == true) {

						this.SubGridsEditable = TagHelper.TagCheckEnumCheck(tag);

					}

					//GridDestructible
					if (tag.Contains("[GridDestructible:") == true) {

						this.GridDestructible = TagHelper.TagCheckEnumCheck(tag);

					}

					//SubGridsDestructible
					if (tag.Contains("[SubGridsDestructible:") == true) {

						this.SubGridsDestructible = TagHelper.TagCheckEnumCheck(tag);

					}

					//RecolorGrid
					if (tag.Contains("[RecolorGrid:") == true) {

						this.RecolorGrid = TagHelper.TagBoolCheck(tag);

					}

					//RecolorSubGrids
					if (tag.Contains("[RecolorSubGrids:") == true) {

						this.RecolorSubGrids = TagHelper.TagBoolCheck(tag);

					}

					//OldBlockColors
					if (tag.Contains("[OldBlockColors:") == true) {

						var tempvalue = TagHelper.TagVector3DCheck(tag);
						tempvalue = tempvalue == Vector3D.Zero ? new Vector3D(-10, -10, -10) : tempvalue;
						this.OldBlockColors.Add(tempvalue);

					}

					//NewBlockColors
					if (tag.Contains("[NewBlockColors:") == true) {

						var tempvalue = TagHelper.TagVector3DCheck(tag);
						tempvalue = tempvalue == Vector3D.Zero ? new Vector3D(-10,-10,-10) : tempvalue;
						this.NewBlockColors.Add(tempvalue);

					}

					//NewBlockSkins
					if (tag.Contains("[NewBlockSkins:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);
						tempvalue = string.IsNullOrWhiteSpace(tempvalue) ? "" : tempvalue;
						this.NewBlockSkins.Add(tempvalue);

					}

					//ChangeBlockOwnership
					if (tag.Contains("[ChangeBlockOwnership:") == true) {

						this.ChangeBlockOwnership = TagHelper.TagBoolCheck(tag);

					}

					//OwnershipBlockNames
					if (tag.Contains("[OwnershipBlockNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.OwnershipBlockNames.Add(tempvalue);

						}

					}

					//OwnershipBlockFactions
					if (tag.Contains("[OwnershipBlockFactions:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.OwnershipBlockFactions.Add(tempvalue);

						}

					}

					//ChangeBlockDamageMultipliers
					if (tag.Contains("[ChangeBlockDamageMultipliers:") == true) {

						this.ChangeBlockDamageMultipliers = TagHelper.TagBoolCheck(tag);

					}

					//DamageMultiplierBlockNames
					if (tag.Contains("[DamageMultiplierBlockNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.DamageMultiplierBlockNames.Add(tempvalue);

						}

					}

					//DamageMultiplierValues
					if (tag.Contains("[DamageMultiplierValues:") == true) {

						var tempvalue = TagHelper.TagIntCheck(tag, 1);
						this.DamageMultiplierValues.Add(tempvalue);

					}

					//RazeBlocksWithNames
					if (tag.Contains("[RazeBlocksWithNames:") == true) {

						this.RazeBlocksWithNames = TagHelper.TagBoolCheck(tag);

					}

					//RazeBlocksNames
					if (tag.Contains("[RazeBlocksNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.DamageMultiplierBlockNames.Add(tempvalue);

						}

					}

					//ManuallyActivateTrigger
					if (tag.Contains("[ManuallyActivateTrigger:") == true) {

						this.ManuallyActivateTrigger = TagHelper.TagBoolCheck(tag);

					}

					//ManuallyActivatedTriggerNames
					if (tag.Contains("[ManuallyActivatedTriggerNames:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

							this.ManuallyActivatedTriggerNames.Add(tempvalue);

						}

					}

					//SendCommandWithoutAntenna
					if (tag.Contains("[SendCommandWithoutAntenna:") == true) {

						this.SendCommandWithoutAntenna = TagHelper.TagBoolCheck(tag);

					}

					//SendCommandWithoutAntennaRadius
					if (tag.Contains("[SendCommandWithoutAntennaRadius:") == true) {

						this.SendCommandWithoutAntennaRadius = TagHelper.TagDoubleCheck(tag, this.SendCommandWithoutAntennaRadius);

					}

					//RemoveKnownPlayerArea
					if (tag.Contains("[RemoveKnownPlayerArea:") == true) {

						this.RemoveKnownPlayerArea = TagHelper.TagBoolCheck(tag);

					}

					//RemoveAllKnownPlayerAreas
					if (tag.Contains("[RemoveAllKnownPlayerAreas:") == true) {

						this.RemoveAllKnownPlayerAreas = TagHelper.TagBoolCheck(tag);

					}

				}

			}

		}

	}

}
