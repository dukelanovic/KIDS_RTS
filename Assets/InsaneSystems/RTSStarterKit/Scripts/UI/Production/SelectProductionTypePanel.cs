using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class SelectProductionTypePanel : MonoBehaviour
	{
		public static ProductionCategory SelectedProductionCategory { get; protected set; }

		public static event ProductionCategoryChangedAction ProductionCategoryChanged;
		public delegate void ProductionCategoryChangedAction(ProductionCategory productionCategory);

		[SerializeField] RectTransform productionIconsPanel;
		
		readonly List<ProductionTypeButton> productionTypeButtons = new List<ProductionTypeButton>();

		void Awake()
		{
			PlayersController.ProductionModuleWasAddedToPlayer += OnProductionModuleSpawned;
			Controls.Selection.ProductionUnitSelected += OnProductionSelected;
		}

		void OnDestroy()
		{
			Controls.Selection.ProductionUnitSelected -= OnProductionSelected;
			PlayersController.ProductionModuleWasAddedToPlayer -= OnProductionModuleSpawned;
		}
		
		void Start()
		{
			var productionCategories = GameController.Instance.MainStorage.availableProductionCategories;
			for (int i = 0; i < productionCategories.Count; i++)
			{
				if (productionCategories[i] == null)
				{
					Debug.LogWarning("Storage contains empty field in Available Production Categories, please remove it, now it will be ignored.");
					continue;
				}

				if (!Player.LocalPlayer.SelectedFaction.ownProductionCategories.Contains(productionCategories[i]))
					continue;

				var spawnedObject = Instantiate(GameController.Instance.MainStorage.productionButtonTemplate, productionIconsPanel);
				var productionIcon = spawnedObject.GetComponent<ProductionTypeButton>();

				productionIcon.SetupWithController(this);
				productionIcon.SetupWithProductionCategory(productionCategories[i]);
				productionTypeButtons.Add(productionIcon);
			}

			OnSelectButtonClick(GameController.Instance.MainStorage.availableProductionCategories[0]);
		}

		void Redraw()
		{
			for (int i = 0; i < productionTypeButtons.Count; i++)
			{
				if (productionTypeButtons[i].GetProductionCategory == SelectedProductionCategory)
					productionTypeButtons[i].SetActive();
				else
					productionTypeButtons[i].SetUnactive();

				productionTypeButtons[i].Redraw();
			}
		}

		public void OnProductionSelected(Production production)
		{
			var productionNumber = production.GetProductionNumber();

			OnSelectButtonClick(production.GetProductionCategory);

			UIController.Instance.SelectProductionNumberPanel.SelectBuildingWithNumber(productionNumber);
		}

		public void OnSelectButtonClick(ProductionCategory productionType)
		{
			SelectedProductionCategory = productionType;

			ProductionCategoryChanged?.Invoke(SelectedProductionCategory);

			Redraw();
		}

		public void OnProductionModuleSpawned(Production production) => Redraw();
	}
}