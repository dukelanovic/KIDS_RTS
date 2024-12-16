using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Abilities 
{
	/// <summary> Just example component for ability, which need additional info from unit object.
	/// Add this component to your unit prefab and now you can get it from ability code using unitOwner.GetComponent(); </summary>
	public class AbilityAdditionalSettings : MonoBehaviour
	{
		public Transform CustomTransform;
		public GameObject CustomGameObject;
	}
}