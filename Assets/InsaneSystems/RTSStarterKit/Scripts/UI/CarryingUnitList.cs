using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public sealed class CarryingUnitList : MonoBehaviour
	{
		const float UpdateTime = 0.25f;
		const float PooledIconsCount = 10;

		[SerializeField] GameObject selfObject;
		[SerializeField] RectTransform unitsIconsParent;
		
		readonly List<GameObject> pooledIcons = new List<GameObject>();
		readonly List<CarryingUnitIcon> drawnIcons = new List<CarryingUnitIcon>();

		float updateTimer;

		bool isPoolLoaded;

		Unit selectedUnit;

		void Start()
		{
			Controls.Selection.UnitSelected += OnUnitSelected;
			Controls.Selection.SelectionCleared += OnClearSelection;

			LoadPool();

			Hide();
		}

		void LoadPool()
		{
			if (isPoolLoaded)
				return;

			for (int i = 0; i < PooledIconsCount; i++)
			{
				var spawnedIconObject = Instantiate(GameController.Instance.MainStorage.unitCarryingIcon, unitsIconsParent);
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

			updateTimer = UpdateTime;
		}
		
		void OnDestroy()
		{
			Controls.Selection.UnitSelected -= OnUnitSelected;
			Controls.Selection.SelectionCleared -= OnClearSelection;
		}

		public void OnClearSelection() => Hide();

		public void OnUnitSelected(Unit unit)
		{
			selectedUnit = unit;

			Redraw();
		}

		public void Redraw()
		{
			if (!selectedUnit)
				return;

			var carryModule = selectedUnit.GetModule<CarryModule>();

			if (!carryModule)
				return;

			Show();
			ClearDrawn();

			var iconTemplate = GameController.Instance.MainStorage.unitMultiselectionIconTemplate;

			var carryingUnits = carryModule.GetCarryingUnits();
			for (int i = 0; i < carryingUnits.Count; i++)
			{
				GameObject iconObject;

				if (pooledIcons.Count > 0)
					iconObject = TakeIconFromPool();
				else
					iconObject = Instantiate(iconTemplate, unitsIconsParent);

				var spawnedIconComponent = iconObject.GetComponent<CarryingUnitIcon>();
				spawnedIconComponent.Setup(carryingUnits[i], selectedUnit, this);

				drawnIcons.Add(spawnedIconComponent);
			}

			if (carryingUnits.Count == 0)
				Hide();
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

		public void MoveIconToPool(GameObject iconObject)
		{
			if (!pooledIcons.Contains(iconObject))
				pooledIcons.Add(iconObject);

			iconObject.SetActive(false);
		}
	}
}