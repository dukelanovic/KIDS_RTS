using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ProductionIconsPanel : MonoBehaviour
	{
		[SerializeField] RectTransform iconsPanel;

		readonly List<UnitIcon> spawnedIcons = new List<UnitIcon>();

		void Awake() => SelectProductionNumberPanel.SelectedProductionChanged += ProductionChangedAction;
		void OnDestroy() => SelectProductionNumberPanel.SelectedProductionChanged -= ProductionChangedAction;

		void ProductionChangedAction(Production production) => Redraw();
		
		public void Redraw()
		{
			ClearDrawn();

			var selectedProduction = SelectProductionNumberPanel.SelectedBuildingProduction;

			if (selectedProduction == null)
				return;
		
			for (int i = 0; i < selectedProduction.AvailableUnits.Count; i++)
			{
				var spawnedObject = Instantiate(GameController.Instance.MainStorage.unitProductionIconTemplate, iconsPanel);
				var unitIconComponent = spawnedObject.GetComponent<UnitIcon>();

				unitIconComponent.SetupWithUnitData(this, selectedProduction.AvailableUnits[i]);
				unitIconComponent.SetHotkeyById(i);
				
				if (SelectProductionTypePanel.SelectedProductionCategory.isBuildings && selectedProduction.UnitsQueue.Count > 0)
				{
					var isCurrentBuildingInQueue = selectedProduction.UnitsQueue[0] != selectedProduction.AvailableUnits[i];
					
					unitIconComponent.SetActive(isCurrentBuildingInQueue);
				}

				spawnedIcons.Add(unitIconComponent);
			}
		}

		public UnitIcon GetIcon(int id) => spawnedIcons.Count > id ? spawnedIcons[id] : null;

		void ClearDrawn()
		{
			for (int i = 0; i < spawnedIcons.Count; i++)
				Destroy(spawnedIcons[i].gameObject);

			spawnedIcons.Clear();
		}
	}
}