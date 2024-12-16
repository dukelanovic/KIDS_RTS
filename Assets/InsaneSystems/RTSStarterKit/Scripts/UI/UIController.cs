using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class UIController : MonoBehaviour
	{
		public static UIController Instance { get; protected set; }

		[SerializeField] Canvas mainCanvas;
		[SerializeField] Text moneyText;
		[SerializeField] GameObject winScreen;
		[SerializeField] GameObject loseScreen;

		public Minimap Minimap { get; protected set; }
		public ProductionHint ProductionHint { get; protected set; }
		public SelectProductionNumberPanel SelectProductionNumberPanel { get; protected set; }
		public MinimapSignal MinimapSignal { get; protected set; }
		public CarryingUnitList CarryingUnitList { get; protected set; }
		public PauseMenu PauseMenu { get; protected set; }
		public UnitAbilities UnitAbilities { get; protected set; }
		public ProductionIconsPanel ProductionIconsPanel { get; protected set; }
		
		public Canvas MainCanvas => mainCanvas;
		
		SelectProductionTypePanel selectProductionPanel;

		void Awake()
		{
			Instance = this;
			
			Minimap = GetComponent<Minimap>();
			ProductionHint = GetComponent<ProductionHint>();
			selectProductionPanel = GetComponent<SelectProductionTypePanel>();
			SelectProductionNumberPanel = GetComponent<SelectProductionNumberPanel>();
			MinimapSignal = GetComponent<MinimapSignal>();
			CarryingUnitList = GetComponent<CarryingUnitList>();
			PauseMenu = GetComponent<PauseMenu>();
			UnitAbilities = GetComponent<UnitAbilities>();
			ProductionIconsPanel = GetComponent<ProductionIconsPanel>();
		}

		protected virtual void Start()
		{
			Unit.UnitDestroyed += Healthbar.RemoveHealthbarOfUnit;
			Unit.UnitHovered += OnUnitHovered;
			Unit.UnitUnhovered += OnUnitUnhovered;
			
			Controls.Selection.UnitSelected += Healthbar.SpawnHealthbarForUnit;
			Controls.Selection.SelectionCleared += Healthbar.RemoveAllHealthbars;

			OnPlayerMoneyChanged(GameController.Instance.PlayersController.PlayersIngame[Player.LocalPlayerId].Money);
			
			Player.LocalPlayer.MoneyChanged += OnPlayerMoneyChanged;
			Player.LocalPlayer.WasDefeated += OnLocalPlayerDefeated;
			
			GameController.Instance.MatchStarted += OnGameInitialized;
			GameController.Instance.MatchFinished += OnMatchFinished;
		}

		protected virtual void OnDestroy()
		{
			Unit.UnitDestroyed -= Healthbar.RemoveHealthbarOfUnit;
			Unit.UnitHovered -= Healthbar.SpawnHealthbarForUnit;
			Unit.UnitUnhovered -= Healthbar.RemoveHealthbarOfUnit;
			

			Controls.Selection.UnitSelected -= Healthbar.SpawnHealthbarForUnit;
			Controls.Selection.SelectionCleared -= Healthbar.RemoveAllHealthbars;

			if (GameController.Instance)
			{
				GameController.Instance.MatchStarted -= OnGameInitialized;
				GameController.Instance.MatchFinished -= OnMatchFinished;
			}

			if (Player.LocalPlayer != null)
			{
				Player.LocalPlayer.MoneyChanged -= OnPlayerMoneyChanged;
				Player.LocalPlayer.WasDefeated -= OnLocalPlayerDefeated;
			}
		}

		void OnMatchFinished(int winnerTeamId)
		{
			if (!Player.LocalPlayer.IsDefeated)
				ShowWinScreen();
		}
		
		protected virtual void OnLocalPlayerDefeated()
		{
			ShowDefeatScreen();
		}

		void OnUnitHovered(Unit unit)
		{
			if (unit.IsSelected || !unit.GetModule<FogOfWarModule>().IsVisibleInFOW)
				return;
			
			Healthbar.SpawnHealthbarForUnit(unit);
		}
		
		void OnUnitUnhovered(Unit unit)
		{
			if (unit.IsSelected)
				return;
			
			Healthbar.RemoveHealthbarOfUnit(unit);
		}
		
		public void OnGameInitialized()
		{
			selectProductionPanel.OnSelectButtonClick(GameController.Instance.MainStorage.availableProductionCategories[0]);
		}

		public void ShowWinScreen() => winScreen.SetActive(true);
		public void ShowDefeatScreen() => loseScreen.SetActive(true);

		protected virtual void OnPlayerMoneyChanged(int moneyValue) => moneyText.text = "$" + moneyValue;
	}
}