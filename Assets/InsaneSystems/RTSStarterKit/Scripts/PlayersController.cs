using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class PlayersController : MonoBehaviour
	{
		public List<Player> PlayersIngame { get; } =  new List<Player>();

		public static event Production.ProductionAction ProductionModuleWasAddedToPlayer;

		/// <summary>Should be called it from GameController's Awake before players initialized. </summary>
		public void PreAwake()
		{
			Unit.UnitSpawned += OnUnitSpawned;
			Unit.UnitDestroyed += OnUnitDestroyed;
			
			Production.ProductionSpawned += AddProductionBuildingToPlayer;
		}
		
		void OnUnitSpawned(Unit unit) => AddBuildingToOwner(unit);
		void OnUnitDestroyed(Unit unit) => RemoveBuildingFromOwner(unit);

		void Start()
		{
			Unit.UnitChangedOwner += OnUnitChangedOwner; // it will ignore first owner changes on load, but it is ok
		}

		void OnDestroy()
		{
			Production.ProductionSpawned -= AddProductionBuildingToPlayer;
			Unit.UnitSpawned -= OnUnitSpawned;
			Unit.UnitDestroyed -= OnUnitDestroyed;
		}
		
		void OnUnitChangedOwner(Unit unit, int newOwner, int previousOwner)
		{
			var production = unit.GetModule<Production>();
			
			if (IsPlayerExist(previousOwner))
			{
				if (unit.Data.isBuilding)
					PlayersIngame[previousOwner].RemoveBuilding(unit);

				if (unit.Data.isProduction)
					PlayersIngame[previousOwner].RemoveProduction(production);
			}
			
			AddBuildingToOwner(unit);
			
			if (unit.Data.isProduction)
				AddProductionBuildingToPlayer(production);
		}

		public bool IsPlayerExist(int playerId) => playerId > -1 && playerId < PlayersIngame.Count;

		void AddBuildingToOwner(Unit unit)
		{
			if (!unit.Data.isBuilding)
				return;
			
			if (IsPlayerExist(unit.OwnerPlayerId))
				PlayersIngame[unit.OwnerPlayerId].AddBuilding(unit);
		}

		void RemoveBuildingFromOwner(Unit unit)
		{
			if (!unit.Data.isBuilding)
				return;
			
			if (IsPlayerExist(unit.OwnerPlayerId))
				PlayersIngame[unit.OwnerPlayerId].RemoveBuilding(unit);
		}

		void AddProductionBuildingToPlayer(Production productionModule)
		{
			var playerOwner = productionModule.SelfUnit.OwnerPlayerId;
			
			var isAdded = PlayersIngame[playerOwner].TryAddProduction(productionModule);

			if (isAdded && ProductionModuleWasAddedToPlayer != null)
				ProductionModuleWasAddedToPlayer.Invoke(productionModule);
		}

		public bool IsPlayersInOneTeam(byte playerAId, byte playerBId)
		{
			if (PlayersIngame.Count <= playerAId || PlayersIngame.Count <= playerBId)
				return false;

			return PlayersIngame[playerAId].TeamIndex == PlayersIngame[playerBId].TeamIndex;
		}

		public void AddPlayer(Player player)
		{
			player.Init((byte) PlayersIngame.Count);
			PlayersIngame.Add(player);
		}
	}
}