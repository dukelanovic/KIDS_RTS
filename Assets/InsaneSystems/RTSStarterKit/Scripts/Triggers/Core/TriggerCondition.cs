using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	[CreateAssetMenu(fileName = "TriggerCondition", menuName = Storage.AssetName + "/Trigger Condition")]
	[System.Serializable]
	public class TriggerCondition : ScriptableObject
	{
		public TriggerConditionType ConditionType = TriggerConditionType.ByEnteringZoneUnits;

		[Header("Units enter/exit zone settings")]
		public bool IsUnitsShouldBeOwnedBySpecificPlayer;
		[Range(0, 15)] public int UnitsPlayerOwnerShouldBe;
		public bool IsUnitsShouldBeOfSpecificType;
		public List<UnitData> UnitsShouldBeOneOfTheseTypes = new List<UnitData>();

		[Header("Time condition settings")]
		public float TimeToStartTrigger;

		public virtual bool IsConditionTrue(Unit unitTriggeredZone = null)
		{
			if (ConditionType == TriggerConditionType.ByTimeLeft)
				return Time.timeSinceLevelLoad >= TimeToStartTrigger;

			if (ConditionType == TriggerConditionType.ByEnteringZoneUnits || ConditionType == TriggerConditionType.ByExitingZoneUnits)
			{
				if (unitTriggeredZone == null)
					return false;
				
				if (!IsUnitTypeSuitable(unitTriggeredZone) || !IsUnitOwnedByACorrectPlayer(unitTriggeredZone))
					return false;

				return true;
			}

			return false;
		}

		protected bool IsUnitOwnedByACorrectPlayer(Unit unit)
		{
			if (!IsUnitsShouldBeOwnedBySpecificPlayer)
				return true;

			return unit.IsOwnedByPlayer(UnitsPlayerOwnerShouldBe);
		}

		protected bool IsUnitTypeSuitable(Unit unit)
		{
			if (!IsUnitsShouldBeOfSpecificType)
				return true;

			return UnitsShouldBeOneOfTheseTypes.Contains(unit.Data);
		}
	}

	public enum TriggerConditionType
	{
		ByEnteringZoneUnits,
		ByExitingZoneUnits,
		ByTimeLeft,
		//ByUnitsCountOnMap,
		//ByKilledUnitsCount,
	}
}