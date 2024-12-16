using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace InsaneSystems.RTSStarterKit.UI
{
	public sealed class Minimap : MonoBehaviour
	{
		const float RadarUpdateDelay = 0.25f;
		
		[SerializeField] RectTransform iconsPanel;
		[SerializeField] RawImage mapBackground;
		[SerializeField] RawImage fowImage;

		Dictionary<Unit, Image> unitsIcons = new Dictionary<Unit, Image>();

		float radarUpdateTimer;

		float mapSize = 256; 
		float final2DScale;

		public float MapImageSize { get; private set; }

		public RectTransform IconsPanel => iconsPanel;

		bool isFOWUsed;

		void Awake()
		{
			Unit.UnitSpawned += OnSpawnedUnit;
			Unit.UnitChangedOwner += OnChangedUnitOwner;
			
			MapImageSize = iconsPanel.sizeDelta.x;
		}

		void Start()
		{
			mapSize = MatchSettings.CurrentMatchSettings.SelectedMap.mapSize;
			final2DScale = iconsPanel.sizeDelta.x / mapSize;
			
			isFOWUsed = GameController.Instance.MainStorage.isFogOfWarOn;
			
			fowImage.enabled = isFOWUsed;
		}

		void Update()
		{
			radarUpdateTimer -= Time.deltaTime;

			if (radarUpdateTimer <= 0)
			{
				foreach (var entry in unitsIcons)
				{
					if (entry.Key == null)
					{
						Destroy(entry.Value.gameObject);

						continue;
					}

					var unit = entry.Key;
					var unitIcon = entry.Value;

					if (isFOWUsed)
						unitIcon.enabled = unit.GetModule<FogOfWarModule>().IsVisibleInFOW;
					
					unitIcon.rectTransform.anchoredPosition = GetUnitOnMapPoint(unit, true);
				}
				
				// todo insane systems - optimize
				unitsIcons = (from icon in unitsIcons
							  where icon.Key != null
							  select icon).ToDictionary(icon => icon.Key, icon => icon.Value);

				radarUpdateTimer = RadarUpdateDelay;
			}
		}

		void OnDestroy()
		{
			Unit.UnitSpawned -= OnSpawnedUnit;
			Unit.UnitChangedOwner -= OnChangedUnitOwner;
		}
		
		void OnSpawnedUnit(Unit unit)
		{
			var spawnedIconObject = Instantiate(GameController.Instance.MainStorage.unitMinimapIconTemplate, iconsPanel);
			var iconImage = spawnedIconObject.GetComponent<Image>();

			iconImage.color = Player.GetPlayerById(unit.OwnerPlayerId).Color;
			unitsIcons.Add(unit, iconImage);
		}

		void OnChangedUnitOwner(Unit unit, int newOwnerPlayerId, int previousOwner)
		{
			if (unitsIcons.TryGetValue(unit, out var image))
				image.color = Player.GetPlayerById((byte)newOwnerPlayerId).Color;
		}

		public Image GetUnitIcon(Unit unit)
		{
			return unitsIcons.TryGetValue(unit, out var icon) ? icon : null;
		}

		public Vector2 GetUnitOnMapPoint(Unit unit, bool scaledToMap = false)
		{
			var result = GetPositionInMapCoords(unit.transform.position);

			if (scaledToMap)
				result *= final2DScale;
			
			return result;
		}

		public Vector2 GetPositionInMapCoords(Vector3 position)
		{
			var resultPosition = new Vector2(Mathf.CeilToInt(position.x), Mathf.CeilToInt(position.z));
			resultPosition.x = Mathf.Clamp(resultPosition.x, 0, mapSize);
			resultPosition.y = Mathf.Clamp(resultPosition.y, 0, mapSize);

			return resultPosition;
		}

		public static Vector3 GetMapPointInWorldCoords(Vector2 mapPoint)
		{
			var resultPosition = new Vector3(mapPoint.x, 0f, mapPoint.y);

			return resultPosition;
		}

		public void SetMapBackground(Texture2D texture) => mapBackground.texture = texture;
		public void SetFowTexture(Texture2D texture) => fowImage.texture = texture;

		public float GetMapSize() => mapSize;
		public float GetDrawPanelSize() => iconsPanel.sizeDelta.x;
		public float GetScaleFactor() => final2DScale;

		public static Vector2 InboundPositionToMap(Vector2 position, float mapImageSize)
		{
			position.x = Mathf.Clamp(position.x, 0, mapImageSize);
			position.y = Mathf.Clamp(position.y, 0, mapImageSize);

			return position;
		}
	}
}