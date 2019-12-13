using System;
using System.Collections.Generic;
using System.Text;

namespace RivalAI.Helpers {

    public enum AutoPilotMode {

        None, //No Movement Applied
        BarrelRoll,
        LegacyAutoPilotTarget, //Uses Vanilla Remote Control Autopilot.
        LegacyAutoPilotWaypoint, //Uses Vanilla Remote Control Autopilot.
        FlyToWaypoint,
        FlyToTarget,
        RotateToTarget, //Applies Gyro Rotation To Face Target. No Thrust.
        RotateToWaypoint, //Applies Gyro Rotation To Face Waypoint. No Thrust.
        RotateToTargetAndStrafe //Applies Gyro Rotation To Face Target. Random Thruster Strafing Included.

    }

    public enum BehaviorMode {

        ApproachTarget,
        ApproachWaypoint,
        BarrelRoll,
        EngageTarget,
        EvadeCollision,
        Idle,
        Init,
        KamikazeCollision,
        Retreat,
        WaitAtWaypoint,
        WaitingForTarget,

    }

    [Flags]
    public enum BlockTargetTypes {

        None = 0,
        All = 1,
        Containers = 2,
        Decoys = 4,
        GravityBlocks = 8,
        Guns = 16,
        JumpDrive = 32,
        Power = 64,
        Production = 128,
        Propulsion = 256,
        Shields = 512,
        ShipControllers = 1024,
        Tools = 2048,
        Turrets = 4096,
        Communications = 8192

    }

    [Flags]
    public enum BroadcastType {

        None = 0,
        Chat = 1,
        Notify = 2,
        Both = 4

    }

    public enum ChatType {

        None,
        Greeting,
        Taunt,
        Retreat,
        Damage,
        Grind,
        TurretTarget,
        Spawning,
        SafeZone,

    }

    public enum CollisionDetectType {

        None,
        Voxel,
        Grid,
        SafeZone,
        DefenseShield

    }

    public enum TargetTypeEnum {

        None,
        Coords,
        Player,
        Entity,
        Grid,
        Block

    }

    public enum TargetDistanceEnum {

        Any,
        Closest,
        Furthest

    }

    [Flags]
    public enum TargetFilterEnum {

        None = 0,
        IgnoreSafeZone = 1,
        IsBroadcasting = 2,
        IgnoreUnderground = 4,
        IncludeGridMinorityOwners = 8,


    }

    public enum TargetObstructionEnum {

        None,
        Voxel,
        Safezone,
        DefenseShield,
        Grid,

    }

    [Flags]
    public enum TargetRelationEnum {

        None = 0,
        Faction = 1,
        Neutral = 2,
        Enemy = 4,
        Friend = 8,
        Unowned = 16

    }

    [Flags]
    public enum TargetOwnerEnum {

        None = 0,
        Unowned = 1,
        Owned = 2,
        Player = 4,
        NPC = 8,
        All = 16

    }


    [Flags]
    public enum TriggerAction {

        None = 0,
        BarrelRoll = 1 << 0,
        ChatBroadcast = 1 << 1,
        Retreat = 1 << 2,
        SelfDestruct = 1 << 3,
        SpawnReinforcements = 1 << 4,
        Strafe = 1 << 5,
        SwitchToTarget = 1 << 6,
        SwitchToBehavior = 1 << 7,
        TriggerTimerBlock = 1 << 8,
        FindNewTarget = 1 << 9,
        ActivateAssertiveAntennas = 1 << 10,

    }

    [Flags]
    public enum TriggerType {

        None = 0,
        Damage = 1 << 0,
        PlayerNear = 1 << 1, 
        TurretTarget = 1 << 2,
        NoWeapon = 1 << 3,
        TargetInSafezone = 1 << 4,
        Grounded = 1 << 5,
        CommandReceive = 1 << 6,
        Timer = 1 << 7,

    }



}
