using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public sealed class ElectricityBar : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] Image electricityFill;
		[SerializeField] Image usedElectricityFill;

		bool isElectricityUsed;

		void Start()
		{
			isElectricityUsed = GameController.Instance.MainStorage.isElectricityUsedInGame;

			selfObject.SetActive(isElectricityUsed);

			if (isElectricityUsed)
				Player.LocalPlayer.ElectricityChanged += OnElectricityChanged;
		}

		void OnDestroy()
		{
			if (isElectricityUsed && Player.LocalPlayer != null)
				Player.LocalPlayer.ElectricityChanged -= OnElectricityChanged;
		}
		
		void OnElectricityChanged(int totalElectricity, int usedElectricity)
		{
			electricityFill.fillAmount = totalElectricity / 100f;
			usedElectricityFill.fillAmount = usedElectricity / 100f;
		}
	}
}