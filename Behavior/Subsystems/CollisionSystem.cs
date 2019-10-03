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
        public double CollisionCheckDistance;

        //General
        public IMyRemoteControl RemoteControl;
		public Vector3D RemoteControlPosition;
		public Vector3 GridVelocity;
		public IMyCubeGrid CubeGrid;

        //Results
        public bool CollisionDetected = false;
        public Vector3D ClosestCollision;
		public double ClosestDistance;
        public double TimeToCollision;
		public CollisionDetectType CollisionDetectType;
        public IMyEntity ClosestEntity;

        //Planet
        public MyPlanet Planet;
        public bool InGravity;


        public CollisionSystem(IMyRemoteControl remoteControl = null){
			
			//General
			
			RemoteControl = null;
			RemoteControlPosition = Vector3D.Zero;
			GridVelocity = Vector3.Zero;
			CubeGrid = null;

            //Detect Settings
            UseCollisionDetection = false;
            UseVoxelDetection = false;
			UseGridDetection = false;
			UseSafeZoneDetection = false;
			UseDefenseShieldDetection = false;
            UsePlayerDetection = false;
            CollisionCheckDistance = 600;

            //Results
            CollisionDetected = false;
            ClosestCollision = Vector3D.Zero;
			ClosestDistance = 0;
            TimeToCollision = 0;
            CollisionDetectType = CollisionDetectType.None;
            ClosestEntity = null;

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

            if(string.IsNullOrWhiteSpace(remoteControl.CustomData) == false) {

                var descSplit = remoteControl.CustomData.Split('\n');

                foreach(var tag in descSplit) {

                    //UseCollisionDetection
                    if(tag.Contains("[UseCollisionDetection") == true) {

                        this.UseCollisionDetection = TagHelper.TagBoolCheck(tag);

                    }

                    //UseVoxelDetection
                    if(tag.Contains("[UseVoxelDetection") == true) {

                        this.UseVoxelDetection = TagHelper.TagBoolCheck(tag);

                    }

                    //UseGridDetection
                    if(tag.Contains("[UseGridDetection") == true) {

                        this.UseGridDetection = TagHelper.TagBoolCheck(tag);

                    }

                    //UseSafeZoneDetection
                    if(tag.Contains("[UseSafeZoneDetection") == true) {

                        this.UseSafeZoneDetection = TagHelper.TagBoolCheck(tag);

                    }

                    //UseDefenseShieldDetection
                    if(tag.Contains("[UseDefenseShieldDetection") == true) {

                        this.UseDefenseShieldDetection = TagHelper.TagBoolCheck(tag);

                    }

                    //UsePlayerDetection
                    if(tag.Contains("[UsePlayerDetection") == true) {

                        this.UsePlayerDetection = TagHelper.TagBoolCheck(tag);

                    }

                    //CollisionCheckDistance
                    if(tag.Contains("[CollisionCheckDistance") == true) {

                        this.CollisionCheckDistance = TagHelper.TagDoubleCheck(tag, this.CollisionCheckDistance);

                    }

                }

            }

            

        }

		public void RequestCheckCollisions(){

			if(RAI_SessionCore.IsServer == false || this.UseCollisionDetection == false){
				
                this.CollisionDetected = false;
                return;
				
			}
			
			if(this.CubeGrid.Physics == null || this.CubeGrid.IsStatic == true){

                this.CollisionDetected = false;
                return;
				
			}
			
			if(this.CubeGrid.Physics.LinearVelocity.Length() < 0.1f){

                this.CollisionDetected = false;
                return;
				
			}

            this.CollisionDetectType = CollisionDetectType.None;
            this.ClosestCollision = Vector3D.Zero;
            this.ClosestDistance = 0;
			this.RemoteControlPosition = RemoteControl.GetPosition();
			this.GridVelocity = this.CubeGrid.Physics.LinearVelocity;
			MyAPIGateway.Parallel.Start(CheckCollisionsThreaded, null);
			
		}
		
		private void CheckCollisionsThreaded(){

            var direction = Vector3D.Normalize((Vector3D)this.GridVelocity);
            var result = TargetHelper.CheckCollisions((IMyTerminalBlock)this.RemoteControl, direction, this.CollisionCheckDistance, this.UseVoxelDetection, this.UseGridDetection, this.UseSafeZoneDetection, this.UseDefenseShieldDetection, this.UsePlayerDetection);

            this.CollisionDetected = result.HasTarget;
            this.ClosestCollision = result.Coords;
            this.ClosestDistance = result.Distance;
            this.CollisionDetectType = result.Type;
            this.ClosestEntity = result.Entity;

            if(this.ClosestDistance > 0 && this.GridVelocity.Length() > 0) {

                this.TimeToCollision = this.ClosestDistance / this.GridVelocity.Length();

            }

        }

	}
	
}