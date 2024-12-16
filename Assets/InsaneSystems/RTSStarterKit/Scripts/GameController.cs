using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InsaneSystems.RTSStarterKit
{
	[RequireComponent(typeof(PlayersController))]
	public sealed class GameController : MonoBehaviour
	{
		public static GameController Instance { get; private set; }
		public static Camera CachedMainCamera { get; private set; }
		public static bool IsGamePaused { get; private set; }

		public event Action MatchStarted;
		public event Action<int> MatchFinished;
		
		[SerializeField] Storage storage;
		[SerializeField] AI.AISettings defaultAIPreset;
		[SerializeField] MapSettings mapSettings;
		[SerializeField] Renderer mapBorderRenderer;

		public PlayersController PlayersController { get; private set; }	
		public Controls.Build Build { get; private set; }	
		public Controls.CameraMover CameraMover { get; private set; }
		public TextsLibrary TextsLibrary { get; private set; }

		public Storage MainStorage => storage;
		public MapSettings MapSettings => mapSettings;

		bool isGameInitialized, isAiInitialized;

		void Awake()
		{
			Instance = this;

			PlayersController = GetComponent<PlayersController>();
			Build = GetComponent<Controls.Build>();
			
			CameraMover = FindObjectOfType<Controls.CameraMover>();
			CachedMainCamera = Camera.main;
			
			PlayersController.PreAwake();

			if (mapSettings != null && mapSettings.isThisMapForSingleplayer)
			{
				MatchSettings.CurrentMatchSettings = new MatchSettings
				{
					PlayersSettings = mapSettings.playersSettingsForSingleplayer
				};
				
				MatchSettings.CurrentMatchSettings.SelectMap(mapSettings);
			}
			else if (MatchSettings.CurrentMatchSettings == null)
			{
				Debug.LogWarning("<b>You can run non-singleplayer map only from Lobby.</b> To test map correctly - save it, open Lobby scene, select players, and run map.");
				SceneManager.LoadScene(0);
				return;
			}

			if (mapSettings == null)
				mapSettings = MatchSettings.CurrentMatchSettings.SelectedMap;

			if (!mapSettings.winCondition)
				mapSettings.winCondition = storage.defaultWinCondition;
			
			Controls.Selection.Initialize();
			ResourcesField.Init();
			Refinery.Init();

			Unit.Init();

			UI.Healthbar.ResetPool();
			UI.HarvesterBar.Init();

			InitializePlayers();

			if (mapBorderRenderer)
			{
				mapBorderRenderer.material.SetInt("_MapSize", MatchSettings.CurrentMatchSettings.SelectedMap.mapSize);

				if (!MainStorage.showMapBorders)
					mapBorderRenderer.enabled = false;
			}

			TextsLibrary = storage.textsLibrary;
			
			if (!TextsLibrary)
				Debug.LogWarning("<b>No Texts Library added to the Storage.</b> Please, add it, otherwise possible game texts problems.");
			
			Unit.UnitDestroyed += OnUnitDestroyed;
		}

		void OnUnitDestroyed(Unit unit) => CheckWinConditions();
		
		void OnDestroy()
		{
			Instance = null;
			
			Unit.UnitDestroyed -= OnUnitDestroyed;
		}

		void Update()
		{
			if (!isGameInitialized)
			{
				isGameInitialized = true;

				if (!isAiInitialized)
					InitializeAI();

				MatchStarted?.Invoke();

				return;
			}

			Controls.Selection.Update();
		}

		void InitializePlayers()
		{
			SpawnController.InitializeStartPoints();

			for (int i = 0; i < MatchSettings.CurrentMatchSettings.PlayersSettings.Count; i++)
			{
				var curPlySettings = MatchSettings.CurrentMatchSettings.PlayersSettings[i];

				var player = new Player(curPlySettings.color, curPlySettings);

				player.SetMoney(mapSettings.isThisMapForSingleplayer
					? curPlySettings.startMoneyForSingleplayer
					: MainStorage.startPlayerMoney);

				PlayersController.AddPlayer(player);				
			}

			if (MapSettings.autoSpawnPlayerStabs)
				for (int i = 0; i < PlayersController.PlayersIngame.Count; i++)
					SpawnController.SpawnPlayerStab((byte)i);
		}

		void InitializeAI()
		{
			for (int i = 0; i < PlayersController.PlayersIngame.Count; i++)
			{
				if (PlayersController.PlayersIngame[i].IsAIPlayer)
				{
					var aiObject = Instantiate(MainStorage.AiControllerPrefab);
					aiObject.name = "AI Controller " + i;
					
					var aiController = aiObject.CheckComponent<AI.AIController>(true);
					aiController.SetupWithAISettings(defaultAIPreset);
					aiController.SetupAIForPlayer((byte)i);
				}
			}

			isAiInitialized = true;
		}

		public void CheckWinConditions()
		{
			if (mapSettings.winCondition.CheckCondition(out var winnerTeam))
				MatchFinished?.Invoke(winnerTeam);
		}

		public void ReturnToLobby()
		{
			Cursors.SetDefaultCursor();
			SceneManager.LoadScene("Lobby");
		}

		public static void SetPauseState(bool isPaused)
		{
			IsGamePaused = isPaused;
			
			Time.timeScale = IsGamePaused ? 0f : 1f;
		}
	}
}