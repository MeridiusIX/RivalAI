using ProtoBuf;
using Sandbox.ModAPI;

namespace RivalAI.Sync {

    public enum SyncMode {

        None,
        BehaviorChange,
        Effect,
        ChatCommand,
        ReputationAlert,

    }

    [ProtoContract]
    public class SyncContainer {

        [ProtoMember(1)]
        public SyncMode Mode;

        [ProtoMember(2)]
        public byte[] Data;

        [ProtoMember(3)]
        public string Sender;

        public SyncContainer() {

            Mode = SyncMode.None;
            Data = new byte[0];
            Sender = "RAI";

        }

        public SyncContainer(SyncMode mode, byte[] data) {

            Mode = mode;
            Data = data;
            Sender = "RAI";

        }
        
        public SyncContainer(Effects effect){
            
            this.Mode = SyncMode.Effect;
            this.Data = MyAPIGateway.Utilities.SerializeToBinary<Effects>(effect);
            this.Sender = "RAI";

        }

        public SyncContainer(ReputationMessage repAlert) {

            this.Mode = SyncMode.ReputationAlert;
            this.Data = MyAPIGateway.Utilities.SerializeToBinary<ReputationMessage>(repAlert);
            this.Sender = "RAI";

        }

    }

}
