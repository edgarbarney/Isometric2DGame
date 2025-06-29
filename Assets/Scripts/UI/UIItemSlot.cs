using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Isometric2DGame.Characters.Player;

namespace Isometric2DGame.UI
{
	public class UIItemSlot : MonoBehaviour
	{
		[SerializeField]
		private Image myImage;
		public Image MyImage
		{
			get { return myImage; }
		}
		[SerializeField]
		private Image myChildImage;
		public Image MyChildImage
		{
			get { return myChildImage; }
		}
		[SerializeField]
		private TextMeshProUGUI myCountText;
		public TextMeshProUGUI MyCountText
		{
			get { return myCountText; }
		}

		private int slotIndex = -1;
		public int SlotIndex
		{
			get { return slotIndex; }
		}
		private Sprite emptySprite;

		private Color unselectedColor;
		public Color UnselectedColor
		{
			get { return unselectedColor; }
		}
		public Color selectedColor;

		private void Awake()
		{
			emptySprite = myChildImage.sprite;
			unselectedColor = myImage.color;
		}

		public void SetSlot(int index)
		{
			slotIndex = index;
			InventorySlot slotData = PlayerInventory.Instance.ItemSlots[slotIndex];

			if (slotData == null || slotData.Item == null)
			{
				myChildImage.sprite = emptySprite;
				myCountText.text = string.Empty;
				return;
			}

			myChildImage.sprite = slotData.Item != null ? slotData.Item.ItemIcon : emptySprite;
			myCountText.text = slotData.Count > 1 ? slotData.Count.ToString() : string.Empty;
		}

		public void OnClick()
		{
			PlayerInventory.Instance.SetSelectedItem(this);
		}
	}
}
