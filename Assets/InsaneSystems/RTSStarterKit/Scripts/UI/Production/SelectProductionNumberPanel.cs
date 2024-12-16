using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class SelectProductionNumberPanel : MonoBehaviour
	{
		public static int SelectedBuildingNumber { get; protected set; }
		public static Production SelectedBuildingProduction { get; protected set; }

		public static event SelectedProductionChangedAction SelectedProductionChanged;
		public delegate void SelectedProductionChangedAction(Production selectedProduction);
		
		[SerializeField] List<BuildingNumberButton> buildNumberIcons = new List<BuildingNumberButton>();

		void Awake() => SelectProductionTypePanel.ProductionCategoryChanged += OnProductionCategoryChanged;
		void OnDestroy() => SelectProductionTypePanel.ProductionCategoryChanged -= OnProductionCategoryChanged;
		
		void OnProductionCategoryChanged(ProductionCategory newCategory) => SelectBuildingWithNumber(0);

		public void SelectBuildingWithNumber(int number)
		{
			var playerProductions = GetPlayerProductionsByCategory(SelectProductionTypePanel.SelectedProductionCategory);

			if (number >= playerProductions.Count)
				return;

			SelectedBuildingNumber = number;
			SelectedBuildingProduction = GetPlayerProductionByTypeAndNumber(SelectProductionTypePanel.SelectedProductionCategory, SelectedBuildingNumber);
			Redraw(SelectProductionTypePanel.SelectedProductionCategory);

			SelectedProductionChanged?.Invoke(SelectedBuildingProduction);
		}

		void Redraw(ProductionCategory newCategory)
		{
			var playerProductions = GetPlayerProductionsByCategory(newCategory);

			for (int i = 0; i < buildNumberIcons.Count; i++)
			{
				var buildingNumberButton = buildNumberIcons[i];

				buildingNumberButton.SetupBuildingId(i);
				buildingNumberButton.SetupWithController(this);

				if (i < playerProductions.Count)
					buildingNumberButton.SetEnabled();
				else
					buildingNumberButton.SetDisabled();

				if (i == SelectedBuildingNumber)
					buildingNumberButton.SetActive();
				else
					buildingNumberButton.SetUnactive();
			}
		}

		Production GetPlayerProductionByTypeAndNumber(ProductionCategory category, int number)
		{
			var playerProductions = GetPlayerProductionsByCategory(category);
			
			return playerProductions.Count > 0 ? playerProductions[number] : null;
		}

		List<Production> GetPlayerProductionsByCategory(ProductionCategory category)
		{
			var localPlayer = GameController.Instance.PlayersController.PlayersIngame[Player.LocalPlayerId];
			var playerProductions = localPlayer.GetProductionBuildingsByCategory(category);

			return playerProductions;
		}

	}
}