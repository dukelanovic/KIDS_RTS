using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Controls
{
	public static class Selection
	{
		public delegate void SelectionAction();
		public delegate void UnitSelectionAction(Unit unit);
		public delegate void MultiSelectionAction(List<Unit> units);
		public delegate void ProductionSelectionAction(Production productionModule);
		
		public static readonly List<Unit> SelectedUnits = new List<Unit>();
		
		public static event SelectionAction SelectionStarted, SelectionEnded, SelectionCleared;
		public static event UnitSelectionAction UnitSelected;
		public static event MultiSelectionAction UnitsListWasSelected, UnitsListWasChanged;
		public static event ProductionSelectionAction ProductionUnitSelected;

		public static Vector2 StartMousePosition { get; set; }
		public static Vector2 EndMousePosition{ get; set; }

		public static bool IsSelectionStarted { get; private set; }

		static float doubleClickTimer;

		static int unitLayerMask;
		
		/// <summary> Number of next unit for select alternative </summary>
		static int unitAlternativeNumber;

		static readonly List<KeyTimer> groupsKeyTimers = new List<KeyTimer>();

		static readonly KeyCode keyToMultipleSelection;

		static Selection()
		{
			keyToMultipleSelection = Keymap.LoadedKeymap.GetAction(KeyActionType.AddToSelectionHoldKey).key;
		}
		
		public static void Initialize()
		{
			unitLayerMask = Globals.LayermaskUnit;
			
			SelectedUnits.Clear();
			IsSelectionStarted = false;

			Damageable.GlobalDied -= UnitDiedAction;
			Damageable.GlobalDied += UnitDiedAction;

			InitializeHotkeys();
		}

		public static void OnStartSelection()
		{
			StartMousePosition = Input.mousePosition;
			IsSelectionStarted = true;

			SelectionStarted?.Invoke();
		}

		public static void OnSingleSelection()
		{
			if (!Input.GetKey(keyToMultipleSelection) || SelectedUnits.Count > 0 && SelectedUnits[0] && SelectedUnits[0].Data.isBuilding)
				OnClearSelection();

			var ray = GameController.CachedMainCamera.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out var hit, 1000, unitLayerMask))
			{
				var unit = hit.collider.GetComponent<Unit>();

				if (!unit || !unit.IsOwnedByPlayer(Player.LocalPlayerId))
				{
					CancelOrEndSelection();
					return;
				}

				OnUnitAddToSelection(unit, true);

				if (doubleClickTimer > 0)
				{
					SelectUnitsSameType(unit);
					doubleClickTimer = 0f;
				}
			}

			unitAlternativeNumber = 0;
			doubleClickTimer = 0.3f;
			CancelOrEndSelection();
		}

		static void SelectUnitsSameType(Unit unit, bool onlyOnScreen = true)
		{
			var isFirstUnitSelected = false;
			for (int i = 0; i < Unit.AllUnits.Count; i++)
			{
				var secondUnit = Unit.AllUnits[i];
				if (secondUnit.OwnerPlayerId == unit.OwnerPlayerId && secondUnit.Data == unit.Data &&
				    (!onlyOnScreen || secondUnit.IsVisibleInViewport())) // check is it on screen
				{
					OnUnitAddToSelection(Unit.AllUnits[i], !isFirstUnitSelected);
					isFirstUnitSelected = true;
				}
			}
		}
		
		static void CancelOrEndSelection()
		{
			IsSelectionStarted = false;

			SelectionEnded?.Invoke();
		}

		public static void OnEndSelection()
		{
			OnClearSelection(); // is it needed here?

			float minX, minY, maxX, maxY;
			if (StartMousePosition.x < EndMousePosition.x)
			{
				minX = StartMousePosition.x;
				maxX = EndMousePosition.x;
			}
			else
			{
				minX = EndMousePosition.x;
				maxX = StartMousePosition.x;
			}
			if (StartMousePosition.y < EndMousePosition.y)
			{
				minY = StartMousePosition.y;
				maxY = EndMousePosition.y;
			}
			else
			{
				minY = EndMousePosition.y;
				maxY = StartMousePosition.y;
			}

			var rect = new Rect
			{
				min = new Vector2(minX, minY),
				max = new Vector2(maxX, maxY)
			};

			var isFirstUnitSelected = false;
			for (int i = 0; i < Unit.AllUnits.Count; i++)
			{
				var unit = Unit.AllUnits[i];

				if (!unit.IsOwnedByPlayer(Player.LocalPlayerId) || unit.Data.isBuilding || unit.IsBeingCarried)
					continue;

				Vector2 screenPos = GameController.CachedMainCamera.WorldToScreenPoint(unit.transform.position);

				if (rect.Contains(screenPos))
				{
					OnUnitAddToSelection(unit, !isFirstUnitSelected);
					isFirstUnitSelected = true;
				}
			}

			IsSelectionStarted = false;

			if (SelectedUnits.Count > 1 && UnitsListWasSelected != null)
				UnitsListWasSelected(SelectedUnits);

			if (SelectionEnded != null)
				SelectionEnded();
		}

		public static void OnUnitAddToSelection(Unit unit, bool isSoundNeeded = false)
		{
			if (SelectedUnits.Contains(unit))
				return;

			if (unit.IsBeingCarried) // we cant select carried units
				return;
			
			SelectedUnits.Add(unit);

			unit.Select(isSoundNeeded);

			if (unit.Production && ProductionUnitSelected != null)
				ProductionUnitSelected(unit.Production);

			UnitSelected?.Invoke(unit);

			if (SelectedUnits.Count > 1 && UnitsListWasSelected != null)
				UnitsListWasSelected(SelectedUnits);
		}

		static void UnitDiedAction(Unit unit) => UnselectUnit(unit);

		public static void UnselectUnit(Unit unit)
		{
			unit.Unselect();
			SelectedUnits.Remove(unit);

			if (SelectedUnits.Count == 0)
				SelectionCleared?.Invoke();
		
			UnitsListWasChanged?.Invoke(SelectedUnits);
		}

		public static void OnClearSelection()
		{
			if (Input.GetKey(keyToMultipleSelection))
				return;

			for (int i = 0; i < SelectedUnits.Count; i++)
				SelectedUnits[i].Unselect();

			SelectedUnits.Clear();

			SelectionCleared?.Invoke();
		}

		// RTS Kit - todo I think this is not player for this initialization. It should be moved to input or something
		static void InitializeHotkeys()
		{
			Keymap.LoadedKeymap.GetAction(KeyActionType.SelectSameUnitsOnScreen).WasPressed +=
				OnPressSelectAllSameUnitsOnScreen;
			Keymap.LoadedKeymap.GetAction(KeyActionType.SelectSameUnitsOnScreen).WasDoublePressed +=
				OnPressSelectAllSameUnits;
			
			Keymap.LoadedKeymap.GetAction(KeyActionType.SelectAllUnitsOnScreen).WasPressed += OnPressSelectAllUnitsOnScreen;
			Keymap.LoadedKeymap.GetAction(KeyActionType.SelectAllUnits).WasPressed += OnPressSelectAllUnits;
			Keymap.LoadedKeymap.GetAction(KeyActionType.StopOrder).WasPressed += OnHotkeyStopOrder;
			
			Keymap.LoadedKeymap.GetAction(KeyActionType.UnitsMoveWithLowestSpeed).WasPressed += OnPressMoveOnLowestSpeed;
			Keymap.LoadedKeymap.GetAction(KeyActionType.MoveToBase).WasPressed += OnPressMoveToBase;
			
			Keymap.LoadedKeymap.GetAction(KeyActionType.SelectUnitAlternative).WasPressed += OnPressSelectUnitAlternative;
			Keymap.LoadedKeymap.GetAction(KeyActionType.SelectHarvesterAlternative).WasPressed += OnPressSelectHarvesterAlternative;
			
			Keymap.LoadedKeymap.GetAction(KeyActionType.LockUnitMovement).WasPressed += OnPressLockMovement;
			Keymap.LoadedKeymap.GetAction(KeyActionType.DisperseUnitsToCorners).WasPressed += OnPressDisperseUnits;

			Keymap.LoadedKeymap.GetAction(KeyActionType.UseFirstAbility).WasPressed += delegate { OnPressUseAbility(0); };
			Keymap.LoadedKeymap.GetAction(KeyActionType.UseSecondAbility).WasPressed += delegate { OnPressUseAbility(1); };
			Keymap.LoadedKeymap.GetAction(KeyActionType.UseThirdAbility).WasPressed += delegate { OnPressUseAbility(2); };
			Keymap.LoadedKeymap.GetAction(KeyActionType.UseFourthAbility).WasPressed += delegate { OnPressUseAbility(3); };
		}
		
		static void OnPressSelectAllSameUnits() => SelectAllSameUnitsForCurrent(false);
		static void OnPressSelectAllSameUnitsOnScreen() => SelectAllSameUnitsForCurrent(true);
		
		static void OnPressSelectUnitAlternative() => SelectUnitAlternative(true);
		static void OnPressSelectHarvesterAlternative() => SelectHarvesterAlternative(true);

		static void OnPressDisperseUnits()
		{
			if (SelectedUnits.Count == 0)
				return;

			var currentGroupCenterPoint = Vector3.zero;

			int aliveUnitsCount = SelectedUnits.Count;

			for (int i = 0; i < SelectedUnits.Count; i++)
				if (SelectedUnits[i])
					currentGroupCenterPoint += SelectedUnits[i].transform.position;
				else
					aliveUnitsCount--;

			currentGroupCenterPoint /= aliveUnitsCount;
			
			for (int i = 0; i < SelectedUnits.Count; i++)
			{
				currentGroupCenterPoint.y = SelectedUnits[i].transform.position.y;
				var destination = (SelectedUnits[i].transform.position - currentGroupCenterPoint).normalized * 3f;
	
				var order = new MovePositionOrder();

				bool foundPoint = NavMesh.SamplePosition(SelectedUnits[i].transform.position + destination, out var hit, 10f, NavMesh.AllAreas);
				order.MovePosition = hit.position;

				if (foundPoint)
					SelectedUnits[i].AddOrder(order, false);
			}
		}
		
		//static void OnPressSelectUnitAlternativeNoFocus() { SelectUnitAlternative(false); }
		
		static void OnPressLockMovement()
		{
			for (var i = 0; i < SelectedUnits.Count; i++)
			{
				SelectedUnits[i].IsMovementLockedByHotkey = !SelectedUnits[i].IsMovementLockedByHotkey;
				var movable = SelectedUnits[i].GetModule<Movable>();
				
				if (movable)
					movable.Stop();
			}
		}

		static void OnPressMoveOnLowestSpeed()
		{
			var lowestSpeed = 300f;
			var useCustomSpeed = true;
			
			for (int i = 0; i < SelectedUnits.Count; i++)
			{
				var moveModule = SelectedUnits[i].GetModule<Movable>();

				if (moveModule && moveModule.UseCustomSpeed)
				{
					useCustomSpeed = false;
					break;
				}
				
				lowestSpeed = Mathf.Min(SelectedUnits[i].Data.moveSpeed, lowestSpeed);
			}

			for (int i = 0; i < SelectedUnits.Count; i++)
			{
				var moveModule = SelectedUnits[i].GetModule<Movable>();

				if (moveModule)
					moveModule.SetCustomSpeed(lowestSpeed, useCustomSpeed);
			}
		}

		static void OnPressMoveToBase()
		{
			if (Player.LocalPlayer.OwnedProductionBuildings.Count < 1)
				return;
					
			var order = new MovePositionOrder
			{
				MovePosition = Player.LocalPlayer.OwnedProductionBuildings[0].transform.position
			};

			for (int i = 0; i < SelectedUnits.Count; i++)
				SelectedUnits[i].AddOrder(order, false);
		}

		static void OnPressUseAbility(int id)
		{
			for (int i = 0; i < SelectedUnits.Count; i++)
			{
				if (!SelectedUnits[i])
					continue;

				var abilitiesModule = SelectedUnits[i].GetComponent<AbilitiesModule>();

				if (abilitiesModule)
				{
					var ability = abilitiesModule.GetAbilityById(id);

					if (ability)
						ability.StartUse();
				}
			}
		}

		static void SelectAllSameUnitsForCurrent(bool onlyOnScreen)
		{
			if (SelectedUnits.Count == 0)
				return;
			
			var unitTypes = new List<Unit>();
			
			for (int i = 0; i < SelectedUnits.Count; i++)
				if (!unitTypes.Find(u => u.Data == SelectedUnits[i].Data))
					unitTypes.Add(SelectedUnits[i]);

			for (int i = 0; i < unitTypes.Count; i++)
				SelectUnitsSameType(unitTypes[i], onlyOnScreen);
		}
		
		static void SelectUnitAlternative(bool focus)
		{
			if (SelectedUnits.Count == 0 || SelectedUnits.Count > 1)
				return;

			SelectUnitAlternative(SelectedUnits[0].Data, focus);
		}
		
		static void SelectUnitAlternative(UnitData unitType, bool focus)
		{
			var selectedUnit = SelectedUnits[0];
			int unitId = 0;

			var unitsOfThisType = Unit.AllUnits.FindAll(u =>
				u && u.OwnerPlayerId == selectedUnit.OwnerPlayerId && u.Data == selectedUnit.Data);

			for (int i = 0; i < unitsOfThisType.Count; i++)
			{
				var secondUnit = unitsOfThisType[i];

				if (unitId >= unitAlternativeNumber)
				{
					OnClearSelection();
					OnUnitAddToSelection(secondUnit, true);
					unitAlternativeNumber++;

					if (focus)
						GameController.Instance.CameraMover.SetPosition(secondUnit.transform.position);

					break;
				}

				unitId++;

				if (unitAlternativeNumber >= unitId && i == unitsOfThisType.Count - 1)
				{
					unitAlternativeNumber = 0;
					OnClearSelection();
					OnUnitAddToSelection(secondUnit, true);

					if (focus)
						GameController.Instance.CameraMover.SetPosition(secondUnit.transform.position);
				}
			}
		}

		static void SelectHarvesterAlternative(bool focus)
		{
			int unitId = 0;
			
			var unitsOfThisType = Unit.AllUnits.FindAll(u =>
				u && u.OwnerPlayerId == Player.LocalPlayerId && u.Data.isHarvester);

			for (int i = 0; i < unitsOfThisType.Count; i++)
			{
				var unit = unitsOfThisType[i];

				if (unitId >= unitAlternativeNumber)
				{
					OnClearSelection();
					OnUnitAddToSelection(unit, true);
					unitAlternativeNumber++;

					if (focus)
						GameController.Instance.CameraMover.SetPosition(unit.transform.position);

					break;
				}

				unitId++;

				if (unitAlternativeNumber >= unitId && i == unitsOfThisType.Count - 1)
				{
					unitAlternativeNumber = 0;
					OnClearSelection();
					OnUnitAddToSelection(unit, true);

					if (focus)
						GameController.Instance.CameraMover.SetPosition(unit.transform.position);
				}
			}
		}

		static void OnPressSelectAllUnitsOnScreen() => SelectAllUnits(true);
		static void OnPressSelectAllUnits() => SelectAllUnits(false);

		static void SelectAllUnits(bool onlyOnScreen = true)
		{
			var isFirstUnitSelected = false;
			
			for (int i = 0; i < Unit.AllUnits.Count; i++)
			{
				var unit = Unit.AllUnits[i];
				
				if (!unit || unit.Data.isBuilding || unit.Data.isHarvester || unit.OwnerPlayerId != Player.LocalPlayerId)
					continue;

				if (unit.IsVisibleInViewport() || !onlyOnScreen)
				{
					OnUnitAddToSelection(Unit.AllUnits[i], !isFirstUnitSelected);
					isFirstUnitSelected = true;
				}
			}
		}

		static void OnHotkeyStopOrder()
		{
			for (int i = 0; i < SelectedUnits.Count; i++)
				SelectedUnits[i].EndCurrentOrder();
		}

		/// <summary>Call this method from GameController's update</summary>
		public static void Update()
		{
			GroupsWork();
			
			doubleClickTimer -= Time.deltaTime;

			for (int i = groupsKeyTimers.Count - 1; i >= 0; i--)
			{
				groupsKeyTimers[i].Tick();

				if (groupsKeyTimers[i].IsFinished())
					groupsKeyTimers.RemoveAt(i);
			}
		}

		static void GroupsWork()
		{
			for (int i = 0; i <= 9; i++)
			{
				if (!Input.GetKeyDown(i.ToString()))
					continue;

				var keyToHold = Application.isEditor
					? KeyCode.LeftShift
					: Keymap.LoadedKeymap.GetAction(KeyActionType.GroupControlsHoldKey).key;

				if (Input.GetKey(keyToHold))
				{
					for (int w = 0; w < Unit.AllUnits.Count; w++)
						if (Unit.AllUnits[w] && Unit.AllUnits[w].UnitSelectionGroup == i)
							Unit.AllUnits[w].SetUnitSelectionGroup(-1);

					for (int w = 0; w < SelectedUnits.Count; w++)
						if (SelectedUnits[w])
							SelectedUnits[w].SetUnitSelectionGroup(i);
				}
				else
				{
					OnClearSelection();

					var wasFocused = false;
					var isKeyWasPressedNearly = groupsKeyTimers.Find(gkt => gkt.NumberKeyId == i) != null;

					var isFirstUnitSelected = false;
					for (int w = 0; w < Unit.AllUnits.Count; w++)
						if (Unit.AllUnits[w] && Unit.AllUnits[w].UnitSelectionGroup == i)
						{
							if (!wasFocused && isKeyWasPressedNearly)
							{
								GameController.Instance.CameraMover.SetPosition(Unit.AllUnits[w].transform.position);
								wasFocused = true; 
							}

							OnUnitAddToSelection(Unit.AllUnits[w], isFirstUnitSelected);
							isFirstUnitSelected = true;
						}

					if (!isKeyWasPressedNearly)
						groupsKeyTimers.Add(new KeyTimer(i));
				}
			}
		}
	}

	/// <summary>This class represents timer which helps to check doublepress of some key.</summary>
	public sealed class KeyTimer
	{
		public int NumberKeyId { get; }
		public float TimeLeft { get; private set; }
		
		public KeyTimer(int keyNumber, float time = 0.4f)
		{
			NumberKeyId = keyNumber;
			TimeLeft = time;
		}

		public void Tick() => TimeLeft -= Time.deltaTime;
		public bool IsFinished() => TimeLeft <= 0;
	}
}