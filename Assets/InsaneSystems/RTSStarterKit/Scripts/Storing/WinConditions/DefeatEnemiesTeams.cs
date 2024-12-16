using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "DefeatEnemiesTeams", menuName = Storage.AssetName + "/Win Conditions/Defeat Enemies Teams")]
	public sealed class DefeatEnemiesTeams : WinCondition
	{
		public override bool CheckCondition(out int winnerTeam)
		{
			var allBuildings = Unit.AllUnits.FindAll(unit => unit.Data.isBuilding);

			var playersIngame = GameController.Instance.PlayersController.PlayersIngame;
			
			for (int i = 0; i < playersIngame.Count; i++)
			{
				if (!allBuildings.Find(unit => unit.OwnerPlayerId == i))
					playersIngame[i].DefeatPlayer();
			}

			var allUndefeatedPlayers = playersIngame.FindAll(player => player.IsDefeated == false);

			winnerTeam = 0;
			
			for (int i = 0; i < allUndefeatedPlayers.Count; i++)
			{
				if (i == 0)
					winnerTeam = allUndefeatedPlayers[i].TeamIndex;
				else if (allUndefeatedPlayers[i].TeamIndex != winnerTeam)
					return false;
			}

			return true;
		}
	}
}