using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	/// <summary>
	/// This class describes component, which allows to draw more info above selected unit (like healthbar etc).
	/// Instantiates when healthbar being instantiated.
	/// </summary>
	public class UnitAboveInfo : MonoBehaviour
	{
		[SerializeField] Text selectionGroupText;
		[SerializeField] GameObject lockedIconObject;
		[SerializeField] RectTransform carryCellsPanel;
		
		readonly List<CarryCell> carryCells = new List<CarryCell>();

		Unit selfUnit;
		float updateTimer;

		Healthbar selfHealthbar;

		int totalCarryCellsCount;
		
		void Awake()
		{
			selectionGroupText.enabled = false;
			selfHealthbar = GetComponent<Healthbar>();
			
			totalCarryCellsCount = GameController.Instance.MainStorage.carriedUnitsIconsCount;

			var carryCellTemplate = GameController.Instance.MainStorage.carryCellTemplate;
			for (var i = 0; i < totalCarryCellsCount; i++)
			{
				var spawnedCell = Instantiate(carryCellTemplate, carryCellsPanel);
				carryCells.Add(spawnedCell.GetComponent<CarryCell>());
				carryCells[i].SetActive(false);
			}

			lockedIconObject.SetActive(false);
		}

		void Update()
		{
			updateTimer -= Time.deltaTime;

			if (updateTimer <= 0)
			{
				if (selfHealthbar && (!selfUnit || selfUnit != selfHealthbar.Damageable.SelfUnit))
					SetupWithUnit(selfHealthbar.Damageable.SelfUnit);

				lockedIconObject.SetActive(selfUnit.IsMovementLockedByHotkey);
				UpdateText();
				UpdateCarryCells();
				updateTimer = 0.2f;
			}
		}

		public void SetupWithUnit(Unit unit)
		{
			selfUnit = unit;

			UpdateText();
			UpdateCarryCells();
		}

		void UpdateText()
		{
			if (!selfUnit)
			{ 
				selectionGroupText.enabled = false;
				return;
			}

			if (selfUnit.UnitSelectionGroup > -1)
			{
				selectionGroupText.enabled = true;
				selectionGroupText.text = selfUnit.UnitSelectionGroup.ToString();
			}
			else
			{
				selectionGroupText.enabled = false;
			}
		}

		void UpdateCarryCells()
		{
			var carrierModule = selfUnit.GetModule<CarryModule>();
			
			for (var i = 0; i < totalCarryCellsCount; i++)
			{
				if (!selfUnit.IsOwnedByPlayer(Player.LocalPlayerId))
				{
					carryCells[i].SetActive(false);
					continue;
				}
				
				carryCells[i].SetActive(selfUnit.Data.canCarryUnitsCount > i);
				
				if (!carrierModule)
				{
					carryCells[i].UpdateState(null);
					continue;
				}

				var units = carrierModule.GetCarryingUnits();
				carryCells[i].UpdateState(units.Count > i ? units[i] : null);
			}
		}
	}
}