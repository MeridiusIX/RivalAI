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

namespace RivalAI.Helpers{
	
	public static class VectorHelper{
		
		public static Random Rnd = new Random();
		
		//ClosestDirection
		public static Vector3D ClosestDirection(MatrixD matrix, Vector3D checkCoords, Vector3D ignoreDirectionA, Vector3D ignoreDirectionB){
			
			Vector3D closestVector = Vector3D.Zero;
			double closestDistance = 0;

            //Forward
            if(matrix.Forward != ignoreDirectionA && matrix.Forward != ignoreDirectionB){
				
				var vectorPos = matrix.Translation + matrix.Forward;
				var distance = Vector3D.Distance(vectorPos, checkCoords);
				bool gotCloser = false;
				
				if(closestVector == Vector3D.Zero){
					
					closestDistance = distance;
					closestVector = vectorPos;
					gotCloser = true;
					
				}
				
				if(gotCloser == false && distance < closestDistance){
					
					closestDistance = distance;
					closestVector = vectorPos;
					
				}
				
			}
			
			//Backward
			if(matrix.Backward != ignoreDirectionA && matrix.Backward != ignoreDirectionB){
				
				var vectorPos = matrix.Translation + matrix.Backward;
				var distance = Vector3D.Distance(vectorPos, checkCoords);
				bool gotCloser = false;
				
				if(closestVector == Vector3D.Zero){
					
					closestDistance = distance;
					closestVector = vectorPos;
					gotCloser = true;
					
				}
				
				if(gotCloser == false && distance < closestDistance){
					
					closestDistance = distance;
					closestVector = vectorPos;
					
				}
				
			}
			
			//Up
			if(matrix.Up != ignoreDirectionA && matrix.Up != ignoreDirectionB){
				
				var vectorPos = matrix.Translation + matrix.Up;
				var distance = Vector3D.Distance(vectorPos, checkCoords);
				bool gotCloser = false;
				
				if(closestVector == Vector3D.Zero){
					
					closestDistance = distance;
					closestVector = vectorPos;
					gotCloser = true;
					
				}
				
				if(gotCloser == false && distance < closestDistance){
					
					closestDistance = distance;
					closestVector = vectorPos;
					
				}
				
			}
			
			//Down
			if(matrix.Down != ignoreDirectionA && matrix.Down != ignoreDirectionB){
				
				var vectorPos = matrix.Translation + matrix.Down;
				var distance = Vector3D.Distance(vectorPos, checkCoords);
				bool gotCloser = false;
				
				if(closestVector == Vector3D.Zero){
					
					closestDistance = distance;
					closestVector = vectorPos;
					gotCloser = true;
					
				}
				
				if(gotCloser == false && distance < closestDistance){
					
					closestDistance = distance;
					closestVector = vectorPos;
					
				}
				
			}
			
			//Left
			if(matrix.Left != ignoreDirectionA && matrix.Left != ignoreDirectionB){
				
				var vectorPos = matrix.Translation + matrix.Left;
				var distance = Vector3D.Distance(vectorPos, checkCoords);
				bool gotCloser = false;
				
				if(closestVector == Vector3D.Zero){
					
					closestDistance = distance;
					closestVector = vectorPos;
					gotCloser = true;
					
				}
				
				if(gotCloser == false && distance < closestDistance){
					
					closestDistance = distance;
					closestVector = vectorPos;
					
				}
				
			}
			
			//Right
			if(matrix.Right != ignoreDirectionA && matrix.Right != ignoreDirectionB){
				
				var vectorPos = matrix.Translation + matrix.Right;
				var distance = Vector3D.Distance(vectorPos, checkCoords);
				bool gotCloser = false;
				
				if(closestVector == Vector3D.Zero){
					
					closestDistance = distance;
					closestVector = vectorPos;
					gotCloser = true;
					
				}
				
				if(gotCloser == false && distance < closestDistance){
					
					closestDistance = distance;
					closestVector = vectorPos;
					
				}
				
			}
			
			return closestVector;
			
		}
		
		//CreateDirectionAndTarget
		public static Vector3D CreateDirectionAndTarget(Vector3D startDir, Vector3D endDir, Vector3D startCoords, double distance){
			
			var direction = Vector3D.Normalize(endDir - startDir);
			return direction * distance + startCoords;
			
		}
		
		//GetAngleBetweenDirections
		public static double GetAngleBetweenDirections(Vector3D dirA, Vector3D dirB){
			
			var radians = MyUtils.GetAngleBetweenVectors(dirA, dirB);
			return (180 / Math.PI) * radians;
			
		}
		
