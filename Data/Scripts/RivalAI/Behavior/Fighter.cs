using RivalAI.Behavior.Subsystems.AutoPilot;
using RivalAI.Helpers;
using Sandbox.ModAPI;

namespace RivalAI.Behavior {

	public class Fighter : CoreBehavior, IBehavior{

		//Configurable
		public double FighterEngageDistanceSpace {

			get {

				return _fighterEngageDistanceSpace > 0 ? _fighterEngageDistanceSpace : AutoPilot.Data.EngageDistanceSpace;

			}

			set {

				_fighterEngageDistanceSpace = value;

			}
		
		}

		public double FighterEngageDistancePlanet {

			get {

				return _fighterEngageDistancePlanet > 0 ? _fighterEngageDistancePlanet : AutoPilot.Data.EngageDistancePlanet;

			}

			set {

				_fighterEngageDistancePlanet = value;

			}

		}

		public double FighterDisengageDistanceSpace {

			get {

				return _fighterDisengageDistanceSpace > 0 ? _fighterDisengageDistanceSpace : AutoPilot.Data.DisengageDistanceSpace;

			}

			set {

				_fighterDisengageDistanceSpace = value;

			}

		}

		public double FighterDisengageDistancePlanet {

			get {

				return _fighterDisengageDistancePlanet > 0 ? _fighterDisengageDistancePlanet : AutoPilot.Data.DisengageDistancePlanet;

			}

			set {

				_fighterDisengageDistancePlanet = value;

			}

		}

		private double _fighterEngageDistanceSpace;
		private double _fighterEngageDistancePlanet;

		private double _fighterDisengageDistanceSpace;
		private double _fighterDisengageDistancePlanet;

		public byte Counter;

		public Fighter() : base() {

			_behaviorType = "Fighter";

			_fighterEngageDistanceSpace = -1;
			_fighterEngageDistancePlanet = -1;

			_fighterDisengageDistanceSpace = -1;
			_fighterDisengageDistancePlanet = -1;
			
			Counter = 0;

		}

		public override void MainBehavior() {

			if(RAI_SessionCore.IsServer == false) {

				return;

			}

			base.MainBehavior();

			//Logger.MsgDebug(Mode.ToString(), DebugTypeEnum.General);
			
			if(Mode != BehaviorMode.Retreat && Settings.DoRetreat == true){

				ChangeCoreBehaviorMode(BehaviorMode.Retreat);
				AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing, CheckEnum.Yes, CheckEnum.No);

			}
			
			if(Mode == BehaviorMode.Init) {

				if(!AutoPilot.Targeting.HasTarget()) {

					ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);

				} else {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget, CheckEnum.Yes, CheckEnum.No);

				}

			}

			if(Mode == BehaviorMode.WaitingForTarget) {

				if(AutoPilot.CurrentMode != AutoPilot.UserCustomModeIdle) {

					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.None, CheckEnum.No, CheckEnum.Yes);

				}

				if(AutoPilot.Targeting.HasTarget()) {

					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget, CheckEnum.Yes, CheckEnum.No);

				} else if(Despawn.NoTargetExpire == true){
					
					Despawn.Retreat();
					
				}

			}

			if(!AutoPilot.Targeting.HasTarget() && Mode != BehaviorMode.Retreat && Mode != BehaviorMode.WaitingForTarget) {


				ChangeCoreBehaviorMode(BehaviorMode.WaitingForTarget);
				AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.None, CheckEnum.No, CheckEnum.Yes);

			}

			//Approach
			if (Mode == BehaviorMode.ApproachTarget) {

				bool inRange = false;

				if (!AutoPilot.InGravity() && AutoPilot.DistanceToTargetWaypoint < this.FighterEngageDistanceSpace)
					inRange = true;

				if(AutoPilot.InGravity() && AutoPilot.DistanceToTargetWaypoint < this.FighterEngageDistancePlanet)
					inRange = true;

				if (inRange) {

					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.Strafe | NewAutoPilotMode.WaypointFromTarget, CheckEnum.Yes, CheckEnum.No);
					ChangeCoreBehaviorMode(BehaviorMode.EngageTarget);
					BehaviorTriggerA = true;

				}

			}

			//Engage
			if (Mode == BehaviorMode.EngageTarget) {

				bool outRange = false;

				if (!AutoPilot.InGravity() && AutoPilot.DistanceToTargetWaypoint > this.FighterDisengageDistanceSpace)
					outRange = true;

				if (AutoPilot.InGravity() && AutoPilot.DistanceToTargetWaypoint > this.FighterDisengageDistancePlanet)
					outRange = true;

				if (outRange) {

					AutoPilot.ActivateAutoPilot(this.RemoteControl.GetPosition(), NewAutoPilotMode.RotateToWaypoint | NewAutoPilotMode.ThrustForward | NewAutoPilotMode.PlanetaryPathing | NewAutoPilotMode.WaypointFromTarget, CheckEnum.Yes, CheckEnum.No);
					ChangeCoreBehaviorMode(BehaviorMode.ApproachTarget);
					BehaviorTriggerB = true;

				}

			}

			//Retreat
			if (Mode == BehaviorMode.Retreat) {

				if (Despawn.NearestPlayer?.Controller?.ControlledEntity?.Entity != null) {

					//Logger.AddMsg("DespawnCoordsCreated", true);
					AutoPilot.SetInitialWaypoint(VectorHelper.GetDirectionAwayFromTarget(this.RemoteControl.GetPosition(), Despawn.NearestPlayer.GetPosition()) * 1000 + this.RemoteControl.GetPosition());

				}

			}


		}

		public override void BehaviorInit(IMyRemoteControl remoteControl) {

			Logger.MsgDebug("Beginning Behavior Init For Fighter", DebugTypeEnum.General);

			//Core Setup
			CoreSetup(remoteControl);

			//Behavior Specific Defaults
			AutoPilot.Data = TagHelper.GetAutopilotProfile("RAI-Generic-Autopilot-Fighter");
			Despawn.UseNoTargetTimer = true;
			AutoPilot.Weapons.UseStaticGuns = true;

			//Get Settings From Custom Data
			InitCoreTags();
			InitTags();
			SetDefaultTargeting();

			SetupCompleted = true;

		}

		public void InitTags() {

			if(string.IsNullOrWhiteSpace(this.RemoteControl?.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach(var tag in descSplit) {
					
					//FighterEngageDistanceSpace
					if(tag.Contains("[FighterEngageDistanceSpace:") == true) {

						this.FighterEngageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.FighterEngageDistanceSpace);

					}	
			
					//FighterEngageDistancePlanet
					if(tag.Contains("[FighterEngageDistancePlanet:") == true) {

						this.FighterEngageDistancePlanet = TagHelper.TagDoubleCheck(tag, this.FighterEngageDistancePlanet);

					}

					//FighterDisengageDistanceSpace
					if (tag.Contains("[FighterDisengageDistanceSpace:") == true) {

						this.FighterDisengageDistanceSpace = TagHelper.TagDoubleCheck(tag, this.FighterDisengageDistanceSpace);

					}

					//FighterDisengageDistancePlanet
					if (tag.Contains("[FighterDisengageDistancePlanet:") == true) {

						this.FighterDisengageDistancePlanet = TagHelper.TagDoubleCheck(tag, this.FighterDisengageDistancePlanet);

					}

				}
				
			}

		}

	}

}
	
