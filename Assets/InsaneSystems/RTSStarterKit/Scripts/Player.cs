using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[Serializable]
	public class Player 
	{
		public delegate void PlayerMoneyChangedAction(int newMoneyValue);
		public delegate void PlayerElectricityChangedAction(int totalElectricity, int usedElectricity);

		public static byte LocalPlayerId { get; private set; }
		public static Player LocalPlayer { get; private set; }

		public event Action WasDefeated;
		public event PlayerMoneyChangedAction MoneyChanged;
		public event PlayerElectricityChangedAction ElectricityChanged;

		public string UserName { get; private set; }
		public Color Color { get; private set; }
		public FactionData SelectedFaction { get; private set; }
		public byte Id { get; private set; }
		public byte TeamIndex { get; private set; }

		public int Money { get; private set; } = 10000;
		public int Electricity { get; private set; }
		public int UsedElectricity { get; private set; }

		public bool IsAIPlayer { get; private set; }
		public bool IsDefeated { get; private set; }

		public List<Production> OwnedProductionBuildings { get; private set; } = new List<Production>();
		public List<Unit> OwnedBuildings { get; private set; } = new List<Unit>();

		public readonly Material PlayerUnitMaterial;

		public Player(Color color, PlayerSettings settings, string username = "Default")
		{
			UserName = username;
			Color = color;
			
			TeamIndex = settings.team;
			SelectedFaction = settings.selectedFaction;
			IsAIPlayer = settings.isAI;

			PlayerUnitMaterial = new Material(GameController.Instance.MainStorage.playerColorMaterialTemplate)
			{
				color = color
			};
		}

		public void Init(byte newId)
		{
			Id = newId;
			
			if (Id == LocalPlayerId)
				LocalPlayer = this;
		}

		public bool IsHaveMoney(int amount) => Money >= amount;

		public void SetMoney(int amount)
		{
			Money = Mathf.Clamp(amount, 0, 1000000);
			
			MoneyChanged?.Invoke(Money);
		}
		
		public void AddMoney(int amount) => SetMoney(Money + amount);
		public void SpendMoney(int amount) => SetMoney(Money - amount);

		public void AddElectricity(int amount) => SetElectricity(Electricity + amount, UsedElectricity);
		public void RemoveElectricity(int amount) => SetElectricity(Electricity - amount, UsedElectricity);
		public void AddUsedElectricity(int amount)  => SetElectricity(Electricity, UsedElectricity + amount);
		public void RemoveUsedElectricity(int amount) => SetElectricity(Electricity, UsedElectricity - amount);

		void SetElectricity(int electricity, int usedElectricity)
		{
			Electricity = Mathf.Clamp(electricity, 0, 10000);
			UsedElectricity = Mathf.Clamp(usedElectricity, 0, 10000);
			
			ElectricityChanged?.Invoke(Electricity, UsedElectricity);
		}
		
		public float GetElectricityUsagePercent() => UsedElectricity / (float)Electricity;
		public bool IsLocalPlayer() => Id == LocalPlayerId;

		public bool TryAddProduction(Production production)
		{
			if (!OwnedProductionBuildings.Contains(production))
			{
				OwnedProductionBuildings.Add(production);
				return true;
			}

			return false;
		}
		
		public void RemoveProduction(Production production) => OwnedProductionBuildings.Remove(production);

		public void AddBuilding(Unit building)
		{
			if (!OwnedBuildings.Contains(building))
				OwnedBuildings.Add(building);
		}
		
		public void RemoveBuilding(Unit building) => OwnedBuildings.Remove(building);

		public bool HasBuildingType(UnitData buildingTypeTemplate)
		{
			if (!buildingTypeTemplate)
				return true;
			
			for (var i = 0; i < OwnedBuildings.Count; i++)
				if (OwnedBuildings[i] && OwnedBuildings[i].Data.textId == buildingTypeTemplate.textId)
					return true;
			
			return false;
		}
		
		public void DefeatPlayer()
		{
			if (IsDefeated)
				return;

			IsDefeated = true;

			for (int i = Unit.AllUnits.Count - 1; i >= 0; i--)
			{
				var unit = Unit.AllUnits[i];
				if (unit && unit.OwnerPlayerId == Id)
				{
					var damageable = unit.GetModule<Damageable>();

					if (damageable)
						damageable.TakeDamage(99999);
				}
			}
			
			WasDefeated?.Invoke();
		}

		public List<Production> GetProductionBuildingsByCategory(ProductionCategory category)
		{
			var resultList = new List<Production>(); // todo insane systems - optimize

			for (int i = 0; i < OwnedProductionBuildings.Count; i++)
				if (OwnedProductionBuildings[i] != null && OwnedProductionBuildings[i].GetProductionCategory == category)
					resultList.Add(OwnedProductionBuildings[i]);

			return resultList;
		}

		public bool IsHaveProductionOfCategory(ProductionCategory category) => GetProductionBuildingsByCategory(category).Count > 0;

		public List<Player> GetEnemyPlayers()
		{
			var allPlayers = GameController.Instance.PlayersController.PlayersIngame;
			var enemyPlayers = new List<Player>();  // todo insane systems - optimize

			for (int i = 0; i < allPlayers.Count; i++)
				if (allPlayers[i] != this && allPlayers[i].TeamIndex != TeamIndex)
					enemyPlayers.Add(allPlayers[i]);

			return enemyPlayers;
		}

		public static Player GetPlayerById(byte playerId)
		{
			var players = GameController.Instance.PlayersController.PlayersIngame;

			if (playerId >= players.Count)
				throw new ArgumentOutOfRangeException(nameof(players), "No player with such ID exist!");
			
			return players[playerId];
		}

		public static void SetLocalPlayerId(byte id)
		{
			if (LocalPlayerId == id)
				return;

			LocalPlayerId = id;
			LocalPlayer = GetPlayerById(id);
		}
	}
}