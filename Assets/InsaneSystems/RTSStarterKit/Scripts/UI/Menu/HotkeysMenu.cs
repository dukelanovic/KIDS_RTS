using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InsaneSystems.RTSStarterKit.Menus
{
	public sealed class HotkeysMenu : MonoBehaviour
	{
		[SerializeField] GameObject pressAnyKeyText;
		[SerializeField] string backScene = "Lobby";
		
		HotkeySelectEntry[] hotkeyEntries;

		void Start()
		{
			pressAnyKeyText.SetActive(false);
			hotkeyEntries = FindObjectsOfType<HotkeySelectEntry>();
		}

		public void SetPressTextState(bool isEnabled) => pressAnyKeyText.SetActive(isEnabled);

		public void BackToLobby() => SceneManager.LoadScene(backScene);

		public void ResetToDefault()
		{
			Keymap.LoadedKeymap.SetupDefaultScheme();

			for (var i = 0; i < hotkeyEntries.Length; i++)
				hotkeyEntries[i].Reload();
		}
	}
}