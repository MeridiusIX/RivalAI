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
using Sandbox.Game.Weapons;
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
    public class OwnershipHelper {


        public static bool CompareAllowedOwnerTypes(TargetOwnerEnum allowedOwner, TargetOwnerEnum resultOwner) {

            //Owner: Unowned
            if(allowedOwner.HasFlag(TargetOwnerEnum.Unowned) && resultOwner.HasFlag(TargetOwnerEnum.Unowned)) {

                return true;

            }

            //Owner: Owned
            if(allowedOwner.HasFlag(TargetOwnerEnum.Owned) && resultOwner.HasFlag(TargetOwnerEnum.Owned)) {

                return true;

            }

            //Owner: Player
            if(allowedOwner.HasFlag(TargetOwnerEnum.Player) && resultOwner.HasFlag(TargetOwnerEnum.Player)) {

                return true;

            }

            //Owner: NPC
            if(allowedOwner.HasFlag(TargetOwnerEnum.NPC) && resultOwner.HasFlag(TargetOwnerEnum.NPC)) {

                return true;

            }

            return false;

        }

        public static bool CompareAllowedReputation(TargetRelationEnum allowedRelations, TargetRelationEnum resultRelation) {

            if(allowedRelations.HasFlag(TargetRelationEnum.Faction) && resultRelation.HasFlag(TargetRelationEnum.Faction)) {

                return true;

            }

            //Relation: Neutral
            if(allowedRelations.HasFlag(TargetRelationEnum.Neutral) && resultRelation.HasFlag(TargetRelationEnum.Neutral)) {

                return true;

            }

            //Relation: Enemy
            if(allowedRelations.HasFlag(TargetRelationEnum.Enemy) && resultRelation.HasFlag(TargetRelationEnum.Enemy)) {

                return true;

            }

            //Relation: Friend
            if(allowedRelations.HasFlag(TargetRelationEnum.Friend) && resultRelation.HasFlag(TargetRelationEnum.Friend)) {

                return true;

            }

            //Relation: Unowned
            if(allowedRelations.HasFlag(TargetRelationEnum.Unowned) && resultRelation.HasFlag(TargetRelationEnum.Unowned)) {

                return true;

            }

            return false;

        }

        public static bool DoesGridHaveHostileOwnership(IMyCubeGrid targetGrid, long myIdentity, bool includeNpcOwnership = false) {

            var gridGroup = MyAPIGateway.GridGroups.GetGroup(targetGrid, GridLinkTypeEnum.Logical);
            var ownerList = new List<long>();

            foreach(var grid in gridGroup) {

                var tempList = new List<long>(grid.BigOwners.ToList());
                tempList = tempList.Concat(grid.SmallOwners).ToList();
                var resultList = tempList.Except(ownerList).ToList();
                ownerList = ownerList.Concat(resultList).ToList();

            }

            foreach(var owner in ownerList) {

                if(owner == 0) {

                    continue;

                }

                if(includeNpcOwnership == false && IsNPC(owner) == true) {

                    continue;

                }

                if(GetReputation(myIdentity, owner) < -500) {

                    return true;

                }

            }

            return false;

        }

        public static TargetOwnerEnum GetOwnershipTypes(IMyCubeGrid cubeGrid, bool includeSmallOwners) {

            if(cubeGrid.BigOwners.Count == 0) {

                return GetOwnershipTypes(new List<long> { 0 });

            }

            var ownerList = new List<long>(cubeGrid.BigOwners.ToList());

            if(includeSmallOwners == true) {

                ownerList = ownerList.Concat(cubeGrid.SmallOwners.ToList()).ToList();

            }

            return GetOwnershipTypes(ownerList);

        }

        public static TargetOwnerEnum GetOwnershipTypes(IMyTerminalBlock block) {

            return GetOwnershipTypes(new List<long> { 0 });

        }

        private static TargetOwnerEnum GetOwnershipTypes(List<long> identities) {

            TargetOwnerEnum result = 0;

            foreach(var identity in identities) {

                if(identity == 0 && result.HasFlag(TargetOwnerEnum.Unowned) == false) {

                    result |= TargetOwnerEnum.Unowned;

                }

                if(IsNPC(identity) == true && result.HasFlag(TargetOwnerEnum.NPC) == false) {

                    result |= TargetOwnerEnum.NPC;

                }

                if(IsNPC(identity) == false && result.HasFlag(TargetOwnerEnum.Player) == false) {

                    result |= TargetOwnerEnum.Player;

                }

            }

            return result;

        }

        public static TargetRelationEnum GetTargetReputation(long myIdentity, IMyTerminalBlock block) {

            if(block.OwnerId == 0) {

                return GetTargetReputation(myIdentity, new List<long> { 0 });

            }

            return GetTargetReputation(myIdentity, new List<long> { block.OwnerId });

        }

        public static TargetRelationEnum GetTargetReputation(long myIdentity, IMyCubeGrid cubeGrid, bool includeSmallOwners = false) {

            if(cubeGrid.BigOwners.Count == 0) {

                return GetTargetReputation(myIdentity, new List<long> { 0 });

            }

            var ownerList = new List<long>(cubeGrid.BigOwners.ToList());

            if(includeSmallOwners == true) {

                ownerList = ownerList.Concat(cubeGrid.SmallOwners.ToList()).ToList();

            }

            return GetTargetReputation(myIdentity, ownerList);

        }

        private static TargetRelationEnum GetTargetReputation(long myIdentity, List<long> identities) {

            var myFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(myIdentity);

            if(myFaction == null) {

                return TargetRelationEnum.None;

            }

            var result = TargetRelationEnum.None;

            if(identities.Count == 0) {

                result |= TargetRelationEnum.Unowned;

            }

            foreach(var identity in identities) {

                if(myFaction.IsMember(myIdentity) == true && result.HasFlag(TargetRelationEnum.Faction) == false) {

                    result |= TargetRelationEnum.Faction;
                    continue;

                }

                var repScore = MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(identity, myFaction.FactionId);

                if(repScore < -500 && result.HasFlag(TargetRelationEnum.Enemy) == false) {

                    result |= TargetRelationEnum.Enemy;
                    continue;

                }

                if(repScore >= -500 && repScore <= 500 && result.HasFlag(TargetRelationEnum.Neutral) == false) {

                    result |= TargetRelationEnum.Neutral;
                    continue;

                }

                if(repScore > 500 && result.HasFlag(TargetRelationEnum.Friend) == false) {

                    result |= TargetRelationEnum.Friend;
                    continue;

                }

            }

            return result;

        }

        public static int GetReputation(long myIdentity, long theirIdentity) {

            var myFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(myIdentity);

            if(myFaction == null) {

                return -1500;

            }

            if(myFaction.IsMember(myIdentity) == true || theirIdentity == 0) {

                return 0;

            }

            return MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(theirIdentity, myFaction.FactionId);

        }

        public static bool IsFactionMember(long myIdentity, long theirIdentity) {

            var myFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(myIdentity);

            if(myFaction == null) {

                return false;

            }

            return myFaction.IsMember(theirIdentity);

        }

        public static bool IsNPC(long identity) {

            if(MyAPIGateway.Players.TryGetSteamId(identity) > 0 || identity == 0) {

                return false;

            }

            return true;

        }

    }

}
