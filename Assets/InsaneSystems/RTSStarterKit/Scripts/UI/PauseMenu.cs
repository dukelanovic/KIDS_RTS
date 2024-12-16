using System;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public sealed class PauseMenu : MonoBehaviour
	{
		public event Action WasShown, WasHidden;
		
		[Tooltip("GameObject of Pause Menu window. Note that this script should not be placed on this window, use other object instead.")]
		[SerializeField] GameObject selfObject;
		
		void Start() => Hide();

		public void ShowOrHide()
		{
			if (selfObject.activeSelf)
				Hide();
			else
				Show();
		}
		
		public void Show()
		{
			if (selfObject.activeSelf)
				return;
			
			selfObject.SetActive(true);
			GameController.SetPauseState(true);
			
			WasShown?.Invoke();
		}
		
		public void Hide()
		{
			if (!selfObject.activeSelf)
				return;
			
			selfObject.SetActive(false);
			GameController.SetPauseState(false);
			
			WasHidden?.Invoke();
		}

		public void ExitToMenu() => GameController.Instance.ReturnToLobby();
	}
}