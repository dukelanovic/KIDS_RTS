using System.Collections.Generic;

namespace InsaneSystems.RTSStarterKit.AI
{
	public class UnitsGroup
	{
		Order groupOrder;

		readonly List<Unit> aliveUnits = new List<Unit>();
		readonly List<Unit> unitsInGroup = new List<Unit>();

		public void AddOrderToGroup(Order order, bool isAdditive = false)
		{
			groupOrder = order;

			var units = GetAliveUnitsOfGroup();

			for (int i = 0; i < units.Count; i++)
				units[i].AddOrder(order, isAdditive, i == 0);
		}

		public void AddUnit(Unit unit) => unitsInGroup.Add(unit);

		public bool IsGroupNeedsUnits(AISettings aiSettings)
		{
			var groupSize = GetAliveUnitsOfGroup().Count;

			return groupSize < aiSettings.UnitsGroupSize;
		}

		public bool IsGroupHaveOrder()
		{
			if (groupOrder is AttackOrder attackOrder && attackOrder.AttackTarget == null)
				groupOrder = null;

			return groupOrder != null;
		}

		List<Unit> GetAliveUnitsOfGroup()
		{
			aliveUnits.Clear();

			for (int i = 0; i < unitsInGroup.Count; i++)
				if (unitsInGroup[i] != null)
					aliveUnits.Add(unitsInGroup[i]);

			return aliveUnits;
		}
	}
}