using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Controls;
using UnityEditor.SearchService;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class ResourcesField : MonoBehaviour
	{
		public static List<ResourcesField> SceneResourceFields { get; } = new List<ResourcesField>();
		
		[Tooltip("Resources will not end on this field, if you set this to true.")]
		[SerializeField] protected bool infResources = true;
		[Tooltip("Resources count on this field. This value will be ignored if Inf Resources set.")]
		[SerializeField] protected int resourcesAmount = 5000;
		
		void Awake() => SceneResourceFields.Add(this);
		void OnDestroy() => SceneResourceFields.Remove(this);
		
		/// <summary> Call on game start. </summary>
		public static void Init() => SceneResourceFields.Clear();

		public virtual void OnMouseEnter()
		{
			if (Selection.SelectedUnits.Count == 0 || !Selection.SelectedUnits[0].Data.isHarvester)
				return;
			
			var selectedHarvester = Selection.SelectedUnits[0].GetModule<Harvester>();
			var needResourcesCursour = selectedHarvester.HarvestedResources < selectedHarvester.MaxResources;
			
			if (needResourcesCursour)
				Cursors.SetResourcesCursor();
			else
				Cursors.SetRestrictCursor();
		}
		
		public virtual void OnMouseExit() => Cursors.SetDefaultCursor();

		public virtual int TakeResources(int value)
		{
			if (infResources)
				return value;

			if (resourcesAmount >= value)
			{
				resourcesAmount -= value;
				
				return value;
			}

			var maxVal = resourcesAmount;
			resourcesAmount = 0;

			return maxVal;
		}

		public virtual bool HaveResources() => infResources || resourcesAmount > 0;
	}
}