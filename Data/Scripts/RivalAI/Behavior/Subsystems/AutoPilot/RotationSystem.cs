using RivalAI.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRageMath;

namespace RivalAI.Behavior.Subsystems.AutoPilot {
	public partial class AutoPilotSystem {

		public List<GyroscopeProfile> GyroProfiles;
		public GyroscopeProfile ActiveGyro;

		public MatrixD RefBlockMatrixRotation;

		public Vector3 RotationToApply;

		public double YawAngleDifference;
		public double PitchAngleDifference;
		public double RollAngleDifference;

		public double YawTargetAngleResult;
		public double PitchTargetAngleResult;
		public double RollTargetAngleResult;

		public bool BarrelRollEnabled;

		public void ApplyGyroRotation() {

			if (ActiveGyro == null || !ActiveGyro.Active)
				return;

			ActiveGyro.ApplyRotation();

		}

		public double CalculateGyroAxisRadians(double angleA, double angleB, double totalAngle, bool isSmallGrid, ref double angleDifference) {

			angleDifference = 0;
			double angleDirection = 1;

			if (angleA > angleB) {

				//Positive
				angleDifference = angleA - angleB;

			} else {

				//Negative
				angleDirection = -1;
				angleDifference = angleB - angleA;

			}

			if (angleDifference <= this.Data.DesiredAngleToTarget) {

				return 0;

			}

			if (totalAngle + angleDifference >= 90) {

				return isSmallGrid ? Math.PI * 2 * angleDirection : Math.PI * angleDirection;

			}

			return (angleDifference * (Math.PI / 180)) * angleDirection;

		}

		public void CalculateGyroRotation() {

			this.RotationToApply = Vector3.Zero;

			var rotationTarget = _currentWaypoint;

			if (Targeting.HasTarget()) {

				if (CurrentMode.HasFlag(NewAutoPilotMode.RotateToTarget) || CurrentMode.HasFlag(NewAutoPilotMode.Ram))
					rotationTarget = Targeting.TargetLastKnownCoords;

			}

			MatrixD referenceMatrix = this.RefBlockMatrixRotation; //This should be either the control block or at least represent what direction the ship should face
			Vector3D directionToTarget = Vector3D.Normalize(rotationTarget - referenceMatrix.Translation);
			Vector3 gyroRotation = new Vector3(0, 0, 0); // Pitch,Yaw,Roll

			//Get Actual Angle To Target
			double angleToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Forward, directionToTarget);
			this.AngleToCurrentWaypoint = angleToTarget;
			this.AngleToUpDirection = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Up, _upDirection);
			//MyVisualScriptLogicProvider.ShowNotificationToAll("Total Angle: " + angleToTarget.ToString(), 166);


			if (angleToTarget <= this.Data.DesiredAngleToTarget && _upDirection == Vector3D.Zero) {

				this.RotationToApply = Vector3.Zero;
				return;

			}

			var gridSize = (_remoteControl.CubeGrid.GridSizeEnum == MyCubeSize.Small);

			//Calculate Yaw

