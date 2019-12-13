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

namespace RivalAI.Behavior.Subsystems{
	
	public enum SpawnPositioningEnum{
		
		RandomDirection,
		SetDirection,
		OffsetFromRemoteControl
		
	}
	
	public class SpawningSystem{
		
		//Configurable
		public bool UseSpawningSystem;
		public List<string> SpawnGroups;
		public List<string> SpawnFactionOverride;
		
		public int MinTimeBetweenSpawns;
		public int MaxTimeBetweenSpawns;
		public int MaxNumberOfSpawnEvents;
		
		public SpawnPositioningEnum SpawnPositioning;
		public double MinSpawnDistance;
		public double MaxSpawnDistance;
		public double SpawnVelocity;
		public Vector3D SpawnOffsetPosition;
		public bool IgnoreSpawningSafetyChecks;
		
		//Non-Configurable
		public IMyRemoteControl RemoteControl;
		
		public DateTime LastSpawnTime;
		public bool ForceSpawn;
		public int TimeUntilNextSpawn;
		public int TotalSpawnEvents;
		
		public bool SpawnInProgress;
		public string SelectedSpawnGroup;
		public string SelectedFactionOverride;
		public Vector3D SpawnCoords;
		public bool SafeToSpawn;
		
		public event Action SpawnChatTrigger;
		public Random Rnd;
		
		public SpawningSystem(IMyRemoteControl remoteControl){
			
			UseSpawningSystem = false;
			SpawnGroups = new List<string>();
			SpawnFactionOverride = new List<string>();
			
			MinTimeBetweenSpawns = 120;
			MaxTimeBetweenSpawns = 300;
			
			MaxNumberOfSpawnEvents = 5;
			
			SpawnPositioning = SpawnPositioningEnum.RandomDirection;
			MinSpawnDistance = 300;
			MaxSpawnDistance = 500;
			SpawnVelocity = 0;
			SpawnOffsetPosition = Vector3D.Zero;
			IgnoreSpawningSafetyChecks = false;
			
			RemoteControl = remoteControl;
			Rnd = new Random();
			
			LastSpawnTime = MyAPIGateway.Session.GameDateTime;
			ForceSpawn = false;
			TimeUntilNextSpawn = Rnd.Next(MinTimeBetweenSpawns, MaxTimeBetweenSpawns);
			TotalSpawnEvents = 0;
			
			SpawnInProgress = false;
			SelectedSpawnGroup = "";
			SelectedFactionOverride = "";
			SpawnCoords = Vector3D.Zero;
			SafeToSpawn = false;
			
		}
		
		public void SpawnRequest(){
			
			if(this.SpawnInProgress == true){
				
				return;
				
			}
			
			
		}
		
		public void SpawningParallelChecks(){
			
			
			
		}
		
		public void CompleteSpawning(){
			
			
			
		}
		
		public void InitTags(){
			
			if(string.IsNullOrWhiteSpace(this.RemoteControl.CustomData) == false) {

                var descSplit = this.RemoteControl.CustomData.Split('\n');

                foreach(var tag in descSplit) {

                    //Tags Go Here
					
				}
				
			}
			
		}
		
	}
	
}