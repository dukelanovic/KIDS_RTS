using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Abilities
{
	[CreateAssetMenu(fileName = "CarryOut", menuName = Storage.AssetName + "/Abilities/Carry Out")]
	public class CarryOut : Ability
	{
		protected override void StartUseAction()
		{
			if (Selection.SelectedUnits.Count == 0)
				return;

			for (int i = 0; i < Selection.SelectedUnits.Count; i++)
			{
				var carryModule = Selection.SelectedUnits[i].GetModule<CarryModule>();

				if (carryModule)
					carryModule.ExitAllUnits();
			}
		}
	}
}