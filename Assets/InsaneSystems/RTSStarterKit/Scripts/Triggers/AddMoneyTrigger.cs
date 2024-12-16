using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public sealed class AddMoneyTrigger : TriggerBase
	{
		[Range(0, 15)] public int playerToAddMoney;
		public int moneyToAdd;

		protected override void ExecuteAction()
		{
			Player.GetPlayerById((byte)playerToAddMoney).AddMoney(moneyToAdd);
		}
	}
}