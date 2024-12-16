using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.AI
{
	[CreateAssetMenu(fileName = "AISettings", menuName = Storage.AssetName + "/AI Settings")]
	public class AISettings : ScriptableObject
	{
		[Tooltip("Time in seconds represents delay between any AI actions")]
		[Range(0f, 2f)] public float thinkTime = 0.2f;
		
		[Tooltip("Time in seconds before AI will try to buy new building.")]
		[Range(0f, 120f)] public float BuildingBuyDelay = 1f;
		[Tooltip("Time in seconds before AI will try to buy new unit.")]
		[Range(0f, 120f)] public float UnitBuyDelay = 0.5f;
		
		[Tooltip("Delay from game start in seconds before AI will start build any buildings.")]
		[Range(0f, 720f)] public float delayBeforeStartCreateBuildings = 0f;

		[Tooltip("Delay from game start in seconds before AI will start build any attacking units.")]
		[Range(0f, 720f)] public float delayBeforeStartBuyingUnits = 0f;

		[Tooltip("Units group size, which AI will try to make before attack player.")]
		public float UnitsGroupSize = 3;

		[Tooltip("How much buildings of the same type AI will try to build.")]
		[Range(1, 8)] public int MaximumSameBuildings = 1;

		[Tooltip("Prority of AI Building. First buildings have bigger priority in AI building queue.")]
		public UnitData[] buildingPriority;

		[Tooltip("Make different lists of units to be built by AI. AI will select lists by order and will build units from the list in its order. After all units from list was built, AI will select next list. When lists ends, it will begin built from first list again. ")]
		public List<UnitsList> UnitsToBuild = new List<UnitsList>();
	}

	[System.Serializable]
	public class UnitsList
	{
		public List<UnitData> Units = new List<UnitData>();

		[Tooltip("Sets up when this group will be built by AI.\n\nAlways means units will be built all game match time.\n\nBuildUntilTime means that group will be built BEFORE game match time reaches this parameter. It is useful to make AI send weak units groups at early-game.\n\nBuildAfterTime means that group will be built AFTER time passed. Useful for late-game units.\n\nTime represented in seconds. ")]
		public BuildTimeType BuildTimeType = BuildTimeType.Always;
		[Tooltip("Time represented in seconds. ")]
		public float TimeValue;

		public bool CanBuildByTime()
		{
			if (BuildTimeType == BuildTimeType.Always)
				return true;
			
			var actualTime = Time.timeSinceLevelLoad;

			switch (BuildTimeType)
			{
				case BuildTimeType.BuildAfterTime when TimeValue > actualTime:
				case BuildTimeType.BuildUntilTime when TimeValue < actualTime:
					return true;
				
				default:
					return false;
			}
		}
	}

	public enum BuildTimeType
	{
		Always,
		BuildAfterTime,
		BuildUntilTime
	}
}