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
using RivalAI.Behavior;
using RivalAI.Behavior.Settings;
using RivalAI.Behavior.Subsystems;
using RivalAI.Helpers;
using RivalAI;

namespace RivalAI.Helpers {

    public class CollisionCheckResult {

        public bool HasTarget = false;
        public Vector3D Coords = new Vector3D(0,0,0);
        public double Distance = 0;
        public CollisionDetectType Type = CollisionDetectType.None;
        public IMyEntity Entity = null;

    }

    public enum CollisionDetectType {

        None,
        Voxel,
        Grid,
        SafeZone,
        DefenseShield

    }

    public static class TargetHelper{

		public static List<MyDefinitionId> ShieldBlockIDs = new List<MyDefinitionId>();
		
		public static Random Rnd = new Random();

        public static IMyTerminalBlock AcquireBlockTarget(IMyRemoteControl remoteControl, double maxDistance, TargetRelationEnum relationEnum, TargetDistanceEnum distanceEnum, TargetOwnerEnum ownerEnum, BlockTargetTypes blocksFilter, TargetFilterEnum filterEnum, long requestedBlockEntity = 0) {

            if(requestedBlockEntity != 0) {

                IMyEntity blockEntity = null;

                if(MyAPIGateway.Entities.TryGetEntityById(requestedBlockEntity, out blockEntity) == false) {

                    return null;

                }

                if((blockEntity as IMyTerminalBlock) != null) {

                    return blockEntity as IMyTerminalBlock;

                }

                return null;

            }

            var entityList = new HashSet<IMyEntity>();
            var gridList = new List<IMyEntity>();
            var blockList = new List<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entityList);

            MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(remoteControl.GetPosition());

            //Filter out non grid, max distance, relations
            foreach(var entity in entityList) {

                IMyCubeGrid cubeGrid = entity as IMyCubeGrid;

                if(cubeGrid == null) {

                    continue;

                }

                if(cubeGrid.Physics == null || cubeGrid.IsSameConstructAs(remoteControl.SlimBlock.CubeGrid)) {

                    continue;

                }

                if(Vector3D.Distance(remoteControl.GetPosition(), cubeGrid.GetPosition()) > maxDistance) {

                    continue;

                }

                if(filterEnum.HasFlag(TargetFilterEnum.IgnoreSafeZone) == true && TargetHelper.IsPositionInSafeZone(cubeGrid.PositionComp.WorldAABB.Center) == true) {

                    continue;

                }

                if(filterEnum.HasFlag(TargetFilterEnum.IsBroadcasting) == true && IsTargetBroadcasting(cubeGrid, remoteControl, true, true) == false) {

                    continue;

                }

                var gridBlocks = new List<IMySlimBlock>();
                cubeGrid.GetBlocks(gridBlocks, x => x.FatBlock != null);

                foreach(var block in gridBlocks) {

                    if(block as IMyTerminalBlock != null) {

                        blockList.Add(block as IMyEntity);

                    }

                }

            }

            //Relation & Owner
            for(int i = blockList.Count - 1; i >= 0; i--) {

                var block = blockList[i] as IMyTerminalBlock;

                if(block == null) {

                    blockList.RemoveAt(i);
                    continue;

                }

                if(filterEnum.HasFlag(TargetFilterEnum.IgnoreUnderground) && VectorHelper.IsPositionUnderground(block.GetPosition(), planet) == true) {

                    blockList.RemoveAt(i);
                    continue;

                }

                var relationResult = OwnershipHelper.GetTargetReputation(remoteControl.OwnerId, block);
                var ownerResults = OwnershipHelper.GetOwnershipTypes(block);

                if(OwnershipHelper.CompareAllowedReputation(relationEnum, relationResult) == false || OwnershipHelper.CompareAllowedOwnerTypes(ownerEnum, ownerResults) == false) {

                    blockList.RemoveAt(i);
                    continue;

                }

            }

            var filteredBlockList = FilterBlocksByFamily(blockList, blocksFilter);

            return TargetHelper.GetEntityAtDistance(remoteControl.GetPosition(), filteredBlockList, distanceEnum) as IMyTerminalBlock;

        }

        public static IMyCubeGrid AcquireGridTarget(IMyRemoteControl remoteControl, double maxDistance, TargetRelationEnum relationEnum, TargetDistanceEnum distanceEnum, TargetOwnerEnum ownerEnum, TargetFilterEnum filterEnum, long requestedGridEntity = 0) {

            if(requestedGridEntity != 0) {

                IMyEntity gridEntity = null;

                if(MyAPIGateway.Entities.TryGetEntityById(requestedGridEntity, out gridEntity) == false) {

                    return null;

                }

                if((gridEntity as IMyCubeGrid) != null) {

                    return gridEntity as IMyCubeGrid;

                }

                return null;

            }

            var entityList = new HashSet<IMyEntity>();
            var gridList = new List<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entityList);