		//GetPlanetSealevelAtPosition
		public static Vector3D GetPlanetSealevelAtPosition(Vector3D coords, MyPlanet planet = null){
			
			if(planet == null){
				
				return Vector3D.Zero;
				
			}
			
			var planetEntity = planet as IMyEntity;
			var direction = Vector3D.Normalize(coords - planetEntity.GetPosition());
			return direction * (double)planet.MinimumRadius + planetEntity.GetPosition();
			
		}
		
		public static Vector3D GetPositionCenter(IMyEntity entity){
			
			if(MyAPIGateway.Entities.Exist(entity) == false){
				
				return Vector3D.Zero;
				
			}
			
			if(entity?.PositionComp == null){
				
				return Vector3D.Zero;
				
			}
			
			return entity.PositionComp.WorldAABB.Center;
			
		}
		
		//GetPlanetSurfaceCoordsAtPosition
		public static Vector3D GetPlanetSurfaceCoordsAtPosition(Vector3D coords, MyPlanet planet = null){
			
			if(planet == null){
				
				return Vector3D.Zero;
				
			}
			
			var checkCoords = coords;
			
			return planet.GetClosestSurfacePointGlobal(ref checkCoords);
			
		}
		
		//GetPlanetSurfaceDifference
		public static double GetPlanetSurfaceDifference(Vector3D myCoords, Vector3D testCoords, MyPlanet planet = null){
			
			if(planet == null){
				
				return 0;
				
			}
			
			var testSurfaceCoords = GetPlanetSurfaceCoordsAtPosition(testCoords, planet);
			var mySealevelCoords = GetPlanetSealevelAtPosition(myCoords, planet);
			var testSealevelCoords = GetPlanetSealevelAtPosition(testSurfaceCoords, planet);
			var myDistance = Vector3D.Distance(mySealevelCoords, myCoords);
			var testDistance = Vector3D.Distance(testSealevelCoords, testSurfaceCoords);
			return myDistance - testDistance;
			
		}

        public static Vector3 GetProjectileLeadPosition(float projectileVelocity, Vector3 myPosition, Vector3 myVelocity, Vector3 targetPosition, Vector3 targetVelocity) {



            return targetPosition;

        }
		
