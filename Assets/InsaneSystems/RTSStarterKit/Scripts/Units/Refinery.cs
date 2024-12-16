using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Refinery : Module
	{
		public static List<Refinery> AllRefineries { get; } = new List<Refinery>();
		
		[SerializeField] Transform carryOutResourcesPoint;
		
		public Transform CarryOutResourcesPoint => carryOutResourcesPoint;

		protected override void AwakeAction() => AllRefineries.Add(this);
		void Start() => SpawnHarvester();
		void OnDestroy() => AllRefineries.Remove(this);

		/// <summary> Call this on game start. </summary>
		public static void Init() => AllRefineries.Clear();

		public void AddResources(int amount)
		{
			GameController.Instance.PlayersController.PlayersIngame[SelfUnit.OwnerPlayerId].AddMoney(amount);
		}

		void SpawnHarvester()
		{
			if (!SelfUnit.Data.RefineryHarversterUnitData)
				return;
			
			var spawnedHarvester = SpawnController.SpawnUnit(SelfUnit.Data.RefineryHarversterUnitData, SelfUnit.OwnerPlayerId, carryOutResourcesPoint);
			spawnedHarvester.GetComponent<Harvester>().SetRefinery(this);
		}
	}
}