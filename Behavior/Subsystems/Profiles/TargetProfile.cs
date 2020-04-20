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
	public class TargetProfile {

		public bool UseCustomTargeting;

		public int TimeUntilTargetAcquisition;

		public bool UseTargetRefresh;

		public int TimeUntilNextRefresh;

		public bool UseTargetLastKnownPosition;

		public int TimeUntilNextEvaluation;

		public TargetTypeEnum Target;

		public BlockTargetTypes BlockTargets;

		public TargetDistanceEnum Distance;

		public double MaxDistance;

		public List<TargetFilterEnum> MatchAllFilters;

		public List<TargetFilterEnum> MatchAnyFilters;

		public TargetOwnerEnum Owners;

		public TargetRelationEnum Relations;

		public double MinAltitude;

		public double MaxAltitude;

		public double NonBroadcastVisualRange;

		public double MinGravity;

		public double MaxGravity;

		public double MinSpeed;

		public double MaxSpeed;

		public string ProfileSubtypeId;

		public bool BuiltUniqueFilterList;

		public List<TargetFilterEnum> AllUniqueFilters;



		public TargetProfile() {

			UseCustomTargeting = false;

			TimeUntilTargetAcquisition = 1;
			UseTargetRefresh = false;
			TimeUntilNextRefresh = 60;
			TimeUntilNextEvaluation = 1;

			Target = TargetTypeEnum.None;
			BlockTargets = BlockTargetTypes.None;

			Distance = TargetDistanceEnum.Closest;
			MaxDistance = 12000;

			MatchAllFilters = new List<TargetFilterEnum>();
			MatchAnyFilters = new List<TargetFilterEnum>();

			Owners = TargetOwnerEnum.None;
			Relations = TargetRelationEnum.None;

			MinAltitude = 0;
			MaxAltitude = 10000;

			NonBroadcastVisualRange = 1500;

			MinGravity = 0;
			MaxGravity = 1.1;

			MinSpeed = 0;
			MaxSpeed = 110;

			ProfileSubtypeId = "";
			BuiltUniqueFilterList = false;
			AllUniqueFilters = new List<TargetFilterEnum>();

		}
		
		public void InitTags(string customData) {

			if(string.IsNullOrWhiteSpace(customData) == false) {

				var descSplit = customData.Split('\n');

				foreach(var tag in descSplit) {

					//UseCustomTargeting
					if(tag.Contains("[UseCustomTargeting:") == true) {

						this.UseCustomTargeting = TagHelper.TagBoolCheck(tag);

					}
					
					//BlockTargets
					if(tag.Contains("[BlockTargets:") == true) {

						var tempValue = TagHelper.TagBlockTargetTypesCheck(tag);

						if(this.BlockTargets.HasFlag(tempValue) == false) {

							this.BlockTargets |= tempValue;

						}

					}
					
					//Distance
					if(tag.Contains("[Distance:") == true) {

						this.Distance = TagHelper.TagTargetDistanceEnumCheck(tag);

					}
					
					//Owners
					if(tag.Contains("[Owners:") == true) {

						var tempValue = TagHelper.TagTargetOwnerEnumCheck(tag);

						if(this.Owners.HasFlag(tempValue) == false) {

							this.Owners |= tempValue;

						}

					}
					
					//Relations
					if(tag.Contains("[Relations:") == true) {

						var tempValue = TagHelper.TagTargetRelationEnumCheck(tag);

						if(this.Relations.HasFlag(tempValue) == false) {

							this.Relations |= tempValue;

						}

					}
					
					//Target
					if(tag.Contains("[Target:") == true) {

						this.Target = TagHelper.TagTargetTypeEnumCheck(tag);

					}
					
					//NonBroadcastingMaxDistance
					if(tag.Contains("[NonBroadcastingMaxDistance:") == true) {

						this.NonBroadcastVisualRange = TagHelper.TagDoubleCheck(tag, this.NonBroadcastVisualRange);

					}
					
					//MaxDistance
					if(tag.Contains("[MaxDistance:") == true) {

						this.MaxDistance = TagHelper.TagDoubleCheck(tag, this.MaxDistance);

					}
					
				}

			}

		}

	}
}
