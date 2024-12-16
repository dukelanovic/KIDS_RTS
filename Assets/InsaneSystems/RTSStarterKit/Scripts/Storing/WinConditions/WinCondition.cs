using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public abstract class WinCondition : ScriptableObject
	{
		public abstract bool CheckCondition(out int winnerTeam);
	}
}