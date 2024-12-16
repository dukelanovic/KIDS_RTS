using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "CollectResourcesAmount", menuName = Storage.AssetName + "/Win Conditions/Collect Resources Amount")]
	public sealed class CollectResourcesAmount : WinCondition
	{
		public int neededAmount = 15000;
		[Tooltip("If AI player should not win if he collect such money amount, check this true.")]
		public bool onlyForNonAIPlayers = true;
		
		public override bool CheckCondition(out int winnerTeam)
		{
			var playersIngame = GameController.Instance.PlayersController.PlayersIngame;
			winnerTeam = -1;
			
			for (int i = 0; i < playersIngame.Count; i++)
			{
				if (onlyForNonAIPlayers && playersIngame[i].IsAIPlayer)
					continue;
				
				if (playersIngame[i].Money >= neededAmount)
				{
					winnerTeam = playersIngame[i].TeamIndex;
					return true;
				}
			}

			return false;
		}
	}
}