            //Filter out non grid, max distance, relations
            foreach(var entity in entityList) {

                IMyCubeGrid cubeGrid = entity as IMyCubeGrid;

                if(cubeGrid == null) {

                    continue;

                }

                if(cubeGrid.Physics == null || cubeGrid.IsSameConstructAs(remoteControl.SlimBlock.CubeGrid)) {

                    continue;

                }

                if(Vector3D.Distance(remoteControl.GetPosition(), cubeGrid.GetPosition()) > maxDistance) {

                    continue;

                }

                if(filterEnum.HasFlag(TargetFilterEnum.IgnoreSafeZone) == true && TargetHelper.IsPositionInSafeZone(cubeGrid.PositionComp.WorldAABB.Center) == true) {

                    continue;

                }

                if(filterEnum.HasFlag(TargetFilterEnum.IsBroadcasting) == true && IsTargetBroadcasting(cubeGrid, remoteControl, true, true) == false) {

                    continue;

                }

                MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(remoteControl.GetPosition());

                if(filterEnum.HasFlag(TargetFilterEnum.IgnoreUnderground) == true) {

                    bool aboveSurface = false;

                    foreach(var corner in cubeGrid.PositionComp.WorldAABB.GetCorners()) {

                        if(VectorHelper.IsPositionUnderground(corner, planet) == false) {

                            aboveSurface = true;
                            break;

                        }

                    }

                    if(aboveSurface == false) {

                        continue;

                    }

                }

                bool includeSmallOwners = filterEnum.HasFlag(TargetFilterEnum.IncludeGridMinorityOwners);

                var relationResult = OwnershipHelper.GetTargetReputation(remoteControl.OwnerId, cubeGrid, includeSmallOwners);
                bool validRelation = OwnershipHelper.CompareAllowedReputation(relationEnum, relationResult);

                if(validRelation == false) {

                    continue;

                }

                var ownerResult = OwnershipHelper.GetOwnershipTypes(cubeGrid, includeSmallOwners);
                bool validOwner = OwnershipHelper.CompareAllowedOwnerTypes(ownerEnum, ownerResult);

                if(validOwner == false) {

                    continue;

                }

                gridList.Add(entity);

            }

