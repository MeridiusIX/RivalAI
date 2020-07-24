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
	
	public static class EffectManager {

		public static bool SoundsPending = false;
		public static bool SoundsPlaying = false;
		public static List<string> SoundsPendingList = new List<string>();
		public static List<string> AvatarPendingList = new List<string>();

		public static IMyEntity CurrentPlayerEntity;
		public static MyEntity3DSoundEmitter SoundEmitter;
		public static bool GotFirstEmitter;
		public static string CurrentAvatar;

		public static void ClientReceiveEffect(Effects effectData) {

			if(effectData.Mode == EffectSyncMode.PlayerSound) {
				
				SoundsPendingList.Add(effectData.SoundId);
				AvatarPendingList.Add(effectData.AvatarId);
				SoundsPending = true;

			}

			if(effectData.Mode == EffectSyncMode.PositionSound) {

				Logger.MsgDebug("Process Position Sound");
				ProcessPositionSoundEffect(effectData);

			}

			if (effectData.Mode == EffectSyncMode.Particle) {

				Logger.MsgDebug("Process Particle");
				ProcessParticleEffect(effectData);

			}

		}

		public static void SendParticleEffectRequest(string id, MatrixD remoteMatrix, Vector3D offset, float scale, float maxTime, Vector3D color) {

			var effect = new Effects();
			effect.Mode = EffectSyncMode.Particle;
			effect.Coords = Vector3D.Transform(offset, remoteMatrix);
			effect.ParticleId = id;
			effect.ParticleScale = scale;
			effect.ParticleColor = color;
			effect.ParticleMaxTime = maxTime;
			effect.ParticleForwardDir = remoteMatrix.Forward;
			effect.ParticleUpDir = remoteMatrix.Up;
			var syncData = new SyncContainer(effect);

			foreach (var player in TargetHelper.GetPlayersWithinDistance(effect.Coords, 15000)) {

				SyncManager.SendSyncMesage(syncData, player.SteamUserId);

			}

		}

		public static void ProcessParticleEffect(Effects effectData) {

			MyParticleEffect effect;
			var particleMatrix = MatrixD.CreateWorld(effectData.Coords, effectData.ParticleForwardDir, effectData.ParticleUpDir);
			var particleCoords = effectData.Coords;

			if (MyParticlesManager.TryCreateParticleEffect(effectData.ParticleId, ref particleMatrix, ref particleCoords, uint.MaxValue, out effect) == false) {

				return;

			}

			effect.UserScale = effectData.ParticleScale;

			if (effectData.ParticleMaxTime > 0) {

				//effect.DurationMin = effectData.ParticleMaxTime;
				//effect.DurationMax = effectData.ParticleMaxTime;

			}

			if (effectData.ParticleColor != Vector3D.Zero) {

				//var newColor = new Vector4((float)effectData.ParticleColor.X, (float)effectData.ParticleColor.Y, (float)effectData.ParticleColor.Z, 1);
				//effect.UserColorMultiplier = newColor;

			}

			effect.Velocity = effectData.Velocity;
			//effect.Loop = false;

		}

		public static void ProcessPlayerSoundEffect() {

			if(SoundsPending == false) {

				return;

			}

			if(CheckPlayerSoundEmitter() == false) {

				return;

			}

			if(SoundEmitter.IsPlaying == true) {

				return;

			}
			
			if(SoundsPendingList.Count == 0){
			
				SoundsPending = false;
				return;
			
			}
			
			var soundPair = new MySoundPair(SoundsPendingList[0]);
			SoundsPendingList.RemoveAt(0);
			SoundEmitter.PlaySound(soundPair, false, false, true, true, false);
			SoundsPlaying = true;

			if (SoundsPendingList.Count == 0){
			
				SoundsPending = false;
			
			}

		}

		public static void ProcessAvatarDisplay() {

			if (SoundEmitter == null)
				return;

			if (!SoundEmitter.IsPlaying || string.IsNullOrWhiteSpace(CurrentAvatar)) {

				SoundsPlaying = false;
				return;

			}


			var camera = MyAPIGateway.Session.Camera;
			var camCenter = camera.Position + camera.ViewMatrix.Forward;
			var screenPos = camera.WorldToScreen(ref camCenter);
			MyTransparentGeometry.AddBillboardOriented(MyStringId.GetOrCompute("Avatar-Glitchy-Berserk"), Color.White, screenPos, camera.ViewMatrix.Left, camera.ViewMatrix.Up, (float)1 * 0.075f, 4);


		}

		public static bool CheckPlayerSoundEmitter() {

			if(MyAPIGateway.Session.LocalHumanPlayer?.Controller?.ControlledEntity?.Entity == null) {

				CurrentPlayerEntity = null;
				SoundEmitter = null;
				SoundsPlaying = false;

				if (GotFirstEmitter) {

					AvatarPendingList.Clear();
					SoundsPendingList.Clear();
					SoundsPending = false;
					SoundsPlaying = false;
				
				}

				return false;

			}

			if(MyAPIGateway.Session.LocalHumanPlayer.Controller.ControlledEntity.Entity == CurrentPlayerEntity) {

				return true;

			}

			CurrentPlayerEntity = MyAPIGateway.Session.LocalHumanPlayer.Character;
			SoundEmitter = new MyEntity3DSoundEmitter(CurrentPlayerEntity as MyEntity);
			GotFirstEmitter = true;
			return true;

		}

		public static void ProcessPositionSoundEffect(Effects effectData) {

			MyVisualScriptLogicProvider.PlaySingleSoundAtPosition(effectData.SoundId, effectData.Coords);

		}

	}
}
