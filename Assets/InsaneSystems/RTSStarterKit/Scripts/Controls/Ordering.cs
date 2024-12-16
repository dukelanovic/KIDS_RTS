using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.Controls
{
	public static class Ordering
	{
		public static void GiveOrder(Vector2 screenPosition, bool isAdditive)
		{
			if (EventSystem.current.IsPointerOverGameObject())
				return;
			
			if (Selection.SelectedUnits.Count == 0 || Selection.SelectedUnits[0].Data.isBuilding)
				return;

			var ray = GameController.CachedMainCamera.ScreenPointToRay(screenPosition);

			if (Physics.Raycast(ray, out var hit, 1000))
			{
				var unit = hit.collider.GetComponent<Unit>();
				Order order;

				if (unit)
				{
					if (!GameController.Instance.PlayersController.IsPlayersInOneTeam(unit.OwnerPlayerId, Player.LocalPlayerId))
					{
						order = new AttackOrder();
						(order as AttackOrder).AttackTarget = unit;
						SpawnEffect(hit.point, GameController.Instance.MainStorage.attackOrderEffect);
					}
					else
					{
						order = new FollowOrder();
					
						(order as FollowOrder).FollowTarget = unit;
						SpawnEffect(hit.point, GameController.Instance.MainStorage.moveOrderEffect);

						var carryModule = unit.GetModule<CarryModule>();
						if (unit.Data.canCarryUnitsCount > 0 && carryModule && carryModule.CanCarryOneMoreUnit())
							carryModule.PrepareToCarryUnits(Selection.SelectedUnits);
					}
				}
				else
				{
					order = new MovePositionOrder();
					(order as MovePositionOrder).MovePosition = hit.point;
					SpawnEffect(hit.point, GameController.Instance.MainStorage.moveOrderEffect);
				}

				SendOrderToSelection(order, isAdditive);
			}
		}

		public static void GiveMapOrder(Vector2 mapPoint)
		{
			if (Selection.SelectedUnits.Count == 0 || Selection.SelectedUnits[0].Data.isBuilding)
				return;
			
			var worldPoint = Minimap.GetMapPointInWorldCoords(mapPoint);
			
			var ray = new Ray(worldPoint + Vector3.up * 100f, Vector3.down);

			if (Physics.Raycast(ray, out var hit, 1000))
			{
				var order = new MovePositionOrder();
				order.MovePosition = hit.point;
				
				SpawnEffect(hit.point, GameController.Instance.MainStorage.moveOrderEffect);
				
				SendOrderToSelection(order, false);
			}
		}

		static void SpawnEffect(Vector3 position, GameObject effect)
		{
			GameObject.Instantiate(effect, position, Quaternion.identity);
		}

		static void SendOrderToSelection(Order order, bool isAdditive)
		{
			SendOrderToUnits(Selection.SelectedUnits, order, isAdditive);
		}

		public static void SendOrderToUnits(List<Unit> units, Order order, bool isAdditive)
		{
			var wayPoints = new List<Vector3>();

			var isMovePositionOrder = order.GetType() == typeof(MovePositionOrder);

			var usedFormation = GameController.Instance.MainStorage.unitsFormation;
			var movePosition = Vector3.zero;

			if (order is MovePositionOrder movePositionOrder)
				movePosition = movePositionOrder.MovePosition;
			else if (order is FollowOrder followOrder)
				movePosition = followOrder.FollowTarget.transform.position;
				
			if (usedFormation == UnitsFormation.Default)
				wayPoints = UnitsFormations.GetWaypointsForUnitsGroup(movePosition, units);
			else if (usedFormation == UnitsFormation.SquarePredict)
				wayPoints = UnitsFormations.GetWaypointsCominedMethods(movePosition, units);
			
			for (int i = 0; i < units.Count; i++)
			{
				var personalOrderForUnit = order.Clone();
				var customMoveOrder = false;

				if (order is FollowOrder && units[i].Data.moveType == UnitData.MoveType.Flying)
				{
					personalOrderForUnit = new MovePositionOrder();
					customMoveOrder = true;
				}

				if ((isMovePositionOrder || customMoveOrder) && personalOrderForUnit is MovePositionOrder personalMovePosition)
					personalMovePosition.MovePosition = wayPoints[i];

				units[i].AddOrder(personalOrderForUnit, isAdditive, i == 0);
			}
		}
	}
}