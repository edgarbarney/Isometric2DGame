using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Isometric2DGame.Items;
using Isometric2DGame.UI;

namespace Isometric2DGame.Characters.Player
{
	[System.Serializable]
	public class InventorySlot
	{
		public BaseItem Item;
		public int Count;

		public InventorySlot(BaseItem item, int count = 1)
		{
			Item = item;
			Count = count;
		}

		public static InventorySlot EmptySlot()
		{
			return new InventorySlot(null, 0);
		}

		public bool IsFullStacked()
		{
			return Item != null && Count >= Item.MaxStack;
		}

		public bool IsEmpty()
		{
			return Item == null || Count <= 0;
		}

		public void Clear()
		{
			Item = null;
			Count = 0;
		}
	}

	[System.Serializable]
	public class PlayerInventory : MonoBehaviour
	{
		private static PlayerInventory instance;
		public static PlayerInventory Instance
		{
			get
			{
				if (instance == null)
				{
					instance = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
				}

				return instance;
			}
		}

		private PlayerController playerController;
		private GameObject inventoryUISlotPrefab;

		[SerializeField]
		private List<InventorySlot> itemSlots = new();
		public InventorySlot[] ItemSlots => itemSlots.ToArray();

		private UIItemSlot lastSelectedItemSlot = null;
		public UIItemSlot LastSelectedItemSlot
		{
			get { return lastSelectedItemSlot; }
			set { lastSelectedItemSlot = value; }
		}

		private List<UIItemSlot> uiItemSlots = new(); // Cache of UI item slots for quick access

		public RectTransform inventoryUISlotHolder;
		public UIPickupItemPrompt uIPickupItemPrompt;
		public UIItemSlotInfo uIItemSlotInfo;

		private bool isInventoryUIOpen = false;
		public bool IsInventoryUIOpen
		{
			get { return isInventoryUIOpen; }
		}

		[SerializeField]
		private int maxSlots = 20;
		[SerializeField]
		private float minSlotUIPadding = 20f; // Minimum padding around each inventory slot in the UI

		private DroppedItem possiblePickup; // The item that the player can pick up on demand.
		public DroppedItem PossiblePickup
		{
			get => possiblePickup;
			private set
			{
				possiblePickup = value;
				uIPickupItemPrompt.SetPickupItem(possiblePickup);
				uIPickupItemPrompt.gameObject.SetActive(possiblePickup != null);

				uIPickupItemPrompt.RefreshPickupKey(playerController);
			}
		}
		
		[SerializeField]
		private float pickupRange = 0.5f; // Range within which the player can pick up items
		[SerializeField]
		private float pickupDelay = 0.1f; // Delay between pickup attempts
		private float lastPickupTime = 0f;

		private void Awake()
		{
			// Component caching.
			playerController = GetComponent<PlayerController>();
			inventoryUISlotPrefab = Resources.Load<GameObject>("Prefabs/UI/InventoryItemSlot");

			if (inventoryUISlotHolder == null)
			{
				Debug.LogError("InventorySlotHolder not found in the scene. Please set it in the inspector");
			}

			if (uIPickupItemPrompt == null)
			{
				Debug.LogError("UIPickupItemPrompt not found in the scene. Please set it in the inspector");
			}

			if (uIItemSlotInfo == null)
			{
				Debug.LogError("UIItemSlotInfo not found in the scene. Please set it in the inspector");
			}

			// Pre-allocate the inventory slots
			itemSlots = new List<InventorySlot>(maxSlots);
			Resize(maxSlots); // Initialize the inventory with the maximum slots

			uiItemSlots = new List<UIItemSlot>(maxSlots);
		}

		private void Start()
		{
			// No need to check it every frame
			// Every tenth of a second is enough to check for pickups
			InvokeRepeating(nameof(CheckForPickups), 0.0f, 0.1f);

			// Set indices
			RefreshUISlots();

			// Make sure the inventory UI is resized correctly
			Invoke(nameof(ResizeUI), 0.1f); // Delay to ensure the UI is ready before resizing
		}

		private void Update()
		{
			if (possiblePickup == null)
				return;

			Vector2 tooltipPos = playerController.PlayerCamera.WorldToScreenPoint(PossiblePickup.GetTooltipPosition());
			uIPickupItemPrompt.SetPosition(tooltipPos);
		}

