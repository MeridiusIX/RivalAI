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
	
	public enum AutoPilotMode{
		
		None, //No Movement Applied
		LegacyAutoPilot, //Uses Vanilla Remote Control Autopilot.
        FlyToWaypoint,
        FlyToTarget, 
		RotateToTarget, //Applies Gyro Rotation To Face Target. No Thrust.
        RotateToWaypoint, //Applies Gyro Rotation To Face Waypoint. No Thrust.
        RotateToTargetAndStrafe //Applies Gyro Rotation To Face Target. Random Thruster Strafing Included.
		
	}

	public class AutoPilotSystem{
		
		public IMyRemoteControl RemoteControl;

        public RotationSystem Rotation;
        public ThrustSystem Thrust;

        public bool CollisionAvoidance;


        public AutoPilotMode Mode;
        public AutoPilotMode PreviousMode;
        

        public float DesiredMaxSpeed;

        public bool WaypointChanged;
        public bool GetWaypointFromTarget;
        public Vector3D WaypointCoords;
        public Vector3D TargetCoords;
		public Vector3D UpDirection;
        public double MinimumTargetDistance;
		
		public AutoPilotSystem(IMyRemoteControl remoteControl = null) {
			
			RemoteControl = null;

            Rotation = new RotationSystem(remoteControl);
			Thrust = new ThrustSystem(remoteControl);

            Mode = AutoPilotMode.None;
            PreviousMode = AutoPilotMode.None;

            DesiredMaxSpeed = 100;

            WaypointChanged = false;
            GetWaypointFromTarget = false;
            WaypointCoords = Vector3D.Zero;
            TargetCoords = Vector3D.Zero;
			UpDirection = Vector3D.Zero;
            MinimumTargetDistance = 0;

            Setup(remoteControl);


        }

        public void ChangeAutoPilotMode(AutoPilotMode newMode) {

            //Handle Previous Mode First
            if(this.Mode == AutoPilotMode.LegacyAutoPilot) {

                SetRemoteControl(this.RemoteControl, false, Vector3D.Zero);

            }

            if(this.Mode == AutoPilotMode.FlyToTarget || this.Mode == AutoPilotMode.FlyToTarget || this.Mode == AutoPilotMode.RotateToTargetAndStrafe) {

                Rotation.StopAllRotation();
                Thrust.StopAllThrust();

            }

            if(this.Mode == AutoPilotMode.RotateToTarget || this.Mode == AutoPilotMode.RotateToWaypoint) {

                Rotation.StopAllRotation();

            }

            this.Mode = newMode;
            EngageAutoPilot();

        }

        public void EngageAutoPilot() {

            if(RAI_SessionCore.IsServer == false) {

                return;

            }

            this.UpDirection = Vector3D.Normalize(this.RemoteControl.GetNaturalGravity()) * -1;

            if(this.Mode != this.PreviousMode) {

                if(this.PreviousMode == AutoPilotMode.LegacyAutoPilot) {

                    SetRemoteControl(this.RemoteControl, false, Vector3D.Zero);

                }

                this.PreviousMode = this.Mode;

            }

            if(this.Mode == AutoPilotMode.None) {

                return;

            }

            if(this.Mode == AutoPilotMode.LegacyAutoPilot) {

                if(this.WaypointCoords != this.TargetCoords && this.GetWaypointFromTarget == true) {

                    this.WaypointCoords = this.TargetCoords;
                    SetRemoteControl(this.RemoteControl, true, this.WaypointCoords);

                }

                if(this.WaypointChanged == true) {

                    this.WaypointChanged = false;
                    SetRemoteControl(this.RemoteControl, true, this.WaypointCoords);

                }

            }

        }

        public void UpdateWaypoint(Vector3D coords) {

            this.WaypointCoords = coords;
            this.WaypointChanged = true;

        }
		
		private void Setup(IMyRemoteControl remoteControl){
			
			if(remoteControl == null || MyAPIGateway.Entities.Exist(remoteControl?.SlimBlock?.CubeGrid) == false){

				return;
				
			}
			
			this.RemoteControl = remoteControl;

        }
		
		//SetRemoteControl
		public void SetRemoteControl(IMyRemoteControl remoteControl, bool enabled, Vector3D targetCoords, float speedLimit = 100, bool collisionAvoidance = false, bool precisionMode = false, Sandbox.ModAPI.Ingame.FlightMode flightMode = Sandbox.ModAPI.Ingame.FlightMode.OneWay, Base6Directions.Direction direction = Base6Directions.Direction.Forward){
			
			if(remoteControl == null){
				
				return;
				
			}
			
			if(enabled == false){
				
				remoteControl.SetAutoPilotEnabled(enabled);
				return;
				
			}
			
			remoteControl.ClearWaypoints();
			remoteControl.AddWaypoint(targetCoords, "TargetCoords");
			remoteControl.SpeedLimit = speedLimit;
			remoteControl.SetCollisionAvoidance(collisionAvoidance);
			remoteControl.SetDockingMode(precisionMode);
			remoteControl.FlightMode = flightMode;
			remoteControl.Direction = direction;
			remoteControl.SetAutoPilotEnabled(enabled);
			
		}
		
	}
	
}