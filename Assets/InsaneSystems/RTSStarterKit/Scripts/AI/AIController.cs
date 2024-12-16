using System;
using System.Collections.Generic;
using UnityEngine;
using InsaneSystems.RTSStarterKit.Controls;
using Random = UnityEngine.Random;

namespace InsaneSystems.RTSStarterKit.AI
{
	/// <summary> AI Controller of specific AI Player. Each AI player has it own AI Controller. It controls basic AI logic - units, building base, etc
	/// <para>Derive your class from this and in it you can code any new logic for AI.</para> </summary>
	public class AIController : MonoBehaviour
	{
		public event Action<Unit> AiUnitWasSpawned, AiBuildingWasSpawned;
		[SerializeField] protected AISettings aiSettings;

		protected byte selfPlayerId { get; private set; }
		protected float thinkTimer { get; private set; }
		protected float buildingBuyTimer { get; private set; }
		protected float unitBuyTimer { get; private set; }
		protected bool isPlayerSettedUp { get; private set; }

		#region Building parameters
		protected Production selfCommandCenter { get; private set; }
		protected readonly List<Unit> finishedBuildings = new List<Unit>();
		protected readonly List<Unit> selfUnits = new List<Unit>();
		#endregion

		#region Units parameters
		protected readonly List<UnitsGroup> unitGroups = new List<UnitsGroup>();
		readonly List<UnitData> unitsSuitableToBuy = new List<UnitData>();

		int unitBuildListId;
		int unitListId;
		
		#endregion

		void Awake()
		{
			Unit.UnitSpawned += OnUnitSpawned;

			if (aiSettings)
				SetupWithAISettings(aiSettings);
		}

		protected virtual void Start() { }

		void OnDestroy() => Unit.UnitSpawned -= OnUnitSpawned;
		
		void Update()
		{
			if (!isPlayerSettedUp)
				return;

			UpdateAction();

			var deltaTime = Time.deltaTime;
			
			aiSettings.delayBeforeStartBuyingUnits -= deltaTime;
			aiSettings.delayBeforeStartCreateBuildings -= deltaTime;

			buildingBuyTimer -= deltaTime;
			unitBuyTimer -= deltaTime;
			
			if (thinkTimer > 0)
			{
				thinkTimer -= deltaTime;
				return;
			}

			DoAction();

			thinkTimer = aiSettings.thinkTime;
		}
		
		/// <summary> Override this method, if you need your custom AI logic in Update. </summary>
		protected virtual void UpdateAction() { }

		void DoAction()
		{
			HandleBuilding();
			HandleUnitsBuilding();
			HandleUnitsControls();

			HandleActions();
		}

		/// <summary> Override this method, if you need add new custom actions for your AI. It will be called periodically using thinkTimer variable. </summary>
		protected virtual void HandleActions() { }

		protected virtual void HandleBuilding()
		{
			if (aiSettings.delayBeforeStartCreateBuildings > 0 || !selfCommandCenter)
				return;

			HandleBuildingSelect();

			if (selfCommandCenter.UnitsQueue.Count == 0 || !selfCommandCenter.IsBuildingReady())
				return;

			var commandCenterPosition = selfCommandCenter.transform.position;
			var randomedOffset = new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20));
			var finalPoint = commandCenterPosition + randomedOffset;

			var currentBuilding = selfCommandCenter.UnitsQueue[0];
			var buildingSize = Build.GetBuildingSize(currentBuilding);
			var buildingRotation = Quaternion.Euler(0, 180, 0);

			var canBuild = Build.CheckZoneToBuild(finalPoint, buildingSize, buildingRotation, selfPlayerId);

