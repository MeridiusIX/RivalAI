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
		public CollisionCheckResult EvasionResult;
		public CollisionCheckResult ForwardResult;
		public CollisionCheckResult BackwardResult;
		public CollisionCheckResult UpResult;
		public CollisionCheckResult DownResult;
		public CollisionCheckResult LeftResult;
		public CollisionCheckResult RightResult;

		//Voxel Results
		public List<IHitInfo> VoxelVelocityResults;
		public List<IHitInfo> VoxelWeaponResult;
		public List<IHitInfo> VoxelStrafeLeftResult;
		public List<IHitInfo> VoxelStrafeRightResult;
		public List<IHitInfo> VoxelStrafeUpResult;
		public List<IHitInfo> VoxelStrafeDownResult;
		public List<IHitInfo> VoxelStrafeForwardResult;
		public List<IHitInfo> VoxelStrafeBackwardResult;

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
			EvasionResult = new CollisionCheckResult(false);
			ForwardResult = new CollisionCheckResult(false);
			BackwardResult = new CollisionCheckResult(false);
			UpResult = new CollisionCheckResult(false);
			DownResult = new CollisionCheckResult(false);
			LeftResult = new CollisionCheckResult(false);
			RightResult = new CollisionCheckResult(false);

			//Voxel Results

	
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

		
		public bool RequestVelocityCheckCollisions(){

			if(RAI_SessionCore.IsServer == false || this.UseCollisionDetection == false){

				this.VelocityResult = new CollisionCheckResult(false);
				return false;
				
			}
			
			if(this.CubeGrid.Physics == null || this.CubeGrid.IsStatic == true){

				this.VelocityResult = new CollisionCheckResult(false);
				return false;
				
			}
			
			if(this.CubeGrid.Physics.LinearVelocity.Length() < 0.1f){

				this.VelocityResult = new CollisionCheckResult(false);
				return false;
				
			}

			this.RemoteControlPosition = RemoteControl.GetPosition();
			this.GridVelocity = this.CubeGrid.Physics.LinearVelocity;
			return true;
			//MyAPIGateway.Parallel.Start(CheckVelocityCollisionsThreaded, VelocityCollisionCheckFinish);
			
		}

		public void CheckDirectionalCollisionsThreaded() {

			if(!this.UseCollisionDetection || this.RemoteControl.SlimBlock.CubeGrid.Physics == null || this.RemoteControl.SlimBlock.CubeGrid.IsStatic == true) {

				SetDirectionalChecksToNoCollision();
				return;

			}

			Logger.MsgDebug("Directional Strafe Collision Checks", DebugTypeEnum.Collision);

		}

		public void CheckPotentialEvasionCollisionThreaded(Vector3D direction, double distance) {

			if (RAI_SessionCore.IsServer == false || this.UseCollisionDetection == false) {

				this.EvasionResult = new CollisionCheckResult(false);
				return;

			}

			if (this.RemoteControl.SlimBlock.CubeGrid.Physics == null || this.RemoteControl.SlimBlock.CubeGrid.IsStatic == true) {

				this.EvasionResult = new CollisionCheckResult(false);
				return;

			}

			Logger.MsgDebug("Potential Evasion Collision Check", DebugTypeEnum.Dev);

			//this.EvasionResult = TargetHelper.CheckCollisions((IMyTerminalBlock)this.RemoteControl, direction, distance, 0, this.CollisionTimeTrigger, this.UseVoxelDetection, this.UseGridDetection, this.UseSafeZoneDetection, this.UseDefenseShieldDetection, this.UsePlayerDetection);

		}

		public void CheckVelocityCollisionsThreaded(){

			if(this.UseCollisionDetection == false) {

				return;

			}

			Logger.MsgDebug("Velocity Collision Check", DebugTypeEnum.Dev);

			//Stress Test This Monday
			List<IHitInfo> hitInfo = new List<IHitInfo>();
			Vector3D from = new Vector3D(0,0,0);
			Vector3D to = new Vector3D(0, 0, 1000);
			MyAPIGateway.Physics.CastRayParallel(ref from, ref to, hitInfo, 28, (result) => { });
			MyAPIGateway.Physics.CastRayParallel(ref from, ref to, hitInfo, 28, (result) => { });
			MyAPIGateway.Physics.CastRayParallel(ref from, ref to, hitInfo, 28, (result) => { });
			MyAPIGateway.Physics.CastRayParallel(ref from, ref to, hitInfo, 28, (result) => { });
			MyAPIGateway.Physics.CastRayParallel(ref from, ref to, hitInfo, 28, (result) => { });
			MyAPIGateway.Physics.CastRayParallel(ref from, ref to, hitInfo, 28, (result) => { });
			MyAPIGateway.Physics.CastRayParallel(ref from, ref to, hitInfo, 28, (result) => { });
			MyAPIGateway.Physics.CastRayParallel(ref from, ref to, hitInfo, 28, (result) => { });
			MyAPIGateway.Physics.CastRayParallel(ref from, ref to, hitInfo, 28, (result) => { });

			var direction = Vector3D.Normalize((Vector3D)this.GridVelocity);
			//this.VelocityResult = TargetHelper.CheckCollisions((IMyTerminalBlock)this.RemoteControl, direction, this.CollisionVelocityCheckDistance, this.GridVelocity.Length(), this.CollisionTimeTrigger, this.UseVoxelDetection, this.UseGridDetection, this.UseSafeZoneDetection, this.UseDefenseShieldDetection, this.UsePlayerDetection);

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