			if (CurrentMode.HasFlag(NewAutoPilotMode.RotateToWaypoint)) {

				double angleLeftToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Left, directionToTarget);
				double angleRightToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Right, directionToTarget);
				YawTargetAngleResult = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Forward, directionToTarget) - VectorHelper.GetAngleBetweenDirections(referenceMatrix.Backward, directionToTarget);
				gyroRotation.Y = (float)CalculateGyroAxisRadians(angleLeftToTarget, angleRightToTarget, YawTargetAngleResult, gridSize, ref YawAngleDifference);

			}

			//Calculate Pitch
			if (_upDirection != Vector3D.Zero && CurrentMode.HasFlag(NewAutoPilotMode.LevelWithGravity) && !CurrentMode.HasFlag(NewAutoPilotMode.Ram)) {

				double angleForwardToUp = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Forward, _upDirection);
				double angleBackwardToUp = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Backward, _upDirection);
				PitchTargetAngleResult = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Up, _upDirection) - VectorHelper.GetAngleBetweenDirections(referenceMatrix.Down, _upDirection);
				gyroRotation.X = (float)CalculateGyroAxisRadians(angleForwardToUp, angleBackwardToUp, PitchTargetAngleResult, gridSize, ref PitchAngleDifference);

			} else {

				double angleUpToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Up, directionToTarget);
				double angleDownToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Down, directionToTarget);
				PitchTargetAngleResult = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Forward, directionToTarget) - VectorHelper.GetAngleBetweenDirections(referenceMatrix.Backward, directionToTarget);
				gyroRotation.X = (float)CalculateGyroAxisRadians(angleDownToTarget, angleUpToTarget, PitchTargetAngleResult, gridSize, ref PitchAngleDifference);

			}

			//Calculate Roll - If Specified
			if (_upDirection != Vector3D.Zero) {

				double angleRollLeftToUp = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Left, _upDirection);
				double angleRollRightToUp = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Right, _upDirection);

				if (angleRollLeftToUp == angleRollRightToUp) {

					angleRollLeftToUp--;
					angleRollRightToUp++;

				}

				RollTargetAngleResult = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Up, _upDirection) - VectorHelper.GetAngleBetweenDirections(referenceMatrix.Down, _upDirection);
				gyroRotation.Z = (float)CalculateGyroAxisRadians(angleRollLeftToUp, angleRollRightToUp, RollTargetAngleResult, gridSize, ref RollAngleDifference);

			} else {

				RollTargetAngleResult = 0;
				RollAngleDifference = 0;

			}

			this.RotationToApply = gyroRotation;

			/*
			var sb = new StringBuilder();
			sb.Append("Rotation Readout:").AppendLine().AppendLine();

			sb.Append("Pitch Angle Difference: ").Append(this.PitchAngleDifference.ToString()).AppendLine();
			sb.Append("Pitch Target Angle: ").Append((_upDirection != Vector3D.Zero && CurrentMode.HasFlag(NewAutoPilotMode.LevelWithGravity) ? this.AngleToUpDirection : angleToTarget).ToString()).AppendLine();
			sb.Append("Pitch Applied: ").Append(gyroRotation.X.ToString()).AppendLine().AppendLine();

			sb.Append("Yaw Angle Difference: ").Append(this.YawAngleDifference.ToString()).AppendLine();
			sb.Append("Yaw Target Angle: ").Append(this.AngleToCurrentWaypoint.ToString()).AppendLine();
			sb.Append("Yaw Applied: ").Append(gyroRotation.Y.ToString()).AppendLine().AppendLine();

			sb.Append("Roll Angle Difference: ").Append(this.RollAngleDifference.ToString()).AppendLine();
			sb.Append("Roll Target Angle: ").Append(this.AngleToUpDirection.ToString()).AppendLine();
			sb.Append("Roll Applied: ").Append(gyroRotation.Z.ToString()).AppendLine().AppendLine();

			Logger.MsgDebug(sb.ToString(), DebugTypeEnum.AutoPilotStats);
			*/
			return;

		}

		public void GetNextEligibleGyro() {

			for (int i = GyroProfiles.Count - 1; i >= 0; i--) {

				var profile = GyroProfiles[i];

				if (!profile.Valid) {

					GyroProfiles.RemoveAt(i);
					continue;

				}

				if (!profile.Working)
					continue;

				profile.Active = true;
				this.ActiveGyro = profile;
				break;

			}
		
		}

		public MatrixD GetReferenceMatrix(MatrixD originalMatrix) {

			if (_behavior.Settings.RotationDirection == Direction.Forward)
				return originalMatrix;

			if (_behavior.Settings.RotationDirection == Direction.Backward)
				return MatrixD.CreateWorld(originalMatrix.Translation, originalMatrix.Backward, originalMatrix.Up);

			if (_behavior.Settings.RotationDirection == Direction.Left)
				return MatrixD.CreateWorld(originalMatrix.Translation, originalMatrix.Left, originalMatrix.Up);

			if (_behavior.Settings.RotationDirection == Direction.Right)
				return MatrixD.CreateWorld(originalMatrix.Translation, originalMatrix.Right, originalMatrix.Up);

			if (_behavior.Settings.RotationDirection == Direction.Down)
				return MatrixD.CreateWorld(originalMatrix.Translation, originalMatrix.Down, originalMatrix.Forward);

			if (_behavior.Settings.RotationDirection == Direction.Up)
				return MatrixD.CreateWorld(originalMatrix.Translation, originalMatrix.Up, originalMatrix.Backward);

			return originalMatrix;

		}

		public void PrepareGyroForRotation() {

			if (ActiveGyro == null || !ActiveGyro.Active)
				GetNextEligibleGyro();

			if (ActiveGyro == null || !ActiveGyro.Active)
				return;

			ActiveGyro.UpdateRotation(this.RotationToApply.X, this.RotationToApply.Y, this.RotationToApply.Z, this.Data.RotationMultiplier, this.RefBlockMatrixRotation);

		}

		public void ProcessRotationParallel(bool hasWaypoint) {

			if (hasWaypoint && CurrentMode.HasFlag(NewAutoPilotMode.RotateToWaypoint)) {

				CalculateGyroRotation();

			} else {

				this.RotationToApply = Vector3.Zero;
			
			}

			if (CurrentMode.HasFlag(NewAutoPilotMode.BarrelRoll)) {

				this.RotationToApply.Z = 10;

			}

			PrepareGyroForRotation();

		}

		public void StopAllRotation() {

			this.RotationToApply = Vector3.Zero;
			ActiveGyro?.StopRotation();

		}

	}

}
