using UnityEngine;
using UnityEngine.UI;
using InsaneSystems.RTSStarterKit.Abilities;
using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.UI
{
	public sealed class AbilityIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] Image iconImage;
		[SerializeField] GameObject hotkeyPanel;
		[SerializeField] Text hotkeyText;

		Button button;

		Ability selfAbility;

		RectTransform rectTransform;

		void Awake()
		{
			button = GetComponent<Button>();
			rectTransform = GetComponent<RectTransform>();
		}

		public void Setup(Ability ability)
		{
			iconImage.sprite = ability.icon;

			selfAbility = ability;

			if (!button)
				button = GetComponent<Button>();

			button.interactable = ability.IsActive;
			
			hotkeyPanel.SetActive(false);
		}

		public void SetupHotkeyByNumber(int id)
		{
			if (!GameController.Instance.MainStorage.showHotkeysOnUI)
				return;
			
			KeyActionType keyActionType;

			switch (id)
			{
				case 0: keyActionType = KeyActionType.UseFirstAbility; break;
				case 1: keyActionType = KeyActionType.UseSecondAbility; break;
				case 2: keyActionType = KeyActionType.UseThirdAbility; break;
				case 3: keyActionType = KeyActionType.UseFourthAbility; break;
				default: return;
			}

			hotkeyPanel.SetActive(true);
			hotkeyText.text = Keymap.LoadedKeymap.GetAction(keyActionType).key.ToString();
		}

		public void OnClick()
		{
			if (selfAbility)
				selfAbility.StartUse();
		}

		public void OnPointerEnter(PointerEventData pointerEventData)
		{
			UIController.Instance.ProductionHint.ShowForAbility(selfAbility, rectTransform.position + new Vector3(0, rectTransform.sizeDelta.y / 2f + 10));
		}

		public void OnPointerExit(PointerEventData pointerEventData)
		{
			UIController.Instance.ProductionHint.Hide();
		}
	}
}