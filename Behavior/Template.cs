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
	
	public class Template : CoreBehavior{

        public byte Counter10;
        public byte Counter30;
        public byte Counter60;

        public Template() {

            Counter10 = 0;
            Counter30 = 0;
            Counter60 = 0;

        }

        public void RunAi() {

            if(Systems.Owner.NpcOwned == false) {

                return;

            }

            RunCoreBehavior();

            Counter10++;
            Counter30++;
            Counter60++;

            if(Counter10 >= 10) {

                Counter10 = 0;
                RunBehavior10();

            }

            if(Counter30 >= 30) {

                Counter30 = 0;
                RunBehavior30();

            }

            if(Counter60 >= 60) {

                Counter60 = 0;
                RunBehavior60();

            }


        }

        public void MainBehavior() {



        }

        public void RunBehavior() {



        }

        public void RunBehavior10() {



        }

        public void RunBehavior30() {



        }

        public void RunBehavior60() {

            if(RAI_SessionCore.IsServer == true) {

                MainBehavior();

            }

        }

    }

}
	