	public static Vector3D GetPlanetWaypointPathing(Vector3D myCoords, Vector3D targetCoords, double minAltitude = 200, double maxDistanceToCheck = 1000){
			
			var planet = MyGamePruningStructure.GetClosestPlanet(targetCoords);
			
			if(planet == null){
				
				return Vector3D.Zero;
				
			}
			
			var planetCoords = planet.PositionComp.WorldAABB.Center;
			var aboveTargetCoords = Vector3D.Normalize(targetCoords - planetCoords) * minAltitude + targetCoords;
			var dirToTarget = Vector3D.Normalize(aboveTargetCoords - myCoords);
			var distToTarget = Vector3D.Distance(targetCoords, myCoords);
			double distanceToUse = distToTarget;
			
			if(distToTarget > maxDistanceToCheck){
				
				distanceToUse = maxDistanceToCheck;
				
			}
			
			List<Vector3D> pathSteps = new List<Vector3D>();
			double currentPathDistance = 0;
			
			while(currentPathDistance < distanceToUse){
				
				if((currentPathDistance - distanceToUse) < 50{
					
					thisStep = currentPathDistance - distanceToUse;
					currentPathDistance = distanceToUse;
					
				}else{
					
					currentPathDistance += 50;
					
				}
				
				pathSteps.Add(dirToTarget * currentPathDistance + myCoords);
				
			}
			
			var myDistToCore = Vector3D.Distance(myCoords, planetCoords);
			double currentHighestDistance = myDistToCore;
			Vector3D pathEnd = Vector3D.Zero;
			
			foreach(var pathPoint in pathSteps){

				Vector3D pathPointRef = pathPoint;
				Vector3D surfacePoint = planet.GetClosestSurfacePointGlobal(ref pathPointRef);
				pathEnd = Vector3D.Normalize(surfacePoint - planetCoords) * minAltitude + surfacePoint;
				double pointDistanceFromCore = Vector3D.Distance(planetCoords, pathEnd);
				
				if(currentHighestDistance < pointDistanceFromCore){
					
					currentHighestDistance = pointDistanceFromCore;
					
				}
				
			}
			
			if(currentHighestDistance > myDistToCore){
				
				var forwardStep = dirToTarget * 50 + myCoords;
				return Vector3D.Normalize(forwardStep - planetCoords) * currentHighestDistance + planetCoords;
				
			} else{
				
				return pathEnd;
				
			}
			
		}

        //IsPositionUnderground
        public static bool IsPositionUnderground(Vector3D coords, MyPlanet planet){
			
			if(planet == null){
				
				return false;
				
			}
			
			var closestSurfacePoint = planet.GetClosestSurfacePointGlobal(coords);
			var planetEntity = planet as IMyEntity;
			
			if(Vector3D.Distance(planetEntity.GetPosition(), coords) < Vector3D.Distance(planetEntity.GetPosition(), closestSurfacePoint)){
				
				return true;
				
			}
			
			return false;
			
		}
		
		//RandomDirection
		public static Vector3D RandomDirection(){
			
			return Vector3D.Normalize(MyUtils.GetRandomVector3D());
			
		}
		
		public static Vector3D RandomBaseDirection(MatrixD matrix, bool ignoreForward = false, bool ignoreBackward = false, bool ignoreUp = false, bool ignoreDown = false, bool ignoreLeft = false, bool ignoreRight = false){
			
			var directionList = new List<Vector3D>();
			
			if(ignoreForward == false){
				
				directionList.Add(matrix.Forward);
				
			}
			
			if(ignoreBackward == false){
				
				directionList.Add(matrix.Backward);
				
			}
			
			if(ignoreUp == false){
				
				directionList.Add(matrix.Up);
				
			}
			
			if(ignoreDown == false){
				
				directionList.Add(matrix.Down);
				
			}
			
			if(ignoreLeft == false){
				
				directionList.Add(matrix.Left);
				
			}
			
			if(ignoreRight == false){
				
				directionList.Add(matrix.Right);
				
			}
			
			return directionList[Rnd.Next(0, directionList.Count)];
			
		}
		
		//RandomPerpendicular
		public static Vector3D RandomPerpendicular(Vector3D referenceDir){
			
			var refDir = referenceDir;
			return Vector3D.Normalize(MyUtils.GetRandomPerpendicularVector(ref refDir));
			
		}


        //License Details For FirstOrderIntercept and FirstOrderInterceptTime

        /*The MIT License (MIT)

        Copyright (c) 2008 Daniel Brauer

        Permission is hereby granted, free of charge, to any person obtaining a copy 
        of this software and associated documentation files (the "Software"), to deal 
        in the Software without restriction, including without limitation the rights 
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
        copies of the Software, and to permit persons to whom the Software is furnished 
        to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all 
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
        WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
        CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

        public static Vector3 FirstOrderIntercept(Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity) {

            Vector3 targetRelativePosition = targetPosition - shooterPosition;
            Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
            float t = FirstOrderInterceptTime(shotSpeed, targetRelativePosition, targetRelativeVelocity);
            return targetPosition + t * (targetRelativeVelocity);

        }

        //first-order intercept using relative target position
        public static float FirstOrderInterceptTime(float shotSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity) {

            float velocitySquared = (float)Math.Pow(targetRelativePosition.Length(), 2);

            if(velocitySquared < 0.001f) {

                return 0f;

            }

            float a = velocitySquared - shotSpeed * shotSpeed;

            //handle similar velocities
            if((float)Math.Abs(a) < 0.001f) {

                float t = -(float)Math.Pow(targetRelativePosition.Length(), 2) / (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition));
                return (float)Math.Max(t, 0f); //don't shoot back in time

            }

            float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
            float c = (float)Math.Pow(targetRelativePosition.Length(), 2);
            float determinant = b * b - 4f * a * c;

            if(determinant > 0f) { //determinant > 0; two intercept paths (most common)

                float t1 = (-b + (float)Math.Sqrt(determinant)) / (2f * a);
                float t2 = (-b - (float)Math.Sqrt(determinant)) / (2f * a);

                if(t1 > 0f) {

                    if(t2 > 0f) {

                        return (float)Math.Min(t1, t2); //both are positive

                    } else {

                        return t1; //only t1 is positive

                    }

                } else {

                    return (float)Math.Max(t2, 0f); //don't shoot back in time

                }

            } else if(determinant < 0f) {

                return 0f; //determinant < 0; no intercept path

            } else { //determinant = 0; one intercept path, pretty much never happens

                return (float)Math.Max(-b / (2f * a), 0f); //don't shoot back in time

            }

        }

    }
	
}