			if (canBuild)
			{
				var spawnedBuildingObject = Build.CreateBuilding(currentBuilding.selfPrefab, finalPoint,
					buildingRotation, selfPlayerId);
				var spawnedBuildingUnit = spawnedBuildingObject.GetComponent<Unit>();
				finishedBuildings.Add(spawnedBuildingUnit);
				
				selfCommandCenter.RemoveUnitFromQueue(currentBuilding, false);
				selfCommandCenter.FinishBuilding();
				
				AiBuildingWasSpawned?.Invoke(spawnedBuildingUnit);
			}
		}

		protected virtual void HandleBuildingSelect()
		{
			if (buildingBuyTimer > 0 || selfCommandCenter.UnitsQueue.Count > 0)
				return;
			
			var selfPlayer = Player.GetPlayerById(selfPlayerId);
			
			for (int i = 0; i < aiSettings.buildingPriority.Length; i++)
			{
				var currentBuilding = aiSettings.buildingPriority[i];

				if (currentBuilding == null || !selfPlayer.HasBuildingType(currentBuilding.requiredBuildingToUnlock))
					continue;

				if (selfCommandCenter.IsUnitOfTypeInQueue(currentBuilding))
					continue;

				var canBuyBuilding = true;
				var sameAmount = 0;
				
				foreach (var finishedBuilding in finishedBuildings) // todo: Insane Systems - use priority to start building secondary building types.
					if (currentBuilding == finishedBuilding.Data)
					{
						sameAmount++;

						if (sameAmount >= aiSettings.MaximumSameBuildings)
						{
							canBuyBuilding = false;
							break;
						}
					}

				if (canBuyBuilding)
				{
					selfCommandCenter.AddUnitToQueue(currentBuilding);
					break;
				}
			}

			buildingBuyTimer = aiSettings.BuildingBuyDelay;
		}

		protected virtual void HandleUnitsBuilding()
		{
			if (unitBuyTimer > 0 || aiSettings.delayBeforeStartBuyingUnits > 0)
				return;

			var playerAI = Player.GetPlayerById(selfPlayerId);

			if (playerAI.GetEnemyPlayers().Count == 0)
				return;

			if (!TryGetUnitToBuild(out var unitData) || !TryGetUnitCategory(unitData, out var selectedCategory))
				return;

			var allSuitableProductions = playerAI.GetProductionBuildingsByCategory(selectedCategory);

			if (allSuitableProductions.Count == 0)
				return;

			var randomedProduction = allSuitableProductions[Random.Range(0, allSuitableProductions.Count)];
			randomedProduction.AddUnitToQueue(unitData);
			
			unitBuyTimer = aiSettings.UnitBuyDelay;
		}

		protected bool TryGetUnitCategory(UnitData unitData, out ProductionCategory category)
		{
			var categories = GameController.Instance.MainStorage.availableProductionCategories;

			foreach (var cat in categories)
			{
				if (cat.availableUnits.Contains(unitData))
				{
					category = cat;
					return true;
				}
			}

			category = default;
			return false;
		}
		
		protected virtual bool TryGetUnitToBuild(out UnitData unitData)
		{
			UpdateSuitableUnits();

			if (unitsSuitableToBuy.Count == 0)
			{
				unitData = default;
				return false;
			}

			// moving till end until suitable unit will not be found
			for (int q = unitBuildListId; q < aiSettings.UnitsToBuild.Count; q++)
			{
				if (!aiSettings.UnitsToBuild[q].CanBuildByTime())
					continue;
				
				var groupUnits = aiSettings.UnitsToBuild[q].Units;

				for (int w = unitListId; w < groupUnits.Count; w++)
				{
					if (!unitsSuitableToBuy.Contains(groupUnits[w]))
						continue;
					
					unitData = groupUnits[w];
					unitListId = w + 1;
						
					if (unitListId >= groupUnits.Count)
					{
						unitListId = 0;
						unitBuildListId++;
					}
						
					if (unitBuildListId >= aiSettings.UnitsToBuild.Count)
						unitBuildListId = 0;
						
					return true;
				}
			}
			
			unitListId = 0; // if suitable unit not found, resetting to start and waiting next call
			unitBuildListId = 0;
			
			unitData = default;
			return false;
		}

		protected virtual void UpdateSuitableUnits()
		{
			var selfPlayer = Player.GetPlayerById(selfPlayerId);
			unitsSuitableToBuy.Clear();

			var categories = GameController.Instance.MainStorage.availableProductionCategories;
			
			foreach (var category in categories)
			{
				for (var i = 0; i < category.availableUnits.Count; i++)
				{
					var unit = category.availableUnits[i];
				
					if (!selfPlayer.HasBuildingType(unit.requiredBuildingToUnlock))
						continue;
				
					if (unit.hasAttackModule)
						unitsSuitableToBuy.Add(unit);
				}
			}
		}

		protected virtual void OnUnitSpawned(Unit unit)
		{
			if (unit.OwnerPlayerId != selfPlayerId)
				return;

			if (unit.Data.isBuilding)
				return;

			if (unit.GetComponent<Harvester>())
			{
				OnHarvesterSpawned(unit);
				return;
			}

			selfUnits.Add(unit);

			var isNewGroupNeeded = true;
			var selectedGroup = unitGroups.Count;
			
			for (int i = 0; i < unitGroups.Count; i++)
			{
				if (unitGroups[i].IsGroupNeedsUnits(aiSettings))
				{
					selectedGroup = i;
					isNewGroupNeeded = false;
					break;
				}
			}

			if (isNewGroupNeeded)
				unitGroups.Add(new UnitsGroup());

			unitGroups[selectedGroup].AddUnit(unit);
			
			AiUnitWasSpawned?.Invoke(unit);
		}

		/// <summary> Override this method, if you need add custom logic to the spawned for AI harvesters </summary>
		protected virtual void OnHarvesterSpawned(Unit harvesterUnit) { }

		protected virtual void HandleUnitsControls()
		{
			if (unitGroups.Count == 0)
				return;

			var enemyPlayers = Player.GetPlayerById(selfPlayerId).GetEnemyPlayers();

			var isAiHasProductions = Player.GetPlayerById(selfPlayerId).OwnedProductionBuildings.Count > 0; // if AI have no productions, groups will be always not full, so in this case not full units groups should attack.

			var fullUnitGroups = unitGroups.FindAll(unitGroup => (!unitGroup.IsGroupNeedsUnits(aiSettings) || !isAiHasProductions) && !unitGroup.IsGroupHaveOrder());

			if (fullUnitGroups.Count == 0 || enemyPlayers.Count == 0)
				return;

			var randomlySelectedGroup = fullUnitGroups[Random.Range(0, fullUnitGroups.Count)];
			var selectedEnemy = enemyPlayers[Random.Range(0, enemyPlayers.Count)];
			var selectedTarget = Unit.AllUnits.Find(unit => unit.OwnerPlayerId == selectedEnemy.Id);

			var attackOrder = new AttackOrder
			{
				AttackTarget = selectedTarget
			};

			randomlySelectedGroup.AddOrderToGroup(attackOrder);
		}

		/// <summary> Sets new AI Settings to this AI and create instance of AISettings Scriptable Object to prevent overriding values in Editor. </summary>
		public void SetupWithAISettings(AISettings newAISettings) => aiSettings = Instantiate(newAISettings);

		public void SetupAIForPlayer(byte playerId)
		{
			selfPlayerId = playerId;

			var aiPlayer = Player.GetPlayerById(playerId);
			if (aiPlayer.OwnedProductionBuildings.Count > 0)
				selfCommandCenter = aiPlayer.OwnedProductionBuildings[0];

			isPlayerSettedUp = true;

			OnAIAddedToPlayer(playerId);
		}

		protected virtual void OnAIAddedToPlayer(byte playerId) { }
	}
}