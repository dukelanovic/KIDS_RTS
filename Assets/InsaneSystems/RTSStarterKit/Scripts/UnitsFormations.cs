using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public static class UnitsFormations
	{
		static readonly List<Vector3> resultsSquareMethod = new List<Vector3>();
		static readonly List<Vector3> resultsSimpleMethod = new List<Vector3>();
		static readonly List<Vector3> resultsCombinedMethod = new List<Vector3>();
		
		/// <summary> Makes square formation</summary>
		public static List<Vector3> GetWaypointsForUnitsGroupSquare(Vector3 centerPosition, int unitsCount, float maxRadiusOfUnit = 1f)
		{
			resultsSquareMethod.Clear();

			if (unitsCount == 1)
			{
				resultsSquareMethod.Add(centerPosition);
				return resultsSquareMethod;
			}

			maxRadiusOfUnit *= 2f; // diameter
			
			int rowsCount = Mathf.FloorToInt(Mathf.Sqrt(unitsCount));
			int unitsPerRow = rowsCount;
			int row = 0, cell = 0;

			var centerOffset = new Vector3(rowsCount * maxRadiusOfUnit / 2f, 0, unitsPerRow * maxRadiusOfUnit / 2f);

			for (int i = 0; i < unitsCount; i++)
			{
				resultsSquareMethod.Add(centerPosition - centerOffset + new Vector3(maxRadiusOfUnit * row + Random.Range(-maxRadiusOfUnit / 10f, maxRadiusOfUnit / 10f), 0, maxRadiusOfUnit * cell + Random.Range(-0.175f, 0.175f))); // adding a little random because fully straight lines of units looks stupid

				cell++;

				if (cell >= unitsPerRow)
				{
					row++;
					cell = 0;
				}
			}

			return resultsSquareMethod;
		}

		/// <summary>Saves current units formation</summary>
		public static List<Vector3> GetWaypointsForUnitsGroup(Vector3 destination, List<Unit> units)
		{
			resultsSimpleMethod.Clear();
			
			var currentGroupCenterPoint = Vector3.zero;

			int aliveUnitsCount = units.Count;

			for (int i = 0; i < units.Count; i++)
				if (units[i])
					currentGroupCenterPoint += units[i].transform.position;
				else
					aliveUnitsCount--;

			currentGroupCenterPoint /= aliveUnitsCount;

			for (int i = 0; i < units.Count; i++)
			{
				if (!units[i])
					continue;

				var unitOffset = units[i].transform.position - currentGroupCenterPoint;
				var unitWaypoint = destination + unitOffset / 4f; // decreasing distances between units

				//Debug.DrawRay(unitWaypoint, Vector3.up, Color.green, 1f);
				resultsSimpleMethod.Add(unitWaypoint);
			}
			
			// this method restorces distances between units if they is to small

			for (int i = 0; i < resultsSimpleMethod.Count; i++)
			{
				var currentWayPointToMove = resultsSimpleMethod[i];

				for (int k = 0; k < resultsSimpleMethod.Count; k++)
				{
					var checkingPoint = resultsSimpleMethod[k];
					var distance =
						Vector3.Distance(currentWayPointToMove, checkingPoint); // todo RTS Kit - needs to be optimized

					if (distance <= 1f) // RTS Kit - todo insert unit size?
					{
						var direction = checkingPoint - currentWayPointToMove;
						currentWayPointToMove -= direction * distance;

						resultsSimpleMethod[i] = currentWayPointToMove;
					}
				}

				//Debug.DrawRay(currentWayPointToMove, Vector3.up, Color.magenta, 1f);
			}
			
			return resultsSimpleMethod;
		}
		
		/// <summary>Returns nearest to unit waypoint and removes it from waypoints list (to prevent selection of this point by other unit)</summary>
		public static Vector3 GetNearestWaypointToUnit(Vector3 unitPos, List<Vector3> waypoints)
		{
			var selectedWaypoint = waypoints[0];
			var minimumDistance = (unitPos - selectedWaypoint).sqrMagnitude;
			int waypointToRemoveId = 0;

			for (int i = 1; i < waypoints.Count; i++)
			{
				var currentDistance = (unitPos - waypoints[i]).sqrMagnitude;

				if (currentDistance <= minimumDistance)
				{
					minimumDistance = currentDistance;
					selectedWaypoint = waypoints[i];
					waypointToRemoveId = i;
				}
			}

			waypoints.RemoveAt(waypointToRemoveId);
			
			return selectedWaypoint;
		}

		/// <summary> Uses combined method of offsetted positions and square formations. </summary>
		public static List<Vector3> GetWaypointsCominedMethods(Vector3 destination, List<Unit> units)
		{
			resultsCombinedMethod.Clear();
			
			var identicalWaypoints = GetWaypointsForUnitsGroup(destination, units);
			var squareWaypoints = GetWaypointsForUnitsGroupSquare(destination, units.Count);
			
			//if (units[0].data.moveType == UnitData.MoveType.Flying) // todo
			//	return squareWaypoints;
			
			for (int i = 0; i < units.Count; i++)
			{
				var finalPosition = GetNearestWaypointToUnit(identicalWaypoints[i], squareWaypoints);
				
				resultsCombinedMethod.Add(finalPosition);
			}

			return resultsCombinedMethod;
		}
	}
}