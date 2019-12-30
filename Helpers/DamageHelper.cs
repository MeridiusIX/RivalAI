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

namespace RivalAI.Helpers {

	public static class DamageHelper {

		public static HashSet<IMyCubeGrid> MonitoredGrids = new HashSet<IMyCubeGrid>();
		public static Dictionary<IMyCubeGrid, Action<object, MyDamageInformation>> RegisteredDamageHandlers = new Dictionary<IMyCubeGrid, Action<object, MyDamageInformation>>();

		public static void DamageHandler(object target, MyDamageInformation info) {

			var block = target as IMySlimBlock;

			if(block == null) {

				return;

			}

			var grid = block.CubeGrid;

			if(MonitoredGrids.Contains(grid)) {

				Action<object, MyDamageInformation> action = null;

				if(RegisteredDamageHandlers.TryGetValue(grid, out action)) {

					action?.Invoke(target, info);
					return;

				}

			}

		}

		public static void ApplyDamageToTarget(long entityId, float amount, string particleEffect, string soundEffect) {

			if(entityId == 0)
				return;

			IMyEntity entity = null;

			if(MyAPIGateway.Entities.TryGetEntityById(entityId, out entity) == false)
				return;

			if(entity as IMyCubeGrid != null)
				return;

			var tool = entity as IMyEngineerToolBase;
			var block = entity as IMyShipToolBase;
			bool didDamage = false;

			if(tool != null) {

				IMyEntity characterEntity = null;

				if(MyAPIGateway.Entities.TryGetEntityById(tool.OwnerId, out characterEntity)) {

					var character = characterEntity as IMyCharacter;

					if(character != null) {

						character.DoDamage(amount, MyStringHash.GetOrCompute("Electrocution"), true);
						didDamage = true;

					}

				}

			}

			if(block != null) {

				block.SlimBlock.DoDamage(amount, MyStringHash.GetOrCompute("Electrocution"), true);
				didDamage = true;

			}

			if(didDamage == false)
				return;



		}
		
		public static long GetAttackOwnerId(long attackingEntity){
			
			IMyEntity entity = null;
			
			if(!MyAPIGateway.Entities.TryGetEntityById(attackingEntity, out entity))
				return 0;
			
			var handGun = entity as IMyGunBaseUser;
			var handTool = entity as IMyEngineerToolBase;
			
			if(handGun != null){
				
				return handGun.OwnerId;
				
			}
			
			if(handTool != null){
				
				return handTool.OwnerIdentityId;
				
			}
			
			var cubeGrid = entity as IMyCubeGrid;
			var block = entity as IMyCubeBlock;
			
			if(block != null){
				
				cubeGrid = block.SlimBlock.CubeGrid;
				
			}
			
			if(cubeGrid == null)
				return 0;
			
			var shipControllers = BlockHelper.GetGridControllers(cubeGrid);
			
			IMyPlayer controlPlayer = null;

			foreach(var controller in shipControllers){
				
				var player = MyAPIGateway.Players.GetPlayerControllingEntity(controller);
				
				if(player == null)
					continue;
				
				controlPlayer = player;

				if (controller.IsMainCockpit || (controller.CanControlShip && controller.IsUnderControl))
					break;
				
			}
			
			long owner = 0;
			
			if(controlPlayer != null){
				
				owner = controlPlayer.IdentityId;
				
			}else{
				
				if(cubeGrid.BigOwners.Count > 0)
					owner = cubeGrid.BigOwners[0];
				
			}
			
			return owner;
			
		}

	}

}
