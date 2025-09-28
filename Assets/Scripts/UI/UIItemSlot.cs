using Isometric2DGame.Characters.Player;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Isometric2DGame.UI
{
	public class UIItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
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
		public Color unselectedColour;
		public Color selectedColour;
		private Color emptyItemColour = new(0.0f, 0.0f, 0.0f, 0.0f);
		private Color normalItemColour = new(1.0f, 1.0f, 1.0f, 1.0f);

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
			myChildImage.color = normalItemColour;
			myCountText.text = slotData.Count > 1 ? slotData.Count.ToString() : string.Empty;
		}

		private void ClearSlot()
		{
			myChildImage.sprite = emptySprite;
			myChildImage.color = emptyItemColour;
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

		// Drag and Drop
		public void OnBeginDrag(PointerEventData eventData)
		{
			if (PlayerInventory.Instance.IsSlotEmpty(slotIndex))
			{
				return;
			}

			var slotData = PlayerInventory.Instance.ItemSlots[slotIndex];

			PlayerInventory.Instance.DraggedSlot = this;
			PlayerInventory.Instance.uISlotDragIcon.gameObject.SetActive(true);
			PlayerInventory.Instance.uISlotDragIcon.SetItem(slotData.Item, slotData.Count);
		}

		public void OnDrag(PointerEventData eventData)
		{
			//PlayerInventory.Instance.uISlotDragIcon.transform.position = eventData.position;
		}
		
		public void OnEndDrag(PointerEventData eventData)
		{
			PlayerInventory.Instance.DraggedSlot = null;
			PlayerInventory.Instance.uISlotDragIcon.gameObject.SetActive(false);
			PlayerInventory.Instance.uISlotDragIcon.SetItem(null, 0);
		}

		public void OnDrop(PointerEventData eventData)
		{
			if (PlayerInventory.Instance.DraggedSlot == null || PlayerInventory.Instance.DraggedSlot == this)
			{ 
				return;
			}

			PlayerInventory.Instance.SwapSlots(PlayerInventory.Instance.DraggedSlot.SlotIndex, this.SlotIndex);
		}
	}
}
