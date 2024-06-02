using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using AYellowpaper;

using NeuroRehab.Interfaces;

namespace NeuroRehab.UI.Managers.ColorPicker {
	public class ColorPickerManager : MonoBehaviour, IPointerClickHandler {
		[Tooltip("Objects of type 'IColorContainer' only here, we have to 'Hack' it because Interface isn't serializable")]
		[SerializeField] private InterfaceReference<IColorContainer> colorContainer; // small hack because we can't serialize Interface

		[SerializeField] private RectTransform colorPickerRectTransform;
		[SerializeField] private RectTransform maskRectTransform;
		[SerializeField] private Texture2D colorSprite;
		[SerializeField] private List<Image> targetImages = new();
		[SerializeField] List<GameObject> colorPaletteObjects = new();
		private Vector2 colorPickerScale;
		private Vector2 colorPickerOffset;
		private bool isShowing = false;

		private Color chosenColor = Color.white;

		private Color ChosenColor {
			get => chosenColor;
			set {
				chosenColor = value;
				colorContainer.Value.SetColor(chosenColor);
			}
		}

		void Start() {
			colorContainer.Value.GetColor();

			colorPickerScale = new Vector2(colorSprite.width / colorPickerRectTransform.sizeDelta.x, colorSprite.height / colorPickerRectTransform.sizeDelta.y);
			colorPickerOffset = new Vector2(maskRectTransform.localPosition.x - maskRectTransform.sizeDelta.x / 2, maskRectTransform.localPosition.y - maskRectTransform.sizeDelta.y / 2);
			ChangeElementsColor();
			SetColorPaletteVisibility();
		}

		public void OnPointerClick(PointerEventData eventData) {
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector2 clickPosition)) {
				return;
			}
			//Debug.Log(clickPosition);
			//Debug.Log($"{(clickPosition.x - colorPickerOffset.x) * colorPickerScale.x}, {(clickPosition.y - colorPickerOffset.y) * colorPickerScale.y}");

			Vector2 scaledPosition = new(x: (clickPosition.x - colorPickerOffset.x) * colorPickerScale.y, y: (clickPosition.y - colorPickerOffset.y) * colorPickerScale.y);

			ChosenColor = colorSprite.GetPixel((int)scaledPosition.x, (int)scaledPosition.y);

			ChangeElementsColor();
		}

		private void ChangeElementsColor() {
			foreach (Image targetImage in targetImages) {
				targetImage.color = chosenColor;
			}
		}

		public void TriggerColorPaletter() {
			isShowing = !isShowing;

			SetColorPaletteVisibility();
		}

		private void SetColorPaletteVisibility() {
			foreach (GameObject colorPaletteObject in colorPaletteObjects) {
				colorPaletteObject.SetActive(isShowing);
			}
		}
	}
}