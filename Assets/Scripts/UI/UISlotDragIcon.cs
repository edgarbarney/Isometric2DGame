using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Isometric2DGame.Items;

namespace Isometric2DGame.UI
{
	public class UISlotDragIcon : MonoBehaviour
	{
		private Image myImage;
		private Sprite defaultSprite;

		private void Awake()
		{
			myImage = GetComponent<Image>();
			defaultSprite = myImage.sprite;
		}

		private void Update()
		{
			if (Mouse.current != null && Mouse.current.position != null)
			{
				Vector2 mousePos = Mouse.current.position.ReadValue();
				transform.position = mousePos;
			}
		}

		public void SetItem(BaseItem item)
		{
			if (item == null)
			{
				myImage.sprite = defaultSprite;
				return;
			}

			myImage.sprite = item.ItemIcon;
		}
	}
}