using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Isometric2DGame.Items;

namespace Isometric2DGame.UI
{
	public class UISlotDragIcon : MonoBehaviour
	{
		private Image myImage;
		private Sprite defaultSprite;
		private TextMeshProUGUI itemCountText;

		private void Awake()
		{
			myImage = GetComponent<Image>();
			defaultSprite = myImage.sprite;
			itemCountText = GetComponentInChildren<TextMeshProUGUI>();
		}

		private void Update()
		{
			if (Mouse.current != null && Mouse.current.position != null)
			{
				Vector2 mousePos = Mouse.current.position.ReadValue();
				transform.position = mousePos;
			}
		}

		public void SetItem(BaseItem item, int count)
		{
			if (item == null)
			{
				myImage.sprite = defaultSprite;
				itemCountText.text = string.Empty;
				return;
			}

			myImage.sprite = item.ItemIcon;
			itemCountText.text = count > 1 ? count.ToString() : string.Empty;
		}
	}
}