		private void CheckForPickups()
		{
			Collider2D[] colliders = Physics2D.OverlapCircleAll(playerController.transform.position, pickupRange, LayerMask.GetMask("ItemPickup"));
			float closestDistance = float.MaxValue;

			if (colliders == null || colliders.Length == 0)
			{
				PossiblePickup = null; // No pickups in range
				return;
			}

			foreach (var collider in colliders)
			{
				DroppedItem item = collider.GetComponent<DroppedItem>();
				if (item != null && item.Item != null)
				{
					float distance = Vector2.Distance(playerController.transform.position, item.transform.position);
					if (distance < closestDistance)
					{
						closestDistance = distance;
						PossiblePickup = item; // Closest pickup!
					}
				}
			}
		}

		// Fills the inventory slot at the given index with the provided InventorySlot.
		// Pass a new reference to the InventorySlot to fill it.
		private void FillSlot(int index, InventorySlot slot)
		{
			itemSlots[index] = slot;
		}

		// Fills the first available slot with the provided InventorySlot.
		private int FillAvailableSlot(InventorySlot slot)
		{
			if (slot == null || slot.IsEmpty())
				return -1;

			if (IsFull())
				return -1;

			for (int i = 0; i < itemSlots.Count; i++)
			{
				if (IsSlotEmpty(i))
				{
					itemSlots[i] = slot;
					return i;
				}
			}

			return -1; // Impossible
		}

		private void ClearSlot(int index = 0)
		{
			itemSlots[index].Clear();
		}

		public bool IsSlotEmpty(int index)
		{
			if (index < 0 || index >= itemSlots.Count)
				return true; // Out of bounds, consider it empty

			return itemSlots[index].IsEmpty();
		}

		public bool IsSlotEmpty(InventorySlot slot)
		{
			return slot.IsEmpty();
		}

		public void SetSelectedItem(UIItemSlot uiItemSlot)
		{
			foreach(var slot in uiItemSlots)
			{
				if (slot != null)
				{
					if (slot.SlotIndex == uiItemSlot.SlotIndex)
					{
						lastSelectedItemSlot = slot; // Set the last selected item slot
						slot.MyImage.color = uiItemSlot.selectedColor;
					}
					else
					{
						slot.MyImage.color = uiItemSlot.unselectedColor;
					}
				}
			}
		}

		public void Resize(int newSize)
		{
			if (newSize < 0 && newSize == itemSlots.Count)
				return;

			if (newSize < itemSlots.Count)
			{
				itemSlots.RemoveRange(newSize, itemSlots.Count - newSize);
			}
			else
			{
				for (int i = itemSlots.Count; i < newSize; i++)
				{
					itemSlots.Add(InventorySlot.EmptySlot());
				}
			}

			maxSlots = newSize;

			ResizeUI();
		}

		private void RefreshUISlots()
		{
			uiItemSlots.Clear();

			int index = 0;
			foreach (Transform child in inventoryUISlotHolder)
			{
				UIItemSlot uiSlot = child.GetComponent<UIItemSlot>();
				if (uiSlot != null)
				{
					uiSlot.SetSlot(index);
					uiItemSlots.Add(uiSlot); // Cache the UI item slot for quick access
				}

				index++;
			}
		}

		private void ResizeUI()
		{
			// No need to cache inventory, resize will be called very rarely.
			GridLayoutGroup gridLayout = inventoryUISlotHolder.GetComponent<GridLayoutGroup>();

			foreach (Transform child in inventoryUISlotHolder)
			{
				Destroy(child.gameObject);
			}

			for (int i = 0; i < maxSlots; i++)
			{
				GameObject slotObject = Instantiate(inventoryUISlotPrefab, inventoryUISlotHolder);
				slotObject.name = $"InventorySlot_{i}";
			}

			// I love maths and over-engineering! :D:D:D:D:D: (Sylvia: Pizzicato)
			// This lovely piece of code will resize the inventory UI to fit the available space

			float containerWidth = inventoryUISlotHolder.rect.width;
			float containerHeight = inventoryUISlotHolder.rect.height;

			float usableWidth = containerWidth - 2 * minSlotUIPadding;
			float usableHeight = containerHeight - 2 * minSlotUIPadding;

			int bestCols = 1;
			int bestRows = maxSlots;
			float bestSlotSize = 0;
			float bestSpacingX = 0;
			float bestSpacingY = 0;

			for (int cols = 1; cols <= maxSlots; cols++)
			{
				int rows = Mathf.CeilToInt((float)maxSlots / cols);

				if (rows <= 0)
					continue;

				float slotSize = Mathf.Floor(Mathf.Min(usableWidth / cols, usableHeight / rows));
				float spacingX = (cols > 1) ? (usableWidth - cols * slotSize) / (cols - 1) : 0f;
				float spacingY = (rows > 1) ? (usableHeight - rows * slotSize) / (rows - 1) : 0f;

				if (spacingX < 0 || spacingY < 0)
					continue;

				if (slotSize > bestSlotSize)
				{
					bestSlotSize = slotSize;
					bestCols = cols;
					bestRows = rows;
					bestSpacingX = spacingX;
					bestSpacingY = spacingY;
				}
			}

			gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; // Already set in the inspector, but just to be sure
			gridLayout.constraintCount = bestCols;
			gridLayout.cellSize = new Vector2(bestSlotSize, bestSlotSize);
			gridLayout.spacing = new Vector2(bestSpacingX, bestSpacingY);
			gridLayout.padding = new RectOffset(Mathf.RoundToInt(minSlotUIPadding), Mathf.RoundToInt(minSlotUIPadding), Mathf.RoundToInt(minSlotUIPadding), Mathf.RoundToInt(minSlotUIPadding));

			RefreshUISlots();
		}

