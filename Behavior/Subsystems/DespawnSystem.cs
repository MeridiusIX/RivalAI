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
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;

namespace RivalAI.Behavior.Subsystems{
	
	public class DespawnSystem{

		public bool UsePlayerDistanceTimer;
		public int PlayerDistanceTimerTrigger;
		public double PlayerDistanceTrigger;

		public bool UseNoTargetTimer;
		public int NoTargetTimerTrigger;

		public bool UseRetreatTimer;
		public int RetreatTimerTrigger;
		public double RetreatDespawnDistance;

		public IMyRemoteControl RemoteControl;

		public int PlayerDistanceTimer; //Storage
		public double PlayerDistance;
		public IMyPlayer NearestPlayer;

		public int RetreatTimer; //Storage

		public bool SuspendNoTargetTimer = false;
		public int NoTargetTimer;

		public bool DoDespawn;
		public bool DoRetreat;
		public bool NoTargetExpire;
		
		public event Action RetreatTriggered;

		public DespawnSystem(IMyRemoteControl remoteControl = null) {

			UsePlayerDistanceTimer = true;
			PlayerDistanceTimerTrigger = 150;
			PlayerDistanceTrigger = 25000;

			UseNoTargetTimer = false;
			NoTargetTimerTrigger = 10;

			UseRetreatTimer = false;
			RetreatTimerTrigger = 600;
			RetreatDespawnDistance = 3000;

			RemoteControl = null;

			PlayerDistanceTimer = 0;
			PlayerDistance = 0;

			RetreatTimer = 0;

			NoTargetTimer = 0;

			DoDespawn = false;
			DoRetreat = false;
			NoTargetExpire = false;

			Setup(remoteControl);


		}
		
		private void Setup(IMyRemoteControl remoteControl){

			if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false) {

				return;

			}

			this.RemoteControl = remoteControl;

		}

		public void InitTags() {

			if (string.IsNullOrWhiteSpace(RemoteControl.CustomData) == false) {

				var descSplit = RemoteControl.CustomData.Split('\n');

				foreach (var tag in descSplit) {

					//UsePlayerDistanceTimer
					if (tag.Contains("[UsePlayerDistanceTimer:") == true) {

						this.UsePlayerDistanceTimer = TagHelper.TagBoolCheck(tag);

					}

					//PlayerDistanceTimerTrigger
					if (tag.Contains("[PlayerDistanceTimerTrigger:") == true) {

						this.PlayerDistanceTimerTrigger = TagHelper.TagIntCheck(tag, this.PlayerDistanceTimerTrigger);

					}

					//PlayerDistanceTrigger
					if (tag.Contains("[PlayerDistanceTrigger:") == true) {

						this.PlayerDistanceTrigger = TagHelper.TagDoubleCheck(tag, this.PlayerDistanceTrigger);

					}

					//UseNoTargetTimer
					if (tag.Contains("[UseNoTargetTimer:") == true) {

						this.UseNoTargetTimer = TagHelper.TagBoolCheck(tag);

					}

					//NoTargetTimerTrigger
					if (tag.Contains("[NoTargetTimerTrigger:") == true) {

						this.NoTargetTimerTrigger = TagHelper.TagIntCheck(tag, this.NoTargetTimerTrigger);

					}

					//UseRetreatTimer
					if (tag.Contains("[UseRetreatTimer:") == true) {

						this.UseRetreatTimer = TagHelper.TagBoolCheck(tag);

					}

					//RetreatTimerTrigger
					if (tag.Contains("[RetreatTimerTrigger:") == true) {

						this.RetreatTimerTrigger = TagHelper.TagIntCheck(tag, this.RetreatTimerTrigger);

					}

					//RetreatDespawnDistance
					if (tag.Contains("[RetreatDespawnDistance:") == true) {

						this.RetreatDespawnDistance = TagHelper.TagDoubleCheck(tag, this.RetreatDespawnDistance);

					}

				}

			}

		}

		public void ProcessTimers(BehaviorMode mode, bool invalidTarget = false){
			
			if(this.RemoteControl == null){
				
				return;
				
			}
			
			this.NearestPlayer = TargetHelper.GetClosestPlayer(this.RemoteControl.GetPosition());

			if(mode == BehaviorMode.Retreat) {

				if(this.NearestPlayer?.Controller?.ControlledEntity?.Entity != null) {

					if(Vector3D.Distance(this.RemoteControl.GetPosition(), this.NearestPlayer.GetPosition()) > this.RetreatDespawnDistance){

						Logger.AddMsg("Retreat Despawn: Player Far Enough", true);
						DoDespawn = true;
						
					}

				} else {

					Logger.AddMsg("Retreat Despawn: No Player", true);
					DoDespawn = true;

				}

			}
			
			if(this.UsePlayerDistanceTimer == true){
				
				if(this.NearestPlayer == null){
					
					PlayerDistanceTimer++;
					
				}else if(this.NearestPlayer?.Controller?.ControlledEntity?.Entity != null){

					if(Vector3D.Distance(this.NearestPlayer.GetPosition(), this.RemoteControl.GetPosition()) > this.PlayerDistanceTrigger) {
						
						PlayerDistanceTimer++;

						if(PlayerDistanceTimer >= PlayerDistanceTimerTrigger) {

							Logger.AddMsg("Despawn: No Player Within Distance", true);
							DoDespawn = true;

						}
						
					}else{
						
						PlayerDistanceTimer = 0;
						
					}
					
				}
				
			}

			if(this.UseNoTargetTimer == true && this.SuspendNoTargetTimer == false) {

				if(invalidTarget == true) {

					this.NoTargetTimer++;

					if(this.NoTargetTimer >= this.NoTargetTimerTrigger) {

						this.NoTargetExpire = true;

					}

				} else {

					this.NoTargetTimer = 0;

				}

			}
			
			if(this.UseRetreatTimer == true && this.DoRetreat == false){
				
				RetreatTimer++;

				if(RetreatTimer >= RetreatTimerTrigger) {

					DoRetreat = true;

				}
				
			}
		
		}
		
		public void Retreat(){

			Logger.AddMsg("Retreat Signal Received For Grid: " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, true);
			DoRetreat = true;
			
		}

		public void DespawnGrid() {

			Logger.AddMsg("Despawning Grid: " + this.RemoteControl.SlimBlock.CubeGrid.CustomName);

			MyAPIGateway.Utilities.InvokeOnGameThread(() => {

				var gridGroup = MyAPIGateway.GridGroups.GetGroup(this.RemoteControl.SlimBlock.CubeGrid, GridLinkTypeEnum.Logical);

				foreach(var grid in gridGroup) {

					if(grid.MarkedForClose == false) {

						grid.Close();

					}

				}

			});
			

		}
		
	}
	
}