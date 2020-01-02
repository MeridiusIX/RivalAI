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
	public class SpawnProfile {

		[ProtoMember(1)]
		public bool UseSpawn;

		[ProtoMember(2)]
		public bool StartsReady;

		[ProtoMember(3)]
		public float FirstSpawnTimeMs;

		[ProtoMember(4)]
		public float SpawnMinCooldown;

		[ProtoMember(5)]
		public float SpawnMaxCooldown;

		[ProtoMember(6)]
		public int MaxSpawns;

		[ProtoMember(7)]
		public List<string> SpawnGroups;

		[ProtoMember(8)]
		public int CooldownTime;

		[ProtoMember(9)]
		public int SpawnCount;

		[ProtoMember(10)]
		public DateTime LastSpawnTime;

		[ProtoMember(11)]
		public bool UseRelativeSpawnPosition;

		[ProtoMember(12)]
		public double MinDistance;

		[ProtoMember(13)]
		public double MaxDistance;
		
		[ProtoMember(14)]
		public double MinAltitude;

		[ProtoMember(15)]
		public double MaxAltitude;
		
		[ProtoMember(16)]
		public Vector3D RelativeSpawnOffset;

		[ProtoMember(17)]
		public Vector3D RelativeSpawnVelocity;

		[ProtoMember(18)]
		public bool IgnoreSafetyChecks;
		
		[ProtoMember(19)]
		public bool InheritNpcAltitude;
		
		[ProtoIgnore]
		public MatrixD CurrentPositionMatrix;
		
		[ProtoIgnore]
		public Random Rnd;

		public SpawnProfile() {

			UseSpawn = false;
			StartsReady = false;
			FirstSpawnTimeMs = 0;
			SpawnMinCooldown = 0;
			SpawnMaxCooldown = 1;
			MaxSpawns = -1;
			SpawnGroups = new List<string>();

			CooldownTime = 0;
			SpawnCount = 0;
			LastSpawnTime = MyAPIGateway.Session.GameDateTime;
			
			UseRelativeSpawnPosition = false;
			MinDistance = 0;
			MaxDistance = 1;
			MinAltitude = 0;
			MaxAltitude = 1;
			RelativeSpawnOffset = Vector3D.Zero;
			RelativeSpawnVelocity = Vector3D.Zero;
			IgnoreSafetyChecks = false;
			InheritNpcAltitude = false;
			
			CurrentPositionMatrix = MatrixD.Identity;
			Rnd = new Random();

		}

		public bool IsReadyToSpawn() {

			if (this.UseSpawn == false) 
				return false;

			if (MaxSpawns >= 0 && SpawnCount >= MaxSpawns) {

				UseSpawn = false;
				return false;

			}

			TimeSpan duration = MyAPIGateway.Session.GameDateTime - LastSpawnTime;

			if (duration.TotalSeconds < CooldownTime) {

				if (StartsReady == true) {

					if (SpawnCount > 0)
						return false;

				} else {

					return false;

				}

			}

			return true;

		}

		public void ProcessSuccessfulSpawn() {

			SpawnCount++;

			if (MaxSpawns >= 0 && SpawnCount >= MaxSpawns) {

				UseSpawn = false;

			}

			TimeSpan duration = MyAPIGateway.Session.GameDateTime - LastSpawnTime;

			if(duration.TotalSeconds < CooldownTime) {

				return;

			}

		}

		public void InitTags(string customData) {

			if(string.IsNullOrWhiteSpace(customData) == false) {

				var descSplit = customData.Split('\n');

				foreach(var tag in descSplit) {

					//UseSpawn
					if(tag.Contains("[UseSpawn:") == true) {

						UseSpawn = TagHelper.TagBoolCheck(tag);

					}

					//FirstSpawnTimeMs
					if(tag.Contains("[FirstSpawnTimeMs:") == true) {

						FirstSpawnTimeMs = TagHelper.TagFloatCheck(tag, FirstSpawnTimeMs);

					}

					//SpawnMinCooldown
					if (tag.Contains("[SpawnMinCooldown:") == true) {

						SpawnMinCooldown = TagHelper.TagFloatCheck(tag, SpawnMinCooldown);

					}

					//SpawnMaxCooldown
					if (tag.Contains("[SpawnMaxCooldown:") == true) {

						SpawnMaxCooldown = TagHelper.TagFloatCheck(tag, SpawnMaxCooldown);

					}

					//MaxSpawns
					if(tag.Contains("MaxSpawns:") == true) {

						MaxSpawns = TagHelper.TagIntCheck(tag, MaxSpawns);

					}

					//SpawnGroups
					if(tag.Contains("SpawnGroups:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempvalue) == false) {

							SpawnGroups.Add(tempvalue);

						}

					}

					//UseRelativeSpawnPosition
					if (tag.Contains("[UseRelativeSpawnPosition:") == true) {

						UseRelativeSpawnPosition = TagHelper.TagBoolCheck(tag);

					}

					//MinDistance
					if (tag.Contains("[MinDistance:") == true) {

						MinDistance = TagHelper.TagDoubleCheck(tag, MinDistance);

					}

					//MaxDistance
					if (tag.Contains("[MaxDistance:") == true) {

						MaxDistance = TagHelper.TagDoubleCheck(tag, MaxDistance);

					}

					//MinAltitude
					if (tag.Contains("[MinAltitude:") == true) {

						MinAltitude = TagHelper.TagDoubleCheck(tag, MinAltitude);

					}

					//MaxAltitude
					if (tag.Contains("[MaxAltitude:") == true) {

						MaxAltitude = TagHelper.TagDoubleCheck(tag, MaxAltitude);

					}

					//RelativeSpawnOffset
					if (tag.Contains("[RelativeSpawnOffset:") == true) {

						RelativeSpawnOffset = TagHelper.TagVector3DCheck(tag);

					}

					//RelativeSpawnVelocity
					if (tag.Contains("[RelativeSpawnVelocity:") == true) {

						RelativeSpawnVelocity = TagHelper.TagVector3DCheck(tag);

					}

					//IgnoreSafetyChecks
					if (tag.Contains("[IgnoreSafetyChecks:") == true) {

						IgnoreSafetyChecks = TagHelper.TagBoolCheck(tag);

					}
					
					//InheritNpcAltitude
					if (tag.Contains("[InheritNpcAltitude:") == true) {

						InheritNpcAltitude = TagHelper.TagBoolCheck(tag);

					}

				}

			}

			if(SpawnMinCooldown > SpawnMaxCooldown) {

				SpawnMinCooldown = SpawnMaxCooldown;

			}

		}

	}
}
