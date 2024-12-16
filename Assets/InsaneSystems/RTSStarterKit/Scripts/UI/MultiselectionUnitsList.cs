using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public sealed class MultiselectionUnitsList : MonoBehaviour
	{
		const float UpdateTime = 0.25f;
		const float PooledIconsCount = 30;

		readonly List<GameObject> pooledIcons = new List<GameObject>();

		[SerializeField] GameObject selfObject;
		[SerializeField] RectTransform unitsIconsParent;

		readonly List<MultiselectionUnitIcon> drawnIcons = new List<MultiselectionUnitIcon>();

		float updateTimer;

		bool isPoolLoaded;

		void Start()
		{
			Controls.Selection.UnitsListWasSelected += OnMultiselection;
			Controls.Selection.UnitsListWasChanged += OnMultiselection;
			Controls.Selection.SelectionCleared += OnClearSelection;

			LoadPool();

			Hide();
		}

		void OnDestroy()
		{
			Controls.Selection.UnitsListWasSelected -= OnMultiselection;
			Controls.Selection.UnitsListWasChanged -= OnMultiselection;
			Controls.Selection.SelectionCleared -= OnClearSelection;
		}
		
		void LoadPool()
		{
			if (isPoolLoaded)
				return;

			for (int i = 0; i < PooledIconsCount; i++)
			{
				var spawnedIconObject = Instantiate(GameController.Instance.MainStorage.unitMultiselectionIconTemplate, unitsIconsParent);
				spawnedIconObject.SetActive(false);

				pooledIcons.Add(spawnedIconObject);
			}

			isPoolLoaded = true;
		}

		void Update()
		{
			if (!selfObject.activeSelf)
				return;

			if (updateTimer > 0)
			{
				updateTimer -= Time.deltaTime;
				return;
			}

			for (int i = 0; i < drawnIcons.Count; i++)
				if (drawnIcons[i] != null)
					drawnIcons[i].UpdateHealthBar();

			updateTimer = UpdateTime;
		}

		public void OnClearSelection() => Hide();

		public void OnMultiselection(List<Unit> selectedUnits)
		{
			if (selectedUnits.Count < 2)
				return;

			Show();
			ClearDrawn();

			var iconTemplate = GameController.Instance.MainStorage.unitMultiselectionIconTemplate;

			var iconsCount = Mathf.Min(selectedUnits.Count, GameController.Instance.MainStorage.unitIconsLimitInMultiselectionUI);

			if (iconsCount == 0)
				iconsCount = selectedUnits.Count;

			for (int i = 0; i < iconsCount; i++)
			{
				GameObject iconObject = null;

				if (pooledIcons.Count > 0)
					iconObject = TakeIconFromPool();
				else
					iconObject = Instantiate(iconTemplate, unitsIconsParent);
				
				var spawnedIconComponent = iconObject.GetComponent<MultiselectionUnitIcon>();
				spawnedIconComponent.SetupWithUnit(selectedUnits[i]);

				drawnIcons.Add(spawnedIconComponent);
			}
		}

		void ClearDrawn()
		{
			for (int i = 0; i < drawnIcons.Count; i++)
				if (drawnIcons[i] != null)
					MoveIconToPool(drawnIcons[i].gameObject);

			drawnIcons.Clear();
		}

		void Show() => selfObject.SetActive(true);
		void Hide() => selfObject.SetActive(false);

		GameObject TakeIconFromPool()
		{
			var iconFromPool = pooledIcons[0];
			pooledIcons.RemoveAt(0);

			iconFromPool.SetActive(true);

			return iconFromPool;
		}

		void MoveIconToPool(GameObject iconObject)
		{
			pooledIcons.Add(iconObject);
			iconObject.SetActive(false);
		}
	}
}