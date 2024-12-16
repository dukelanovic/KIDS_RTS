using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> Production module allows unit to produce other units. Usually it should be added to the buildings like Factory etc. </summary>
	public class Production : Module
	{
		public delegate void ProductionAction(Production productionModule);
		
		public static event ProductionAction ProductionSpawned, ProductionSelected, ProductionUnselected;

		public event Action StartedProduce, EndedProduce;

		[Tooltip("Id of production category in UnitData settings")]
		[SerializeField, Range(0, 4)] int categoryId;
		[Tooltip("Point where units will be appeared after building finished.")]
		[SerializeField] Transform spawnPoint;
		[Tooltip("Point where units will move after spawn.")]
		[SerializeField] Transform moveWaypoint;
		
		public List<UnitData> UnitsQueue { get; } = new List<UnitData>();
		public float TimeToBuildCurrentUnit { get; protected set; } = 999f;

		public List<UnitData> AvailableUnits => productionCategory.availableUnits;
		public ProductionCategory GetProductionCategory => productionCategory;
		
		public Transform SpawnPoint
		{
			get => spawnPoint;
			set => spawnPoint = value;
		}
		
		public Transform SpawnWaypoint
		{
			get => moveWaypoint;
			set => moveWaypoint = value;
		}

		bool isBuildingNow;
		ProductionCategory productionCategory;

		readonly Collider[] colliders = new Collider[7];
		
		void Start()
		{
			if (SelfUnit.Data.productionCategories.Count <= categoryId)
			{
				Debug.LogWarning("[Production module] Your unit " + name + " have incorrectly setted up Production categories.");
				enabled = false;
				return;
			}
			
			productionCategory = SelfUnit.Data.productionCategories[categoryId];

			ProductionSpawned?.Invoke(this);

			SelfUnit.Selected += OnSelected;
			SelfUnit.Unselected += OnUnselected;
		}

		void Update()
		{
			if (isBuildingNow)
				HandleProductionProgress();
			else if (UnitsQueue.Count > 0)
				StartProduction();
		}

		void HandleProductionProgress()
		{
			if (TimeToBuildCurrentUnit > 0)
			{
				TimeToBuildCurrentUnit -= Time.deltaTime * GetBuildingSpeedCoefficient();
				return;
			}

			if (!productionCategory.isBuildings)
			{
				SpawnCurrentUnit(UnitsQueue[0]);
				FinishBuilding();
			}
		}

		void SpawnCurrentUnit(UnitData unitData)
		{
			var spawnedUnit = SpawnController.SpawnUnit(unitData, SelfUnit.OwnerPlayerId, spawnPoint);

			if (spawnedUnit.GetComponent<Harvester>()) // resource harvesters have their own code to move to the resource field.
				return;

			var order = new MovePositionOrder
			{
				MovePosition = moveWaypoint.position
			};

			if (unitData.moveType == UnitData.MoveType.Flying)
				order.MovePosition += new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
			
			spawnedUnit.AddOrder(order, false, false);
			
			ShuffleUnitsOnExit(order.MovePosition, SelfUnit);
		}

		void StartProduction()
		{
			isBuildingNow = true;
			TimeToBuildCurrentUnit = UnitsQueue[0].buildTime;
			
			StartedProduce?.Invoke();
		}

		public void AddUnitToQueue(UnitData unitData)
		{
			if (unitData.isBuilding && UnitsQueue.Count > 0)
				return;
			
			if (UnitsQueue.Count == 0)
				TimeToBuildCurrentUnit = unitData.buildTime;
			
			var playerOwner = Player.GetPlayerById(SelfUnit.OwnerPlayerId);
			if (playerOwner.IsHaveMoney(unitData.price))
			{
				UnitsQueue.Add(unitData);
				playerOwner.SpendMoney(unitData.price);
			}
		}
		
		public void AddUnitToQueueByIndex(int index, int productionCategoryId = 0)
		{
			var prodCats = SelfUnit.Data.productionCategories;
			
			productionCategoryId = Mathf.Clamp(productionCategoryId, 0, prodCats.Count - 1);

			var category = prodCats[productionCategoryId];

			if (index > category.availableUnits.Count - 1)
				return;
			
			index = Mathf.Clamp(index, 0, category.availableUnits.Count - 1);
			
			AddUnitToQueue(category.availableUnits[index]);
		}

		public void RemoveUnitFromQueue(UnitData unitData, bool isCancel)
		{
			if (UnitsQueue.Count == 0)
				return;

			var isFirstUnitTypeLikeThis = UnitsQueue[0] == unitData;
			if (UnitsQueue.Remove(unitData) && isFirstUnitTypeLikeThis)
				StopProduction();

			if (isCancel)
			{
				var playerOwner = Player.GetPlayerById(SelfUnit.OwnerPlayerId);
				playerOwner.AddMoney(unitData.price);
			}
		}

		public int GetUnitsOfSpecificTypeInQueue(UnitData unitData)
		{
			int result = 0;

			for (int i = 0; i < UnitsQueue.Count; i++)
				if (UnitsQueue[i] == unitData) // todo change this check
					result++;

			return result;
		}

		public bool IsUnitOfTypeCurrentlyBuilding(UnitData unitData) => UnitsQueue.Count > 0 && UnitsQueue[0] == unitData;
		public bool IsUnitOfTypeInQueue(UnitData unitData) => UnitsQueue.Count > 0 && UnitsQueue.Contains(unitData);
		public float GetBuildProgressPercents() => 1f - TimeToBuildCurrentUnit / UnitsQueue[0].buildTime;

		public bool IsBuildingReady() => TimeToBuildCurrentUnit <= 0;

		public void FinishBuilding()
		{		
			if (UnitsQueue.Count > 0)
				UnitsQueue.RemoveAt(0);

			StopProduction();
		}

		void StopProduction()
		{
			EndedProduce?.Invoke();
			isBuildingNow = false;
		}

		void OnSelected(Unit _) => ProductionSelected?.Invoke(this);
		void OnUnselected(Unit _) => ProductionUnselected?.Invoke(this);

		public int GetProductionNumber()
		{
			var productionsOfThisType = SelfUnit.GetOwnerPlayer().GetProductionBuildingsByCategory(productionCategory);
			return productionsOfThisType.IndexOf(this);
		}

		public void ShuffleUnitsOnExit(Vector3 origin, Unit askedFromUnit, int depth = 0)
		{
			var count = Physics.OverlapCapsuleNonAlloc(origin, origin + Vector3.up * 20f, 1f, colliders);

			for (int i = 0; i < count; i++)
			{
				var unit = colliders[i].GetComponent<Unit>();
				
				if (unit && askedFromUnit != unit && unit.IsInMyTeam(SelfUnit) && unit.Data.moveType == askedFromUnit.Data.moveType) //  && !unit.HasOrders() // has orders was needed to prevent stopping units who just moved near exit point, but works bad
				{
					int randomizeDirection = Random.Range(0, 2);
					var direction = randomizeDirection == 0 ? 1 : -1;

					var moveOrder = new MovePositionOrder();
					moveOrder.MovePosition = unit.transform.position + (3f * direction * unit.transform.right);

					if (depth < 2)
						ShuffleUnitsOnExit(moveOrder.MovePosition, unit, depth + 1);

					unit.AddOrder(moveOrder, false, false);
				}
			}
		}

		float GetBuildingSpeedCoefficient()
		{
			var storage = GameController.Instance.MainStorage;

			if (!storage.isElectricityUsedInGame || SelfUnit.GetOwnerPlayer().GetElectricityUsagePercent() < 1f)
				return 1f;

			return 1f * GameController.Instance.MainStorage.speedCoefForProductionsWithoutElectricity;
		}

		/// <summary> This method changes production category id, which will be loaded in game from unit data. Use it only for editor scripting purposes. </summary>
		public void SetCategoryId(int id) => categoryId = id;
	}
}