using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class UnitInfo : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] Image unitIcon;
		[SerializeField] Text unitName;

		void Start()
		{
			Controls.Selection.UnitSelected += SelectUnit;
			Controls.Selection.SelectionCleared += UnselectUnit;
			UnselectUnit();
		}

		void OnDestroy()
		{
			Controls.Selection.UnitSelected -= SelectUnit;
			Controls.Selection.SelectionCleared -= UnselectUnit;
		}

		public void SelectUnit(Unit unit)
		{
			selfObject.SetActive(true);

			unitName.text = unit.Data.textId;
			unitIcon.sprite = unit.Data.icon;
		}

		public void UnselectUnit() => selfObject.SetActive(false);
	}
}