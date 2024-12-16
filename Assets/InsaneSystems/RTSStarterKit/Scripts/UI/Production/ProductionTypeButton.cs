using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ProductionTypeButton : MonoBehaviour
	{
		[SerializeField] ProductionCategory productionCategory;
		[SerializeField] Color selectedColor = Color.green;
		
		SelectProductionTypePanel controllerPanel;

		Image selfImage;
		Button selfButton;
		Color defaultColor;

		public ProductionCategory GetProductionCategory => productionCategory;

		void Awake()
		{
			selfImage = GetComponent<Image>();
			selfButton = GetComponent<Button>();
			defaultColor = selfImage.color;
		}

		void Start() => Redraw();

		public virtual void SetActive() => selfImage.color = selectedColor;
		public virtual void SetUnactive() => selfImage.color = defaultColor;
		public virtual void OnClick() => controllerPanel.OnSelectButtonClick(productionCategory);

		public virtual void Redraw() => selfButton.interactable = Player.LocalPlayer.IsHaveProductionOfCategory(productionCategory);

		public void SetupWithController(SelectProductionTypePanel typePanel) => controllerPanel = typePanel;

		public virtual void SetupWithProductionCategory(ProductionCategory category)
		{
			productionCategory = category;

			selfImage.sprite = category.icon;

			Redraw();
		}
	}
}