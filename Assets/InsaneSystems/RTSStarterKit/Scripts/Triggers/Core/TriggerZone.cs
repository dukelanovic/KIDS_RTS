using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	[RequireComponent(typeof(BoxCollider))]
	[RequireComponent(typeof(Rigidbody))]
	public sealed class TriggerZone : MonoBehaviour
	{
		[Comment("Use trigger zones to run a trigger under a certain condition.")] 
		[SerializeField] TriggerCondition conditionToStartTriggers;
		
		[Tooltip("Add here text ids of all triggers you want to call when this zone being triggered. You should setup text id of triggers in Trigger Editor to call them.")]
		[SerializeField][Trigger] List<string> triggersToCall = new List<string>();
		[SerializeField] bool removeThisZoneAfterTrigger = true;
		[SerializeField] Color zoneColor = new Color(0.6f, 0f, 1f, 1f);

		void Start()
		{
			gameObject.layer = Globals.LayerIgnoreRaycast;

			if (!GetComponent<BoxCollider>())
				gameObject.AddComponent<BoxCollider>();

			var boxCollider = GetComponent<BoxCollider>();
			boxCollider.isTrigger = true;

			if (!GetComponent<Rigidbody>())
				gameObject.AddComponent<Rigidbody>();

			GetComponent<Rigidbody>().isKinematic = true;
		}

		void Update()
		{
			if (conditionToStartTriggers.ConditionType == TriggerConditionType.ByTimeLeft)
				if (conditionToStartTriggers.IsConditionTrue())
					DoTriggerAction();
		}

		void OnTriggerEnter(Collider other)
		{
			if (conditionToStartTriggers.ConditionType != TriggerConditionType.ByEnteringZoneUnits)
				return;

			var otherUnit = other.GetComponent<Unit>();

			if (!otherUnit)
				return;

			OnTriggerEnterExitCheck(otherUnit, true);
		}

		void OnTriggerExit(Collider other)
		{
			if (conditionToStartTriggers.ConditionType != TriggerConditionType.ByExitingZoneUnits)
				return;

			var otherUnit = other.GetComponent<Unit>();

			if (!otherUnit)
				return;

			OnTriggerEnterExitCheck(otherUnit, false);
		}

		void OnTriggerEnterExitCheck(Unit unitTriggeredZone, bool isEnter)
		{
			if (!conditionToStartTriggers.IsConditionTrue(unitTriggeredZone))
				return;

			DoTriggerAction();
		}

		void DoTriggerAction()
		{
			for (int i = 0; i < triggersToCall.Count; i++)
				TriggerController.Instance.ExecuteTrigger(triggersToCall[i]);

			if (removeThisZoneAfterTrigger)
				Destroy(gameObject);
		}

		void OnDrawGizmos()
		{
			var boxCollider = GetComponent<BoxCollider>();
			var scaleModifier = Vector3.one;

			if (boxCollider)
				scaleModifier = boxCollider.size;

			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.3f);
			Gizmos.DrawCube(Vector3.zero, scaleModifier);
			Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
			Gizmos.DrawWireCube(Vector3.zero, scaleModifier);
		}
	}
}