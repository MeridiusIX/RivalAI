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
using Sandbox.Game.Weapons;
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
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Behavior.Subsystems{
	
	public class TargetingSystem{

		//Configurable
		public TargetTypeEnum TargetType;
		public TargetRelationEnum TargetRelation;
		public TargetDistanceEnum TargetDistance;
		public TargetOwnerEnum TargetOwner;
		public TargetFilterEnum TargetFilter;
		public BlockTargetTypes BlockFilter;
		public double MaximumTargetScanDistance;

		public TargetProfile TargetData;

		//Non-Configurable
		public IMyRemoteControl RemoteControl;

		public DateTime LastNewTargetCheck;
		public int TimeUntilNewTarget;

		public bool NeedsTarget;
		public bool UpdateTargetRequested;
		public long UpdateSpecificTarget;
		public bool SearchingForTarget;
		public bool InvalidTarget;
		public bool TargetIsShielded;
		public Vector3D TargetCoords;
		public IMyPlayer TargetPlayer;
		public IMyEntity TargetEntity;
		public IMyCubeGrid TargetGrid;
		public IMyTerminalBlock TargetBlock;
		public long RequestedGridId;
		public long RequestedBlockId;

		
		public TargetEvaluation Target;
		public event Action<TargetEvaluation> WeaponTrigger;

		public float PrimaryAmmoVelocity;

		public Random Rnd;
		
		public TargetingSystem(IMyRemoteControl remoteControl = null) {
			
			RemoteControl = null;

			LastNewTargetCheck = MyAPIGateway.Session.GameDateTime;

			NeedsTarget = false;
			SearchingForTarget = false;
			TargetType = TargetTypeEnum.None;
			InvalidTarget = true;
			TargetIsShielded = false;
			TargetCoords = Vector3D.Zero;
			TargetPlayer = null;
			TargetEntity = null;
			TargetGrid = null;
			TargetBlock = null;
			RequestedGridId = 0;
			RequestedBlockId = 0;

			Target = new TargetEvaluation(null, TargetTypeEnum.None);

			TargetData = new TargetProfile();
			TargetDistance = TargetDistanceEnum.Closest;
			MaximumTargetScanDistance = 15000;

			BlockFilter = BlockTargetTypes.All;
			
			PrimaryAmmoVelocity = 0;

			Rnd = new Random();

			Setup(remoteControl);


		}
		
		private void Setup(IMyRemoteControl remoteControl) {

			if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false) {

				return;

			}

			this.RemoteControl = remoteControl;

		}

		public void InitTags() {

			if(string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach(var tag in descSplit) {

					//TargetData
					if(tag.Contains("[TargetData:") == true) {

						var tempValue = TagHelper.TagStringCheck(tag);

						if(string.IsNullOrWhiteSpace(tempValue) == false) {

							byte[] byteData = { };

							if(TagHelper.TargetObjectTemplates.TryGetValue(tempValue, out byteData) == true) {

								try {

									var profile = MyAPIGateway.Utilities.SerializeFromBinary<TargetProfile>(byteData);

									if(profile != null) {

										this.TargetData = profile;

									}

								} catch(Exception) {



								}

							}

						}

					}

				}

			}

		}

		public void RequestTarget(long requestGridId = 0, long requestBlockId = 0){

			if(RAI_SessionCore.IsServer == false) {

				return;

			}

			if(this.RemoteControl == null || MyAPIGateway.Entities.Exist(this.RemoteControl?.SlimBlock?.CubeGrid) == false) {

				return;

			}

			this.TargetIsShielded = false;
			this.TargetPlayer = null;
			this.TargetEntity = null;
			this.TargetGrid = null;
			this.TargetBlock = null;
			this.RequestedGridId = requestGridId;
			this.RequestedBlockId = requestBlockId;
			
		}

		public void RequestTargetParallel() {

			try {

				if (this.TargetData.UseTimeout && !this.InvalidTarget) {

					var duration = MyAPIGateway.Session.GameDateTime - this.LastNewTargetCheck;

					if (duration.TotalSeconds > this.TimeUntilNewTarget) {

						Logger.MsgDebug("Target Expired. Refreshing...", DebugTypeEnum.Target);
						this.InvalidTarget = false;
						SetNewTimeout();

					}

				}

				if (this.UpdateSpecificTarget != 0) {

					Logger.MsgDebug("Target Update Requested", DebugTypeEnum.Target);
					TargetTypeEnum targetType = TargetTypeEnum.None;
					var targetEntity = TargetHelper.GetTargetFromId(this.UpdateSpecificTarget, out targetType);
					this.UpdateSpecificTarget = 0;

					if (targetEntity != null) {

						Logger.MsgDebug("Target Update Successful", DebugTypeEnum.Target);
						this.TargetEntity = targetEntity;
						this.Target = new TargetEvaluation(this.TargetEntity, targetType);

						if (targetType == TargetTypeEnum.Player) {

							this.Target.TargetPlayer = TargetHelper.MatchPlayerToEntity(targetEntity);

						}

						if (targetType == TargetTypeEnum.Block) {

							this.Target.TargetBlock = targetEntity as IMyTerminalBlock;
							this.Target.Target = this.Target.TargetBlock?.SlimBlock?.CubeGrid;

						}

						if (this.TargetData.UseTimeout)
							SetNewTimeout();

						this.InvalidTarget = false;

					} else {

						Logger.MsgDebug("Target Update Fail", DebugTypeEnum.Target);

					}

				} else if ((this.NeedsTarget == true && this.InvalidTarget == true) || this.UpdateTargetRequested) {

					Logger.MsgDebug("Attempting To Get New Target", DebugTypeEnum.Target);

					this.UpdateTargetRequested = false;
					AcquireTarget();
					this.Target = new TargetEvaluation(this.TargetEntity, this.TargetData.Target);
					this.Target.TargetPlayer = this.TargetPlayer;
					this.Target.TargetBlock = this.TargetBlock;
					this.InvalidTarget = false;

				}

				if (this.NeedsTarget == true && this.InvalidTarget == false && this.Target != null) {

					Logger.MsgDebug("Evaluating Target", DebugTypeEnum.Target);
					this.Target.Evaluate(this.RemoteControl, this.TargetData);
					//Logger.AddMsg("Target Coords: " + this.Target.TargetCoords.ToString(), true);


					if (this.Target.TargetExists == false) {

						Logger.MsgDebug("Evaluated Target Invalid", DebugTypeEnum.Target);
						this.InvalidTarget = true;

					}

				}

			} catch (Exception exc) {

				Logger.MsgDebug("Acquire Target Exception", DebugTypeEnum.Target);
				Logger.MsgDebug(exc.ToString(), DebugTypeEnum.Target);

			}

		}

		public void SetNewTimeout() {

			this.TimeUntilNewTarget = Rnd.Next(this.TargetData.MinTimeout, this.TargetData.MaxTimeout);
			this.LastNewTargetCheck = MyAPIGateway.Session.GameDateTime;

		}

		//Parallel
		public void AcquireTarget(long UpdateSpecificTarget = 0) {

			this.SearchingForTarget = true;
			this.InvalidTarget = false;

			//Players
			if(TargetData.Target == TargetTypeEnum.Player) {

				try {

					this.TargetPlayer = TargetHelper.AcquirePlayerTarget(this.RemoteControl, this.TargetData);

				} catch(Exception exc) {

					Logger.MsgDebug("Exception Getting Player Target: ", DebugTypeEnum.Target); //

				}
				

				if(this.TargetPlayer == null) {

					//Logger.AddMsg("Cannot Find Player", true);
					this.InvalidTarget = true;
					this.SearchingForTarget = false;
					return;

				}

				//Logger.AddMsg("Got Player", true);
				this.TargetEntity = this.TargetPlayer.Controller.ControlledEntity.Entity;
 
			}

			//Grids
			if(TargetData.Target == TargetTypeEnum.Grid) {

				this.TargetGrid = TargetHelper.AcquireGridTarget(this.RemoteControl, this.TargetData, this.RequestedGridId);

				if(this.TargetGrid == null) {

					this.InvalidTarget = true;
					this.SearchingForTarget = false;
					return;

				}

				this.TargetEntity = this.TargetGrid;

			}

			//Blocks
			if(TargetData.Target == TargetTypeEnum.Block) {

				this.TargetBlock = TargetHelper.AcquireBlockTarget(this.RemoteControl, this.TargetData, this.RequestedBlockId);

				if(this.TargetBlock == null) {

					this.InvalidTarget = true;
					this.SearchingForTarget = false;
					return;

				}

				this.TargetEntity = this.TargetBlock.SlimBlock.CubeGrid;

			}

			this.SearchingForTarget = false;

		}

		public Vector3D GetTargetPosition(Vector3D originalCoords = new Vector3D()){

			//None
			if(this.TargetData.Target == TargetTypeEnum.None){
				
				this.InvalidTarget = false;
				return originalCoords;
				
			}
			
			//Coords
			if(this.TargetData.Target == TargetTypeEnum.Coords){
				
				this.InvalidTarget = false;
				return this.TargetCoords;
				
			}
			
			if(this.Target.TargetExists == true) {

				return Target.TargetCoords;

			}
			

			this.InvalidTarget = true;
			return originalCoords;
			
		}
		
	}
		
}