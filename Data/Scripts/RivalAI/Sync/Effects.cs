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

namespace RivalAI.Sync {

	public enum EffectSyncMode {

		None,
		PlayerSound,
		PositionSound,
		Particle,

	}

	[ProtoContract]
	public class Effects {

		[ProtoMember(1)]
		public EffectSyncMode Mode;

		[ProtoMember(2)]
		public Vector3D Coords;

		[ProtoMember(3)]
		public string SoundId;

		[ProtoMember(4)]
		public string ParticleId;

		[ProtoMember(5)]
		public float ParticleScale;

		[ProtoMember(6)]
		public Vector3D ParticleColor;

		[ProtoMember(7)]
		public float ParticleMaxTime;

		[ProtoMember(8)]
		public Vector3D ParticleForwardDir;

		[ProtoMember(9)]
		public Vector3D ParticleUpDir;

		[ProtoMember(10)]
		public Vector3 Velocity;

		[ProtoMember(11)]
		public string AvatarId;

		public Effects() {

			Mode = EffectSyncMode.None;
			Coords = Vector3D.Zero;
			SoundId = "";
			ParticleId = "";
			ParticleScale = 1;
			ParticleColor = Vector3D.Zero;
			ParticleMaxTime = -1;
			ParticleForwardDir = Vector3D.Forward;
			ParticleUpDir = Vector3D.Up;
			Velocity = Vector3.Zero;
			AvatarId = "";

		}

	}

}
