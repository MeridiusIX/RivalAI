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

		}

		public void MinMaxSanityChecks() {

			MathTools.MinMaxRangeSafety(ref OffsetSpaceMinDistFromTarget, ref OffsetSpaceMaxDistFromTarget);
			MathTools.MinMaxRangeSafety(ref OffsetPlanetMinDistFromTarget, ref OffsetPlanetMaxDistFromTarget);
			MathTools.MinMaxRangeSafety(ref OffsetPlanetMinTargetAltitude, ref OffsetPlanetMaxTargetAltitude);

			MathTools.MinMaxRangeSafety(ref StrafeMinDurationMs, ref StrafeMaxDurationMs);
			MathTools.MinMaxRangeSafety(ref StrafeMinCooldownMs, ref StrafeMaxCooldownMs);

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



		}

	}

}
