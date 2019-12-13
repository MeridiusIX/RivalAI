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
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI.Behavior.Subsystems.Profiles;

namespace RivalAI.Behavior.Subsystems {

    public enum ThrustMode {

        None,
        Strafe,
        ConstantForward

    }

	public class ThrustSystem{

        public bool AllowStrafing;
        public int StrafeMinDurationMs;
        public int StrafeMaxDurationMs;
        public int StrafeMinCooldownMs;
        public int StrafeMaxCooldownMs;
        public double StrafeSpeedCutOff;
        public double StrafeDistanceCutOff;
        public Vector3I AllowedStrafingDirectionsSpace;
        public Vector3I AllowedStrafingDirectionsPlanet;

        public IMyRemoteControl RemoteControl;
        public ThrustMode Mode;
        public List<ThrustProfile> ThrustProfiles;
        public Random Rnd;

        private AutoPilotSystem AutoPilot;
        private CollisionSystem Collision;

        public Vector3I PreviousAllowedThrust;
        public Vector3I PreviousRequiredThrust;
        public Vector3I CurrentAllowedThrust;
        public Vector3I CurrentRequiredThrust;

        public bool Strafing;
        public Vector3I CurrentStrafeDirections;
        public Vector3I CurrentAllowedStrafeDirections;
        public bool InvertStrafingActivated;
        public int ThisStrafeDuration;
        public int ThisStrafeCooldown;
        public DateTime LastStrafeStartTime;
        public DateTime LastStrafeEndTime;
		
        public ThrustSystem(IMyRemoteControl remoteControl){

            AllowStrafing = false;
            StrafeMinDurationMs = 750;
            StrafeMaxDurationMs = 1500;
            StrafeMinCooldownMs = 1000;
            StrafeMaxCooldownMs = 3000;
            StrafeSpeedCutOff = 100;
            StrafeDistanceCutOff = 1000;
            AllowedStrafingDirectionsSpace = new Vector3I(1, 1, 1);
            AllowedStrafingDirectionsPlanet = new Vector3I(1, 1, 1);

            RemoteControl = null;
            Mode = ThrustMode.None;
            ThrustProfiles = new List<ThrustProfile>();
            Rnd = new Random();

            CurrentAllowedThrust = Vector3I.Zero;
            CurrentRequiredThrust = Vector3I.Zero;

            if(StrafeMinDurationMs >= StrafeMaxDurationMs) {

                StrafeMaxDurationMs = StrafeMinDurationMs + 1;

            }

            if(StrafeMinCooldownMs >= StrafeMaxCooldownMs) {

                StrafeMaxCooldownMs = StrafeMinCooldownMs + 1;

            }

            Strafing = false;
            CurrentStrafeDirections = Vector3I.Zero;
            CurrentAllowedStrafeDirections = Vector3I.Zero;
            ThisStrafeDuration = Rnd.Next(StrafeMinDurationMs, StrafeMaxDurationMs);
            ThisStrafeCooldown = Rnd.Next(StrafeMinCooldownMs, StrafeMaxCooldownMs);
            LastStrafeStartTime = MyAPIGateway.Session.GameDateTime;
            LastStrafeEndTime = MyAPIGateway.Session.GameDateTime;

            Setup(remoteControl);


        }

		private void Setup(IMyRemoteControl remoteControl){
			
			if(remoteControl == null){

				return;
				
			}
			
			this.RemoteControl = remoteControl;
			var blockList = new List<IMySlimBlock>();
			this.RemoteControl.SlimBlock.CubeGrid.GetBlocks(blockList);
			
			foreach(var block in blockList.Where(item => item.FatBlock as IMyThrust != null)){

                this.ThrustProfiles.Add(new ThrustProfile(block.FatBlock as IMyThrust, this.RemoteControl));

			}
			
		}

        public void SetupReferences(AutoPilotSystem autoPilot, CollisionSystem collision) {

            this.AutoPilot = autoPilot;
            this.Collision = collision;

        }

        public void ChangeMode(ThrustMode newMode) {

            if(newMode == this.Mode) {

                return;

            }

            this.Mode = newMode;
            this.Strafing = false;
            this.SetThrust(new Vector3I(0, 0, 0), new Vector3I(0, 0, 0));

        }

