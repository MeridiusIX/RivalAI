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
using RivalAI.Behavior.Settings;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;

namespace RivalAI.Behavior{
	
	public class Fighter : CoreBehavior{

        //Configurable
        public double FighterEngageDistanceSpace;
        public double FighterEngageDistancePlanet;

        public byte Counter;

        public Fighter() {

            FighterEngageDistanceSpace = 300;
            FighterEngageDistancePlanet = 300;
            Counter = 0;

        }

        public void RunAi() {

            if(Systems.Owner.NpcOwned == false) {

                return;

            }

            RunCoreAi();

            if(EndScript == true) {

                return;

            }

            Counter++;

            if(Counter >= 60) {

                MainBehavior();
                Counter = 0;

            }


        }

        public void MainBehavior() {

            if(RAI_SessionCore.IsServer == false) {

                return;

            }

            if(Mode == BehaviorMode.Init) {

                if(Systems.Targeting.InvalidTarget == true) {

                    Mode = BehaviorMode.WaitingForTarget;

                } else {

                    Mode = BehaviorMode.ApproachTarget;
                    Systems.AutoPilot.ChangeAutoPilotMode(AutoPilotMode.LegacyAutoPilot);

                }

            }

            if(Mode == BehaviorMode.WaitingForTarget) {

                if(Systems.AutoPilot.Mode != AutoPilotMode.None) {

                    Systems.AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);

                }

                if(Systems.Targeting.InvalidTarget == false) {

                    Mode = BehaviorMode.ApproachTarget;

                }

            }

            //Space Approach
            if(Mode == BehaviorMode.ApproachTarget && Systems.AutoPilot.UpDirection == Vector3D.Zero) {

                Systems.AutoPilot.GetWaypointFromTarget = false;
                var newCoords = VectorHelper.CreateDirectionAndTarget(Systems.AutoPilot.TargetCoords, RemoteControl.GetPosition(), Systems.AutoPilot.TargetCoords, this.FighterEngageDistanceSpace);

                if(Systems.Targeting.Target.TargetDistance < this.FighterEngageDistanceSpace) {

                    Mode = BehaviorMode.EngageTarget;

                }

            }

            //Gravity Approach
            if(Mode == BehaviorMode.ApproachTarget && Systems.AutoPilot.UpDirection != Vector3D.Zero) {

                Systems.AutoPilot.GetWaypointFromTarget = false;

                if(Systems.Targeting.Target.TargetDistance < this.FighterEngageDistancePlanet) {

                    Mode = BehaviorMode.EngageTarget;

                } else {



                }

            }

            //Space Engage
            if(Mode == BehaviorMode.EngageTarget && Systems.AutoPilot.UpDirection == Vector3D.Zero) {

                

            }

            //Planet Engage
            if(Mode == BehaviorMode.EngageTarget && Systems.AutoPilot.UpDirection != Vector3D.Zero) {



            }

            //Space Evade

            //Planet Evade

        }

    }

}
	
