using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Abilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This module allows unit to carry other units on board. </summary>
	[DisallowMultipleComponent]
	public class CarryModule : Module
	{
		public delegate void OnCarryStateChanged(bool isCarried);
		public delegate void UnitsExitAction();
		public delegate void UnitExitAction(Unit unit);
		
		public static float CarryingUnitsDamageOnDieCoef { get; set; } = 0.5f;
		
		public event OnCarryStateChanged CarryStateChanged;
		public event UnitsExitAction AllUnitsExited;
		public event UnitExitAction UnitExited;
		
		readonly List<Unit> carryingUnits = new List<Unit>();
		
		[SerializeField] Ability carryOutAbility;
		
		/// <summary> List of units which will be taken on board by this carrier when reach take distance.</summary>
		readonly List<Unit> unitsToTake = new List<Unit>();
		readonly List<Vector3> randomedOffsets = new List<Vector3>();
		
		float unitMaxSize;

		void Start()
		{
			SelfUnit.GetModule<AbilitiesModule>().CheckForAbility(carryOutAbility);
			
			for (var i = 0; i < SelfUnit.Data.canCarryUnitsCount; i++)
			{
				var randomedX = Mathf.Sin(Random.Range(-1f, 1f) * Mathf.PI) * 0.2f;
				var randomedZ = Mathf.Cos(Random.Range(-1f, 1f) * Mathf.PI) * 0.2f;

				randomedOffsets.Add(new Vector3(randomedX, 0f, randomedZ));
			}

			var unitSize = SelfUnit.GetSize();
			unitMaxSize = Mathf.Max(unitSize.x, unitSize.z);
			
			Damageable.GlobalDied += OnDied;
		}
		
		void OnDied(Unit unitowner)
		{
			if (unitowner == SelfUnit)
				ExitAllUnits(true);
		}
		
		void OnDestroy() => Damageable.GlobalDied -= OnDied;

		void Update()
		{
			for (var i = 0; i < carryingUnits.Count; i++)
				carryingUnits[i].transform.position = transform.position + randomedOffsets[i];

			for (var i = unitsToTake.Count - 1; i >= 0; i--)
			{
				if (!CanCarryOneMoreUnit())
				{
					unitsToTake[i].EndCurrentOrder();
					unitsToTake.RemoveAt(i);
					continue;
				}

				if (Vector3.Distance(unitsToTake[i].transform.position, transform.position) <= unitMaxSize + 0.5f)
				{
					CarryUnit(unitsToTake[i]);
					unitsToTake.RemoveAt(i);
				}
			}
		}

		public bool CanCarryOneMoreUnit() => SelfUnit.Data.canCarryUnitsCount > carryingUnits.Count;

		public int CanCarryCount()
		{
			return Mathf.Clamp(SelfUnit.Data.canCarryUnitsCount - carryingUnits.Count, 0, SelfUnit.Data.canCarryUnitsCount);
		}

		public void PrepareToCarryUnits(List<Unit> units)
		{
			for (var i = 0; i < units.Count; i++)
				PrepareToCarryUnit(units[i]);
		}
		
		public void PrepareToCarryUnit(Unit unit)
		{
			if (!unit || !unit.IsInMyTeam(SelfUnit) || !unit.Data.canBeCarried)
				return;

			var order = new FollowOrder
			{
				FollowTarget = SelfUnit
			};

			unit.AddOrder(order, false);
			
			if (!unitsToTake.Contains(unit))
				unitsToTake.Add(unit);
		}
		
		public void CarryUnit(Unit unit)
		{
			if (!CanCarryOneMoreUnit() || unit.IsBeingCarried)
				return;
			
			SetUnitCarryState(unit, true);
			carryingUnits.Add(unit);

			UI.UIController.Instance.CarryingUnitList.Redraw();
		}

		public void ExitUnit(Unit unit)
		{
			SetUnitCarryState(unit, false);

			var randomedX = Mathf.Sin(Random.Range(-1f, 1f) * Mathf.PI);
			var randomedZ = Mathf.Cos(Random.Range(-1f, 1f) * Mathf.PI);
			
			unit.transform.position = transform.position + new Vector3(randomedX, 0f, randomedZ);

			var order = new MovePositionOrder
			{
				MovePosition = unit.transform.position + new Vector3(randomedX, 0f, randomedZ) * 2f
			};
			
			unit.AddOrder(order, false, false);
			
			carryingUnits.Remove(unit);

			UnitExited?.Invoke(unit);
		}

		void SetUnitCarryState(Unit unit, bool isCarried)
		{
			if (unit.Data.hasMoveModule && isCarried)
				unit.GetModule<Movable>().Stop();
			
			unit.SetCarryState(isCarried);

			CarryStateChanged?.Invoke(isCarried);

			if (carryingUnits.Count == 0 && AllUnitsExited != null)
				AllUnitsExited.Invoke();
		}

		public void ExitAllUnits(bool dieExit = false)
		{
			for (var i = carryingUnits.Count - 1; i >= 0; i--)
			{
				if (dieExit && carryingUnits[i].GetModule<Damageable>())
					carryingUnits[i].GetModule<Damageable>().TakeDamage(carryingUnits[i].Data.maxHealth * CarryingUnitsDamageOnDieCoef);
				
				ExitUnit(carryingUnits[i]);
			}

			UI.UIController.Instance.CarryingUnitList.Redraw();

			AllUnitsExited?.Invoke();
		}

		public List<Unit> GetCarryingUnits() => carryingUnits;
	}
}