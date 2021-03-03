using ProtoBuf;
using RivalAI.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RivalAI.Behavior.Subsystems.AutoPilot {

	[ProtoContract]
	public class AutoPilotProfile {

		//Profile
		[ProtoMember(1)]
		public string ProfileSubtypeId;

		//Speed Config
		[ProtoMember(2)]
		public float IdealMaxSpeed;

		[ProtoMember(3)]
		public float IdealMinSpeed;

		[ProtoMember(4)]
		public bool SlowDownOnWaypointApproach;

		[ProtoMember(5)]
		public double ExtraSlowDownDistance;

		[ProtoMember(6)]
		public float MaxSpeedTolerance;

		//Planet Config
		[ProtoMember(7)]
		public bool FlyLevelWithGravity;

		[ProtoMember(8)]
		public bool LevelWithGravityWhenIdle;

		[ProtoMember(9)]
		public double MaxPlanetPathCheckDistance;

		[ProtoMember(10)]
		public double IdealPlanetAltitude;

		[ProtoMember(11)]
		public double MinimumPlanetAltitude;

		[ProtoMember(12)]
		public double AltitudeTolerance;

		[ProtoMember(13)]
		public double WaypointTolerance;

		//Offset Space Config
		[ProtoMember(14)]
		public double OffsetSpaceMinDistFromTarget;

		[ProtoMember(15)]
		public double OffsetSpaceMaxDistFromTarget;

		//Offset Planet Config
		[ProtoMember(16)]
		public double OffsetPlanetMinDistFromTarget;

		[ProtoMember(17)]
		public double OffsetPlanetMaxDistFromTarget;

		[ProtoMember(18)]
		public double OffsetPlanetMinTargetAltitude;

		[ProtoMember(19)]
		public double OffsetPlanetMaxTargetAltitude;

		//Collision Config
		[ProtoMember(20)]
		public bool UseVelocityCollisionEvasion;

		[ProtoMember(21)]
		public double CollisionEvasionWaypointDistance; //Make Space and Planet Variant - OR... Make This Based on Detection Type!

		[ProtoMember(22)]
		public double CollisionFallEvasionWaypointDistance;

		[ProtoMember(23)]
		public double CollisionEvasionResumeDistance;

		[ProtoMember(24)]
		public int CollisionEvasionResumeTime;

		[ProtoMember(25)]
		public bool CollisionEvasionWaypointCalculatedAwayFromEntity;

		[ProtoMember(26)]
		public double CollisionEvasionWaypointFromEntityMaxAngle;

		//Lead Config

		[ProtoMember(27)]
		public bool UseProjectileLeadPrediction;

		[ProtoMember(28)]
		public bool UseCollisionLeadPrediction;

		//Thrust Settings
		[ProtoMember(29)]
		public double AngleAllowedForForwardThrust;

		[ProtoMember(30)]
		public double MaxVelocityAngleForSpeedControl;

		//Strafe Settings
		[ProtoMember(31)]
		public bool AllowStrafing;

		[ProtoMember(32)]
		public int StrafeMinDurationMs;

		[ProtoMember(33)]
		public int StrafeMaxDurationMs;

		[ProtoMember(34)]
		public int StrafeMinCooldownMs;

		[ProtoMember(35)]
		public int StrafeMaxCooldownMs;

		[ProtoMember(36)]
		public double StrafeSpeedCutOff;

		[ProtoMember(37)]
		public double StrafeDistanceCutOff;

		[ProtoMember(38)]
		public double StrafeMinimumTargetDistance;

		[ProtoMember(39)]
		public double StrafeMinimumSafeAngleFromTarget;

		//Rotation Settings
		[ProtoMember(40)]
		public float RotationMultiplier;

		[ProtoMember(41)]
		public double DesiredAngleToTarget;

		[ProtoMember(42)]
		public bool DisableInertiaDampeners;

		[ProtoMember(43)]
		public bool ReverseOffsetDistAltAboveHeight;

		[ProtoMember(44)]
		public double ReverseOffsetHeight;

		[ProtoMember(45)]
		public double PadDistanceFromTarget;

		[ProtoMember(46)]
		public int BarrelRollMinDurationMs;

		[ProtoMember(47)]
		public int BarrelRollMaxDurationMs;

		[ProtoMember(48)]
		public int RamMinDurationMs;

		[ProtoMember(49)]
		public int RamMaxDurationMs;

		[ProtoMember(50)]
		public double EngageDistanceSpace;

		[ProtoMember(51)]
		public double EngageDistancePlanet;

		[ProtoMember(52)]
		public double DisengageDistanceSpace;

		[ProtoMember(53)]
		public double DisengageDistancePlanet;

		[ProtoMember(54)]
		public int WaypointWaitTimeTrigger;

		[ProtoMember(55)]
		public int WaypointAbandonTimeTrigger;

		[ProtoMember(56)]
		public double AttackRunDistanceSpace;

		[ProtoMember(57)]
		public double AttackRunDistancePlanet;

		[ProtoMember(58)]
		public double AttackRunBreakawayDistance;

		[ProtoMember(59)]
		public int OffsetRecalculationTime;

		[ProtoMember(60)]
		public bool AttackRunUseSafePlanetPathing;

		[ProtoMember(61)]
		public bool AttackRunUseCollisionEvasionSpace;

		[ProtoMember(62)]
		public bool AttackRunUseCollisionEvasionPlanet;

		[ProtoMember(63)]
		public bool AttackRunOverrideWithDistanceAndTimer;

		[ProtoMember(64)]
		public int AttackRunOverrideTimerTrigger;

		[ProtoMember(65)]
		public double AttackRunOverrideDistance;

		[ProtoMember(66)]
		public double DespawnCoordsMinDistance;

		[ProtoMember(67)]
		public double DespawnCoordsMaxDistance;

		[ProtoMember(68)]
		public double DespawnCoordsMinAltitude;

		[ProtoMember(69)]
		public double DespawnCoordsMaxAltitude;

		[ProtoMember(70)]
		public double MinAngleForLeveledDescent;

		[ProtoMember(71)]
		public double MaxAngleForLeveledAscent;

		[ProtoMember(72)]
		public bool LimitRotationSpeed;

		[ProtoMember(73)]
		public double MaxRotationMagnitude;

		[ProtoMember(74)]
		public double MinGravity;

		[ProtoMember(75)]
		public double MaxGravity;

		[ProtoMember(76)]
		public bool AvoidPlayerCollisions;

		[ProtoMember(77)]
		public bool UseSurfaceHoverThrustMode;

		[ProtoMember(78)]
		public double MaxVerticalSpeed;

		[ProtoMember(79)]
		public double HoverPathStepDistance;

		public AutoPilotProfile() {

			ProfileSubtypeId = "";

			DisableInertiaDampeners = false;
			IdealMaxSpeed = 100;
			IdealMinSpeed = 10;
			SlowDownOnWaypointApproach = false;
			ExtraSlowDownDistance = 25; 
			MaxSpeedTolerance = 15;

			FlyLevelWithGravity = false; 
			LevelWithGravityWhenIdle = false; 
			MaxPlanetPathCheckDistance = 1000;
			IdealPlanetAltitude = 200;
			MinimumPlanetAltitude = 110;
			AltitudeTolerance = 10;
			WaypointTolerance = 10;

			OffsetSpaceMinDistFromTarget = 100;
			OffsetSpaceMaxDistFromTarget = 200;

			OffsetPlanetMinDistFromTarget = 100;
			OffsetPlanetMaxDistFromTarget = 200;
			OffsetPlanetMinTargetAltitude = 100;
			OffsetPlanetMaxTargetAltitude = 200;

			ReverseOffsetDistAltAboveHeight = false;
			ReverseOffsetHeight = 1300;

			PadDistanceFromTarget = 0;

			UseVelocityCollisionEvasion = true;
			CollisionEvasionWaypointDistance = 300;
			CollisionFallEvasionWaypointDistance = 75;
			CollisionEvasionResumeDistance = 25;
			CollisionEvasionResumeTime = 10;
			CollisionEvasionWaypointCalculatedAwayFromEntity = false;
			CollisionEvasionWaypointFromEntityMaxAngle = 15;

			UseProjectileLeadPrediction = true;
			UseCollisionLeadPrediction = false;

			AngleAllowedForForwardThrust = 35;
			MaxVelocityAngleForSpeedControl = 5;

			AllowStrafing = false;
			StrafeMinDurationMs = 750;
			StrafeMaxDurationMs = 1500;
			StrafeMinCooldownMs = 1000;
			StrafeMaxCooldownMs = 3000;
			StrafeSpeedCutOff = 100;
			StrafeDistanceCutOff = 100;

			StrafeMinimumTargetDistance = 250;
			StrafeMinimumSafeAngleFromTarget = 25;

			RotationMultiplier = 1;

			BarrelRollMinDurationMs = 3000;
			BarrelRollMaxDurationMs = 5000;

			RamMinDurationMs = 7000;
			RamMaxDurationMs = 12000;

			EngageDistanceSpace = 500;
			EngageDistancePlanet = 500;
			DisengageDistanceSpace = 600;
			DisengageDistancePlanet = 600;

			WaypointWaitTimeTrigger = 5;
			WaypointAbandonTimeTrigger = 30;

			AttackRunDistanceSpace = 0;
			AttackRunDistancePlanet = 0;
			AttackRunBreakawayDistance = 0;
			OffsetRecalculationTime = 0;
			AttackRunUseSafePlanetPathing = false;
			AttackRunUseCollisionEvasionSpace = false;
			AttackRunUseCollisionEvasionPlanet = false;
			AttackRunOverrideWithDistanceAndTimer = false;
			AttackRunOverrideTimerTrigger = 0;
			AttackRunOverrideDistance = 0;

			DespawnCoordsMinDistance = 8000;
			DespawnCoordsMaxDistance = 11000;

			DespawnCoordsMinAltitude = 1500;
			DespawnCoordsMaxAltitude = 2500;

			MinAngleForLeveledDescent = 0;
			MaxAngleForLeveledAscent = 180;

			LimitRotationSpeed = false;
			MaxRotationMagnitude = 6.28;

			MinGravity = -1;
			MaxGravity = -1;

			AvoidPlayerCollisions = true;

			UseSurfaceHoverThrustMode = false;
			HoverPathStepDistance = 50;

			MaxVerticalSpeed = -1;

		}

		public void MinMaxSanityChecks() {

			MathTools.MinMaxRangeSafety(ref OffsetSpaceMinDistFromTarget, ref OffsetSpaceMaxDistFromTarget);
			MathTools.MinMaxRangeSafety(ref OffsetPlanetMinDistFromTarget, ref OffsetPlanetMaxDistFromTarget);
			MathTools.MinMaxRangeSafety(ref OffsetPlanetMinTargetAltitude, ref OffsetPlanetMaxTargetAltitude);

			MathTools.MinMaxRangeSafety(ref StrafeMinDurationMs, ref StrafeMaxDurationMs);
			MathTools.MinMaxRangeSafety(ref StrafeMinCooldownMs, ref StrafeMaxCooldownMs);

			MathTools.MinMaxRangeSafety(ref BarrelRollMinDurationMs, ref BarrelRollMaxDurationMs);
			MathTools.MinMaxRangeSafety(ref RamMinDurationMs, ref RamMaxDurationMs);

		}

		public void InitTags(string tagData) {

			if (!string.IsNullOrWhiteSpace(tagData)) {

				var descSplit = tagData.Split('\n');

				foreach (var tag in descSplit) {

					InitTag(tag);

				}

			}
		
		}

		public void InitTag(string tag) {

			//DisableInertiaDampeners
			if (tag.Contains("[DisableInertiaDampeners:") == true) {

				this.DisableInertiaDampeners = TagHelper.TagBoolCheck(tag);

			}

			//IdealMaxSpeed
			if (tag.Contains("[IdealMaxSpeed:") == true) {

				this.IdealMaxSpeed = TagHelper.TagFloatCheck(tag, this.IdealMaxSpeed);

			}

			//IdealMinSpeed
			if (tag.Contains("[IdealMinSpeed:") == true) {

				this.IdealMinSpeed = TagHelper.TagFloatCheck(tag, this.IdealMinSpeed);

			}

			//SlowDownOnWaypointApproach
			if (tag.Contains("[SlowDownOnWaypointApproach:") == true) {

				this.SlowDownOnWaypointApproach = TagHelper.TagBoolCheck(tag);

			}

			//ExtraSlowDownDistance
			if (tag.Contains("[ExtraSlowDownDistance:") == true) {

				this.ExtraSlowDownDistance = TagHelper.TagDoubleCheck(tag, this.ExtraSlowDownDistance);

			}

			//MaxSpeedTolerance
			if (tag.Contains("[MaxSpeedTolerance:")) {

				this.MaxSpeedTolerance = TagHelper.TagFloatCheck(tag, this.MaxSpeedTolerance);

			}

			//FlyLevelWithGravity
			if (tag.Contains("[FlyLevelWithGravity:") == true) {

				this.FlyLevelWithGravity = TagHelper.TagBoolCheck(tag);

			}

			//LevelWithGravityWhenIdle
			if (tag.Contains("[LevelWithGravityWhenIdle:") == true) {

				this.LevelWithGravityWhenIdle = TagHelper.TagBoolCheck(tag);

			}

			//MaxPlanetPathCheckDistance
			if (tag.Contains("[MaxPlanetPathCheckDistance:") == true) {

				this.MaxPlanetPathCheckDistance = TagHelper.TagDoubleCheck(tag, this.MaxPlanetPathCheckDistance);

			}

			//IdealPlanetAltitude
			if (tag.Contains("[IdealPlanetAltitude:") == true) {

				this.IdealPlanetAltitude = TagHelper.TagDoubleCheck(tag, this.IdealPlanetAltitude);

			}

			//MinimumPlanetAltitude
			if (tag.Contains("[MinimumPlanetAltitude:") == true) {

				this.MinimumPlanetAltitude = TagHelper.TagDoubleCheck(tag, this.MinimumPlanetAltitude);

			}

			//AltitudeTolerance
			if (tag.Contains("[AltitudeTolerance:") == true) {

				this.AltitudeTolerance = TagHelper.TagDoubleCheck(tag, this.AltitudeTolerance);

			}

			//WaypointTolerance
			if (tag.Contains("[WaypointTolerance:") == true) {

				this.WaypointTolerance = TagHelper.TagDoubleCheck(tag, this.WaypointTolerance);

			}

			//OffsetSpaceMinDistFromTarget
			if (tag.Contains("[OffsetSpaceMinDistFromTarget:") == true) {

				this.OffsetSpaceMinDistFromTarget = TagHelper.TagDoubleCheck(tag, this.OffsetSpaceMinDistFromTarget);

			}

			//OffsetSpaceMaxDistFromTarget
			if (tag.Contains("[OffsetSpaceMaxDistFromTarget:") == true) {

				this.OffsetSpaceMaxDistFromTarget = TagHelper.TagDoubleCheck(tag, this.OffsetSpaceMaxDistFromTarget);

			}

			//OffsetPlanetMinDistFromTarget
			if (tag.Contains("[OffsetPlanetMinDistFromTarget:") == true) {

				this.OffsetPlanetMinDistFromTarget = TagHelper.TagDoubleCheck(tag, this.OffsetPlanetMinDistFromTarget);

			}

			//OffsetPlanetMaxDistFromTarget
			if (tag.Contains("[OffsetPlanetMaxDistFromTarget:") == true) {

				this.OffsetPlanetMaxDistFromTarget = TagHelper.TagDoubleCheck(tag, this.OffsetPlanetMaxDistFromTarget);

			}

			//OffsetPlanetMinTargetAltitude
			if (tag.Contains("[OffsetPlanetMinTargetAltitude:") == true) {

				this.OffsetPlanetMinTargetAltitude = TagHelper.TagDoubleCheck(tag, this.OffsetPlanetMinTargetAltitude);

			}

			//OffsetPlanetMaxTargetAltitude
			if (tag.Contains("[OffsetPlanetMaxTargetAltitude:") == true) {

				this.OffsetPlanetMaxTargetAltitude = TagHelper.TagDoubleCheck(tag, this.OffsetPlanetMaxTargetAltitude);

			}

			//ReverseOffsetDistAltAboveHeight
			if (tag.Contains("[ReverseOffsetDistAltAboveHeight:") == true) {

				this.ReverseOffsetDistAltAboveHeight = TagHelper.TagBoolCheck(tag);

			}

			//ReverseOffsetHeight
			if (tag.Contains("[ReverseOffsetHeight:") == true) {

				this.ReverseOffsetHeight = TagHelper.TagDoubleCheck(tag, this.ReverseOffsetHeight);

			}

			//UseVelocityCollisionEvasion
			if (tag.Contains("[UseVelocityCollisionEvasion:") == true) {

				this.UseVelocityCollisionEvasion = TagHelper.TagBoolCheck(tag);

			}

			//CollisionEvasionWaypointDistance
			if (tag.Contains("[CollisionEvasionWaypointDistance:") == true) {

				this.CollisionEvasionWaypointDistance = TagHelper.TagDoubleCheck(tag, this.CollisionEvasionWaypointDistance);

			}

			//CollisionFallEvasionWaypointDistance
			if (tag.Contains("[CollisionFallEvasionWaypointDistance:") == true) {

				this.CollisionFallEvasionWaypointDistance = TagHelper.TagDoubleCheck(tag, this.CollisionFallEvasionWaypointDistance);

			}

			//CollisionEvasionResumeDistance
			if (tag.Contains("[CollisionEvasionResumeDistance:") == true) {

				this.CollisionEvasionResumeDistance = TagHelper.TagDoubleCheck(tag, this.CollisionEvasionResumeDistance);

			}

			//CollisionEvasionResumeTime
			if (tag.Contains("[CollisionEvasionResumeTime:") == true) {

				this.CollisionEvasionResumeTime = TagHelper.TagIntCheck(tag, this.CollisionEvasionResumeTime);

			}

			//CollisionEvasionWaypointCalculatedAwayFromEntity
			if (tag.Contains("[CollisionEvasionWaypointCalculatedAwayFromEntity:") == true) {

				this.CollisionEvasionWaypointCalculatedAwayFromEntity = TagHelper.TagBoolCheck(tag);

			}

			//CollisionEvasionWaypointFromEntityMaxAngle
			if (tag.Contains("[CollisionEvasionWaypointFromEntityMaxAngle:") == true) {

				this.CollisionEvasionWaypointFromEntityMaxAngle = TagHelper.TagDoubleCheck(tag, this.CollisionEvasionWaypointFromEntityMaxAngle);

			}

			//UseProjectileLeadPrediction
			if (tag.Contains("[UseProjectileLeadPrediction:") == true) {

				this.UseProjectileLeadPrediction = TagHelper.TagBoolCheck(tag);

			}

			//UseCollisionLeadPrediction
			if (tag.Contains("[UseCollisionLeadPrediction:") == true) {

				this.UseCollisionLeadPrediction = TagHelper.TagBoolCheck(tag);

			}

			////////////////////
			//Rotation and Thrust
			////////////////////

			//RotationMultiplier
			if (tag.Contains("[RotationMultiplier:") == true) {

				this.RotationMultiplier = TagHelper.TagFloatCheck(tag, this.RotationMultiplier);

			}

			//AngleAllowedForForwardThrust
			if (tag.Contains("[AngleAllowedForForwardThrust:") == true) {

				this.AngleAllowedForForwardThrust = TagHelper.TagDoubleCheck(tag, this.AngleAllowedForForwardThrust);

			}

			//MaxVelocityAngleForSpeedControl
			if (tag.Contains("[MaxVelocityAngleForSpeedControl:") == true) {

				this.MaxVelocityAngleForSpeedControl = TagHelper.TagDoubleCheck(tag, this.MaxVelocityAngleForSpeedControl);

			}

			//AllowStrafing
			if (tag.Contains("[AllowStrafing:") == true) {

				this.AllowStrafing = TagHelper.TagBoolCheck(tag);

			}

			//StrafeMinDurationMs
			if (tag.Contains("[StrafeMinDurationMs:") == true) {

				this.StrafeMinDurationMs = TagHelper.TagIntCheck(tag, this.StrafeMinDurationMs);

			}

			//StrafeMaxDurationMs
			if (tag.Contains("[StrafeMaxDurationMs:") == true) {

				this.StrafeMaxDurationMs = TagHelper.TagIntCheck(tag, this.StrafeMaxDurationMs);

			}

			//StrafeMinCooldownMs
			if (tag.Contains("[StrafeMinCooldownMs:") == true) {

				this.StrafeMinCooldownMs = TagHelper.TagIntCheck(tag, this.StrafeMinCooldownMs);

			}

			//StrafeMaxCooldownMs
			if (tag.Contains("[StrafeMaxCooldownMs:") == true) {

				this.StrafeMaxCooldownMs = TagHelper.TagIntCheck(tag, this.StrafeMaxCooldownMs);

			}

			//StrafeSpeedCutOff
			if (tag.Contains("[StrafeSpeedCutOff:") == true) {

				this.StrafeSpeedCutOff = TagHelper.TagDoubleCheck(tag, this.StrafeSpeedCutOff);

			}

			//StrafeDistanceCutOff
			if (tag.Contains("[StrafeDistanceCutOff:") == true) {

				this.StrafeDistanceCutOff = TagHelper.TagDoubleCheck(tag, this.StrafeDistanceCutOff);

			}

			//StrafeMinimumTargetDistance
			if (tag.Contains("[StrafeMinimumTargetDistance:") == true) {

				this.StrafeMinimumTargetDistance = TagHelper.TagDoubleCheck(tag, this.StrafeMinimumTargetDistance);

			}

			//StrafeMinimumSafeAngleFromTarget
			if (tag.Contains("[StrafeMinimumSafeAngleFromTarget:") == true) {

				this.StrafeMinimumSafeAngleFromTarget = TagHelper.TagDoubleCheck(tag, this.StrafeMinimumSafeAngleFromTarget);

			}

			////////////////////
			//The Rest
			////////////////////

			//PadDistanceFromTarget
			if (tag.Contains("[PadDistanceFromTarget:") == true) {

				this.PadDistanceFromTarget = TagHelper.TagDoubleCheck(tag, this.PadDistanceFromTarget);

			}

			//BarrelRollMinDurationMs
			if (tag.Contains("[BarrelRollMinDurationMs:") == true) {

				this.BarrelRollMinDurationMs = TagHelper.TagIntCheck(tag, this.BarrelRollMinDurationMs);

			}

			//BarrelRollMaxDurationMs
			if (tag.Contains("[BarrelRollMaxDurationMs:") == true) {

				this.BarrelRollMaxDurationMs = TagHelper.TagIntCheck(tag, this.BarrelRollMaxDurationMs);

			}

			//RamMinDurationMs
			if (tag.Contains("[RamMinDurationMs:") == true) {

				this.RamMinDurationMs = TagHelper.TagIntCheck(tag, this.RamMinDurationMs);

			}

			//RamMaxDurationMs
			if (tag.Contains("[RamMaxDurationMs:") == true) {

				this.RamMaxDurationMs = TagHelper.TagIntCheck(tag, this.RamMaxDurationMs);

			}

			//EngageDistanceSpace
			if (tag.Contains("[EngageDistanceSpace:") == true) {

				this.EngageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.EngageDistanceSpace);

			}

			//EngageDistancePlanet
			if (tag.Contains("[EngageDistancePlanet:") == true) {

				this.EngageDistancePlanet = TagHelper.TagDoubleCheck(tag, this.EngageDistancePlanet);

			}

			//DisengageDistanceSpace
			if (tag.Contains("[DisengageDistanceSpace:") == true) {

				this.DisengageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.DisengageDistanceSpace);

			}

			//DisengageDistancePlanet
			if (tag.Contains("[DisengageDistancePlanet:") == true) {

				this.DisengageDistancePlanet = TagHelper.TagDoubleCheck(tag, this.DisengageDistancePlanet);

			}

			//WaypointWaitTimeTrigger
			if (tag.Contains("[WaypointWaitTimeTrigger:") == true) {

				this.WaypointWaitTimeTrigger = TagHelper.TagIntCheck(tag, this.WaypointWaitTimeTrigger);

			}

			//WaypointAbandonTimeTrigger
			if (tag.Contains("[WaypointAbandonTimeTrigger:") == true) {

				this.WaypointAbandonTimeTrigger = TagHelper.TagIntCheck(tag, this.WaypointAbandonTimeTrigger);

			}

			//AttackRunDistanceSpace
			if (tag.Contains("[AttackRunDistanceSpace:") == true) {

				this.AttackRunDistanceSpace = TagHelper.TagDoubleCheck(tag, this.AttackRunDistanceSpace);

			}

			//AttackRunDistancePlanet
			if (tag.Contains("[AttackRunDistancePlanet:") == true) {

				this.AttackRunDistancePlanet = TagHelper.TagDoubleCheck(tag, this.AttackRunDistancePlanet);

			}

			//AttackRunBreakawayDistance
			if (tag.Contains("[AttackRunBreakawayDistance:") == true) {

				this.AttackRunBreakawayDistance = TagHelper.TagDoubleCheck(tag, this.AttackRunBreakawayDistance);

			}

			//OffsetRecalculationTime
			if (tag.Contains("[OffsetRecalculationTime:") == true) {

				this.OffsetRecalculationTime = TagHelper.TagIntCheck(tag, this.OffsetRecalculationTime);

			}

			//AttackRunUseSafePlanetPathing
			if (tag.Contains("[AttackRunUseSafePlanetPathing:") == true) {

				this.AttackRunUseSafePlanetPathing = TagHelper.TagBoolCheck(tag);

			}

			//AttackRunUseCollisionEvasionSpace
			if (tag.Contains("[AttackRunUseCollisionEvasionSpace:") == true) {

				this.AttackRunUseCollisionEvasionSpace = TagHelper.TagBoolCheck(tag);

			}

			//AttackRunUseCollisionEvasionPlanet
			if (tag.Contains("[AttackRunUseCollisionEvasionPlanet:") == true) {

				this.AttackRunUseCollisionEvasionPlanet = TagHelper.TagBoolCheck(tag);

			}

			//AttackRunOverrideWithDistanceAndTimer
			if (tag.Contains("[AttackRunOverrideWithDistanceAndTimer:") == true) {

				this.AttackRunOverrideWithDistanceAndTimer = TagHelper.TagBoolCheck(tag);

			}

			//AttackRunOverrideTimerTrigger
			if (tag.Contains("[AttackRunOverrideTimerTrigger:") == true) {

				this.AttackRunOverrideTimerTrigger = TagHelper.TagIntCheck(tag, this.AttackRunOverrideTimerTrigger);

			}

			//AttackRunOverrideDistance
			if (tag.Contains("[AttackRunOverrideDistance:") == true) {

				this.AttackRunOverrideDistance = TagHelper.TagDoubleCheck(tag, this.AttackRunOverrideDistance);

			}

			//DespawnCoordsMinDistance
			if (tag.Contains("[DespawnCoordsMinDistance:") == true) {

				this.DespawnCoordsMinDistance = TagHelper.TagDoubleCheck(tag, this.DespawnCoordsMinDistance);

			}

			//DespawnCoordsMaxDistance
			if (tag.Contains("[DespawnCoordsMaxDistance:") == true) {

				this.DespawnCoordsMaxDistance = TagHelper.TagDoubleCheck(tag, this.DespawnCoordsMaxDistance);

			}

			//DespawnCoordsMinAltitude
			if (tag.Contains("[DespawnCoordsMinAltitude:") == true) {

				this.DespawnCoordsMinAltitude = TagHelper.TagDoubleCheck(tag, this.DespawnCoordsMinAltitude);

			}

			//DespawnCoordsMaxAltitude
			if (tag.Contains("[DespawnCoordsMaxAltitude:") == true) {

				this.DespawnCoordsMaxAltitude = TagHelper.TagDoubleCheck(tag, this.DespawnCoordsMaxAltitude);

			}

			//MinAngleForLeveledDescent
			if (tag.Contains("[MinAngleForLeveledDescent:") == true) {

				this.MinAngleForLeveledDescent = TagHelper.TagDoubleCheck(tag, this.MinAngleForLeveledDescent);

			}

			//MaxAngleForLeveledAscent
			if (tag.Contains("[MaxAngleForLeveledAscent:") == true) {

				this.MaxAngleForLeveledAscent = TagHelper.TagDoubleCheck(tag, this.MaxAngleForLeveledAscent);

			}

			//LimitRotationSpeed
			if (tag.Contains("[LimitRotationSpeed:") == true) {

				this.LimitRotationSpeed = TagHelper.TagBoolCheck(tag);

			}

			//MaxRotationMagnitude
			if (tag.Contains("[MaxRotationMagnitude:") == true) {

				this.MaxRotationMagnitude = TagHelper.TagDoubleCheck(tag, this.MaxRotationMagnitude);

			}

			//MinGravity
			if (tag.Contains("[MinGravity:") == true) {

				this.MinGravity = TagHelper.TagDoubleCheck(tag, this.MinGravity);

			}

			//MaxGravity
			if (tag.Contains("[MaxGravity:") == true) {

				this.MaxGravity = TagHelper.TagDoubleCheck(tag, this.MaxGravity);

			}

			//AvoidPlayerCollisions
			if (tag.Contains("[AvoidPlayerCollisions:") == true) {

				this.AvoidPlayerCollisions = TagHelper.TagBoolCheck(tag);

			}

			//UseSurfaceHoverThrustMode
			if (tag.Contains("[UseSurfaceHoverThrustMode:") == true) {

				this.UseSurfaceHoverThrustMode = TagHelper.TagBoolCheck(tag);

			}

			//MaxVerticalSpeed
			if (tag.Contains("[MaxVerticalSpeed:") == true) {

				this.MaxVerticalSpeed = TagHelper.TagDoubleCheck(tag, this.MaxVerticalSpeed);

			}

			//HoverPathStepDistance
			if (tag.Contains("[HoverPathStepDistance:") == true) {

				this.HoverPathStepDistance = TagHelper.TagDoubleCheck(tag, this.HoverPathStepDistance);

			}

		}

	}

}