		public void ToggleInventoryUI(bool setOpen)
		{
			isInventoryUIOpen = setOpen;
			if (isInventoryUIOpen)
			{
				//ResizeUI();
				//RefreshUISlots();
				inventoryUISlotHolder.parent.gameObject.SetActive(true);
			}
			else
			{
				inventoryUISlotHolder.parent.gameObject.SetActive(false);
			}
		}

		public void UseUISelectedItem()
		{
			if (lastSelectedItemSlot == null)
				return;

			InventorySlot slot = itemSlots[lastSelectedItemSlot.SlotIndex];

			if (!IsSlotEmpty(slot))
			{
				slot.Item.Use(playerController.gameObject);
				DeductItemFromSlot(lastSelectedItemSlot.SlotIndex, 1);
			}

			RefreshUISlots();
		}

		public void DropUISelectedItem()
		{
			if (lastSelectedItemSlot == null)
				return;

			InventorySlot slot = itemSlots[lastSelectedItemSlot.SlotIndex];

			if (!IsSlotEmpty(slot))
			{
				DroppedItem.DropItem(slot.Item, transform.position);
				DeductItemFromSlot(lastSelectedItemSlot.SlotIndex, 1);
				RefreshUISlots();
			}
		}

		public void DropEveryItem()
		{
			if (IsEmpty())
				return;

			for (int i = itemSlots.Count - 1; i >= 0; i--)
			{
				InventorySlot slot = itemSlots[i];
				if (!IsSlotEmpty(slot))
				{
					DroppedItem.DropItem(slot.Item, transform.position);
					ClearSlot(i);
				}
			}
			RefreshUISlots();
		}

		public void HoverSlot(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= itemSlots.Count)
			{
				ExitHoverSlot();
				return;
			}

			InventorySlot slot = itemSlots[slotIndex];

			if (IsSlotEmpty(slot))
			{
				ExitHoverSlot();
			}
			else
			{
				uIItemSlotInfo.SetItem(slot.Item);
				uIItemSlotInfo.gameObject.SetActive(true);
			}
		}

		public void ExitHoverSlot()
		{
			uIItemSlotInfo.SetItem(null);
			uIItemSlotInfo.gameObject.SetActive(false);
		}

		// Add an item to the inventory
		// Returns true if the item was added successfully, false if not.
		public bool AddItem(BaseItem item)
		{
			return AddItems(item, 1) <= 0;
		}

		// Adds multiple items to the inventory
		// Returns the remaining count of items that could not be added or stacked.
		// Returns 0 if all items were stacked or added successfully.
		public int AddItems(BaseItem item, int count)
		{
			InventorySlot slot = IsAbleToStack(item);

			while (count > 0)
			{
				if (slot == null) // No stackable item found, so we need to create a new one
				{
					if (IsFull())
						break;

					int newSlot = FillAvailableSlot(new InventorySlot(item));
					slot = itemSlots[newSlot];
					count--;

					if (count <= 0)
						break;
				}

				// Let's stack

				int spaceLeft = item.MaxStack - slot.Count; // How many more items can we stack in this inventory item slot
				if (spaceLeft <= 0)
				{
					slot = IsAbleToStack(item);
					continue;
				}
				else if (spaceLeft >= count)
				{
					slot.Count += count;
					count = 0; // All items are stacked
				}
				else
				{
					slot.Count += spaceLeft;
					count -= spaceLeft;
					slot = IsAbleToStack(item); // Need another slot for the remaining items
				}
			}

			RefreshUISlots();

			return count; // Remaining item count
		}

