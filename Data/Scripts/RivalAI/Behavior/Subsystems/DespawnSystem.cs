using RivalAI.Helpers;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;
using VRageMath;

namespace RivalAI.Behavior.Subsystems {

	public class DespawnSystem{

		private IBehavior _behavior;

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

		public bool NoTargetExpire;
		
		public event Action RetreatTriggered;

		private bool _mesDespawnTriggerCheck;

		public DespawnSystem(IBehavior behavior, IMyRemoteControl remoteControl = null) {

			UsePlayerDistanceTimer = true;
			PlayerDistanceTimerTrigger = 150;
			PlayerDistanceTrigger = 25000;

			UseNoTargetTimer = false;
			NoTargetTimerTrigger = 60;

			UseRetreatTimer = false;
			RetreatTimerTrigger = 600;
			RetreatDespawnDistance = 3000;

			RemoteControl = null;

			PlayerDistanceTimer = 0;
			PlayerDistance = 0;

			RetreatTimer = 0;

			NoTargetTimer = 0;

			NoTargetExpire = false;

			Setup(remoteControl);
			_behavior = behavior;


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

						Logger.MsgDebug("Retreat Despawn: Player Far Enough", DebugTypeEnum.Despawn);
						_behavior.Settings.DoDespawn = true;
						
					}

				} else {

					Logger.MsgDebug("Retreat Despawn: No Player", DebugTypeEnum.Despawn);
					_behavior.Settings.DoDespawn = true;

				}

			}
			
			if(this.UsePlayerDistanceTimer == true){
				
				if(this.NearestPlayer == null){
					
					PlayerDistanceTimer++;
					
				}else if(this.NearestPlayer?.Controller?.ControlledEntity?.Entity != null){

					if(Vector3D.Distance(this.NearestPlayer.GetPosition(), this.RemoteControl.GetPosition()) > this.PlayerDistanceTrigger) {
						
						PlayerDistanceTimer++;

						if(PlayerDistanceTimer >= PlayerDistanceTimerTrigger) {

							Logger.MsgDebug("No Player Within Distance", DebugTypeEnum.Despawn);
							_behavior.Settings.DoDespawn = true;

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
			
			if(this.UseRetreatTimer == true && _behavior.Settings.DoRetreat == false){
				
				RetreatTimer++;

				if(RetreatTimer >= RetreatTimerTrigger) {

					_behavior.Settings.DoRetreat = true;

				}
				
			}

			if (_behavior.Settings.DoDespawn) {
				_behavior.Trigger.ProcessDespawnTriggers();
				DespawnGrid();
			
			}
		
		}
		
		public void Retreat(){

			_behavior.Trigger.ProcessRetreatTriggers();
			Logger.MsgDebug("Retreat Signal Received For Grid: " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.Despawn);
			_behavior.Settings.DoRetreat = true;
			
		}

		public void DespawnGrid() {

			Logger.MsgDebug("Despawning Grid: " + this.RemoteControl.SlimBlock.CubeGrid.CustomName, DebugTypeEnum.Despawn);

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