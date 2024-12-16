using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	[System.Serializable]
	public sealed class TriggerData
	{
		public TriggerType TriggerType;
		public string TriggerTextId = "Trigger ID";
		public TriggerBase Trigger;
	}

	/// <summary>List of all available trigger types. Add new types only to the end of enum list. Don't forget to add new trigger types when you create it.</summary>
	public enum TriggerType
	{
		None,
		SpawnUnits,
		AddMoney,
		ChangeOwner,
		MoveOrder,
		ProductionAddUnit
	}

	public class TriggerAttribute : PropertyAttribute { }
}