using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.DebugTools
{
	public sealed class DebugMenu : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] GameObject infoPanel;
		[SerializeField] Transform spawnUnitsPlayersListPanel;
		[SerializeField] Transform spawnUnitsListPanel;
		[SerializeField] Text timeText, infoText;
		[SerializeField] Image selectedPlayerImage;

		[Header("Templates")]
		[SerializeField] Button spawnUnitButtonTemplate;

		DebugMenuState state;
		
		bool isShown;
		UnitData selectedToSpawn;

		byte spawnUnitsPlayerId;

		void Start()
		{
			Hide();
			HideInfoPanel();

			ChangeSpawnPlayer(Player.LocalPlayerId);

			var prodCats = GameController.Instance.MainStorage.availableProductionCategories;

			if (!spawnUnitButtonTemplate || prodCats == null)
				return;

			for (int q = 0; q < prodCats.Count; q++)
			{
				var curCat = prodCats[q];

				for (int w = 0; w < curCat.availableUnits.Count; w++)
				{
					var spawnedButtonObj = Instantiate(spawnUnitButtonTemplate, spawnUnitsListPanel);
					var unit = curCat.availableUnits[w];

					var button = spawnedButtonObj.GetComponent<Button>();

					if (button)
						button.onClick.AddListener(() => SpawnUnit(unit));

					var textTransform = spawnedButtonObj.transform.Find("Text");

					if (textTransform)
					{
						var text = textTransform.GetComponent<Text>();

						if (text)
							text.text = unit.textId;
					}
				}
			}
			
			var playersIngame = GameController.Instance.PlayersController.PlayersIngame;
			for (int q = 0; q < playersIngame.Count; q++)
			{
				var spawnedButtonObj = Instantiate(spawnUnitButtonTemplate, spawnUnitsPlayersListPanel);
				var button = spawnedButtonObj.GetComponent<Button>();
				int playerId = q;

				if (button)
				{
					button.GetComponent<Image>().color = playersIngame[q].Color;
					button.onClick.AddListener(delegate { ChangeSpawnPlayer(playerId); });
					button.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 20);

					var textTransform = spawnedButtonObj.transform.Find("Text");

					if (textTransform)
						Destroy(textTransform.gameObject);
				}
			}
		}

		void Update()
		{
			HandleSpawn();
			HandleDamage();
			
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
			{
				if (!isShown)
					Show();
				else
					Hide();
			}
		}

		void HandleSpawn()
		{
			if (!isShown || state != DebugMenuState.Spawn || EventSystem.current.IsPointerOverGameObject())
				return;

			var hit = Controls.InputHandler.CurrentCursorWorldHit;

			if (Input.GetMouseButtonDown(0))
			{
				NavMesh.SamplePosition(hit.point, out var navHit, 100f, NavMesh.AllAreas);

				SpawnController.SpawnUnit(selectedToSpawn, spawnUnitsPlayerId, navHit.position, Quaternion.identity); 
				SetState(DebugMenuState.None);
				HideInfoPanel();
			}
		}

		void HandleDamage()
		{
			if (!isShown || state != DebugMenuState.Damage || EventSystem.current.IsPointerOverGameObject())
				return;
			
			var hit = Controls.InputHandler.CurrentCursorWorldHit;

			if (Input.GetMouseButtonDown(0) && hit.collider)
			{
				var damageable = hit.collider.GetComponent<Damageable>();

				if (damageable)
					damageable.TakeDamage(50);
			}
		}

		public void SpawnUnit(UnitData unitData)
		{
			SetState(DebugMenuState.Spawn);

			selectedToSpawn = unitData;

			ShowInfoPanel("Select point on map where you want to spawn unit and press LMB on it.");
		}

		public void ChangeDamageState()
		{
			SetState(state != DebugMenuState.Damage ? DebugMenuState.Damage : DebugMenuState.None);
		}

		void SetState(DebugMenuState newState)
		{
			state = newState;

			switch (newState)
			{
				case DebugMenuState.Spawn:
					ShowInfoPanel("Select point on map where you want to spawn unit and press LMB on it.");
					break;
				case DebugMenuState.Damage:
					ShowInfoPanel("Press LMB when cursor on any unit to damage it.");
					break;
				case DebugMenuState.None:
					HideInfoPanel();
					break;
			}
		}

		public void ChangeSpawnPlayer(int newId)
		{
			spawnUnitsPlayerId = (byte)newId;
			
			if (selectedPlayerImage)
				selectedPlayerImage.color = Player.GetPlayerById(spawnUnitsPlayerId).Color;
		}

		public void SetTime(float value)
		{
			Time.timeScale = value;

			if (timeText)
				timeText.text = "x" + value;
		}

		public void Show()
		{
			selfObject.SetActive(true); 
			isShown = true;
		}

		public void Hide()
		{
			selfObject.SetActive(false); 
			isShown = false;
		}

		void ShowInfoPanel(string text)
		{
			infoPanel.SetActive(true);
			infoText.text = text;
		}

		void HideInfoPanel() => infoPanel.SetActive(false);
	}

	public enum DebugMenuState
	{
		None,
		Spawn,
		Damage
	}
}