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
	
	
	
	public class CollisionSystem{


		//Configurable
		public bool UseCollisionDetection;
		public bool UseVoxelDetection;
		public bool UseGridDetection;
		public bool UseSafeZoneDetection;
		public bool UseDefenseShieldDetection;
		public bool UsePlayerDetection;
		public double CollisionVelocityCheckDistance;
		public double CollisionDirectionsCheckDistance;

		public double CollisionTimeTrigger;
		public bool CreateCollisionEvasionWaypoint;
		public double CollisionEvasionDistance;

		//General
		public IMyRemoteControl RemoteControl;
		public Vector3D RemoteControlPosition;
		public MatrixD RemoteMaxtrix;
		public Vector3 GridVelocity;
		public IMyCubeGrid CubeGrid;

		//Reference
		private ThrustSystem Thrust;

		//Results
		public CollisionCheckResult VelocityResult;
		public CollisionCheckResult ForwardResult;
		public CollisionCheckResult BackwardResult;
		public CollisionCheckResult UpResult;
		public CollisionCheckResult DownResult;
		public CollisionCheckResult LeftResult;
		public CollisionCheckResult RightResult;

		/*
		public bool CollisionDetected = false;
		public Vector3D ClosestCollision;
		public double ClosestDistance;
		public double TimeToCollision;
		public CollisionDetectType CollisionDetectType;
		public IMyEntity ClosestEntity;
		*/
		public Vector3D SpaceEvadeCoords;
		public Vector3D PlanetEvadeCoords;
		
		public event Action<Vector3D> TriggerWarning;
		public event Action AutopilotUpdate;

		//Planet
		public MyPlanet Planet;
		public bool InGravity;


		public CollisionSystem(IMyRemoteControl remoteControl = null){
			
			//General
			
			RemoteControl = null;
			RemoteControlPosition = Vector3D.Zero;
			RemoteMaxtrix = MatrixD.Identity;
			GridVelocity = Vector3.Zero;
			CubeGrid = null;

			//Detect Settings
			UseCollisionDetection = true;
			UseVoxelDetection = true;
			UseGridDetection = true;
			UseSafeZoneDetection = true;
			UseDefenseShieldDetection = true;
			UsePlayerDetection = false;
			CollisionVelocityCheckDistance = 600;
			CollisionDirectionsCheckDistance = 150;
			CollisionTimeTrigger = 5;

			//Results
			VelocityResult = new CollisionCheckResult(false);
			ForwardResult = new CollisionCheckResult(false);
			BackwardResult = new CollisionCheckResult(false);
			UpResult = new CollisionCheckResult(false);
			DownResult = new CollisionCheckResult(false);
			LeftResult = new CollisionCheckResult(false);
			RightResult = new CollisionCheckResult(false);
			/*
			CollisionDetected = false;
			ClosestCollision = Vector3D.Zero;
			ClosestDistance = 0;
			TimeToCollision = 0;
			CollisionDetectType = CollisionDetectType.None;
			ClosestEntity = null;
			SpaceEvadeCoords = Vector3D.Zero;
			PlanetEvadeCoords = Vector3D.Zero;
			*/
			//Planet
			Planet = null;
			InGravity = false;

			Setup(remoteControl);

		}
		
		private void Setup(IMyRemoteControl remoteControl){
			
			if(remoteControl == null){
				
				return;
				
			}
			
			this.RemoteControl = remoteControl;
			this.CubeGrid = remoteControl.SlimBlock.CubeGrid;

		}

		public void SetupReferences(ThrustSystem thrust) {

			this.Thrust = thrust;

		}

		public void InitTags() {

			if(string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == false) {

				var descSplit = this.RemoteControl.CustomData.Split('\n');

				foreach(var tag in descSplit) {

					//UseCollisionDetection
					if(tag.Contains("[UseCollisionDetection:") == true) {

						this.UseCollisionDetection = TagHelper.TagBoolCheck(tag);

					}

					//UseVoxelDetection
					if(tag.Contains("[UseVoxelDetection:") == true) {

						this.UseVoxelDetection = TagHelper.TagBoolCheck(tag);

					}

					//UseGridDetection
					if(tag.Contains("[UseGridDetection:") == true) {

						this.UseGridDetection = TagHelper.TagBoolCheck(tag);

					}

					//UseSafeZoneDetection
					if(tag.Contains("[UseSafeZoneDetection:") == true) {

						this.UseSafeZoneDetection = TagHelper.TagBoolCheck(tag);

					}

					//UseDefenseShieldDetection
					if(tag.Contains("[UseDefenseShieldDetection:") == true) {

						this.UseDefenseShieldDetection = TagHelper.TagBoolCheck(tag);

					}

					//UsePlayerDetection
					if(tag.Contains("[UsePlayerDetection:") == true) {

						this.UsePlayerDetection = TagHelper.TagBoolCheck(tag);

					}

					//CollisionCheckDistance
					if(tag.Contains("[CollisionCheckDistance:") == true) {

						this.CollisionVelocityCheckDistance = TagHelper.TagDoubleCheck(tag, this.CollisionVelocityCheckDistance);

					}

				}

			}

		}


		public void RequestVelocityCheckCollisions(){

			if(RAI_SessionCore.IsServer == false || this.UseCollisionDetection == false){

				this.VelocityResult = new CollisionCheckResult(false);
				VelocityCollisionCheckFinish();
				return;
				
			}
			
			if(this.CubeGrid.Physics == null || this.CubeGrid.IsStatic == true){

				this.VelocityResult = new CollisionCheckResult(false);
				VelocityCollisionCheckFinish();
				return;
				
			}
			
			if(this.CubeGrid.Physics.LinearVelocity.Length() < 0.1f){

				this.VelocityResult = new CollisionCheckResult(false);
				VelocityCollisionCheckFinish();
				return;
				
			}

			this.RemoteControlPosition = RemoteControl.GetPosition();
			this.GridVelocity = this.CubeGrid.Physics.LinearVelocity;
			MyAPIGateway.Parallel.Start(CheckVelocityCollisionsThreaded, VelocityCollisionCheckFinish);
			
		}

		public void CheckDirectionalCollisionsThreaded() {

			if(RAI_SessionCore.IsServer == false || this.UseCollisionDetection == false) {

				SetDirectionalChecksToNoCollision();
				return;

			}

			if(this.RemoteControl.SlimBlock.CubeGrid.Physics == null || this.RemoteControl.SlimBlock.CubeGrid.IsStatic == true) {

				SetDirectionalChecksToNoCollision();
				return;

			}

			this.ForwardResult = TargetHelper.CheckCollisions((IMyTerminalBlock)this.RemoteControl, this.RemoteMaxtrix.Forward, this.CollisionDirectionsCheckDistance, 0, this.CollisionTimeTrigger, this.UseVoxelDetection, this.UseGridDetection, this.UseSafeZoneDetection, this.UseDefenseShieldDetection, this.UsePlayerDetection);
			this.BackwardResult = TargetHelper.CheckCollisions((IMyTerminalBlock)this.RemoteControl, this.RemoteMaxtrix.Backward, this.CollisionDirectionsCheckDistance, 0, this.CollisionTimeTrigger, this.UseVoxelDetection, this.UseGridDetection, this.UseSafeZoneDetection, this.UseDefenseShieldDetection, this.UsePlayerDetection);
			this.UpResult = TargetHelper.CheckCollisions((IMyTerminalBlock)this.RemoteControl, this.RemoteMaxtrix.Up, this.CollisionDirectionsCheckDistance, 0, this.CollisionTimeTrigger, this.UseVoxelDetection, this.UseGridDetection, this.UseSafeZoneDetection, this.UseDefenseShieldDetection, this.UsePlayerDetection);
			this.DownResult = TargetHelper.CheckCollisions((IMyTerminalBlock)this.RemoteControl, this.RemoteMaxtrix.Down, this.CollisionDirectionsCheckDistance, 0, this.CollisionTimeTrigger, this.UseVoxelDetection, this.UseGridDetection, this.UseSafeZoneDetection, this.UseDefenseShieldDetection, this.UsePlayerDetection);
			this.LeftResult = TargetHelper.CheckCollisions((IMyTerminalBlock)this.RemoteControl, this.RemoteMaxtrix.Left, this.CollisionDirectionsCheckDistance, 0, this.CollisionTimeTrigger, this.UseVoxelDetection, this.UseGridDetection, this.UseSafeZoneDetection, this.UseDefenseShieldDetection, this.UsePlayerDetection);
			this.RightResult = TargetHelper.CheckCollisions((IMyTerminalBlock)this.RemoteControl, this.RemoteMaxtrix.Right, this.CollisionDirectionsCheckDistance, 0, this.CollisionTimeTrigger, this.UseVoxelDetection, this.UseGridDetection, this.UseSafeZoneDetection, this.UseDefenseShieldDetection, this.UsePlayerDetection);

		}

		private void CheckVelocityCollisionsThreaded(){

			if(this.UseCollisionDetection == false) {

				return;

			}

			var direction = Vector3D.Normalize((Vector3D)this.GridVelocity);
			this.VelocityResult = TargetHelper.CheckCollisions((IMyTerminalBlock)this.RemoteControl, direction, this.CollisionVelocityCheckDistance, this.GridVelocity.Length(), this.CollisionTimeTrigger, this.UseVoxelDetection, this.UseGridDetection, this.UseSafeZoneDetection, this.UseDefenseShieldDetection, this.UsePlayerDetection);

			if(this.VelocityResult.Type != CollisionDetectType.None) {

				this.SpaceEvadeCoords = SpaceEvadeCoordCalulate(this.VelocityResult.Coords);
				this.PlanetEvadeCoords = PlanetEvadeCoordCalulate(this.VelocityResult.Coords);

			}

		}

		private Vector3D SpaceEvadeCoordCalulate(Vector3D coords) {

			var dirBackwards = Vector3D.Normalize(this.RemoteControl.GetPosition() - coords);
			var randomPerpDir = MyUtils.GetRandomPerpendicularVector(ref dirBackwards);
			return (dirBackwards * 150 + coords) + (randomPerpDir * 150 + coords);

		}

		private Vector3D PlanetEvadeCoordCalulate(Vector3D coords) {

			var dirBackwards = Vector3D.Normalize(this.RemoteControl.GetPosition() - coords);
			var dirUp = VectorHelper.GetPlanetUpDirection(this.RemoteControl.GetPosition());
			return Vector3D.Normalize(dirBackwards + dirUp) * 200 + coords;

		}

		private void VelocityCollisionCheckFinish() {

			MyAPIGateway.Utilities.InvokeOnGameThread(() => {

				//Logger.AddMsg("Time To Collision: " + this.CollisionDetected.ToString() + "/" + this.TimeToCollision.ToString(), true);

				if(this.VelocityResult.HasTarget == true && this.VelocityResult.CollisionImminent == true) {

					this.TriggerWarning?.Invoke(this.VelocityResult.Coords);
					//Logger.AddMsg("Collision Event Invoked", true);

				}



			});

		}

		private void SetDirectionalChecksToNoCollision() {

			this.ForwardResult = new CollisionCheckResult(false);
			this.BackwardResult = new CollisionCheckResult(false);
			this.UpResult = new CollisionCheckResult(false);
			this.DownResult = new CollisionCheckResult(false);
			this.LeftResult = new CollisionCheckResult(false);
			this.RightResult = new CollisionCheckResult(false);

		}


	}

}