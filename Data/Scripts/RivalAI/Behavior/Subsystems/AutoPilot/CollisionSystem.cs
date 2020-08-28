using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using Sandbox.ModAPI;
using System.Text;
using VRageMath;

namespace RivalAI.Behavior.Subsystems.AutoPilot {
	public class CollisionSystem {

		public bool UseCollisionDetection;

		public double MinimumSpeedForVelocityChecks = 1;
		public bool CollisionAsteroidUsesBoundingBoxForVelocity = false;
		public int CollisionTimeTrigger = 5;
		public bool CollisionUseVelocityCheckCooldown = false;
		public int CollisionVelocityCheckCooldownTime = 6;

		public double DistanceForForwardDirection = 2000;
		public double DistanceForVelocityDirection = 1000;
		public double DistanceForOtherDirections = 500;

		public CollisionResult VelocityResult;
		public CollisionResult TargetResult;
		public CollisionResult ForwardResult;
		public CollisionResult BackwardResult;
		public CollisionResult UpResult;
		public CollisionResult DownResult;
		public CollisionResult LeftResult;
		public CollisionResult RightResult;

		public IMyRemoteControl RemoteControl;
		public MatrixD Matrix;
		public Vector3D Velocity;
		public long Owner;

		public AutoPilotSystem AutoPilot;

		public CollisionSystem(IMyRemoteControl remoteControl, AutoPilotSystem autoPilot) {

			if (remoteControl == null || !MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid))
				return;

			UseCollisionDetection = true;

			RemoteControl = remoteControl;
			Matrix = MatrixD.Identity;
			Velocity = Vector3D.Zero;
			Owner = 0;

			AutoPilot = autoPilot;

			VelocityResult = new CollisionResult(this, Direction.None);
			TargetResult = new CollisionResult(this, Direction.None);
			ForwardResult = new CollisionResult(this, Direction.Forward);
			BackwardResult = new CollisionResult(this, Direction.Backward);
			UpResult = new CollisionResult(this, Direction.Up);
			DownResult = new CollisionResult(this, Direction.Down);
			LeftResult = new CollisionResult(this, Direction.Left);
			RightResult = new CollisionResult(this, Direction.Right);

		}

		public void PrepareCollisionChecks() {

			if (!UseCollisionDetection)
				return;

			//Logger.MsgDebug("Start Collision Prechecks: ", DebugTypeEnum.Collision);
			Matrix = RemoteControl.WorldMatrix;
			Velocity = RemoteControl?.SlimBlock?.CubeGrid?.Physics != null ? (Vector3D)RemoteControl.SlimBlock.CubeGrid.Physics.LinearVelocity : Vector3D.Zero;
			Owner = RemoteControl.OwnerId;

			if (Velocity.Length() > 0.2) {

				VelocityResult.CalculateCollisions(Vector3D.Normalize(Velocity), DistanceForVelocityDirection, true, CollisionAsteroidUsesBoundingBoxForVelocity);

			} else {

				VelocityResult.ResetResults();

			}

			if (AutoPilot.Targeting.HasTarget() && AutoPilot.Targeting.Data.MaxLineOfSight > 0) {

				var dirToTarget = Vector3D.Normalize(AutoPilot.Targeting.TargetLastKnownCoords - RemoteControl.GetPosition());
				TargetResult.CalculateCollisions(dirToTarget, AutoPilot.Targeting.Data.MaxLineOfSight);
				TargetResult.DetermineClosestCollision();

			}

			ForwardResult.CalculateCollisions(Matrix.Forward, DistanceForForwardDirection);
			VelocityResult.DetermineClosestCollision();
			ForwardResult.DetermineClosestCollision();

		}

		public void RunSecondaryCollisionChecks(bool onlyUseWithinTargetDirection = false, Vector3D targetDirection = new Vector3D()) {

			if (!UseCollisionDetection)
				return;

			BackwardResult.CalculateCollisions(Matrix.Backward, DistanceForOtherDirections);
			UpResult.CalculateCollisions(Matrix.Up, DistanceForOtherDirections);
			DownResult.CalculateCollisions(Matrix.Down, DistanceForOtherDirections);
			LeftResult.CalculateCollisions(Matrix.Left, DistanceForOtherDirections);
			RightResult.CalculateCollisions(Matrix.Right, DistanceForOtherDirections);

			BackwardResult.DetermineClosestCollision();
			UpResult.DetermineClosestCollision();
			DownResult.DetermineClosestCollision();
			LeftResult.DetermineClosestCollision();
			RightResult.DetermineClosestCollision();

			/*
			var sb = new StringBuilder();
			sb.Append("6-Direction Collision Checks").AppendLine();
			sb.Append(" - ").Append("Forward: " + ForwardResult.Type.ToString() + ", " + ForwardResult.GetCollisionDistance().ToString()).AppendLine();
			sb.Append(" - ").Append("Backward: " + BackwardResult.Type.ToString() + ", " + BackwardResult.GetCollisionDistance().ToString()).AppendLine();
			sb.Append(" - ").Append("Up: " + UpResult.Type.ToString() + ", " + UpResult.GetCollisionDistance().ToString()).AppendLine();
			sb.Append(" - ").Append("Down: " + DownResult.Type.ToString() + ", " + DownResult.GetCollisionDistance().ToString()).AppendLine();
			sb.Append(" - ").Append("Left: " + LeftResult.Type.ToString() + ", " + LeftResult.GetCollisionDistance().ToString()).AppendLine();
			sb.Append(" - ").Append("Right: " + RightResult.Type.ToString() + ", " + RightResult.GetCollisionDistance().ToString()).AppendLine();
			Logger.MsgDebug(sb.ToString(), DebugTypeEnum.Collision);
			*/
		}



		public CollisionResult GetResult(Direction direction) {

			if (direction == Direction.Forward)
				return ForwardResult;

			if (direction == Direction.Backward)
				return BackwardResult;

			if (direction == Direction.Up)
				return UpResult;

			if (direction == Direction.Down)
				return DownResult;

			if (direction == Direction.Left)
				return LeftResult;

			if (direction == Direction.Right)
				return RightResult;

			return null;

		}

	}

	public enum CollisionType {

		None,
		Grid,
		Voxel,
		Safezone,
		Shield,
		Player

	}

}
