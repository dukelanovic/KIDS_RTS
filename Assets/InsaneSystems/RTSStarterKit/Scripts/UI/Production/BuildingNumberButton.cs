using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class BuildingNumberButton : MonoBehaviour
	{
		[SerializeField] Text numberText;
		[SerializeField] Button selfButton;
		[SerializeField] [Range(0, 4)] int buildingId;
		[SerializeField] Color activeColor = Color.green;

		SelectProductionNumberPanel controllerPanel;

		Image selfImage;
		Color defaultColor;

		void Awake()
		{
			selfImage = GetComponent<Image>();
			defaultColor = selfImage.color;
		}

		public virtual void SetActive() => selfImage.color = activeColor;
		public virtual void SetUnactive() => selfImage.color = defaultColor;
		public virtual void SetEnabled() => selfButton.interactable = true;
		public virtual void SetDisabled() => selfButton.interactable = false;

		public virtual void OnClick() => controllerPanel.SelectBuildingWithNumber(buildingId);

		public void SetupBuildingId(int id)
		{
			buildingId = id;
			numberText.text = (buildingId + 1).ToString();
		}

		public void SetupWithController(SelectProductionNumberPanel controllerPanel) => this.controllerPanel = controllerPanel;
	}
}