using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public sealed class CarryCell : MonoBehaviour
	{
		[SerializeField] Image iconImage;

		public void SetActive(bool isEnabled) => gameObject.SetActive(isEnabled);

		public void UpdateState(Unit unitIn)
		{
			if (!unitIn)
			{
				iconImage.enabled = false;
				return;
			}

			iconImage.enabled = true;
			iconImage.sprite = unitIn.Data.icon;
		}
	}
}