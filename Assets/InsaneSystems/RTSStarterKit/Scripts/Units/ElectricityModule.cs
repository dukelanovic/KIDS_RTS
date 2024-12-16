using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This module allows unit to produce on consume electricity. </summary>
	[DisallowMultipleComponent]
	public sealed class ElectricityModule : Module
	{
		int addsElectricity, neededElectricity;

		protected override void AwakeAction()
		{
			addsElectricity = SelfUnit.Data.addsElectricity;
			neededElectricity = SelfUnit.Data.usesElectricity;

			Unit.UnitSpawned += OnBuildingComplete;
		}

		void Start()
		{
			var damageable = SelfUnit.GetModule<Damageable>();
			
			if (damageable)
				damageable.Died += OnDie;
		}

		void OnDestroy() => Unit.UnitSpawned -= OnBuildingComplete;
		
		void OnBuildingComplete(Unit unit)
		{
			if (unit != SelfUnit)
				return;

			Player.GetPlayerById(SelfUnit.OwnerPlayerId).AddElectricity(addsElectricity);
			Player.GetPlayerById(SelfUnit.OwnerPlayerId).AddUsedElectricity(neededElectricity);
		}

		void OnDie(Unit unit)
		{
			if (unit != SelfUnit)
				return;

			Player.GetPlayerById(SelfUnit.OwnerPlayerId).RemoveElectricity(addsElectricity);
			Player.GetPlayerById(SelfUnit.OwnerPlayerId).RemoveUsedElectricity(neededElectricity);
		}

		public void IncreaseAddingElectricity(int addToAdding)
		{
			addsElectricity += addToAdding;
			Player.GetPlayerById(SelfUnit.OwnerPlayerId).AddElectricity(addToAdding);
		}

	}
}