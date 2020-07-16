﻿using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace RivalAI.Helpers {
	public static class MathTools {

		private static Random _rnd = new Random();

		/// <summary>
		/// Calculates Acceleration from Provided Force and Mass
		/// </summary>
		/// <param name="forceNewtons">Total Force in Newtons</param>
		/// <param name="mass">Total Mass in Kilograms</param>
		/// <returns>Acceleration of the Force and Mass in m/s^2</returns>
		public static double CalculateAcceleration(double forceNewtons, double mass) {

			//A = F/M
			return forceNewtons / mass;
		
		}

		/// <summary>
		/// This method will calculate the distance that a ship will stop at whent Braking Acceleration is applied.
		/// </summary>
		/// <param name="brakingAcceleration">The acceleration amount provided by breaking force</param>
		/// <param name="currentVelocity">Current velocity in m/s</param>
		/// <param name="desiredStopSpeed">What velocity you want to end at (default 0)</param>
		/// <param name="gravityAcceleration">Gravity acceleration (in m/s) that will be factored into the calculation (assuming stopping is in same direction)</param>
		/// <returns>Distance in Meters that Braking Acceleration needs to be applied at</returns>
		public static double StoppingDistance(double brakingAcceleration, double currentVelocity, double desiredStopSpeed = 0, double gravityAcceleration = 0) {

			double acceleration = Math.Abs(brakingAcceleration) - Math.Abs(gravityAcceleration);

			if (acceleration <= 0)
				return -1;

			double time = (desiredStopSpeed - currentVelocity) / (brakingAcceleration * -1);
			double timeMultipliedSpeed = time * currentVelocity;
			double distance = ((brakingAcceleration * -1) * (time * time)) / 2;
			return timeMultipliedSpeed + distance;

		}

		/// <summary>
		/// This method will provide a linear interpolation (lerp) of a provided value between a min and max range.
		/// Ex: Providing a value of 5 between -10 and 10 would result in 0.75
		/// </summary>
		/// <param name="minValue">The lowest value used in the range</param>
		/// <param name="maxValue">The highest value used in the range</param>
		/// <param name="providedValue">A value that lives between the provided minValue and maxValue values</param>
		/// <returns>Returns the value in a range between 0-1</returns>
		public static double LerpToMultiplier(double minValue, double maxValue, double providedValue) {

			var range = maxValue - minValue;
			var unit = range / 100;
			var percent = (providedValue * unit) / maxValue;
			return percent;

		}

		/// <summary>
		/// This method will provide a linear interpolation (lerp) of a provided value between a min and max range.
		/// Ex: Providing a value of 0.25 between -10 and 10 would result in -5
		/// </summary>
		/// <param name="minValue">The lowest value used in the range</param>
		/// <param name="maxValue">The highest value used in the range</param>
		/// <param name="providedMultiplier">A value between 0-1</param>
		/// <returns>Returns the value in the provided range</returns>
		public static double LerpToValue(double minValue, double maxValue, double providedMultiplier) {

			var range = maxValue - minValue;
			var unit = providedMultiplier * range;
			var newValue = minValue + unit;
			return newValue;

		}

		public static void MinMaxRangeSafety(ref double min, ref double max) {

			if (min < max)
				return;

			if (min == max)
				max = min + 1;

			if (min > max) {

				var tempMin = min;
				var tempMax = max;
				min = tempMax;
				max = tempMin;

			}

		}

		public static void MinMaxRangeSafety(ref float min, ref float max) {

			if (min < max)
				return;

			if (min == max)
				max = min + 1;

			if (min > max) {

				var tempMin = min;
				var tempMax = max;
				min = tempMax;
				max = tempMin;

			}

		}

		public static void MinMaxRangeSafety(ref int min, ref int max) {

			if (min < max)
				return;

			if (min == max)
				max = min + 1;

			if (min > max) {

				var tempMin = min;
				var tempMax = max;
				min = tempMax;
				max = tempMin;

			}

		}

		/// <summary>
		/// This method will convert a radian value into degrees
		/// </summary>
		/// <param name="radians">The value, in radians</param>
		/// <returns>Value in Degrees</returns>
		public static double RadiansToDegrees(double radians) {

			return (180 / Math.PI) * radians;

		}

		/// <summary>
		/// This method calculates how long it will take to reach a desired velocity with the provided acceleration and current velocity.
		/// </summary>
		/// <param name="acceleration">Acceleration of object</param>
		/// <param name="targetVelocity">The desired velocity to reach</param>
		/// <param name="currentVelocity">The current velocity of the object</param>
		/// <returns></returns>
		public static double TimeToVelocity(double acceleration, double targetVelocity, double currentVelocity = 0) {

			return Math.Abs((targetVelocity - currentVelocity) / acceleration);
		
		}

		public static bool WithinTolerance(double number, double target, double tolerance) {

			return !UnderTolerance(number, target, tolerance) && !OverTolerance(number, target, tolerance);

		}

		public static bool UnderTolerance(double number, double target, double tolerance) {

			return number - tolerance < target;

		}

		public static bool OverTolerance(double number, double target, double tolerance) {

			return number + tolerance > target;

		}

		public static double ValueBetween(double a, double b) {

			if (a == b)
				return a;

			double min = a < b ? a : b;
			double max = a < b ? b : a;
			return min + ((max - min) / 2);

		}

		public static double RandomBetween(double a, double b) {

			if (a == b)
				return a;

			double min = a < b ? a : b;
			double max = a < b ? b : a;
			return _rnd.Next((int)min, (int)max);

		}

	}
}
