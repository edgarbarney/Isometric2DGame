using Isometric2DGame.Items;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

		private PlayerController playerController; // Reference to the player controller for interaction

		[SerializeField]
		private List<InventorySlot> itemSlots;
		public InventorySlot[] ItemSlots => itemSlots.ToArray();

		[SerializeField]
		private int maxSlots = 20;

		private DroppedItem possiblePickup; // The item that the player can pick up on demand.
		public DroppedItem PossiblePickup
		{
			get => possiblePickup;
			set
			{
				possiblePickup = value;
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

			// Pre-allocate the inventory slots
			itemSlots = new List<InventorySlot>(maxSlots);
			for (int i = 0; i < maxSlots; i++)
			{
				itemSlots.Add(InventorySlot.EmptySlot()); // Every slot starts empty
			}
		}

		private void FixedUpdate()
		{
			// No need to boxcast every frame. Especially if the PC is powerful enough to run at 60 FPS or more.
			// So FixedUpdate is better place to check for possible pickups.

			Collider2D[] colliders = Physics2D.OverlapCircleAll(playerController.transform.position, pickupRange, LayerMask.GetMask("ItemPickup"));
			float closestDistance = float.MaxValue;

			if (colliders == null || colliders.Length == 0)
			{
				possiblePickup = null; // No pickups in range
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
						possiblePickup = item; // Closest pickup!
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
					itemSlots.Add(null);
				}
			}

			maxSlots = newSize; // Update the maximum slots
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

			return count; // Remaining item count
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
			
			if (AddItem(possiblePickup.Item))
			{
				Destroy(possiblePickup.gameObject);
				possiblePickup = null;
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

		// Placeholder Inventory Debug GUI
		public void OnGUI()
		{
			if (possiblePickup != null)
			{
				// Show a small box with the item name and the interct key to pick it up
				// Use possiblePickup.Item.ItemName and playerController.InteractKey to display the pickup message
				// Use possiblePickup.GetTooltipPosition() to position the tooltip

				Vector2 tooltipPosition = playerController.PlayerCamera.WorldToScreenPoint(possiblePickup.GetTooltipPosition());

				// Ugh, this is a bit hacky, but it works for now.
				string keyName = playerController.MyPlayerInput.actions["Interact"].bindings.Count > 0 ? InputControlPath.ToHumanReadableString(playerController.MyPlayerInput.actions["Interact"].bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice) : "";

				// Theese are off a bit. It have to be centre.
				GUILayout.BeginArea(new Rect(tooltipPosition.x - 100, Screen.height - tooltipPosition.y - 50, 200, 50));
				GUILayout.Box($"Press {keyName} to pick up {possiblePickup.Item.ItemName}");
				GUILayout.EndArea();
			}


			if (itemSlots == null || itemSlots.Count == 0)
				return; // No slots to display yet

			GUILayout.BeginArea(new Rect(10, 10, 200, 800));
			GUILayout.Label("Inventory:");
			foreach (var slot in itemSlots)
			{
				if (IsSlotEmpty(slot))
				{
					GUILayout.Label("Empty Slot");
					continue; // Skip empty slots
				}

				GUILayout.Label($"{slot.Item.ItemName} x{slot.Count}");
			}
			GUILayout.EndArea();
		}
	}
}
