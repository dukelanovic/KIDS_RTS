using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ProductionHint : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] Text nameText;
		[SerializeField] Text descriptionText;

		RectTransform rectTransform;

		void Start()
		{
			rectTransform = selfObject.GetComponent<RectTransform>();

			Hide();
		}

		public void Show(UnitData unitData, Vector2 position)
		{
			selfObject.SetActive(true);

			rectTransform.anchoredPosition = position;
			nameText.text = unitData.textId;

			var electricityText = "";
			var requiredText = "";

			var textLibrary = GameController.Instance.TextsLibrary;

			if (GameController.Instance.MainStorage.isElectricityUsedInGame)
			{
				if (unitData.addsElectricity > 0)
					electricityText = textLibrary.GetUITextById("addsElectricity") + ": " + unitData.addsElectricity + "\n";

				if (unitData.usesElectricity > 0)
					electricityText = textLibrary.GetUITextById("usesElectricity") + ": " + unitData.usesElectricity + "\n";
			}

			if (unitData.requiredBuildingToUnlock)
				requiredText = textLibrary.GetUITextById("requiredBuilding") + ": " + unitData.requiredBuildingToUnlock.textId;
			
			descriptionText.text = textLibrary.GetUITextById("price") + ": " + unitData.price + "\n" + electricityText + textLibrary.GetUITextById("buildTime") + ": " + unitData.buildTime + "s\n" + requiredText;

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)descriptionText.transform); // needed for content size fitter ## необходимо для того, чтобы Content Size Fitter обновлял размер
		}

		public void ShowForAbility(Abilities.Ability ability, Vector2 position)
		{
			selfObject.SetActive(true);

			rectTransform.anchoredPosition = position;
			nameText.text = ability.abilityName;
			descriptionText.text = "";

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)descriptionText.transform);
		}

		public void Hide() => selfObject.SetActive(false);
	}
}