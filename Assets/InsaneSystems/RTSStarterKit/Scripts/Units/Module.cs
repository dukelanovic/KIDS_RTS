using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Module : MonoBehaviour
	{
		public Unit SelfUnit
		{
			get
			{
				if (!selfUnit)
					selfUnit = GetComponent<Unit>();

				return selfUnit;
			}
		}

		Unit selfUnit;
		
		protected void Awake()
		{
			SelfUnit.RegisterModule(this);
			AwakeAction();
		}
		
		protected virtual void AwakeAction() { }
	}
}