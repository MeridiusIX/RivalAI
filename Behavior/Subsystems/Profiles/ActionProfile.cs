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
		public ChatProfile ChatData;

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
		public SpawnProfile Spawner;
		
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
		public bool ClearSettingsOnBehaviorSwitch;
		
		[ProtoMember(18)]
		public bool RefreshTarget;

		[ProtoMember(19)]
		public bool SwitchTargetProfile;

		[ProtoMember(20)]
		public string NewTargetProfile;

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

		public ActionProfile(){

			UseChatBroadcast = false;
			ChatData = new ChatProfile();

			BarrelRoll = false;
			Strafe = false;
			
			ChangeAutopilotSpeed = false;
			NewAutopilotSpeed = 0;
			
			SpawnEncounter = false;
			Spawner = new SpawnProfile();

			SelfDestruct = false;
			
			Retreat = false;

			BroadcastCurrentTarget = false;
			BroadcastDamagerTarget = false;
			BroadcastSendCode = "";
			SwitchToReceivedTarget = false;

			SwitchToBehavior = false;
			NewBehavior = "";
			ClearSettingsOnBehaviorSwitch = false;
			
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

										ChatData = profile;
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

										Spawner = profile;
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
					
					//BroadcastCurrentTarget
					if(tag.Contains("[BroadcastCurrentTarget:") == true) {

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
					
					//SwitchToBehavior
					if(tag.Contains("[SwitchToBehavior:") == true) {

						this.SwitchToBehavior = TagHelper.TagBoolCheck(tag);

					}
					
					//NewBehavior
					if(tag.Contains("[NewBehavior:") == true) {

						this.NewBehavior = TagHelper.TagStringCheck(tag);

					}
					
					//ClearSettingsOnBehaviorSwitch
					if(tag.Contains("[ClearSettingsOnBehaviorSwitch:") == true) {

						this.ClearSettingsOnBehaviorSwitch = TagHelper.TagBoolCheck(tag);

					}
					
					//RefreshTarget
					if(tag.Contains("[RefreshTarget:") == true) {

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
					
					//DamageToolAttacker
					if(tag.Contains("[DamageToolAttacker:") == true) {

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

					//BehaviorSpecificEventA
					if (tag.Contains("[BehaviorSpecificEventA:") == true) {

						this.BehaviorSpecificEventA = TagHelper.TagBoolCheck(tag);

					}


				}

			}

		}

	}

}
