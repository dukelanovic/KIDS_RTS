using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class UnitIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		[SerializeField] Image fillImage;
		[SerializeField] Image iconImage;
		[SerializeField] Button button;
		[SerializeField] Text countText, hotkeyText;
		[SerializeField] GameObject hotkeyPanel;
		Image hotkeyBG;

		UnitData unitDataTemplate;

		ProductionIconsPanel selfProductionIconsPanel;

		RectTransform rectTransform;
		
		void Start()
		{
			rectTransform = GetComponent<RectTransform>();

			if (hotkeyPanel)
				hotkeyBG = hotkeyPanel.GetComponent<Image>();
		}
		
		void HotkeyDrawMode(bool visible)
		{
			if (!hotkeyBG)
				return;
			
			var color = hotkeyBG.color;
			color.a = visible ? 1f : 0.25f;
			hotkeyBG.color = color;
		}

		void Update() => Redraw();

		public void Redraw()
		{
			var selectedProduction = SelectProductionNumberPanel.SelectedBuildingProduction;
			var isBuilding = IsBuildingType();
			var isInProductionQueue = selectedProduction.IsUnitOfTypeInQueue(unitDataTemplate);
			
			if (!selectedProduction)
				return;

			iconImage.sprite = unitDataTemplate.icon;

			if (selectedProduction.IsUnitOfTypeCurrentlyBuilding(unitDataTemplate))
				fillImage.fillAmount = 1f - selectedProduction.GetBuildProgressPercents();
			else if ((isBuilding && isInProductionQueue) || (!isBuilding && isInProductionQueue))
				fillImage.fillAmount = 1f;
			else
				fillImage.fillAmount = 0f;

			int unitsCount = selectedProduction.GetUnitsOfSpecificTypeInQueue(unitDataTemplate);
			countText.text = unitsCount > 0 ? unitsCount.ToString() : "";

			var isActive = true;
			
			if (isBuilding && IsAnyBuildingInQueue(selectedProduction))
				isActive =IsCurrentBuildingInQueue(selectedProduction);

			if (isActive)
				isActive = CheckUnlockConditions();
			
			SetActive(isActive);
			
			HotkeyDrawMode(InputHandler.HotkeysInputMode == HotkeysInputType.Building);
		}

		bool CheckUnlockConditions()
		{
			if (!unitDataTemplate.requiredBuildingToUnlock)
				return true;

			return Player.LocalPlayer.HasBuildingType(unitDataTemplate.requiredBuildingToUnlock);
		}

		public void SetHotkeyById(int id)
		{
			if (!GameController.Instance.MainStorage.showHotkeysOnUI)
			{
				hotkeyPanel.SetActive(false);
				return;
			}

			hotkeyText.text = InputHandler.SceneInstance.BuildingInputKeys[id].ToString();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
				OnClick();
			else if (eventData.button == PointerEventData.InputButton.Right)
				OnRightClick();
		}

		public void OnClick()
		{
			var selectedProduction = SelectProductionNumberPanel.SelectedBuildingProduction;

			if (!selectedProduction || !CheckUnlockConditions())
				return;

			if (IsBuildingType())
			{
				bool isBuildingReady = IsCurrentBuildingInQueue(selectedProduction) && selectedProduction.IsBuildingReady();

				if (IsAnyBuildingInQueue(selectedProduction))
				{
					if (isBuildingReady)
						GameController.Instance.Build.EnableBuildMode(unitDataTemplate.selfPrefab, () =>
						{
							selectedProduction.FinishBuilding();
							selfProductionIconsPanel.Redraw();
						});
					
					return;
				}
			}
			
			selectedProduction.AddUnitToQueue(unitDataTemplate);
		}

		void OnRightClick()
		{
			var selectedProduction = SelectProductionNumberPanel.SelectedBuildingProduction;

			if (!selectedProduction)
				return;

			selectedProduction.RemoveUnitFromQueue(unitDataTemplate, true);

			selfProductionIconsPanel.Redraw();
		}

		bool IsAnyBuildingInQueue(Production production) => production.UnitsQueue.Count > 0;
		bool IsCurrentBuildingInQueue(Production production) => production.UnitsQueue.Count > 0 && production.UnitsQueue[0] == unitDataTemplate;
		bool IsBuildingType() => SelectProductionTypePanel.SelectedProductionCategory.isBuildings;

		public void SetActive(bool value) => button.interactable = value;

		public void SetupWithUnitData(ProductionIconsPanel selfPanel, UnitData unitData)
		{
			selfProductionIconsPanel = selfPanel;
			unitDataTemplate = unitData;
			Redraw();
		}

		public void OnPointerEnter(PointerEventData pointerEventData)
		{
			UIController.Instance.ProductionHint.Show(unitDataTemplate, rectTransform.position + new Vector3(0, rectTransform.sizeDelta.y / 2f + 10));
		}

		public void OnPointerExit(PointerEventData pointerEventData)
		{
			UIController.Instance.ProductionHint.Hide();
		}
	}
}