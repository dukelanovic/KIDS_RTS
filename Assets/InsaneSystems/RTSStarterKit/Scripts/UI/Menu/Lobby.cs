using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.Menus
{
	public sealed class Lobby : MonoBehaviour
	{
		public delegate void OnFreeColorsChanged();
		
		public event OnFreeColorsChanged FreeColorsChanged;
		
		[Comment("LobbyController - entry point to the game from the menu. Shows the user interface with a selection of game match parameters.")]
		[SerializeField] Storage storage;

		[Header("Scene UI")]
		[SerializeField] RectTransform playerEntriesPanel;
		[SerializeField] Button addAiPlayerButton;
		[SerializeField] Text mapNameText, maxPlayersText;
		[SerializeField] Dropdown mapDropdown;
		[SerializeField] Image mapPreviewImage;
		[SerializeField] GameObject loadingScreen;
		[SerializeField] string hotkeySceneName = "HotkeysSettings";

		public List<Color> FreeColors { get; set; }
		public Storage GetStorage => storage;
		
		bool startLoading;

		void Awake()
		{
			Application.targetFrameRate = storage.DesiredFpsCap;
			Time.timeScale = 1f;
		}

		void Start()
		{
			var eventSystem = FindObjectOfType<EventSystem>();

			if (!eventSystem)
			{
				var go = new GameObject("Event System");
				go.AddComponent<EventSystem>();
				go.AddComponent<StandaloneInputModule>();
			}
			
			FreeColors = new List<Color>(storage.availablePlayerColors);
			
			MatchSettings.CurrentMatchSettings = new MatchSettings();

			SetupMapsDropdown();
			ChangeMap(mapDropdown);
		}

		void ResetPlayers()
		{
			for (var i = MatchSettings.CurrentMatchSettings.PlayersSettings.Count - 1; i >= 0 ; i--)
				RemovePlayer(MatchSettings.CurrentMatchSettings.PlayersSettings[i]);

			AddEntryForPlayer();
			AddEntryForPlayer(true, 1);
		}

		void Update()
		{
			if (startLoading)
			{
				SceneManager.LoadScene(MatchSettings.CurrentMatchSettings.SelectedMap.mapSceneName);
				startLoading = false;
			}
		}

		void SetupMapsDropdown()
		{
			mapDropdown.ClearOptions();

			var dropdownOptions = new List<Dropdown.OptionData>();

			for (int i = 0; i < storage.availableMaps.Count; i++)
			{
				if (storage.availableMaps[i] == null)
				{
					Debug.LogWarning("Storage contains empty field in Available Maps, please remove it, now it will be ignored.");
					continue;
				}

				var mapName = storage.availableMaps[i].mapSceneName;
				dropdownOptions.Add(new Dropdown.OptionData(mapName));
			}

			mapDropdown.AddOptions(dropdownOptions);
		}

		void AddEntryForPlayer(bool isAI = false, int team = 0)
		{
			var playerColor = GetFreeColor();
			var playerSettings = new PlayerSettings((byte)team, playerColor, isAI);
			
			if (isAI)
				playerSettings.nickName = "AI Player";

			MatchSettings.CurrentMatchSettings.AddPlayerSettings(playerSettings);

			var spawnedObject = Instantiate(storage.playerEntry, playerEntriesPanel);
			var playerEntry = spawnedObject.GetComponent<PlayerEntry>();

			playerEntry.SetupWithPlayerSettings(playerSettings, this);
			playerSettings.playerLobbyEntry = playerEntry;
			
			TakeColor(playerColor);

			if (MatchSettings.CurrentMatchSettings.PlayersSettings.Count == MatchSettings.CurrentMatchSettings.SelectedMap.maxMapPlayersCount)
				addAiPlayerButton.interactable = false;
		}

		public void RemovePlayer(PlayerSettings playerSettings)
		{
			FreeColor(playerSettings.color);
			MatchSettings.CurrentMatchSettings.RemovePlayerSettings(playerSettings);

			if (MatchSettings.CurrentMatchSettings.PlayersSettings.Count < MatchSettings.CurrentMatchSettings.SelectedMap.maxMapPlayersCount)
				addAiPlayerButton.interactable = true;
			
			if (playerSettings.playerLobbyEntry)
				Destroy(playerSettings.playerLobbyEntry.gameObject);
		}

		public void RemovePlayer(byte id)
		{
			var playerSettings = MatchSettings.CurrentMatchSettings.PlayersSettings[id];
			RemovePlayer(playerSettings);
		}

		public void StartGame()
		{
			loadingScreen.SetActive(true);
			startLoading = true;
		}

		public void AddAIPlayer() => AddEntryForPlayer(true);

		public Color GetFreeColor() => FreeColors[0];

		void TakeColor(Color color, bool callEvent = true)
		{
			FreeColors.Remove(color);

			if ( callEvent)
				FreeColorsChanged?.Invoke();
		}

		void FreeColor(Color color, bool callEvent = true)
		{
			FreeColors.Add(color);

			if (callEvent)
				FreeColorsChanged?.Invoke();
		}

		public void PlayerChangeColor(PlayerSettings playerSettings, Color newColor)
		{
			FreeColor(playerSettings.color, false);
			playerSettings.color = newColor;
			TakeColor(newColor);
		}

		public List<Color> GetFreeColorsForPlayer(PlayerSettings playerSettings)
		{
			var colorsList = new List<Color>(FreeColors);

			if (!colorsList.Contains(playerSettings.color))
				colorsList.Add(playerSettings.color);

			return colorsList;
		}

		public byte GetPlayerSettingsIndex(PlayerSettings playerSettings)
		{
			return (byte)MatchSettings.CurrentMatchSettings.PlayersSettings.LastIndexOf(playerSettings);
		}

		public void ChangeMap(Dropdown mapDropdownSender)
		{
			var map = storage.GetMapBySceneName(mapDropdownSender.captionText.text);
			MatchSettings.CurrentMatchSettings.SelectMap(map);
			
			ResetPlayers();
			
			mapPreviewImage.sprite = map.mapPreviewImage;

			mapNameText.text = "Map: " + map.mapSceneName;
			
			if (maxPlayersText)
				maxPlayersText.text = "Max players: " + map.maxMapPlayersCount;
		}

		public void OpenHotkeysSettings() => SceneManager.LoadScene(hotkeySceneName);
	}
}