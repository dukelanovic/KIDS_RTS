using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.Menus
{
	public sealed class ColorDropdown : MonoBehaviour
	{
		[SerializeField] Image selectedColorImage;
		[SerializeField] RectTransform colorsPanel;

		Lobby parentLobby;
		PlayerEntry selfPlayerEntry;

		Color currentColor;

		readonly List<GameObject> drawnObjects = new List<GameObject>();

		void Start() => HideColors();
		
		void OnDestroy()
		{
			if (parentLobby)
				parentLobby.FreeColorsChanged -= Redraw;
			
			ClearDrawnColors();
		}

		public void SetupWithData(Lobby lobby, PlayerEntry playerEntry)
		{
			parentLobby = lobby;
			selfPlayerEntry = playerEntry;

			lobby.FreeColorsChanged += Redraw;

			Redraw();
		}

		public void SetColorValue(Color color)
		{
			currentColor = color;

			if (selectedColorImage)
				selectedColorImage.sprite = CreateSpriteByColor(color);
		}

		public void OnMainObjectClick() => ShowColors();

		public void Redraw()
		{
			GenerateOptionsByColors(parentLobby.GetFreeColorsForPlayer(selfPlayerEntry.SelfPlayerSettings));
		}

		void GenerateOptionsByColors(List<Color> colors)
		{
			ClearDrawnColors();

			for (int i = 0; i < colors.Count; i++)
			{
				var color = colors[i];

				var colorObject = new GameObject($"Color: {colors[i]}");
				colorObject.transform.SetParent(colorsPanel);

				var colorImage = colorObject.AddComponent<Image>();
				var colorSprite = CreateSpriteByColor(colors[i]);

				colorImage.sprite = colorSprite;

				var colorButton = colorObject.AddComponent<Button>();
				colorButton.onClick.AddListener(() => ColorSelectButtonAction(color));

				drawnObjects.Add(colorObject);
			}

			SetColorValue(currentColor);
		}

		void ColorSelectButtonAction(Color color)
		{
			selfPlayerEntry.OnColorDropdownChanged(color);
			SetColorValue(color);
			HideColors();
		}

		void ClearDrawnColors()
		{
			for (int i = 0; i < drawnObjects.Count; i++)
				Destroy(drawnObjects[i]);

			drawnObjects.Clear();
		}

		Sprite CreateSpriteByColor(Color color)
		{
			var colorTexture = new Texture2D(1, 1);
			colorTexture.SetPixel(0, 0, color);
			colorTexture.Apply();

			var colorSprite = Sprite.Create(colorTexture, new Rect(0, 0, 1, 1), Vector2.zero);

			return colorSprite;
		}

		void HideColors() => colorsPanel.gameObject.SetActive(false);
		void ShowColors() => colorsPanel.gameObject.SetActive(true);
	}
}