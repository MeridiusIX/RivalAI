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

namespace RivalAI.Behavior.Subsystems.Trigger {

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

		[ProtoMember(20)]
		public string ProfileSubtypeId;

		[ProtoMember(21)]
		public bool ForceSameFactionOwnership;

		[ProtoMember(22)]
		public SpawnTypeEnum SpawningType;

		[ProtoMember(23)]
		public Direction CustomRelativeForward;

		[ProtoMember(24)]
		public Direction CustomRelativeUp;

		[ProtoIgnore]
		public MatrixD CurrentPositionMatrix;

		[ProtoIgnore]
		public string CurrentFactionTag;

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
			ProfileSubtypeId = "";

			ForceSameFactionOwnership = false;

			SpawningType = SpawnTypeEnum.CustomSpawn;

			CustomRelativeForward = Direction.None;
			CustomRelativeUp = Direction.None;

			CurrentPositionMatrix = MatrixD.Identity;
			CurrentFactionTag = "";
			Rnd = new Random();

		}

		public void AssignInitialMatrix(MatrixD initialMatrix) {

			var position = initialMatrix.Translation;
			var forward = initialMatrix.Forward;
			var up = initialMatrix.Up;

			if (CustomRelativeForward != Direction.None)
				forward = GetDirectionFromMatrixAndEnum(initialMatrix, CustomRelativeForward);

			if (CustomRelativeUp != Direction.None)
				up = GetDirectionFromMatrixAndEnum(initialMatrix, CustomRelativeUp);

			if (Vector3D.ArePerpendicular(ref forward, ref up))
				CurrentPositionMatrix = MatrixD.CreateWorld(position, forward, up);
			else {

				CurrentPositionMatrix = initialMatrix;
				Logger.MsgDebug(string.Format("Warning: Custom Spawn Directions [{0}] and [{1}] are not perpendicular. Using default directions instead", CustomRelativeForward, CustomRelativeUp), DebugTypeEnum.Spawn);

			}
		}

		public Vector3D GetDirectionFromMatrixAndEnum(MatrixD matrix, Direction direction) {

			if (direction == Direction.None)
				return Vector3D.Zero;

			if (direction == Direction.Forward)
				return matrix.Forward;

			if (direction == Direction.Backward)
				return matrix.Backward;

			if (direction == Direction.Left)
				return matrix.Left;

			if (direction == Direction.Right)
				return matrix.Right;

			if (direction == Direction.Up)
				return matrix.Up;

			return matrix.Down;

		}

		public bool IsReadyToSpawn() {

			if (UseSpawn == false)
				return false;

			if (MaxSpawns >= 0 && SpawnCount >= MaxSpawns) {

				Logger.MsgDebug(ProfileSubtypeId + ": Max Spawns Already Exceeded", DebugTypeEnum.Spawn);
				UseSpawn = false;
				return false;

			}

			TimeSpan duration = MyAPIGateway.Session.GameDateTime - LastSpawnTime;

			if (duration.TotalSeconds < CooldownTime) {

				if (StartsReady == true) {

					Logger.MsgDebug(ProfileSubtypeId + ": Spawn Cooldown Not Finished", DebugTypeEnum.Spawn);
					if (SpawnCount > 0)
						return false;

				} else {

					Logger.MsgDebug(ProfileSubtypeId + ": Spawn Cooldown Not Finished", DebugTypeEnum.Spawn);
					return false;

				}

			}

			Logger.MsgDebug(ProfileSubtypeId + ": Spawn Cooldown Finished", DebugTypeEnum.Spawn);
			return true;

		}

		public void ProcessSuccessfulSpawn() {

			SpawnCount++;

			if (MaxSpawns >= 0 && SpawnCount >= MaxSpawns) {

				UseSpawn = false;

			}

			TimeSpan duration = MyAPIGateway.Session.GameDateTime - LastSpawnTime;

			if (duration.TotalSeconds < CooldownTime) {

				return;

			}

		}

		public void InitTags(string customData) {

			if (string.IsNullOrWhiteSpace(customData) == false) {

				var descSplit = customData.Split('\n');

				foreach (var tag in descSplit) {

					//UseSpawn
					if (tag.Contains("[UseSpawn:") == true) {

						UseSpawn = TagHelper.TagBoolCheck(tag);

					}

					//FirstSpawnTimeMs
					if (tag.Contains("[FirstSpawnTimeMs:") == true) {

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
					if (tag.Contains("MaxSpawns:") == true) {

						MaxSpawns = TagHelper.TagIntCheck(tag, MaxSpawns);

					}

					//SpawnGroups
					if (tag.Contains("SpawnGroups:") == true) {

						var tempvalue = TagHelper.TagStringCheck(tag);

						if (string.IsNullOrWhiteSpace(tempvalue) == false) {

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

					//ForceSameFactionOwnership
					if (tag.Contains("[ForceSameFactionOwnership:") == true) {

						ForceSameFactionOwnership = TagHelper.TagBoolCheck(tag);

					}

					//SpawningType
					if (tag.Contains("[SpawningType:") == true) {

						SpawningType = TagHelper.TagSpawnTypeEnumCheck(tag);

					}

					//CustomRelativeForward
					if (tag.Contains("[CustomRelativeForward:") == true) {

						CustomRelativeForward = TagHelper.TagDirectionEnumCheck(tag);

					}

					//CustomRelativeUp
					if (tag.Contains("[CustomRelativeUp:") == true) {

						CustomRelativeUp = TagHelper.TagDirectionEnumCheck(tag);

					}

				}

			}

			if (SpawnMinCooldown > SpawnMaxCooldown) {

				SpawnMinCooldown = SpawnMaxCooldown;

			}

		}

	}
}
