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

namespace RivalAI.Behavior {

    public class Strike:CoreBehavior {

        //Configurable
        public double StrikePlanetTargetInitialAltitude;
        public double StrikePlanetEngageFromInitialDistance;
        public double StrikeBreakawayDistnace;
        public int StrikeMaximumAttackRuns;

        public Vector3D PlanetInitalTargetCoords;
        public Vector3D PlanetApproachCoords;
        public int CurrentAttackRuns;

        public bool ReceivedEvadeSignal;
        public bool ReceivedRetreatSignal;
        public bool ReceivedExternalTarget;

        public byte Counter;

        public Strike() {

            StrikePlanetTargetInitialAltitude = 1200;
            StrikePlanetEngageFromInitialDistance = 100;
            StrikeBreakawayDistnace = 450;
            StrikeMaximumAttackRuns = 10;

            PlanetInitalTargetCoords = Vector3D.Zero;
            PlanetApproachCoords = Vector3D.Zero;
            CurrentAttackRuns = 0;

            ReceivedEvadeSignal = false;
            ReceivedRetreatSignal = false;
            ReceivedExternalTarget = false;

            Counter = 0;

        }

        public void RunAi() {

            if(!IsAIReady())
                return;

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


        }

        public void CheckTarget() {
            /*
            if(Targeting.InvalidTarget == true) {

                Mode = BehaviorMode.WaitingForTarget;
                AutoPilot.ChangeAutoPilotMode(AutoPilotMode.None);

            }
            */
        }

        public void BehaviorInit(IMyRemoteControl remoteControl) {

            //Core Setup
            CoreSetup(remoteControl);

            //Behavior Specific Defaults
            Despawn.UseNoTargetTimer = true;
            NewAutoPilot.Targeting.NeedsTarget = true;
            NewAutoPilot.Weapons.UseStaticGuns = true;
            NewAutoPilot.Collision.CollisionTimeTrigger = 5;

            //Get Settings From Custom Data
            InitCoreTags();

            //Behavior Specific Default Enums (If None is Not Acceptable)
            if(NewAutoPilot.Targeting.TargetType == TargetTypeEnum.None) {

                NewAutoPilot.Targeting.TargetType = TargetTypeEnum.Player;

            }

            if(NewAutoPilot.Targeting.TargetRelation == TargetRelationEnum.None) {

                NewAutoPilot.Targeting.TargetRelation = TargetRelationEnum.Enemy;

            }

            if(NewAutoPilot.Targeting.TargetOwner == TargetOwnerEnum.None) {

                NewAutoPilot.Targeting.TargetOwner = TargetOwnerEnum.Player;

            }

        }

        public void InitTags() {

            //Core Tags


            //Behavior Tags


        }

    }

}

