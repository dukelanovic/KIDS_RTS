using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class BuildingDrawer : MonoBehaviour
	{
		[SerializeField] protected Renderer[] renderers;
		
		static readonly int tintColor = Shader.PropertyToID("_TintColor");

		public virtual void SetBuildingAllowedState(bool isAllowed) => SetMeshesColor(isAllowed ? Color.green : Color.red);

		protected virtual void SetMeshesColor(Color color)
		{
			for (int i = 0; i < renderers.Length; i++)
				renderers[i].material.SetColor(tintColor, color);
		}
	}
}