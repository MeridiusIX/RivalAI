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
		
		public bool RotationEnabled;
		public IMyGyro ControlGyro;
		public IMyRemoteControl RemoteControl;
		public IMyTerminalBlock ReferenceBlock;
		public IMyCubeGrid CubeGrid;
        public List<IMyGyro> BrokenGyros;
		
		public Vector3D RotationTarget;
		public Vector3D UpDirection;
		
		public bool ControlYaw;
		public bool ControlPitch;
		public bool ControlRoll;

		public double MinimumRotationMultiplier;
		public double RotationMultiplier;
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

        public Vector3 RotationToApply;
		
		public RotationSystem(IMyRemoteControl remoteControl){
			
			RotationEnabled = false;
			ControlGyro = null;
			RemoteControl = null;
			ReferenceBlock = null;
            BrokenGyros = new List<IMyGyro>();

            RotationTarget = Vector3D.Zero;
			UpDirection = Vector3D.Zero;
			
			ControlYaw = true;
			ControlPitch = true;
			ControlRoll = true;
			
			MinimumRotationMultiplier = .005;
			RotationMultiplier = 1;
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
			
			RotationToApply = Vector3.Zero;

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
			
			if(block == null || MyAPIGateway.Entities.Exist(block?.SlimBlock?.CubeGrid) == false){

                return;
				
			}

            this.ReferenceBlock = block;
			this.RotationTarget = rotationTarget;
            MyAPIGateway.Parallel.Start(CalculateGyroRotation, ApplyGyroRotation);
			
			
		}

        public void StopAllRotation() {

            if(this.ControlGyro != null && MyAPIGateway.Entities.Exist(this.ControlGyro?.SlimBlock?.CubeGrid) == true) {

                this.ControlGyro.GyroOverride = false;

            }

            this.RotationEnabled = false;

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

                    return;

                }

                if(this.RotationToApply != Vector3.Zero) {

                    this.ControlGyro.GyroOverride = true;

                } else {

                    this.ControlGyro.GyroOverride = false;

                }

                this.ControlGyro.GyroPower = this.ControlGyroStrength;
                this.ControlGyro.Yaw = this.RotationToApply.X;
                this.ControlGyro.Pitch = this.RotationToApply.Y;
                this.ControlGyro.Roll = this.RotationToApply.Z;

            });
			
		}
		
		//Updated Method
		public void CalculateGyroRotation(){
			
			//Check That Control Gyro Exists
			if(this.ControlGyro == null){
				
				this.ControlGyro = GetControlGyro(CubeGrid);
				
				if(this.ControlGyro == null){

                   return;
					
				}

			}
			
			//Check If Control Gyro is Functional
			if(this.ControlGyro.IsFunctional == false || this.ControlGyro.IsWorking == false) {
				
				this.ControlGyro = GetControlGyro(CubeGrid);
				
				if(this.ControlGyro == null){

                    return;
					
				}
				
			}
			
			if(this.ReferenceBlock == null){

              return;
				
			}
			
			if(this.RotationEnabled == false){

                this.ControlGyro.GyroOverride = false;
				return;
				
			}

            if(this.UpdateMassAndForceBeforeRotation == true) {

                this.UpdateMassAndForceBeforeRotation = false;
                UpdateMassAndGyroForce();

            }

            float maxRotationSlider = 3.14f;

            if(this.ControlGyro.CubeGrid.GridSizeEnum == MyCubeSize.Small) {

                maxRotationSlider = 6.28f;

            }
			
			MatrixD referenceMatrix = ReferenceBlock.WorldMatrix;
			Vector3D directionToTarget = Vector3D.Normalize(this.RotationTarget - referenceMatrix.Translation);
			Vector3 gyroRotation = new Vector3(0,0,0); // Pitch,Yaw,Roll
			
			//Get Actual Angle To Target
			double angleToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Forward, directionToTarget);
            //MyVisualScriptLogicProvider.ShowNotificationToAll("Total Angle: " + angleToTarget.ToString(), 166);


            if(angleToTarget <= this.DesiredAngleToTarget && this.UpDirection == Vector3D.Zero){

				this.RotationToApply = Vector3.Zero;
				return;
			
			}

            //Calculate Yaw
            double angleLeftToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Left, directionToTarget);
            double angleRightToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Right, directionToTarget);
			double tempYawAngleDifference = 0;
            gyroRotation.X = (float)CalculateAxisRotation(angleRightToTarget, angleLeftToTarget, angleToTarget, out tempYawAngleDifference) * maxRotationSlider;
			this.CurrentYawDifference = tempYawAngleDifference;
			
            //Calculate Pitch
            double angleUpToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Up, directionToTarget);
			double angleDownToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Down, directionToTarget);
			double tempPitchAngleDifference = 0;
            gyroRotation.Y = (float)CalculateAxisRotation(angleUpToTarget, angleDownToTarget, angleToTarget, out tempPitchAngleDifference) * maxRotationSlider;
            this.CurrentPitchDifference = tempPitchAngleDifference;
			
            //Calculate Roll - If Specified
            if(this.UpDirection != Vector3D.Zero){

                double rollAngleToTarget = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Up, this.UpDirection);
                double angleRollLeftToUp = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Left, this.UpDirection);
				double angleRollRightToUp = VectorHelper.GetAngleBetweenDirections(referenceMatrix.Right, this.UpDirection);
				double tempRollAngleDifference = 0;
                gyroRotation.Z = (float)CalculateAxisRotation(angleRollLeftToUp, angleRollRightToUp, rollAngleToTarget, out tempRollAngleDifference) * maxRotationSlider;
				this.CurrentRollDifference = tempRollAngleDifference;

            }
			
			if(this.ControlYaw == false){
				
				gyroRotation.X = 0;
				
			}
			
			if(this.ControlPitch == false){
				
				gyroRotation.Y = 0;
				
			}
			
			if(this.ControlRoll == false){
				
				gyroRotation.Z = 0;
				
			}
			
            this.RotationToApply = Vector3.TransformNormal(gyroRotation, this.ControlGyro.Orientation);
            this.RotationToApply.Y *= -1;
            
        }
		
		public double CalculateAxisRotation(double angleA, double angleB, double totalAngle, out double angleDifference){
			
			angleDifference = 0;
			double angleDirection = 1;
			
			if(angleA > angleB){
				
				//Positive
				angleDifference = angleA - angleB;
				
			}else{
				
				//Negative
				angleDirection = -1;
				angleDifference = angleB - angleA;
				
			}

            //MyVisualScriptLogicProvider.ShowNotificationToAll("Angle Diff: " + Math.Round(angleDifference, 2).ToString(), 166);
			
			if(angleDifference <= this.DesiredAngleToTarget){
				
				return 0;
				
			}
			
			if(totalAngle >= 90 && angleDifference >= 180) {
				
				return 1 * angleDirection;
				
			}
			
			var numB = angleDifference / 180;

            if(numB < this.MinimumRotationMultiplier) {

                numB = this.MinimumRotationMultiplier;

            }
			
			//TODO: Validate this is done
			return numB * angleDirection;
			
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