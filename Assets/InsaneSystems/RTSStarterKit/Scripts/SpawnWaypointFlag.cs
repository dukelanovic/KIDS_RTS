using UnityEngine;
using UnityEngine.AI;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class SpawnWaypointFlag : MonoBehaviour
	{
		GameObject selfObject;
		new Transform transform;

		Production currentProduction;

		void Awake()
		{
			selfObject = gameObject;
			transform = GetComponent<Transform>();
		}

		void Start()
		{
			Production.ProductionSelected += OnProductionSelected;
			Production.ProductionUnselected += OnProductionUnselected;

			Hide();
		}

		void OnDestroy()
		{
			Production.ProductionSelected -= OnProductionSelected;
			Production.ProductionUnselected -= OnProductionUnselected;
		}
		
		void Update()
		{
			if (Input.GetMouseButtonDown(1) && currentProduction != null)
			{
				if (currentProduction.SpawnWaypoint == null)
				{
					Debug.LogWarning("No Move Waypoint setted up in selected Production. Please, set up this setting in component of prefab.");
					return;
				}

				NavMesh.SamplePosition(Controls.InputHandler.CurrentCursorWorldHit.point, out var navHit, 10, NavMesh.AllAreas);

				currentProduction.SpawnWaypoint.position = navHit.position;
				ShowAtPoint(currentProduction.SpawnWaypoint.position);
			}
		}

		void OnProductionSelected(Production production)
		{
			currentProduction = production;

			ShowAtPoint(production.SpawnWaypoint ? production.SpawnWaypoint.position : production.transform.position);
		}

		void OnProductionUnselected(Production production) => Hide();

		void ShowAtPoint(Vector3 point)
		{
			selfObject.SetActive(true);
			transform.position = point;
		}

		void Hide()
		{
			currentProduction = null;

			selfObject.SetActive(false);
		}
	}
}