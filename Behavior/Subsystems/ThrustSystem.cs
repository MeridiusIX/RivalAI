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
	
	public class ThrustSystem{
		
		public IMyRemoteControl RemoteControl;
		
		public List<IMyThrust> AllThrust;

		public List<IMyThrust> ForwardThrust;
        public double ForwardThrustForce;

        public List<IMyThrust> BackwardThrust;
        public double BackwardThrustForce;

        public List<IMyThrust> UpThrust;
        public double UpThrustForce;

        public List<IMyThrust> DownThrust;
        public double DownThrustForce;

        public List<IMyThrust> LeftThrust;
        public double LeftThrustForce;

        public List<IMyThrust> RightThrust;
        public double RightThrustForce;

        public ThrustSystem(IMyRemoteControl remoteControl){
			
			RemoteControl = null;
			
			AllThrust = new List<IMyThrust>();

			ForwardThrust = new List<IMyThrust>();
            ForwardThrustForce = 0;

			BackwardThrust = new List<IMyThrust>();
            BackwardThrustForce = 0;

            UpThrust = new List<IMyThrust>();
            UpThrustForce = 0;

            DownThrust = new List<IMyThrust>();
            DownThrustForce = 0;

            LeftThrust = new List<IMyThrust>();
            LeftThrustForce = 0;

            RightThrust = new List<IMyThrust>();
            RightThrustForce = 0;

            Setup(remoteControl);


        }
		
		private void Setup(IMyRemoteControl remoteControl){
			
			if(remoteControl == null){

				return;
				
			}
			
			this.RemoteControl = remoteControl;
			var blockList = new List<IMySlimBlock>();
			this.RemoteControl.SlimBlock.CubeGrid.GetBlocks(blockList);
			
			var rcEntity = this.RemoteControl as IMyEntity;
			
			foreach(var block in blockList.Where(item => item.FatBlock as IMyThrust != null)){
				
				var thrust = block.FatBlock as IMyThrust;
                thrust.IsWorkingChanged += UpdateThrustForces;
				AllThrust.Add(thrust);
				
				if(this.RemoteControl.WorldMatrix.Forward == thrust.WorldMatrix.Backward){

                    this.ForwardThrust.Add(thrust);
                    this.ForwardThrustForce += (double)thrust.MaxEffectiveThrust;
					
				}
				
				if(this.RemoteControl.WorldMatrix.Backward == thrust.WorldMatrix.Forward){

                    this.BackwardThrust.Add(thrust);
                    this.BackwardThrustForce += (double)thrust.MaxEffectiveThrust;

                }
				
				if(this.RemoteControl.WorldMatrix.Up == thrust.WorldMatrix.Down){

                    this.UpThrust.Add(thrust);
                    this.UpThrustForce += (double)thrust.MaxEffectiveThrust;

                }
				
				if(this.RemoteControl.WorldMatrix.Down == thrust.WorldMatrix.Up){

                    this.DownThrust.Add(thrust);
                    this.DownThrustForce += (double)thrust.MaxEffectiveThrust;

                }
				
				if(this.RemoteControl.WorldMatrix.Left == thrust.WorldMatrix.Right){

                    this.LeftThrust.Add(thrust);
                    this.LeftThrustForce += (double)thrust.MaxEffectiveThrust;

                }
				
				if(this.RemoteControl.WorldMatrix.Right == thrust.WorldMatrix.Left){

                    this.RightThrust.Add(thrust);
                    this.RightThrustForce += (double)thrust.MaxEffectiveThrust;

                }
				
			}
			
		}

        public void StopAllThrust() {



        }

        public void UpdateThrustForces(IMyCubeBlock cubeBlock) {

            var thrust = cubeBlock as IMyThrust;

            if(thrust == null) {

                return;

            }

            //Forward
            if(this.ForwardThrust.Contains(thrust) == true) {

                if(thrust.IsWorking == true && thrust.IsFunctional == true) {

                    ForwardThrustForce += thrust.MaxEffectiveThrust;

                } else {

                    ForwardThrustForce -= thrust.MaxEffectiveThrust;

                }

            }

            //Backward
            if(this.BackwardThrust.Contains(thrust) == true) {

                if(thrust.IsWorking == true && thrust.IsFunctional == true) {

                    BackwardThrustForce += thrust.MaxEffectiveThrust;

                } else {

                    BackwardThrustForce -= thrust.MaxEffectiveThrust;

                }

            }

            //Up
            if(this.UpThrust.Contains(thrust) == true) {

                if(thrust.IsWorking == true && thrust.IsFunctional == true) {

                    UpThrustForce += thrust.MaxEffectiveThrust;

                } else {

                    UpThrustForce -= thrust.MaxEffectiveThrust;

                }

            }

            //Down
            if(this.DownThrust.Contains(thrust) == true) {

                if(thrust.IsWorking == true && thrust.IsFunctional == true) {

                    DownThrustForce += thrust.MaxEffectiveThrust;

                } else {

                    DownThrustForce -= thrust.MaxEffectiveThrust;

                }

            }

            //Left
            if(this.LeftThrust.Contains(thrust) == true) {

                if(thrust.IsWorking == true && thrust.IsFunctional == true) {

                    LeftThrustForce += thrust.MaxEffectiveThrust;

                } else {

                    LeftThrustForce -= thrust.MaxEffectiveThrust;

                }

            }

            //Right
            if(this.RightThrust.Contains(thrust) == true) {

                if(thrust.IsWorking == true && thrust.IsFunctional == true) {

                    RightThrustForce += thrust.MaxEffectiveThrust;

                } else {

                    RightThrustForce -= thrust.MaxEffectiveThrust;

                }

            }

        }
		
	}
	
}