		public bool DeductItemFromSlot(int slotIndex, int count)
		{
			if (slotIndex < 0 || slotIndex >= itemSlots.Count)
				return false;

			InventorySlot slot = itemSlots[slotIndex];
			if (IsSlotEmpty(slot) || slot.Count < count)
				return false;

			slot.Count -= count;
			if (slot.Count <= 0)
			{
				ClearSlot(slotIndex);
			}
			return true;
		}

		public bool RemoveItem(BaseItem item)
		{
			return RemoveItems(item, 1);
		}

		// Remove items from the inventory.
		// Slots should contain at least the given count of items. Otherwise, it will not remove anything.
		// Returns true if the items were removed successfully, false if not.
		public bool RemoveItems(BaseItem item, int count)
		{
			if (count <= 0)
				return false;

			if (IsEmpty())
				return false;

			if (count > GetItemCount(item))
				return false; // Not enough items to remove

			ForceRemoveItems(item, count);
			return true; // Items were removed successfully
		}

		// Removes items from the inventory.
		// This will remove items of the given type and count, regardless of the slots.
		// Returns the number of items that could not be removed.
		public int ForceRemoveItems (BaseItem item, int count)
		{
			if (item == null || count <= 0)
				return count; // Nothing to remove

			if (IsEmpty())
				return count; // Inventory is empty, nothing to remove

			int totalRemoved = 0;
			for (int i = itemSlots.Count - 1; i >= 0; i--)
			{
				InventorySlot slot = itemSlots[i];

				if (IsSlotEmpty(slot))
					continue; // Skip empty slots

				if (slot.Item == item)
				{
					if (slot.Count <= count)
					{
						count -= slot.Count;
						totalRemoved += slot.Count;
						ClearSlot(i); // Remove the slot
					}
					else
					{
						slot.Count -= count;
						totalRemoved += count;
						count = 0; // All items removed
					}
				}
				if (count <= 0)
					break; // No more items to remove
			}
			return count; // Remaining items that were not removed
		}

		public bool PossiblePickupInteract()
		{
			if (possiblePickup == null)
				return false;

			if (lastPickupTime + pickupDelay > Time.time)
				return false;

			if (IsFullCompletely())
				return false;
			
			if (AddItem(PossiblePickup.Item))
			{
				Destroy(PossiblePickup.gameObject);
				PossiblePickup = null;
				lastPickupTime = Time.time; // Reset the pickup delay
				return true; 
			}

			return false;
		}

		public bool HasItem(BaseItem item)
		{
			if (item == null)
				return false;

			if (IsEmpty())
				return false;

			foreach (var slot in itemSlots)
			{
				if (IsSlotEmpty(slot))
					continue;

				if (slot.Item == item)
					return true;
			}
			return false;
		}

		// Returns the total count of given item in the inventory.
		// If the item is not found, returns 0.
		public int GetItemCount(BaseItem item)
		{
			if (item == null)
				return 0;

			if (IsEmpty())
				return 0;

			int totalCount = 0;
			
			foreach (var slot in itemSlots)
			{
				if (IsSlotEmpty(slot))
					continue;

				if (slot.Item == item)
				{
					totalCount += slot.Count;
				}
			}

			return totalCount;
		}

		// Are there no items in the inventory?
		// This checks if all slots are empty, not just if the inventory is full or not.
		public bool IsEmpty()
		{
			foreach (var slot in itemSlots)
			{
				if (!IsSlotEmpty(slot))
					return false;
			}

			return true;
		}

		// Are there no empty slots in the inventory?
		// NOTE: This does not check if the slots are full stacked, just that there are no empty slots.
		public bool IsFull()
		{
			foreach (var slot in itemSlots)
			{
				if (IsSlotEmpty(slot))
					return false;
			}

			return true;
		}

		// Are all slot in the inventory full stacked?
		// This checks if the inventory is full and if all slot are stacked to their maximum count.
		public bool IsFullCompletely()
		{
			foreach (var slot in itemSlots)
			{
				if (IsSlotEmpty(slot))
					return false; // There is at least one empty slot

				if (slot.Count < slot.Item.MaxStack)
					return false; // There is at least one item that can still be stacked
			}
			return true; // All slots are full stacked
		}

		// Is there room to stack this item in the inventory?
		// Returns the InventorySlot of the item if it can be stacked, otherwise returns null.
		// This does not check if the inventory is full, just if there is room to stack the item.
		public InventorySlot IsAbleToStack(BaseItem item)
		{
			if (item == null)
				return null;

			foreach (var slot in itemSlots)
			{
				if (IsSlotEmpty(slot))
					continue;

				if (slot.Item == item && slot.Count < item.MaxStack)
					return slot;
			}

			return null; // No stackable item found
		}
	}
}
