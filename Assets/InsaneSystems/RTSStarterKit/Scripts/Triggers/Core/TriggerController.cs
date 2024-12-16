using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public sealed class TriggerController : MonoBehaviour
	{
		public static TriggerController Instance { get; private set; }

		[SerializeField] TriggerData[] triggerDatas = Array.Empty<TriggerData>();

		void Awake()
		{
			if (Instance)
			{
				Debug.LogWarning("Several Trigger Controllers found on scene. All exclude one will be disabled. Your scene should have only one TriggerController.");
				enabled = false;
			}
			else
			{
				Instance = this;
			}
		}

		public void ExecuteTrigger(string triggerTextId)
		{
			for (int i = 0; i < triggerDatas.Length; i++)
				if (triggerDatas[i].TriggerTextId == triggerTextId && triggerDatas[i].Trigger)
				{
					triggerDatas[i].Trigger.Execute();
					return;
				}

			Debug.LogWarning("No trigger with name " + triggerTextId + " found!");
		}

		public int GetTriggerIndexByName(string name)
		{
			for (int i = 0; i < triggerDatas.Length; i++)
				if (triggerDatas[i].TriggerTextId == name)
					return i;

			return -1;
		}

		public string GetNameByIndex(int index)
		{
			if (index < triggerDatas.Length)
				return triggerDatas[index].TriggerTextId;

			return "NO SUCH TRIGGER";
		}

		public string[] GetTriggersNames()
		{
			var names = new string[triggerDatas.Length];

			for (int i = 0; i < triggerDatas.Length; i++)
				names[i] = triggerDatas[i].TriggerTextId;

			return names;
		}

		public int GetTriggersCount() => triggerDatas.Length;
	}
}