            return TargetHelper.GetEntityAtDistance(remoteControl.GetPosition(), gridList, distanceEnum) as IMyCubeGrid;

        }

        public static IMyPlayer AcquirePlayerTarget(IMyRemoteControl remoteControl, double maxDistance, TargetRelationEnum relationEnum, TargetDistanceEnum distanceEnum, bool ignoreTargetsInSafezone = false) {

            var playerList = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(playerList);

            if(playerList.Count == 0) {

                return null;

            }

            //Filter Out By Relation First and Outside Max Distance
            for(int i = playerList.Count - 1;i >= 0;i--) {

                var player = playerList[1];

                //Ignore Non-Character and Bots
                if(player.IsBot == true || player.Character == null) {

                    playerList.RemoveAt(i);
                    continue;

                }

                if(Vector3D.Distance(remoteControl.GetPosition(), player.GetPosition()) > maxDistance) {

                    playerList.RemoveAt(i);
                    continue;

                }

                if(ignoreTargetsInSafezone == true && IsPositionInSafeZone(player.GetPosition()) == true) {

                    playerList.RemoveAt(i);
                    continue;

                }

                var relation = OwnershipHelper.GetReputation(remoteControl.OwnerId, player.IdentityId);

                //Valid Enemy
                if(relation < -500 && relationEnum == TargetRelationEnum.Enemy) {

                    continue;

                }

                //Valid Neutral
                if(relation >= -500 && relation <= 500 && relationEnum == TargetRelationEnum.Neutral) {

                    continue;

                }

                //Valid Friend
                if(relation > 500 && relationEnum == TargetRelationEnum.Friend) {

                    continue;

                }

                //Valid FactionMember
                if(OwnershipHelper.IsFactionMember(remoteControl.OwnerId, player.IdentityId) == true && relationEnum == TargetRelationEnum.Faction) {

                    continue;

                }

                //No Valid Player Relation
                playerList.RemoveAt(i);

            }

            if(playerList.Count == 0) {

                return null;

            } else if(playerList.Count == 1) {

                return playerList[0];

            }

            //Distance - Any
            if(distanceEnum == TargetDistanceEnum.Any) {


                return playerList[Rnd.Next(0, playerList.Count)];

            }

            IMyPlayer closestPlayer = null;
            double closestPlayerDistance = 0;

            //Distance - Closest
            if(distanceEnum == TargetDistanceEnum.Closest) {

                foreach(var player in playerList) {

                    var currentDist = Vector3D.Distance(remoteControl.GetPosition(), player.GetPosition());

                    if(closestPlayer == null || (closestPlayer != null && currentDist < closestPlayerDistance)) {

                        closestPlayer = player;
                        closestPlayerDistance = currentDist;

                    }

                }

                return closestPlayer;

            }

            //Distance - Furthest
            if(distanceEnum == TargetDistanceEnum.Furthest) {

                foreach(var player in playerList) {

                    var currentDist = Vector3D.Distance(remoteControl.GetPosition(), player.GetPosition());

                    if(closestPlayer == null || (closestPlayer != null && currentDist > closestPlayerDistance)) {

                        closestPlayer = player;
                        closestPlayerDistance = currentDist;

                    }

                }

                return closestPlayer;

            }

            return null;

        }

        //CheckCollisions
        public static CollisionCheckResult CheckCollisions(IMyTerminalBlock sourceBlock, Vector3D directionCheck, double distance, bool detectVoxel, bool detectGrid, bool detectSafeZone, bool detectShield, bool detectPlayer) {

            CollisionCheckResult result = new CollisionCheckResult();

            try {

                Vector3D remoteControlPosition = sourceBlock.GetPosition();
                Vector3D closestCollisionCoords = Vector3D.Zero;
                double closestCollisionDistance = 0;
                CollisionDetectType detectType = CollisionDetectType.None;
                IMyEntity closestEntity = null;

                if(detectVoxel) {

                    var collision = TargetHelper.VoxelIntersectionCheck(remoteControlPosition, directionCheck, distance);

                    if(collision != Vector3D.Zero) {

                        var thisCollisionDist = Vector3D.Distance(collision, remoteControlPosition);
                        closestCollisionCoords = collision;
                        closestCollisionDistance = thisCollisionDist;
                        detectType = CollisionDetectType.Voxel;

                    }

                }

                if(detectGrid == true) {

                    IMyCubeGrid targetGrid = null;
                    var collision = TargetHelper.TargetIntersectionCheck(sourceBlock, directionCheck, distance, out targetGrid);

                    if(targetGrid != null) {

                        var thisCollisionDist = Vector3D.Distance(collision, remoteControlPosition);

                        if(thisCollisionDist < closestCollisionDistance || closestCollisionCoords == Vector3D.Zero) {

                            closestCollisionCoords = collision;
                            closestCollisionDistance = thisCollisionDist;
                            detectType = CollisionDetectType.Grid;

                        }

                    }

                }

                if(detectSafeZone == true) {

                    var collision = TargetHelper.SafeZoneIntersectionCheck(remoteControlPosition, directionCheck, distance);

                    if(collision != Vector3D.Zero) {

                        var thisCollisionDist = Vector3D.Distance(collision, remoteControlPosition);

                        if(thisCollisionDist < closestCollisionDistance || closestCollisionCoords == Vector3D.Zero) {

                            closestCollisionCoords = collision;
                            closestCollisionDistance = thisCollisionDist;
                            detectType = CollisionDetectType.SafeZone;

                        }

                    }

                }

                if(detectShield == true) {

                    var collision = TargetHelper.ShieldIntersectionCheck(remoteControlPosition, directionCheck, distance);

                    if(collision != Vector3D.Zero) {

                        var thisCollisionDist = Vector3D.Distance(collision, remoteControlPosition);

                        if(thisCollisionDist < closestCollisionDistance || closestCollisionCoords == Vector3D.Zero) {

                            closestCollisionCoords = collision;
                            closestCollisionDistance = thisCollisionDist;
                            detectType = CollisionDetectType.DefenseShield;

                        }

                    }

                }

                //TODO: Player Check



                if(closestEntity != null) {

                    result.HasTarget = true;
                    result.Coords = closestCollisionCoords;
                    result.Distance = closestCollisionDistance;
                    result.Type = detectType;
                    result.Entity = closestEntity;

                }


                if(Logger.LoggerDebugMode == true) {

                    if(closestCollisionCoords != Vector3D.Zero) {

                        var color = Color.Red.ToVector4();
                        //MySimpleObjectDraw.DrawLine(remoteControlPosition, closestCollisionCoords, MyStringId.GetOrCompute("WeaponLaser"), ref color, 1);
                        MyVisualScriptLogicProvider.ShowNotificationToAll("Collision: " + detectType.ToString() + " / " + closestCollisionDistance.ToString(), 166);

                    }

                }

            } catch(Exception exc) {

                Logger.AddMsg("Caught Error In AI Parallel Collision Detection.");
                Logger.AddMsg(exc.ToString(), true);

            }

            return result;

        }

        //EvaluateTargetWeaponsRange
        public static float EvaluateTargetTurretRange(IMyCubeGrid cubeGrid){
			
			float furthestWeaponDistance = 0;
			
			try{
				
				var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid);
				var blockList = new List<IMyLargeTurretBase>();
				gts.GetBlocksOfType<IMyLargeTurretBase>(blockList);

				foreach(var turret in blockList){
					
					if(turret.IsFunctional == false){
						
						continue;
						
					}
					
					if(turret.GetInventory().Empty() == true){
						
						continue;
						
					}
					
					float range = turret.Range;
					
					if(range > furthestWeaponDistance){
						
						furthestWeaponDistance = range;
						
					}
					
				}
				
			}catch(Exception exc){
				
				Logger.AddMsg("Caught Error in EvaluateTargetWeaponsRange Method.");
				return 0;
				
			}
			
			return furthestWeaponDistance;
			
		}

        public static List<IMyEntity> FilterBlocksByFamily(List<IMyEntity> entityList, BlockTargetTypes family, bool replaceResultWithDecoys = false) {

            var blockList = new List<IMyEntity>();
            var decoyList = new List<IMyEntity>();

            if(family.HasFlag(BlockTargetTypes.All) == true) {

                return entityList;

            }

            for(int i = blockList.Count - 1; i >= 0; i--) {

                var block = blockList[i] as IMyTerminalBlock;

                if(block == null) {

                    blockList.RemoveAt(i);
                    continue;

                }

                //Decoys
                if(family.HasFlag(BlockTargetTypes.Decoys) == true) {

                    if(block as IMyDecoy != null) {

                        blockList.Add(block);
                        decoyList.Add(block);
                        continue;

                    }

                }

                //Shields
                if(family.HasFlag(BlockTargetTypes.Shields) == true) {

                    if(ShieldBlockIDs.Contains(block.SlimBlock.BlockDefinition.Id) == true) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //Containers
                if(family.HasFlag(BlockTargetTypes.Containers) == true) {

                    if(block as IMyCargoContainer != null) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //GravityBlocks
                if(family.HasFlag(BlockTargetTypes.GravityBlocks) == true) {

                    if(block as IMyGravityGeneratorBase != null || block as IMyArtificialMassBlock != null) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //Guns
                if(family.HasFlag(BlockTargetTypes.Guns) == true) {

                    if(block as IMyUserControllableGun != null && block as IMyLargeTurretBase == null) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //JumpDrive
                if(family.HasFlag(BlockTargetTypes.JumpDrive) == true) {

                    if(block as IMyJumpDrive != null) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //Power
                if(family.HasFlag(BlockTargetTypes.Power) == true) {

                    if(block as IMyPowerProducer != null) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //Production
                if(family.HasFlag(BlockTargetTypes.Production) == true) {

                    if(block as IMyProductionBlock != null || block as IMyGasGenerator != null) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //Propulsion
                if(family.HasFlag(BlockTargetTypes.Propulsion) == true) {

                    if(block as IMyThrust != null || block as IMyGyro != null) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //ShipControllers
                if(family.HasFlag(BlockTargetTypes.ShipControllers) == true) {

                    if(block as IMyShipController != null) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //Tools
                if(family.HasFlag(BlockTargetTypes.Tools) == true) {

                    if(block as IMyShipToolBase != null) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //Turrets
                if(family.HasFlag(BlockTargetTypes.Turrets) == true) {

                    if(block as IMyLargeTurretBase != null) {

                        blockList.Add(block);
                        continue;

                    }

                }

                //Communications
                if(family.HasFlag(BlockTargetTypes.Communications) == true) {

                    if(block as IMyRadioAntenna != null || block as IMyLaserAntenna != null) {

                        blockList.Add(block);
                        continue;

                    }

                }

            }

            if(replaceResultWithDecoys == true && decoyList.Count > 0) {

                return decoyList;

            } else {
                
                return blockList;

            }

        }

        //GetAllBlocks
        public static List<IMySlimBlock> GetAllBlocks(IMyCubeGrid cubeGrid) {

            List<IMySlimBlock> totalList = new List<IMySlimBlock>();
            cubeGrid.GetBlocks(totalList);
            var gridGroup = MyAPIGateway.GridGroups.GetGroup(cubeGrid, GridLinkTypeEnum.Physical);

            foreach(var grid in gridGroup) {

                List<IMySlimBlock> blockList = new List<IMySlimBlock>();
                cubeGrid.GetBlocks(blockList);
                blockList = new List<IMySlimBlock>(blockList.Except(totalList).ToList());
                totalList = new List<IMySlimBlock>(blockList.Concat(totalList).ToList());

            }

            return totalList;

        }

        //GetClosestPlayer
        public static IMyPlayer GetClosestPlayer(Vector3D coords){
			
			var activePlayers = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(activePlayers);
			IMyPlayer closestPlayer = null;
			double closestPlayerDistance = 0;
			
			foreach(var player in activePlayers){
				
				if(player.Character == null || player.IsBot == true){
					
					continue;
					
				}
				
				var distance = Vector3D.Distance(player.GetPosition(), coords);
				
				if(closestPlayer == null){
					
					closestPlayer = player;
					closestPlayerDistance = distance;
					continue;
					
				}
				
				if(distance < closestPlayerDistance){
					
					closestPlayer = player;
					closestPlayerDistance = distance;
					
				}
				
			}
			
			return closestPlayer;
			
		}

        public static IMyEntity GetEntityAtDistance(Vector3D coords, List<IMyEntity> entityList, TargetDistanceEnum distanceEnum) {

            IMyEntity result = null;
            double closestDistance = -1;

            if(distanceEnum == TargetDistanceEnum.Any && entityList.Count > 0) {

                return entityList[Rnd.Next(0, entityList.Count)];

            }

            if(distanceEnum == TargetDistanceEnum.Closest) {

                foreach(var entity in entityList) {

                    var distance = Vector3D.Distance(coords, entity.GetPosition());

                    if(closestDistance == -1 || distance < closestDistance) {

                        result = entity;
                        closestDistance = distance;

                    }

                }

            }

            if(distanceEnum == TargetDistanceEnum.Furthest) {

                foreach(var entity in entityList) {

                    var distance = Vector3D.Distance(coords, entity.GetPosition());

                    if(closestDistance == -1 || distance > closestDistance) {

                        result = entity;
                        closestDistance = distance;

                    }

                }

            }

            return result;

        }

        public static List<MySafeZone> GetSafeZones(){
			
			var entities = new HashSet<IMyEntity>();
			MyAPIGateway.Entities.GetEntities(entities);
			var safezoneList = new List<MySafeZone>();
			
			foreach(var zoneEntity in entities){
				
				var safezone = zoneEntity as MySafeZone;
				
				if(safezone == null){
					
					continue;
					
				}
				
				safezoneList.Add(safezone);
				
			}
			
			return safezoneList;
			
		}
		
		public static Vector2 GetTargetGridPower(IMyCubeGrid cubeGrid){
			
			Vector2 result = Vector2.Zero;
			float currentPower = 0;
			float maxPower = 0;
			
			try{
				
				var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid);
				List<IMyPowerProducer> blockList = new List<IMyPowerProducer>();
				gts.GetBlocksOfType<IMyPowerProducer>(blockList);
				
				foreach(var block in blockList){
					
					if(block.IsFunctional == true || block.IsWorking){
						
						currentPower += block.CurrentOutput;
						maxPower += block.MaxOutput;
						
					}
				
				}
				
			}catch(Exception exc){
				
				result.X = -1;
				result.Y = -1;
				return result;
				
			}
			
			result.X = currentPower;
			result.Y = maxPower;
			return result;
			
		}

		//GetTargetShipSystem
		public static void GetFilteredBlockLists(IMyCubeGrid targetGrid, BlockTargetTypes systemTarget, out List<IMyTerminalBlock> targetBlocksList, out List<IMyTerminalBlock> decoyList){
			
			decoyList = new List<IMyTerminalBlock>();
			targetBlocksList = new List<IMyTerminalBlock>();
			
			try{
				
				var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(targetGrid);
				List<IMyTerminalBlock> blockList = new List<IMyTerminalBlock>();
				gts.GetBlocksOfType<IMyTerminalBlock>(blockList);

				foreach(var block in blockList){
					
					if(block.IsFunctional == false){
						
						continue;
						
					}
					
					if(block as IMyDecoy != null){
						
						decoyList.Add(block);
						
						if(systemTarget == BlockTargetTypes.Decoys){
							
							targetBlocksList.Add(block);
							
						}

						continue;
						
					}
					
					if(systemTarget == BlockTargetTypes.All){
						
						targetBlocksList.Add(block);
						continue;
						
					}
					
					if(systemTarget == BlockTargetTypes.Communications){
						
						if(block as IMyLaserAntenna != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
						if(block as IMyBeacon != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}

						if(block as IMyRadioAntenna != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.Containers){
						
						if(block as IMyCargoContainer != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.GravityBlocks){
						
						if(block as IMyGravityGeneratorBase != null || block as IMyArtificialMassBlock != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.Guns){
						
						if(block as IMyUserControllableGun != null && block as IMyLargeTurretBase == null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.JumpDrive){
						
						if(block as IMyJumpDrive != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.Power){
						
						if(block as IMyPowerProducer != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.Production){
						
						if(block as IMyProductionBlock != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
						if(block as IMyGasGenerator != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.Propulsion){
						
						if(block as IMyThrust != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.Shields){
						
						if(ShieldBlockIDs.Contains(block.SlimBlock.BlockDefinition.Id) == true){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.ShipControllers){
						
						if(block as IMyShipController != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.Tools){
						
						if(block as IMyShipToolBase != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
					if(systemTarget == BlockTargetTypes.Turrets){
						
						if(block as IMyLargeTurretBase != null){
							
							targetBlocksList.Add(block);
							continue;
							
						}
						
					}
					
				}

			}catch(Exception exc){
				
				
				
			}

		}

        //IsGridPowered
        public static bool IsGridPowered(IMyCubeGrid cubeGrid) {

            if(cubeGrid == null) {

                return false;

            }

            var powerId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");
            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid);
            List<IMyTerminalBlock> blockList = new List<IMyTerminalBlock>();
            gts.GetBlocksOfType<IMyTerminalBlock>(blockList);

            foreach(var block in blockList) {

                var sink = block.Components.Get<MyResourceSinkComponent>();

                if(sink == null) {

                    continue;

                }

                return sink.IsPowerAvailable(powerId, 0.001f);

            }

            return false;

        }

		//IsHumanControllingTarget
		public static bool IsHumanControllingTarget(IMyCubeGrid cubeGrid){

            if(cubeGrid == null) {

                return false;

            }

			var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid);
			List<IMyShipController> blockList = new List<IMyShipController>();
			gts.GetBlocksOfType<IMyShipController>(blockList);
			
			foreach(var cockpit in blockList){
				
				if(cockpit.Pilot != null && cockpit.CanControlShip == true){
					
					return true;
					
				}
				
			}
			
			return false;
			
		}

        public static bool IsPositionInGravity(Vector3D position, MyPlanet planet) {

            if(planet == null) {

                return false;

            }

            var planetEntity = planet as IMyEntity;
            var gravityProvider = planetEntity.Components.Get<MyGravityProviderComponent>();

            if(gravityProvider == null) {

                return false;

            }

            return gravityProvider.IsPositionInRange(position);

        }

        //IsPositionInSafeZone
        public static bool IsPositionInSafeZone(Vector3D position){
			
			var zones = GetSafeZones();
			
			foreach(var safezone in zones){
				
				var zoneEntity = safezone as IMyEntity;
				
				if(zoneEntity == null){
					
					continue;
					
				}

				var checkPosition = position;
				bool inZone = false;
				
				if (safezone.Shape == MySafeZoneShape.Sphere){
					
					if(zoneEntity.PositionComp.WorldVolume.Contains(checkPosition) == ContainmentType.Contains){
						
						inZone = true;
						
					}
					
				}else{
					
					MyOrientedBoundingBoxD myOrientedBoundingBoxD = new MyOrientedBoundingBoxD(zoneEntity.PositionComp.LocalAABB, zoneEntity.PositionComp.WorldMatrix);
					inZone = myOrientedBoundingBoxD.Contains(ref checkPosition);
				
				}
				
				if(inZone == true){
					
					return true;
					
				}
				
			}
			
			return false;
			
		}

        //IsTargetBroadcasting
        public static bool IsTargetBroadcasting(IMyCubeGrid cubeGrid, IMyRemoteControl sourceBlock, bool checkAntennas, bool checkBeacons){
			
			var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid);
			
			if(checkAntennas == true){
				
				List<IMyRadioAntenna> antennaList = new List<IMyRadioAntenna>();
				gts.GetBlocksOfType<IMyRadioAntenna>(antennaList);
				
				foreach(var antenna in antennaList){
					
					if(antenna.IsWorking == true && antenna.IsFunctional == true && antenna.IsBroadcasting == true){
						
						var distToNPC = (float)Vector3D.Distance(sourceBlock.GetPosition(), antenna.GetPosition());
						
						if(antenna.Radius >= distToNPC){
							
							return true;
							
						}
						
					}
					
				}
				
			}
			
			if(checkBeacons == true){
				
				List<IMyBeacon> beaconList = new List<IMyBeacon>();
				gts.GetBlocksOfType<IMyBeacon>(beaconList);
				
				foreach(var beacon in beaconList){
					
					if(beacon.IsWorking == true && beacon.IsFunctional == true){
						
						var distToNPC = (float)Vector3D.Distance(sourceBlock.GetPosition(), beacon.GetPosition());
						
						if(beacon.Radius >= distToNPC){
							
							return true;
							
						}
						
					}
					
				}
				
			}
			
			return false;
			
		}

        //IsTargetFaction
        public static bool IsTargetFaction(long myOwnerId, IMyEntity targetEntity) {

            var myFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(myOwnerId);

            if(myFaction == null) {

                return false;

            }

            long targetId = 0;

            if(targetEntity as IMyCubeGrid != null) {

                var cubeGrid = targetEntity as IMyCubeGrid;

                if(cubeGrid.BigOwners.Count > 0) {

                    targetId = cubeGrid.BigOwners[0];

                }

            } else if(targetEntity as IMyCubeBlock != null) {

                var cubeBlock = targetEntity as IMyCubeBlock;
                targetId = cubeBlock.OwnerId;

            }

            if(targetId == 0) {

                return false;

            }

            var otherFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(targetId);

            if(otherFaction == null) {

                return false;

            }

            if(myFaction.FactionId == otherFaction.FactionId) {

                return true;

            }

            return false;

        }

        //IsTargetNPC
        public static bool IsTargetNPC(IMyEntity targetEntity) {

            long targetId = 0;

            if(targetEntity as IMyCubeGrid != null) {

                var cubeGrid = targetEntity as IMyCubeGrid;

                if(cubeGrid.BigOwners.Count > 0) {

                    targetId = cubeGrid.BigOwners[0];

                }

            } else if(targetEntity as IMyCubeBlock != null) {

                var cubeBlock = targetEntity as IMyCubeBlock;
                targetId = cubeBlock.OwnerId;

            }

            if(targetId == 0) {

                return false;

            }

            var npcSteamId = MyAPIGateway.Players.TryGetSteamId(targetId);

            if(npcSteamId == 0) {

                return true;

            }

            return false;

        }

        //IsTargetNeutral
        public static bool IsTargetNeutral(long myOwnerId, IMyEntity targetEntity) {

            var myFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(myOwnerId);

            if(myFaction == null) {

                return false;

            }

            long targetId = 0;

            if(targetEntity as IMyCubeGrid != null) {

                var cubeGrid = targetEntity as IMyCubeGrid;

                if(cubeGrid.BigOwners.Count > 0) {

                    targetId = cubeGrid.BigOwners[0];

                }

            } else if(targetEntity as IMyCubeBlock != null) {

                var cubeBlock = targetEntity as IMyCubeBlock;
                targetId = cubeBlock.OwnerId;

            }

            if(targetId == 0) {

                return false;

            }

            var otherFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(targetId);

            if(otherFaction == null) {

                return false;

            }

            if(MyAPIGateway.Session.Factions.AreFactionsEnemies(myFaction.FactionId, otherFaction.FactionId) == false) {

                return true;

            }

            return false;

        }

        //IsTargetOwned
        public static bool IsTargetOwned(long myOwnerId, IMyEntity targetEntity) {

            long targetId = 0;

            if(targetEntity as IMyCubeGrid != null) {

                var cubeGrid = targetEntity as IMyCubeGrid;

                if(cubeGrid.BigOwners.Count > 0) {

                    targetId = cubeGrid.BigOwners[0];

                }

            } else if(targetEntity as IMyCubeBlock != null) {

                var cubeBlock = targetEntity as IMyCubeBlock;
                targetId = cubeBlock.OwnerId;

            }

            if(targetId == 0) {

                return false;

            }

            if(targetId == myOwnerId) {

                return true;

            }

            return false;

        }

        //IsTargetPlayer
        public static bool IsTargetPlayer(IMyEntity targetEntity) {

            long targetId = 0;

            if(targetEntity as IMyCubeGrid != null) {

                var cubeGrid = targetEntity as IMyCubeGrid;

                if(cubeGrid.BigOwners.Count > 0) {

                    targetId = cubeGrid.BigOwners[0];

                }

            } else if(targetEntity as IMyCubeBlock != null) {

                var cubeBlock = targetEntity as IMyCubeBlock;
                targetId = cubeBlock.OwnerId;

            }

            if(targetId == 0) {

                return false;

            }

            var npcSteamId = MyAPIGateway.Players.TryGetSteamId(targetId);

            if(npcSteamId != 0) {

                return true;

            }

            return false;

        }

        //IsTargetUnowned
        public static bool IsTargetUnowned(IMyEntity targetEntity) {

            long targetId = 0;

            if(targetEntity as IMyCubeGrid != null) {

                var cubeGrid = targetEntity as IMyCubeGrid;

                if(cubeGrid.BigOwners.Count > 0) {

                    targetId = cubeGrid.BigOwners[0];

                }

            } else if(targetEntity as IMyCubeBlock != null) {

                var cubeBlock = targetEntity as IMyCubeBlock;
                targetId = cubeBlock.OwnerId;

            }

            if(targetId == 0) {

                return true;

            }

            return false;

        }

        public static bool IsTargetUsingDefenseShield(IMyCubeGrid targetGrid){

			if(RAI_SessionCore.Instance.ShieldApiLoaded){
				
				var api = RAI_SessionCore.Instance.SApi;
				
				if(api.GridHasShield(targetGrid) == true && api.GridShieldOnline(targetGrid) == true){
					
					return true;
					
				}
				
			}
			
			return false;
			
		}
		
		public static Vector3D SafeZoneIntersectionCheck(Vector3D fromCoords, Vector3D direction, double distance){
			
			var safezoneList = GetSafeZones();
			double closestDistance = -1;
			var ray = new RayD(fromCoords, direction);
			
			foreach(var zone in safezoneList){

				var zoneEntity = zone as IMyEntity;
				
				if(zoneEntity == null){
					
					continue;
					
				}
				
				if(zone.Shape == MySafeZoneShape.Sphere){
					
					var result = ray.Intersects(zoneEntity.PositionComp.WorldVolume);
					
					if(result.HasValue == true){
						
						if((double)result <= distance){
							
							if((double)result < closestDistance || closestDistance == -1){
								
								closestDistance = (double)result;
								
							}
							
						}
						
					}
					
				}else{
					
					var result = ray.Intersects(zoneEntity.PositionComp.WorldAABB);
					
					if(result.HasValue == true){
						
						if((double)result <= distance){
							
							if((double)result < closestDistance || closestDistance == -1){
								
								closestDistance = (double)result;
								
							}
							
						}
						
					}
					
				}
				
			}
			
			if(closestDistance == -1){
				
				return Vector3D.Zero;
				
			}
			
			return closestDistance * direction + fromCoords;
			
		}
		
		public static Vector3D ShieldIntersectionCheck(Vector3D fromCoords, Vector3D direction, double distance, long ownerId = 0){
			
			if(RAI_SessionCore.Instance.ShieldApiLoaded){
				
				var api = RAI_SessionCore.Instance.SApi;
				var toCoords = direction * distance + fromCoords;
				var line = new LineD(fromCoords, toCoords);
				var result = api.ClosestShieldInLine(line, true);
				
				if(result.Item1.HasValue){
							
					var dist = (double)result.Item1.Value;

                    if(ownerId != 0) {

                        var shield = result.Item2;
                        var reputation = OwnershipHelper.GetTargetReputation(ownerId, shield);

                        if(reputation.HasFlag(TargetRelationEnum.Enemy) == false) {

                            return Vector3D.Zero;

                        }

                    }

                    return fromCoords + (line.Direction * dist);

                }
				
			}
			
			return Vector3D.Zero;
			
		}
		
		public static Vector3D TargetIntersectionCheck(IMyTerminalBlock sourceBlock, Vector3D direction, double distance, out IMyCubeGrid closestGrid){
			
			var resultList = new List<MyLineSegmentOverlapResult<MyEntity>>();
			var fromCoords = sourceBlock.GetPosition();
			var toCoords = direction * distance + fromCoords;
			var ray = new LineD(fromCoords, toCoords);
			MyGamePruningStructure.GetTopmostEntitiesOverlappingRay(ref ray, resultList);
			
			closestGrid = null;
			double closestDistance = 0;
			
			foreach(var item in resultList){
				
				var targetGrid = item.Element as IMyCubeGrid;
				
				if(targetGrid == null){
					
					continue;
					
				}
				
				//TODO: Validate Same Construct Thing
				if(targetGrid == sourceBlock.SlimBlock.CubeGrid /*|| sourceBlock.SlimBlock.CubeGrid.IsSameConstructAs(targetGrid) == true*/){
					
					continue;
					
				}
				
				if(closestGrid == null || (closestGrid != null && item.Distance < closestDistance)){
					
					closestGrid = targetGrid;
					closestDistance = item.Distance;
					
				}
	
			}
			
			if(closestDistance == 0){
				
				return Vector3D.Zero;
				
			}

            var blockPos = closestGrid.RayCastBlocks(fromCoords, toCoords);

            if(blockPos.HasValue == false) {

                closestGrid = null;
                return Vector3D.Zero;

            }

            IMySlimBlock slimBlock = closestGrid.GetCubeBlock(blockPos.Value);

            if(slimBlock == null) {

                closestGrid = null;
                return Vector3D.Zero;

            }

            Vector3D blockPosition = Vector3D.Zero;
            slimBlock.ComputeWorldCenter(out blockPosition);

            return blockPosition;
			
		}
		
		public static Vector3D VoxelIntersectionCheck(Vector3D startScan, Vector3D scanDirection, double distance){
			
			var voxelFrom = startScan;
			var voxelTo = scanDirection * distance + voxelFrom;
			var line = new LineD(voxelFrom, voxelTo);
			
			List<IMyVoxelBase> nearbyVoxels = new List<IMyVoxelBase>();
			MyAPIGateway.Session.VoxelMaps.GetInstances(nearbyVoxels);
			Vector3D closestDistance = Vector3D.Zero;
			
			foreach(var voxel in nearbyVoxels){
				
				if(Vector3D.Distance(voxel.GetPosition(), voxelFrom) > 120000){
					
					continue;
					
				}
				
				var voxelBase = voxel as MyVoxelBase;
				Vector3D? nearestHit = null;
				
				if(voxelBase.GetIntersectionWithLine(ref line, out nearestHit) == true){
					
					if(nearestHit.HasValue == true){
						
						if(closestDistance == Vector3D.Zero){
							
							closestDistance = (Vector3D)nearestHit;
							continue;
							
						}
						
						if(Vector3D.Distance(voxelFrom, (Vector3D)nearestHit) < Vector3D.Distance(voxelFrom, closestDistance)){
							
							closestDistance = (Vector3D)nearestHit;
							
						}
						
					}
					
				}

			}
			
			return closestDistance;
			
		}

	}
	
}