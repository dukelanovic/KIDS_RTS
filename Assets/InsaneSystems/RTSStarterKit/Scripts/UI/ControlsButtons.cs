using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ControlsButtons : MonoBehaviour
	{
		[SerializeField] Button repairButton, sellButton;
		
		void Start()
		{
			if (repairButton)
				repairButton.onClick.AddListener(() => InputHandler.SceneInstance.SetCustomControls(CustomControls.Repair));
			
			if (sellButton)
				sellButton.onClick.AddListener(() => InputHandler.SceneInstance.SetCustomControls(CustomControls.Sell));
		}
	}
}