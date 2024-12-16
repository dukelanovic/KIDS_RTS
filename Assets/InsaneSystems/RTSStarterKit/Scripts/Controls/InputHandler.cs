using InsaneSystems.RTSStarterKit.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.Controls
{
	public sealed class InputHandler : MonoBehaviour
	{
		public static InputHandler SceneInstance { get; private set; }

		/// <summary> Contains current player world cursor hit point, getted by ScreenPointToRay method. </summary>
		public static RaycastHit CurrentCursorWorldHit { get; set; }

		static CustomControls customControlsMode;
		public static HotkeysInputType HotkeysInputMode { get; private set; }

		public string BuildingInputKeys => "qwerasdfyxcv";

		void Awake() => SceneInstance = this;

		void Start()
		{
			HotkeysInputMode = HotkeysInputType.Default;

			Selection.ProductionUnitSelected += OnProductionSelected;
			Selection.SelectionCleared += OnClearSelection;
		}

		void OnProductionSelected(Production production) => HotkeysInputMode = HotkeysInputType.Building;
		void OnClearSelection() => HotkeysInputMode = HotkeysInputType.Default;

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				UIController.Instance.PauseMenu.ShowOrHide();
			
			HandleSelectionInput();
			HandleOrdersInput();
			HandleWorldCursorPosition();
			HandleCustomControls();

			HandleHotkeys();
		}
		
		void OnDestroy()
		{
			Selection.ProductionUnitSelected -= OnProductionSelected;
			Selection.SelectionCleared -= OnClearSelection;
		}
		
		void HandleHotkeys()
		{
			switch (HotkeysInputMode)
			{
				case HotkeysInputType.Default:
					Keymap.LoadedKeymap.CheckAllKeys();
					break;
				
				case HotkeysInputType.Building:
					for (int i = 0; i < BuildingInputKeys.Length; i++)
					{
						if (Input.GetKeyDown(BuildingInputKeys[i].ToString()))
						{
							var icon = UIController.Instance.ProductionIconsPanel.GetIcon(i);
							
							if (icon)
								icon.OnClick();
						}
					}
					break;
			}
		}

		void HandleCustomControls()
		{
			if (customControlsMode == CustomControls.None)
				return;
			
			if (Input.GetMouseButtonDown(1))
				SetCustomControls(CustomControls.None);

			if (Input.GetMouseButtonDown(0) && CurrentCursorWorldHit.collider)
			{
				var unit = CurrentCursorWorldHit.collider.GetComponent<Unit>();

				if (!unit || !unit.Data.isBuilding || !unit.IsOwnedByPlayer(Player.LocalPlayerId))
					return;

				if (customControlsMode == CustomControls.Repair)
				{
					var repair = unit.GetComponent<Repair>();

					if (repair)
						repair.RemoveRepair();
					else
						unit.gameObject.AddComponent<Repair>();
				}
				else if (customControlsMode == CustomControls.Sell)
				{
					var damageable = unit.GetModule<Damageable>();

					if (!damageable)
						return;
					
					var sellPercents = GameController.Instance.MainStorage.buildingSellReturnPercents;
					
					unit.GetOwnerPlayer().AddMoney((int)(unit.Data.price * sellPercents * damageable.GetHealthPercents()));
					
					damageable.Die();
				}
			}
		}

		void HandleWorldCursorPosition()
		{
			var ray = GameController.CachedMainCamera.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out var hit, 10000))
				CurrentCursorWorldHit = hit;
		}

		void HandleSelectionInput()
		{
			if (Build.IsBuildMode)
				return;

			if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				Selection.StartMousePosition = Input.mousePosition;

				Selection.OnStartSelection();
			}

			if (Input.GetMouseButtonUp(0))
			{
				Selection.EndMousePosition = Input.mousePosition;

				if (IsJustClick(Selection.StartMousePosition, Selection.EndMousePosition) && !EventSystem.current.IsPointerOverGameObject())
					Selection.OnSingleSelection();
				else if (Selection.IsSelectionStarted)
					Selection.OnEndSelection();
			}
		}

		static bool IsJustClick(Vector2 positionA, Vector2 positionB) => Vector2.Distance(positionA, positionB) < 5f;

		void HandleOrdersInput()
		{
			if (Selection.SelectedUnits.Count == 0)
				return;

			if (Input.GetMouseButtonUp(1))
				Ordering.GiveOrder(Input.mousePosition, Input.GetKey(KeyCode.LeftShift));
		}

		public void SetCustomControls(CustomControls newControls)
		{
			customControlsMode = customControlsMode != newControls ? newControls : CustomControls.None;

			switch (customControlsMode)
			{
				case CustomControls.Sell: Cursors.SetSellCursor(); break;
				case CustomControls.Repair: Cursors.SetRepairCursor(); break;
				case CustomControls.None:
					Cursors.lockCursorChange = false; 
					Cursors.SetDefaultCursor();
					break;
			}
		}
	}
}