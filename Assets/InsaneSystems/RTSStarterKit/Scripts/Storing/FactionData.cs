using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "FactionData", menuName = Storage.AssetName + "/Faction Data")]
	public class FactionData : ScriptableObject
	{
		[Comment("Note that factions are in preview, so in next versions their functionality will be improved and extended, and possible bugs will be fixed.")]
		public string textId = "Faction Name";
		[Tooltip("Command center of faction, which will be spawned on game start.")]
		public UnitData factionCommandCenter;
		[Tooltip("Default house color of faction. It actually not used in the asset, but you can use it in your own scripts.")]
		public Color defaultColor = Color.red;
		[Tooltip("Drag here all production categories which belongs to this faction.")]
		public List<ProductionCategory> ownProductionCategories = new List<ProductionCategory>();
	}
}