        public void ProcessThrust(Vector3D upDirection = new Vector3D(), Vector3D destination = new Vector3D(), double minAltitude = -1, double minTargetDist = -1) {

            if(this.Mode == ThrustMode.None) {

                StopAllThrust();

            }

            if(Mode == ThrustMode.Strafe && this.AllowStrafing == true) {

                if(this.Strafing == false) {

                    TimeSpan duration = MyAPIGateway.Session.GameDateTime - this.LastStrafeEndTime;
                    if(duration.TotalMilliseconds >= this.ThisStrafeCooldown) {

                        //Logger.AddMsg("Begin Strafe", true);
                        this.LastStrafeStartTime = MyAPIGateway.Session.GameDateTime;
                        this.ThisStrafeDuration = Rnd.Next(StrafeMinDurationMs, StrafeMaxDurationMs);
                        this.Strafing = true;
                        Collision.RemoteControlPosition = this.RemoteControl.GetPosition();
                        Collision.RemoteMaxtrix = this.RemoteControl.WorldMatrix;

                        MyAPIGateway.Parallel.Start(() => {

                            Collision.CheckDirectionalCollisionsThreaded();
                            this.CurrentStrafeDirections = new Vector3I(Rnd.Next(-1, 2), Rnd.Next(-1, 2), Rnd.Next(-1, 2));

                            if(this.CurrentStrafeDirections.X != 0) {

                                if(this.CurrentStrafeDirections.X == 1) {

                                    if(Collision.RightResult.HasTarget == true && Collision.LeftResult.HasTarget == false) {

                                        //Logger.AddMsg("Strafe: X Reverse", true);
                                        this.CurrentStrafeDirections.X *= -1;

                                    } else if(Collision.RightResult.HasTarget == true && Collision.LeftResult.HasTarget == true) {

                                        //Logger.AddMsg("Strafe: X Negate", true);
                                        this.CurrentStrafeDirections.X = 0;

                                    }

                                } else {

                                    if(Collision.LeftResult.HasTarget == true && Collision.RightResult.HasTarget == false) {

                                        //Logger.AddMsg("Strafe: X Reverse", true);
                                        this.CurrentStrafeDirections.X *= -1;

                                    } else if(Collision.LeftResult.HasTarget == true && Collision.RightResult.HasTarget == true) {

                                        //Logger.AddMsg("Strafe: X Negate", true);
                                        this.CurrentStrafeDirections.X = 0;

                                    }

                                }

                            }

                            if(this.CurrentStrafeDirections.Y != 0) {

                                if(this.CurrentStrafeDirections.Y == 1) {

                                    if(Collision.UpResult.HasTarget == true && Collision.DownResult.HasTarget == false) {

                                        //Logger.AddMsg("Strafe: Y Reverse", true);
                                        this.CurrentStrafeDirections.Y *= -1;

                                    } else if(Collision.UpResult.HasTarget == true && Collision.DownResult.HasTarget == true) {

                                        //Logger.AddMsg("Strafe: Y Negate", true);
                                        this.CurrentStrafeDirections.Y = 0;

                                    }

                                } else {

                                    if(Collision.DownResult.HasTarget == true && Collision.UpResult.HasTarget == false) {

                                        //Logger.AddMsg("Strafe: Y Reverse", true);
                                        this.CurrentStrafeDirections.Y *= -1;

                                    } else if(Collision.DownResult.HasTarget == true && Collision.UpResult.HasTarget == true) {

                                        //Logger.AddMsg("Strafe: Y Negate", true);
                                        this.CurrentStrafeDirections.Y = 0;

                                    }

                                }

                            }

                            if(this.CurrentStrafeDirections.Z != 0) {

                                if(this.CurrentStrafeDirections.Z == 1) {

                                    if(Collision.ForwardResult.HasTarget == true && Collision.BackwardResult.HasTarget == false) {

                                        //Logger.AddMsg("Strafe: Z Reverse", true);
                                        this.CurrentStrafeDirections.Z *= -1;

                                    } else if(Collision.ForwardResult.HasTarget == true && Collision.BackwardResult.HasTarget == true) {

                                        //Logger.AddMsg("Strafe: Z Negate", true);
                                        this.CurrentStrafeDirections.Z = 0;

                                    }

                                } else {

                                    if(Collision.BackwardResult.HasTarget == true && Collision.ForwardResult.HasTarget == false) {

                                        //Logger.AddMsg("Strafe: Z Reverse", true);
                                        this.CurrentStrafeDirections.Z *= -1;

                                    } else if(Collision.BackwardResult.HasTarget == true && Collision.ForwardResult.HasTarget == true) {

                                        //Logger.AddMsg("Strafe: Z Negate", true);
                                        this.CurrentStrafeDirections.Z = 0;

                                    }

                                }

                            }


                        }, () => {

                            
                            MyAPIGateway.Utilities.InvokeOnGameThread(() => {

                                //Game Thread
                                SetThrust(this.CurrentAllowedStrafeDirections, this.CurrentStrafeDirections);

                            });

                        });

                        
                        //SafeStrafeDirections(upDirection, destination, minAltitude, minTargetDist);
                        //Logger.AddMsg("Duration: " + this.ThisStrafeDuration.ToString(), true);

                    }

                } else {

                    //SafeStrafeDirections(upDirection, destination, minAltitude, minTargetDist);

                    TimeSpan duration = MyAPIGateway.Session.GameDateTime - this.LastStrafeStartTime;

                    if(duration.TotalMilliseconds >= this.ThisStrafeDuration) {

                        //Logger.AddMsg("End Strafe", true);
                        this.InvertStrafingActivated = false;
                        this.LastStrafeEndTime = MyAPIGateway.Session.GameDateTime;
                        this.ThisStrafeCooldown = Rnd.Next(StrafeMinCooldownMs, StrafeMaxCooldownMs);
                        this.Strafing = false;
                        SetThrust(new Vector3I(0, 0, 0), new Vector3I(0, 0, 0));
                        //Logger.AddMsg("Cooldown: " + this.ThisStrafeCooldown.ToString(), true);

                    }

                }

            }

            if(Mode == ThrustMode.ConstantForward) {

                SetThrust(this.CurrentAllowedThrust, this.CurrentRequiredThrust);

            }

        }

