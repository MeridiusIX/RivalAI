using RivalAI.Behavior;
using System;
using VRage.ModAPI;
using VRageMath;

namespace RivalAI.Helpers {

    public enum CommandType {
    
        DroneAntenna,
        PlayerChat,
        
    }

    public class Command {

        public string CommandCode;

        public CommandType Type;

        public IMyEntity RemoteControl;

        public IMyEntity Character;

        public IMyEntity SenderEntity { get { return (RemoteControl != null ? RemoteControl : Character); } }

        public double Radius;

        public bool IgnoreAntennaRequirement;

        public long TargetEntityId;

        public IMyEntity TargetEntity;

        public float DamageAmount;

        public Vector3D Position;

        public long PlayerIdentity;

        public bool UseTriggerTargetDistance;

        public long DamagerEntityId;

        public bool SingleRecipient;

        public long Recipient;

        public EncounterWaypoint Waypoint;


        public Command() {

            CommandCode = "";
            Type = CommandType.DroneAntenna;
            RemoteControl = null;
            Character = null;
            Radius = 0;
            IgnoreAntennaRequirement = false;
            TargetEntityId = 0;
            TargetEntity = null;
            Position = Vector3D.Zero;
            PlayerIdentity = 0;
            UseTriggerTargetDistance = false;
            SingleRecipient = false;
            Recipient = 0;
            Waypoint = null;

        }
        
    }

    public static class CommandHelper {

        public static Action<Command> CommandTrigger;
    
    }
    
}
