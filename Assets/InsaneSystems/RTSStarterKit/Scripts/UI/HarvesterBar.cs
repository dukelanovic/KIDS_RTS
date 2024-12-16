using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public sealed class HarvesterBar : MonoBehaviour
	{
		static Camera mainCamera;
		static readonly Dictionary<Harvester, HarvesterBar> spawnedBars = new Dictionary<Harvester, HarvesterBar>();

		[SerializeField] Image fillBar;
		RectTransform rectTransform;

		public Harvester harvester { get; private set; }

		void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			rectTransform.SetAsFirstSibling();

			if (!mainCamera)
				mainCamera = Camera.main;
		}

		void Update()
		{
			if (!harvester)
			{
				Destroy(gameObject);
				return;
			}

			rectTransform.anchoredPosition = mainCamera.WorldToScreenPoint(harvester.transform.position + Vector3.up);
		}

		void OnDestroy()
		{
			if (harvester)
				harvester.ResourcesChanged -= OnResourcesChanged;
		}

		/// <summary> Should be called on every game match initialization. </summary>
		public static void Init()
		{
			spawnedBars.Clear();
		}
		
		public void SetupWithHarvester(Harvester harvester)
		{
			this.harvester = harvester;

			harvester.ResourcesChanged += OnResourcesChanged;

			OnResourcesChanged(harvester.HarvestedResources, harvester.MaxResources);
		}

		void OnResourcesChanged(float newValue, float maxValue) => fillBar.fillAmount = newValue / maxValue;

		public static void RemoveBarOfHarvester(Harvester harvester)
		{
			if (spawnedBars.TryGetValue(harvester, out var result))
			{
				Destroy(result.gameObject);
				spawnedBars.Remove(harvester);
			}
		}

		public static void SpawnForHarvester(Harvester harvester)
		{
			var spawnedBar = Instantiate(GameController.Instance.MainStorage.harvesterBarTemplate, UIController.Instance.MainCanvas.transform);
			var harvesterBar = spawnedBar.GetComponent<HarvesterBar>();

			harvesterBar.SetupWithHarvester(harvester);

			spawnedBars.Add(harvester, harvesterBar);
		}

	}
}