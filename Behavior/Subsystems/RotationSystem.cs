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
	
	public class RotationSystem{
		
		public float RotationMultiplier;
		
			public bool RotationEnabled;
		public IMyGyro ControlGyro;
		public IMyRemoteControl RemoteControl;
		public IMyTerminalBlock ReferenceBlock;
		public IMyCubeGrid CubeGrid;
		public List<IMyGyro> BrokenGyros;

		public bool ControlGyroNotFound;
		public bool NewGyroFound;

		public MatrixD RefBlockMatrix;
		public MatrixD GyroMatrix;
		
		public Vector3D RotationTarget;
		public Vector3D UpDirection;

		public bool ControlYaw;
		public bool ControlPitch;
		public bool ControlRoll;

		
		public float ControlGyroStrength;

		public bool UpdateMassAndForceBeforeRotation;
		public double GridMass;
		public double GridGyroForce;
		public float GyroMaxPower;
		
		public double CurrentAngleToTarget;
		public double CurrentYawDifference;
		public double CurrentPitchDifference;
		public double CurrentRollDifference;
		public double DesiredAngleToTarget;

		public bool BarrelRollEnabled;
		public double BarrellRollMagnitudePerEvent;

		public Vector3 RotationToApply;
		public Dictionary<string, Vector3D> ControlGyroRotationTranslation;
		
		public RotationSystem(IMyRemoteControl remoteControl){
			
			RotationMultiplier = 1;
			
			RotationEnabled = false;
			ControlGyro = null;
			RemoteControl = null;
			ReferenceBlock = null;
			BrokenGyros = new List<IMyGyro>();

			ControlGyroNotFound = false;
			NewGyroFound = false;

			RotationTarget = Vector3D.Zero;
			UpDirection = Vector3D.Zero;
			
			ControlYaw = true;
			ControlPitch = true;
			ControlRoll = true;
			
			
			ControlGyroStrength = 1;

			UpdateMassAndForceBeforeRotation = true;
			GridMass = 0;
			GridGyroForce = 0;
			GyroMaxPower = 100;
			
			CurrentAngleToTarget = 0;
			CurrentYawDifference = 0;
			CurrentPitchDifference = 0;
			CurrentRollDifference = 0;
			DesiredAngleToTarget = 0.5;

			BarrelRollEnabled = false;
			BarrellRollMagnitudePerEvent = 1;
			
			RotationToApply = Vector3.Zero;
			ControlGyroRotationTranslation = new Dictionary<string, Vector3D>();

			Setup(remoteControl);
			
		}
		
		private void Setup(IMyRemoteControl remoteControl){
			
			if(remoteControl == null){

				return;
				
			}
			
			this.RemoteControl = remoteControl;
			this.CubeGrid = remoteControl.SlimBlock.CubeGrid;
			UpdateMassAndGyroForce();
			
		}
		
		public IMyGyro GetControlGyro(IMyCubeGrid cubeGrid){
			
			if(cubeGrid == null || MyAPIGateway.Entities.Exist(cubeGrid) == false){
				
				return null;
				
			}
			
			IMyGyro gyro = null;
			var blockList = new List<IMySlimBlock>();
			cubeGrid.GetBlocks(blockList);
			
			foreach(var slimBlock in blockList){
				
				if(slimBlock.FatBlock == null || slimBlock.CubeGrid != cubeGrid){
					
					continue;
					
				}
				
				var testGyro = slimBlock.FatBlock as IMyGyro;
				
				if(testGyro == null){
					
					continue;
					
				}

				if(testGyro.IsFunctional == false || testGyro.IsWorking == false){
					
					continue;
					
				}
				
				if(gyro == null){
					
					gyro = testGyro;
					
				}
				
				gyro.GyroOverride = false;
				gyro.GyroPower = 1;
				gyro.Yaw = 0;
				gyro.Pitch = 0;
				gyro.Roll = 0;


			}
			
			return gyro;
			
		}
		
		public void StartCalculation(Vector3D rotationTarget, IMyTerminalBlock block = null, Vector3D upDirection = new Vector3D()){

			if(this.ControlGyroNotFound == true) {

				return;

			}

			if(block == null || MyAPIGateway.Entities.Exist(block?.SlimBlock?.CubeGrid) == false){

				return;
				
			}

			if(this.ControlGyro == null || MyAPIGateway.Entities.Exist(this.ControlGyro?.SlimBlock?.CubeGrid) == false) {

				this.ControlGyro = GetControlGyro(this.RemoteControl.SlimBlock.CubeGrid);

				if(this.ControlGyro == null) {

					ControlGyroNotFound = true;
					return;

				} else {

					this.NewGyroFound = true;

				}

			}

			this.RotationEnabled = true;
			this.ReferenceBlock = block;
			this.RotationTarget = rotationTarget;
			this.RefBlockMatrix = this.ReferenceBlock.WorldMatrix;
			this.GyroMatrix = this.ControlGyro.WorldMatrix;
			this.UpDirection = upDirection;
			MyAPIGateway.Parallel.Start(CalculateGyroRotation, ApplyGyroRotation);
			
		}

		public void StopAllRotation() {

			this.RotationEnabled = false;
			this.BarrelRollEnabled = false;

			if(this.ControlGyro != null && MyAPIGateway.Entities.Exist(this.ControlGyro?.SlimBlock?.CubeGrid) == true) {

				//Logger.AddMsg("Stopping Control Gyro", true);
				this.ControlGyro.GyroOverride = false;

			}

		}
		
		public void ApplyGyroRotation(){

			MyAPIGateway.Utilities.InvokeOnGameThread(() => {

				foreach(var gyro in this.BrokenGyros.ToList()) {

					if(gyro != null && MyAPIGateway.Entities.Exist(gyro?.SlimBlock?.CubeGrid) == true) {

						gyro.GyroOverride = false;

					}

					this.BrokenGyros.Remove(gyro);

				}

				if(MyAPIGateway.Entities.Exist(this.ControlGyro?.SlimBlock?.CubeGrid) == false || this.ControlGyro == null) {

					this.ControlGyro = null;
					return;

				}

				if(this.RotationToApply != Vector3.Zero && this.RotationEnabled == true) {

					this.ControlGyro.GyroOverride = true;

				} else {

					this.ControlGyro.GyroOverride = false;

				}

				this.ControlGyro.GyroPower = this.ControlGyroStrength;
				this.ControlGyro.Yaw = this.RotationToApply.Y * this.RotationMultiplier;
				this.ControlGyro.Pitch = this.RotationToApply.X * this.RotationMultiplier;
				this.ControlGyro.Roll = this.RotationToApply.Z * this.RotationMultiplier;
				//Logger.AddMsg(this.ControlGyro.Pitch.ToString() + " - " + this.ControlGyro.Yaw.ToString() + " - " + this.ControlGyro.Roll.ToString(), true);

				if(this?.ControlGyro?.SlimBlock?.CubeGrid?.Physics != null && this.BarrelRollEnabled == false) {

					if(this.CurrentAngleToTarget < 45) {

						var angularVel = this.ControlGyro.SlimBlock.CubeGrid.Physics.AngularVelocity;
						var angularMag = angularVel.Length();

						if(angularMag > 0.4) {

							//Logger.AddMsg("Fix Rotation", true);
							this.ControlGyro.SlimBlock.CubeGrid.Physics.AngularVelocity = angularVel - (angularVel * 0.25f);

						}

					}

				}

			});
			
		}
		
		//Updated Method
		public void CalculateGyroRotation(){

			bool updatedGyro = false;
			//Check That Control Gyro Exists
			if(this.ControlGyro == null){
				
				this.ControlGyro = GetControlGyro(CubeGrid);
				updatedGyro = true;

				if(this.ControlGyro == null){

				   return;
					
				}

			}
			
			//Check If Control Gyro is Functional
			if(this.ControlGyro.IsFunctional == false || this.ControlGyro.IsWorking == false) {
				
				this.ControlGyro = GetControlGyro(CubeGrid);
				updatedGyro = true;

				if(this.ControlGyro == null){

					return;
					
				}
				
			}

			if(this.ReferenceBlock == null){

			  return;
				
			}

			if(this.NewGyroFound == true) {

				this.NewGyroFound = false;
				this.ControlGyroRotationTranslation = VectorHelper.GetTransformedGyroRotations(this.RefBlockMatrix, this.GyroMatrix);

			}

			if(this.RotationEnabled == false){

				this.ControlGyro.GyroOverride = false;
				return;
				
			}

			MatrixD referenceMatrix = this.RefBlockMatrix;
			Vector3D directionToTarget = Vector3D.Normalize(this.RotationTarget - referenceMatrix.Translation);
			Vector3 gyroRotation = new Vector3(0,0,0); // Pitch,Yaw,Roll

			if(this.BarrelRollEnabled == false) {

				//Get Actual Angle To Target
				double angleToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Forward, directionToTarget);
				this.CurrentAngleToTarget = angleToTarget;
				//MyVisualScriptLogicProvider.ShowNotificationToAll("Total Angle: " + angleToTarget.ToString(), 166);


				if(angleToTarget <= this.DesiredAngleToTarget && this.UpDirection == Vector3D.Zero) {

					this.RotationToApply = Vector3.Zero;
					return;

				}

				var gridSize = (ControlGyro.CubeGrid.GridSizeEnum == MyCubeSize.Small);

				//Calculate Yaw
				double angleLeftToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Left, directionToTarget);
				double angleRightToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Right, directionToTarget);
				gyroRotation.Y = (float)CalculateAxisRotation(angleLeftToTarget, angleRightToTarget, angleToTarget, gridSize);

				//Calculate Pitch
				double angleUpToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Up, directionToTarget);
				double angleDownToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Down, directionToTarget);
				gyroRotation.X = (float)CalculateAxisRotation(angleDownToTarget, angleUpToTarget, angleToTarget, gridSize);

				//Calculate Roll - If Specified
				if(this.UpDirection != Vector3D.Zero) {

					double rollAngleToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Up, this.UpDirection);
					double angleRollLeftToUp = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Left, this.UpDirection);
					double angleRollRightToUp = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Right, this.UpDirection);

					if (angleRollLeftToUp == angleRollRightToUp) {

						angleRollLeftToUp--;
						angleRollRightToUp++;

					}
					
					gyroRotation.Z = (float)CalculateAxisRotation(angleRollLeftToUp, angleRollRightToUp, rollAngleToTarget, gridSize);

				}

			} else {

				gyroRotation.Z = 10;

			}
			
			
			if(this.ControlYaw == false){
				
				gyroRotation.Y = 0;
				
			}
			
			if(this.ControlPitch == false){
				
				gyroRotation.X = 0;
				
			}
			
			if(this.ControlRoll == false){
				
				gyroRotation.Z = 0;
				
			}

			var pitchVector = Vector3D.Zero;
			var yawVector = Vector3D.Zero;
			var rollVector = Vector3D.Zero;

			this.ControlGyroRotationTranslation.TryGetValue("Pitch", out pitchVector);
			this.ControlGyroRotationTranslation.TryGetValue("Yaw", out yawVector);
			this.ControlGyroRotationTranslation.TryGetValue("Roll", out rollVector); //

			pitchVector *= gyroRotation.X;
			yawVector *= gyroRotation.Y;
			rollVector *= gyroRotation.Z;
			this.RotationToApply = pitchVector + yawVector + rollVector;
			
			/*
			var sb = new StringBuilder();
			sb.Append("RotationData:").AppendLine();

			if(gyroRotation.X > 0) {

				sb.Append(" - Pitch: Up").AppendLine();

			} else if(gyroRotation.X < 0) {

				sb.Append(" - Pitch: Down").AppendLine();

			}

			if(gyroRotation.Y > 0) {

				sb.Append(" - Yaw:   Right").AppendLine();

			} else if(gyroRotation.Y < 0) {

				sb.Append(" - Yaw:   Left").AppendLine();

			}

			if(gyroRotation.Z > 0) {

				sb.Append(" - Roll:  Right").AppendLine();

			} else if(gyroRotation.Z < 0) {

				sb.Append(" - Roll:  Left").AppendLine();

			}
			
			sb.Append(" - Angle To Target: ").Append(this.CurrentAngleToTarget.ToString()).AppendLine();
			sb.Append(" - Required Rotation: ").Append(gyroRotation.ToString()).AppendLine();
			sb.Append(" - Actual Rotation:   ").Append(this.RotationToApply.ToString()).AppendLine();
			
			//sb.Append(" - ").Append().AppendLine();

			Logger.AddMsg(sb.ToString(), true);
			*/

			//gyroRotation.Y *= -1;
			//this.RotationToApply = Vector3.TransformNormal(gyroRotation, this.ControlGyro.Orientation);
			//this.RotationToApply.Y *= -1;

		}

		public double CalculateAxisRotation(double angleA, double angleB, double totalAngle, bool isSmallGrid = false){
			
			double angleDifference = 0;
			double angleDirection = 1;
			
			if(angleA > angleB){
				
				//Positive
				angleDifference = angleA - angleB;
				
			}else{
				
				//Negative
				angleDirection = -1;
				angleDifference = angleB - angleA;
				
			}

			if(angleDifference <= this.DesiredAngleToTarget){
				
				return 0;
				
			}

			if(totalAngle + angleDifference >= 170) {

				//Logger.MsgDebug((totalAngle + angleDifference).ToString(), DebugTypeEnum.Dev);
				return isSmallGrid ? Math.PI * 2 * angleDirection : Math.PI * angleDirection;
				
			}

			return (angleDifference * (Math.PI / 180)) * angleDirection;

		}
		
		public void UpdateMassAndGyroForce(){
			
			this.GridMass = (double)this.RemoteControl.CalculateShipMass().TotalMass;
			//Logger.AddMsg("GridMass: " + this.GridMass.ToString(), true);
			var blockList = new List<IMySlimBlock>();
			this.RemoteControl.SlimBlock.CubeGrid.GetBlocks(blockList);
			double totalForceMagnitude = 0;
			
			foreach(var block in blockList){
				
				if(block.FatBlock == null){
					
					continue;
					
				}
				
				var gyro = block.FatBlock as IMyGyro;
				
				if(gyro == null){
					
					continue;
					
				}
				
				var gyroDefinition = (MyGyroDefinition)block.BlockDefinition;
				
				if(gyroDefinition == null){
					
					continue;
					
				}
				
				totalForceMagnitude += (double)gyroDefinition.ForceMagnitude;
				
			}
			
			this.GridGyroForce = totalForceMagnitude;
			//Logger.AddMsg("GridRotationMagnitude: " + this.GridGyroForce.ToString(), true);

			var numA = 0.004 * this.GridGyroForce;
			var numB = this.GridMass / numA;

			if(numB >= 1) {

				numB = 1;

			}

			this.GyroMaxPower = (float)numB;
			
			
		}
		
		
		
	}
	
}
