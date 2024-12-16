using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InsaneSystems.RTSStarterKit
{
	[DisallowMultipleComponent]
	public class Harvester : Module
	{
		const float RandomFieldDistance = 2f;
		const float SqrRandomFieldDistance = 24f;
		const float SqrRefineryDistance = 8f; // todo rts kit replace this hardcode

		public enum HarvestState
		{
			MoveToField,
			Harvest,
			MoveToRefinery,
			CarryOutResources,
			Idle
		}

		public event HarvesterResourcesChanged ResourcesChanged;
		public event Action StartedHarvest, StoppedHarvest;
		
		public int MaxResources => SelfUnit.Data.harvestMaxResources;
		public int HarvestedResources { get; protected set; }

		HarvestState harvestState;

		Refinery nearestRefinery;
		ResourcesField resourcesField;
		float recheckTimer = 1f;

		float harvestTimeLeft;
		float carryOutTimeLeft;
		int canHarvestFromField;
		
		/// <summary> Field for temporary colliders store for some physical radius checks of harvester. </summary>
		readonly Collider[] nearestColliders = new Collider[15];

		public delegate void HarvesterResourcesChanged(float newValue, float maxValue);

		void Start()
		{
			SelfUnit.ReceivedOrder += OnUnitReceivedOrder;
			SelfUnit.Selected += OnSelected;
			SelfUnit.Unselected += OnUnselected;

			ResourcesChanged?.Invoke(0, MaxResources);
		}

		void OnSelected(Unit _) => UI.HarvesterBar.SpawnForHarvester(this);
		void OnUnselected(Unit _) => UI.HarvesterBar.RemoveBarOfHarvester(this);

		void Update()
		{
			if (!nearestRefinery)
			{
				SearchNearestRefinery();

				return;
			}

			if (!resourcesField)
			{
				SearchNearestResourcesField();

				return;
			}

			switch (harvestState)
			{
				case HarvestState.MoveToField: MoveToField(); break;
				case HarvestState.Harvest: Harvest(); break;
				case HarvestState.MoveToRefinery: MoveToRefinery(); break;
				case HarvestState.CarryOutResources: CarryOutResources(); break;
			}
		}

		void MoveToField()
		{
			if ((transform.position - resourcesField.transform.position).sqrMagnitude < SqrRandomFieldDistance)
				SetHarvestState(HarvestState.Harvest);
		}

		void Harvest()
		{
			harvestTimeLeft -= Time.deltaTime;
			HarvestedResources = (int)Mathf.Lerp(0, canHarvestFromField, 1f - harvestTimeLeft / SelfUnit.Data.harvestTime);

			ResourcesChanged?.Invoke(HarvestedResources, canHarvestFromField);

			if (harvestTimeLeft <= 0)
			{
				harvestTimeLeft = 0f;
				HarvestedResources = canHarvestFromField;

				SetHarvestState(HarvestState.MoveToRefinery);
			}
		}

		void MoveToRefinery()
		{
			var sqrDistance = (transform.position - nearestRefinery.CarryOutResourcesPoint.position).sqrMagnitude;
			if (sqrDistance < SqrRefineryDistance)
				SetHarvestState(HarvestState.CarryOutResources);
		}

		void CarryOutResources()
		{
			carryOutTimeLeft -= Time.deltaTime;

			if (carryOutTimeLeft <= 0)
			{
				carryOutTimeLeft = 0f;
				
				nearestRefinery.AddResources(HarvestedResources);
				HarvestedResources = 0;

				ResourcesChanged?.Invoke(HarvestedResources, MaxResources);

				SetHarvestState(HarvestState.MoveToField);
			}
		}

		void SearchNearestRefinery()
		{
			if (recheckTimer > 0)
			{
				recheckTimer -= Time.deltaTime;
				return;
			}

			var allRefineries = Refinery.AllRefineries;
			allRefineries = allRefineries.FindAll(refinery => refinery.SelfUnit.OwnerPlayerId == SelfUnit.OwnerPlayerId);

			var sqrDist = float.MaxValue - 1f;

			for (int i = 0; i < allRefineries.Count; i++)
			{
				var currentSqrDist = (transform.position - allRefineries[i].transform.position).sqrMagnitude;

				if (currentSqrDist < sqrDist)
				{
					nearestRefinery = allRefineries[i];
					sqrDist = currentSqrDist;
				}
			}

			recheckTimer = 1f;
		}

		void SearchNearestResourcesField()
		{
			if (recheckTimer > 0)
			{
				recheckTimer -= Time.deltaTime;
				return;
			}

			var allFields = ResourcesField.SceneResourceFields;
			var sqrDist = float.MaxValue - 1f;

			for (int i = 0; i < allFields.Count; i++)
			{
				if (!allFields[i].HaveResources())
					continue;
				
				var currentSqrDist = (transform.position - allFields[i].transform.position).sqrMagnitude;

				if (currentSqrDist < sqrDist)
				{
					resourcesField = allFields[i];
					sqrDist = currentSqrDist;
				}
			}

			if (resourcesField)
				SetHarvestState(HarvestState.MoveToField);

			recheckTimer = 1f;
		}

		public void SetHarvestState(HarvestState newState)
		{
			harvestState = newState;

			switch (harvestState)
			{
				case HarvestState.MoveToField:
					var order = new MovePositionOrder();
					order.Executor = SelfUnit;
					order.MovePosition = resourcesField.transform.position + new Vector3(Random.Range(-RandomFieldDistance, RandomFieldDistance), 0, Random.Range(-RandomFieldDistance, RandomFieldDistance)); // todo RTS Kit - change to proportion of resource field size
					SelfUnit.AddOrder(order, false, isReceivedEventNeeded: false);
					break;

				case HarvestState.Harvest:
					harvestTimeLeft = SelfUnit.Data.harvestTime;
					canHarvestFromField = resourcesField.TakeResources(MaxResources);
					
					if (canHarvestFromField == 0)
					{
						SetHarvestState(HarvestState.MoveToRefinery);
						break;
					}

					StartedHarvest?.Invoke();

					break;

				case HarvestState.MoveToRefinery:
					var orderBack = new MovePositionOrder();
					orderBack.MovePosition = nearestRefinery.CarryOutResourcesPoint.position;
					SelfUnit.AddOrder(orderBack, false, isReceivedEventNeeded: false);

					StoppedHarvest?.Invoke();

					break;

				case HarvestState.CarryOutResources:
					carryOutTimeLeft = SelfUnit.Data.harvestCarryOutTime;
					break;
			}
		}

		void OnUnitReceivedOrder(Unit unit, Order order)
		{
			if (order is MovePositionOrder positionOrder)
			{
				var position = positionOrder.MovePosition;

				var size = Physics.OverlapSphereNonAlloc(position, 7f, nearestColliders);

				for (int i = 0; i < size; i++)
				{
					var field = nearestColliders[i].GetComponent<ResourcesField>();

					if (!field)
						continue;
					
					resourcesField = field;
					SetHarvestState(HarvestState.MoveToField);

					return;
				}
			}
			else if (order is FollowOrder followOrder)
			{
				var target = followOrder.FollowTarget;

				var refinery = target.GetComponent<Refinery>();

				if (refinery)
				{
					SetRefinery(refinery);

					if (HarvestedResources > 0)
						SetHarvestState(HarvestState.MoveToRefinery);

					return;
				}
			}

			SetHarvestState(HarvestState.Idle);
		}

		public void SetRefinery(Refinery refinery) => nearestRefinery = refinery;
	}
}