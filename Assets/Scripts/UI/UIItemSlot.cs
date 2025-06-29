using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Isometric2DGame.Characters.Player;

namespace Isometric2DGame.UI
{
	public class UIItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

		public Sprite emptySprite;
		public Color unselectedColor;
		public Color selectedColor;

		public void SetSlot(int index)
		{
			slotIndex = index;

			if (index < 0 || index >= PlayerInventory.Instance.ItemSlots.Length)
			{
				ClearSlot();
				return;
			}

			InventorySlot slotData = PlayerInventory.Instance.ItemSlots[index];
			if (slotData.Item == null)
			{
				ClearSlot();
				return;
			}

			myChildImage.sprite = slotData.Item.ItemIcon;
			myCountText.text = slotData.Count > 1 ? slotData.Count.ToString() : string.Empty;
		}

		private void ClearSlot()
		{
			myChildImage.sprite = emptySprite;
			myCountText.text = string.Empty;
		}

		public void OnClick()
		{
			PlayerInventory.Instance.SetSelectedItem(this);
		}

		// Hover
		public void OnPointerEnter(PointerEventData eventData)
		{
			PlayerInventory.Instance.HoverSlot(slotIndex);
		}

		// Exit hover
		public void OnPointerExit(PointerEventData eventData)
		{
			PlayerInventory.Instance.ExitHoverSlot();
		}
	}
}