        public void SafeStrafeDirections(Vector3D upDirection, Vector3D destination, double minAltitude, double minTargetDist){

            if(upDirection != new Vector3D()) {

                double elevation = 0;

                if(this.RemoteControl.TryGetPlanetElevation(Sandbox.ModAPI.Ingame.MyPlanetElevation.Surface, out elevation) == true) {

                    if(elevation < minAltitude) {

                        var newThrust = VectorHelper.GetThrustDirectionsAwayFromSurface(this.RemoteControl.WorldMatrix, upDirection, this.CurrentStrafeDirections);

                        if(newThrust != this.CurrentStrafeDirections) {

                            this.CurrentStrafeDirections = newThrust;
                            SetThrust(this.CurrentAllowedStrafeDirections, this.CurrentStrafeDirections);

                        }

                    }

                }

            }

            if(Vector3D.Distance(this.RemoteControl.GetPosition(), destination) < minTargetDist) {

                var dirToCollision = Vector3D.Normalize(destination - this.RemoteControl.GetPosition());
                var newThrust = VectorHelper.GetThrustDirectionsAwayFromSurface(this.RemoteControl.WorldMatrix, -dirToCollision, this.CurrentStrafeDirections);

                if(newThrust != this.CurrentStrafeDirections) {

                    this.CurrentStrafeDirections = newThrust;
                    SetThrust(this.CurrentAllowedStrafeDirections, this.CurrentStrafeDirections);

                }

            }

        }

        public void InvertStrafe(Vector3D collisionCoords) {

            if(this.Strafing == false) {

                return;

            }

            SetThrust(new Vector3I(0, 0, 0), new Vector3I(0, 0, 0));

            /*
            var dirToCollision = Vector3D.Normalize(collisionCoords - this.RemoteControl.GetPosition());
            var newThrust = VectorHelper.GetThrustDirectionsAwayFromSurface(this.RemoteControl.WorldMatrix, -dirToCollision, this.CurrentStrafeDirections);

            if(newThrust != this.CurrentStrafeDirections) {

                this.CurrentStrafeDirections = newThrust;
                SetThrust(this.CurrentAllowedStrafeDirections, this.CurrentStrafeDirections);

            }
            */

        }

        public void SetThrust(Vector3I allowedThrust, Vector3I requiredThrust) {

            if(this.PreviousAllowedThrust == allowedThrust && this.PreviousRequiredThrust == requiredThrust)
                return;

            foreach(var thrustProfile in this.ThrustProfiles.ToList()) {

                if(thrustProfile.ThrustBlock == null || MyAPIGateway.Entities.Exist(thrustProfile.ThrustBlock?.SlimBlock?.CubeGrid) == false) {

                    this.ThrustProfiles.Remove(thrustProfile);
                    continue;

                }

                if(thrustProfile.ThrustBlock.SlimBlock.CubeGrid != this.RemoteControl.SlimBlock.CubeGrid) {

                    this.ThrustProfiles.Remove(thrustProfile);
                    continue;

                }

                thrustProfile.UpdateThrust(allowedThrust, requiredThrust);

            }

            this.PreviousAllowedThrust = allowedThrust;
            this.PreviousRequiredThrust = requiredThrust;

        }

        public void StopAllThrust() {

            this.Strafing = false;
            this.SetThrust(new Vector3I(0, 0, 0), new Vector3I(0, 0, 0));

        }

	}
	
}